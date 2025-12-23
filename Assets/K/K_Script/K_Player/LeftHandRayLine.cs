using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LeftHandRayLine : MonoBehaviour
{
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private bool shortenOnPhysicsHit = true;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Optional Reticle (dot)")]
    [SerializeField] private Transform reticle;

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
