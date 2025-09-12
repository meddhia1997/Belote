public enum TeamId { Us = 0, Them = 1 };

public static class SeatTeamUtils
{
    public static bool ArePartners(SeatId a, SeatId b)
    {
        return (a == SeatId.South && b == SeatId.North) ||
               (a == SeatId.North && b == SeatId.South) ||
               (a == SeatId.West && b == SeatId.East) ||
               (a == SeatId.East && b == SeatId.West);
    }
}
