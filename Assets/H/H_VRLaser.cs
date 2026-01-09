using UnityEngine;

public class H_VRLaser : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float maxDistance = 3.0f;

    [Header("Raycast Mask (여기서 Grabbable 레이어 제외)")]
    public LayerMask rayMask = ~0;

    [Header("잡는 동안 레이저 끄기")]
    public bool disableWhileGripping = true;
    public OVRGrabber grabber;   // ✅ 오른손이면 RightHandAnchor의 OVRGrabber 드래그

    void Awake()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (lineRenderer == null) return;

        // ✅ 입력값 대신 "실제로 뭔가 들고 있으면" 레이저 끄기
        if (disableWhileGripping && grabber != null && IsHoldingSomething(grabber))
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;

        lineRenderer.SetPosition(0, transform.position);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, rayMask, QueryTriggerInteraction.Ignore))
            lineRenderer.SetPosition(1, hit.point);
        else
            lineRenderer.SetPosition(1, transform.position + transform.forward * maxDistance);
    }

    bool IsHoldingSomething(OVRGrabber g)
    {
        // Oculus Integration 버전에 따라 grabbedObject가 public일 수도 있고 아닐 수도 있어서
        // 가장 흔한 케이스(프로퍼티) 먼저 시도:
        var prop = g.GetType().GetProperty("grabbedObject");
        if (prop != null) return prop.GetValue(g) != null;

        // 버전에 따라 private 필드명이 다를 수 있음 (여기까지 오면 그냥 입력기반으로 처리하는 게 낫긴 함)
        return false;
    }
}
