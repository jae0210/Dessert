using UnityEngine;
using UnityEngine.Events;

public enum FoodType
{
    Liquid, // 컵 안의 음료 (찰랑거림)
    Soft,   // 떡 (말랑함, 찌그러짐, 늘어남)
    Hard    // 사탕 (딱딱함, 변형 없음)
}

public class K_FoodPhysicsController : MonoBehaviour
{
    [Header("음식 타입 설정")]
    public FoodType foodType = FoodType.Soft;

    [Header("공통 설정")]
    public float impactThreshold = 2.0f; // 이 이상의 충격이어야 반응함
    public UnityEvent onImpact;          // 충돌 시 공통 이벤트 (소리 재생 등)

    [Header("Soft (떡/젤리) 설정")]
    public float elasticity = 5f;        // 복원력 (높을수록 빨리 원래 모양으로 돌아옴)
    public float wobbleAmount = 0.05f;   // 움직일 때 출렁거리는 정도
    public float maxSquash = 0.5f;       // 최대 찌그러짐 제한

    [Header("Liquid (음료) 설정")]
    public Transform liquidSurface;      // 컵 안의 액체 수면(Mesh) 오브젝트
    public float sloshSpeed = 1.0f;      // 찰랑거리는 속도
    public float sloshAmount = 60.0f;    // 찰랑거리는 최대 각도

    // 내부 변수
    private Vector3 initialScale;
    private Vector3 currentVelocity;
    private Vector3 lastPos;
    private Vector3 lastRot;
    private Quaternion targetLiquidRotation;

    void Start()
    {
        initialScale = transform.localScale;
        lastPos = transform.position;
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        switch (foodType)
        {
            case FoodType.Soft:
                ProcessSoftPhysics(deltaTime);
                break;
            case FoodType.Liquid:
                ProcessLiquidPhysics(deltaTime);
                break;
            case FoodType.Hard:
                // 딱딱한 물체는 변형 로직 없음 (필요 시 반짝임 효과 등 추가)
                break;
        }

        // 속도 계산 (이전 프레임 위치 기반)
        Vector3 velocity = (transform.position - lastPos) / deltaTime;
        currentVelocity = Vector3.Lerp(currentVelocity, velocity, deltaTime * 5f);
        lastPos = transform.position;
    }

    // 1. Soft 타입: 젤리 같은 탄성 및 이동 시 출렁임 처리
    void ProcessSoftPhysics(float deltaTime)
    {
        // 원래 크기로 돌아오려는 탄성 (Lerp를 이용한 복원)
        transform.localScale = Vector3.Lerp(transform.localScale, initialScale, deltaTime * elasticity);

        // 빠른 속도로 움직일 때 진행 방향으로 길어지고 얇아짐 (Squash & Stretch)
        if (currentVelocity.magnitude > 0.1f)
        {
            float stretchFactor = 1.0f + (currentVelocity.magnitude * wobbleAmount * 0.1f);
            Vector3 stretchScale = initialScale;

            // 간단한 시각적 연출: Y축은 늘리고 X, Z는 줄임 (부피 보존 느낌)
            // 실제로는 이동 방향에 맞춰 회전시켜야 하지만, 여기선 단순화함
            stretchScale.y *= stretchFactor;
            stretchScale.x /= stretchFactor;
            stretchScale.z /= stretchFactor;

            // 현재 스케일에 미세하게 반영 (너무 심하게 변하지 않도록)
            transform.localScale = Vector3.Lerp(transform.localScale, stretchScale, deltaTime * 5f);
        }
    }

    // 2. Liquid 타입: 컵 안의 액체 회전 처리
    void ProcessLiquidPhysics(float deltaTime)
    {
        if (liquidSurface == null) return;

        // 컵의 기울기 + 움직임에 따른 관성을 계산해 액체 표면을 회전시킴
        // 컵이 기울어져도 액체는 월드 기준 수평을 유지하려는 성질 + 관성

        Quaternion targetRot = Quaternion.FromToRotation(transform.up, Vector3.up);

        // 이동 관성 추가 (컵을 오른쪽으로 확 밀면 액체는 왼쪽으로 쏠림)
        Vector3 sloshVector = new Vector3(currentVelocity.z, 0, -currentVelocity.x) * sloshAmount * 0.01f;
        Quaternion sloshRot = Quaternion.Euler(sloshVector);

        // 컵의 로컬 회전과 반대로 액체를 회전시켜 수평 유지 느낌 구현
        Quaternion finalRot = Quaternion.Inverse(transform.rotation) * (Quaternion.LookRotation(Vector3.forward, Vector3.up) * sloshRot);

        // 부드럽게 회전 적용
        liquidSurface.localRotation = Quaternion.Slerp(liquidSurface.localRotation, finalRot, deltaTime * sloshSpeed);
    }

    // 3. 외부(VR 핸드)에서 호출: 잡아서 늘릴 때
    // stretchFactor: 0 (기본) ~ 1 (최대 늘어남)
    public void OnGrabAndStretch(float stretchFactor)
    {
        if (foodType != FoodType.Soft) return; // 떡만 늘어남

        // 늘어나는 로직
        Vector3 newScale = initialScale;
        newScale.y += stretchFactor;       // 길어짐
        newScale.x -= stretchFactor * 0.3f; // 얇아짐
        newScale.z -= stretchFactor * 0.3f; // 얇아짐

        transform.localScale = newScale;
    }

    // 충돌 감지 (던져서 맞았을 때)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude < impactThreshold) return;

        onImpact.Invoke(); // 공통 충돌 이벤트 (소리 등)

        if (foodType == FoodType.Soft)
        {
            // 떡은 충돌 시 납작해짐
            DeformOnImpact(collision.relativeVelocity.magnitude);
        }
        else if (foodType == FoodType.Liquid)
        {
            // 음료는 충돌 시 튀는 효과 (파티클 등)를 여기에 추가 가능
        }
    }

    void DeformOnImpact(float force)
    {
        // 충격량에 비례해 납작해짐
        float deformation = Mathf.Clamp(force * 0.05f, 0, maxSquash);

        Vector3 squashedScale = initialScale;
        squashedScale.y -= deformation;
        squashedScale.x += deformation * 0.5f;
        squashedScale.z += deformation * 0.5f;

        transform.localScale = squashedScale;
    }
}