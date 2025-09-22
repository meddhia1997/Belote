using UnityEngine;

[CreateAssetMenu(fileName="SaudiBalootOrderingPolicy", menuName="Belote/Rules/Policies/Ordering/SaudiBaloot")]
public class SaudiBalootOrderingPolicySO : ScriptableObject, IOrderingPolicy
{
    // Hokom (trump) order
    public string[] orderAtTrump = { "J","9","A","10","K","Q","8","7" };
    // Sun / off-trump order
    public string[] orderOff     = { "A","10","K","Q","J","9","8","7" };

    // Higher value = stronger
    public int GetOrderValue(string rank, bool atTrump)
    {
        var order = atTrump ? orderAtTrump : orderOff;
        for (int i=0;i<order.Length;i++)
            if (order[i]==rank) return order.Length - i;
        return 0;
    }
}
