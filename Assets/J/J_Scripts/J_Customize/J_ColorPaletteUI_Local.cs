using UnityEngine;
using UnityEngine.UI;

public class J_ColorPaletteUI_Local : MonoBehaviour
{
    public enum TargetPart { Body, Hat }

    [Header("Preview")]
    public J_PreviewAvatar preview;

    [Header("UI")]
    public Transform contentParent;
    public Button swatchPrefab;

    [Header("Palettes")]
    public Color[] bodyPalette;
    public Color[] hatPalette;

    [Header("State")]
    public TargetPart currentTarget = TargetPart.Body;
    public Color selectedBody = Color.white;
    public Color selectedHat = Color.white;

    // ✅ 이제는 SelectedBorder가 아니라 “마지막 선택된 Swatch UI”를 기억
    J_SwatchButtonUI lastSelectedSwatch;

    void Start()
    {
        if (bodyPalette != null && bodyPalette.Length > 0) selectedBody = bodyPalette[0];
        if (hatPalette != null && hatPalette.Length > 0) selectedHat = hatPalette[0];

        preview?.SetBodyColor(selectedBody);
        preview?.SetHatColor(selectedHat);

        Rebuild();
    }

    // ✅ Toggle의 OnValueChanged(bool)에서 쓰기 좋게 래퍼 추가
    public void OnBodyTabChanged(bool isOn)
    {
        if (!isOn) return;
        SetTargetBody();
    }
    public void OnHatTabChanged(bool isOn)
    {
        if (!isOn) return;
        SetTargetHat();
    }

    public void SetTargetBody()
    {
        currentTarget = TargetPart.Body;
        Rebuild();
    }

    public void SetTargetHat()
    {
        currentTarget = TargetPart.Hat;
        Rebuild();
    }

    void Rebuild()
    {
        // 기존 제거
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        lastSelectedSwatch = null;

        Color[] palette = currentTarget == TargetPart.Body ? bodyPalette : hatPalette;
        if (palette == null) return;

        Color currentSelected = currentTarget == TargetPart.Body ? selectedBody : selectedHat;

        for (int i = 0; i < palette.Length; i++)
        {
            Color c = palette[i];
            c.a = 1f;

            var btn = Instantiate(swatchPrefab, contentParent);

            // ✅ Swatch UI 컴포넌트로 ColorFill에 색 넣기
            var ui = btn.GetComponent<J_SwatchButtonUI>();
            if (ui != null)
            {
                ui.SetColor(c);
                ui.SetSelected(false);
            }
            else
            {
                // 혹시 컴포넌트 없을 때 대비(백업): ColorFill 찾아서 칠함
                var fillT = btn.transform.Find("ColorFill");
                if (fillT)
                {
                    var fillImg = fillT.GetComponent<Image>();
                    if (fillImg) fillImg.color = c;
                }
            }

            // 클로저 캡처 안전
            var uiCaptured = ui;
            btn.onClick.AddListener(() => Pick(c, uiCaptured));

            // 기본 선택 표시
            if (ui != null && ApproximatelySameColor(c, currentSelected))
            {
                ui.SetSelected(true);
                lastSelectedSwatch = ui;
            }
        }
    }

    void Pick(Color c, J_SwatchButtonUI ui)
    {
        c.a = 1f;

        if (lastSelectedSwatch != null)
            lastSelectedSwatch.SetSelected(false);

        if (ui != null)
        {
            ui.SetSelected(true);
            lastSelectedSwatch = ui;
        }

        if (currentTarget == TargetPart.Body)
        {
            selectedBody = c;
            preview?.SetBodyColor(c);
        }
        else
        {
            selectedHat = c;
            preview?.SetHatColor(c);
        }
    }

    bool ApproximatelySameColor(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.01f &&
               Mathf.Abs(a.g - b.g) < 0.01f &&
               Mathf.Abs(a.b - b.b) < 0.01f;
    }
}
