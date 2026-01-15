using Photon.Pun;
using UnityEngine;

/// <summary>
/// 버튼/동영상 등 UI를 본인에게만 보이게 하는 컴포넌트.
/// Room_Photon.unity의 버튼/동영상 오브젝트에 붙이면 됨.
/// </summary>
public class J_LocalOnlyUI : MonoBehaviourPun
{
    [Header("이 오브젝트와 자식들을 본인에게만 보이게")]
    [SerializeField] private bool hideForOthers = true;

    void Start()
    {
        // PhotonView가 없으면 항상 보임 (로컬 전용)
        if (photonView == null)
        {
            return;
        }

        // 본인이 아니면 숨김
        if (!photonView.IsMine && hideForOthers)
        {
            gameObject.SetActive(false);
        }
    }
}

