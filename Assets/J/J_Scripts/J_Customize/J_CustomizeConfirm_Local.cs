using UnityEngine;
using UnityEngine.SceneManagement;

public class J_CustomizeConfirm_Local : MonoBehaviour
{
    public const string KEY_BODY = "bodyCol";
    public const string KEY_HAT = "hatCol";

    public J_ColorPaletteUI_Local paletteUI;
    public string mainSceneName = "Room_J_T";

    public void Confirm()
    {
        int bodyPacked = J_ColorPack.Pack(paletteUI.selectedBody);
        int hatPacked = J_ColorPack.Pack(paletteUI.selectedHat);

        PlayerPrefs.SetInt(KEY_BODY, bodyPacked);
        PlayerPrefs.SetInt(KEY_HAT, hatPacked);
        PlayerPrefs.Save();

        SceneManager.LoadScene(mainSceneName);
    }
}
