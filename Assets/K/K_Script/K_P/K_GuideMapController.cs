using UnityEngine;
using UnityEngine.EventSystems;

public class K_GuideMapController : MonoBehaviour, IPointerClickHandler
{
    [Header("이미지 설정")]
    public Sprite tutorialSprite;  // 튜토리얼 이미지 (처음에 보임)
    public Sprite mapSprite;       // 박물관 안내도 이미지 (클릭 시 보임)

    private SpriteRenderer spriteRenderer;
    private bool isShowingTutorial = true; // 현재 튜토리얼을 보여주고 있는지 여부

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 요구하신 대로 시작할 때 '튜토리얼' 이미지로 설정
        if (tutorialSprite != null)
            spriteRenderer.sprite = tutorialSprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleImage();
    }

    public void ToggleImage()
    {
        if (isShowingTutorial)
        {
            // 튜토리얼 상태에서 클릭 -> 안내도로 변경
            spriteRenderer.sprite = mapSprite;
            isShowingTutorial = false;
        }
        else
        {
            // 안내도 상태에서 클릭 -> 다시 튜토리얼로 변경
            spriteRenderer.sprite = tutorialSprite;
            isShowingTutorial = true;
        }
    }
}