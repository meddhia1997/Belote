using System.Collections.Generic;

public interface IAgentAI
{
    /// <summary>
    /// Pick a card to play from the list of legal moves.
    /// </summary>
    CardDefinitionSO ChooseCard(RulesContext ctx, List<CardDefinitionSO> legal, SeatId seat);
}
