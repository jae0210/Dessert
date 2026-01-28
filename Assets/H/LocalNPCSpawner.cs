using UnityEngine;
using UnityEngine.AI;

public class LocalNPCSpawner : MonoBehaviour
{
    public GameObject npcPrefab;
    public int count = 5;
    public float spawnRadius = 5f;

    [Header("Exhibit LookPoints in Scene (drag from Hierarchy)")]
    public Transform[] exhibits;

    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 r = Random.insideUnitCircle * spawnRadius;
            Vector3 pos = transform.position + new Vector3(r.x, 0f, r.y);

            if (NavMesh.SamplePosition(pos, out var hit, 3f, NavMesh.AllAreas))
                pos = hit.position;

            var npc = Instantiate(npcPrefab, pos, Quaternion.Euler(0, Random.Range(0, 360f), 0));

            // ✅ 생성 후 주입 (프리팹이 씬 오브젝트 참조 못하는 문제 해결)
            var tour = npc.GetComponent<LocalNPCMuseumTour>();
            if (tour != null)
                tour.exhibits = exhibits;
        }
    }
}
