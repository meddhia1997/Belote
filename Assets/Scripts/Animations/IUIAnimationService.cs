using System.Collections;
using UnityEngine;

public interface IUIAnimationService
{
    IEnumerator SelectUp(RectTransform rt, CardAnimSettingsSO cfg);
    IEnumerator SelectDown(RectTransform rt, CardAnimSettingsSO cfg);
    IEnumerator MoveTo(RectTransform rt, Vector2 targetAnchoredPos, CardAnimSettingsSO cfg);
    void ReparentToCanvasKeepScreenPos(RectTransform rt, RectTransform canvas);
}
