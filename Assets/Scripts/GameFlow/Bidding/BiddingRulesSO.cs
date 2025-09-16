using UnityEngine;
using GameFlow.Bidding;

[CreateAssetMenu(fileName = "BiddingRules", menuName = "Belote/Bidding/Rules")]
public class BiddingRulesSO : ScriptableObject
{
    [Header("Policies")]
    public ScriptableObject orderPolicy;     // IBidOrderPolicy
    public ScriptableObject comparator;      // IBidComparator
    public ScriptableObject validator;       // IBidValidator
    public ScriptableObject evaluator;       // IBidEvaluator

    [Header("Timing")]
    [Range(0f, 2f)] public float aiThinkDelay = 0.35f;
    [Range(0f, 2f)] public float betweenTurnsDelay = 0.15f;

    public IBidOrderPolicy OrderPolicy => orderPolicy as IBidOrderPolicy;
    public IBidComparator Comparator   => comparator as IBidComparator;
    public IBidValidator Validator     => validator as IBidValidator;
    public IBidEvaluator Evaluator     => evaluator as IBidEvaluator;
}
