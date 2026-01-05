using UnityEngine;

// 이 스크립트는 OVRGrabbable과 Rigidbody가 있어야 작동합니다.
[RequireComponent(typeof(OVRGrabbable))]
[RequireComponent(typeof(Rigidbody))]
public class HammerGravity : MonoBehaviour
{
    private OVRGrabbable grabbable;
    private Rigidbody rb;

    void Start()
    {
        grabbable = GetComponent<OVRGrabbable>();
        rb = GetComponent<Rigidbody>();

        // 시작할 때는 중력이 켜져 있어야 바닥에 잘 놓여 있습니다.
        rb.useGravity = true;
    }

    void Update()
    {
        // 핵심 로직: 잡고 있는 동안엔 중력을 끄고, 놓으면 다시 켭니다.
        if (grabbable.isGrabbed)
        {
            rb.useGravity = false; // 잡았을 때: 무중력 (손에 착 붙음)
        }
        else
        {
            rb.useGravity = true;  // 놓았을 때: 중력 적용 (바닥으로 떨어짐)
        }
    }
}