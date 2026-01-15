using Photon.Pun;
using UnityEngine;

public class J_NetCapsuleAvatar : MonoBehaviourPun, IPunObservable
{
    [Header("내 것일 때만 사용: 이 Transform을 따라감")]
    [SerializeField] private Transform followTarget;

    [Header("내 것일 때 숨길 렌더러(캡슐 MeshRenderer 등)")]
    [SerializeField] private Renderer[] hideWhenMine;

    private Vector3 netPos;
    private Quaternion netRot;
    private PhotonTransformViewClassic _photonTransformView;
    private CapsuleCollider _collider;

    void Awake()
    {
        // PhotonTransformView를 즉시 비활성화 (로컬 플레이어의 경우)
        _photonTransformView = GetComponent<PhotonTransformViewClassic>();
        _collider = GetComponent<CapsuleCollider>();
        
        if (photonView != null && photonView.IsMine)
        {
            if (_photonTransformView != null)
            {
                _photonTransformView.enabled = false; // 즉시 비활성화
            }
            
            // 로컬 플레이어의 콜라이더는 IsTrigger로 설정 (충돌 방지)
            if (_collider != null)
            {
                _collider.isTrigger = true;
            }
        }
    }

    public void BindLocalRig(Transform rigRoot)
    {
        followTarget = rigRoot;

        // 나는 컨트롤러만 보이게: 내 캡슐은 숨김
        if (hideWhenMine != null)
        {
            foreach (var r in hideWhenMine)
                if (r) r.enabled = false;
        }

        // Awake에서 이미 처리했지만, 확실히 하기 위해 다시 확인
        if (_photonTransformView != null && photonView.IsMine)
        {
            _photonTransformView.enabled = false;
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (followTarget == null) return;

            // 캡슐이 내 VR 리그를 따라감
            transform.position = followTarget.position;

            // 회전은 Yaw만(원하면) - 필요 없으면 아래 줄 지워도 됨
            Vector3 e = followTarget.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, e.y, 0f);
        }
        else
        {
            // 리모트는 부드럽게 보정
            transform.position = Vector3.Lerp(transform.position, netPos, Time.deltaTime * 12f);
            transform.rotation = Quaternion.Slerp(transform.rotation, netRot, Time.deltaTime * 12f);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            netPos = (Vector3)stream.ReceiveNext();
            netRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
