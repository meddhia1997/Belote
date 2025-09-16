using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime rules context for a round. Holds trump, current trick, seat state, etc.
/// Designed to be passed into rule policies (legal moves, trick resolver, scoring).
/// </summary>
public class RulesContext
{
    public RulesProfileSO Profile { get; private set; }

    /// <summary>
    /// Current trump suit for this round.
    /// Private setter ensures only SetTrump/ResetTrump can modify.
    /// </summary>
    public Suit Trump { get; private set; } = Suit.None;

    public Trick CurrentTrick { get; set; }
    public SeatId CurrentSeat { get; set; }
    public int TrickIndex { get; set; }

    /// <summary>
    /// Function to parse suit string (injected by TurnFlow/scene).
    /// </summary>
    public Func<string, Suit> ParseSuit { get; set; }

    public RulesContext(RulesProfileSO profile)
    {
        Profile = profile;
    }

    /// <summary>
    /// Explicitly set the trump suit for this round.
    /// </summary>
    public void SetTrump(Suit trump)
    {
        Trump = trump;
    }

    /// <summary>
    /// Reset trump back to "None" — useful at the beginning of a new round.
    /// </summary>
    public void ResetTrump()
    {
        Trump = Suit.None;
    }
}
