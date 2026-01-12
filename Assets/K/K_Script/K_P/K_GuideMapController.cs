using UnityEngine;
using UnityEngine.EventSystems;

public class K_GuidePanelToggle : MonoBehaviour, IPointerClickHandler
{
    [Header("패널 오브젝트 연결")]
    public GameObject tutorialObj; // 튜토리얼 이미지가 있는 오브젝트
    public GameObject mapObj;      // 안내도 이미지가 있는 오브젝트

    private bool isShowingTutorial = true; // 현재 상태 추적

    void Start()
    {
        // 시작할 때 튜토리얼만 켜고, 안내도는 끕니다.
        ShowTutorial();
    }

    // VR 컨트롤러로 클릭했을 때 실행
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isShowingTutorial)
        {
            ShowMap(); // 튜토리얼 보고 있으면 -> 맵 보여주기
        }
        else
        {
            ShowTutorial(); // 맵 보고 있으면 -> 튜토리얼 보여주기
        }
    }

    public void ShowTutorial()
    {
        if (tutorialObj) tutorialObj.SetActive(true);
        if (mapObj) mapObj.SetActive(false);
        isShowingTutorial = true;
    }

    public void ShowMap()
    {
        if (tutorialObj) tutorialObj.SetActive(false);
        if (mapObj) mapObj.SetActive(true);
        isShowingTutorial = false;
    }
}