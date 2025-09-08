using System.Collections;
using UnityEngine;

public class UIAnimationService : MonoBehaviour, IUIAnimationService
{
    public IEnumerator SelectUp(RectTransform rt, CardAnimSettingsSO cfg)
    {
        Vector3 startScale = rt.localScale;
        Vector3 targetScale = Vector3.one * cfg.selectScale;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 targetPos = startPos + new Vector2(0, cfg.selectLift);

        yield return Tween(cfg.selectTime, cfg, (a) => {
            rt.localScale = Vector3.LerpUnclamped(startScale, targetScale, a);
            rt.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, a);
        });
    }

    public IEnumerator SelectDown(RectTransform rt, CardAnimSettingsSO cfg)
    {
        Vector3 startScale = rt.localScale;
        Vector3 targetScale = Vector3.one;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 targetPos = startPos + new Vector2(0, -cfg.selectLift);

        yield return Tween(cfg.selectTime, cfg, (a) => {
            rt.localScale = Vector3.LerpUnclamped(startScale, targetScale, a);
            rt.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, a);
        });
    }

    public IEnumerator MoveTo(RectTransform rt, Vector2 targetAnchoredPos, CardAnimSettingsSO cfg)
    {
        Vector2 startPos = rt.anchoredPosition;
        Quaternion startRot = rt.localRotation;
        float time = cfg.playTime;

        yield return Tween(time, cfg, (a) => {
            rt.anchoredPosition = Vector2.LerpUnclamped(startPos, targetAnchoredPos, a);
            if (cfg.straightenRotationOnMove)
                rt.localRotation = Quaternion.Slerp(startRot, Quaternion.identity, a);
        });
    }

    public void ReparentToCanvasKeepScreenPos(RectTransform rt, RectTransform canvas)
    {
        Vector3 world = rt.position;
        rt.SetParent(canvas, true);
        rt.position = world;
    }

    // --- helpers ---
    static IEnumerator Tween(float duration, CardAnimSettingsSO cfg, System.Action<float> onStep)
    {
        if (duration <= 0f) { onStep?.Invoke(1f); yield break; }
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            float e = cfg?.ease != null ? cfg.ease.Evaluate(a) : a;
            onStep?.Invoke(e);
            yield return null;
        }
        onStep?.Invoke(1f);
    }
}
