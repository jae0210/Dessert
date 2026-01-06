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
    public bool adminResetHiddenByDefault = true;
    public float adminRevealSeconds = 8f;
    public KeyCode adminRevealKey = KeyCode.R;

    bool adminRevealActive;
    float adminRevealUntil;

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

        // 관리자 초기화 버튼(관리자만 보이기)
        if (adminResetButton != null)
        {
            // 무조건 숨김(표시는 안전장치 스크립트가 담당)
            adminResetButton.gameObject.SetActive(false);

            // J_PollUI가 여기서 onClick을 세팅하지 않게
            adminResetButton.onClick.RemoveAllListeners();
        }
    }

    void Update()
    {
        if (adminResetButton == null) return;
        if (gsheetClient == null || !gsheetClient.IsAdmin) return;
        if (!adminResetHiddenByDefault) return;

        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (ctrl && shift && Input.GetKeyDown(adminRevealKey))
        {
            adminRevealActive = true;
            adminRevealUntil = Time.unscaledTime + Mathf.Max(1f, adminRevealSeconds);
            adminResetButton.gameObject.SetActive(true);
        }

        if (adminRevealActive && Time.unscaledTime > adminRevealUntil)
        {
            adminRevealActive = false;
            adminResetButton.gameObject.SetActive(false);
        }
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
