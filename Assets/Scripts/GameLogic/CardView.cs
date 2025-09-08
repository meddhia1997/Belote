using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Single-image card view. Uses the CardDefinitionSO's face/back sprites.
/// No Button required; taps are handled by CardClickRelay.
/// </summary>
public class CardView : MonoBehaviour
{
    [Header("Card Image")]
    [Tooltip("Drag the Image that displays the card sprite.")]
    public Image cardImage;      // required

    private CanvasGroup _cg;
    private CardDefinitionSO _data;
    private Sprite _faceSprite;
    private Sprite _backSprite;

    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        if (!_cg) _cg = gameObject.AddComponent<CanvasGroup>();

        if (cardImage) cardImage.raycastTarget = true;
    }

    /// <summary>Assigns data and shows FACE by default.</summary>
    public void SetCard(CardDefinitionSO data)
    {
        _data = data;
        _faceSprite = _data ? _data.CardSprite : null;
        _backSprite = _data ? (_data.CardBackSprite ?? _data.CardSprite) : null;

        if (!cardImage)
        {
            Debug.LogError("[CardView] 'cardImage' is not assigned.");
            return;
        }

        if (_faceSprite == null)
            Debug.LogWarning("[CardView] CardDefinitionSO has no CardSprite.");

        cardImage.sprite = _faceSprite; // default face-up
    }

    /// <summary>Flip using the single Image: faceUp = true shows face; false shows back.</summary>
    public void ShowFace(bool faceUp)
    {
        if (!cardImage) return;
        cardImage.sprite = faceUp ? _faceSprite : (_backSprite ?? _faceSprite);
    }

    /// <summary>Master switch for input & raycasts. When false, card is display-only.</summary>
    public void SetInteractable(bool interactable)
    {
        if (_cg) _cg.blocksRaycasts = interactable;

        var graphics = GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics) g.raycastTarget = interactable;

        var relay = GetComponent<CardClickRelay>();
        if (relay) relay.enabled = interactable;
    }

    public CardDefinitionSO GetData() => _data;
}
