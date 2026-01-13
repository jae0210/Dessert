using UnityEngine;

public class GlobalSoundManager : MonoBehaviour
{
    public static GlobalSoundManager Instance; // 싱글톤 패턴

    [Header("Audio Components")]
    public AudioSource bgmSource;
    public AudioClip titleBGM;

    private void Awake()
    {
        // 1. 싱글톤 설정: 이미 있으면 나는 파괴, 없으면 나를 보존
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 핵심: 씬이 넘어가도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }
    }

    private void Start()
    {
        // BGM 재생 (이미 재생 중이 아니라면)
        if (!bgmSource.isPlaying)
        {
            bgmSource.clip = titleBGM;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    // 필요 시 BGM을 바꾸는 함수
    public void ChangeBGM(AudioClip newClip)
    {
        if (bgmSource.clip == newClip) return; // 같은 곡이면 무시

        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.Play();
    }
}