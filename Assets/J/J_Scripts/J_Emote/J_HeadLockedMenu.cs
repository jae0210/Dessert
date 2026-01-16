using Photon.Pun;
using UnityEngine;

public class J_HeadLockedMenu : MonoBehaviourPun
{
    [Header("Head Locked")]
    public Vector3 localOffset = new Vector3(0f, -0.05f, 0.6f); // 눈앞 0.6m, 살짝 아래
    public bool lockRoll = true;

    void Start()
    {
        // ★ 내 것만 보이게 (중요)
        var pv = GetComponentInParent<PhotonView>();
        if (pv != null && !pv.IsMine)
        {
            gameObject.SetActive(false);
            return;
        }

        var rig = FindObjectOfType<OVRCameraRig>();
        if (rig == null) return;

        // CenterEyeAnchor 밑으로 붙이기
        transform.SetParent(rig.centerEyeAnchor, false);
        transform.localPosition = localOffset;
        transform.localRotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        if (!lockRoll) return;

        // 롤만 제거(머리 기울임으로 UI가 기울어지는 거 방지)
        var e = transform.localEulerAngles;
        transform.localRotation = Quaternion.Euler(e.x, e.y, 0f);
    }
}
