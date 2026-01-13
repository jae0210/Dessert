using UnityEngine;

[RequireComponent(typeof(OVRGrabbable))]
public class K_TwoHandSupportGrip_OVR : MonoBehaviour
{
    [Header("Grabbers")]
    public OVRGrabber leftGrabber;
    public OVRGrabber rightGrabber;

    [Header("Grabber Grip Transforms (네가 만든 LeftGripTransform/RightGripTransform 넣기)")]
    public Transform leftGripTransform;
    public Transform rightGripTransform;

    [Header("보조 손 잡기 판정")]
    public float secondaryGrabDistance = 0.15f;
    public bool useIndexTrigger = false;   // true=검지 트리거, false=그립(HandTrigger)
    public float pressThreshold = 0.55f;

    OVRGrabbable grabbable;
    OVRGrabber primary;
    OVRGrabber secondary;

    Quaternion rotOffset;
    Collider mainCol;

    void Awake()
    {
        grabbable = GetComponent<OVRGrabbable>();
        mainCol = GetComponent<Collider>();
    }

    void Update()
    {
        if (!grabbable.isGrabbed)
        {
            primary = null;
            secondary = null;
            return;
        }

        primary = grabbable.grabbedBy;
        if (primary == null) return;

        var other = (primary == leftGrabber) ? rightGrabber : leftGrabber;
        if (other == null) return;

        // 보조 손 시작
        if (secondary == null)
        {
            if (IsPressed(other) && IsClose(other))
            {
                secondary = other;

                Vector3 dir0 = GetPos(secondary) - GetPos(primary);
                if (dir0.sqrMagnitude < 1e-6f) dir0 = primary.transform.forward;

                rotOffset = Quaternion.Inverse(Quaternion.LookRotation(dir0, Vector3.up)) * transform.rotation;
            }
        }
        else
        {
            // 보조 손 해제
            if (!IsPressed(secondary) || !IsClose(secondary))
                secondary = null;
        }
    }

    void LateUpdate()
    {
        if (primary == null || secondary == null) return;

        Vector3 dir = GetPos(secondary) - GetPos(primary);
        if (dir.sqrMagnitude < 1e-6f) return;

        transform.rotation = Quaternion.LookRotation(dir, Vector3.up) * rotOffset;
    }

    bool IsClose(OVRGrabber g)
    {
        Vector3 p = GetPos(g);

        if (mainCol != null)
        {
            Vector3 cp = mainCol.ClosestPoint(p);
            return Vector3.Distance(cp, p) <= secondaryGrabDistance;
        }

        return Vector3.Distance(transform.position, p) <= secondaryGrabDistance;
    }

    Vector3 GetPos(OVRGrabber g)
    {
        // ✅ OVRGrabber 내부 gripTransform에 접근하지 말고, 직접 넣은 Transform을 사용
        if (g == leftGrabber && leftGripTransform != null) return leftGripTransform.position;
        if (g == rightGrabber && rightGripTransform != null) return rightGripTransform.position;

        return g.transform.position;
    }

    bool IsPressed(OVRGrabber g)
    {
        bool isRight = (g == rightGrabber);
        var controller = isRight ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;

        OVRInput.Axis1D axis;
        if (useIndexTrigger)
            axis = isRight ? OVRInput.Axis1D.SecondaryIndexTrigger : OVRInput.Axis1D.PrimaryIndexTrigger;
        else
            axis = isRight ? OVRInput.Axis1D.SecondaryHandTrigger : OVRInput.Axis1D.PrimaryHandTrigger;

        return OVRInput.Get(axis, controller) >= pressThreshold;
    }
}
