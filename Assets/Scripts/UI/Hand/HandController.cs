using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Setup")]
    public RectTransform handAnchor;           // HandAnchor_South
    public RectTransform trickArea;            // TrickArea
    public HandLayoutSettingsSO settings;
    public CardView cardViewPrefab;            // assign your CardView.prefab

    private readonly List<CardView> _cards = new();
    private CardView _selected;

    #region Fan Layout
    public void LayoutFan()
    {
        int n = _cards.Count;
        if (n == 0) return;

        float arc = settings.arcDegrees;
        float start = -arc * 0.5f;
        float step = (n > 1) ? arc / (n - 1) : 0f;

        for (int i = 0; i < n; i++)
        {
            var cv = _cards[i];
            var rt = (RectTransform)cv.transform;

            float angDeg = start + i * step;
            // place on circle; 0° = to the right, so rotate by +90 to make 0° up:
            float theta = Mathf.Deg2Rad * (angDeg + 90f);

            Vector2 pos = new Vector2(
                settings.radius * Mathf.Cos(theta),
                settings.radius * Mathf.Sin(theta) + settings.overlapLift
            );

            rt.SetParent(handAnchor, false);
            rt.localScale = Vector3.one;
            rt.localRotation = settings.rotateCards ? Quaternion.Euler(0, 0, angDeg) : Quaternion.identity;
            rt.anchoredPosition = pos;
        }
    }
    #endregion

    #region Select / Deselect
    public void OnCardTapped(CardView card)
    {
        if (_selected == card)
        {
            // second tap: play
            StartCoroutine(PlayCardRoutine(card));
            return;
        }

        // change selection
        if (_selected != null) Deselect(_selected);
        Select(card);
    }

    void Select(CardView card)
    {
        _selected = card;
        var rt = (RectTransform)card.transform;
        rt.SetAsLastSibling(); // bring on top visually

        StopAllCoroutines();
        StartCoroutine(TweenSelect(rt, up:true));
    }

    void Deselect(CardView card)
    {
        var rt = (RectTransform)card.transform;
        StartCoroutine(TweenSelect(rt, up:false));
        _selected = null;
    }

    IEnumerator TweenSelect(RectTransform rt, bool up)
    {
        Vector3 startScale = rt.localScale;
        Vector3 targetScale = up ? Vector3.one * settings.selectScale : Vector3.one;

        Vector2 startPos = rt.anchoredPosition;
        Vector2 targetPos = startPos + new Vector2(0, up ? settings.selectLift : -settings.selectLift);

        float t = 0f;
        float d = settings.selectTime;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.SmoothStep(0,1,t/d);
            rt.localScale = Vector3.Lerp(startScale, targetScale, a);
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, a);
            yield return null;
        }
        rt.localScale = targetScale;
        rt.anchoredPosition = targetPos;
    }
    #endregion

    #region Play (move to trick, remove, relayout)
    IEnumerator PlayCardRoutine(CardView card)
    {
        // lock input
        card.SetInteractable(false);

        var rt = (RectTransform)card.transform;

        // reparent to top canvas to avoid being affected by hand rotation
        var canvas = handAnchor.GetComponentInParent<Canvas>().transform as RectTransform;
        Vector3 world = rt.position;
        rt.SetParent(canvas, true);
        rt.position = world;

        // animate to trick center (in canvas space)
        Vector2 start = ((RectTransform)rt).anchoredPosition;
        Vector2 target = trickArea.anchoredPosition;

        float t = 0f, d = settings.playTime;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.SmoothStep(0,1,t/d);
            ((RectTransform)rt).anchoredPosition = Vector2.Lerp(start, target, a);
            // optional: straighten rotation gradually
            rt.localRotation = Quaternion.Slerp(rt.localRotation, Quaternion.identity, a);
            yield return null;
        }
        ((RectTransform)rt).anchoredPosition = target;
        rt.localRotation = Quaternion.identity;

        // remove from hand list
        _cards.Remove(card);

        // optionally keep the played card in TrickArea (reparent)
        rt.SetParent(trickArea, false);

        // clear selection & relayout
        _selected = null;
        LayoutFan();

        // TODO: notify game flow to advance turn (offline AI next)
    }
    #endregion

    #region API for Bootstrap / Dealing
    public void ClearHand()
    {
        foreach (var cv in _cards) if (cv) Destroy(cv.gameObject);
        _cards.Clear();
        _selected = null;
    }

    public void AddCard(CardView cv)
    {
        // add click handler
        var click = cv.gameObject.AddComponent<CardClickRelay>();
        click.controller = this;
        click.card = cv;

        _cards.Add(cv);
    }
    #endregion
}
