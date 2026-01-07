using UnityEngine;

public class ImagePopupController : MonoBehaviour
{
    [Header("확대되어 나올 이미지 오브젝트")]
    public GameObject expandedImage;

    void Start()
    {
        // 게임 시작 시 확대 이미지가 켜져있다면 강제로 끕니다.
        if (expandedImage != null)
        {
            expandedImage.SetActive(false);
        }
    }

    // 버튼을 눌렀을 때 실행될 함수
    public void ToggleImage()
    {
        if (expandedImage != null)
        {
            // 현재 꺼져있으면 켜고, 켜져있으면 끕니다 (토글 방식)
            bool isActive = expandedImage.activeSelf;
            expandedImage.SetActive(!isActive);

            // 만약 토글이 아니라 '켜기만' 하고 싶다면 아래 줄을 쓰세요.
            // expandedImage.SetActive(true); 
        }
    }
}