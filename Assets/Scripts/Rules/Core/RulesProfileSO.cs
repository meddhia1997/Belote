using UnityEngine;

[CreateAssetMenu(fileName="RulesProfile", menuName="Belote/Rules/Profile")]
public class RulesProfileSO : ScriptableObject, IRulesProfile
{
    [Header("Policies")]
    public ScriptableObject trumpPolicy;     // ITrumpPolicy
    public ScriptableObject orderingPolicy;  // IOrderingPolicy
    public ScriptableObject scoringPolicy;   // IScoringPolicy
    public ScriptableObject legalMovePolicy; // ILegalMovePolicy
    public ScriptableObject trickResolver;   // ITrickResolver

    public ITrumpPolicy      TrumpPolicy     => trumpPolicy     as ITrumpPolicy;
    public IOrderingPolicy   OrderingPolicy  => orderingPolicy  as IOrderingPolicy;
    public IScoringPolicy    ScoringPolicy   => scoringPolicy   as IScoringPolicy;
    public ILegalMovePolicy  LegalMovePolicy => legalMovePolicy as ILegalMovePolicy;
    public ITrickResolver    TrickResolver   => trickResolver   as ITrickResolver;
}
