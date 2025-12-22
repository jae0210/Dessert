using UnityEngine;

public class K_Rotator : MonoBehaviour
{
    [Tooltip("초당 회전 속도 (양수: 시계방향, 음수: 반시계방향)")]
    public float rotationSpeed = 30f;

    [Tooltip("회전할 축 설정")]
    public Vector3 rotationAxis = Vector3.up; // 기본은 Y축(수직) 회전

    void Update()
    {
        // 매 프레임마다 설정한 축을 기준으로 회전
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}