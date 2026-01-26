using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // ✅ 네임스페이스 추가

// ✅ IPointerEnterHandler 인터페이스 상속 추가
public class J_TabStyle : MonoBehaviour, IPointerEnterHandler
{
    public Toggle toggle;
    public Image background;
    public Text label;
    public GameObject underline;

    public Color normalBg = Color.white;
    public Color selectedBg = Color.white;

    public Color normalText = Color.black;
    public Color selectedText = Color.black;

    void Start()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnChanged);
            OnChanged(toggle.isOn);
        }
    }

    public void OnChanged(bool on)
    {
        Debug.Log($"[UI Event] {gameObject.name} 상태 변경됨: {on}");

        if (background != null) background.color = on ? selectedBg : normalBg;
        if (label != null) label.color = on ? selectedText : normalText;
        if (underline != null) underline.SetActive(on);
    }

    // ✅ 레이가 UI 위에 올라갔을 때 실행되는 디버그 함수
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"<color=yellow>[Ray Hover]</color> {gameObject.name} 위에 레이가 닿았습니다!");
    }
}