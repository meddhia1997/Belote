using UnityEngine;

[CreateAssetMenu(fileName = "TrumpSource_Random", menuName = "Belote/Rules/TrumpSource/Random")]
public class TrumpSource_RandomSO : ScriptableObject, ITrumpSource
{
    // Because random can decide instantly
    public bool IsImmediate => true;

    public Suit DecideTrump(SeatId dealer)
    {
        // Return a random suit
        int v = Random.Range(0, 4);
        return (Suit)v; // assuming Suit enum is 0=Clubs,1=Diamonds,2=Hearts,3=Spades
    }
}
