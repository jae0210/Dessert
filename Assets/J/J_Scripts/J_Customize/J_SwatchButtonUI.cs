using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class J_SwatchButtonUI : MonoBehaviour, IPointerDownHandler
{
    public Image colorFill;
    public GameObject selectedGlow;
    public GameObject checkMark;

    [HideInInspector] public Color color;
    private J_ColorPaletteUI_Local controller;

    public void Setup(Color c, J_ColorPaletteUI_Local ctrl)
    {
        color = c;
        controller = ctrl;

        if (colorFill) colorFill.color = c;
        SetSelected(false);
    }

    public void SetSelected(bool on)
    {
        if (selectedGlow) selectedGlow.SetActive(on);
        if (checkMark) checkMark.SetActive(on);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!controller) controller = GetComponentInParent<J_ColorPaletteUI_Local>();
        if (controller != null)
        {
            controller.PickFromSwatch(this);
        }
        else
        {
            Debug.LogWarning($"[Swatch] J_ColorPaletteUI_Local not found in parents: {name}", this);
        }
    }
}
