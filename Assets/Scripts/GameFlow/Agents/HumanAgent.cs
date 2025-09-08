public class HumanAgent : ISeatAgent
{
    public SeatContext Context { get; }
    public bool IsLocal => true;

    public HumanAgent(SeatContext ctx) { Context = ctx; }
    public void OnRoundStart() { /* nothing yet */ }
}
