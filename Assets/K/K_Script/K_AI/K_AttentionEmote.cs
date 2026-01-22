using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class K_AttentionEmote : MonoBehaviourPun
{
    [Header("UI Settings")]
    [Tooltip("머리 위에 띄울 캔버스 오브젝트")]
    public GameObject attentionCanvasObj;
    [Tooltip("이미지를 표시할 UI Image 컴포넌트")]
    public Image displayImage;
    [Tooltip("표시할 스프라이트 (0: 느낌표, 1: 물음표 등)")]
    public Sprite[] emotionSprites;

    [Header("Sound")]
    public AudioSource audioSource;      // 3D AudioSource 권장
    public AudioClip[] emoteSounds;       // 0: 느낌표, 1: 물음표 등

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
        // 내 캐릭터만 입력 처리
        if (!photonView.IsMine) return;

        // ▶ 오른손 A 버튼 → 느낌표
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            photonView.RPC("RpcShowAttention", RpcTarget.All, 0);

            // 진동 (로컬만)
            OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.RTouch);
            StartCoroutine(StopVibration(OVRInput.Controller.RTouch));
        }

        // ▶ 왼손 X 버튼 → 물음표
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            if (emotionSprites.Length > 1)
                photonView.RPC("RpcShowAttention", RpcTarget.All, 1);

            // 진동 (로컬만)
            OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.LTouch);
            StartCoroutine(StopVibration(OVRInput.Controller.LTouch));
        }
    }

    // ▶ 0.1초 후 진동 OFF
    IEnumerator StopVibration(OVRInput.Controller controller)
    {
        yield return new WaitForSeconds(0.1f);
        OVRInput.SetControllerVibration(0, 0, controller);
    }

    // ▶ 모든 클라이언트에서 실행됨
    [PunRPC]
    public void RpcShowAttention(int spriteIndex)
    {
        if (attentionCanvasObj == null) return;

        // 이미지 변경
        if (spriteIndex >= 0 && spriteIndex < emotionSprites.Length)
        {
            displayImage.sprite = emotionSprites[spriteIndex];
        }

        // 🔊 사운드 재생 (모든 클라이언트)
        if (audioSource != null && emoteSounds != null && spriteIndex < emoteSounds.Length)
        {
            audioSource.PlayOneShot(emoteSounds[spriteIndex]);
        }

        // 애니메이션 코루틴 재시작
        StopAllCoroutines();
        StartCoroutine(PopUpEffect());
    }

    IEnumerator PopUpEffect()
    {
        attentionCanvasObj.SetActive(true);
        attentionCanvasObj.transform.localScale = Vector3.zero;

        float timer = 0f;
        float popTime = 0.2f;

        // 1. 팝업
        while (timer < popTime)
        {
            timer += Time.deltaTime;
            float t = timer / popTime;
            attentionCanvasObj.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, popScale, t);
            yield return null;
        }

        // 2. 원래 크기
        attentionCanvasObj.transform.localScale = originalScale;

        // 3. 유지
        yield return new WaitForSeconds(displayDuration);

        // 4. 숨김
        attentionCanvasObj.SetActive(false);
    }
}
