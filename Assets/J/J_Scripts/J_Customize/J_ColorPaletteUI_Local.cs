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

    GameObject lastSelectedBorder;

    void Start()
    {
        // 기본값(첫 색) 선택
        if (bodyPalette != null && bodyPalette.Length > 0) selectedBody = bodyPalette[0];
        if (hatPalette != null && hatPalette.Length > 0) selectedHat = hatPalette[0];

        preview?.SetBodyColor(selectedBody);
        preview?.SetHatColor(selectedHat);

        Rebuild(); // 여기서 기본 선택 테두리도 켬
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

        lastSelectedBorder = null;

        Color[] palette = currentTarget == TargetPart.Body ? bodyPalette : hatPalette;
        if (palette == null) return;

        // 현재 파트에서 “이미 선택된 색”이 무엇인지
        Color currentSelected = currentTarget == TargetPart.Body ? selectedBody : selectedHat;

        for (int i = 0; i < palette.Length; i++)
        {
            Color c = palette[i];   // 루프 캡처 안전
            c.a = 1f;

            var btn = Instantiate(swatchPrefab, contentParent);

            // 버튼 색 표시
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = c;

            // 테두리 찾기
            Transform borderT = btn.transform.Find("SelectedBorder");
            GameObject borderGO = borderT ? borderT.gameObject : null;
            if (borderGO != null) borderGO.SetActive(false);

            // 클릭 연결
            btn.onClick.AddListener(() => Pick(c, borderGO));

            // ✅ 기본 선택 테두리 켜기(현재 선택색과 같으면)
            if (ApproximatelySameColor(c, currentSelected) && borderGO != null)
            {
                borderGO.SetActive(true);
                lastSelectedBorder = borderGO;
            }
        }
    }

    void Pick(Color c, GameObject borderObj)
    {
        c.a = 1f;

        if (lastSelectedBorder != null) lastSelectedBorder.SetActive(false);

        if (borderObj != null)
        {
            borderObj.SetActive(true);
            lastSelectedBorder = borderObj;
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

    // Color는 float라 “완전 동일” 비교가 안 맞을 때가 있어. 가까우면 같은 걸로 처리.
    bool ApproximatelySameColor(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.01f &&
               Mathf.Abs(a.g - b.g) < 0.01f &&
               Mathf.Abs(a.b - b.b) < 0.01f;
    }
}
