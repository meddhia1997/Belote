using UnityEngine;

public class ChooserRegistry : MonoBehaviour
{
    [Header("Seat → Chooser")]
    public SeatId localSeat = SeatId.South;
    public HumanCardChooser humanChooser;
    public AICardChooser aiChooserPrefab; // optional if you want to spawn per seat

    // Simple accessor: return human for local seat; AI for others
    public ICardChooser GetChooser(SeatId seat)
    {
        if (seat == localSeat) return humanChooser;
        // You can hold dedicated instances per seat; for now share one AI
        return aiChooserPrefab;
    }
}
