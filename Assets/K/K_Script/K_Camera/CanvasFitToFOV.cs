using UnityEngine;

[ExecuteAlways]
public class CanvasFitToFOV : MonoBehaviour
{
    public Camera targetCamera;
    [Range(0.3f, 10f)] public float distance = 1.2f;   // 카메라 앞 거리(m)
    [Range(0.8f, 1.0f)] public float fill = 0.95f;    // 1.0이면 완전 꽉, 0.95면 살짝 여백
    public bool matchVertical = true;                 // 보통 세로 기준으로 맞춤

    RectTransform rt;

    void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        Fit();
    }

    void LateUpdate() => Fit();

    void Fit()
    {
        if (!targetCamera || !rt) return;

        // 카메라 앞에 고정
        transform.localPosition = new Vector3(0, 0, distance);
        transform.localRotation = Quaternion.identity;

        // 카메라 FOV로 "그 거리에서의 화면 높이/폭(m)" 계산
        float fovY = targetCamera.fieldOfView * Mathf.Deg2Rad;
        float worldH = 2f * distance * Mathf.Tan(fovY * 0.5f) * fill;
        float worldW = worldH * targetCamera.aspect;

        // 현재 캔버스 픽셀 크기(예: 1920x1080)를 월드 크기에 맞게 스케일 설정
        float scale;
        if (matchVertical) scale = worldH / rt.rect.height;
        else scale = worldW / rt.rect.width;

        transform.localScale = Vector3.one * scale;
    }
}
