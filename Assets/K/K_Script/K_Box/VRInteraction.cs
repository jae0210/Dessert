using UnityEngine;

public class VRInteraction : MonoBehaviour
{
    public GameObject interactionPanel;
    private bool isPlayerNearby = false;

    void Start()
    {
        interactionPanel.SetActive(false);
    }

    // 1. 거리 감지 (플레이어가 근처에 있을 때만 클릭 가능하게 제한)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            interactionPanel.SetActive(false); // 멀어지면 자동으로 꺼짐
        }
    }

    // 2. VR 컨트롤러로 클릭했을 때 호출될 함수
    public void OnVRClick()
    {
        if (isPlayerNearby)
        {
            // 패널이 꺼져있으면 켜고, 켜져있으면 끕니다 (Toggle)
            interactionPanel.SetActive(!interactionPanel.activeSelf);
        }
    }
}