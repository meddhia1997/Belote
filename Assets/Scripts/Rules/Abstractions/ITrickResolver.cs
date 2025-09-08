public interface ITrickResolver
{
    // returns winner seat & total points of trick
    (SeatId winner, int points) ResolveTrick(RulesContext ctx);
}
