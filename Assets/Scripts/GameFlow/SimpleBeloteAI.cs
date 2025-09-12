using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Very naive AI: just picks the first legal card.
/// Replace with heuristics later.
/// </summary>
[CreateAssetMenu(fileName="SimpleBeloteAI", menuName="Belote/AI/Simple AI")]
public class SimpleBeloteAI : ScriptableObject, IAgentAI
{
    public bool randomize = false;

    public CardDefinitionSO ChooseCard(RulesContext ctx, List<CardDefinitionSO> legal, SeatId seat)
    {
        if (legal == null || legal.Count == 0) return null;

        if (randomize)
        {
            int idx = Random.Range(0, legal.Count);
            return legal[idx];
        }

        // Default: play first
        return legal[0];
    }
}
