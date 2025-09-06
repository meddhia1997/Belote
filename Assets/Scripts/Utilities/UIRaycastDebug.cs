using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIRaycastDebug : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ev = new PointerEventData(EventSystem.current){ position = Input.mousePosition };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ev, hits);
            foreach (var h in hits) Debug.Log($"Hit: {h.gameObject.name}");
        }
    }
}
