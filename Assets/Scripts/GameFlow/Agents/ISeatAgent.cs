using System.Collections;

public interface ISeatAgent
{
    SeatContext Context { get; }
    bool IsLocal { get; }

    // Called when a new round starts, if you need per-round state
    void OnRoundStart();

    // Later: coroutine to play a turn, or callback-based
    // IEnumerator PlayTurn(GameState state);
}
