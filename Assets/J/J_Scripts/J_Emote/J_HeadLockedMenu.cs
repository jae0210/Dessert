using Photon.Pun;
using UnityEngine;

public class J_HeadLockedMenu : MonoBehaviourPun
{
    [Header("Head Locked")]
    public Vector3 localOffset = new Vector3(0f, -0.05f, 0.6f); // 눈앞 0.6m, 살짝 아래
    public bool lockRoll = true;

    [Header("Emote SFX")]
    public AudioClip emoteSfx;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    public bool playSfxWhenShown = true;
    public bool playSfxWhenSpriteChanged = true;

    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    private bool prevEnabled;
    private Sprite prevSprite;

    void Start()
    {
        // ★ 내 것만 보이게 (중요)
        var pv = GetComponentInParent<PhotonView>();
        if (pv != null && !pv.IsMine)
        {
            gameObject.SetActive(false);
            return;
        }

        // AudioSource 준비(없으면 자동 추가)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D(UI)처럼 들리게

        // SpriteRenderer 캐시 (자식에 있을 수도 있으니 InChildren)
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        if (spriteRenderer != null)
        {
            prevEnabled = spriteRenderer.enabled;
            prevSprite = spriteRenderer.sprite;
        }

        var rig = FindObjectOfType<OVRCameraRig>();
        if (rig == null) return;

        // CenterEyeAnchor 밑으로 붙이기
        transform.SetParent(rig.centerEyeAnchor, false);
        transform.localPosition = localOffset;
        transform.localRotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        // 롤만 제거(머리 기울임으로 UI가 기울어지는 거 방지)
        if (lockRoll)
        {
            var e = transform.localEulerAngles;
            transform.localRotation = Quaternion.Euler(e.x, e.y, 0f);
        }

        // 이모티콘 "표시/변경" 감지해서 SFX 재생
        if (spriteRenderer == null || emoteSfx == null || audioSource == null) return;

        bool nowEnabled = spriteRenderer.enabled;
        Sprite nowSprite = spriteRenderer.sprite;

        // 1) 꺼져있다가 켜질 때(이모티콘 뜨는 순간)
        if (playSfxWhenShown && !prevEnabled && nowEnabled)
        {
            audioSource.PlayOneShot(emoteSfx, sfxVolume);
        }
        // 2) 이미 켜진 상태에서 sprite가 바뀔 때(연속 이모티콘)
        else if (playSfxWhenSpriteChanged && nowEnabled && nowSprite != prevSprite)
        {
            audioSource.PlayOneShot(emoteSfx, sfxVolume);
        }

        prevEnabled = nowEnabled;
        prevSprite = nowSprite;
    }
}
