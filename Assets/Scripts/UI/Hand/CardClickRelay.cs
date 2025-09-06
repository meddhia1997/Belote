using UnityEngine;
using UnityEngine.EventSystems;

public class CardClickRelay : MonoBehaviour, IPointerClickHandler
{
    public HandController controller;
    public CardView card;

    public void OnPointerClick(PointerEventData eventData)
    {
            Debug.Log("Clicked " + gameObject.name);

        controller.OnCardTapped(card);
    }
}
