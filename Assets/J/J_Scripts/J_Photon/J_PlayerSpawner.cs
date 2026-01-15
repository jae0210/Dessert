using Photon.Pun;
using UnityEngine;

public class J_PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private string prefabName = "J_NetCapsule";

    [Header("내 VR 리그의 루트(OVRCameraRig 또는 XR Origin) Transform 드래그")]
    [SerializeField] private Transform localRigRoot;

    public override void OnJoinedRoom()
    {
        if (localRigRoot == null)
        {
            Debug.LogError("[Spawner] localRigRoot가 비어있음! 씬에서 OVRCameraRig/XR Origin을 드래그해줘.");
            return;
        }

        Vector3 spawnPos = GetSpawnPos(localRigRoot.position);

        GameObject go = PhotonNetwork.Instantiate(prefabName, spawnPos, Quaternion.identity);

        // 내 오브젝트면: 캡슐이 내 VR 리그를 따라가도록 연결
        var avatar = go.GetComponent<J_NetCapsuleAvatar>();
        if (avatar != null && avatar.photonView.IsMine)
        {
            avatar.BindLocalRig(localRigRoot);
        }
    }

    private Vector3 GetSpawnPos(Vector3 basePos)
    {
        // 겹침 방지용으로 살짝씩 옆으로
        float xOffset = (PhotonNetwork.LocalPlayer.ActorNumber % 4) * 1.5f;
        return basePos + new Vector3(xOffset, 0f, 0f);
    }
}
