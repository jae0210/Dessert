using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;

public class J_PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private string prefabName = "J_NetCapsule";

    [Header("내 VR 리그의 루트(OVRCameraRig 또는 XR Origin) Transform 드래그")]
    [SerializeField] private Transform localRigRoot;

    // CustomizeScene에서 저장한 키와 동일해야 함
    private const string KEY_BODY = "bodyCol";
    private const string KEY_HAT = "hatCol";

    public override void OnJoinedRoom()
    {
        if (localRigRoot == null)
        {
            Debug.LogError("[Spawner] localRigRoot가 비어있음! 씬에서 OVRCameraRig/XR Origin을 드래그해줘.");
            return;
        }

        Vector3 spawnPos = GetSpawnPos(localRigRoot.position);

        // ✅ CustomizeScene에서 저장해둔 색을 읽기(없으면 흰색)
        int bodyPacked = PlayerPrefs.GetInt(KEY_BODY, J_ColorPack.Pack(Color.white));
        int hatPacked = PlayerPrefs.GetInt(KEY_HAT, J_ColorPack.Pack(Color.white));

        // ✅ InstantiateData로 같이 전달
        object[] data = { bodyPacked, hatPacked };

        GameObject go = PhotonNetwork.Instantiate(prefabName, spawnPos, Quaternion.identity, 0, data);

        // (선택) CustomProperties에도 저장해두면 재접속/디버깅에 도움
        var props = new Hashtable { { KEY_BODY, bodyPacked }, { KEY_HAT, hatPacked } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 내 오브젝트면: 캡슐이 내 VR 리그를 따라가도록 연결(기존 로직 유지)
        var avatar = go.GetComponent<J_NetCapsuleAvatar>();
        if (avatar != null && avatar.photonView.IsMine)
        {
            avatar.BindLocalRig(localRigRoot);
        }
    }

    private Vector3 GetSpawnPos(Vector3 basePos)
    {
        float xOffset = (PhotonNetwork.LocalPlayer.ActorNumber % 4) * 1.5f;
        return basePos + new Vector3(xOffset, 0f, 0f);
    }
}
