using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class J_RandomBodyColor_PUN : MonoBehaviourPun
{
    [Header("Body 찾기")]
    [SerializeField] private string BodyObjectName = "Body";
    [SerializeField] private Renderer BodyRenderer;

    [Header("Debug Log")]
    [SerializeField] private bool debugLog = true;

    private IEnumerator Start()
    {
        yield return null;

        CacheBodyRenderer();

        if (debugLog)
        {
            Debug.Log($"[BodyColor] Start | InRoom={PhotonNetwork.InRoom} | IsMine={photonView.IsMine} | ViewID={photonView.ViewID}", this);

            if (BodyRenderer != null && BodyRenderer.sharedMaterial != null)
            {
                bool hasColor = BodyRenderer.sharedMaterial.HasProperty("_Color"); // ✅ 백슬래시 없음
                Debug.Log($"[BodyColor] Mat={BodyRenderer.sharedMaterial.name} | Shader={BodyRenderer.sharedMaterial.shader.name} | Has _Color={hasColor}", this);
            }
        }

        // 내 캐릭터만 랜덤 생성 -> RPC 전파
        if (photonView.IsMine)
        {
            Color c = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.6f, 1f);
            photonView.RPC(nameof(RPC_SetBodyColor), RpcTarget.AllBuffered, c.r, c.g, c.b, c.a);
        }
    }

    private void CacheBodyRenderer()
    {
        if (BodyRenderer != null) return;

        Transform t = transform.Find(BodyObjectName);

        if (t == null)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == BodyObjectName) { t = child; break; }
            }
        }

        if (t == null)
        {
            Debug.LogWarning($"[BodyColor] '{BodyObjectName}' Transform NOT FOUND", this);
            return;
        }

        BodyRenderer = t.GetComponentInChildren<Renderer>(true);
    }

    [PunRPC]
    private void RPC_SetBodyColor(float r, float g, float b, float a, PhotonMessageInfo info)
    {
        if (BodyRenderer == null) CacheBodyRenderer();
        if (BodyRenderer == null) return;

        Color c = new Color(r, g, b, a);

        // ✅ 인스펙터 Albedo 옆 색상칸 = _Color 변경(플레이어별 인스턴스)
        var mats = BodyRenderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] == null) continue;
            if (mats[i].HasProperty("_Color")) mats[i].SetColor("_Color", c);
        }

        if (debugLog)
            Debug.Log($"[BodyColor] Applied _Color tint | fromActor={info.Sender?.ActorNumber} | color={c}", this);
    }
}
