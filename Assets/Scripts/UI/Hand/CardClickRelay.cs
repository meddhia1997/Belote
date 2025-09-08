using UnityEngine;
using UnityEngine.EventSystems;

public class CardClickRelay : MonoBehaviour, IPointerClickHandler
{
    public LocalHandInput input;  // target (exists only on local seat)
    public CardView card;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (input) input.OnCardTapped(card);
    }
}
