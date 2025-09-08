using System.Collections.Generic;

public interface ILegalMovePolicy
{
    List<CardDefinitionSO> GetLegalMoves(
        RulesContext ctx,
        IReadOnlyList<CardDefinitionSO> hand,
        SeatId seatToPlay
    );
}
