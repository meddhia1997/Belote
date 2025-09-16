namespace GameFlow.Bidding
{
    public interface IBiddingTelemetry
    {
        void TrackShown();
        void TrackSeatTurn(SeatId seat);
        void TrackBid(SeatId seat, Bid bid, bool accepted);
        void TrackFinished(Contract c);
    }
}
