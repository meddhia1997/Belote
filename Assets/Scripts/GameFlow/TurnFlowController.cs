using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Awake()
    {
        if (!ValidateSceneWiring("Awake")) return;

        _ctx = new RulesContext(rulesProfile)
        {
            ParseSuit = SuitUtils.Parse
        };
    }

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

        // start scoring round
        roundController?.BeginNewRound(dealer);

        // Only local seat interactable
        foreach (var s in seatRegistry.All())
            s.Hand?.SetInteractable(s.IsLocal);

        Debug.Log($"[TurnFlow] Round started. Dealer={dealer}, Leader={leader}");
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
        // RoundController handles scoring finalization on 8th trick
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

        // Order from leader
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
                Debug.LogError($"[TurnFlow] Seat or Hand is null for seat {seatId}. Check SeatRegistry assignments.");
                yield break;
            }

            var handViews = seat.Hand.GetCards();
            if (handViews == null)
            {
                Debug.LogError($"[TurnFlow] GetCards() returned null for seat {seatId}.");
                yield break;
            }

            var handDefs = ExtractHandDefs(handViews);
            if (handDefs.Count == 0)
            {
                Debug.LogWarning($"[TurnFlow] No cards in hand for seat {seatId}.");
            }

            // Legal moves
            List<CardDefinitionSO> legal;
            if (rulesProfile == null || rulesProfile.LegalMovePolicy == null)
            {
                Debug.LogWarning("[TurnFlow] RulesProfile or LegalMovePolicy is null. Falling back to all cards legal.");
                legal = new List<CardDefinitionSO>(handDefs);
            }
            else
            {
                legal = rulesProfile.LegalMovePolicy.GetLegalMoves(_ctx, handDefs, seatId);
            }

            // Chooser
            var chooser = chooserRegistry ? chooserRegistry.GetChooser(seatId) : null;
            if (chooser == null)
            {
                Debug.LogWarning($"[TurnFlow] No chooser for seat {seatId}. Picking first legal automatically.");
                var fallback = (legal.Count > 0) ? legal[0] : null;
                if (fallback == null)
                {
                    Debug.LogError($"[TurnFlow] No legal card to play for seat {seatId}.");
                    yield break;
                }
                yield return StartCoroutine(PlayChosenCard(i, seat, seatId, fallback));
            }
            else
            {
                CardDefinitionSO chosen = null;
                System.Action<CardDefinitionSO> onPick = c => { chosen = c; };

                chooser.OnCardChosen += onPick;
                chooser.BeginChoose(_ctx, legal, seatId);

                // wait until chosen
                while (chosen == null) yield return null;

                chooser.OnCardChosen -= onPick;
                yield return StartCoroutine(PlayChosenCard(i, seat, seatId, chosen));
            }

            _ctx.CurrentSeat = SeatRegistry.Next(seatId);
            yield return new WaitForSeconds(config ? config.afterPlayDelay : 0.2f);
        }

        // Resolve trick
        if (rulesProfile == null || rulesProfile.TrickResolver == null)
        {
            Debug.LogError("[TurnFlow] RulesProfile or TrickResolver is null.");
            yield break;
        }

        var (winner, pts) = rulesProfile.TrickResolver.ResolveTrick(_ctx);
        Debug.Log($"[TurnFlow] Trick winner = {winner}, pts = {pts}");

        // Move played cards to winner’s pile, then notify RoundController
        if (trickPiles != null)
        {
            yield return StartCoroutine(trickPiles.CollectTrick(winner));
        }
        else
        {
            Debug.LogError("[TurnFlow] trickPiles is NULL. Assign TrickPileController in TurnFlowController.");
        }

        if (roundController != null)
        {
            roundController.OnTrickResolved(winner, pts);
        }
        else
        {
            Debug.LogWarning("[TurnFlow] roundController is NULL. Scoring won’t update.");
        }

        // Next trick (winner leads)
        _ctx.CurrentTrick = new Trick { leader = winner, leadSuit = Suit.None, cards = new List<PlayedCard>(4) };
        _ctx.CurrentSeat = winner;

        // Keep only local seat interactable
        foreach (var s in seatRegistry.All())
            s.Hand?.SetInteractable(s.IsLocal);
    }

    IEnumerator PlayChosenCard(int indexInTrick, SeatContext seat, SeatId seatId, CardDefinitionSO chosen)
    {
        if (chosen == null)
        {
            Debug.LogError($"[TurnFlow] Chosen card is null for seat {seatId}.");
            yield break;
        }

        if (indexInTrick == 0)
            _ctx.CurrentTrick.leadSuit = SuitUtils.Parse(chosen.Suit);

        var view = FindView(seat.Hand.GetCards(), chosen);
        if (view == null)
        {
            Debug.LogError($"[TurnFlow] Could not find CardView for {chosen.DisplayName} @ {seatId}.");
            yield break;
        }

        Debug.Log($"[TurnFlow] {seatId} plays {chosen.ShortName} → TrickArea");
        yield return StartCoroutine(seat.Hand.PlayCardToTrick(view));

        _ctx.CurrentTrick.cards.Add(new PlayedCard { seat = seatId, card = chosen });
    }

    // --- helpers ---
    List<CardDefinitionSO> ExtractHandDefs(IReadOnlyList<CardView> views)
    {
        var list = new List<CardDefinitionSO>(views.Count);
        for (int i = 0; i < views.Count; i++)
        {
            var v = views[i];
            if (v == null) continue;
            var def = v.GetCardDefinition();
            if (def != null) list.Add(def);
        }
        return list;
    }

    CardView FindView(IReadOnlyList<CardView> views, CardDefinitionSO def)
    {
        // 1) reference equality
        for (int i = 0; i < views.Count; i++)
        {
            var v = views[i];
            if (v && v.GetCardDefinition() == def) return v;
        }
        // 2) short name
        for (int i = 0; i < views.Count; i++)
        {
            var v = views[i];
            var d = v ? v.GetCardDefinition() : null;
            if (d != null && def != null && d.ShortName == def.ShortName) return v;
        }
        // 3) suit + rank
        for (int i = 0; i < views.Count; i++)
        {
            var v = views[i];
            var d = v ? v.GetCardDefinition() : null;
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
        if (rulesProfile.LegalMovePolicy == null) { Debug.LogWarning($"[TurnFlow] LegalMovePolicy null at {where} (falling back to all cards legal)."); }
        if (rulesProfile.TrickResolver == null) { Debug.LogError($"[TurnFlow] TrickResolver null at {where}."); return false; }
        if (chooserRegistry == null) { Debug.LogWarning($"[TurnFlow] chooserRegistry is null at {where} (AI/Human choice fallback will be used)."); }
        return true;
    }
}
