using UnityEngine;

public class DalgonaOutlineProgress : MonoBehaviour
{
    [Header("References")]
    public Transform dalgona;       // 기준 Transform (OutlinePoints 생성 기준과 동일해야 함)
    public Transform needleTip;     // Needle/Tip
    public Transform pointsParent;  // OutlinePoints (P000..)

    [Header("Tuning")]
    [Tooltip("라인 허용 거리(±). 처음엔 0.03으로 넉넉하게 추천")]
    public float halfWidth = 0.03f;

    [Tooltip("진행 분할. 테스트는 12~20 추천, 최종은 60~120")]
    public int bins = 12;

    [Header("Fail")]
    public float crack = 0f;
    public float crackPerSecOffPath = 2.0f;
    public float crackMax = 1f;

    [Header("Haptics")]
    public bool hapticsOnPath = true;
    public OVRInput.Controller hapticController = OVRInput.Controller.RTouch;
    [Range(0f, 1f)] public float hapticAmplitude = 0.25f;
    [Range(0f, 1f)] public float hapticFrequency = 0.6f;

    [Header("Debug")]
    public bool debugLogProgress = true;
    public bool debugLogOnPath = false;
    public bool drawGizmos = true;

    // 내부 캐시
    private Vector3[] pts;   // dalgona 로컬(xz 평면) 좌표
    private float totalLen;
    private bool[] visited;
    private int visitedCount;

    // 디버그용
    private float lastBestDist = -1f;
    private Vector3 lastClosestLocal;
    private bool lastOnPath = false;

    void Awake()
    {
        Rebuild();
    }

    void OnDisable()
    {
        StopHaptics();
    }

    void OnDestroy()
    {
        StopHaptics();
    }

    private void StopHaptics()
    {
        if (hapticsOnPath)
            OVRInput.SetControllerVibration(0f, 0f, hapticController);
    }

    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        if (!dalgona || !pointsParent)
        {
            Debug.LogWarning("[DalgonaOutlineProgress] Missing dalgona or pointsParent");
            return;
        }

        int n = pointsParent.childCount;
        if (n < 3)
        {
            Debug.LogWarning("[DalgonaOutlineProgress] Need at least 3 outline points");
            return;
        }

        pts = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            Vector3 local = dalgona.InverseTransformPoint(pointsParent.GetChild(i).position);
            local.y = 0f; // 평면 투영
            pts[i] = local;
        }

        totalLen = 0f;
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            totalLen += Vector3.Distance(pts[i], pts[j]);
        }

        visited = new bool[Mathf.Max(16, bins)];
        visitedCount = 0;
        crack = 0f;

        if (debugLogProgress)
            Debug.Log($"[DalgonaOutlineProgress] Rebuild OK. points={n}, bins={visited.Length}, totalLen={totalLen:0.###}");
    }

    void Update()
    {
        if (!dalgona || !needleTip || pts == null || pts.Length < 3 || visited == null || visited.Length == 0)
            return;

        // Tip을 dalgona 로컬로 변환 후 평면 투영
        Vector3 p = dalgona.InverseTransformPoint(needleTip.position);
        p.y = 0f;

        // 외곽선 세그먼트 중 가장 가까운 점 찾기
        float bestDist = float.MaxValue;
        float bestS = 0f;
        Vector3 bestClosest = Vector3.zero;

        float sAtStart = 0f;
        int n = pts.Length;

        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;

            Vector3 a = pts[i];
            Vector3 b = pts[j];
            Vector3 ab = b - a;

            float ab2 = Vector3.Dot(ab, ab);
            float t = (ab2 > 1e-6f) ? Vector3.Dot(p - a, ab) / ab2 : 0f;
            t = Mathf.Clamp01(t);

            Vector3 closest = a + ab * t;
            float d = Vector3.Distance(p, closest);

            if (d < bestDist)
            {
                bestDist = d;
                float segLen = Mathf.Sqrt(ab2);
                bestS = sAtStart + segLen * t;
                bestClosest = closest;
            }

            sAtStart += Vector3.Distance(a, b);
        }

        bool onPath = bestDist <= halfWidth;

        // 디버그 캐시
        lastBestDist = bestDist;
        lastClosestLocal = bestClosest;
        lastOnPath = onPath;

        // ✅ 라인 위면 진동, 아니면 끄기
        if (hapticsOnPath)
        {
            if (onPath) OVRInput.SetControllerVibration(hapticFrequency, hapticAmplitude, hapticController);
            else OVRInput.SetControllerVibration(0f, 0f, hapticController);
        }

        // 진행/실패 처리
        if (onPath)
        {
            float u = (totalLen > 1e-6f) ? (bestS / totalLen) : 0f;
            int idx = Mathf.Clamp(Mathf.FloorToInt(u * visited.Length), 0, visited.Length - 1);

            if (!visited[idx])
            {
                visited[idx] = true;
                visitedCount++;

                if (debugLogProgress)
                    Debug.Log($"Progress: {Progress01():P0}  (bestDist={bestDist:0.000})");
            }
        }
        else
        {
            crack += crackPerSecOffPath * Time.deltaTime;
            crack = Mathf.Clamp01(crack);

            if (debugLogOnPath)
                Debug.Log($"OffPath: bestDist={bestDist:0.000}, crack={crack:0.00}");
        }

        if (crack >= crackMax)
        {
            Debug.Log("FAIL (cracked)");
            StopHaptics();
            enabled = false;
            return;
        }

        if (visitedCount >= visited.Length)
        {
            Debug.Log("SUCCESS (completed outline)");
            StopHaptics();
            enabled = false;
            return;
        }
    }

    public float Progress01() => visited == null || visited.Length == 0 ? 0f : (float)visitedCount / visited.Length;

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (!dalgona) return;

        // Tip -> closest 디버그 선 (Scene 뷰에서만 참고)
        if (needleTip != null && pts != null && pts.Length >= 3)
        {
            Gizmos.color = lastOnPath ? Color.green : Color.red;

            Vector3 tipLocal = dalgona.InverseTransformPoint(needleTip.position);
            tipLocal.y = 0f;

            Vector3 tipWorld = dalgona.TransformPoint(tipLocal);
            Vector3 closestWorld = dalgona.TransformPoint(lastClosestLocal);

            Gizmos.DrawLine(tipWorld, closestWorld);
            Gizmos.DrawSphere(closestWorld, 0.005f);
        }
    }
}
