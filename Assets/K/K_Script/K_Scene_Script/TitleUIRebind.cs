using UnityEngine;
using UnityEngine.EventSystems;

public class TitleUIRebind : MonoBehaviour
{
    void Start()
    {
        var rig = FindObjectOfType<OVRCameraRig>(true);
        var input = FindObjectOfType<OVRInputModule>(true);
        if (rig && input) input.rayTransform = rig.rightHandAnchor;

        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
    }
}
