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

    [Tooltip("원격만 부드럽게 보이게 하는 보간 시간")]
    [SerializeField] private float remoteDampTime = 0.12f;

    [Header("Teleport Cut")]
    [SerializeField] private float teleportDistanceCutoff = 0.8f;

    [Header("Stop Dead Zone")]
    [Tooltip("이 값보다 느리면 0으로 처리(멈출 때 잔떨림 제거)")]
    [SerializeField] private float stopDeadZone = 0.05f;

    private Vector3 prevPos;
    private float netSpeed;         // 원격에서 받은 speed
    private float localSpeedToSend; // 로컬이 계산해서 보낼 speed

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>(true);
        if (!speedSource) speedSource = transform;

        prevPos = speedSource.position;
    }

    void Update()
    {
        if (!animator) return;

        if (photonView.IsMine)
        {
            // ✅ 이동량(델타) 기반 속도: 어떤 이동 방식이든 확실히 잡힘
            Vector3 cur = speedSource.position;
            Vector3 delta = cur - prevPos;
            prevPos = cur;

            // 텔레포트/순간이동 컷
            if (delta.magnitude > teleportDistanceCutoff) delta = Vector3.zero;

            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            float speed = new Vector2(delta.x, delta.z).magnitude / dt;
            speed *= speedMultiplier;

            // 멈출 때 잔떨림 컷
            if (speed < stopDeadZone) speed = 0f;

            // ✅ 보낼 값 저장(이 값 그대로 네트워크 전송)
            localSpeedToSend = speed;

            // ✅ 로컬은 즉시 반영(딜레이 거의 0)
            animator.SetFloat(speedParam, speed);
        }
        else
        {
            // ✅ 원격은 살짝 부드럽게(보기 좋게)
            float remote = netSpeed;
            if (remote < stopDeadZone) remote = 0f;

            animator.SetFloat(speedParam, remote, remoteDampTime, Time.deltaTime);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // ✅ animator.GetFloat() 말고, 로컬이 계산한 값을 그대로 보냄
            stream.SendNext(localSpeedToSend);
        }
        else
        {
            netSpeed = (float)stream.ReceiveNext();
        }
    }
}
