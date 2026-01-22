using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class K_AttentionEmote : MonoBehaviourPun
{
    [Header("UI Settings")]
    [Tooltip("머리 위에 띄울 캔버스")]
    public GameObject attentionCanvasObj;
    [Tooltip("표시할 레거시 UI Text")]
    public Text emoteText;

    [Header("Emote Messages")]
    public string[] emoteMessages; // 0: "!", 1: "?", 2: "도와주세요!" 등

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip[] emoteSounds;

    [Header("Animation")]
    public float displayDuration = 2.0f;
    public Vector3 popScale = new Vector3(1.2f, 1.2f, 1.0f);
    private Vector3 originalScale;

    private void Start()
    {
        if (attentionCanvasObj != null)
        {
            originalScale = attentionCanvasObj.transform.localScale;
            attentionCanvasObj.SetActive(false);
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        // ▶ 오른손 버튼 → "!"
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            photonView.RPC("RpcShowEmote", RpcTarget.All, 0);

            // 진동 (로컬)
            OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.RTouch);
            StartCoroutine(StopVibration(OVRInput.Controller.RTouch));
        }

        // ▶ 왼손 버튼 → "?"
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            if (emoteMessages.Length > 1)
                photonView.RPC("RpcShowEmote", RpcTarget.All, 1);

            // 진동 (로컬)
            OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.LTouch);
            StartCoroutine(StopVibration(OVRInput.Controller.LTouch));
        }
    }

    IEnumerator StopVibration(OVRInput.Controller controller)
    {
        yield return new WaitForSeconds(0.1f);
        OVRInput.SetControllerVibration(0, 0, controller);
    }

    // ▶ 모든 클라이언트에서 실행됨
    [PunRPC]
    public void RpcShowEmote(int index)
    {
        if (attentionCanvasObj == null || emoteText == null) return;

        // 텍스트 설정
        if (index >= 0 && index < emoteMessages.Length)
        {
            emoteText.text = emoteMessages[index];
        }

        // 🔊 사운드 재생
        if (audioSource != null && emoteSounds != null && index < emoteSounds.Length)
        {
            audioSource.PlayOneShot(emoteSounds[index]);
        }

        StopAllCoroutines();
        StartCoroutine(PopUpEffect());
    }

    IEnumerator PopUpEffect()
    {
        attentionCanvasObj.SetActive(true);
        attentionCanvasObj.transform.localScale = Vector3.zero;

        float timer = 0f;
        float popTime = 0.2f;

        // 팝업 애니메이션
        while (timer < popTime)
        {
            timer += Time.deltaTime;
            float t = timer / popTime;
            attentionCanvasObj.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, popScale, t);
            yield return null;
        }

        attentionCanvasObj.transform.localScale = originalScale;

        yield return new WaitForSeconds(displayDuration);

        attentionCanvasObj.SetActive(false);
    }
}
