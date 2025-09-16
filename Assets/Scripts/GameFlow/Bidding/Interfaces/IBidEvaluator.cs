using System.Collections.Generic;

namespace GameFlow.Bidding
{
    public interface IBidEvaluator
    {
        /// <summary>Construit la liste des annonces permises (ex: Pass + 4 couleurs niveau 1).</summary>
        List<Bid> BuildAllowed(Bid currentHigh);
    }
}
