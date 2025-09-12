public class RoundScorer : IRoundScorer
{
    private int us, them;
    private TeamId? lastTrickWinner;

    public void ResetRound()
    {
        us = 0;
        them = 0;
        lastTrickWinner = null;
    }

    public void AddTrick(TeamId team, int points)
    {
        if (team == TeamId.Us) us += points;
        else them += points;
    }

    public void SetLastTrickWinner(TeamId team)
    {
        lastTrickWinner = team;
    }

    public RoundScore GetRoundScore()
    {
        return new RoundScore
        {
            us = us,
            them = them,
            lastTrickWinner = lastTrickWinner
        };
    }
}
