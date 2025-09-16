using System.Collections.Generic;

namespace GameFlow.Bidding
{
    /// <summary>
    /// Définit l’ordre de prise de parole pendant l’enchère.
    /// </summary>
    public interface IBidOrderPolicy
    {
        /// <summary>
        /// Retourne la séquence des sièges qui vont parler,
        /// en commençant à gauche du donneur.
        /// </summary>
        IEnumerable<SeatId> EnumerateOrder(SeatId dealer);
    }
}
