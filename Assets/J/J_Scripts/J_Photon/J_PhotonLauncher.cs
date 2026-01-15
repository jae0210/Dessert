using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class J_PhotonLauncher : MonoBehaviourPunCallbacks
{
    [Header("같은 값이면 같은 방으로 모임")]
    [SerializeField] private string roomName = "J_TestRoom";
    [SerializeField] private byte maxPlayers = 10;

    void Start()
    {
        // 두 사람이 '서로 다른 방' 들어가는 걸 막으려면 고정 룸명이 가장 안전함
        PhotonNetwork.GameVersion = "0.1";   // 빌드/에디터 둘 다 같게
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom(
            roomName,
            new RoomOptions { MaxPlayers = maxPlayers },
            TypedLobby.Default
        );
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[PUN] JoinedRoom: {PhotonNetwork.CurrentRoom.Name} Players:{PhotonNetwork.CurrentRoom.PlayerCount}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"[PUN] Disconnected: {cause}");
    }
}
