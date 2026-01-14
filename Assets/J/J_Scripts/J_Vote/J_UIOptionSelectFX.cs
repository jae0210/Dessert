using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class J_UIOptionSelectFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Assign")]
    public RectTransform targetRoot; // 버튼 루트(없으면 자기 자신 RectTransform 사용)
    public Image frameImage;         // SelectedFrame의 Image (Raycast Target OFF 권장)

    [Header("Scale")]
    public float hoverScale = 1.03f;     // ✅ 레이로 가리킬 때
    public float selectedScale = 1.06f;  // ✅ 선택됐을 때
    public float animTime = 0.10f;

    [Header("Frame Color")]
    public Color deselectedColor = new Color(1f, 1f, 1f, 0f);     // 꺼짐(투명)
    public Color hoverColor = new Color(1f, 0.95f, 0.6f, 0.35f); // 은은한 골드
    public Color selectedColor = new Color(1f, 0.95f, 0.6f, 1f);    // 진한 골드

    [Header("Pulse (selected only)")]
    public bool pulse = true;
    public float pulseAmount = 0.015f;
    public float pulseSpeed = 6f;

    Coroutine co;
    Vector3 baseScale;

    bool isSelected;
    bool isHover;

    void Awake()
    {
        if (targetRoot == null) targetRoot = GetComponent<RectTransform>();
        baseScale = targetRoot != null ? targetRoot.localScale : Vector3.one;

        if (frameImage != null)
        {
            frameImage.color = deselectedColor;
            frameImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 선택된 상태일 때만 pulse
        if (!isSelected || !pulse || targetRoot == null) return;

        float s = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
        targetRoot.localScale = baseScale * selectedScale * s;
    }

    // ✅ 기존(선택) API는 그대로 유지
    public void SetSelected(bool on)
    {
        if (targetRoot == null) return;

        isSelected = on;

        // 비활성 오브젝트에서 코루틴 돌리면 에러났던 적 있어서 방어
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
        {
            ApplyImmediate();
            return;
        }

        StartTransition();
    }

    // ✅ 레이가 올라올 때(hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;

        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
        {
            ApplyImmediate();
            return;
        }

        StartTransition();
    }

    // ✅ 레이가 빠질 때
    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;

        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
        {
            ApplyImmediate();
            return;
        }

        StartTransition();
    }

    void StartTransition()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(CoAnim());
    }

    void ApplyImmediate()
    {
        var st = GetState();
        ApplyState(st.showFrame, st.color, st.scale);
        co = null;
    }

    (bool showFrame, Color color, Vector3 scale) GetState()
    {
        // 우선순위: Selected > Hover > None
        if (isSelected)
            return (true, selectedColor, baseScale * selectedScale);

        if (isHover)
            return (true, hoverColor, baseScale * hoverScale);

        return (false, deselectedColor, baseScale);
    }

    void ApplyState(bool show, Color col, Vector3 scale)
    {
        if (frameImage != null)
        {
            frameImage.color = col;
            frameImage.gameObject.SetActive(show);
        }

        // selected + pulse는 Update가 계속 스케일을 덮어쓰니
        // 여기서는 selected가 아닐 때만 정확히 적용
        if (!isSelected && targetRoot != null)
            targetRoot.localScale = scale;

        // selected인데 pulse 꺼져있으면 여기서 적용
        if (isSelected && !pulse && targetRoot != null)
            targetRoot.localScale = scale;
    }

    IEnumerator CoAnim()
    {
        var st = GetState();

        Vector3 fromScale = targetRoot != null ? targetRoot.localScale : Vector3.one;
        Vector3 toScale = st.scale;

        Color fromCol = frameImage != null ? frameImage.color : Color.white;
        Color toCol = st.color;

        if (frameImage != null && st.showFrame)
            frameImage.gameObject.SetActive(true);

        float t = 0f;
        while (t < animTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / animTime);

            // 선택+펄스면 Update가 스케일을 계속 바꾸니까, 여기선 선택 아닐 때만 Lerp
            if (targetRoot != null && !(isSelected && pulse))
                targetRoot.localScale = Vector3.Lerp(fromScale, toScale, k);

            if (frameImage != null)
                frameImage.color = Color.Lerp(fromCol, toCol, k);

            yield return null;
        }

        ApplyImmediate();
        co = null;
    }
}
