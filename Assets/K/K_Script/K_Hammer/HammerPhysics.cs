using UnityEngine;

public class HammerPhysics : MonoBehaviour
{
    private Vector3 lastPosition;
    public float currentSpeed { get; private set; } // 떡이 이 값을 가져다 씁니다.

    void Start()
    {
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        // 손에 들려있어서 Kinematic 상태여도 직접 속도를 계산함
        float distance = Vector3.Distance(transform.position, lastPosition);
        currentSpeed = distance / Time.fixedDeltaTime;

        lastPosition = transform.position;
    }
}