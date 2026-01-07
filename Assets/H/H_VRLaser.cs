using UnityEngine;

public class H_VRLaser : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float maxDistance = 3.0f;

    [Header("Raycast Mask (여기서 Grabbable 레이어 제외)")]
    public LayerMask rayMask = ~0; // 인스펙터에서 조절

    [Header("잡는 동안 레이저 끄기")]
    public bool disableWhileGripping = true;
    public bool useIndexTrigger = false;   // 검지 트리거로 끌지, 그립으로 끌지
    public float triggerThreshold = 0.55f;
    public bool isRightHand = true;        // 오른손 레이저면 true

    void Update()
    {
        if (lineRenderer == null) return;

        if (disableWhileGripping && IsGripping())
        {
            if (lineRenderer.enabled) lineRenderer.enabled = false;
            return;
        }
        else
        {
            if (!lineRenderer.enabled) lineRenderer.enabled = true;
        }

        lineRenderer.SetPosition(0, transform.position);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, rayMask, QueryTriggerInteraction.Ignore))
        {
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            lineRenderer.SetPosition(1, transform.position + transform.forward * maxDistance);
        }
    }

    bool IsGripping()
    {
        var c = isRightHand ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;

        OVRInput.Axis1D axis;
        if (useIndexTrigger)
            axis = isRightHand ? OVRInput.Axis1D.SecondaryIndexTrigger : OVRInput.Axis1D.PrimaryIndexTrigger;
        else
            axis = isRightHand ? OVRInput.Axis1D.SecondaryHandTrigger : OVRInput.Axis1D.PrimaryHandTrigger;

        return OVRInput.Get(axis, c) >= triggerThreshold;
    }
}
