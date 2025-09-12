using System;
using System.Collections.Generic;

public interface ICardChooser
{
    void BeginChoose(RulesContext ctx, List<CardDefinitionSO> legal, SeatId seat);
    event Action<CardDefinitionSO> OnCardChosen;
    void Cancel();
}
