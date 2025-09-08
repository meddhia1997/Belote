using UnityEngine;

[CreateAssetMenu(fileName="ClassicOrderingPolicy", menuName="Belote/Rules/Policies/Ordering/Classic")]
public class ClassicOrderingPolicySO : ScriptableObject, IOrderingPolicy
{
    public string[] orderAtTrump = { "J","9","A","10","K","Q","8","7" };
    public string[] orderOff     = { "A","10","K","Q","J","9","8","7" };

    public int GetOrderValue(string rank, bool atTrump)
    {
        var order = atTrump ? orderAtTrump : orderOff;
        for (int i=0;i<order.Length;i++)
            if (order[i]==rank) return order.Length - i;
        return 0;
    }
}
