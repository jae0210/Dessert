using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class J_UIRaycastDebug : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        var es = EventSystem.current;
        if (!es) { Debug.LogError("No EventSystem"); return; }

        var data = new PointerEventData(es) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        es.RaycastAll(data, results);

        Debug.Log($"[UIRaycast] hits = {results.Count}");
        for (int i = 0; i < Mathf.Min(results.Count, 10); i++)
        {
            Debug.Log($"  {i}: {results[i].gameObject.name} (module={results[i].module})");
        }
    }
}
