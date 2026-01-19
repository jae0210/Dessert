using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class J_RandomHatColor_PUN : MonoBehaviourPun
{
    [Header("Hat 찾기")]
    [SerializeField] private string hatObjectName = "Hat";
    [SerializeField] private Renderer hatRenderer;

    [Header("Debug Log")]
    [SerializeField] private bool debugLog = true;

    private MaterialPropertyBlock mpb;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();

        // PhotonView 존재 여부 체크
        if (debugLog)
        {
            var pv = GetComponent<PhotonView>();
            Debug.Log($"[HatColor] Awake | PhotonView exists? {(pv != null)} | GO={gameObject.name}", this);
        }
    }

    private IEnumerator Start()
    {
        // 하위 오브젝트 준비 한 프레임 대기
        yield return null;

        CacheHatRenderer();

        if (debugLog)
        {
            string inRoom = PhotonNetwork.InRoom.ToString();
            string isMine = (photonView != null) ? photonView.IsMine.ToString() : "PhotonView NULL";
            string viewId = (photonView != null) ? photonView.ViewID.ToString() : "-1";
            string ownerNr = (photonView != null) ? photonView.OwnerActorNr.ToString() : "-1";
            string ownerNick = (photonView != null && photonView.Owner != null) ? photonView.Owner.NickName : "NULL";

            Debug.Log($"[HatColor] Start | InRoom={inRoom} | IsMine={isMine} | ViewID={viewId} | OwnerActorNr={ownerNr} | OwnerNick={ownerNick} | GO={gameObject.name}", this);

            if (hatRenderer == null)
            {
                Debug.LogWarning("[HatColor] Start | hatRenderer = NULL (Hat 이름/구조 확인 필요)", this);
            }
            else
            {
                var mat = hatRenderer.sharedMaterial;
                string matName = mat ? mat.name : "NULL";
                string shaderName = (mat && mat.shader) ? mat.shader.name : "NULL";
                bool hasColor = mat && mat.HasProperty("_Color");
                bool hasBaseColor = mat && mat.HasProperty("_BaseColor");

                Debug.Log($"[HatColor] HatRenderer found | type={hatRenderer.GetType().Name} | name={hatRenderer.name} | mat={matName} | shader={shaderName} | Has _Color={hasColor} | Has _BaseColor={hasBaseColor}", this);
            }
        }

        // 내 캐릭터만 랜덤 생성 -> RPC 전파
        if (photonView != null && photonView.IsMine)
        {
            Color c = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.6f, 1f);

            if (debugLog)
                Debug.Log($"[HatColor] Sending RPC(AllBuffered) color={c} | ViewID={photonView.ViewID}", this);

            photonView.RPC(nameof(RPC_SetHatColor), RpcTarget.AllBuffered, c.r, c.g, c.b, c.a);
        }
        else
        {
            if (debugLog)
                Debug.Log($"[HatColor] Not sending RPC | reason: photonView null or IsMine=false | ViewID={(photonView != null ? photonView.ViewID : -1)}", this);
        }
    }

    private void CacheHatRenderer()
    {
        if (hatRenderer != null)
        {
            if (debugLog) Debug.Log("[HatColor] CacheHatRenderer | hatRenderer already assigned in Inspector", this);
            return;
        }

        Transform t = transform.Find(hatObjectName);

        if (t == null)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name == hatObjectName)
                {
                    t = child;
                    break;
                }
            }
        }

        if (t == null)
        {
            if (debugLog) Debug.LogWarning($"[HatColor] CacheHatRenderer | '{hatObjectName}' Transform NOT FOUND under {gameObject.name}", this);
            return;
        }

        hatRenderer = t.GetComponentInChildren<Renderer>(true);

        if (debugLog)
            Debug.Log($"[HatColor] CacheHatRenderer | Found Hat Transform='{t.name}' | Renderer={(hatRenderer != null ? hatRenderer.GetType().Name : "NULL")}", this);
    }

    [PunRPC]
    private void RPC_SetHatColor(float r, float g, float b, float a, PhotonMessageInfo info)
    {
        if (debugLog)
            Debug.Log($"[HatColor] RPC_SetHatColor RECEIVED | fromActor={info.Sender?.ActorNumber} nick={info.Sender?.NickName} | color=({r:F2},{g:F2},{b:F2},{a:F2}) | localViewID={(photonView != null ? photonView.ViewID : -1)}", this);

        if (hatRenderer == null) CacheHatRenderer();
        if (hatRenderer == null)
        {
            if (debugLog) Debug.LogWarning("[HatColor] RPC_SetHatColor | hatRenderer is still NULL -> cannot apply", this);
            return;
        }

        Color c = new Color(r, g, b, a);

        // MaterialPropertyBlock 적용
        hatRenderer.GetPropertyBlock(mpb);
        mpb.SetColor("_BaseColor", c);
        mpb.SetColor("_Color", c);
        hatRenderer.SetPropertyBlock(mpb);

        if (debugLog)
            Debug.Log($"[HatColor] Applied color via MPB | renderer={hatRenderer.name} | color={c}", this);
    }
}
