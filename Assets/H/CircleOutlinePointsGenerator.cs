using UnityEngine;

public class CircleOutlinePointsGenerator : MonoBehaviour
{
    [Header("Where to generate")]
    public Transform dalgona;          // 기준(보통 Dalgona)
    public Transform pointsParent;     // 비어있으면 자동 생성
    public string pointsParentName = "OutlinePoints";

    [Header("Circle settings")]
    public int pointCount = 64;
    public float radius = 0.12f;       // (dalgona 로컬 기준) 반지름
    public float yOffset = 0f;
    public float startAngleDeg = 0f;

    [Header("Optional: auto radius from a circle mesh")]
    public MeshFilter circleMesh;      // 주황 원 라인 메쉬 있으면 드래그
    public bool useMeshToEstimateRadius = true;

    [Header("Cleanup")]
    public bool clearExistingChildren = true;

    [ContextMenu("Generate Circle Points")]
    public void Generate()
    {
        if (!dalgona) dalgona = transform;

        if (!pointsParent)
        {
            var go = new GameObject(pointsParentName);
            pointsParent = go.transform;
            pointsParent.SetParent(dalgona, false);
            pointsParent.localPosition = Vector3.zero;
            pointsParent.localRotation = Quaternion.identity;
            pointsParent.localScale = Vector3.one;
        }

        if (clearExistingChildren)
        {
            for (int i = pointsParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(pointsParent.GetChild(i).gameObject);
            }
        }

        float r = radius;

        // 원 라인 메쉬가 있으면 (대충) 반지름 추정해서 쓰기
        if (useMeshToEstimateRadius && circleMesh && circleMesh.sharedMesh)
        {
            // circleMesh 로컬 bounds에서 +X 방향의 한 점을 잡아 dalgona 로컬로 변환해서 반지름 추정
            var b = circleMesh.sharedMesh.bounds;
            Vector3 localOnMesh = b.center + new Vector3(b.extents.x, 0f, 0f);
            Vector3 worldOnMesh = circleMesh.transform.TransformPoint(localOnMesh);
            Vector3 dalgonaLocal = dalgona.InverseTransformPoint(worldOnMesh);
            dalgonaLocal.y = 0f;
            r = dalgonaLocal.magnitude;
        }

        int n = Mathf.Clamp(pointCount, 8, 512);
        float startRad = startAngleDeg * Mathf.Deg2Rad;

        for (int i = 0; i < n; i++)
        {
            float t = (i / (float)n) * Mathf.PI * 2f + startRad;
            Vector3 pos = new Vector3(Mathf.Cos(t) * r, yOffset, Mathf.Sin(t) * r);

            var p = new GameObject($"P{i:000}");
            p.transform.SetParent(pointsParent, false);
            p.transform.localPosition = pos;
            p.transform.localRotation = Quaternion.identity;
            p.transform.localScale = Vector3.one;
        }

        Debug.Log($"Generated {n} outline points. radius={r:0.###} (local)");
    }
}
