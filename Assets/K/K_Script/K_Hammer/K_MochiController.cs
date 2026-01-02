using UnityEngine;
using UnityEngine.Events; // 이벤트 사용을 위해 필요

public class K_MochiController : MonoBehaviour
{
    [Header("떡 설정")]
    public int maxHits = 10;          // 떡이 완성되기 위해 필요한 타격 횟수
    public float squashAmount = 0.05f; // 한 번 맞을 때마다 납작해지는 정도
    public float minHeight = 0.3f;    // 떡이 너무 납작해지지 않게 최소 높이 제한

    [Header("완성 효과")]
    public Material finishedMaterial; // 떡이 완성되면 바뀔 재질 (예: 하얗고 윤기나는 재질)
    public GameObject steamEffect;    // 완성 시 나올 김(Steam) 효과
    public UnityEvent onMochiFinished; // 떡 완성 시 실행할 다른 이벤트들 (UI 띄우기 등)

    private int currentHits = 0;      // 현재 맞은 횟수
    private Vector3 initialScale;     // 초기 크기 저장

    void Start()
    {
        initialScale = transform.localScale;
    }

    // 망치 스크립트에서 이 함수를 호출할 겁니다.
    // impactForce: 망치가 때린 세기 (세게 때리면 더 많이 납작해짐)
    public void OnHit(float impactForce)
    {
        // 1. 이미 완성된 떡이면 반응 안 함
        if (currentHits >= maxHits) return;

        // 2. 타격 횟수 증가
        currentHits++;
        Debug.Log($"떡 타격! ({currentHits}/{maxHits}) - 강도: {impactForce}");

        // 3. 모양 변형 (Squash & Stretch)
        DeformMochi(impactForce);

        // 4. 완성 체크
        if (currentHits >= maxHits)
        {
            FinishMochi();
        }
    }

    void DeformMochi(float force)
    {
        // 때린 세기에 비례해서 변형량 계산 (기본 양 + 세기 보정)
        float deformation = squashAmount * (1.0f + force * 0.1f);

        // 현재 크기 가져오기
        Vector3 newScale = transform.localScale;

        // Y축은 줄어들고 (납작해짐), X/Z축은 늘어남 (퍼짐) -> 부피 보존 느낌
        newScale.y -= deformation;
        newScale.x += deformation * 0.5f;
        newScale.z += deformation * 0.5f;

        // 너무 납작해지지 않게 제한 (Clamp)
        if (newScale.y < initialScale.y * minHeight)
        {
            newScale.y = initialScale.y * minHeight;
        }

        transform.localScale = newScale;
    }

    void FinishMochi()
    {
        Debug.Log("떡 완성!");

        // 재질 변경 (설정되어 있다면)
        if (finishedMaterial != null)
        {
            GetComponent<Renderer>().material = finishedMaterial;
        }

        // 김이 모락모락 나는 이펙트 켜기
        if (steamEffect != null)
        {
            steamEffect.SetActive(true);
        }

        // 추가 이벤트 실행 (예: 게임 클리어 UI 표시)
        onMochiFinished.Invoke();
    }
}