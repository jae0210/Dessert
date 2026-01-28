using UnityEngine;
using UnityEngine.AI;

public class LocalNPCRoam : MonoBehaviour
{
    public float roamRadius = 6f;
    public float minIdle = 0.5f;
    public float maxIdle = 2.0f;

    public Animator animator;
    public string speedParam = "Speed";

    NavMeshAgent agent;
    float idleUntil;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (animator != null) animator.applyRootMotion = false; // Agent가 이동 주도
    }

    void Start()
    {
        PickNewPoint();
    }

    void Update()
    {
        if (animator != null && agent != null)
            animator.SetFloat(speedParam, agent.velocity.magnitude);

        if (agent == null || !agent.isOnNavMesh) return;

        if (Time.time < idleUntil) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            idleUntil = Time.time + Random.Range(minIdle, maxIdle);
            PickNewPoint();
        }
    }

    void PickNewPoint()
    {
        Vector2 r = Random.insideUnitCircle * roamRadius;
        Vector3 candidate = transform.position + new Vector3(r.x, 0f, r.y);

        if (NavMesh.SamplePosition(candidate, out var hit, 2f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }
}
