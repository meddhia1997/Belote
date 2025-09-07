using UnityEngine;

[CreateAssetMenu(fileName = "CardAnimSettings", menuName = "Belote/Card Anim Settings")]
public class CardAnimSettingsSO : ScriptableObject
{
    [Header("Select/Deselect")]
    public float selectLift = 22f;
    public float selectScale = 1.12f;
    public float selectTime  = 0.08f;

    [Header("Play/Move")]
    public float playTime    = 0.45f;
    public bool  straightenRotationOnMove = true;

    [Header("Easing")]
    public AnimationCurve ease = AnimationCurve.EaseInOut(0,0,1,1);
}
