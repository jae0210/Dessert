using UnityEngine;

public class K_HammerAction : MonoBehaviour
{
    [Header("설정 값")]
    public float hitThreshold = 2.0f; // 타격 인정 최소 속도
    public float vibrationForce = 1.0f; // 진동 세기

    [Header("효과")]
    public AudioSource hitSound;      // 떡 치는 소리
    public GameObject hitEffect;      // 타격 파티클 효과
    public Transform effectSpawnPoint; // 이펙트 생성 위치

    // 내부 변수
    private Vector3 lastPosition;
    public float currentSpeed { get; private set; }
    private OVRGrabbable ovrGrabbable;

    void Start()
    {
        lastPosition = transform.position;
        ovrGrabbable = GetComponent<OVRGrabbable>();
    }

    void FixedUpdate()
    {
        // 속도 계산 로직
        float distance = Vector3.Distance(transform.position, lastPosition);
        currentSpeed = distance / Time.fixedDeltaTime;
        lastPosition = transform.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        // "Mochi" 태그를 가진 물체와 충돌했는지 확인
        if (collision.gameObject.CompareTag("Mochi"))
        {
            // 일정 속도 이상일 때만 타격 처리
            if (currentSpeed >= hitThreshold)
            {
                HitMochi(collision);
            }
        }
    }

    void HitMochi(Collision collision)
    {
        // A. 소리 재생
        if (hitSound != null) hitSound.Play();

        // B. 이펙트 생성
        if (hitEffect != null && effectSpawnPoint != null)
        {
            Instantiate(hitEffect, effectSpawnPoint.position, Quaternion.identity);
        }

        // C. 컨트롤러 진동 (햅틱)
        if (ovrGrabbable != null && ovrGrabbable.isGrabbed)
        {
            OVRInput.Controller grabbedHand = OVRInput.Controller.None;
            var grabber = ovrGrabbable.grabbedBy;

            if (grabber != null)
            {
                if (grabber.name.Contains("Left")) grabbedHand = OVRInput.Controller.LTouch;
                else if (grabber.name.Contains("Right")) grabbedHand = OVRInput.Controller.RTouch;

                OVRInput.SetControllerVibration(1, vibrationForce, grabbedHand);
                Invoke("StopVibration", 0.2f);
            }
        }

        // D. [수정됨] 떡 스크립트(K_MochiController)에 타격 신호 전달
        K_MochiController mochi = collision.gameObject.GetComponent<K_MochiController>();
        if (mochi != null)
        {
            // 떡에게 현재 망치의 속도(파워)를 전달
            mochi.OnHit(currentSpeed);
        }
    }

    void StopVibration()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }
}