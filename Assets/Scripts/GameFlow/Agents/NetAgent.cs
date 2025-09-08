public class NetAgent : ISeatAgent
{
    public SeatContext Context { get; }
    public bool IsLocal => false;

    public NetAgent(SeatContext ctx) { Context = ctx; }
    public void OnRoundStart() { /* set up network bindings */ }
}
