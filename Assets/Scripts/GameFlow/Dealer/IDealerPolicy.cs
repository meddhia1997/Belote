public interface IDealerPolicy
{
    SeatId NextDealer(SeatId currentDealer);
}
