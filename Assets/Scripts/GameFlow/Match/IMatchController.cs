using System;

public interface IMatchController
{
    event Action<MatchScore> OnMatchFinished;

    void StartMatch(SeatId startingDealer);
    void StopMatch();                         // optional hard stop (returns to menu)
    MatchScore GetCurrentMatchScore();
}
