using System.Collections;
using UnityEngine;

public partial class J_PollUI
{
    void OnClickSubmitVote()
    {
        if (isSending || isFetchingResults) return;

        if (gsheetClient == null)
        {
            SetStatus("GSheetClient 연결 안됨");
            PlaySfx(failClip);
            return;
        }

        if (string.IsNullOrEmpty(selectedId))
        {
            SetStatus("먼저 항목을 선택해 주세요.");
            PlaySfx(failClip);
            return;
        }

        isSending = true;
        if (submitVoteButton != null) submitVoteButton.interactable = false;

        SetBlocked(true);
        SetStatus("전송 중...");

        gsheetClient.SubmitVote(selectedId, selectedLabel, (res) =>
        {
            isSending = false;
            SetBlocked(false);

            if (res == null || !res.ok)
            {
                SetStatus("투표 실패: " + (res != null ? res.error : "null response"));
                PlaySfx(failClip);
                if (submitVoteButton != null) submitVoteButton.interactable = true;
                return;
            }

            SetStatus("");
            PlaySfx(successClip);

            ShowResultPanel();
            ApplyResultsToCards(res);
        });
    }

    void StartAutoRefresh()
    {
        if (!autoRefreshResults) return;
        if (gsheetClient == null) return;
        if (resultPanel == null || !resultPanel.activeInHierarchy) return;

        if (autoRefreshCo != null) StopCoroutine(autoRefreshCo);
        autoRefreshCo = StartCoroutine(CoAutoRefresh());
    }

    void StopAutoRefresh()
    {
        if (autoRefreshCo != null)
        {
            StopCoroutine(autoRefreshCo);
            autoRefreshCo = null;
        }
    }

    IEnumerator CoAutoRefresh()
    {
        var wait = new WaitForSecondsRealtime(Mathf.Max(0.5f, autoRefreshInterval));

        while (resultPanel != null && resultPanel.activeInHierarchy)
        {
            yield return wait;

            if (isSending || isFetchingResults) continue;

            FetchAndApplyResults(showStatusOnAutoRefresh);
        }

        autoRefreshCo = null;
    }

    void FetchAndApplyResults(bool showStatus)
    {
        if (gsheetClient == null) return;

        isFetchingResults = true;
        if (showStatus) SetStatus("결과 불러오는 중...");

        gsheetClient.FetchResults((res) =>
        {
            isFetchingResults = false;

            if (res == null || !res.ok)
            {
                if (showStatus) SetStatus("결과 로드 실패: " + (res != null ? res.error : "null response"));
                return;
            }

            if (showStatus) SetStatus("");

            // ✅ 여기서 ShowResultPanel() 다시 호출하지 않음(자동새로고침 꼬임 방지)
            ApplyResultsToCards(res);
        });
    }

    public void OnClickRevote()
    {
        ClearSelection();
        ShowVotePanel();
        SetStatus("");
        PlaySfx(clickClip);
    }

    public void OnClickAdminReset()
    {
        if (gsheetClient == null) return;

        if (!gsheetClient.IsAdmin)
        {
            SetStatus("관리자만 초기화 가능");
            PlaySfx(failClip);
            return;
        }

        SetBlocked(true);
        SetStatus("초기화 중...");

        gsheetClient.ResetAll((res) =>
        {
            SetBlocked(false);

            if (res == null || !res.ok)
            {
                SetStatus("초기화 실패: " + (res != null ? res.error : "null response"));
                PlaySfx(failClip);
                return;
            }

            SetStatus("");
            PlaySfx(successClip);

            ShowResultPanel();
            ApplyResultsToCards(res);
        });
    }

    public void RequestResultsNow(bool showStatus = false)
    {
        if (gsheetClient == null) return;
        if (isSending || isFetchingResults) return;

        // 결과 패널이 꺼져있다면 켜고(=자동 새로고침도 시작됨)
        if (resultPanel != null && !resultPanel.activeInHierarchy)
            ShowResultPanel();

        FetchAndApplyResults(showStatus);
    }
}
