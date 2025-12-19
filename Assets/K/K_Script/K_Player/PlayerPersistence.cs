using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence instance;

    [SerializeField] private string titleSceneName = "K_Title"; // 타이틀 씬 이름으로 맞추기

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ✅ 타이틀로 돌아오면 룸1 플레이어(OVRCameraRig Variant) 제거
        if (scene.name == titleSceneName)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
            Destroy(gameObject);
            return;
        }

        // ✅ 게임 씬에서는 스폰포인트로 이동
        GameObject spawnPoint = GameObject.Find("SpawnPoint");
        if (spawnPoint != null)
        {
            // (추천) 실제 카메라(머리) 위치가 SpawnPoint에 맞도록 보정
            Camera cam = GetComponentInChildren<Camera>(true);
            if (cam != null)
            {
                Vector3 rigToHead = cam.transform.position - transform.position;
                transform.position = spawnPoint.transform.position - rigToHead;
                transform.rotation = spawnPoint.transform.rotation;
            }
            else
            {
                transform.SetPositionAndRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
