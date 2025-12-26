using UnityEngine;

public class LaserPointer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float maxDistance = 5.0f; // 레이저 길이

    void Update()
    {
        lineRenderer.SetPosition(0, transform.position); // 시작점: 컨트롤러 위치

        RaycastHit hit;
        // 정면으로 레이를 쏴서 무언가에 맞았다면
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance))
        {
            lineRenderer.SetPosition(1, hit.point); // 끝점: 부딪힌 위치
        }
        else
        {
            // 안 맞았다면 최대 거리까지 쭉 뻗음
            lineRenderer.SetPosition(1, transform.position + transform.forward * maxDistance);
        }
    }
}