public enum Suit { Hearts, Diamonds, Clubs, Spades, None }

public static class SuitUtils
{
    public static Suit Parse(string suitStr)
    {
        switch (suitStr)
        {
            case "Hearts":   return Suit.Hearts;
            case "Diamonds": return Suit.Diamonds;
            case "Clubs":    return Suit.Clubs;
            case "Spades":   return Suit.Spades;
            default:         return Suit.None;
        }
    }
}
