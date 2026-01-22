using UnityEngine;
using Photon.Pun;

public class K_NetworkOVRGrabbable : OVRGrabbable
{
    private PhotonView photonView;

    protected override void Start()
    {
        base.Start();
        photonView = GetComponent<PhotonView>();
    }

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        // 원래의 잡기 로직 수행
        base.GrabBegin(hand, grabPoint);

        // 포톤 소유권 가져오기
        if (photonView && photonView.Owner != PhotonNetwork.LocalPlayer)
        {
            photonView.RequestOwnership();
        }
    }
}