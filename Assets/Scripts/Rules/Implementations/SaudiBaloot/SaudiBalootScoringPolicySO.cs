using UnityEngine;

[CreateAssetMenu(fileName="SaudiBalootScoringPolicy", menuName="Belote/Rules/Policies/Scoring/SaudiBaloot")]
public class SaudiBalootScoringPolicySO : ScriptableObject, IScoringPolicy
{
    [Header("Points At Trump (Hokom)")]
    public int J = 20; public int _9 = 14; public int A = 11; public int _10 = 10; public int K = 4; public int Q = 3; public int _8 = 0; public int _7 = 0;

    [Header("Points Off / Sun")]
    public int A_off = 11; public int _10_off = 10; public int K_off = 4; public int Q_off = 3; public int J_off = 2; public int _9_off = 0; public int _8_off = 0; public int _7_off = 0;

    [Header("Bonuses")]
    [SerializeField] private int lastTrickBonus = 10;
    public int LastTrickBonus => lastTrickBonus;

    [Header("Optional multipliers")]
    [Min(1)] public int hokomMultiplier = 1;
    [Min(1)] public int sunMultiplier = 1;

    public int GetCardPoints(Suit suit, string rank, Suit trump)
    {
        bool isSun = (trump == Suit.None);
        bool atTrump = (!isSun) && (suit == trump);

        int pts = atTrump
            ? (rank switch { "J"=>J, "9"=>_9, "A"=>A, "10"=>_10, "K"=>K, "Q"=>Q, "8"=>_8, "7"=>_7, _=>0 })
            : (rank switch { "A"=>A_off, "10"=>_10_off, "K"=>K_off, "Q"=>Q_off, "J"=>J_off, "9"=>_9_off, "8"=>_8_off, "7"=>_7_off, _=>0 });

        return (isSun ? sunMultiplier : hokomMultiplier) * pts;
    }
}
