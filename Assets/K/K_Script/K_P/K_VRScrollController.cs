using UnityEngine;
using UnityEngine.UI;

public class VRScrollController : MonoBehaviour
{
    [Header("연결할 컴포넌트")]
    public ScrollRect myScrollRect; // UI의 Scroll View를 연결

    [Header("속도 설정")]
    public float scrollSpeed = 0.5f; // 스크롤 속도

    void Update()
    {
        // OVRPlayerController를 쓰고 있다면 OVRInput을 사용합니다.
        // SecondaryThumbstick은 '오른쪽 컨트롤러' 조이스틱입니다.
        // (왼손잡이라면 PrimaryThumbstick으로 변경)
        Vector2 stickInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        // 조이스틱을 위아래로(Y축) 움직였을 때만 작동
        if (Mathf.Abs(stickInput.y) > 0.1f)
        {
            // 스크롤 위치 변경 (VerticalNormalizedPosition은 0~1 사이 값)
            // 1이 맨 위, 0이 맨 아래입니다.
            // 조이스틱을 위로 밀면(+), 내용은 아래로 내려가야(위쪽을 봄) 하므로 더해줍니다.
            myScrollRect.verticalNormalizedPosition += stickInput.y * scrollSpeed * Time.deltaTime;
        }
    }
}