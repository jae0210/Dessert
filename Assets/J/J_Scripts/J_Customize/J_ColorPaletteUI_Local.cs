using UnityEngine;
using UnityEngine.UI;

public class J_ColorPaletteUI_Local : MonoBehaviour
{
    public enum TargetPart { Body, Hat }

    [Header("Preview (optional)")]
    public J_PreviewAvatar preview;

    [Header("UI")]
    public RectTransform contentParent;   // Content
    public Button swatchPrefab;           // SwatchButton prefab (root has Button + J_SwatchButtonUI)
    public ScrollRect scrollRect;         // PaletteScroll (optional)

    [Header("Palettes")]
    public Color[] bodyPalette;
    public Color[] hatPalette;

    [Header("Selected Colors (ReadOnly)")]
    public Color selectedBody = Color.white;
    public Color selectedHat = Color.white;
    public TargetPart currentTarget = TargetPart.Body;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip pickSfx;           // 스와치(색상) 클릭
    public AudioClip tabSfx;            // 바디/모자 탭 전환
    [Range(0f, 1f)] public float pickVolume = 0.8f;
    [Range(0f, 1f)] public float tabVolume = 0.8f;
    public bool playOnlyWhenChanged = true;
    public bool suppressFirstTabSound = true; // 시작 시 토글 초기 호출에서 소리 안 나게

    private J_SwatchButtonUI lastSelected;
    private bool sfxReady;

    void Start()
    {
        // AudioSource 준비
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f; // UI용 2D
        }

        // 초기값이 비어있으면 팔레트 첫 색으로
        if (bodyPalette != null && bodyPalette.Length > 0 && selectedBody == default) selectedBody = bodyPalette[0];
        if (hatPalette != null && hatPalette.Length > 0 && selectedHat == default) selectedHat = hatPalette[0];

        ApplyToPreview_();
        Rebuild_();

        // 시작 직후 탭 이벤트로 소리 나는 거 방지용
        sfxReady = !suppressFirstTabSound;
        if (suppressFirstTabSound)
            Invoke(nameof(EnableTabSfx_), 0.05f);
    }

    void EnableTabSfx_() => sfxReady = true;

    // ✅ 토글 OnValueChanged에 그대로 연결하면 됨
    public void OnBodyTab(bool on)
    {
        if (!on) return;

        // 탭 사운드
        if (sfxReady && tabSfx != null && sfxSource != null)
            sfxSource.PlayOneShot(tabSfx, tabVolume);

        currentTarget = TargetPart.Body;
        Rebuild_();
    }

    public void OnHatTab(bool on)
    {
        if (!on) return;

        // 탭 사운드
        if (sfxReady && tabSfx != null && sfxSource != null)
            sfxSource.PlayOneShot(tabSfx, tabVolume);

        currentTarget = TargetPart.Hat;
        Rebuild_();
    }

    // ✅ 스와치가 눌렸을 때 여기로 들어옴
    public void PickFromSwatch(J_SwatchButtonUI swatch)
    {
        if (!swatch) return;

        // 같은 색을 또 눌렀을 때 소리/처리 생략(옵션)
        if (playOnlyWhenChanged && lastSelected == swatch)
            return;

        // 스와치 클릭 사운드
        if (pickSfx != null && sfxSource != null)
            sfxSource.PlayOneShot(pickSfx, pickVolume);

        if (lastSelected) lastSelected.SetSelected(false);
        swatch.SetSelected(true);
        lastSelected = swatch;

        var c = swatch.color; c.a = 1f;

        if (currentTarget == TargetPart.Body)
        {
            selectedBody = c;
            if (preview) preview.SetBodyColor(c);
        }
        else
        {
            selectedHat = c;
            if (preview) preview.SetHatColor(c);
        }

        Debug.Log($"[Pick {currentTarget}] {c}");
    }

    void Rebuild_()
    {
        if (!contentParent || !swatchPrefab)
        {
            Debug.LogWarning("[Palette] contentParent or swatchPrefab is null", this);
            return;
        }

        // 기존 스와치 제거
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        lastSelected = null;

        var palette = (currentTarget == TargetPart.Body) ? bodyPalette : hatPalette;
        var current = (currentTarget == TargetPart.Body) ? selectedBody : selectedHat;
        if (palette == null) return;

        for (int i = 0; i < palette.Length; i++)
        {
            var c = palette[i]; c.a = 1f;

            var btn = Instantiate(swatchPrefab, contentParent);
            // 혹시 프리팹에 OnClick이 붙어있으면 싹 제거 (중복 방지)
            btn.onClick.RemoveAllListeners();

            var ui = btn.GetComponent<J_SwatchButtonUI>();
            if (ui != null)
            {
                ui.Setup(c, this);

                if (Approximately_(c, current))
                {
                    ui.SetSelected(true);
                    lastSelected = ui;
                }
            }
        }

        // 레이아웃/스크롤 갱신
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);

        if (scrollRect)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    void ApplyToPreview_()
    {
        if (!preview) return;
        preview.SetBodyColor(selectedBody);
        preview.SetHatColor(selectedHat);
    }

    bool Approximately_(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.001f &&
               Mathf.Abs(a.g - b.g) < 0.001f &&
               Mathf.Abs(a.b - b.b) < 0.001f;
    }
}
