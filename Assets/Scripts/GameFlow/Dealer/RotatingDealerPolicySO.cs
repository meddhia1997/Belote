using UnityEngine;

[CreateAssetMenu(fileName = "RotatingDealerPolicy", menuName = "Belote/Rules/Dealer/Rotating")]
public class RotatingDealerPolicySO : ScriptableObject, IDealerPolicy
{
    public bool clockwise = true;

    public SeatId NextDealer(SeatId currentDealer)
    {
        return clockwise ? SeatRegistry.Next(currentDealer) 
                         : (SeatId)(((int)currentDealer + 3) % 4); // counter-clockwise
    }
}
