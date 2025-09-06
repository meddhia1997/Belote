using UnityEngine;

[CreateAssetMenu(fileName = "CardDefinition", menuName = "Belote/Card Definition")]
public class CardDefinitionSO : ScriptableObject
{
    [Header("Card Identity")]
    public string DisplayName;     // "Ace of Hearts"
    public string ShortName;       // "AH"
    public string Rank;            // "7, 8, 9, 10, J, Q, K, A"
    public string Suit;            // "Hearts, Diamonds, Clubs, Spades"

    [Header("Belote Scoring")]
    public int PointsAtTrump;
    public int PointsOffTrump;

    [Header("Trick Order")]
    public int OrderAtTrump;
    public int OrderOffTrump;

    [Header("Visuals")]
    public Sprite CardSprite;              // reference via Addressable later
    public string AddressableKey;          // alternative for dynamic loading
}
