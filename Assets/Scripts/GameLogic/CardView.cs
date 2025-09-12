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
    private bool _isFaceUp;

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
        // If CardBackSprite is not defined on the SO, we fallback to face sprite
        _backSprite = _data ? (_data.CardBackSprite != null ? _data.CardBackSprite : _data.CardSprite) : null;

        if (!cardImage)
        {
            Debug.LogError("[CardView] 'cardImage' is not assigned.");
            return;
        }

        if (_faceSprite == null)
            Debug.LogWarning("[CardView] CardDefinitionSO has no CardSprite.");

        // default to face-up in hand (deal logic may call ShowFace(false) for opponents)
        ShowFace(true);
    }

    /// <summary>Switch between face (true) or back (false).</summary>
    public void ShowFace(bool faceUp)
    {
        _isFaceUp = faceUp;
        if (!cardImage) return;
        if (faceUp && _faceSprite != null)
            cardImage.sprite = _faceSprite;
        else
            cardImage.sprite = _backSprite ?? _faceSprite;
    }

    /// <summary>Master switch for input & raycasts. When false, card is display-only.</summary>
    public void SetInteractable(bool interactable)
    {
        if (_cg) _cg.blocksRaycasts = interactable;

        // also toggle raycast on any child graphics if you add badges/highlights later
        var graphics = GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics) g.raycastTarget = interactable;

        var relay = GetComponent<CardClickRelay>();
        if (relay) relay.enabled = interactable;
    }

    public CardDefinitionSO GetData() => _data;
    public CardDefinitionSO GetCardDefinition() => _data;
    public bool IsFaceUp => _isFaceUp;
}
