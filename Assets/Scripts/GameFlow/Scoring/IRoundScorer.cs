public interface IRoundScorer
{
    void ResetRound();
    void AddTrick(TeamId team, int points);
    void SetLastTrickWinner(TeamId team);
    RoundScore GetRoundScore();
}
