using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class J_GSheetVoteRequest
{
    // 서버 분기: vote / results / reset
    public string action = "vote";

    // 투표/조회 토큰 (GoogleSheet Config!B1)
    public string token;

    // 기기 고유ID (재투표 시 표 이동 처리용)
    public string deviceId;

    public string choiceId;
    public string choiceLabel;
}

[Serializable]
public class J_GSheetResetRequest
{
    // 초기화 요청
    public string action = "reset";

    // 투표 토큰 (Config!B1)
    public string token;

    // 관리자 토큰 (Config!B2) - 관리자 빌드에서만 채워짐
    public string adminToken;
}

[Serializable]
public class J_GSheetResultEntry
{
    public int rank;
    public string id;
    public string label;
    public int votes;
    public float percent;
}

[Serializable]
public class J_GSheetResults
{
    public bool ok;
    public int total;
    public J_GSheetResultEntry[] ranked;
    public string error;
}

public class J_GSheetClient : MonoBehaviour
{
    [Header("Google Apps Script WebApp URL (반드시 /exec)")]
    public string webAppUrl;

    [Header("Vote Token (GoogleSheet Config!B1)")]
    public string token;

    [Header("Admin Token (GoogleSheet Config!B2) - 자동 주입")]
    [SerializeField] private string adminToken = "";

    // 관리자 여부를 UI에서 판단할 때 쓰기 좋게 공개
    public bool IsAdmin => !string.IsNullOrEmpty(adminToken);

    // 기기 고유키: 한 번 생성 후 계속 유지
    string DeviceId
    {
        get
        {
            const string key = "J_DeviceId";
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetString(key, Guid.NewGuid().ToString("N"));
                PlayerPrefs.Save();
            }
            return PlayerPrefs.GetString(key);
        }
    }

    void Awake()
    {
        // 관리자 빌드에서만 로컬 파일(J_AdminToken.local.cs)의 토큰을 주입
        // 일반 빌드는 심볼이 없어서 이 코드가 컴파일되지 않음(= 토큰 포함 X)
#if J_ADMIN_BUILD
        adminToken = J_AdminTokenLocal.Value;   // <- 관리자 PC에만 존재하는 로컬 파일에서 가져옴
#else
        adminToken = "";                        // <- 일반 빌드는 무조건 빈 값
#endif
    }

    // ======================
    // 1) 투표 전송
    // ======================
    public void SubmitVote(string choiceId, string choiceLabel, Action<J_GSheetResults> onDone)
    {
        StartCoroutine(CoSubmitVote(choiceId, choiceLabel, onDone));
    }

    IEnumerator CoSubmitVote(string choiceId, string choiceLabel, Action<J_GSheetResults> onDone)
    {
        var req = new J_GSheetVoteRequest
        {
            token = token,
            deviceId = DeviceId,
            choiceId = choiceId,
            choiceLabel = choiceLabel
        };

        string json = JsonUtility.ToJson(req);

        var uwr = new UnityWebRequest(webAppUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            onDone?.Invoke(new J_GSheetResults { ok = false, error = uwr.error });
            yield break;
        }

        var res = JsonUtility.FromJson<J_GSheetResults>(uwr.downloadHandler.text);
        onDone?.Invoke(res);
    }

    // ======================
    // 2) 결과 조회
    // ======================
    public void FetchResults(Action<J_GSheetResults> onDone)
    {
        StartCoroutine(CoFetchResults(onDone));
    }

    IEnumerator CoFetchResults(Action<J_GSheetResults> onDone)
    {
        string url = webAppUrl + "?mode=results";
        using var uwr = UnityWebRequest.Get(url);

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            onDone?.Invoke(new J_GSheetResults { ok = false, error = uwr.error });
            yield break;
        }

        var res = JsonUtility.FromJson<J_GSheetResults>(uwr.downloadHandler.text);
        onDone?.Invoke(res);
    }

    // ======================
    // 3) 관리자 초기화
    // ======================
    public void ResetAll(Action<J_GSheetResults> onDone)
    {
        StartCoroutine(CoResetAll(onDone));
    }

    IEnumerator CoResetAll(Action<J_GSheetResults> onDone)
    {
        // 일반 빌드는 adminToken이 비어있어서 여기서 막힘
        if (string.IsNullOrEmpty(adminToken))
        {
            onDone?.Invoke(new J_GSheetResults { ok = false, error = "not admin" });
            yield break;
        }

        var req = new J_GSheetResetRequest
        {
            token = token,
            adminToken = adminToken
        };

        string json = JsonUtility.ToJson(req);

        var uwr = new UnityWebRequest(webAppUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            onDone?.Invoke(new J_GSheetResults { ok = false, error = uwr.error });
            yield break;
        }

        var res = JsonUtility.FromJson<J_GSheetResults>(uwr.downloadHandler.text);
        onDone?.Invoke(res);
    }
}
