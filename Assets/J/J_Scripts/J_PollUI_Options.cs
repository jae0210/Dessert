using UnityEngine;
using UnityEngine.UI;

public partial class J_PollUI
{
    void BuildVoteOptions()
    {
        foreach (var go in spawnedOptionBtns) Object.Destroy(go);
        spawnedOptionBtns.Clear();

        if (manager == null || manager.currentPoll == null) return;

        if (questionText != null)
            questionText.text = manager.currentPoll.question;

        foreach (var opt in manager.currentPoll.options)
        {
            // 캡처 안전
            string id = opt.id;
            string labelStr = opt.label;

            Button btn = Object.Instantiate(optionButtonPrefab, optionsRoot);

            Text label = btn.GetComponentInChildren<Text>(true);
            if (label != null) label.text = labelStr;

            var fx = btn.GetComponent<J_UIOptionSelectFX>();
            if (fx != null) fx.SetSelected(false);

            btn.onClick.AddListener(() =>
            {
                if (isSending || isFetchingResults) return;

                PlaySfx(clickClip);

                // 이전 선택 해제
                if (selectedFx != null) selectedFx.SetSelected(false);

                // 새 선택 적용
                selectedId = id;
                selectedLabel = labelStr;
                selectedFx = fx;

                if (selectedFx != null) selectedFx.SetSelected(true);

                if (submitVoteButton != null)
                    submitVoteButton.interactable = true;

                SetStatus("");
            });

            spawnedOptionBtns.Add(btn.gameObject);
        }
    }

    void ClearSelection()
    {
        if (selectedFx != null) selectedFx.SetSelected(false);
        selectedFx = null;
        selectedId = null;
        selectedLabel = null;

        if (submitVoteButton != null) submitVoteButton.interactable = false;
    }
}
