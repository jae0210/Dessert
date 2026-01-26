using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class J_NpcRandomLoopAnimator : MonoBehaviour
{
    [Header("Animator에 있는 State 이름들 (Animator 창의 상태 이름과 완전 동일)")]
    public List<string> stateNames = new List<string>();

    [Header("끊김 최소화를 위한 크로스페이드")]
    [Range(0f, 0.5f)] public float crossFadeTime = 0.08f;

    [Header("클립 끝나기 몇 % 지점에서 다음으로 넘어갈지")]
    [Range(0.5f, 0.99f)] public float switchAtNormalizedTime = 0.92f;

    [Header("연속으로 같은 동작 나오지 않게(가능하면)")]
    public bool avoidSameTwice = true;

    private Animator anim;
    private readonly List<int> bag = new List<int>();
    private int lastIndex = -1;
    private int currentIndex = -1;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (stateNames == null || stateNames.Count == 0)
        {
            Debug.LogError("[J_NpcRandomLoopAnimator] stateNames가 비어있어! Animator 상태 이름을 넣어줘.");
            enabled = false;
            return;
        }

        RefillBag();
        PlayNext(immediate: true);
    }

    void Update()
    {
        if (currentIndex < 0) return;

        // 현재 재생 중인 상태 정보
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        // normalizedTime: 0~1이 1회 재생, 루프면 1,2,3... 계속 증가
        float t = info.normalizedTime % 1f;

        // 전환 중이면 기다림
        if (anim.IsInTransition(0)) return;

        // 끝나기 직전에 다음 클립으로 크로스페이드
        if (t >= switchAtNormalizedTime)
        {
            PlayNext(immediate: false);
        }
    }

    void PlayNext(bool immediate)
    {
        if (bag.Count == 0) RefillBag();

        int next = DrawFromBag();

        // 연속 동일 방지(가능하면 한 번 더 뽑기)
        if (avoidSameTwice && stateNames.Count > 1 && next == lastIndex)
        {
            if (bag.Count == 0) RefillBag();
            next = DrawFromBag();
        }

        string nextState = stateNames[next];

        if (immediate)
            anim.Play(nextState, 0, 0f);
        else
            anim.CrossFadeInFixedTime(nextState, crossFadeTime, 0, 0f);

        lastIndex = currentIndex;
        currentIndex = next;
    }

    int DrawFromBag()
    {
        int pick = Random.Range(0, bag.Count);
        int idx = bag[pick];
        bag.RemoveAt(pick);
        return idx;
    }

    void RefillBag()
    {
        bag.Clear();
        for (int i = 0; i < stateNames.Count; i++)
            bag.Add(i);
    }
}
