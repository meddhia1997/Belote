using UnityEngine;

[CreateAssetMenu(fileName="ClassicScoringPolicy", menuName="Belote/Rules/Policies/Scoring/Classic")]
public class ClassicScoringPolicySO : ScriptableObject, IScoringPolicy
{
    [Header("Points At Trump")]
    public int J = 20; public int _9 = 14; public int A = 11; public int _10 = 10; public int K = 4; public int Q = 3; public int _8 = 0; public int _7 = 0;
    [Header("Points Off Trump")]
    public int A_off = 11; public int _10_off = 10; public int K_off = 4; public int Q_off = 3; public int J_off = 2; public int _9_off = 0; public int _8_off = 0; public int _7_off = 0;
    [Header("Bonuses")]
    [SerializeField] private int lastTrickBonus = 10;
    public int LastTrickBonus => lastTrickBonus;

    public int GetCardPoints(Suit suit, string rank, Suit trump)
    {
        bool atTrump = (suit == trump);
        if (atTrump)
        {
            return rank switch { "J"=>J, "9"=>_9, "A"=>A, "10"=>_10, "K"=>K, "Q"=>Q, "8"=>_8, "7"=>_7, _=>0 };
        }
        else
        {
            return rank switch { "A"=>A_off, "10"=>_10_off, "K"=>K_off, "Q"=>Q_off, "J"=>J_off, "9"=>_9_off, "8"=>_8_off, "7"=>_7_off, _=>0 };
        }
    }
}
