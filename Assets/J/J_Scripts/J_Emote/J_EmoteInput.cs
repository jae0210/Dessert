using UnityEngine;

public class J_EmoteInput : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private J_NetEmote netEmote;
    [SerializeField] private GameObject menuRoot;               // MenuRoot (Ring/ButtonsRoot/RadialMenu 포함)
    [SerializeField] private J_EmoteRadialMenuUI radialMenuUI;  // RadialMenu 오브젝트
    [SerializeField] private Sprite[] icons;

    [Header("Ray Select")]
    [SerializeField] private Transform rayOrigin; // RightHandAnchor 등
    [SerializeField] private float maxRayDistance = 3f;
    [SerializeField, Range(0f, 0.9f)] private float deadZone01 = 0.35f;

    [Header("Input")]
    [SerializeField] private bool holdToOpen = true; // 지금 방식 그대로
    [SerializeField] private OVRInput.Button openButton = OVRInput.Button.One; // A

    private bool menuOpen;
    private int pendingIndex = -1;
    private bool built;

    void Awake()
    {
        if (menuRoot) menuRoot.SetActive(false);
    }

    void Start()
    {
        // 미리 한번 빌드해도 되고, 열릴 때 빌드해도 됨
        TryBuildOnce();
    }

    void Update()
    {
        if (!netEmote || !menuRoot || !radialMenuUI) return;

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
            // 토글 모드(필요하면)
            if (down)
            {
                if (!menuOpen) OpenMenu();
                else CloseMenu();
            }
            if (menuOpen) UpdateSelection();
        }
    }

    void TryBuildOnce()
    {
        if (built) return;
        if (icons == null || icons.Length == 0) return;

        radialMenuUI.Build(icons, null); // 클릭은 안 씀(우린 A-Release로 확정)
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
        // A를 떼는 순간 확정
        if (pendingIndex >= 0)
            netEmote.RequestEmote(pendingIndex);

        CloseMenu();
    }
}
