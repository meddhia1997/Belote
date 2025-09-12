using System;
using System.Collections.Generic;
using UnityEngine;

public class AICardChooser : MonoBehaviour, ICardChooser
{
    [Tooltip("Assign a ScriptableObject implementing IAgentAI (e.g., SimpleBeloteAI).")]
    public ScriptableObject aiAsset;

    private IAgentAI _ai;
    public event Action<CardDefinitionSO> OnCardChosen;

    void Awake()
    {
        _ai = aiAsset as IAgentAI;
        if (_ai == null) Debug.LogError("[AICardChooser] No valid AI assigned.");
    }

    public void BeginChoose(RulesContext ctx, List<CardDefinitionSO> legal, SeatId seat)
    {
        var choice = _ai != null ? _ai.ChooseCard(ctx, legal, seat) : (legal.Count > 0 ? legal[0] : null);
        OnCardChosen?.Invoke(choice);
    }

    public void Cancel() { }
}
