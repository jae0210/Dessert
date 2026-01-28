using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class H_VRLaser : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float maxDistance = 3.0f;

    [Header("잡는 동안 레이저 끄기")]
    public bool disableWhileGripping = true;
    public OVRGrabber grabber;

    // 재사용(할당 줄이기)
    private readonly List<RaycastResult> _uiHits = new List<RaycastResult>(16);

    void Awake()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (lineRenderer == null) return;

        // 들고 있으면 레이저 끄기
        if (disableWhileGripping && grabber != null && IsHoldingSomething(grabber))
        {
            lineRenderer.enabled = false;
            return;
        }

        // ✅ UI를 실제로 맞췄을 때만 레이저 켜기
        if (!TryGetUIHit(out Vector3 hitPoint))
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, hitPoint);
    }

    bool TryGetUIHit(out Vector3 hitPoint)
    {
        hitPoint = transform.position + transform.forward * maxDistance;

        if (EventSystem.current == null) return false;

        // OVRRaycaster는 OVRPointerEventData의 worldSpaceRay를 사용함
        var eventData = new OVRPointerEventData(EventSystem.current)
        {
            pointerId = -1,
            position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f),
            worldSpaceRay = new Ray(transform.position, transform.forward)
        };

        _uiHits.Clear();
        EventSystem.current.RaycastAll(eventData, _uiHits);

        float bestDist = float.PositiveInfinity;
        bool found = false;

        // 여러 Raycaster(PhysicsRaycaster 등) 결과가 섞일 수 있어서 OVRRaycaster 결과만 고름
        for (int i = 0; i < _uiHits.Count; i++)
        {
            var r = _uiHits[i];
            if (r.gameObject == null) continue;
            if (!(r.module is OVRRaycaster)) continue;   // ✅ UI만

            if (r.distance < bestDist)
            {
                bestDist = r.distance;
                hitPoint = r.worldPosition;
                found = true;
            }
        }

        // OVRRaycaster가 worldPosition을 못 채우는 경우 대비(드물게 0 벡터로 들어오는 경우)
        if (found && bestDist < float.PositiveInfinity)
        {
            // worldPosition이 0일 때는 Ray로 다시 계산
            if (hitPoint == Vector3.zero)
                hitPoint = transform.position + transform.forward * Mathf.Min(bestDist, maxDistance);

            return true;
        }

        return false;
    }

    bool IsHoldingSomething(OVRGrabber g)
    {
        var prop = g.GetType().GetProperty("grabbedObject");
        if (prop != null) return prop.GetValue(g) != null;
        return false;
    }
}
