using UnityEngine;
using UnityEngine.EventSystems;

public class K_GuideMapController : MonoBehaviour, IPointerClickHandler
{
    [Header("안내 패널 리스트 (순서대로 넣으세요)")]
    // 배열로 선언하여 인스펙터에서 사이즈를 자유롭게 조절 가능합니다.
    public GameObject[] guidePages;

    private int currentIndex = 0; // 현재 보고 있는 페이지 번호

    void Start()
    {
        // 시작 시 첫 번째 페이지만 보여주고 나머지는 끕니다.
        UpdatePageVisibility();
    }

    // VR 컨트롤러로 클릭했을 때 실행
    public void OnPointerClick(PointerEventData eventData)
    {
        // 다음 페이지로 인덱스 증가
        currentIndex++;

        // 만약 마지막 페이지를 넘어가면 다시 처음(0번)으로 돌아감
        if (currentIndex >= guidePages.Length)
        {
            currentIndex = 0;
        }

        // 화면 갱신
        UpdatePageVisibility();
    }

    // 현재 인덱스에 맞는 페이지만 켜고 나머지는 끄는 함수
    private void UpdatePageVisibility()
    {
        if (guidePages == null || guidePages.Length == 0) return;

        for (int i = 0; i < guidePages.Length; i++)
        {
            if (guidePages[i] != null)
            {
                // 현재 순서인 오브젝트는 켜고(true), 나머지는 끔(false)
                guidePages[i].SetActive(i == currentIndex);
            }
        }
    }
}