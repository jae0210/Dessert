using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class J_RandomBodyColor_PUN : MonoBehaviourPun
{
    [Header("Body 찾기")]
    [SerializeField] private string bodyObjectName = "Body";
    [SerializeField] private Renderer bodyRenderer;

    [Header("어느 머티리얼에 색을 입힐까?")]
    [SerializeField] private string targetMaterialNameContains = "bobusang_body";
    [SerializeField] private int fallbackMaterialIndex = 1;

    [Header("Debug Log")]
    [SerializeField] private bool debugLog = true;

    private int targetMatIndex = -1;

    private IEnumerator Start()
    {
        yield return null;

        CacheBodyRenderer();
        CacheTargetMaterialIndex();

        if (debugLog && bodyRenderer != null)
        {
            var sm = bodyRenderer.sharedMaterials;
            Debug.Log($"[BodyColor] Start | mats={sm.Length} | targetMatIndex={targetMatIndex} | InRoom={PhotonNetwork.InRoom} | IsMine={photonView.IsMine}", this);
            for (int i = 0; i < sm.Length; i++)
                Debug.Log($"[BodyColor] sharedMat[{i}]={(sm[i] ? sm[i].name : "NULL")}", this);
        }

        if (photonView.IsMine)
        {
            Color c = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.6f, 1f);
            photonView.RPC(nameof(RPC_SetBodyColor), RpcTarget.AllBuffered, c.r, c.g, c.b, c.a);
        }
    }

    private void CacheBodyRenderer()
    {
        if (bodyRenderer != null) return;

        Transform t = transform.Find(bodyObjectName);
        if (t == null)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
                if (child.name == bodyObjectName) { t = child; break; }
        }

        if (t == null)
        {
            Debug.LogWarning($"[BodyColor] '{bodyObjectName}' Transform NOT FOUND", this);
            return;
        }

        bodyRenderer = t.GetComponentInChildren<Renderer>(true);
    }

    private void CacheTargetMaterialIndex()
    {
        targetMatIndex = -1;
        if (bodyRenderer == null) return;

        var mats = bodyRenderer.sharedMaterials;
        if (mats == null || mats.Length == 0) return;

        for (int i = 0; i < mats.Length; i++)
        {
            var m = mats[i];
            if (m == null) continue;

            if (!string.IsNullOrEmpty(targetMaterialNameContains) && m.name.Contains(targetMaterialNameContains))
            {
                targetMatIndex = i;
                break;
            }
        }

        if (targetMatIndex < 0)
        {
            if (fallbackMaterialIndex >= 0 && fallbackMaterialIndex < mats.Length)
                targetMatIndex = fallbackMaterialIndex;
            else
                targetMatIndex = 0;
        }
    }

    [PunRPC]
    private void RPC_SetBodyColor(float r, float g, float b, float a, PhotonMessageInfo info)
    {
        if (bodyRenderer == null) CacheBodyRenderer();
        if (bodyRenderer == null) return;
        if (targetMatIndex < 0) CacheTargetMaterialIndex();

        Color c = new Color(r, g, b, a);

        // ✅ 이 방식이 오버레이(서브메시 1개/머티리얼 2개)에서도 가장 확실함
        var mats = bodyRenderer.materials; // 인스턴스 생성(플레이어별)
        if (mats == null || mats.Length == 0) return;
        if (targetMatIndex < 0 || targetMatIndex >= mats.Length) return;

        var m = mats[targetMatIndex];
        if (m != null)
        {
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);         // Standard
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c); // URP 대비
        }

        if (debugLog)
            Debug.Log($"[BodyColor] Applied to matIndex={targetMatIndex} | fromActor={info.Sender?.ActorNumber} | color={c}", this);
    }
}
