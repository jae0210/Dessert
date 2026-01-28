using UnityEngine;
using System.Collections;

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

    [Header("Haptics (진동)")]
    public bool enableHaptics = true;
    public float grabHapticAmp = 0.6f;      // 0~1
    public float grabHapticFreq = 0.9f;     // 0~1 (체감상 큰 의미는 적지만 가능)
    public float grabHapticDuration = 0.08f;

    public float secondaryHapticAmp = 0.4f;
    public float secondaryHapticFreq = 0.9f;
    public float secondaryHapticDuration = 0.06f;

    OVRGrabbable grabbable;
    OVRGrabber primary;
    OVRGrabber secondary;

    Quaternion rotOffset;
    Collider mainCol;

    bool wasGrabbed = false;   // ✅ 잡힘 상태 변화 감지용

    void Awake()
    {
        grabbable = GetComponent<OVRGrabbable>();
        mainCol = GetComponent<Collider>();
    }

    void Update()
    {
        // ✅ "잡는 순간" 진동 (안 잡힘 -> 잡힘)
        if (!wasGrabbed && grabbable.isGrabbed)
        {
            var p = grabbable.grabbedBy;
            if (enableHaptics && p != null)
                StartCoroutine(HapticPulse(GetController(p), grabHapticFreq, grabHapticAmp, grabHapticDuration));
        }

        // ✅ 놓는 순간 정리
        if (wasGrabbed && !grabbable.isGrabbed)
        {
            primary = null;
            secondary = null;

            // 혹시 남아있을 진동 끄기
            StopAllCoroutines();
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }

        wasGrabbed = grabbable.isGrabbed;

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

                // ✅ 보조 손 붙는 순간 진동
                if (enableHaptics)
                    StartCoroutine(HapticPulse(GetController(secondary), secondaryHapticFreq, secondaryHapticAmp, secondaryHapticDuration));

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

    // ---- Haptics helpers ----
    OVRInput.Controller GetController(OVRGrabber g)
    {
        return (g == rightGrabber) ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
    }

    IEnumerator HapticPulse(OVRInput.Controller controller, float freq, float amp, float duration)
    {
        OVRInput.SetControllerVibration(freq, amp, controller);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, controller);
    }
}
