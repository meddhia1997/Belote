using UnityEngine;

[CreateAssetMenu(fileName = "HandLayoutSettings", menuName = "Belote/Hand Layout Settings")]
public class HandLayoutSettingsSO : ScriptableObject
{
    [Header("Fan")]
    public float arcDegrees = 90f;   // total span of fan
    public float radius = 220f;      // distance from anchor
    public bool rotateCards = true;  // rotate along arc
    public float overlapLift = 0f;   // if you want slight y bias per index

    [Header("Select FX")]
    public float selectLift = 22f;
    public float selectScale = 1.12f;
    public float selectTime = 0.08f;

    [Header("Play")]
    public float playTime = 0.45f;
}
