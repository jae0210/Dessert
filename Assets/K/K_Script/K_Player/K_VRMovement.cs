using UnityEngine;

public class VRMovement : MonoBehaviour
{
    public float speed = 2.0f; // 이동 속도
    public Transform cameraTransform; // OVRCameraRig의 CenterEyeAnchor를 연결

    void Update()
    {
        // 왼쪽 컨트롤러 조이스틱 입력 받기 (PrimaryThumbstick)
        // 오른쪽을 원하면 SecondaryThumbstick으로 변경
        Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        // 입력이 없으면 실행 안 함
        if (input.magnitude < 0.1f) return;

        // 카메라가 바라보는 방향 기준으로 이동 방향 계산
        Vector3 direction = new Vector3(input.x, 0, input.y);

        // 카메라의 Y축 회전만 반영 (하늘로 날아가지 않게)
        Vector3 headRotation = cameraTransform.eulerAngles;
        headRotation.x = 0;
        headRotation.z = 0;

        // 방향 벡터 회전
        direction = Quaternion.Euler(headRotation) * direction;

        // 이동 적용
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
}