using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class K_GuideMapController2 : MonoBehaviour, IPointerClickHandler
{
    [Header("---- [설정 1] 역할 설정 ----")]
    public bool isSkipButtonOnly = false;

    [Header("---- [설정 2] 공통 설정 ----")]
    public string nextSceneName = "MainGame";

    [Header("---- [설정 3] 안내판일 경우에만 사용 ----")]
    public GameObject[] guidePages;

    private int currentIndex = 0;

    // 중복 클릭 방지
    private float lastActionTime = 0f;
    private const float COOLDOWN = 0.5f;

    void Start()
    {
        if (!isSkipButtonOnly)
        {
            UpdatePageVisibility();
        }
    }

    // 레이저 클릭
    public void OnPointerClick(PointerEventData eventData)
    {
        // 쿨다운 체크
        if (Time.time - lastActionTime < COOLDOWN) return;

        if (isSkipButtonOnly)
        {
            GoToNextScene(); // 스킵 버튼은 이 함수(쿨다운 포함)를 씀
            return;
        }

        // 안내판은 여기서 처리
        ProcessNextPage();
    }

    void Update()
    {
        if (!isSkipButtonOnly)
        {
            if (OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                ProcessNextPage();
            }
        }
    }

    void ProcessNextPage()
    {
        // 1. 여기서 쿨다운 체크를 하고 시간을 갱신함
        if (Time.time - lastActionTime < COOLDOWN) return;
        lastActionTime = Time.time;

        currentIndex++;

        // 2. 마지막 페이지라면?
        if (guidePages != null && currentIndex >= guidePages.Length)
        {
            Debug.Log(">> 마지막 페이지 도달! 씬 이동 실행");

            // ★ [수정된 부분] GoToNextScene()을 부르지 않고 바로 이동시킴
            // 이유: GoToNextScene을 부르면 또 쿨다운 체크를 해서 막혀버림
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            UpdatePageVisibility();
        }
    }

    // 스킵 버튼 전용 씬 이동 (쿨다운 체크 포함)
    public void GoToNextScene()
    {
        if (Time.time - lastActionTime < COOLDOWN) return;
        lastActionTime = Time.time;

        Debug.Log($"[스킵 버튼] 씬 이동: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    private void UpdatePageVisibility()
    {
        if (guidePages == null || guidePages.Length == 0) return;

        for (int i = 0; i < guidePages.Length; i++)
        {
            if (guidePages[i] != null)
                guidePages[i].SetActive(i == currentIndex);
        }
    }
}