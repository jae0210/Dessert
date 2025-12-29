using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class J_PollUI : MonoBehaviour
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

    [Header("Result UI (Legacy)")]
    public Text resultTitleText;
    public Text resultSummaryText;
    public Transform resultsRoot;
    public Text resultRowPrefab;

    [Header("Admin UI")]
    public GameObject adminResetButton; // 결과창에 있는 "초기화" 버튼 오브젝트

    private readonly List<GameObject> spawnedOptionBtns = new List<GameObject>();
    private readonly List<GameObject> spawnedResultRows = new List<GameObject>();

    void Start()
    {
        // 관리자 빌드(=adminToken 존재)에서만 버튼 보이게
        if (adminResetButton != null)
        {
            bool isAdmin = (gsheetClient != null && gsheetClient.IsAdmin);
            adminResetButton.SetActive(isAdmin);
        }
    }

    public void BuildAll()
    {
        BuildVoteOptions();
        ShowVotePanel();
        if (statusText != null) statusText.text = "";
    }

    private void BuildVoteOptions()
    {
        foreach (var go in spawnedOptionBtns) Destroy(go);
        spawnedOptionBtns.Clear();

        if (questionText != null)
            questionText.text = manager.currentPoll.question;

        foreach (var opt in manager.currentPoll.options)
        {
            Button btn = Instantiate(optionButtonPrefab, optionsRoot);

            Text label = btn.GetComponentInChildren<Text>(true);
            if (label != null) label.text = opt.label;

            btn.onClick.AddListener(() =>
            {
                if (gsheetClient == null)
                {
                    if (statusText != null) statusText.text = "GSheetClient 연결 안됨";
                    return;
                }

                btn.interactable = false;
                if (statusText != null) statusText.text = "전송 중...";

                gsheetClient.SubmitVote(opt.id, opt.label, (res) =>
                {
                    btn.interactable = true;

                    if (res == null || !res.ok)
                    {
                        if (statusText != null)
                            statusText.text = "투표 실패: " + (res != null ? res.error : "null");
                        return;
                    }

                    if (statusText != null) statusText.text = "투표 완료!";
                    ShowResultPanel();
                    BuildResultsFromServer(res);
                });
            });

            spawnedOptionBtns.Add(btn.gameObject);
        }
    }

    private void BuildResultsFromServer(J_GSheetResults res)
    {
        foreach (var go in spawnedResultRows) Destroy(go);
        spawnedResultRows.Clear();

        if (resultTitleText != null) resultTitleText.text = "투표 결과 (순위)";
        if (resultSummaryText != null) resultSummaryText.text = "총 투표수 : " + res.total;

        if (res.ranked == null) return;

        foreach (var r in res.ranked)
        {
            Text row = Instantiate(resultRowPrefab, resultsRoot);
            row.text = $"{r.rank}위  {r.label}  -  {r.votes}표  ({r.percent:0}%)";
            spawnedResultRows.Add(row.gameObject);
        }
    }

    public void ShowVotePanel()
    {
        votePanel.SetActive(true);
        resultPanel.SetActive(false);
    }

    public void ShowResultPanel()
    {
        votePanel.SetActive(false);
        resultPanel.SetActive(true);
    }

    public void OnClickRevote()
    {
        ShowVotePanel();
        if (statusText != null) statusText.text = "다시 선택해 주세요.";
    }

    public void OnClickRefreshResults()
    {
        if (gsheetClient == null) return;

        if (statusText != null) statusText.text = "결과 불러오는 중...";
        gsheetClient.FetchResults((res) =>
        {
            if (res == null || !res.ok)
            {
                if (statusText != null)
                    statusText.text = "결과 로드 실패: " + (res != null ? res.error : "null");
                return;
            }

            ShowResultPanel();
            BuildResultsFromServer(res);
            if (statusText != null) statusText.text = "";
        });
    }

    // 결과창 "초기화" 버튼 OnClick에 연결할 함수
    public void OnClickAdminReset()
    {
        if (gsheetClient == null)
        {
            if (statusText != null) statusText.text = "GSheetClient 연결 안됨";
            return;
        }

        if (!gsheetClient.IsAdmin)
        {
            if (statusText != null) statusText.text = "관리자만 초기화 가능";
            return;
        }

        if (statusText != null) statusText.text = "집계 초기화 중...";
        gsheetClient.ResetAll((res) =>
        {
            if (res == null || !res.ok)
            {
                if (statusText != null)
                    statusText.text = "초기화 실패: " + (res != null ? res.error : "null");
                return;
            }

            ShowResultPanel();
            BuildResultsFromServer(res);
            if (statusText != null) statusText.text = "초기화 완료!";
        });
    }
}
