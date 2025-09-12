using System;

public interface IRoundController
{
    event Action<RoundScore> OnRoundFinished;
    void BeginNewRound(SeatId dealer);
    void OnTrickResolved(SeatId winnerSeat, int trickPoints);
}
