using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string titleSceneName = "K_Title"; // 타이틀 씬 이름

    private bool loading;

    private void OnTriggerEnter(Collider other)
    {
        if (loading) return;
        if (!other.CompareTag("Player")) return;

        loading = true;
        var col = GetComponent<Collider>();
        if (col) col.enabled = false; // 여러번 트리거 방지

        // ✅ 타이틀로 돌아갈 때는 퍼시스턴트 플레이어(룸1 리그)부터 제거
        if (targetSceneName == titleSceneName && PlayerPersistence.instance != null)
        {
            PlayerPersistence.instance.DestroySelf();
        }

        SceneManager.LoadScene(targetSceneName);
    }
}
