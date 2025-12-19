using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject[] panels;

    [Header("Scene")]
    [SerializeField] private string startSceneName = "Room_1";

    public void StartGame()
    {
        SceneManager.LoadScene(startSceneName);
    }

    public void ShowPanel(GameObject panelToShow)
    {
        HideAllPanels();
        if (panelToShow != null) panelToShow.SetActive(true);
    }

    public void HideAllPanels()
    {
        foreach (GameObject panel in panels)
            panel.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
