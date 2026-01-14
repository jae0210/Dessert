using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class J_PollOption
{
    public string id;
    public string label;
}

[Serializable]
public class J_Poll
{
    public string question;
    public List<J_PollOption> options = new List<J_PollOption>();
}

public struct J_PollResultEntry
{
    public int rank;
    public string id;
    public string label;
    public int votes;
    public float percent;
}

public class J_PollManager : MonoBehaviour
{
    public J_Poll currentPoll;

    [Header("Optional: persist stats (누적 통계)")]
    public bool persistToPlayerPrefs = false;
    public string prefsKeyPrefix = "J_Vote_";

    private int[] votes;
    private Dictionary<string, int> idToIndex = new Dictionary<string, int>();

    // 유저가 무엇에 투표했는지 저장 (재투표용)
    private Dictionary<string, int> userChoiceIndex = new Dictionary<string, int>();

    public event Action OnChanged;

    public void StartPoll(J_Poll poll)
    {
        currentPoll = poll;

        if (poll == null || poll.options == null)
        {
            votes = null;
            idToIndex.Clear();
            userChoiceIndex.Clear();
            OnChanged?.Invoke();
            return;
        }

        votes = new int[poll.options.Count];

        idToIndex = poll.options
            .Select((opt, idx) => new { opt.id, idx })
            .Where(x => !string.IsNullOrEmpty(x.id))
            .ToDictionary(x => x.id, x => x.idx);

        userChoiceIndex.Clear();

        if (persistToPlayerPrefs)
        {
            for (int i = 0; i < poll.options.Count; i++)
            {
                string key = prefsKeyPrefix + poll.options[i].id;
                votes[i] = PlayerPrefs.GetInt(key, 0);
            }
        }

        OnChanged?.Invoke();
    }

    // 같은 유저가 다시 투표하면 "표 이동"
    public bool VoteOrChange(string userKey, string optionId)
    {
        if (currentPoll == null) return false;
        if (string.IsNullOrEmpty(userKey)) userKey = "local";
        if (string.IsNullOrEmpty(optionId)) return false;

        if (!idToIndex.TryGetValue(optionId, out int newIdx)) return false;

        // 이미 투표한 적 있으면 이전 표를 빼고 새 표를 더함
        if (userChoiceIndex.TryGetValue(userKey, out int prevIdx))
        {
            if (prevIdx == newIdx)
            {
                // 같은 거 다시 누르면 변화 없음(성공 처리)
                return true;
            }

            votes[prevIdx] = Mathf.Max(0, votes[prevIdx] - 1);
            votes[newIdx]++;

            userChoiceIndex[userKey] = newIdx;

            SaveIfNeeded(prevIdx);
            SaveIfNeeded(newIdx);

            OnChanged?.Invoke();
            return true;
        }

        // 처음 투표
        userChoiceIndex[userKey] = newIdx;
        votes[newIdx]++;

        SaveIfNeeded(newIdx);

        OnChanged?.Invoke();
        return true;
    }

    private void SaveIfNeeded(int idx)
    {
        if (!persistToPlayerPrefs) return;
        if (currentPoll == null || currentPoll.options == null) return;
        if (idx < 0 || idx >= currentPoll.options.Count) return;

        string key = prefsKeyPrefix + currentPoll.options[idx].id;
        PlayerPrefs.SetInt(key, votes[idx]);
        PlayerPrefs.Save();
    }

    public int GetTotalVotes()
    {
        if (votes == null) return 0;
        int total = 0;
        for (int i = 0; i < votes.Length; i++) total += votes[i];
        return total;
    }

    // 동점 같은 순위(1,1,3) + percent 소수 1자리 반올림
    public List<J_PollResultEntry> GetRankedResults()
    {
        if (currentPoll == null || currentPoll.options == null)
            return new List<J_PollResultEntry>();

        int total = GetTotalVotes();
        var list = new List<J_PollResultEntry>();

        for (int i = 0; i < currentPoll.options.Count; i++)
        {
            var opt = currentPoll.options[i];
            float pct = (total == 0) ? 0f : (votes[i] * 100f / total);

            // 소수 1자리 반올림(33.34 -> 33.3)
            pct = Mathf.Round(pct * 10f) / 10f;

            list.Add(new J_PollResultEntry
            {
                rank = 0,
                id = opt.id,
                label = opt.label,
                votes = votes[i],
                percent = pct
            });
        }

        // 득표 desc, 라벨 asc
        list = list.OrderByDescending(x => x.votes).ThenBy(x => x.label).ToList();

        int rank = 0;
        int prevVotes = int.MinValue;

        for (int i = 0; i < list.Count; i++)
        {
            var e = list[i];

            if (i == 0) rank = 1;
            else if (e.votes < prevVotes) rank = i + 1;   // 동점이면 rank 유지

            e.rank = rank;
            prevVotes = e.votes;

            list[i] = e;
        }

        return list;
    }

    public void ResetSavedVotes()
    {
        if (currentPoll == null || currentPoll.options == null) return;

        foreach (var opt in currentPoll.options)
            PlayerPrefs.DeleteKey(prefsKeyPrefix + opt.id);

        PlayerPrefs.Save();
        StartPoll(currentPoll);
    }
}
