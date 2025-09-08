public interface IRulesProfile
{
    ITrumpPolicy      TrumpPolicy      { get; }
    IOrderingPolicy   OrderingPolicy   { get; }
    IScoringPolicy    ScoringPolicy    { get; }
    ILegalMovePolicy  LegalMovePolicy  { get; }
    ITrickResolver    TrickResolver    { get; }
}
