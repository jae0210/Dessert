using Photon.Pun;
using UnityEngine;

/// <summary>
/// VR 리그(머리/손) 위치를 네트워크로 동기화.
/// - 내 것: OVRCameraRig에서 읽어서 전송
/// - 상대 것: 네트워크에서 받아서 표시
/// </summary>
public class J_NetRigSync : MonoBehaviourPun, IPunObservable
{
    [Header("VR 리그 타겟 (내 것일 때만 사용)")]
    public Transform headTarget;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    [Header("리모트 보간")]
    public float posLerp = 15f;
    public float rotLerp = 15f;

    // 내 VR 리그 소스
    private Transform srcHead, srcL, srcR;

    // 네트워크 데이터
    private Vector3 netHeadPos, netLPos, netRPos;
    private Quaternion netHeadRot, netLRot, netRRot;

    void Awake()
    {
        if (photonView.IsMine)
        {
            // OVRCameraRig 찾기
            var rig = FindObjectOfType<OVRCameraRig>();
            if (rig != null)
            {
                srcHead = rig.centerEyeAnchor;
                srcL = rig.leftHandAnchor;
                srcR = rig.rightHandAnchor;
            }

            // 타겟이 없으면 자동 생성
            if (headTarget == null)
            {
                var go = new GameObject("HeadTarget");
                go.transform.SetParent(transform);
                headTarget = go.transform;
            }
            if (leftHandTarget == null)
            {
                var go = new GameObject("LeftHandTarget");
                go.transform.SetParent(transform);
                leftHandTarget = go.transform;
            }
            if (rightHandTarget == null)
            {
                var go = new GameObject("RightHandTarget");
                go.transform.SetParent(transform);
                rightHandTarget = go.transform;
            }
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (srcHead == null || srcL == null || srcR == null) return;

            // 내 VR 리그 위치를 타겟에 복사
            headTarget.SetPositionAndRotation(srcHead.position, srcHead.rotation);
            leftHandTarget.SetPositionAndRotation(srcL.position, srcL.rotation);
            rightHandTarget.SetPositionAndRotation(srcR.position, srcR.rotation);
        }
        else
        {
            // 리모트는 부드럽게 보간
            headTarget.position = Vector3.Lerp(headTarget.position, netHeadPos, Time.deltaTime * posLerp);
            headTarget.rotation = Quaternion.Slerp(headTarget.rotation, netHeadRot, Time.deltaTime * rotLerp);

            leftHandTarget.position = Vector3.Lerp(leftHandTarget.position, netLPos, Time.deltaTime * posLerp);
            leftHandTarget.rotation = Quaternion.Slerp(leftHandTarget.rotation, netLRot, Time.deltaTime * rotLerp);

            rightHandTarget.position = Vector3.Lerp(rightHandTarget.position, netRPos, Time.deltaTime * posLerp);
            rightHandTarget.rotation = Quaternion.Slerp(rightHandTarget.rotation, netRRot, Time.deltaTime * rotLerp);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(headTarget.position);
            stream.SendNext(headTarget.rotation);
            stream.SendNext(leftHandTarget.position);
            stream.SendNext(leftHandTarget.rotation);
            stream.SendNext(rightHandTarget.position);
            stream.SendNext(rightHandTarget.rotation);
        }
        else
        {
            netHeadPos = (Vector3)stream.ReceiveNext();
            netHeadRot = (Quaternion)stream.ReceiveNext();
            netLPos = (Vector3)stream.ReceiveNext();
            netLRot = (Quaternion)stream.ReceiveNext();
            netRPos = (Vector3)stream.ReceiveNext();
            netRRot = (Quaternion)stream.ReceiveNext();
        }
    }
}

