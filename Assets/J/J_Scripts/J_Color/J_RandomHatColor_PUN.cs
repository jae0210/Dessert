using System.Collections;
using UnityEngine;
using Photon.Pun;

public class J_RandomHatColor_PUN : MonoBehaviourPun
{
    [Header("Hat 찾기")]
    [SerializeField] private string hatObjectName = "Hat";
    [SerializeField] private Renderer hatRenderer;

    [Header("옵션: 테스트로 키 누르면 다시 랜덤(원하면 켜기)")]
    [SerializeField] private bool rerollWithKey = false;
    [SerializeField] private KeyCode rerollKey = KeyCode.LeftAlt;

    private MaterialPropertyBlock mpb;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
    }

    private IEnumerator Start()
    {
        // 네트워크 Instantiate 직후엔 하위 오브젝트 준비가 한 프레임 늦을 수 있어서 안전하게 한 번 대기
        yield return null;

        CacheHatRenderer();

        // 내 캐릭터만 랜덤 색을 정해서 전체에게 전파
        if (photonView.IsMine)
        {
            Color c = Random.ColorHSV(
                0f, 1f,      // Hue
                0.6f, 1f,    // Saturation
                0.6f, 1f     // Value
            );

            // AllBuffered: 늦게 들어온 플레이어도 버퍼된 RPC로 같은 색을 보게 됨
            photonView.RPC(nameof(RPC_SetHatColor), RpcTarget.AllBuffered, c.r, c.g, c.b, c.a);
        }
    }

    private void Update()
    {
        if (!rerollWithKey) return;

        if (photonView.IsMine && Input.GetKeyDown(rerollKey))
        {
            Color c = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.6f, 1f);
            photonView.RPC(nameof(RPC_SetHatColor), RpcTarget.AllBuffered, c.r, c.g, c.b, c.a);
        }
    }

    private void CacheHatRenderer()
    {
        if (hatRenderer != null) return;

        // 1) 직계 자식에 Hat이 있는 경우 빠르게 찾기
        Transform t = transform.Find(hatObjectName);

        // 2) 더 깊은 곳에 있으면 전체 자식에서 이름으로 찾기
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

        if (t != null)
            hatRenderer = t.GetComponentInChildren<Renderer>(true);
    }

    [PunRPC]
    private void RPC_SetHatColor(float r, float g, float b, float a)
    {
        if (hatRenderer == null) CacheHatRenderer();
        if (hatRenderer == null) return;

        Color c = new Color(r, g, b, a);

        // MaterialPropertyBlock 사용: 머티리얼 복제 없이 인스턴스별 색 적용(권장)
        hatRenderer.GetPropertyBlock(mpb);
        mpb.SetColor("_BaseColor", c); // URP Lit
        mpb.SetColor("_Color", c);     // Built-in/기타
        hatRenderer.SetPropertyBlock(mpb);
    }
}
