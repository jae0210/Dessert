using UnityEngine;

public class K_VRLaser : MonoBehaviour
{
    public LineRenderer lineRenderer; // 선을 그리는 컴포넌트
    public float maxDistance = 3.0f;  // 레이저의 최대 길이

    void Update()
    {
        // 1. 선의 시작점은 항상 컨트롤러의 현재 위치
        lineRenderer.SetPosition(0, transform.position);

        RaycastHit hit;
        // 2. 컨트롤러 정면으로 레이저를 쏴서 무언가에 부딪혔다면
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
        {
            // 끝점을 부딪힌 위치로 설정 (선이 물체에서 멈춤)
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            // 허공이라면 최대 길이만큼 길게 뻗음
            lineRenderer.SetPosition(1, transform.position + transform.forward * maxDistance);
        }
    }
}