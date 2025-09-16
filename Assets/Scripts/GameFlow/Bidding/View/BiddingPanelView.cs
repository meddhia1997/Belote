using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameFlow.Bidding
{
    /// <summary>
    /// Concrete UI implementation of IBiddingView + helpers used by HumanBidSource.
    /// Displays bidding panel, logs, and exposes human button clicks via OnHumanBidPicked.
    /// </summary>
    public class BiddingPanelView : MonoBehaviour, IBiddingView
    {
        [Header("UI Elements")]
        [Tooltip("Root GameObject of the bidding panel.")]
        public GameObject panelRoot;                // entire panel root
        public TMP_Text logText;                    // log of bids
        public TMP_Text currentSeatText;            // "South's turn"
        public TMP_Text currentHighText;            // current highest bid

        [Header("Buttons (Normal bids)")]
        public Button passButton;
        public Button clubsButton;
        public Button diamondsButton;
        public Button heartsButton;
        public Button spadesButton;

        [Header("Optional / Variants")]
        [Tooltip("Optional: Only if your variant supports No Trump.")]
        public Button noTrumpButton;                // optional
        [Tooltip("Optional: Double and Redouble support.")]
        public Button doubleButton;                 // optional
        public Button redoubleButton;               // optional

        [Header("Settings")]
        [Tooltip("Level used for Normal bids (classic Belote can stay at 1).")]
        public int normalLevel = 1;

        // ============== IBiddingView event ==============
        public event Action<Bid> OnHumanBidPicked;

        // ============== Private: current wired callbacks ==============
        private Action _cbPass, _cbHearts, _cbDiamonds, _cbClubs, _cbSpades, _cbNoTrump, _cbDouble, _cbRedouble;

        void Awake()
        {
            // default wiring to raise IBiddingView event (fallback if not rebound by HumanBidSource)
            if (passButton)     passButton.onClick.AddListener(() => RaiseBid(Bid.Pass()));
            if (clubsButton)    clubsButton.onClick.AddListener(() => RaiseBid(Bid.Normal(Suit.Clubs,    normalLevel)));
            if (diamondsButton) diamondsButton.onClick.AddListener(() => RaiseBid(Bid.Normal(Suit.Diamonds, normalLevel)));
            if (heartsButton)   heartsButton.onClick.AddListener(() => RaiseBid(Bid.Normal(Suit.Hearts,   normalLevel)));
            if (spadesButton)   spadesButton.onClick.AddListener(() => RaiseBid(Bid.Normal(Suit.Spades,   normalLevel)));

            if (noTrumpButton)  noTrumpButton.onClick.AddListener(() => RaiseBid(Bid.Normal(Suit.None,    normalLevel)));
            if (doubleButton)   doubleButton.onClick.AddListener(() => RaiseBid(Bid.Double()));
            if (redoubleButton) redoubleButton.onClick.AddListener(() => RaiseBid(Bid.Redouble()));
        }

        // ============================================================
        // == IBiddingView implementation
        // ============================================================
        public void Show()
        {
            if (panelRoot) panelRoot.SetActive(true);
        }

        public void Hide()
        {
            if (panelRoot) panelRoot.SetActive(false);
        }

        public void SetCurrentSeat(SeatId seat)
        {
            if (currentSeatText) currentSeatText.text = $"Current: {seat}";
        }

        public void SetCurrentHigh(Bid bid)
        {
            if (currentHighText) currentHighText.text = $"High: {bid}";
        }

        public void AppendLog(SeatId seat, Bid bid)
        {
            if (!logText) return;
            if (string.IsNullOrEmpty(logText.text))
                logText.text = $"{seat}: {bid}";
            else
                logText.text += $"\n{seat}: {bid}";
        }

        public void EnableHumanControls(bool enable)
        {
            if (passButton)     passButton.interactable     = enable;
            if (clubsButton)    clubsButton.interactable    = enable;
            if (diamondsButton) diamondsButton.interactable = enable;
            if (heartsButton)   heartsButton.interactable   = enable;
            if (spadesButton)   spadesButton.interactable   = enable;

            if (noTrumpButton)  noTrumpButton.interactable  = enable;
            if (doubleButton)   doubleButton.interactable   = enable;
            if (redoubleButton) redoubleButton.interactable = enable;
        }

        // ============================================================
        // == Helpers expected by HumanBidSource
        // ============================================================

        /// <summary>
        /// Enable/disable buttons based on the allowed bids set.
        /// </summary>
        public void SetButtonsFromAllowed(IReadOnlyList<Bid> allowed)
        {
            // default: disable all
            SetAllButtons(false);

            if (allowed == null) return;

            for (int i = 0; i < allowed.Count; i++)
            {
                var b = allowed[i];
                switch (b.type)
                {
                    case BidType.Pass:
                        if (passButton) passButton.interactable = true;
                        break;

                    case BidType.Normal:
                        switch (b.suit)
                        {
                            case Suit.Clubs:    if (clubsButton)    clubsButton.interactable = true;    break;
                            case Suit.Diamonds: if (diamondsButton) diamondsButton.interactable = true; break;
                            case Suit.Hearts:   if (heartsButton)   heartsButton.interactable = true;   break;
                            case Suit.Spades:   if (spadesButton)   spadesButton.interactable = true;   break;
                            case Suit.None:     if (noTrumpButton)  noTrumpButton.interactable  = true;  break; // only if you actually use NT
                        }
                        break;

                    case BidType.Double:
                        if (doubleButton) doubleButton.interactable = true;
                        break;

                    case BidType.Redouble:
                        if (redoubleButton) redoubleButton.interactable = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Bind callbacks from HumanBidSource so the buttons call back into the source.
        /// Any null callback leaves the existing (default event-raising) behavior.
        /// </summary>
        public void BindCallbacks(
            Action onPass,
            Action onHearts,
            Action onDiamonds,
            Action onClubs,
            Action onSpades,
            Action onNoTrump = null,
            Action onDouble = null,
            Action onRedouble = null)
        {
            _cbPass     = onPass;
            _cbHearts   = onHearts;
            _cbDiamonds = onDiamonds;
            _cbClubs    = onClubs;
            _cbSpades   = onSpades;
            _cbNoTrump  = onNoTrump;
            _cbDouble   = onDouble;
            _cbRedouble = onRedouble;

            // Rebind button listeners to dispatch to callbacks if provided
            Rewire(passButton,     _cbPass,     () => RaiseBid(Bid.Pass()));
            Rewire(heartsButton,   _cbHearts,   () => RaiseBid(Bid.Normal(Suit.Hearts,   normalLevel)));
            Rewire(diamondsButton, _cbDiamonds, () => RaiseBid(Bid.Normal(Suit.Diamonds, normalLevel)));
            Rewire(clubsButton,    _cbClubs,    () => RaiseBid(Bid.Normal(Suit.Clubs,    normalLevel)));
            Rewire(spadesButton,   _cbSpades,   () => RaiseBid(Bid.Normal(Suit.Spades,   normalLevel)));

            if (noTrumpButton) Rewire(noTrumpButton, _cbNoTrump, () => RaiseBid(Bid.Normal(Suit.None, normalLevel)));
            if (doubleButton)  Rewire(doubleButton,  _cbDouble,  () => RaiseBid(Bid.Double()));
            if (redoubleButton)Rewire(redoubleButton,_cbRedouble,() => RaiseBid(Bid.Redouble()));
        }

        /// <summary>
        /// Open the panel for a specific seat, show current high bid and enable controls.
        /// </summary>
        public void OpenForSeat(SeatId seat, Bid currentHigh)
        {
            Show();
            SetCurrentSeat(seat);
            SetCurrentHigh(currentHigh);
            EnableHumanControls(true);
        }

        /// <summary>
        /// Close the panel and disable controls.
        /// </summary>
        public void Close()
        {
            EnableHumanControls(false);
            Hide();
        }

        // ============================================================
        // == Internals
        // ============================================================

        private void SetAllButtons(bool enable)
        {
            if (passButton)     passButton.interactable     = enable;
            if (clubsButton)    clubsButton.interactable    = enable;
            if (diamondsButton) diamondsButton.interactable = enable;
            if (heartsButton)   heartsButton.interactable   = enable;
            if (spadesButton)   spadesButton.interactable   = enable;

            if (noTrumpButton)  noTrumpButton.interactable  = enable;
            if (doubleButton)   doubleButton.interactable   = enable;
            if (redoubleButton) redoubleButton.interactable = enable;
        }

        private void Rewire(Button btn, Action cb, Action fallback)
        {
            if (!btn) return;
            btn.onClick.RemoveAllListeners();
            if (cb != null) btn.onClick.AddListener(() => cb());
            else            btn.onClick.AddListener(() => fallback());
        }

        private void RaiseBid(Bid bid)
        {
            // Default path (when not rebound by HumanBidSource)
            Debug.Log($"[BiddingPanelView] Human picked: {bid}");
            OnHumanBidPicked?.Invoke(bid);
        }
    }
}
