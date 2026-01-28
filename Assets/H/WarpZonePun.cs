using UnityEngine;
using Photon.Pun;

public class WarpZonePun : MonoBehaviour
{
    public Transform target;
    public float upOffset = 0.05f;   // 바닥 끼임 방지
    public bool matchYaw = true;     // 착지 방향 맞추기(추천)

    void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (target == null) return;

        var pv = other.GetComponentInParent<PhotonView>();
        if (pv != null && !pv.IsMine) return;

        // ★ 여기서 moveRoot는 "실제 플레이어 리그(OVRPlayerController)"여야 함
        var ovr = FindObjectOfType<OVRPlayerController>();
        if (ovr == null) return;

        Transform root = ovr.transform;

        // HMD(카메라) 트랜스폼
        Transform head = Camera.main != null ? Camera.main.transform : null;
        if (head == null) return;

        // CC 잠깐 끄고 워프 (튕김 방지)
        var cc = root.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 1) Yaw(수평 회전)만 맞추기 (Pitch/Roll은 건드리면 어지러움)
        if (matchYaw)
        {
            float yawDelta = target.eulerAngles.y - head.eulerAngles.y;
            root.Rotate(0f, yawDelta, 0f);
        }

        // 회전 후 head 위치가 바뀌므로 다시 읽기
        Vector3 headPos = head.position;

        // 2) "head가 target로 오도록" root를 XZ로 보정 이동
        Vector3 dst = target.position + Vector3.up * upOffset;
        Vector3 delta = dst - headPos;

        root.position += new Vector3(delta.x, 0f, delta.z);
        // 3) 바닥 높이는 target Y로 스냅(층 이동 목적이면 이게 안정적)
        root.position = new Vector3(root.position.x, target.position.y, root.position.z);

        if (cc != null) cc.enabled = true;
    }
}
