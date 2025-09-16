using System;
using UnityEngine;

/// <summary>
/// Type de l’annonce (enchère).
/// </summary>
public enum BidType
{
    Pass,       // je passe
    Normal,     // je prends à un atout
    Double,     // je contre
    Redouble    // je surcontre
}

/// <summary>
/// Représente une enchère : type + couleur (pour Normal) + niveau.
/// Niveau est utile si on veut supporter variantes avec plusieurs "hauteurs".
/// </summary>
[Serializable]
public struct Bid
{
    public BidType type;
    public Suit suit;   // valable seulement si type == Normal
    public int level;   // par défaut = 1 pour Belote

    // --- Factories ---
    public static Bid Pass() => new Bid { type = BidType.Pass, suit = Suit.None, level = 0 };
    public static Bid Normal(Suit s, int lvl = 1) => new Bid { type = BidType.Normal, suit = s, level = lvl };
    public static Bid Double() => new Bid { type = BidType.Double, suit = Suit.None, level = 0 };
    public static Bid Redouble() => new Bid { type = BidType.Redouble, suit = Suit.None, level = 0 };

    public bool IsPass => type == BidType.Pass;
    public bool IsNormal => type == BidType.Normal;
    public bool IsDouble => type == BidType.Double;
    public bool IsRedouble => type == BidType.Redouble;

    public override string ToString()
    {
        return type switch
        {
            BidType.Pass => "Pass",
            BidType.Normal => $"Take {suit} (L{level})",
            BidType.Double => "Double!",
            BidType.Redouble => "Redouble!",
            _ => "Unknown"
        };
    }
}

/// <summary>
/// Contrat final issu des enchères (taker, couleur d’atout, etc.).
/// </summary>
[Serializable]
public struct Contract
{
    public SeatId taker;   // qui a pris
    public Suit trump;     // atout choisi
    public int level;      // par défaut = 1

    public override string ToString() => $"Taker={taker}, Trump={trump}, L{level}";
}
