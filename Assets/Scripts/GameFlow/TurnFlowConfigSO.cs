using UnityEngine;

[CreateAssetMenu(fileName="TurnFlowConfig", menuName="Belote/Game/Turn Flow Config")]
public class TurnFlowConfigSO : ScriptableObject
{
    [Header("General")]
    public SeatId startingDealer = SeatId.East;
    public bool clockwise = true;

    [Header("Timing")]
    public float afterPlayDelay = 0.2f;
    public float afterTrickDelay = 0.6f;
    public float humanTurnTimeout = 0f; // 0 = disabled
}
