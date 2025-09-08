using UnityEngine;

public interface IHandView
{
    RectTransform HandAnchor { get; }
    void ClearHand();
    void AddCard(CardView cv);
    void LayoutFan();
    void SetInteractable(bool interactable);
}

[System.Serializable]
public class SeatContext
{
    public SeatId Id;
    public TeamId Team;
    public bool IsLocal;            // true if the human player sits here
    public HandController Hand;     // implements IHandView

    // Runtime
    public ISeatAgent Agent;
}
