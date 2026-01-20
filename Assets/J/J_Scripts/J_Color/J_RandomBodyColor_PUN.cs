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
    [Tooltip("bobusang_body처럼 머티리얼 이름(또는 포함 문자열)로 찾음")]
    [SerializeField] private string targetMaterialNameContains = "bobusang_body";

    [Tooltip("못 찾으면 이 인덱스를 사용. (네 스샷 기준: 1)")]
    [SerializeField] private int fallbackMaterialIndex = 1;

    [Header("Debug Log")]
    [SerializeField] private bool debugLog = true;

    private int targetMatIndex = -1;
    private MaterialPropertyBlock mpb;

    private IEnumerator Start()
    {
        mpb = new MaterialPropertyBlock();
        yield return null;

        CacheBodyRenderer();
        CacheTargetMaterialIndex();

        if (debugLog)
        {
            Debug.Log($"[BodyColor] Start | InRoom={PhotonNetwork.InRoom} | IsMine={photonView.IsMine} | ViewID={photonView.ViewID}", this);
            if (bodyRenderer != null)
                Debug.Log($"[BodyColor] Renderer={bodyRenderer.name} | materials={bodyRenderer.sharedMaterials.Length} | targetMatIndex={targetMatIndex}", this);
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
        if (bodyRenderer != null) return;

        Transform t = transform.Find(bodyObjectName);
        if (t == null)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == bodyObjectName) { t = child; break; }
            }
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

        // 1) 이름으로 찾기
        for (int i = 0; i < mats.Length; i++)
        {
            var m = mats[i];
            if (m == null) continue;

            // Unity는 머티리얼 이름 뒤에 " (Instance)"가 붙을 수 있어서 포함 검사 추천
            if (!string.IsNullOrEmpty(targetMaterialNameContains) && m.name.Contains(targetMaterialNameContains))
            {
                targetMatIndex = i;
                break;
            }
        }

        // 2) 못 찾으면 fallback 인덱스
        if (targetMatIndex < 0)
        {
            if (fallbackMaterialIndex >= 0 && fallbackMaterialIndex < mats.Length)
                targetMatIndex = fallbackMaterialIndex;
            else
                targetMatIndex = 0; // 최후 fallback
        }
    }

    [PunRPC]
    private void RPC_SetBodyColor(float r, float g, float b, float a, PhotonMessageInfo info)
    {
        if (bodyRenderer == null) CacheBodyRenderer();
        if (bodyRenderer == null) return;

        if (targetMatIndex < 0) CacheTargetMaterialIndex();

        Color c = new Color(r, g, b, a);

        // ✅ 특정 머티리얼 슬롯(= bobusang_body)만 색 적용
        bodyRenderer.GetPropertyBlock(mpb, targetMatIndex);

        // Standard: _Color / URP Lit: _BaseColor 둘 다 넣어두면 안전
        mpb.SetColor("_Color", c);
        mpb.SetColor("_BaseColor", c);

        bodyRenderer.SetPropertyBlock(mpb, targetMatIndex);

        if (debugLog)
            Debug.Log($"[BodyColor] Applied color | fromActor={info.Sender?.ActorNumber} | matIndex={targetMatIndex} | color={c}", this);
    }
}
