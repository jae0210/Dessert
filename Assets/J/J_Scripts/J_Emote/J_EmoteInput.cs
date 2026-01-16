using Photon.Pun;
using UnityEngine;

public class J_EmoteInput : MonoBehaviourPun
{
    [Header("Refs")]
    [SerializeField] private J_NetEmote netEmote;
    [SerializeField] private GameObject menuRoot;                // MenuRoot (켜고/끄는 용도)
    [SerializeField] private J_EmoteRadialMenuUI radialMenuUI;    // RadialMenu 오브젝트에 붙은 컴포넌트
    [SerializeField] private Sprite[] icons;                      // 이모티콘 아이콘들(= netEmote emotes와 같은 순서 권장)

    [Header("Input")]
    [SerializeField] private bool holdToOpen = true;              // A 누르고 있는 동안만 열기
    [SerializeField] private float deadZone = 0.35f;              // 스틱 데드존

    private bool isOpen;
    private int currentIndex = -1;

    void Start()
    {
        if (!photonView.IsMine)
        {
            enabled = false;
            return;
        }

        if (menuRoot) menuRoot.SetActive(false);

        // 버튼 클릭도 되게 Build
        if (radialMenuUI && icons != null && icons.Length > 0)
        {
            radialMenuUI.Build(icons, idx =>
            {
                netEmote.RequestEmote(idx);
                Close();
            });
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        if (netEmote == null || menuRoot == null) return;

        bool aDown =
            OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch) ||
            OVRInput.GetDown(OVRInput.RawButton.A, OVRInput.Controller.RTouch);

        bool aUp =
            OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch) ||
            OVRInput.GetUp(OVRInput.RawButton.A, OVRInput.Controller.RTouch);

        if (holdToOpen)
        {
            if (aDown) Open();
            if (isOpen) UpdateStickSelection();
            if (aUp) ConfirmAndClose();
        }
        else
        {
            if (aDown)
            {
                if (isOpen) Close();
                else Open();
            }
            if (isOpen) UpdateStickSelection();
        }
    }

    void Open()
    {
        isOpen = true;
        currentIndex = -1;
        menuRoot.SetActive(true);
    }

    void Close()
    {
        isOpen = false;
        currentIndex = -1;
        menuRoot.SetActive(false);
    }

    void ConfirmAndClose()
    {
        if (currentIndex >= 0)
            netEmote.RequestEmote(currentIndex);

        Close();
    }

    void UpdateStickSelection()
    {
        // 오른손 스틱(오큘러스는 보통 SecondaryThumbstick이 오른손)
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.RTouch);

        if (stick.magnitude < deadZone)
        {
            currentIndex = -1;
            return;
        }

        int count = (icons != null) ? icons.Length : 0;
        if (count == 0) return;

        // 위쪽을 0번으로
        float ang = Mathf.Atan2(stick.y, stick.x) * Mathf.Rad2Deg;
        ang = (ang + 450f) % 360f; // -90 보정(위가 시작점)

        float step = 360f / count;
        int idx = Mathf.FloorToInt(ang / step);
        currentIndex = Mathf.Clamp(idx, 0, count - 1);

        // ★ 하이라이트는 “일단 간단히” 스케일로 처리
        // (radialMenuUI가 만든 버튼들이 root 아래에 있으니 Root 아래 자식들 스케일 조정)
        // 여기서 root 접근이 필요하면, radialMenuUI에 Highlight 함수 추가하는 게 깔끔함.
    }
}
