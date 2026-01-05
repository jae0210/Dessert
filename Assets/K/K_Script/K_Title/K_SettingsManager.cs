using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class K_SettingsManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer masterMixer;

    [Header("Mixer Parameters")]
    [SerializeField] private string masterParam = "MasterVolume";
    [SerializeField] private string bgmParam = "BGMVolume";
    [SerializeField] private string sfxParam = "SFXVolume";

    [Header("UI Components")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Image muteButtonImage; // (선택) 버튼 아이콘 변경용
    [SerializeField] private Sprite soundOnSprite;  // (선택) 소리 켜짐 아이콘
    [SerializeField] private Sprite soundOffSprite; // (선택) 소리 꺼짐 아이콘

    private const float MIN_VOLUME = 0.0001f;
    private bool isMuted = false; // 현재 음소거 상태

    void Start()
    {
        // 1. 저장된 볼륨 불러오기
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        // 2. 슬라이더 설정
        masterSlider.minValue = MIN_VOLUME;
        bgmSlider.minValue = MIN_VOLUME;
        sfxSlider.minValue = MIN_VOLUME;

        masterSlider.value = masterVol;
        bgmSlider.value = bgmVol;
        sfxSlider.value = sfxVol;

        // 3. 리스너 연결
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // 4. 초기값 적용
        ApplyVolume(masterParam, masterVol);
        ApplyVolume(bgmParam, bgmVol);
        ApplyVolume(sfxParam, sfxVol);

        // 아이콘 초기화 (선택 사항)
        UpdateMuteIcon();
    }

    // --- 음소거 토글 기능 (버튼에 연결하세요) ---
    public void ToggleMasterMute()
    {
        isMuted = !isMuted; // 상태 반전 (On <-> Off)

        if (isMuted)
        {
            // 음소거: 볼륨을 최저(-80dB)로 설정
            masterMixer.SetFloat(masterParam, -80f);
        }
        else
        {
            // 음소거 해제: 현재 슬라이더 값으로 복구
            SetMasterVolume(masterSlider.value);
        }

        UpdateMuteIcon();
    }

    // 아이콘 변경 로직 (UI Image가 연결되어 있을 때만 작동)
    private void UpdateMuteIcon()
    {
        if (muteButtonImage != null && soundOnSprite != null && soundOffSprite != null)
        {
            muteButtonImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
        }
    }

    // --- 기존 볼륨 조절 함수들 ---

    public void SetMasterVolume(float volume)
    {
        // 음소거 상태에서 슬라이더를 건드리면 음소거가 풀리게 할지, 유지할지 결정
        // 여기서는 슬라이더를 움직이면 음소거가 풀리도록 설정 (사용자 직관성)
        if (isMuted)
        {
            isMuted = false;
            UpdateMuteIcon();
        }

        ApplyVolume(masterParam, volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetBGMVolume(float volume)
    {
        ApplyVolume(bgmParam, volume);
        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        ApplyVolume(sfxParam, volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    private void ApplyVolume(string param, float volume)
    {
        float v = Mathf.Clamp(volume, MIN_VOLUME, 1f);
        masterMixer.SetFloat(param, Mathf.Log10(v) * 20f);
    }
}