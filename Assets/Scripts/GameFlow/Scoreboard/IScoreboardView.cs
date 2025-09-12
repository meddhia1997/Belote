public interface IScoreboardView
{
    void SetRound(int us, int them);
    void SetMatch(int us, int them);
    void FlashWinner(TeamId team);
}
