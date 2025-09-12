using UnityEngine;

[CreateAssetMenu(fileName = "TeamMap", menuName = "Belote/Rules/Team Map")]
public class TeamMapSO : ScriptableObject, ITeamMap
{
    [System.Serializable]
    public struct SeatTeamPair
    {
        public SeatId seat;
        public TeamId team;
    }

    public SeatTeamPair[] mapping = new SeatTeamPair[4];

    public TeamId GetTeam(SeatId seat)
    {
        foreach (var pair in mapping)
            if (pair.seat == seat) return pair.team;

        Debug.LogWarning($"[TeamMapSO] Seat {seat} not mapped, defaulting to Us.");
        return TeamId.Us;
    }
}
