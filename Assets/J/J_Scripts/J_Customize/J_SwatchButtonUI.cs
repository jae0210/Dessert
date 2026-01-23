using UnityEngine;
using UnityEngine.UI;

public class J_SwatchButtonUI : MonoBehaviour
{
    public Image colorFill;
    public GameObject selectedGlow;
    public GameObject checkMark;

    public void SetColor(Color c)
    {
        if (colorFill) colorFill.color = c;
    }

    public void SetSelected(bool on)
    {
        if (selectedGlow) selectedGlow.SetActive(on);
        if (checkMark) checkMark.SetActive(on);
    }
}
