using Photon.Pun;
using UnityEngine;

public class J_NetEmote : MonoBehaviourPun
{
    [Header("Renderers")]
    [SerializeField] private SpriteRenderer headEmoteRenderer;   // 상대가 보는 머리 위
    [SerializeField] private SpriteRenderer frontEmoteRenderer;  // 내가 보는 눈앞(로컬 전용)

    [Header("Assets")]
    [SerializeField] private Sprite[] emotes;

    [Header("Options")]
    [SerializeField] private float showSeconds = 2f;
    [SerializeField] private bool showSelfInFront = true;

    // unscaled 기준 “꺼질 시간”
    private float headHideAt = -1f;
    private float frontHideAt = -1f;

    // 로컬 입력에서 호출
    public void RequestEmote(int emoteIndex)
    {
        if (!photonView.IsMine) return;

        // 1) 나는 눈앞에
        if (showSelfInFront)
            PlayOnFront(emoteIndex);

        // 2) 다른 사람들은 내 머리 위로 보게
        photonView.RPC(nameof(RPC_PlayHeadEmote), RpcTarget.Others, emoteIndex);
    }

    [PunRPC]
    private void RPC_PlayHeadEmote(int emoteIndex)
    {
        PlayOnHead(emoteIndex);
    }

    private void Update()
    {
        float now = Time.unscaledTime;

        if (headEmoteRenderer != null && headEmoteRenderer.enabled && headHideAt > 0f && now >= headHideAt)
        {
            headEmoteRenderer.enabled = false;
            headHideAt = -1f;
        }

        if (frontEmoteRenderer != null && frontEmoteRenderer.enabled && frontHideAt > 0f && now >= frontHideAt)
        {
            frontEmoteRenderer.enabled = false;
            frontHideAt = -1f;
        }
    }

    private void PlayOnHead(int emoteIndex)
    {
        if (!IsValidIndex(emoteIndex) || headEmoteRenderer == null) return;

        headEmoteRenderer.sprite = emotes[emoteIndex];
        headEmoteRenderer.enabled = true;

        // showSeconds가 0 이하라도 “즉시 숨김”되게 처리
        headHideAt = Time.unscaledTime + Mathf.Max(0.01f, showSeconds);
    }

    private void PlayOnFront(int emoteIndex)
    {
        if (!IsValidIndex(emoteIndex)) return;

        // front가 없으면 fallback으로 head에 뜨게
        if (frontEmoteRenderer == null)
        {
            PlayOnHead(emoteIndex);
            return;
        }

        frontEmoteRenderer.sprite = emotes[emoteIndex];
        frontEmoteRenderer.enabled = true;

        frontHideAt = Time.unscaledTime + Mathf.Max(0.01f, showSeconds);
    }

    private void OnDisable()
    {
        // 혹시 비활성화되면서 “켜진 채로” 남는 걸 방지
        if (headEmoteRenderer != null) headEmoteRenderer.enabled = false;
        if (frontEmoteRenderer != null) frontEmoteRenderer.enabled = false;

        headHideAt = -1f;
        frontHideAt = -1f;
    }

    private bool IsValidIndex(int index)
    {
        return emotes != null && emotes.Length > 0 && index >= 0 && index < emotes.Length;
    }
}
