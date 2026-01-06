using UnityEngine;
using UnityEngine.Video;

public class ExhibitMediaToggle : MonoBehaviour
{
    public GameObject textPanel;
    public GameObject videoPanel;
    public VideoPlayer videoPlayer;

    void Start()
    {
        // 시작 상태: 텍스트 ON, 비디오 OFF
        if (textPanel) textPanel.SetActive(true);
        if (videoPanel) videoPanel.SetActive(false);

        if (videoPlayer)
        {
            videoPlayer.Stop();
        }
    }

    public void ShowVideo()
    {
        if (textPanel) textPanel.SetActive(false);
        if (videoPanel) videoPanel.SetActive(true);

        if (videoPlayer)
        {
            videoPlayer.time = 0;
            videoPlayer.Play();
        }
    }

    public void ShowText()
    {
        if (videoPlayer) videoPlayer.Stop();

        if (videoPanel) videoPanel.SetActive(false);
        if (textPanel) textPanel.SetActive(true);
    }
}
