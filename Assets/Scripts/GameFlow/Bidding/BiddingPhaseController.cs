using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFlow.Bidding;

/// <summary>
/// Lancement des enchères après la donne. Interroge les sources IBidSource dans l'ordre,
/// applique validation + comparaison. Termine en émettant:
///  - OnBiddingFinished(Contract)
///  - OnTrumpChosen(Suit) (pour compatibilité avec ton flux actuel)
/// </summary>
public class BiddingPhaseController : MonoBehaviour
{
    [Header("Rules")]
    public BiddingRulesSO rules;

    [Header("Sources")]
    public BidderRegistry bidderRegistry;

    [Header("View / Telemetry (optional)")]
    public MonoBehaviour biddingView;     // IBiddingView
    public MonoBehaviour telemetrySource; // IBiddingTelemetry

    public event Action<Contract> OnBiddingFinished;
    public event Action<Suit> OnTrumpChosen;

    private IBiddingView _view;
    private IBiddingTelemetry _telemetry;
    private bool _running;

    void Awake()
    {
        _view = biddingView as IBiddingView;
        _telemetry = telemetrySource as IBiddingTelemetry ?? new NullBiddingTelemetry();
        if (rules == null || rules.OrderPolicy == null || rules.Comparator == null || rules.Validator == null || rules.Evaluator == null)
            Debug.LogError("[BiddingPhaseController] Missing BiddingRulesSO or one of its policies.");
        if (bidderRegistry == null)
            Debug.LogError("[BiddingPhaseController] BidderRegistry not assigned.");
    }

    public void Begin(SeatId dealer)
    {
        if (_running) return;
        StartCoroutine(BiddingRoutine(dealer));
    }

    public void Cancel()
    {
        _running = false;
        bidderRegistry?.CancelAll();
        _view?.Hide();
    }

    IEnumerator BiddingRoutine(SeatId dealer)
    {
        _running = true;
        _view?.Show();
        _telemetry.TrackShown();

        var current = Bid.Pass();
        SeatId lastBidder = SeatId.South;
        int consecutivePasses = 0;

        var order = rules.OrderPolicy.EnumerateOrder(dealer).GetEnumerator();
        if (!order.MoveNext()) { _running = false; yield break; }
        SeatId turn = order.Current;

        while (_running)
        {
            var src = bidderRegistry?.Get(turn);
            if (src == null)
            {
                Debug.LogError($"[Bidding] No source for seat {turn}. Forcing Pass.");
                ApplyBid(turn, Bid.Pass(), ref current, ref lastBidder, ref consecutivePasses);
            }
            else
            {
                var allowed = rules.Evaluator.BuildAllowed(current);
                _view?.SetCurrentSeat(turn);
                _view?.SetCurrentHigh(current);
                _view?.EnableHumanControls(src.IsHuman);
                _telemetry.TrackSeatTurn(turn);

                Bid picked = Bid.Pass();
                bool done = false;
                void OnPick(Bid b) { picked = b; done = true; }

                src.OnBidChosen += OnPick;
                src.BeginBid(turn, current, allowed);

                if (!src.IsHuman && rules.aiThinkDelay > 0f)
                    yield return new WaitForSeconds(rules.aiThinkDelay);

                while (!done) yield return null;

                src.OnBidChosen -= OnPick;

                bool accepted;
                if (rules.Validator.IsValid(picked, current))
                {
                    // Accept Pass always; else must be better than current or current is Pass.
                    accepted = (picked.type == BidType.Pass) || (current.type == BidType.Pass) || rules.Comparator.IsBetter(picked, current);
                }
                else
                {
                    accepted = false;
                }

                _telemetry.TrackBid(turn, picked, accepted);
                ApplyBid(turn, accepted ? picked : Bid.Pass(), ref current, ref lastBidder, ref consecutivePasses);
                _view?.AppendLog(turn, current);
            }

            // End conditions:
            if (current.type == BidType.Pass && consecutivePasses >= 4)
            {
                // Personne ne prend : à toi de choisir le fallback (ici: None).
                FinishWithContract(new Contract { taker = dealer, trump = Suit.None, level = 0 });
                yield break;
            }
            if (current.type == BidType.Normal && consecutivePasses >= 3)
            {
                FinishWithContract(new Contract { taker = lastBidder, trump = current.suit, level = current.level });
                yield break;
            }

            // Next seat (cycle on order)
            if (!order.MoveNext())
            {
                order = rules.OrderPolicy.EnumerateOrder(dealer).GetEnumerator();
                order.MoveNext();
            }
            turn = order.Current;

            if (rules.betweenTurnsDelay > 0f) yield return new WaitForSeconds(rules.betweenTurnsDelay);
        }
    }

    void ApplyBid(SeatId seat, Bid bid, ref Bid current, ref SeatId lastBidder, ref int passes)
    {
        if (bid.type == BidType.Pass)
        {
            passes++;
        }
        else
        {
            if (current.type == BidType.Pass || rules.Comparator.IsBetter(bid, current))
            {
                current = bid;
                lastBidder = seat;
            }
            passes = 0;
        }
    }

    void FinishWithContract(Contract c)
    {
        _running = false;
        _view?.Hide();
        Debug.Log($"[Bidding] Finished: {c}");
        OnBiddingFinished?.Invoke(c);
        OnTrumpChosen?.Invoke(c.trump);
    }
}

/// <summary>Telemetry par défaut (no-op)</summary>
public class NullBiddingTelemetry : IBiddingTelemetry
{
    public void TrackShown() { }
    public void TrackSeatTurn(SeatId seat) { }
    public void TrackBid(SeatId seat, Bid bid, bool accepted) { }
    public void TrackFinished(Contract c) { }
}
