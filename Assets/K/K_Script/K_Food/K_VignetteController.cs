using UnityEngine;
using UnityEngine.UI;

public class K_VignetteController : MonoBehaviour
{
    [Header("UI Reference")]
    public Image vignetteImage;

    [Header("Settings")]
    [Tooltip("입력 값에 대한 민감도입니다. 값이 클수록 스틱을 조금만 밀어도 어두워집니다.")]
    public float inputSensitivity = 2.0f; // 입력값(0~1)에 곱해질 배수
    public float maxAlpha = 0.7f;
    public float smoothTime = 5.0f;

    // 더 이상 Transform 추적은 필요하지 않습니다.
    // public Transform playerTransform; 

    private float targetAlpha;
    private float currentAlpha;

    void Start()
    {
        if (vignetteImage != null)
        {
            Color c = vignetteImage.color;
            c.a = 0f;
            vignetteImage.color = c;
        }
    }

    void Update()
    {
        if (vignetteImage == null) return;

        // 1. 컨트롤러 입력 감지 (OVRInput 기준)
        // 왼쪽 컨트롤러 스틱 (이동)
        Vector2 moveInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        // 오른쪽 컨트롤러 스틱 (회전)
        Vector2 rotateInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        // 2. 입력 강도 계산 (0.0 ~ 1.0)
        // 스틱을 기울인 정도(magnitude)를 가져옵니다.
        float moveMagnitude = moveInput.magnitude;
        float rotateMagnitude = rotateInput.magnitude;

        // 3. 목표 알파값 계산
        // 실제 몸의 움직임은 무시하고, 오직 스틱 입력이 있을 때만 값이 발생합니다.
        float moveFactor = Mathf.Clamp01(moveMagnitude * inputSensitivity);
        float rotateFactor = Mathf.Clamp01(rotateMagnitude * inputSensitivity);

        // 이동과 회전 중 더 큰 입력값을 기준으로 함
        float intensity = Mathf.Max(moveFactor, rotateFactor);
        targetAlpha = intensity * maxAlpha;

        // 4. 부드러운 전환 (Lerp)
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * smoothTime);

        // 5. 적용
        Color color = vignetteImage.color;
        color.a = currentAlpha;
        vignetteImage.color = color;
    }
}