using System.Collections;
using System.Linq;
using UnityEngine;

public class J_VoteBoardBootstrap : MonoBehaviour
{
    public J_PollManager manager;
    public J_PollUI ui;

    public string question = "최애 디저트에 투표해 주세요!";

    [Header("Board Mode (Result Only)")]
    public bool startInResultBoardMode = false;
    public float firstFetchDelay = 0.2f;

    void Start()
    {
        var exhibits = FindObjectsOfType<J_ExhibitVoteOption>(true)
            .Where(e => !string.IsNullOrEmpty(e.optionId))
            .ToList();

        var dup = exhibits.GroupBy(e => e.optionId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (dup.Count > 0)
            Debug.LogError("중복 optionId 발견: " + string.Join(", ", dup));

        exhibits = exhibits.OrderBy(e => e.optionId).ToList();

        var poll = new J_Poll { question = question };

        foreach (var e in exhibits)
        {
            poll.options.Add(new J_PollOption
            {
                id = e.optionId,
                label = string.IsNullOrEmpty(e.displayName) ? e.optionId : e.displayName
            });
        }

        manager.StartPoll(poll);
        ui.BuildAll();

        if (startInResultBoardMode)
            StartCoroutine(CoStartBoardMode());
    }

    IEnumerator CoStartBoardMode()
    {
        ui.ShowResultPanel();
        yield return new WaitForSecondsRealtime(firstFetchDelay);
        ui.RequestResultsNow(false);
    }
}
