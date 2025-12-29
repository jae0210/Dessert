using UnityEngine;

public class DoughHit : MonoBehaviour
{
    [Header("Settings")]
    public float squashAmount = 0.1f;      // 칠 때마다 납작해지는 정도
    public float minHeight = 0.1f;         // 최소 높이
    public float hitForceThreshold = 2.0f; // 타격 인정 최소 강도

    [Header("Game Rule")]
    public int maxHits = 10;               // [추가] 최대 타격 가능 횟수 (인스펙터에서 수정 가능)

    // 현재 타격 횟수 (외부에서 볼 필요 없으므로 private, 보고 싶다면 [SerializeField] 추가)
    private int currentHitCount = 0;

    [Header("Effects")]
    public ParticleSystem hitEffect;       // 타격 이펙트
    public AudioSource hitAudio;           // 타격 소리

    // 상태 저장을 위한 변수들
    private Vector3 originalScale;  // 최초 크기
    private Vector3 lastScale;      // 맞기 직전 크기 (되돌리기용)

    void Start()
    {
        originalScale = transform.localScale;
        lastScale = transform.localScale;
        currentHitCount = 0; // 시작할 때 0으로 초기화
    }

    void Update()
    {
        // 1. PC 테스트용 (키보드 R)
        if (Input.GetKeyDown(KeyCode.R))
        {
            UndoLastHit();
        }

        // 2. VR 왼쪽 컨트롤러 X 버튼 (되돌리기)
        // OVRInput.Button.Three가 일반적으로 'X' 버튼입니다.
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            UndoLastHit();
        }
    }

    // 망치가 떡을 통과하는 순간 감지 (Trigger)
    private void OnTriggerEnter(Collider other)
    {
        // [추가] 제한 횟수를 넘었으면 더 이상 반응하지 않음
        if (currentHitCount >= maxHits)
        {
            return;
        }

        if (other.CompareTag("Hammer"))
        {
            // 망치 속도 가져오기
            HammerPhysics hammerPhysics = other.GetComponent<HammerPhysics>();

            float impactForce = 0f;
            if (hammerPhysics != null) impactForce = hammerPhysics.currentSpeed;

            // 일정 힘 이상일 때만 반응
            if (impactForce > hitForceThreshold)
            {
                // [중요] 변하기 전 상태 저장
                lastScale = transform.localScale;

                // [추가] 타격 횟수 증가
                currentHitCount++;
                Debug.Log($"떡 타격! 현재 횟수: {currentHitCount} / {maxHits}");

                SquashDough();

                // 충돌 위치 계산 (이펙트용)
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                PlayEffects(hitPoint);
            }
        }
    }

    // 떡 납작하게 만들기
    void SquashDough()
    {
        Vector3 currentScale = transform.localScale;
        float newY = Mathf.Max(currentScale.y - squashAmount, minHeight);
        float spreadAmount = squashAmount * 0.5f;

        transform.localScale = new Vector3(
            currentScale.x + spreadAmount,
            newY,
            currentScale.z + spreadAmount
        );
    }

    // 직전 상태로 되돌리기 (Undo)
    public void UndoLastHit()
    {
        // 현재 스케일을 직전 스케일로 되돌림
        // (주의: 현재 코드는 1단계 Undo만 지원합니다)
        if (transform.localScale != lastScale)
        {
            transform.localScale = lastScale;

            // [추가] 횟수 차감 (0보다는 작아지지 않게)
            if (currentHitCount > 0)
            {
                currentHitCount--;
            }

            Debug.Log($"되돌리기 완료! 현재 횟수: {currentHitCount}");
        }
    }

    // 초기화 (Reset) - 필요 시 호출
    public void ResetDough()
    {
        transform.localScale = originalScale;
        lastScale = originalScale;
        currentHitCount = 0; // [추가] 횟수도 초기화
        Debug.Log("떡이 새것이 되었습니다.");
    }

    // 이펙트 재생 함수
    void PlayEffects(Vector3 hitPoint)
    {
        if (hitAudio != null) hitAudio.Play();

        if (hitEffect != null)
        {
            hitEffect.transform.position = hitPoint;
            hitEffect.Play();
        }
    }
}