using System.Collections.Generic;

public struct PlayedCard
{
    public SeatId seat;             // who played it
    public CardDefinitionSO card;   // the SO you already have
}

public class Trick
{
    public SeatId leader;                   // seat who opened the trick
    public List<PlayedCard> cards = new();  // in play order
    public Suit leadSuit = Suit.None;       // set when first card is played
}
