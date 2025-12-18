using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 필수

public class ScenePortal : MonoBehaviour
{
    [SerializeField] private string targetSceneName; // 이동할 씬 이름

    // 무언가 이 오브젝트의 트리거 범위 안으로 들어왔을 때 실행됨
    private void OnTriggerEnter(Collider other)
    {
        // 부딪힌 오브젝트의 태그가 "Player"인 경우에만 이동
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}