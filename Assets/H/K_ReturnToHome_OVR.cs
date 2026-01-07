using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(OVRGrabbable))]
public class K_ReturnToHome_OVR : MonoBehaviour
{
    [Header("Home(원위치)")]
    public Transform home;
    public bool resetRotation = true;

    [Header("Idle(전시 상태) 물리")]
    public bool idleKinematic = true;
    public bool idleUseGravity = false;

    [Header("Return 조건: 바닥")]
    public bool useFloorTag = true;
    public string floorTag = "Floor";
    public LayerMask floorLayers;

    [Header("Return Settings")]
    public float returnDelayAfterHit = 0.1f;
    public float returnDuration = 0.6f;
    public bool disableRotationWhileHeld = true;

    [Header("Held 중 손과 충돌 무시(흔들림 방지)")]
    public bool ignoreHandCollisionsWhileHeld = true;

    OVRGrabbable grabbable;
    Rigidbody rb;
    K_Rotator rotator;

    Vector3 homePos;
    Quaternion homeRot;

    bool wasGrabbed;
    bool waitingForFloor;
    bool returning;
    Coroutine returnCo;

    // 충돌 무시용
    Collider[] objCols;
    Collider[] handCols;

    void Awake()
    {
        grabbable = GetComponent<OVRGrabbable>();
        rb = GetComponent<Rigidbody>();
        rotator = GetComponent<K_Rotator>();

        objCols = GetComponentsInChildren<Collider>(true);

        if (home == null)
        {
            homePos = transform.position;
            homeRot = transform.rotation;
        }
        else
        {
            homePos = home.position;
            homeRot = home.rotation;
        }

        SetIdlePhysics(); // 시작은 전시 상태
    }

    void OnDestroy()
    {
        RestoreHandCollisions();
    }

    void Update()
    {
        bool grabbed = grabbable.isGrabbed;

        if (grabbed && !wasGrabbed) OnGrab();
        if (!grabbed && wasGrabbed) OnRelease();

        wasGrabbed = grabbed;
    }

    void OnGrab()
    {
        waitingForFloor = false;
        returning = false;

        if (returnCo != null) StopCoroutine(returnCo);

        if (disableRotationWhileHeld && rotator != null)
            rotator.enabled = false;

        // ✅ 잡는 동안엔 손에 안정적으로 붙게(물리 충돌로 흔들리지 않게)
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        // ✅ 손 콜라이더와 충돌 무시(있으면 흔들림/밀림 크게 줄어듦)
        if (ignoreHandCollisionsWhileHeld)
            IgnoreHandCollisions();
    }

    void OnRelease()
    {
        // ✅ 놓는 순간엔 다시 물리 켜서 떨어지고/던져지게
        RestoreHandCollisions();

        rb.isKinematic = false;
        rb.useGravity = true;

        // 바닥에 닿을 때까지 기다림
        waitingForFloor = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!waitingForFloor) return;
        if (grabbable.isGrabbed) return;
        if (returning) return;

        if (IsFloor(collision.collider))
        {
            waitingForFloor = false;

            if (returnCo != null) StopCoroutine(returnCo);
            returnCo = StartCoroutine(ReturnRoutine());
        }
    }

    bool IsFloor(Collider col)
    {
        if (useFloorTag)
            return col.CompareTag(floorTag);

        return ((1 << col.gameObject.layer) & floorLayers.value) != 0;
    }

    IEnumerator ReturnRoutine()
    {
        returning = true;
        yield return new WaitForSeconds(returnDelayAfterHit);

        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;

        Vector3 targetPos = (home != null) ? home.position : homePos;
        Quaternion targetRot = (home != null) ? home.rotation : homeRot;

        // 복귀 중엔 물리 멈추고 이동
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        float t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / returnDuration);

            transform.position = Vector3.Lerp(fromPos, targetPos, k);
            if (resetRotation) transform.rotation = Quaternion.Slerp(fromRot, targetRot, k);

            yield return null;
        }

        transform.position = targetPos;
        if (resetRotation) transform.rotation = targetRot;

        SetIdlePhysics();

        if (rotator != null) rotator.enabled = true;

        returning = false;
    }

    void SetIdlePhysics()
    {
        rb.isKinematic = idleKinematic;
        rb.useGravity = idleUseGravity;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void IgnoreHandCollisions()
    {
        var grabber = grabbable.grabbedBy;
        if (grabber == null) return;

        handCols = grabber.GetComponentsInChildren<Collider>(true);
        foreach (var hc in handCols)
            foreach (var oc in objCols)
            {
                if (hc && oc) Physics.IgnoreCollision(hc, oc, true);
            }
    }

    void RestoreHandCollisions()
    {
        if (handCols == null) return;

        foreach (var hc in handCols)
            foreach (var oc in objCols)
            {
                if (hc && oc) Physics.IgnoreCollision(hc, oc, false);
            }
        handCols = null;
    }
}
