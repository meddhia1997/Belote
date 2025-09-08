public class AIAgent : ISeatAgent
{
    public SeatContext Context { get; }
    public bool IsLocal => false;

    public AIAgent(SeatContext ctx) { Context = ctx; }
    public void OnRoundStart() { /* reset AI state */ }
    // public IEnumerator PlayTurn(GameState state) { ... }
}
