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

    [Header("Return 조건: 바닥(옵션)")]
    public bool useFloorTag = true;
    public string floorTag = "Floor";
    public LayerMask floorLayers;

    [Header("Return 조건: 멈춤(추천)")]
    public bool returnWhenStopped = true;
    public float linearSpeedThreshold = 0.15f;
    public float angularSpeedThreshold = 25f;
    public float stoppedTimeRequired = 0.35f;
    public float maxWaitAfterRelease = 6f;

    [Header("Return Settings")]
    public float returnDelayAfterHit = 0.1f;
    public float returnDuration = 0.6f;
    public bool disableRotationWhileHeld = true;

    [Header("Held 중 손과 충돌 무시(흔들림 방지)")]
    public bool ignoreHandCollisionsWhileHeld = true;

    [Header("Held 중 플레이어 캡슐(CharacterController)과 충돌 무시(밀림 방지)")]
    public bool ignorePlayerCapsuleWhileHeld = true;
    public CharacterController playerCC;

    OVRGrabbable grabbable;
    Rigidbody rb;
    K_Rotator rotator;

    // ✅ 추가된 변수: 음식 물리 제어용
    K_FoodPhysicsController foodPhysics;

    Vector3 homePos;
    Quaternion homeRot;

    bool wasGrabbed;
    bool waitingForFloor;
    bool returning;
    Coroutine returnCo;

    float stoppedTimer;
    float releaseTimer;

    Collider[] objCols;
    Collider[] handCols;
    bool playerIgnored;

    OVRGrabber currentGrabber;

    void Awake()
    {
        grabbable = GetComponent<OVRGrabbable>();
        rb = GetComponent<Rigidbody>();
        rotator = GetComponent<K_Rotator>();

        // ✅ 추가된 로직: 같은 오브젝트에 있는 FoodPhysicsController 가져오기
        foodPhysics = GetComponent<K_FoodPhysicsController>();

        objCols = GetComponentsInChildren<Collider>(true);

        if (playerCC == null)
        {
            var p = FindObjectOfType<OVRPlayerController>();
            if (p != null) playerCC = p.GetComponent<CharacterController>();
        }

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

        SetIdlePhysics();
    }

    void OnDisable()
    {
        RestoreHandCollisions();
        IgnorePlayerCapsule(false);
        currentGrabber = null;
    }

    void OnDestroy()
    {
        RestoreHandCollisions();
        IgnorePlayerCapsule(false);
        currentGrabber = null;
    }

    void Update()
    {
        bool grabbed = grabbable.isGrabbed;

        if (grabbed)
        {
            var gb = grabbable.grabbedBy;

            if (!wasGrabbed)
            {
                OnGrab(gb);
            }
            else
            {
                if (gb != null && gb != currentGrabber)
                    OnGrabberChanged(gb);
            }
        }
        else
        {
            if (wasGrabbed) OnRelease();

            if (returnWhenStopped && !returning)
                CheckStopAndReturn();
        }

        wasGrabbed = grabbed;
    }

    void OnGrab(OVRGrabber gb)
    {
        waitingForFloor = false;
        returning = false;

        stoppedTimer = 0f;
        releaseTimer = 0f;

        if (returnCo != null) StopCoroutine(returnCo);

        if (disableRotationWhileHeld && rotator != null)
            rotator.enabled = false;

        // ✅ 잡았을 때 음식 물리는 켜기 (손 안에서 출렁거리도록)
        if (foodPhysics != null) foodPhysics.enabled = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

        currentGrabber = gb;

        if (ignoreHandCollisionsWhileHeld && currentGrabber != null)
            IgnoreHandCollisions(currentGrabber);

        if (ignorePlayerCapsuleWhileHeld)
            IgnorePlayerCapsule(true);
    }

    void OnGrabberChanged(OVRGrabber newGrabber)
    {
        RestoreHandCollisions();
        currentGrabber = newGrabber;

        if (ignoreHandCollisionsWhileHeld && currentGrabber != null)
            IgnoreHandCollisions(currentGrabber);

        if (ignorePlayerCapsuleWhileHeld)
            IgnorePlayerCapsule(true);
    }

    void OnRelease()
    {
        RestoreHandCollisions();
        IgnorePlayerCapsule(false);
        currentGrabber = null;

        rb.isKinematic = false;
        rb.useGravity = true;

        waitingForFloor = !returnWhenStopped;
        stoppedTimer = 0f;
        releaseTimer = 0f;
    }

    void CheckStopAndReturn()
    {
        if (rb.isKinematic) return;

        releaseTimer += Time.deltaTime;

        float v = rb.velocity.magnitude;
        float wDeg = rb.angularVelocity.magnitude * Mathf.Rad2Deg;

        bool slow = (v <= linearSpeedThreshold) && (wDeg <= angularSpeedThreshold);

        if (slow) stoppedTimer += Time.deltaTime;
        else stoppedTimer = 0f;

        if (stoppedTimer >= stoppedTimeRequired || releaseTimer >= maxWaitAfterRelease)
        {
            StartReturn();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (returnWhenStopped) return;

        if (!waitingForFloor) return;
        if (grabbable.isGrabbed) return;
        if (returning) return;

        if (IsFloor(collision.collider))
        {
            waitingForFloor = false;
            StartReturn();
        }
    }

    void StartReturn()
    {
        if (returning) return;

        returning = true;
        waitingForFloor = false;

        if (returnCo != null) StopCoroutine(returnCo);
        returnCo = StartCoroutine(ReturnRoutine());
    }

    bool IsFloor(Collider col)
    {
        if (useFloorTag)
            return col.CompareTag(floorTag);

        return ((1 << col.gameObject.layer) & floorLayers.value) != 0;
    }

    IEnumerator ReturnRoutine()
    {
        if (returnDelayAfterHit > 0f)
            yield return new WaitForSeconds(returnDelayAfterHit);

        // ✅ 추가된 로직: 귀환 시작 시 물리 끄기 (빠른 이동 시 찌그러짐 방지)
        if (foodPhysics != null) foodPhysics.enabled = false;

        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;

        Vector3 targetPos = (home != null) ? home.position : homePos;
        Quaternion targetRot = (home != null) ? home.rotation : homeRot;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

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

        RestoreHandCollisions();
        IgnorePlayerCapsule(false);
        currentGrabber = null;

        SetIdlePhysics();

        if (rotator != null) rotator.enabled = true;

        // ✅ 추가된 로직: 귀환 완료 후 물리 다시 켜기
        if (foodPhysics != null)
        {
            foodPhysics.enabled = true;
        }

        returning = false;
    }

    void SetIdlePhysics()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = idleUseGravity;
        rb.isKinematic = idleKinematic;
    }

    void IgnoreHandCollisions(OVRGrabber grabber)
    {
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

    void IgnorePlayerCapsule(bool ignore)
    {
        if (!ignorePlayerCapsuleWhileHeld) return;
        if (playerCC == null || objCols == null) return;

        if (playerIgnored == ignore) return;

        foreach (var oc in objCols)
        {
            if (oc) Physics.IgnoreCollision(oc, playerCC, ignore);
        }

        playerIgnored = ignore;
    }
}