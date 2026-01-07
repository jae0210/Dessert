using UnityEngine;
using UnityEngine.UI;

public class K_SimpleVignette : MonoBehaviour
{
    [Header("설정")]
    public OVRPlayerController playerController; // 플레이어 컨트롤러 연결
    public CanvasGroup vignetteCanvasGroup; // 비네트 캔버스 그룹 연결
    public float fadeSpeed = 5f; // 효과가 나타나는 속도

    void Update()
    {
        // 1. 플레이어가 움직이고 있는지 확인 (키보드, 조이스틱 등)
        // OVRPlayerController는 보통 CharacterController를 통해 움직입니다.
        // 혹은 OVRInput을 직접 체크합니다.

        bool isMoving = false;

        // 왼쪽/오른쪽 썸스틱 입력 감지 (이동 혹은 회전 시)
        Vector2 moveInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector2 rotateInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        // 입력값이 일정 이상이면 움직이는 것으로 간주
        if (moveInput.magnitude > 0.1f || rotateInput.magnitude > 0.1f)
        {
            isMoving = true;
        }

        // 2. 움직임 여부에 따라 투명도(Alpha) 조절
        float targetAlpha = isMoving ? 1.0f : 0.0f;

        // 부드럽게 Alpha값 변경
        vignetteCanvasGroup.alpha = Mathf.Lerp(vignetteCanvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }
}