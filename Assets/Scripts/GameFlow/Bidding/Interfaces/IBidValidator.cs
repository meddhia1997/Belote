namespace GameFlow.Bidding
{
    public interface IBidValidator
    {
        bool IsValid(Bid candidate, Bid current);
    }
}
