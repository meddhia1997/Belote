using UnityEngine;

[CreateAssetMenu(fileName = "MatchRules", menuName = "Belote/Rules/Match Rules")]
public class MatchRulesSO : ScriptableObject, IMatchRules
{
    public int targetPoints = 1000;
    public bool winByTwo = false;

    public int TargetPoints => targetPoints;
    public bool WinByTwo => winByTwo;
}
