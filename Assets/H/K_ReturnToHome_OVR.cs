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
    public float linearSpeedThreshold = 0.15f;     // m/s (씬 스케일에 맞게 조절)
    public float angularSpeedThreshold = 25f;      // deg/s
    public float stoppedTimeRequired = 0.35f;      // 이 시간 동안 계속 느리면 귀환
    public float maxWaitAfterRelease = 6f;         // 너무 오래 굴러가면 강제 귀환

    [Header("Return Settings")]
    public float returnDelayAfterHit = 0.1f;       // 멈춘 뒤/바닥 닿은 뒤 기다렸다 귀환
    public float returnDuration = 0.6f;
    public bool disableRotationWhileHeld = true;

    [Header("Held 중 손과 충돌 무시(흔들림 방지)")]
    public bool ignoreHandCollisionsWhileHeld = true;

    [Header("Held 중 플레이어 캡슐(CharacterController)과 충돌 무시(밀림 방지)")]
    public bool ignorePlayerCapsuleWhileHeld = true;
    public CharacterController playerCC; // 비워두면 자동 탐색(권장). 안 되면 수동 드래그.

    OVRGrabbable grabbable;
    Rigidbody rb;
    K_Rotator rotator;

    Vector3 homePos;
    Quaternion homeRot;

    bool wasGrabbed;
    bool waitingForFloor;
    bool returning;
    Coroutine returnCo;

    // 멈춤 감지 타이머
    float stoppedTimer;
    float releaseTimer;

    // 충돌 무시용 캐시
    Collider[] objCols;
    Collider[] handCols;
    bool playerIgnored;

    // 현재 잡고 있는 손(Grabber) 추적(Offhand Grab 전환 처리)
    OVRGrabber currentGrabber;

    void Awake()
    {
        grabbable = GetComponent<OVRGrabbable>();
        rb = GetComponent<Rigidbody>();
        rotator = GetComponent<K_Rotator>();

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
                // 잡고 있는 상태에서 손이 바뀌는(Offhand Grab) 경우 처리
                if (gb != null && gb != currentGrabber)
                    OnGrabberChanged(gb);
            }
        }
        else
        {
            if (wasGrabbed) OnRelease();

            // ✅ 놓인 상태에서 "멈추면 귀환"
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

        // 잡는 동안 안정(손에 붙는 느낌)
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
        // 이전 손 ignore 복구
        RestoreHandCollisions();

        currentGrabber = newGrabber;

        // 새 손 ignore 적용
        if (ignoreHandCollisionsWhileHeld && currentGrabber != null)
            IgnoreHandCollisions(currentGrabber);

        // 플레이어 캡슐 ignore는 잡는 동안 계속 유지
        if (ignorePlayerCapsuleWhileHeld)
            IgnorePlayerCapsule(true);
    }

    void OnRelease()
    {
        RestoreHandCollisions();
        IgnorePlayerCapsule(false);
        currentGrabber = null;

        // 놓는 순간엔 다시 물리 켜서 떨어지고/던져지게
        rb.isKinematic = false;
        rb.useGravity = true;

        // ✅ 바닥 기반 귀환 vs 멈춤 기반 귀환 선택
        waitingForFloor = !returnWhenStopped;

        stoppedTimer = 0f;
        releaseTimer = 0f;
    }

    void CheckStopAndReturn()
    {
        // 전시상태(kinematic)면 체크할 필요 없음
        if (rb.isKinematic) return;

        releaseTimer += Time.deltaTime;

        float v = rb.velocity.magnitude;
        float wDeg = rb.angularVelocity.magnitude * Mathf.Rad2Deg;

        bool slow = (v <= linearSpeedThreshold) && (wDeg <= angularSpeedThreshold);

        if (slow) stoppedTimer += Time.deltaTime;
        else stoppedTimer = 0f;

        // 일정 시간 이상 느리게 유지되거나 너무 오래 기다리면 귀환
        if (stoppedTimer >= stoppedTimeRequired || releaseTimer >= maxWaitAfterRelease)
        {
            StartReturn();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 멈춤 귀환을 쓰면 바닥 귀환은 보통 필요 없음(옵션 유지)
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
        // 멈춘 뒤/바닥 닿은 뒤 약간 텀 주기
        if (returnDelayAfterHit > 0f)
            yield return new WaitForSeconds(returnDelayAfterHit);

        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;

        Vector3 targetPos = (home != null) ? home.position : homePos;
        Quaternion targetRot = (home != null) ? home.rotation : homeRot;

        // 복귀 중엔 물리 멈추고 이동
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

        // 안전 복구
        RestoreHandCollisions();
        IgnorePlayerCapsule(false);
        currentGrabber = null;

        SetIdlePhysics();

        if (rotator != null) rotator.enabled = true;

        returning = false;
    }

    void SetIdlePhysics()
    {
        // 경고 줄이기: 속도 0 -> kinematic 세팅 순서
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
