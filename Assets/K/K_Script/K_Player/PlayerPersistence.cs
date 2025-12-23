using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence instance;

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

    public void DestroySelf()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        instance = null;
        Destroy(gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawnPoint = GameObject.Find("SpawnPoint");
        if (spawnPoint != null)
        {
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
