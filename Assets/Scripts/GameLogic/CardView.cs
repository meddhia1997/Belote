using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Single-image card view (no back, no Button). 
/// Assign 'cardImage' in the prefab. Works with CardClickRelay for tap.
/// </summary>
public class CardView : MonoBehaviour
{
    [Header("Card Image")]
    [Tooltip("Drag the Image that displays the card face sprite.")]
    public Image cardImage;   // <- REQUIRED

    private CanvasGroup _cg;
    private CardDefinitionSO _data;

    void Awake()
    {
        // Ensure we have a CanvasGroup so we can block raycasts when needed
        _cg = GetComponent<CanvasGroup>();
        if (!_cg) _cg = gameObject.AddComponent<CanvasGroup>();

        // If the prefab forgot to enable raycasts, make sure the main image can receive clicks
        if (cardImage) cardImage.raycastTarget = true;
    }

    /// <summary>Assigns the ScriptableObject and updates the sprite.</summary>
    public void SetCard(CardDefinitionSO card)
    {
        _data = card;
        if (_data != null && _data.CardSprite != null)
        {
            if (!cardImage)
                Debug.LogError("[CardView] 'cardImage' is not assigned on the prefab.");
            else
                cardImage.sprite = _data.CardSprite;
        }
        else
        {
            Debug.LogWarning("[CardView] Missing sprite on CardDefinitionSO.");
        }
    }

    /// <summary>Master switch for input & raycasts. When false, card is display-only.</summary>
    public void SetInteractable(bool interactable)
    {
        if (_cg) _cg.blocksRaycasts = interactable;

        // Also toggle raycastTarget on all Graphics under this object
        var graphics = GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics) g.raycastTarget = interactable;

        // Disable the click relay if present (we use it for tap)
        var relay = GetComponent<CardClickRelay>();
        if (relay) relay.enabled = interactable;
    }

    public CardDefinitionSO GetData() => _data;
}
