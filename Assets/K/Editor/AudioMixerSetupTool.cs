using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;

public class AudioMixerSetupTool : EditorWindow
{
    [MenuItem("Tools/Audio/Auto Connect AudioSources")]
    public static void ConnectAudioSources()
    {
        // 1. 오디오 믹서 리소스 로드 (Resources 폴더나 경로 확인 필요, 여기서는 이름으로 찾습니다)
        // 주의: 프로젝트에 "MasterMixer"라는 이름의 믹서가 있어야 합니다.
        // 만약 못 찾는다면 직접 할당하는 방식으로 코드를 수정해야 합니다.
        string mixerPath = "MasterMixer"; // 믹서 파일 이름 (확장자 제외)
        AudioMixer mixer = Resources.Load<AudioMixer>(mixerPath);

        if (mixer == null)
        {
            // Resources 폴더에 없다면 에셋 데이터베이스에서 전체 검색
            string[] guids = AssetDatabase.FindAssets(mixerPath + " t:AudioMixer");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
            }
        }

        if (mixer == null)
        {
            Debug.LogError($"❌ '{mixerPath}'를 찾을 수 없습니다. 오디오 믹서 이름을 확인해주세요.");
            return;
        }

        // 2. 믹서 그룹 찾기
        AudioMixerGroup[] groups = mixer.FindMatchingGroups("Master"); // 최상위 그룹 검색
        AudioMixerGroup bgmGroup = null;
        AudioMixerGroup sfxGroup = null;

        // 그룹 이름으로 매칭 (BGM, SFX 정확한 그룹명을 사용해야 합니다)
        foreach (var group in mixer.FindMatchingGroups("Master"))
        {
            if (group.name.Contains("BGM")) bgmGroup = group;
            else if (group.name.Contains("SFX")) sfxGroup = group;
        }

        if (bgmGroup == null || sfxGroup == null)
        {
            Debug.LogError("❌ BGM 또는 SFX 그룹을 믹서에서 찾을 수 없습니다.");
            return;
        }

        // 3. 씬에 있는 모든 AudioSource 찾기
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        int count = 0;

        foreach (var source in sources)
        {
            // 이미 연결되어 있으면 패스 (원하면 이 조건문을 제거하여 강제 재설정 가능)
            if (source.outputAudioMixerGroup != null) continue;

            // 4. 자동 분류 로직 (이름 기반 추측)
            if (source.gameObject.name.Contains("BGM") || source.gameObject.name.Contains("Music"))
            {
                source.outputAudioMixerGroup = bgmGroup;
                Debug.Log($"🎵 [BGM] 연결됨: {source.gameObject.name}");
            }
            else
            {
                // 나머지는 모두 SFX로 간주
                source.outputAudioMixerGroup = sfxGroup;
                Debug.Log($"🔊 [SFX] 연결됨: {source.gameObject.name}");
            }
            count++;
        }

        Debug.Log($"✅ 총 {count}개의 오디오 소스가 자동으로 믹서에 연결되었습니다!");
    }
}