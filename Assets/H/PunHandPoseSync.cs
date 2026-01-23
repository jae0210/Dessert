using UnityEngine;
using Photon.Pun;

public class PunHandPoseSync : MonoBehaviourPun, IPunObservable
{
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    // 리모트 보간(덜 끊겨보이게)
    public float posLerp = 20f;
    public float rotLerp = 20f;

    Vector3 leftPosNet, rightPosNet;
    Quaternion leftRotNet, rightRotNet;

    void Update()
    {
        if (photonView.IsMine) return;

        // 리모트만: 받은 값으로 부드럽게 적용
        leftHandTarget.localPosition = Vector3.Lerp(leftHandTarget.localPosition, leftPosNet, Time.deltaTime * posLerp);
        leftHandTarget.localRotation = Quaternion.Slerp(leftHandTarget.localRotation, leftRotNet, Time.deltaTime * rotLerp);

        rightHandTarget.localPosition = Vector3.Lerp(rightHandTarget.localPosition, rightPosNet, Time.deltaTime * posLerp);
        rightHandTarget.localRotation = Quaternion.Slerp(rightHandTarget.localRotation, rightRotNet, Time.deltaTime * rotLerp);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(leftHandTarget.localPosition);
            stream.SendNext(leftHandTarget.localRotation);
            stream.SendNext(rightHandTarget.localPosition);
            stream.SendNext(rightHandTarget.localRotation);
        }
        else
        {
            leftPosNet = (Vector3)stream.ReceiveNext();
            leftRotNet = (Quaternion)stream.ReceiveNext();
            rightPosNet = (Vector3)stream.ReceiveNext();
            rightRotNet = (Quaternion)stream.ReceiveNext();
        }
    }
}
