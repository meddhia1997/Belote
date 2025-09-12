using UnityEngine;

[CreateAssetMenu(fileName = "ClassicScorePolicy", menuName = "Belote/Rules/Scoring/Classic")]
public class ClassicScorePolicySO : ScriptableObject, IScorePolicy
{
    [Header("Classic Bonuses")]
    public int lastTrickBonus = 10;
    public int beloteBonus = 20;

    public int LastTrickBonus() => lastTrickBonus;
    public int BeloteBonus() => beloteBonus;
}
