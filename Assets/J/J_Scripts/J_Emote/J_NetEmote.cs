using System.Collections;
using Photon.Pun;
using UnityEngine;

public class J_NetEmote : MonoBehaviourPun
{
    [Header("Renderer & Assets")]
    [SerializeField] private SpriteRenderer emoteRenderer;
    [SerializeField] private Sprite[] emotes;

    [Header("Options")]
    [SerializeField] private float showSeconds = 2f;
    [SerializeField] private bool showOnSelf = false; // 나는 보일지 여부

    Coroutine running;

    public Sprite[] EmoteSprites => emotes;

    // 로컬 입력에서 호출
    public void RequestEmote(int emoteIndex)
    {
        if (!photonView.IsMine) return;

        if (showOnSelf) PlayEmoteLocal(emoteIndex);

        // "내가 눌렀다"는 사실(결과)만 다른 사람에게 보냄
        photonView.RPC(nameof(RPC_PlayEmote), RpcTarget.Others, emoteIndex);
    }

    [PunRPC]
    void RPC_PlayEmote(int emoteIndex)
    {
        PlayEmoteLocal(emoteIndex);
    }

    void PlayEmoteLocal(int emoteIndex)
    {
        if (emoteRenderer == null) return;
        if (emotes == null || emotes.Length == 0) return;
        if (emoteIndex < 0 || emoteIndex >= emotes.Length) return;

        emoteRenderer.sprite = emotes[emoteIndex];
        emoteRenderer.enabled = true;

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(HideAfter());
    }

    IEnumerator HideAfter()
    {
        yield return new WaitForSeconds(showSeconds);
        emoteRenderer.enabled = false;
        running = null;
    }
}
