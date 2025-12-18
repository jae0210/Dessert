using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public GameObject interactionPanel;

    [Header("카메라 앞에 띄우기 설정")]
    public Transform playerCamera;
    public float panelDistance = 1.5f;
    public Vector3 panelOffset = new Vector3(0f, -0.1f, 0f);
    public bool followWhileVisible = true;
    public bool faceCamera = true;

    [Header("트리거 거리 설정")]
    public float triggerDistance = 2.0f;          // 원하는 감지 거리(미터)
    public SphereCollider triggerCollider;        // 없으면 GetComponent로 자동 찾음

    private void Start()
    {
        // 카메라 자동 찾기
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        // 트리거 콜라이더 세팅(거리 = SphereCollider.radius)
        if (triggerCollider == null)
            triggerCollider = GetComponent<SphereCollider>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
            triggerCollider.radius = triggerDistance;
        }
        else
        {
            Debug.LogWarning("SphereCollider가 없습니다. 트리거 거리를 쓰려면 SphereCollider를 추가하세요.");
        }

        // 시작할 때 패널 OFF
        if (interactionPanel != null)
            interactionPanel.SetActive(false);
    }

    private void LateUpdate()
    {
        if (followWhileVisible && interactionPanel != null && interactionPanel.activeSelf)
            PlacePanelInFrontOfCamera();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactionPanel == null) return;

            interactionPanel.SetActive(true);
            PlacePanelInFrontOfCamera();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactionPanel == null) return;

            interactionPanel.SetActive(false);
        }
    }

    private void PlacePanelInFrontOfCamera()
    {
        if (playerCamera == null || interactionPanel == null) return;

        Vector3 pos = playerCamera.position + playerCamera.forward * panelDistance;
        pos += playerCamera.TransformDirection(panelOffset);

        interactionPanel.transform.position = pos;

        if (faceCamera)
        {
            Vector3 dir = playerCamera.position - pos;
            interactionPanel.transform.rotation = Quaternion.LookRotation(dir, playerCamera.up);
        }
    }
}
