using System;
using UnityEngine;
using UnityEngine.UI;

public class J_EmoteRadialMenuUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform root;   // 원형 배치 기준
    [SerializeField] private Button buttonPrefab;  // 아이콘 버튼 프리팹
    [SerializeField] private float radius = 140f;  // 원형 반지름(px)

    private Action<int> onSelect;

    public void Build(Sprite[] icons, Action<int> onSelectCallback)
    {
        onSelect = onSelectCallback;

        // 기존 버튼 정리
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);

        if (icons == null || icons.Length == 0) return;

        float step = 360f / icons.Length;

        for (int i = 0; i < icons.Length; i++)
        {
            float ang = (step * i - 90f) * Mathf.Deg2Rad; // 위쪽부터 시작
            Vector2 pos = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius;

            var btn = Instantiate(buttonPrefab, root);
            var rt = btn.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;

            var img = btn.GetComponentInChildren<Image>();
            if (img != null) img.sprite = icons[i];

            int idx = i;
            btn.onClick.AddListener(() => onSelect?.Invoke(idx));
        }
    }
}
