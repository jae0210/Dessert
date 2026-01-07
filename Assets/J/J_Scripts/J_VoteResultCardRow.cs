using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class J_VoteResultCardRow : MonoBehaviour
{
    [Header("Texts (Legacy)")]
    public Text rankText;
    public Text labelText;
    public Text votesPercentText;

    [Header("Images")]
    public Image badgeImage;      // Top3 뱃지(왕관/리본 등)
    public Image barFillImage;    // Image Type = Filled 권장

    [Header("Rank Sprites (Top3 Only)")]
    public Sprite badgeGold;      // 1위 뱃지 스프라이트(왕관/리본 느낌)
    public Sprite badgeSilver;    // 2위
    public Sprite badgeBronze;    // 3위

    [Header("BarFill Sprites")]
    public Sprite barFillGold;    // 1위 바
    public Sprite barFillSilver;  // 2위 바
    public Sprite barFillBronze;  // 3위 바
    public Sprite barFillGray;    // 4위~ 기본 바

    [Header("Behaviour")]
    public bool hideBadgeAfterTop3 = true;    // 4위 이후 배지 숨김(권장: enabled=false)
    public bool forceWhiteImageColor = true;  // 스프라이트 원색 유지(틴트 방지)

    [Header("Rank Text")]
    public bool hideRankTextForTop3 = true;   // ✅ 1~3위 RankText 숨김

    [Header("Bar Animation")]
    public bool animateOnSet = true;
    public bool startFromZero = true;
    public bool useUnscaledTime = true;
    public float animSeconds = 0.35f;
    public float rankDelayStep = 0.03f;
    public AnimationCurve ease = null;

    [Header("Shine (End Highlight)")]
    public RectTransform shineEnd;    // ShineEnd의 RectTransform
    public Image shineEndImage;       // ShineEnd의 Image
    public bool showShineWhenZero = false;
    public float shineBaseAlpha = 0.15f;
    public float shinePeakAlpha = 0.85f;
    public float shineMinScale = 0.95f;
    public float shineMaxScale = 1.35f;
    public float shinePulseHz = 3.5f;     // 펄스 속도
    public float shineAfterAlpha = 0.25f; // 애니 끝난 뒤 남길 알파(0이면 꺼짐)

    Coroutine barCo;

    void Awake()
    {
        if (ease == null || ease.length == 0)
            ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

        if (barFillImage != null)
        {
            barFillImage.type = Image.Type.Filled;
            barFillImage.fillMethod = Image.FillMethod.Horizontal;
            barFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

            if (forceWhiteImageColor) barFillImage.color = Color.white;

            if (barFillGray != null) barFillImage.sprite = barFillGray;

            if (startFromZero) barFillImage.fillAmount = 0f;
        }

        if (badgeImage != null && forceWhiteImageColor)
            badgeImage.color = Color.white;

        InitShine();
    }

    void OnDisable()
    {
        if (barCo != null) StopCoroutine(barCo);
        barCo = null;
    }

    void LateUpdate()
    {
        UpdateShinePosition();
    }

    void InitShine()
    {
        if (shineEndImage != null)
        {
            var c = shineEndImage.color;
            c.a = 0f;
            shineEndImage.color = c;
            shineEndImage.enabled = false;
        }
        if (shineEnd != null)
        {
            shineEnd.localScale = Vector3.one * shineMinScale;
        }
    }

    void UpdateShinePosition()
    {
        if (shineEnd == null || barFillImage == null) return;

        float a = Mathf.Clamp01(barFillImage.fillAmount);

        RectTransform barRt = barFillImage.rectTransform;
        float x = barRt.rect.xMin + (barRt.rect.width * a);

        var p = shineEnd.anchoredPosition;
        shineEnd.anchoredPosition = new Vector2(x, p.y);
    }

    public void Set(int rank, string label, int votes, float percent, float bar01)
    {
        bool isTop3 = (rank <= 3);

        // ✅ RankText: Top3 숨김 / 4위부터 표시
        if (rankText != null)
        {
            // 혹시 부모가 꺼져있던 경우 대비 (원하면 제거 가능)
            if (!rankText.gameObject.activeSelf) rankText.gameObject.SetActive(true);

            if (hideRankTextForTop3 && isTop3)
            {
                rankText.enabled = false;
            }
            else
            {
                rankText.enabled = true;
                rankText.text = rank + "위";
            }
        }

        if (labelText != null) labelText.text = label;

        if (votesPercentText != null)
            votesPercentText.text = $"{votes}표  {percent:0.0}%";

        // ✅ Top3만 스프라이트 교체 + 4위부터 배지 숨김(단, GO는 끄지 않음)
        ApplyRankSprites(rank);

        // 바 채우기
        float target = Mathf.Clamp01(bar01);

        if (barFillImage == null)
        {
            SetShineStatic(target);
            return;
        }

        if (!animateOnSet || !isActiveAndEnabled || animSeconds <= 0.001f)
        {
            barFillImage.fillAmount = target;
            SetShineStatic(target);
            return;
        }

        if (barCo != null) StopCoroutine(barCo);

        float from = startFromZero ? 0f : barFillImage.fillAmount;
        if (startFromZero) barFillImage.fillAmount = 0f;

        float delay = Mathf.Max(0f, (rank - 1) * rankDelayStep);
        barCo = StartCoroutine(CoFill(from, target, delay));
    }

    void ApplyRankSprites(int rank)
    {
        bool isTop3 = rank <= 3;

        // ✅ 배지: Top3만 보이기
        // 중요: 4위 이후에 rankText가 배지의 자식일 수 있으니,
        // badgeImage.gameObject.SetActive(false) 하지 말고 badgeImage.enabled=false로 숨김.
        if (badgeImage != null)
        {
            // GO는 항상 켜둠(자식 텍스트 살리기)
            if (!badgeImage.gameObject.activeSelf) badgeImage.gameObject.SetActive(true);

            if (isTop3)
            {
                badgeImage.enabled = true;
                Sprite s = GetBadgeSprite(rank);
                if (s != null) badgeImage.sprite = s;
                if (forceWhiteImageColor) badgeImage.color = Color.white;
            }
            else
            {
                if (hideBadgeAfterTop3)
                {
                    badgeImage.enabled = false; // ✅ 여기!
                }
                else
                {
                    badgeImage.enabled = true;  // 숨기지 않는 옵션이면 유지
                }
            }
        }

        // 바: Top3는 금/은/동, 나머지는 회색
        if (barFillImage != null)
        {
            Sprite s = GetBarFillSprite(rank);
            if (s != null) barFillImage.sprite = s;
            if (forceWhiteImageColor) barFillImage.color = Color.white;
        }
    }

    Sprite GetBadgeSprite(int rank)
    {
        if (rank == 1) return badgeGold;
        if (rank == 2) return badgeSilver;
        if (rank == 3) return badgeBronze;
        return null;
    }

    Sprite GetBarFillSprite(int rank)
    {
        if (rank == 1 && barFillGold != null) return barFillGold;
        if (rank == 2 && barFillSilver != null) return barFillSilver;
        if (rank == 3 && barFillBronze != null) return barFillBronze;
        return barFillGray; // 4위~
    }

    void SetShineStatic(float fill01)
    {
        if (shineEndImage == null) return;

        if (fill01 <= 0f && !showShineWhenZero)
        {
            shineEndImage.enabled = false;
            return;
        }

        shineEndImage.enabled = true;

        var c = shineEndImage.color;
        c.a = shineAfterAlpha;
        shineEndImage.color = c;

        if (shineEnd != null)
            shineEnd.localScale = Vector3.one * shineMinScale;
    }

    IEnumerator CoFill(float from, float to, float delay)
    {
        if (delay > 0f)
        {
            if (useUnscaledTime)
            {
                float d = 0f;
                while (d < delay)
                {
                    d += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }
        }

        // shine 켜기
        if (shineEndImage != null)
        {
            shineEndImage.enabled = true;
            var c = shineEndImage.color;
            c.a = shineBaseAlpha;
            shineEndImage.color = c;
        }
        if (shineEnd != null) shineEnd.localScale = Vector3.one * shineMinScale;

        float t = 0f;
        while (t < animSeconds)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t / animSeconds);
            float e = (ease != null) ? ease.Evaluate(k) : k;

            barFillImage.fillAmount = Mathf.Lerp(from, to, e);

            float ping = Mathf.PingPong((useUnscaledTime ? Time.unscaledTime : Time.time) * shinePulseHz, 1f);
            float a = Mathf.Lerp(shineBaseAlpha, shinePeakAlpha, ping);
            float s = Mathf.Lerp(shineMinScale, shineMaxScale, ping);

            if (shineEndImage != null)
            {
                var c = shineEndImage.color;
                c.a = a;
                shineEndImage.color = c;
            }
            if (shineEnd != null) shineEnd.localScale = Vector3.one * s;

            yield return null;
        }

        barFillImage.fillAmount = to;

        // 애니 끝난 뒤 shine 정리
        if (shineEndImage != null)
        {
            if (to <= 0f && !showShineWhenZero)
            {
                shineEndImage.enabled = false;
            }
            else
            {
                var c = shineEndImage.color;
                c.a = shineAfterAlpha;
                shineEndImage.color = c;
                shineEndImage.enabled = (shineAfterAlpha > 0f);
            }
        }
        if (shineEnd != null) shineEnd.localScale = Vector3.one * shineMinScale;

        barCo = null;
    }
}
