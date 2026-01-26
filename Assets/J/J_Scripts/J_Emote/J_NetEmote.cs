using System.Collections;
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

    private Coroutine headRoutine;
    private Coroutine frontRoutine;

    // 토큰(시퀀스)으로 최신 재생만 유효하게 만들기
    private int headSeq = 0;
    private int frontSeq = 0;

    public Sprite[] EmoteSprites => emotes;

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

    private void PlayOnHead(int emoteIndex)
    {
        if (!IsValidIndex(emoteIndex) || headEmoteRenderer == null) return;

        headSeq++;
        int seq = headSeq;

        headEmoteRenderer.sprite = emotes[emoteIndex];
        headEmoteRenderer.enabled = true;

        if (headRoutine != null) StopCoroutine(headRoutine);
        headRoutine = StartCoroutine(HideAfter(headEmoteRenderer, showSeconds, seq, isHead: true));
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

        frontSeq++;
        int seq = frontSeq;

        frontEmoteRenderer.sprite = emotes[emoteIndex];
        frontEmoteRenderer.enabled = true;

        if (frontRoutine != null) StopCoroutine(frontRoutine);
        frontRoutine = StartCoroutine(HideAfter(frontEmoteRenderer, showSeconds, seq, isHead: false));
    }

    private IEnumerator HideAfter(SpriteRenderer r, float seconds, int seq, bool isHead)
    {
        yield return new WaitForSeconds(seconds);

        // 최신 요청이 아닐 경우(중간에 다른 이모티콘이 재생된 경우) 무시
        if (isHead)
        {
            if (seq != headSeq) yield break;
            headRoutine = null;
        }
        else
        {
            if (seq != frontSeq) yield break;
            frontRoutine = null;
        }

        if (r != null) r.enabled = false;
    }

    private bool IsValidIndex(int index)
    {
        return emotes != null && emotes.Length > 0 && index >= 0 && index < emotes.Length;
    }
}
