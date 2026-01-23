using UnityEngine;
using Photon.Pun;

public class LocalHandTargetFollow : MonoBehaviourPun
{
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    // OVR이면 leftHandAnchor/rightHandAnchor 넣기
    // XRI면 Left/Right Controller Transform 넣기
    public Transform leftController;
    public Transform rightController;

    void Start()
    {
        if (!photonView.IsMine) return;

        // OVR 자동 찾기(원하면)
        if ((leftController == null || rightController == null))
        {
            var rig = FindFirstObjectByType<OVRCameraRig>();
            if (rig != null)
            {
                leftController = rig.leftHandAnchor;
                rightController = rig.rightHandAnchor;
            }
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (leftController != null)
        {
            leftHandTarget.position = leftController.position;
            leftHandTarget.rotation = leftController.rotation;
        }

        if (rightController != null)
        {
            rightHandTarget.position = rightController.position;
            rightHandTarget.rotation = rightController.rotation;
        }
    }
}
