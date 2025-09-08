using UnityEngine;

[CreateAssetMenu(fileName = "DealingFlowSettings", menuName = "Belote/Anim/Dealing Flow Settings")]
public class DealingFlowSettingsSO : ScriptableObject
{
    [Header("Packet pattern (e.g., 3-2-3)")]
    public int[] packetPattern = new int[] { 3, 2, 3 };

    [Header("Timing")]
    [Tooltip("Delay between individual cards in seconds.")]
    public float interCardDelay = 0.04f;

    [Tooltip("Optional pause between packets per player.")]
    public float interPacketDelay = 0.0f;

    [Header("Deck anchor (runtime)")]
    [Tooltip("Canvas-anchored position of the virtual deck (no scene object).")]
    public Vector2 deckAnchoredPos = new Vector2(0, -60f);
}
