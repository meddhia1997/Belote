public interface IOrderingPolicy
{
    // Higher value = stronger card
    int GetOrderValue(string rank, bool atTrump);
}
