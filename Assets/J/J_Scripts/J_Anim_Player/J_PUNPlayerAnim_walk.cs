using Photon.Pun;
using UnityEngine;

public class J_PUNPlayerAnim_Walk : MonoBehaviourPun, IPunObservable
{
    [Header("Animator (Visual child)")]
    [SerializeField] private Animator animator;

    [Header("Speed Source (실제로 움직이는 Transform)")]
    [Tooltip("비우면 this.transform 기준. 루트가 안 움직이면 실제로 움직이는 오브젝트를 넣기")]
    [SerializeField] private Transform speedSource;

    [Header("Param")]
    [SerializeField] private string speedParam = "Speed";

    [Header("Tuning")]
    [SerializeField] private float speedMultiplier = 1f;

    [Tooltip("원격 플레이어만 부드럽게 보간할 때 사용 (로컬은 즉시 반영)")]
    [SerializeField] private float dampTime = 0.12f;

    [Header("Teleport Cut")]
    [SerializeField] private float teleportDistanceCutoff = 0.8f;

    [Header("Stop Dead Zone")]
    [Tooltip("이 값보다 느리면 멈춘 것으로 처리해서 Idle 전환 딜레이/떨림을 줄임")]
    [SerializeField] private float stopDeadZone = 0.05f;

    private CharacterController cc;
    private Vector3 prevPos;
    private float netSpeed;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>(true);
        if (!speedSource) speedSource = transform;

        cc = GetComponent<CharacterController>();
        prevPos = speedSource.position;
    }

    void Update()
    {
        if (!animator) return;

        if (photonView.IsMine)
        {
            float speed;

            // 1) CC가 있으면 velocity가 제일 안정적
            if (cc != null)
            {
                Vector3 v = cc.velocity;
                speed = new Vector2(v.x, v.z).magnitude * speedMultiplier;
            }
            else
            {
                // 2) 없으면 위치 변화량으로 계산
                Vector3 cur = speedSource.position;
                Vector3 delta = cur - prevPos;
                prevPos = cur;

                if (delta.magnitude > teleportDistanceCutoff) delta = Vector3.zero;

                float dt = Mathf.Max(Time.deltaTime, 0.0001f);
                speed = new Vector2(delta.x, delta.z).magnitude / dt;
                speed *= speedMultiplier;
            }

            // 멈출 때 미세 떨림/관성 컷
            if (speed < stopDeadZone) speed = 0f;

            // ✅ 로컬은 즉시 반영(딜레이 0)
            animator.SetFloat(speedParam, speed);
        }
        else
        {
            // ✅ 원격은 살짝 부드럽게(보기 좋게)
            float remote = netSpeed;
            if (remote < stopDeadZone) remote = 0f;

            animator.SetFloat(speedParam, remote, dampTime, Time.deltaTime);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            float s = animator ? animator.GetFloat(speedParam) : 0f;
            stream.SendNext(s);
        }
        else
        {
            netSpeed = (float)stream.ReceiveNext();
        }
    }
}
