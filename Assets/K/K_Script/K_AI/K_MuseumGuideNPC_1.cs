using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class K_MuseumGuideNPC_1 : MonoBehaviour
{
    [Header("Basic Settings")]
    public Transform playerCamera;
    public float detectionRange = 3.0f;
    public float rotationSpeed = 5.0f;

    [Header("UI Settings (World Space Canvas)")]
    public GameObject explanationPanel;
    public Text titleText;
    public Text bodyText;

    [Header("Exhibit Data")]
    public List<ExhibitPoint> exhibitRoute;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private int currentTargetIndex = 0;
    private bool isWaitingForPlayer = false;
    private bool isPanelActive = false;

    [System.Serializable]
    public class ExhibitPoint
    {
        public string exhibitName;
        [TextArea(3, 10)]
        public string explanationText;
        public Transform destination;
        public AudioClip voiceGuide;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;

        if (explanationPanel != null)
            explanationPanel.SetActive(false);

        MoveToNextExhibit();
    }

    void Update()
    {
        FaceTarget(playerCamera.position);

        if (isPanelActive && explanationPanel != null)
        {
            FaceUIOverlay(explanationPanel.transform, playerCamera.position);
        }

        // [수정] 도착 판정 로직 개선 (디버그 로그 추가)
        if (!isWaitingForPlayer && !isPanelActive)
        {
            // 남은 거리가 0.5 이하이거나, pathPending이 아닌데 멈춤 거리에 도달했을 때
            if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(0.5f, agent.stoppingDistance))
            {
                Debug.Log("✅ NPC 도착 완료! 플레이어를 기다립니다.");
                isWaitingForPlayer = true;
            }
        }

        if (isWaitingForPlayer)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerCamera.position);

            // 거리 확인용 로그 (테스트 후 지우셔도 됩니다)
            // Debug.Log($"플레이어와의 거리: {distanceToPlayer}");

            if (distanceToPlayer <= detectionRange)
            {
                Debug.Log("👀 플레이어 감지됨! 패널을 엽니다.");
                isWaitingForPlayer = false;
                ShowExplanationPanel();
            }
        }
    }

    void ShowExplanationPanel()
    {
        if (currentTargetIndex >= exhibitRoute.Count) return;

        isPanelActive = true;
        agent.isStopped = true;

        ExhibitPoint currentExhibit = exhibitRoute[currentTargetIndex];

        if (titleText != null) titleText.text = currentExhibit.exhibitName;
        if (bodyText != null) bodyText.text = currentExhibit.explanationText;

        if (explanationPanel != null) explanationPanel.SetActive(true);

        if (currentExhibit.voiceGuide != null)
        {
            audioSource.clip = currentExhibit.voiceGuide;
            audioSource.Play();
        }
    }

    public void ClosePanelAndMoveOn()
    {
        Debug.Log("👉 다음 장소로 이동합니다.");
        if (explanationPanel != null) explanationPanel.SetActive(false);
        isPanelActive = false;

        audioSource.Stop();

        currentTargetIndex++;
        MoveToNextExhibit();
    }

    void MoveToNextExhibit()
    {
        if (exhibitRoute.Count == 0 || currentTargetIndex >= exhibitRoute.Count)
        {
            Debug.Log("🏁 모든 관람이 끝났습니다.");
            return;
        }

        Debug.Log($"🚶 {currentTargetIndex + 1}번째 목적지로 이동 시작");
        agent.isStopped = false;
        agent.SetDestination(exhibitRoute[currentTargetIndex].destination.position);
    }

    void FaceTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void FaceUIOverlay(Transform uiTransform, Vector3 targetPos)
    {
        uiTransform.LookAt(targetPos);
        uiTransform.Rotate(0, 180, 0);
    }
}