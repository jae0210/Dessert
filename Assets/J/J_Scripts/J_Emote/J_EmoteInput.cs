using UnityEngine;

public class J_EmoteInput : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private J_NetEmote netEmote;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private J_EmoteRadialMenuUI radialMenuUI;
    [SerializeField] private Sprite[] icons;

    [Header("Ray Select")]
    [SerializeField] private Transform rayOrigin; // 비워두면 자동탐색
    [SerializeField] private bool autoFindRayOrigin = true;
    [SerializeField] private float maxRayDistance = 3f;
    [SerializeField, Range(0f, 0.9f)] private float deadZone01 = 0.35f;

    [Header("Input")]
    [SerializeField] private bool holdToOpen = true;
    [SerializeField] private OVRInput.Button openButton = OVRInput.Button.One;

    private bool menuOpen;
    private int pendingIndex = -1;
    private bool built;

    void Awake()
    {
        if (menuRoot) menuRoot.SetActive(false);
        EnsureRayOrigin(); // 여기서 1차 시도
    }

    void Start()
    {
        TryBuildOnce();
        EnsureRayOrigin(); // Start에서 한 번 더(씬 로딩 순서 대비)
    }

    void Update()
    {
        if (!netEmote || !menuRoot || !radialMenuUI) return;

        EnsureRayOrigin(); // 혹시 늦게 생성되는 경우 대비

        bool down = OVRInput.GetDown(openButton);
        bool held = OVRInput.Get(openButton);
        bool up = OVRInput.GetUp(openButton);

        if (holdToOpen)
        {
            if (down) OpenMenu();
            if (menuOpen && held) UpdateSelection();
            if (menuOpen && up) ConfirmAndClose();
        }
        else
        {
            if (down)
            {
                if (!menuOpen) OpenMenu();
                else CloseMenu();
            }
            if (menuOpen) UpdateSelection();
        }
    }

    void EnsureRayOrigin()
    {
        if (rayOrigin != null) return;
        if (!autoFindRayOrigin) return;

        // 1) 이름으로 직접 찾기 (가장 흔한 케이스)
        var go = GameObject.Find("RightHandAnchor");
        if (go != null) { rayOrigin = go.transform; return; }

        go = GameObject.Find("RightHandAnchorDetached");
        if (go != null) { rayOrigin = go.transform; return; }

        // 2) OVRCameraRig에서 찾기
        var rig = FindObjectOfType<OVRCameraRig>(true);
        if (rig != null && rig.rightHandAnchor != null)
        {
            rayOrigin = rig.rightHandAnchor;
            return;
        }

        // 3) 최후: 메인 카메라(헤드)라도 사용
        if (Camera.main != null)
            rayOrigin = Camera.main.transform;
    }

    void TryBuildOnce()
    {
        if (built) return;
        if (icons == null || icons.Length == 0) return;

        radialMenuUI.Build(icons, null); // 클릭은 안 씀 (A 떼면 확정)
        built = true;
    }

    void OpenMenu()
    {
        TryBuildOnce();
        menuRoot.SetActive(true);
        menuOpen = true;
        pendingIndex = -1;
        radialMenuUI.ClearHighlight();
    }

    void CloseMenu()
    {
        menuRoot.SetActive(false);
        menuOpen = false;
        pendingIndex = -1;
        radialMenuUI.ClearHighlight();
    }

    void UpdateSelection()
    {
        if (!rayOrigin) return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        int idx = radialMenuUI.PickIndexFromWorldRay(ray, maxRayDistance, deadZone01);

        if (idx >= 0)
        {
            pendingIndex = idx;
            radialMenuUI.SetHighlight(idx);
        }
        else
        {
            pendingIndex = -1;
            radialMenuUI.ClearHighlight();
        }
    }

    void ConfirmAndClose()
    {
        if (pendingIndex >= 0)
            netEmote.RequestEmote(pendingIndex);

        CloseMenu();
    }
}
