using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Transform targetCamera; // CenterEyeAnchor 또는 Main Camera 연결
    public float distance = 2.0f;  // 카메라와의 거리
    public float followSpeed = 5.0f; // 따라오는 속도 (값이 낮을수록 부드러움)

    void Update()
    {
        if (targetCamera == null) return;

        // 목적지 계산 (카메라 앞 정면)
        Vector3 targetPosition = targetCamera.position + (targetCamera.forward * distance);
        Quaternion targetRotation = targetCamera.rotation;

        // 부드럽게 이동 및 회전
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);
    }
}