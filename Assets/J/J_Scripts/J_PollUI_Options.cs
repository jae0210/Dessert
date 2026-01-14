using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class J_PollUI
{
    // ✅ optionId -> 아이콘 스프라이트 맵
    Dictionary<string, Sprite> iconById = new Dictionary<string, Sprite>();

    // ✅ Bootstrap에서 미리 주입
    public void SetIconMap(Dictionary<string, Sprite> map)
    {
        iconById = map ?? new Dictionary<string, Sprite>();
    }

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

            // 라벨
            Text label = btn.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = labelStr;
                label.raycastTarget = false; // 텍스트가 레이를 막는 경우 방지
            }

            // 아이콘
            var icon = btn.transform.Find("IconMask/Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                icon.raycastTarget = false;
                icon.preserveAspect = false;

                if (iconById != null && iconById.TryGetValue(id, out var sp) && sp != null)
                {
                    icon.sprite = sp;
                    icon.enabled = true;
                }
                else
                {
                    icon.enabled = false; // 아이콘 없으면 숨김
                }
            }

            // 선택/호버 FX
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

        // ✅ 후보를 다시 만들고 나면 레이아웃 계산 뒤 맨 위로 보내기
        ForceVoteScrollTop();
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
