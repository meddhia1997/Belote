using UnityEngine;

public class BidderRegistry : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        [Header("Seat")]
        public SeatId seat;

        [Header("Source Choice (one of)")]
        public ScriptableObject sourceSO; // must implement IBidSource
        public MonoBehaviour sourceMB;    // must implement IBidSource
    }

    public Entry south;
    public Entry west;
    public Entry north;
    public Entry east;

    public IBidSource Get(SeatId seat)
    {
        Entry e = seat switch
        {
            SeatId.South => south,
            SeatId.West  => west,
            SeatId.North => north,
            SeatId.East  => east,
            _ => south
        };

        IBidSource mb = e.sourceMB as IBidSource;
        if (mb != null) return mb;
        return e.sourceSO as IBidSource;
    }

    public void CancelAll()
    {
        Get(SeatId.South)?.Cancel();
        Get(SeatId.West )?.Cancel();
        Get(SeatId.North)?.Cancel();
        Get(SeatId.East )?.Cancel();
    }
}
