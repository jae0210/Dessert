using UnityEngine;

[RequireComponent(typeof(OVRGrabbable))]
[RequireComponent(typeof(K_FoodPhysicsController))]
public class K_FoodStretchBridge : MonoBehaviour
{
    [Header("참조 (자동 할당 시도함)")]
    public OVRGrabber leftGrabber;
    public OVRGrabber rightGrabber;

    [Header("늘리기 설정")]
    public float stretchMultiplier = 2.0f; // 실제 거리보다 얼마나 더 민감하게 반응할지
    public float minStretchThreshold = 0.05f; // 이 이상 벌려야 늘어나기 시작

    // 내부 변수
    private OVRGrabbable grabbable;
    private K_FoodPhysicsController foodPhysics;
    private float initialHandDistance;
    private bool isStretching = false;

    void Awake()
    {
        grabbable = GetComponent<OVRGrabbable>();
        foodPhysics = GetComponent<K_FoodPhysicsController>();

        // 씬 내의 그랩버 자동 찾기 (필요시 인스펙터에서 수동 할당 권장)
        if (leftGrabber == null || rightGrabber == null)
        {
            OVRGrabber[] grabbers = FindObjectsOfType<OVRGrabber>();
            foreach (var g in grabbers)
            {
                // 이름이나 태그 등으로 좌/우 구분 필요. 여기선 단순히 할당.
                // 실제 프로젝트 설정에 맞춰 태그나 이름으로 구분해서 넣으세요.
                if (g.name.Contains("Left") || g.name.Contains("LTouch")) leftGrabber = g;
                else if (g.name.Contains("Right") || g.name.Contains("RTouch")) rightGrabber = g;
            }
        }
    }

    void Update()
    {
        // 1. 떡(Soft) 타입이 아니면 계산할 필요 없음
        if (foodPhysics.foodType != FoodType.Soft) return;

        // 2. 잡혀있지 않다면 리셋
        if (!grabbable.isGrabbed || grabbable.grabbedBy == null)
        {
            isStretching = false;
            return;
        }

        // 3. 메인 잡은 손과 반대 손 식별
        OVRGrabber primary = grabbable.grabbedBy;
        OVRGrabber secondary = (primary == leftGrabber) ? rightGrabber : leftGrabber;

        if (secondary == null) return;

        // 4. 반대 손이 트리거를 당기고 있는지 확인 (OVRInput 사용)
        bool isSecondaryGripping = IsGripping(secondary);

        if (isSecondaryGripping)
        {
            float currentDist = Vector3.Distance(primary.transform.position, secondary.transform.position);

            if (!isStretching)
            {
                // 막 늘리기 시작했을 때 초기 거리 저장
                initialHandDistance = currentDist;
                isStretching = true;
            }
            else
            {
                // 늘어난 거리 계산
                float delta = currentDist - initialHandDistance;

                // 음수(손이 가까워짐)는 무시하거나 0으로 처리
                if (delta < minStretchThreshold) delta = 0;

                // FoodPhysicsController에게 전달
                foodPhysics.OnGrabAndStretch(delta * stretchMultiplier);
            }
        }
        else
        {
            isStretching = false;
        }
    }

    // 간단한 트리거 입력 확인 (K_TwoHandSupportGrip_OVR의 로직과 유사하게)
    bool IsGripping(OVRGrabber g)
    {
        bool isRight = (g == rightGrabber);
        var controller = isRight ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;

        // HandTrigger(중지)와 IndexTrigger(검지) 둘 중 하나라도 누르면 잡은 것으로 간주
        return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller) > 0.5f ||
               OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller) > 0.5f;
    }
}