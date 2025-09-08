using UnityEngine;

public interface IAgentFactory
{
    ISeatAgent Create(SeatContext ctx);
}

public enum SessionMode { OfflineSolo, OnlinePvP }

public class AgentFactory : MonoBehaviour, IAgentFactory
{
    [Header("Session Mode")]
    public SessionMode Mode = SessionMode.OfflineSolo;

    public ISeatAgent Create(SeatContext ctx)
    {
        if (Mode == SessionMode.OfflineSolo)
            return ctx.IsLocal ? new HumanAgent(ctx) : new AIAgent(ctx);

        // Online: local seat is human, others are network-driven
        return ctx.IsLocal ? new HumanAgent(ctx) : new NetAgent(ctx);
    }
}
