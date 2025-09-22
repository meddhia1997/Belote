using UnityEngine;

[CreateAssetMenu(fileName="SaudiBalootTrumpPolicy", menuName="Belote/Rules/Policies/Trump/SaudiBaloot")]
public class SaudiBalootTrumpPolicySO : ScriptableObject, ITrumpPolicy
{
    [Tooltip("Set to Suit.None for Sun (no-trump). For Hokom, set a suit.")]
    [SerializeField] private Suit trump = Suit.None;

    public Suit CurrentTrump => trump;

    // If you wire bidding later:
     public void SetTrump(Suit s) => trump = s;
}
