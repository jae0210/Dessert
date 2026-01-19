using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class J_EmoteRadialMenuUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform root;          // ButtonsRoot
    [SerializeField] private RectTransform ringRect;      // Ring RectTransform
    [SerializeField] private Button buttonPrefab;

    [Header("Layout")]
    [SerializeField] private int spokeCount = 8;
    [SerializeField] private bool placeBetweenSpokes = true; // false=선 위, true=선 사이
    [SerializeField] private float radiusPadding = 18f;
    [SerializeField] private Vector2 buttonSize = new Vector2(55, 55);
    [SerializeField] private float highlightedScale = 1.15f;

    private readonly List<Button> buttons = new();
    private int highlighted = -1;
    private float lastRadius = 140f;

    public int Count => buttons.Count;
    public int HighlightedIndex => highlighted;

    public void Build(Sprite[] icons, UnityAction<int> onClick)
    {
        if (!root || !buttonPrefab) return;

        // 버튼만 정리(링은 root 밖에 있어야 안전)
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);

        buttons.Clear();
        highlighted = -1;

        if (icons == null || icons.Length == 0) return;

        int nIcons = icons.Length;
        int nSlots = Mathf.Max(1, spokeCount);

        // 링 크기 기반 radius 계산
        float radius = 140f;
        if (ringRect != null)
        {
            float w = ringRect.rect.width;
            float h = ringRect.rect.height;
            float r = Mathf.Min(w, h) * 0.5f;
            radius = Mathf.Max(0f, r - radiusPadding);
        }
        lastRadius = radius;

        float step = 360f / nSlots;
        float offset = placeBetweenSpokes ? step * 0.5f : 0f;

        for (int i = 0; i < nIcons; i++)
        {
            float aDeg = (step * i + offset - 90f); // 0번이 위쪽 기준
            float a = aDeg * Mathf.Deg2Rad;

            Button btn = Instantiate(buttonPrefab, root);
            btn.name = $"EmoteBtn_{i}";

            // 네비게이션 때문에 "첫번째가 선택됨" 같은 현상 방지
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;

            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = buttonSize;
            rt.anchoredPosition = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            rt.localScale = Vector3.one;

            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = icons[i];
                img.preserveAspect = true;
                img.raycastTarget = true;
                img.color = Color.white;
            }

            // Hover(포인터가 올라가면 커지게)
            var trig = btn.gameObject.GetComponent<EventTrigger>();
            if (!trig) trig = btn.gameObject.AddComponent<EventTrigger>();
            trig.triggers ??= new List<EventTrigger.Entry>();
            trig.triggers.Clear();

            int idx = i;

            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => SetHighlight(idx));
            trig.triggers.Add(enter);

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => { /* 유지 or Clear는 Input에서 처리 */ });
            trig.triggers.Add(exit);

            // 클릭은 옵션 (지금은 "A 떼면 확정"이라 클릭 없어도 됨)
            btn.onClick.RemoveAllListeners();
            if (onClick != null) btn.onClick.AddListener(() => onClick.Invoke(idx));

            buttons.Add(btn);
        }
    }

    public void SetHighlight(int index)
    {
        if (buttons.Count == 0) return;

        if (index < 0 || index >= buttons.Count)
        {
            ClearHighlight();
            return;
        }

        if (highlighted == index) return;

        if (highlighted >= 0 && highlighted < buttons.Count)
            buttons[highlighted].transform.localScale = Vector3.one;

        highlighted = index;
        buttons[highlighted].transform.localScale = Vector3.one * highlightedScale;
    }

    public void ClearHighlight()
    {
        if (highlighted >= 0 && highlighted < buttons.Count)
            buttons[highlighted].transform.localScale = Vector3.one;
        highlighted = -1;
    }

    // 컨트롤러 레이로 "섹터"를 계산해서 인덱스 뽑기(버튼을 정확히 찍지 않아도 선택되게)
    public int PickIndexFromWorldRay(Ray ray, float maxDistance, float deadZone01 = 0.35f)
    {
        if (!root || buttons.Count == 0) return -1;

        Transform planeT = (ringRect != null) ? ringRect.transform : root.transform;
        Plane plane = new Plane(planeT.forward, planeT.position);

        if (!plane.Raycast(ray, out float enter)) return -1;
        if (enter < 0f || enter > maxDistance) return -1;

        Vector3 hit = ray.GetPoint(enter);
        Vector3 local = root.InverseTransformPoint(hit);
        Vector2 p = new Vector2(local.x, local.y);

        // 중심부는 선택 안 되게(실수 방지)
        float dz = Mathf.Max(0.0001f, lastRadius * deadZone01);
        if (p.magnitude < dz) return -1;

        float step = 360f / Mathf.Max(1, spokeCount);
        float offset = placeBetweenSpokes ? step * 0.5f : 0f;

        // 위쪽(12시)을 0도로 맞춘 뒤, offset 보정
        float ang = Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg;   // right=0, up=90
        float a = (ang + 90f + 360f) % 360f;                 // up=0
        a = (a - offset + 360f) % 360f;

        int idx = Mathf.FloorToInt(a / step);

        if (idx < 0 || idx >= buttons.Count) return -1;
        return idx;
    }
}
