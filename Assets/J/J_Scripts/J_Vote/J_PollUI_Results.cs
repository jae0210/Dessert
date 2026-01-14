using System.Collections.Generic;
using UnityEngine;

public partial class J_PollUI
{
    void ApplyResultsToCards(J_GSheetResults res)
    {
        if (res == null) return;

        if (resultTitleText != null) resultTitleText.text = "투표 결과";
        if (resultSummaryText != null) resultSummaryText.text = "총 투표수 : " + res.total;

        if (res.ranked == null || resultCardPrefab == null || resultsRoot == null)
            return;

        // maxVotes(막대 기준)
        int maxVotes = 0;
        for (int i = 0; i < res.ranked.Length; i++)
            if (res.ranked[i].votes > maxVotes) maxVotes = res.ranked[i].votes;

        // 동점 같은 순위(1,1,3)
        int displayRank = 0;
        int prevVotes = int.MinValue;

        HashSet<string> seen = new HashSet<string>();

        for (int i = 0; i < res.ranked.Length; i++)
        {
            var r = res.ranked[i];

            string key = string.IsNullOrEmpty(r.id) ? r.label : r.id;
            seen.Add(key);

            if (i == 0) displayRank = 1;
            else if (r.votes < prevVotes) displayRank = i + 1;
            prevVotes = r.votes;

            float percent = (res.total <= 0) ? 0f : (r.votes * 100f / res.total);
            percent = Mathf.Round(percent * 10f) / 10f;

            float bar01 = (maxVotes <= 0) ? 0f : (float)r.votes / maxVotes;

            if (!cardByKey.TryGetValue(key, out var card) || card == null)
            {
                card = Object.Instantiate(resultCardPrefab, resultsRoot);
                cardByKey[key] = card;
            }

            card.gameObject.SetActive(true);
            card.Set(displayRank, r.label, r.votes, percent, bar01);

            // 정렬 유지
            card.transform.SetSiblingIndex(i);
        }

        // 이번 결과에 없는 카드 숨김
        foreach (var kv in cardByKey)
        {
            if (kv.Value == null) continue;
            if (!seen.Contains(kv.Key))
                kv.Value.gameObject.SetActive(false);
        }
    }
}
