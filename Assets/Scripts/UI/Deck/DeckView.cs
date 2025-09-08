using UnityEngine;
using UnityEngine.UI;

public class DeckView : MonoBehaviour
{
    public RectTransform Anchor;   // optional (if null, uses this RectTransform)
    public Image PileImage;        // optional
    public Text CountLabel;        // optional (or TMP_Text)

    RectTransform _rt;

    void Awake()
    {
        _rt = (RectTransform)transform;
        if (!Anchor) Anchor = _rt;

        // Let clicks pass through
        var cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
    }

    public Vector3 GetWorldSpawnPos()
    {
        return (Anchor ? Anchor : _rt).position;
    }

    public Vector2 GetAnchoredSpawnPosIn(RectTransform targetParent)
    {
        var world = GetWorldSpawnPos();
        return (Vector2)targetParent.InverseTransformPoint(world);
    }

    public void SetBackSprite(Sprite back) { if (PileImage) PileImage.sprite = back; }
    public void SetCount(int remaining)    { if (CountLabel) CountLabel.text = remaining.ToString(); }
    public void Pulse() { /* optional micro FX if you want */ }
}
