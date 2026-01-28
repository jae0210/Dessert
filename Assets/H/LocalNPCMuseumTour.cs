using UnityEngine;
using UnityEngine.AI;

public class LocalNPCMuseumTour : MonoBehaviour
{
    [Header("Exhibits (look points on NavMesh)")]
    public Transform[] exhibits;

    [Header("Move/Look")]
    public float arriveDistance = 0.4f;
    public float minLookTime = 2.0f;
    public float maxLookTime = 5.0f;
    public float turnSpeed = 240f; // deg/sec

    [Header("Animation")]
    public Animator animator;
    public string speedParam = "Speed";

    NavMeshAgent agent;
    int current = -1;
    float lookUntil = 0f;
    bool looking = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.applyRootMotion = false;
    }

    void Start()
    {
        if (exhibits == null || exhibits.Length == 0)
        {
            // 전시물 지정 안 하면 그냥 기존 로밍으로 두고 싶으면 여기서 enabled=false 해도 됨
            enabled = false;
            return;
        }

        GoNextExhibit();
    }

    void Update()
    {
        if (animator != null && agent != null)
            animator.SetFloat(speedParam, agent.velocity.magnitude);

        if (agent == null || !agent.isOnNavMesh) return;
        if (exhibits == null || exhibits.Length == 0) return;

        if (!looking)
        {
            // 도착 체크
            if (!agent.pathPending && agent.remainingDistance <= arriveDistance)
            {
                looking = true;
                agent.isStopped = true;

                lookUntil = Time.time + Random.Range(minLookTime, maxLookTime);
            }
        }
        else
        {
            // 전시물 쪽으로 천천히 고개(몸) 돌리기
            var target = exhibits[current];
            if (target != null)
            {
                Vector3 dir = target.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion desired = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, turnSpeed * Time.deltaTime);
                }
            }

            // 구경 시간 끝나면 다음 전시물로
            if (Time.time >= lookUntil)
            {
                looking = false;
                agent.isStopped = false;
                GoNextExhibit();
            }
        }
    }

    void GoNextExhibit()
    {
        // 랜덤으로 고르기(같은 곳 연속 방지)
        int next = current;
        if (exhibits.Length == 1) next = 0;
        else
        {
            while (next == current)
                next = Random.Range(0, exhibits.Length);
        }

        current = next;

        Transform t = exhibits[current];
        if (t == null) return;

        // NavMesh 위로 스냅 후 목적지 설정
        if (NavMesh.SamplePosition(t.position, out var hit, 1.5f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            agent.SetDestination(t.position);
    }
}
