using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class J_PollUI : MonoBehaviour
{
    [Header("Refs")]
    public J_PollManager manager;
    public J_GSheetClient gsheetClient;

    [Header("Panels")]
    public GameObject votePanel;
    public GameObject resultPanel;

    [Header("Vote UI (Legacy)")]
    public Text questionText;
    public Transform optionsRoot;
    public Button optionButtonPrefab;
    public Text statusText;

    [Header("Submit")]
    public Button submitVoteButton; // "투표하기"

    [Header("Blocker/Loading (Optional)")]
    public GameObject inputBlocker;       // 전체 클릭 막는 패널(Stretch)
    public bool blockDuringNetwork = true;

    [Header("Auto Refresh Results")]
    public bool autoRefreshResults = true;
    public float autoRefreshInterval = 2.5f;
    public bool showStatusOnAutoRefresh = false;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip clickClip;
    public AudioClip successClip;
    public AudioClip failClip;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Result UI (Legacy)")]
    public Text resultTitleText;
    public Text resultSummaryText;
    public Transform resultsRoot;
    public J_VoteResultCardRow resultCardPrefab;

    [Header("Buttons (Result Panel)")]
    public Button revoteButton;        // 다시 투표하기
    public Button adminResetButton;    // 초기화(관리자만)

    [Header("Admin Reset Safety")]
    public bool adminResetHiddenByDefault = true;   // true면 기본 숨김(관리자만 조합으로 잠깐 노출)
    public float adminRevealSeconds = 8f;           // 노출 지속 시간(초)

    // (에디터/PC 테스트용) Ctrl + Shift + adminRevealKey
    public KeyCode adminRevealKey = KeyCode.R;

    [Header("Admin Reveal (OVR Controller)")]
    public bool useOvrControllerReveal = true;

    [Tooltip("양손 그립을 이 시간 이상 유지해야 관리자 버튼을 띄울 수 있습니다.")]
    public float adminRevealHoldSecondsOvr = 2.0f;

    [Tooltip("왼손 그립(기본: PrimaryHandTrigger)")]
    public OVRInput.Button adminHoldLeft = OVRInput.Button.PrimaryHandTrigger;

    [Tooltip("오른손 그립(기본: SecondaryHandTrigger)")]
    public OVRInput.Button adminHoldRight = OVRInput.Button.SecondaryHandTrigger;

    [Tooltip("그립 유지 후 누를 확인 버튼(기본: 오른손 스틱 클릭)")]
    public OVRInput.Button adminConfirmButton = OVRInput.Button.SecondaryThumbstick;

    bool adminRevealActive;
    float adminRevealUntil;
    float adminHoldTimer;

    // runtime
    readonly List<GameObject> spawnedOptionBtns = new List<GameObject>();
    readonly Dictionary<string, J_VoteResultCardRow> cardByKey = new Dictionary<string, J_VoteResultCardRow>();

    string selectedId;
    string selectedLabel;
    J_UIOptionSelectFX selectedFx;

    bool isSending;
    bool isFetchingResults;
    Coroutine autoRefreshCo;

    public void BuildAll()
    {
        // ✅ 먼저 투표 패널 켜고 옵션 만들기(비활성 부모 문제 예방)
        ShowVotePanel();
        BuildVoteOptions();

        SetStatus("");

        // 선택 초기화
        selectedId = null;
        selectedLabel = null;
        selectedFx = null;

        // 투표하기 버튼
        if (submitVoteButton != null)
        {
            submitVoteButton.interactable = false;
            submitVoteButton.onClick.RemoveAllListeners();
            submitVoteButton.onClick.AddListener(OnClickSubmitVote);
        }

        // 다시 투표하기
        if (revoteButton != null)
        {
            revoteButton.onClick.RemoveAllListeners();
            revoteButton.onClick.AddListener(OnClickRevote);
        }

        // 관리자 초기화 버튼(관리자만)
        if (adminResetButton != null)
        {
            // ✅ 눌렀을 때 실제 초기화 동작 연결 (안 하면 “보여도 눌러도 안 됨”)
            adminResetButton.onClick.RemoveAllListeners();
            adminResetButton.onClick.AddListener(OnClickAdminReset);

            bool isAdmin = (gsheetClient != null && gsheetClient.IsAdmin);

            if (!isAdmin)
            {
                // 관리자가 아니면 항상 숨김
                adminResetButton.gameObject.SetActive(false);
                adminRevealActive = false;
                adminHoldTimer = 0f;
            }
            else
            {
                // 관리자면: 숨김 옵션에 따라 기본 표시 여부 결정
                adminResetButton.gameObject.SetActive(!adminResetHiddenByDefault);
                adminRevealActive = false;
                adminHoldTimer = 0f;
            }
        }
    }

    void Update()
    {
        if (adminResetButton == null) return;
        if (gsheetClient == null || !gsheetClient.IsAdmin) return;

        // 숨김 기능을 안 쓰면(=항상 노출) Update에서 할 일 없음
        if (!adminResetHiddenByDefault) return;

        // -----------------------
        // 1) 에디터/PC 테스트용: Ctrl + Shift + adminRevealKey
        // -----------------------
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (ctrl && shift && Input.GetKeyDown(adminRevealKey))
        {
            RevealAdminResetButton();
        }

        // -----------------------
        // 2) Quest/컨트롤러: 양손 그립을 일정 시간 유지 + 확인 버튼 클릭
        // -----------------------
        if (useOvrControllerReveal)
        {
            bool holding =
                OVRInput.Get(adminHoldLeft) &&
                OVRInput.Get(adminHoldRight);

            if (holding) adminHoldTimer += Time.unscaledDeltaTime;
            else adminHoldTimer = 0f;

            // 그립을 충분히 오래 누르고 있는 상태에서 "확인 버튼"까지 눌러야 노출
            if (adminHoldTimer >= adminRevealHoldSecondsOvr &&
                OVRInput.GetDown(adminConfirmButton))
            {
                adminHoldTimer = 0f;
                RevealAdminResetButton();
            }
        }

        // -----------------------
        // 3) 노출 시간이 끝나면 다시 숨김
        // -----------------------
        if (adminRevealActive && Time.unscaledTime > adminRevealUntil)
        {
            adminRevealActive = false;
            adminResetButton.gameObject.SetActive(false);
        }
    }

    void RevealAdminResetButton()
    {
        adminRevealActive = true;
        adminRevealUntil = Time.unscaledTime + Mathf.Max(1f, adminRevealSeconds);
        adminResetButton.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        StopAutoRefresh();
        SetBlocked(false);
    }

    void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    void SetBlocked(bool on)
    {
        if (!blockDuringNetwork) return;
        if (inputBlocker != null) inputBlocker.SetActive(on);
    }

    public void ShowVotePanel()
    {
        if (votePanel != null) votePanel.SetActive(true);
        if (resultPanel != null) resultPanel.SetActive(false);
        StopAutoRefresh();
    }

    public void ShowResultPanel()
    {
        if (votePanel != null) votePanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(true);
        StartAutoRefresh();
    }
}
