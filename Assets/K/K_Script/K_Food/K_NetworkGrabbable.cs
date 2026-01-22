using UnityEngine;
using Photon.Pun;

// OVRGrabbable을 상속받아 기존 기능 유지 + 네트워크 기능 추가
public class K_NetworkGrabbable : OVRGrabbable
{
    private PhotonView pv;

    protected override void Start()
    {
        base.Start();
        pv = GetComponent<PhotonView>();
    }

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        base.GrabBegin(hand, grabPoint);

        // 잡는 순간 소유권 요청 -> 이제 내가 주인(IsMine = true)이 됨
        if (pv != null)
        {
            pv.RequestOwnership();
        }
    }
}