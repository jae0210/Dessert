using UnityEngine;
using Photon.Pun; // PUN 2 네임스페이스
using Photon.Realtime;

// OVRGrabbable 상속 + PUN 기능 사용
public class K_NetworkGrabbable_PUN : OVRGrabbable
{
    private PhotonView pv;
    private Rigidbody rb;

    protected override void Start()
    {
        base.Start();
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
    }

    // 잡을 때 호출
    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        base.GrabBegin(hand, grabPoint);

        // 핵심: 소유권 요청 (내가 주인 할래!)
        if (pv != null && !pv.IsMine)
        {
            pv.RequestOwnership();
        }
    }

    // 던질 때 호출
    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        base.GrabEnd(linearVelocity, angularVelocity);

        // 주인이 되었으므로 물리력 동기화는 PhotonRigidbodyView가 자동으로 처리함
        // 하지만 확실한 반응을 위해 로컬 물리도 즉시 적용
        if (rb != null)
        {
            rb.velocity = linearVelocity;
            rb.angularVelocity = angularVelocity;
        }
    }
}