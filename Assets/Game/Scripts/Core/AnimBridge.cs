// Assets/Game/Scripts/Core/AnimBridge.cs
using UnityEngine;
using System;

[DefaultExecutionOrder(-50)] // AI���� ���� �ʱ�ȭ
[DisallowMultipleComponent]
public class AnimBridge : MonoBehaviour
{
    [Header("Auto-wire if empty")]
    public SPUM_Prefabs spum;
    public Animator anim;

    [Header("Options")]
    [SerializeField] bool playIdleOnAwake = true;

    public Action OnHitEvent; // �ִϸ��̼� �̺�Ʈ���� ȣ��

    void Awake()
    {
        // 1) Animator: ���� ������Ʈ �켱 �� �ڽ� �˻�
        if (!anim) anim = GetComponent<Animator>();
        if (!anim) anim = GetComponentInChildren<Animator>(true);

        // 2) SPUM: �θ𿡼� �켱 �˻� �� (���ٸ�) �ڽ� �˻�
        if (!spum) spum = GetComponentInParent<SPUM_Prefabs>(true);
        if (!spum) spum = GetComponentInChildren<SPUM_Prefabs>(true);

        // 3) SPUM �ʱ�ȭ(������-����)
        if (spum != null)
        {
            if (!spum.allListsHaveItemsExist())
                spum.PopulateAnimationLists();

            spum.OverrideControllerInit();

            if (playIdleOnAwake && spum.IDLE_List != null && spum.IDLE_List.Count > 0)
                spum.PlayAnimation(PlayerState.IDLE, 0);
        }
    }

    // ����+�ε��� ��� (SPUM ����)
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

    // �̵� Bool (SPUM ��Ʈ�ѷ��� "1_Move"�� ��Ī)
    public void SetMove(bool v)
    {
        if (anim) anim.SetBool("1_Move", v);
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ���� �޼��� �̸�(��: "Anim_Hit")�� ����
    public void Anim_Hit() => OnHitEvent?.Invoke();
}
