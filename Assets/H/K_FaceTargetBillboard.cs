using UnityEngine;

public class K_FaceTargetBillboard : MonoBehaviour
{
    [Header("Target")]
    public Transform target;              // 보통 CenterEyeAnchor(카메라)

    [Header("Options")]
    public bool yAxisOnly = true;         // Y축만 회전(안내판이 눕지 않게)
    public bool flip180 = false;          // 방향이 반대로 보이면 체크
    public float rotationSpeed = 8f;      // 0이면 즉시 회전, 값 크면 부드럽게

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position);

        if (yAxisOnly)
            dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);

        if (flip180)
            lookRot *= Quaternion.Euler(0f, 180f, 0f);

        if (rotationSpeed <= 0f)
            transform.rotation = lookRot;
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
    }
}
