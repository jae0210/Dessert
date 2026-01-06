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
            bool isAdmin = (gsheetClient != null && gsheetClient.IsAdmin);
            adminResetButton.gameObject.SetActive(isAdmin);

            adminResetButton.onClick.RemoveAllListeners();
            adminResetButton.onClick.AddListener(OnClickAdminReset);
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
