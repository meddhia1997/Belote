public interface IScoringPolicy
{
    int GetCardPoints(Suit suit, string rank, Suit trump);
    int LastTrickBonus { get; }
    // later: Belote/Rebelote, capot, coinche multipliers, etc.
}
