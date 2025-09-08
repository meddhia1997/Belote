using System;
using System.Collections.Generic;

public class RulesContext
{
    public IRulesProfile Profile { get; }
    public Suit Trump => Profile.TrumpPolicy.CurrentTrump;

    public Trick CurrentTrick;                         // active trick (may be partial)
    public SeatId CurrentSeat;                         // seat to act (optional)
    public int TrickIndex;                             // 0..7 (optional)

    // helpers injected at bootstrap:
    public Func<string, Suit> ParseSuit;               // e.g., SuitUtils.Parse
    public Func<SeatId, IReadOnlyList<CardView>> GetHandViews; // optional

    public RulesContext(IRulesProfile profile)
    {
        Profile = profile;
    }
}
