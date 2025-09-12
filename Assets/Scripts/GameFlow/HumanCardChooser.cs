using System;
using System.Collections.Generic;
using UnityEngine;

public class HumanCardChooser : MonoBehaviour, ICardChooser
{
    [Tooltip("Local player input on the local seat.")]
    public LocalHandInput localInput;

    private HashSet<CardDefinitionSO> _legal = new();
    private bool _active;

    public event Action<CardDefinitionSO> OnCardChosen;

    void Awake()
    {
        if (!localInput) localInput = FindObjectOfType<LocalHandInput>(includeInactive: true);
        if (localInput) localInput.OnConfirmPlay += HandleConfirm;
    }

    public void BeginChoose(RulesContext ctx, List<CardDefinitionSO> legal, SeatId seat)
    {
        _active = true;
        _legal = new HashSet<CardDefinitionSO>(legal);
        if (localInput) localInput.SetLegal(_legal);
    }

    public void Cancel()
    {
        _active = false;
        _legal.Clear();
        if (localInput) localInput.SetLegal(null);
    }

    private void HandleConfirm(CardDefinitionSO chosen)
    {
        if (!_active) return;
        if (_legal.Count > 0 && !_legal.Contains(chosen)) return; // block illegal
        _active = false;
        OnCardChosen?.Invoke(chosen);
    }
}
