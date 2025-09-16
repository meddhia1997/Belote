using System;
using System.Collections.Generic;
using UnityEngine;
namespace GameFlow.Bidding
{
    /// <summary>
    /// Source humaine. Utilise BiddingPanelView pour l'UI.
    /// Les boutons de la vue appellent les méthodes publiques ici.
    /// </summary>
    public class HumanBidSource : MonoBehaviour, IBidSource
    {
        public BiddingPanelView view;
        public bool IsHuman => true;
        public event Action<Bid> OnBidChosen;

        private IReadOnlyList<Bid> _allowed;

        public void BeginBid(SeatId seat, Bid currentHigh, IReadOnlyList<Bid> allowed)
        {
            _allowed = allowed;
            if (view)
            {
                view.SetButtonsFromAllowed(allowed);
                view.BindCallbacks(UI_Pass, UI_Hearts, UI_Diamonds, UI_Clubs, UI_Spades);
                view.OpenForSeat(seat, currentHigh);
            }
            else
            {
                Debug.LogWarning("[HumanBidSource] No BiddingPanelView assigned. Defaulting to Pass.");
                OnBidChosen?.Invoke(Bid.Pass());
            }
        }


        public void Cancel()
        {
            if (view) view.Close();
            _allowed = null;
        }

        // UI methods
        public void UI_Pass() => ChooseIfAllowed(Bid.Pass());
        public void UI_Hearts() => ChooseIfAllowed(Bid.Normal(Suit.Hearts, 1));
        public void UI_Diamonds() => ChooseIfAllowed(Bid.Normal(Suit.Diamonds, 1));
        public void UI_Clubs() => ChooseIfAllowed(Bid.Normal(Suit.Clubs, 1));
        public void UI_Spades() => ChooseIfAllowed(Bid.Normal(Suit.Spades, 1));

        void ChooseIfAllowed(Bid b)
        {
            if (_allowed == null) { OnBidChosen?.Invoke(Bid.Pass()); return; }
            for (int i = 0; i < _allowed.Count; i++)
            {
                if (_allowed[i].type == b.type &&
                    (_allowed[i].type == BidType.Pass || (_allowed[i].suit == b.suit && _allowed[i].level == b.level)))
                {
                    view?.Close();
                    OnBidChosen?.Invoke(b);
                    return;
                }
            }
            Debug.Log($"[HumanBidSource] Bid not in allowed set: {b}");
        }
    }
}
