using UnityEngine;

[CreateAssetMenu(fileName = "TrumpSource_Preset", menuName = "Belote/Rules/TrumpSource/Preset")]
public class TrumpSource_PresetSO : ScriptableObject, ITrumpSource
{
    [Header("Preset Trump")]
    public Suit trumpSuit = Suit.Hearts;

    // Because this source can decide instantly
    public bool IsImmediate => true;

    public Suit DecideTrump(SeatId dealer)
    {
        return trumpSuit;
    }
}
