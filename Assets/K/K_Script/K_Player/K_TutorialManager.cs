using UnityEngine;

public class K_TutorialManager : MonoBehaviour
{
    [Header("Settings")]
    public OVRPlayerController playerController; // 플레이어 이동 제어 스크립트 연결

    void Start()
    {
        // 게임 시작 시, 플레이어가 움직이지 못하게 이동 기능을 끕니다.
        if (playerController != null)
        {
            playerController.EnableLinearMovement = false; // 이동 불가
            playerController.EnableRotation = false;       // 회전도 막고 싶다면 false (보통 회전은 둡니다)
        }
    }

    void Update()
    {
        // A 버튼이나 트리거를 누르면
        if (OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            GameStart();
        }

        // (키보드 테스트용) 스페이스바
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameStart();
        }
    }

    void GameStart()
    {
        // 1. 플레이어 이동 잠금 해제
        if (playerController != null)
        {
            playerController.EnableLinearMovement = true;
            // playerController.EnableRotation = true; 
        }

        // 2. 안내판(이 오브젝트) 비활성화 -> 사라짐
        gameObject.SetActive(false);
    }
}