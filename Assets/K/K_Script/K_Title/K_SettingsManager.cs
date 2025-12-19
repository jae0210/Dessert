using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer masterMixer;

    [Header("Mixer Parameters")]
    [SerializeField] private string bgmParam = "BGMVolume";
    [SerializeField] private string sfxParam = "SFXVolume";

    [Header("UI Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private const float MIN_VOLUME = 0.0001f;

    void Start()
    {
        // 저장된 값 불러오기
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        // 🔧 슬라이더 최소값 보장
        bgmSlider.minValue = MIN_VOLUME;
        sfxSlider.minValue = MIN_VOLUME;

        // 슬라이더 값 적용
        bgmSlider.value = bgmVol;
        sfxSlider.value = sfxVol;

        // 리스너 등록
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // 초기 볼륨 즉시 적용
        ApplyVolume(bgmParam, bgmVol);
        ApplyVolume(sfxParam, sfxVol);
    }

    void OnDestroy()
    {
        // 🔧 리스너 중복 방지
        bgmSlider.onValueChanged.RemoveListener(SetBGMVolume);
        sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
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
