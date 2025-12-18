using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    // 유니티 에디터에서 보여줄 UI 패널 연결창
    public GameObject interactionPanel;

    private void Start()
    {
        // 시작할 때 패널이 꺼져 있는지 확인
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
        }
    }

    // 플레이어가 콜라이더 영역 안으로 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactionPanel.SetActive(true);
        }
    }

    // 플레이어가 콜라이더 영역 밖으로 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactionPanel.SetActive(false);
        }
    }
}