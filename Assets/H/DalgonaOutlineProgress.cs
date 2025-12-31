using UnityEngine;

public class DalgonaOutlineProgress : MonoBehaviour
{
    [Header("References")]
    public Transform dalgona;       // 보통 Dalgona(원판) Transform
    public Transform needleTip;     // Needle/Tip
    public Transform pointsParent;  // 외곽선 점(P0..Pn) 부모

    [Header("Tuning")]
    public float halfWidth = 0.01f; // 라인 허용 두께(±)
    public int bins = 80;           // 진행 분할(40~160 추천)

    [Header("Fail")]
    public float crack = 0f;
    public float crackPerSecOffPath = 0.35f;

    Vector3[] pts;
    float totalLen;
    bool[] visited;
    int visitedCount;

    void Awake() => Rebuild();

    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        if (!dalgona || !pointsParent) return;

        int n = pointsParent.childCount;
        if (n < 3) return;

        pts = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            Vector3 local = dalgona.InverseTransformPoint(pointsParent.GetChild(i).position);
            local.y = 0f;
            pts[i] = local;
        }

        // 전체 길이 계산(폐곡선)
        totalLen = 0f;
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            totalLen += Vector3.Distance(pts[i], pts[j]);
        }

        visited = new bool[Mathf.Max(16, bins)];
        visitedCount = 0;
        crack = 0f;
    }

    void Update()
    {
        if (!dalgona || !needleTip || pts == null || pts.Length < 3) return;

        Vector3 p = dalgona.InverseTransformPoint(needleTip.position);
        p.y = 0f;

        float bestDist = float.MaxValue;
        float bestS = 0f;

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
            }

            sAtStart += Vector3.Distance(a, b);
        }

        bool onPath = bestDist <= halfWidth;

        if (onPath)
        {
            float u = (totalLen > 1e-6f) ? (bestS / totalLen) : 0f;
            int idx = Mathf.Clamp(Mathf.FloorToInt(u * visited.Length), 0, visited.Length - 1);

            if (!visited[idx])
            {
                visited[idx] = true;
                visitedCount++;
                // Debug.Log($"Progress: {Progress01():P0}");
            }
        }
        else
        {
            crack += crackPerSecOffPath * Time.deltaTime;
            crack = Mathf.Clamp01(crack);
        }

        if (crack >= 1f)
        {
            Debug.Log("FAIL (cracked)");
            enabled = false;
        }

        if (visitedCount >= visited.Length)
        {
            Debug.Log("SUCCESS (completed outline)");
            enabled = false;
        }
    }

    public float Progress01() => visited.Length == 0 ? 0f : (float)visitedCount / visited.Length;
}
