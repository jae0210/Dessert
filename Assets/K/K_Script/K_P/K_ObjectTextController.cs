using UnityEngine;
using UnityEngine.UI;

public class K_ObjectTextController : MonoBehaviour
{
    // 연결된 텍스트 컴포넌트
    public Text myLegacyText;

    // 인스펙터에서 입력할 내용
    [TextArea(3, 10)]
    public string initialMessage = "여기에 원하는 텍스트를 입력하세요.";

    void Start()
    {
        // 텍스트 컴포넌트 설정 강제 적용
        if (myLegacyText != null)
        {
            // [핵심 1] 가로로 글자가 길어지면 자동으로 다음 줄로 내림 (Wrap)
            myLegacyText.horizontalOverflow = HorizontalWrapMode.Wrap;

            // [핵심 2] 세로로 글자가 넘치면 박스 밖의 글자는 안 보이게 자름 (Truncate)
            myLegacyText.verticalOverflow = VerticalWrapMode.Truncate;

            // [핵심 3] 글자가 많으면 폰트 크기를 자동으로 줄여서 박스 안에 맞춤 (Best Fit)
            // 필요 없다면 이 부분은 주석 처리하거나 false로 바꾸세요.
            myLegacyText.resizeTextForBestFit = true;
            myLegacyText.resizeTextMinSize = 10; // 글자가 작아질 수 있는 최소 크기
            myLegacyText.resizeTextMaxSize = 100; // 글자가 커질 수 있는 최대 크기
        }

        // 게임 시작 시 텍스트 업데이트
        UpdateText(initialMessage);
    }

    // 텍스트 변경 함수
    public void UpdateText(string msg)
    {
        if (myLegacyText != null)
        {
            myLegacyText.text = msg;
        }
    }

    // 인스펙터에서 값을 바꿀 때마다 바로 화면 갱신 (테스트용)
    void OnValidate()
    {
        UpdateText(initialMessage);
    }
}