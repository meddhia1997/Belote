using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives a round: 8 tricks, enforcing legal moves, collecting tricks,
/// notifying RoundController, and exposing trump changes for HUD.
/// </summary>
public class TurnFlowController : MonoBehaviour
{
    [Header("Core")]
    public SeatRegistry seatRegistry;
    public RulesProfileSO rulesProfile;
    public TurnFlowConfigSO config;

    [Header("Services")]
    public ChooserRegistry chooserRegistry;
    public TrickPileController trickPiles;     // assign in Inspector
    public RoundController roundController;    // assign in Inspector

    // runtime
    private RulesContext _ctx;

    // === Trump change event + getter (for HUD/UI) ===
    public event System.Action<Suit> OnTrumpChanged;
    public Suit CurrentTrump => _ctx != null ? _ctx.Trump : Suit.None;

    void Awake()
    {
        if (!ValidateSceneWiring("Awake")) return;

        _ctx = new RulesContext(rulesProfile)
        {
            ParseSuit = SuitUtils.Parse
        };
    }

    /// <summary>Called by Dealing/Match just before PlayRound().</summary>
    public void StartRound(SeatId dealer)
    {
        if (!ValidateSceneWiring("StartRound")) return;

        var leader = SeatRegistry.Next(dealer);

        _ctx.CurrentTrick = new Trick
        {
            leader   = leader,
            leadSuit = Suit.None,
            cards    = new List<PlayedCard>(4)
        };
        _ctx.CurrentSeat = leader;
        _ctx.TrickIndex  = 0;

        // round scorer reset is kicked by DealingController before this (BeginNewRound)

        // Only local seat interactable initially
        foreach (var s in seatRegistry.All())
            s.Hand?.SetInteractable(s.IsLocal);

        if (_ctx.Trump == Suit.None)
            Debug.LogWarning("[TurnFlow] Starting round with TRUMP=None. Set it before StartRound.");
    }

    public IEnumerator PlayRound()
    {
        if (!ValidateSceneWiring("PlayRound")) yield break;

        for (int t = 0; t < 8; t++)
        {
            yield return StartCoroutine(PlayOneTrick());
            _ctx.TrickIndex++;
            yield return new WaitForSeconds(config ? config.afterTrickDelay : 0.5f);
        }
        // RoundController finalizes after 8th trick.
    }

    IEnumerator PlayOneTrick()
    {
        if (_ctx == null || _ctx.CurrentTrick == null)
        {
            Debug.LogError("[TurnFlow] Context or CurrentTrick is null.");
            yield break;
        }

        var trick = _ctx.CurrentTrick;
        if (trick.cards == null) trick.cards = new List<PlayedCard>(4);
        trick.cards.Clear();

        // Order starting from leader
        var leader = trick.leader;
        var order = new SeatId[]
        {
            leader,
            SeatRegistry.Next(leader),
            SeatRegistry.Next(SeatRegistry.Next(leader)),
            SeatRegistry.Next(SeatRegistry.Next(SeatRegistry.Next(leader)))
        };

        for (int i = 0; i < 4; i++)
        {
            var seatId = order[i];
            var seat = seatRegistry?.Get(seatId);
            if (seat == null || seat.Hand == null)
            {
                Debug.LogError($"[TurnFlow] Seat or Hand is null for {seatId}.");
                yield break;
            }

            var handViews = seat.Hand.GetCards();
            var handDefs  = ExtractHandDefs(handViews);

            // legal moves
            List<CardDefinitionSO> legal =
                (rulesProfile?.LegalMovePolicy == null)
                    ? new List<CardDefinitionSO>(handDefs)
                    : rulesProfile.LegalMovePolicy.GetLegalMoves(_ctx, handDefs, seatId);

            // chooser (human/AI)
            var chooser = chooserRegistry ? chooserRegistry.GetChooser(seatId) : null;
            CardDefinitionSO chosen = null;

            if (chooser == null)
            {
                chosen = (legal.Count > 0) ? legal[0] : null;
                if (chosen == null) { Debug.LogError($"[TurnFlow] No legal card for {seatId}."); yield break; }
            }
            else
            {
                System.Action<CardDefinitionSO> onPick = c => { chosen = c; };
                chooser.OnCardChosen += onPick;
                chooser.BeginChoose(_ctx, legal, seatId);
                while (chosen == null) yield return null;
                chooser.OnCardChosen -= onPick;
            }

            yield return StartCoroutine(PlayChosenCard(i, seat, seatId, chosen));

            _ctx.CurrentSeat = SeatRegistry.Next(seatId);
            yield return new WaitForSeconds(config ? config.afterPlayDelay : 0.2f);
        }

        // resolve trick
        var (winner, pts) = rulesProfile.TrickResolver.ResolveTrick(_ctx);
        Debug.Log($"[TurnFlow] Trick winner = {winner}, pts = {pts}");

        // move visuals then score
        if (trickPiles != null)
            yield return StartCoroutine(trickPiles.CollectTrick(winner));

        roundController?.OnTrickResolved(winner, pts);

        // next trick (winner leads)
        _ctx.CurrentTrick = new Trick { leader = winner, leadSuit = Suit.None, cards = new List<PlayedCard>(4) };
        _ctx.CurrentSeat = winner;

        // Only local seat interactable again
        foreach (var s in seatRegistry.All())
            s.Hand?.SetInteractable(s.IsLocal);
    }

    IEnumerator PlayChosenCard(int indexInTrick, SeatContext seat, SeatId seatId, CardDefinitionSO chosen)
    {
        if (indexInTrick == 0)
            _ctx.CurrentTrick.leadSuit = SuitUtils.Parse(chosen.Suit);

        var view = FindView(seat.Hand.GetCards(), chosen);
        if (view == null) { Debug.LogError($"[TurnFlow] Missing CardView for {chosen.ShortName} @ {seatId}."); yield break; }

        yield return StartCoroutine(seat.Hand.PlayCardToTrick(view));
        _ctx.CurrentTrick.cards.Add(new PlayedCard { seat = seatId, card = chosen });
    }

    // === Trump API ===
    public void ResetTrump()
    {
        if (_ctx == null) return;
        _ctx.ResetTrump();
        OnTrumpChanged?.Invoke(_ctx.Trump);
    }

    public void SetTrump(Suit trump)
    {
        if (_ctx == null) return;
        _ctx.SetTrump(trump);
        Debug.Log($"[TurnFlow] Trump set to {trump}");
        OnTrumpChanged?.Invoke(trump);
    }

    // helpers
    List<CardDefinitionSO> ExtractHandDefs(IReadOnlyList<CardView> views)
    {
        var list = new List<CardDefinitionSO>(views.Count);
        for (int i = 0; i < views.Count; i++)
        {
            var v = views[i];
            var def = v ? v.GetCardDefinition() : null;
            if (def != null) list.Add(def);
        }
        return list;
    }

    CardView FindView(IReadOnlyList<CardView> views, CardDefinitionSO def)
    {
        for (int i = 0; i < views.Count; i++)
        {
            var v = views[i];
            if (v && v.GetCardDefinition() == def) return v;
        }
        for (int i = 0; i < views.Count; i++)
        {
            var v = views[i]; var d = v ? v.GetCardDefinition() : null;
            if (d != null && def != null && d.ShortName == def.ShortName) return v;
        }
        for (int i = 0; i < views.Count; i++)
        {
            var v = views[i]; var d = v ? v.GetCardDefinition() : null;
            if (d != null && def != null && d.Suit == def.Suit && d.Rank == def.Rank) return v;
        }
        return null;
    }

    bool ValidateSceneWiring(string where)
    {
        if (seatRegistry == null) { Debug.LogError($"[TurnFlow] seatRegistry is null at {where}."); return false; }
        if (rulesProfile == null) { Debug.LogError($"[TurnFlow] rulesProfile is null at {where}."); return false; }
        if (rulesProfile.TrumpPolicy == null) { Debug.LogError($"[TurnFlow] TrumpPolicy null at {where}."); return false; }
        if (rulesProfile.OrderingPolicy == null) { Debug.LogError($"[TurnFlow] OrderingPolicy null at {where}."); return false; }
        if (rulesProfile.ScoringPolicy == null) { Debug.LogError($"[TurnFlow] ScoringPolicy null at {where}."); return false; }
        if (rulesProfile.TrickResolver == null) { Debug.LogError($"[TurnFlow] TrickResolver null at {where}."); return false; }
        if (chooserRegistry == null) { Debug.LogWarning($"[TurnFlow] chooserRegistry is null at {where} (fallback will be used)."); }
        return true;
    }
}
