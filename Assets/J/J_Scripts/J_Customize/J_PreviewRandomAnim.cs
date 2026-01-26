using System.Collections;
using UnityEngine;

public class J_PreviewExhibitAnim : MonoBehaviour
{
    public Animator animator;

    [Header("Base Idle")]
    public string baseIdleState = "Bobusang_Idle";

    [Header("Gesture States (non-loop 추천)")]
    public string[] gestureStates =
    {
        "Gesture_Wave",
        "Gesture_LookAround",
        "Gesture_Scratch"
    };

    [Header("Chance")]
    [Range(0f, 1f)]
    public float gestureChance = 0.50f; // 50%면 체감상 제스처 자주 나옴

    [Header("Idle timing (일반 대기)")]
    public float idleMinSeconds = 1.5f;
    public float idleMaxSeconds = 3.0f;

    [Header("After Gesture (제스처 직후 대기)")]
    public float postGestureIdleSeconds = 0.15f;  // ★ 제스처 끝나면 거의 바로 다음으로

    [Header("Blend")]
    public float crossFadeToIdle = 0.20f;
    public float crossFadeToGesture = 0.30f;
    public float idleBridgeSeconds = 0.08f;

    [Header("Switch timing")]
    [Range(0.7f, 0.98f)]
    public float switchAtNormalized = 0.90f;

    public bool avoidRepeat = true;

    int lastGestureIndex = -1;
    bool skipLongIdleOnce = false; // ★ 제스처 뒤에는 긴 idle 대기 스킵

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (!animator) return;
        StartCoroutine(CoLoop());
    }

    IEnumerator CoLoop()
    {
        yield return null;

        animator.CrossFadeInFixedTime(baseIdleState, 0.01f, 0, 0f);

        while (true)
        {
            // ✅ 제스처 직후에는 긴 대기(IdleMin/Max) 대신 아주 짧게만 쉼
            float wait = skipLongIdleOnce ? postGestureIdleSeconds : Random.Range(idleMinSeconds, idleMaxSeconds);
            skipLongIdleOnce = false;
            yield return new WaitForSeconds(wait);

            // 제스처 안 나오는 턴이면 그냥 Idle 유지하고 다음 루프로
            if (Random.value > gestureChance || gestureStates == null || gestureStates.Length == 0)
                continue;

            // 전환을 루프 끝에서만
            yield return WaitUntilLoopNearEnd();

            // Idle로 살짝 정리(자연스러운 브릿지)
            animator.CrossFadeInFixedTime(baseIdleState, crossFadeToIdle, 0, 0f);
            yield return new WaitForSeconds(idleBridgeSeconds);

            // 제스처 선택
            int g = PickGestureIndex();
            lastGestureIndex = g;

            animator.CrossFadeInFixedTime(gestureStates[g], crossFadeToGesture, 0, 0f);

            // 제스처 길이만큼 대기 후 Idle로 복귀
            float gestureLen = GetCurrentClipLengthApprox();
            if (gestureLen < 0.1f) gestureLen = 1.5f;
            yield return new WaitForSeconds(gestureLen * 0.95f);

            animator.CrossFadeInFixedTime(baseIdleState, crossFadeToIdle, 0, 0f);

            // ★ 핵심: 제스처 끝났으니 "다음 결정을 빨리" 하도록 긴 idle 대기를 한 번 스킵
            skipLongIdleOnce = true;
        }
    }

    IEnumerator WaitUntilLoopNearEnd()
    {
        while (true)
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);
            float t = info.normalizedTime % 1f;
            if (t >= switchAtNormalized) yield break;
            yield return null;
        }
    }

    int PickGestureIndex()
    {
        if (!avoidRepeat || gestureStates.Length <= 1 || lastGestureIndex < 0)
            return Random.Range(0, gestureStates.Length);

        int n;
        do { n = Random.Range(0, gestureStates.Length); }
        while (n == lastGestureIndex);
        return n;
    }

    float GetCurrentClipLengthApprox()
    {
        var clips = animator.GetCurrentAnimatorClipInfo(0);
        if (clips != null && clips.Length > 0 && clips[0].clip != null)
            return clips[0].clip.length;
        return 0f;
    }
}
