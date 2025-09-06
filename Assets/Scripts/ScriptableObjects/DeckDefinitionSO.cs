using UnityEngine;

[CreateAssetMenu(fileName = "DeckDefinition", menuName = "Belote/Deck Definition")]
public class DeckDefinitionSO : ScriptableObject
{
    [Header("Deck Identity")]

    public string DeckName = "Belote32";

    [Header("Card List")]
    public CardDefinitionSO[] Cards; // exactly 32 cards for Belote
}
