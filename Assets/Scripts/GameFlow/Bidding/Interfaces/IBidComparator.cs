namespace GameFlow.Bidding
{
    public interface IBidComparator
    {
        bool IsBetter(Bid candidate, Bid current);
    }
}
