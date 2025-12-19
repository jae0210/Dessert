using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RightHandRayLine : MonoBehaviour
{
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private bool shortenOnPhysicsHit = true;
    [SerializeField] private LayerMask hitMask = ~0; // 전부

    [Header("Optional Reticle (dot)")]
    [SerializeField] private Transform reticle; // 작은 구체 등(선택)

    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
    }

    private void Update()
    {
        Vector3 origin = transform.position;
        Vector3 dir = transform.forward;

        float dist = maxDistance;

        // 참고: UI(UGUI)는 물리 Raycast에 안 맞는 경우가 많아요.
        // 그래서 이건 "물리 오브젝트에 닿으면 선을 줄이는 옵션" 정도로만 사용.
        if (shortenOnPhysicsHit && Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            dist = hit.distance;

            if (reticle != null)
            {
                reticle.gameObject.SetActive(true);
                reticle.position = hit.point;
                reticle.forward = -dir;
            }
        }
        else
        {
            if (reticle != null)
            {
                reticle.gameObject.SetActive(true);
                reticle.position = origin + dir * dist;
                reticle.forward = -dir;
            }
        }

        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + dir * dist);
    }
}
