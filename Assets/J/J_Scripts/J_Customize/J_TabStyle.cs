using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class J_TabStyle : MonoBehaviour
{
    public Toggle toggle;
    public Image background;
    public Text label;
    public GameObject underline;

    [Header("Colors")]
    public Color normalBg = new Color32(34, 41, 55, 200);   // #222937
    public Color selectedBg = new Color32(46, 54, 68, 230); // #2E3644
    public Color normalText = new Color32(190, 198, 210, 255);
    public Color selectedText = Color.white;

    void Reset()
    {
        toggle = GetComponent<Toggle>();
    }

    void OnEnable()
    {
        if (!toggle) toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnChanged);
        OnChanged(toggle.isOn);
    }

    void OnDisable()
    {
        if (toggle) toggle.onValueChanged.RemoveListener(OnChanged);
    }

    void OnChanged(bool on)
    {
        if (background) background.color = on ? selectedBg : normalBg;
        if (label) label.color = on ? selectedText : normalText;
        if (underline) underline.SetActive(on);
    }
}
