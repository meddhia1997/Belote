using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    public Image cardImage; // drag your CardSprite here in prefab

    private CardDefinitionSO data;

    public void SetCard(CardDefinitionSO card)
    {
        data = card;
        if (data != null && data.CardSprite != null)
            cardImage.sprite = data.CardSprite;
        else
            Debug.LogWarning("[CardView] Missing sprite on CardDefinitionSO.");
    }
    public void SetInteractable(bool interactable)
{
    var cg = GetComponent<CanvasGroup>();
    if (cg) cg.blocksRaycasts = interactable;
}

}
