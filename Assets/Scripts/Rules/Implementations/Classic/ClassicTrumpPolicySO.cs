using UnityEngine;

[CreateAssetMenu(fileName="ClassicTrumpPolicy", menuName="Belote/Rules/Policies/Trump/Classic")]
public class ClassicTrumpPolicySO : ScriptableObject, ITrumpPolicy
{
    [SerializeField] private Suit trump = Suit.Hearts; // will be set by bidding later
    public Suit CurrentTrump => trump;

    // you can add a public method later to set during bidding
    // public void SetTrump(Suit s) => trump = s;
}
