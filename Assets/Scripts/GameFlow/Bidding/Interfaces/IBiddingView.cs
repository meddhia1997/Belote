using System;

namespace GameFlow.Bidding
{
    /// <summary>
    /// Vue/UI pour l’enchère. Contrôle l’affichage et les interactions humaines.
    /// </summary>
    public interface IBiddingView
    {
        void Show();
        void Hide();

        void SetCurrentSeat(SeatId seat);
        void SetCurrentHigh(Bid bid);
        void AppendLog(SeatId seat, Bid bid);

        /// <summary>
        /// Active/désactive les contrôles (boutons) lorsque c’est au joueur local de parler.
        /// </summary>
        void EnableHumanControls(bool enable);

        /// <summary>
        /// Événement envoyé par la vue quand le joueur humain choisit une enchère.
        /// </summary>
        event Action<Bid> OnHumanBidPicked;
    }
}
