using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class ExhibitMediaToggle : MonoBehaviour
{
    [SerializeField] private GameObject textPanel;
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private VideoPlayer videoPlayer;

    // VideoPanel 안에 RawImage가 있다면 자동으로 찾아서 텍스처 연결
    private RawImage _rawImage;
    private RenderTexture _ownedRT;

    private void Awake()
    {
        AutoAssignIfNeeded();
        MakeRenderTargetUniquePerInstance();
    }

    private void Start()
    {
        if (textPanel) textPanel.SetActive(true);
        if (videoPanel) videoPanel.SetActive(false);
        if (videoPlayer) videoPlayer.Stop();
    }

    private void OnDestroy()
    {
        // 런타임 생성 RT 메모리 정리
        if (_ownedRT != null)
        {
            _ownedRT.Release();
            Destroy(_ownedRT);
        }
    }

    private void AutoAssignIfNeeded()
    {
        if (!videoPlayer) videoPlayer = GetComponentInChildren<VideoPlayer>(true);
        if (videoPanel == null)
        {
            var vpTr = videoPlayer ? videoPlayer.transform : null;
            if (vpTr) videoPanel = vpTr.gameObject; // 필요하면 부모로 바꿔도 됨
        }

        if (videoPanel)
            _rawImage = videoPanel.GetComponentInChildren<RawImage>(true);
    }

    private void MakeRenderTargetUniquePerInstance()
    {
        if (!videoPlayer) return;

        // RenderTexture를 쓰는 경우에만 해당
        if (videoPlayer.renderMode != VideoRenderMode.RenderTexture) return;

        // 기존 타겟 텍스처(에셋)를 복제해서 내 것(_ownedRT)으로 만든다
        var src = videoPlayer.targetTexture;

        RenderTexture newRT;
        if (src != null)
        {
            newRT = new RenderTexture(src.descriptor);
        }
        else
        {
            // 혹시 targetTexture가 비어있으면 적당한 크기로 생성
            newRT = new RenderTexture(1024, 1024, 0);
        }

        newRT.name = $"{gameObject.name}_VideoRT";
        newRT.Create();

        _ownedRT = newRT;
        videoPlayer.targetTexture = _ownedRT;

        // VideoPanel이 RawImage로 보여주는 구조면 RawImage에도 같은 RT를 물린다
        if (_rawImage) _rawImage.texture = _ownedRT;
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
