// Assets/Game/Scripts/Core/AnimBridge.cs
using UnityEngine;
using System;

[DefaultExecutionOrder(-50)] // AI보다 먼저 초기화
[DisallowMultipleComponent]
public class AnimBridge : MonoBehaviour
{
    [Header("Auto-wire if empty")]
    public SPUM_Prefabs spum;
    public Animator anim;

    [Header("Options")]
    [SerializeField] bool playIdleOnAwake = true;

    public Action OnHitEvent; // 애니메이션 이벤트에서 호출

    void Awake()
    {
        // 1) Animator: 같은 오브젝트 우선 → 자식 검색
        if (!anim) anim = GetComponent<Animator>();
        if (!anim) anim = GetComponentInChildren<Animator>(true);

        // 2) SPUM: 부모에서 우선 검색 → (없다면) 자식 검색
        if (!spum) spum = GetComponentInParent<SPUM_Prefabs>(true);
        if (!spum) spum = GetComponentInChildren<SPUM_Prefabs>(true);

        // 3) SPUM 초기화(스프링-안전)
        if (spum != null)
        {
            if (!spum.allListsHaveItemsExist())
                spum.PopulateAnimationLists();

            spum.OverrideControllerInit();

            if (playIdleOnAwake && spum.IDLE_List != null && spum.IDLE_List.Count > 0)
                spum.PlayAnimation(PlayerState.IDLE, 0);
        }
    }

    // 상태+인덱스 재생 (SPUM 경유)
    public void PlayAnimation(PlayerState state, int index = 0)
    {
        if (spum == null) return;

        if (!spum.StateAnimationPairs.TryGetValue(state.ToString(), out var list) ||
            list == null || list.Count == 0) return;

        index = Mathf.Clamp(index, 0, list.Count - 1);
        spum.PlayAnimation(state, index);
    }

    public int GetAnimCount(PlayerState state)
    {
        if (spum == null) return 0;
        return spum.StateAnimationPairs.TryGetValue(state.ToString(), out var list) && list != null
            ? list.Count : 0;
    }

    // 이동 Bool (SPUM 컨트롤러의 "1_Move"와 매칭)
    public void SetMove(bool v)
    {
        if (anim) anim.SetBool("1_Move", v);
    }

    // 애니메이션 이벤트에서 호출할 메서드 이름(예: "Anim_Hit")로 연결
    public void Anim_Hit() => OnHitEvent?.Invoke();
}
