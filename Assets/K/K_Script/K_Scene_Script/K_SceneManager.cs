using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필수!

public class SceneChanger : MonoBehaviour
{
    // 방법 1: 씬 이름을 직접 입력해서 이동
    public void ChangeSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 방법 2: 빌드 세팅에 등록된 인덱스 번호로 이동
    public void ChangeSceneByIndex(int index)
    {
        SceneManager.LoadScene(index);
    }
}