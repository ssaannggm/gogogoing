// Assets/Game/Scripts/Combat/MeleeWeapon.cs
using UnityEngine;
using System.Reflection;
using Game.Combat;

public class MeleeWeapon : MonoBehaviour, IWeapon
{
    [Header("Attack Settings")]
    [Min(0f)] public float radius = 1f;

    [Header("Layer Mask")]
    [Tooltip("수동 지정 시 사용합니다. 0이면 아무것도 맞지 않습니다.")]
    public LayerMask targetMask;
    [Tooltip("Awake에서 오너의 팀/레이어를 보고 자동으로 targetMask를 설정합니다.")]
    public bool setMaskByTeamOnAwake = true;
    [Tooltip("아군/적 레이어 이름(프로젝트 레이어에 실제로 존재해야 합니다).")]
    public string allyLayerName = "Ally";
    public string enemyLayerName = "Enemy";

    [Header("Debug")]
    public bool enableDebug = true;

    UnitStats _ownerCached;

    void Awake()
    {
        _ownerCached = GetComponentInParent<UnitStats>();

        if (setMaskByTeamOnAwake && _ownerCached != null)
        {
            bool ownerIsAlly = IsOwnerAllyByLayer(_ownerCached.gameObject.layer);
            if (!ownerIsAlly && !IsOwnerEnemyByLayer(_ownerCached.gameObject.layer))
                ownerIsAlly = IsOwnerAllyByField(_ownerCached);

            string targetLayerName = ownerIsAlly ? enemyLayerName : allyLayerName;
            int layerIndex = LayerMask.NameToLayer(targetLayerName);

            if (layerIndex >= 0)
            {
                targetMask = 1 << layerIndex;
                if (enableDebug)
                    Debug.Log($"[MeleeWeapon] Auto targetMask -> '{targetLayerName}' (bit {layerIndex}) on {_ownerCached.name}", this);
            }
            else if (enableDebug)
            {
                Debug.LogWarning($"[MeleeWeapon] Layer '{targetLayerName}' 가 없습니다. Project Settings > Layers에서 생성하거나 targetMask를 수동 설정하세요.", this);
            }
        }

        if (targetMask.value == 0 && enableDebug)
            Debug.LogWarning($"[MeleeWeapon] targetMask=0. 현재 상태로는 어떤 것도 맞지 않습니다. setMaskByTeamOnAwake 또는 수동 설정을 확인하세요.", this);
    }

    public void TryHit(UnitStats ownerStats, Transform preferredTarget = null)
    {
        if (ownerStats == null)
        {
            // enableDebug 변수가 있다면 그대로 사용하시고, 없다면 if문을 지우셔도 됩니다.
            // if (enableDebug) Debug.LogWarning("[MeleeWeapon] TryHit(owner=null)", this);
            return;
        }

        if (targetMask.value == 0)
        {
            // if (enableDebug) Debug.LogWarning("[MeleeWeapon] targetMask=0 이라 탐지가 없습니다.", this);
            return;
        }

        Vector2 center = ownerStats.transform.position;
        var hits = Physics2D.OverlapCircleAll(center, radius, targetMask);

        // if (enableDebug)
        //     Debug.Log($"[MeleeWeapon] {ownerStats.name} TryHit -> {hits.Length} hits");

        foreach (var h in hits)
        {
            if (!h) continue;

            var targetStats = h.GetComponentInParent<UnitStats>();
            var targetHealth = h.GetComponentInParent<Health>();

            if (!targetStats || !targetHealth) continue;
            if (targetStats.team == ownerStats.team) continue; // IsSameTeam과 동일한 기능

            // --- ✨ 여기가 핵심 수정 부분 ✨ ---

            // 1. CombatCalculator에게 전투 결과 계산을 요청합니다.
            HitInfo hit = CombatCalculator.CalculateAttack(ownerStats, targetStats);

            // 2. 계산 결과에 따라 다른 처리를 합니다.
            switch (hit.outcome)
            {
                case HitOutcome.Evade:
                    Debug.Log($"<color=grey>[Attack] {ownerStats.name} -> {targetStats.name} | EVADED!</color>");
                    // TODO: 빗나감 사운드 또는 이펙트 재생
                    break;

                case HitOutcome.Block:
                case HitOutcome.Hit:
                case HitOutcome.Crit:
                    Debug.Log($"<color=white>[Attack] {ownerStats.name} -> {targetStats.name} | {hit.amount} DMG ({hit.outcome})</color>");
                    // 유효한 타격이므로, 계산된 HitInfo를 그대로 Health 컴포넌트에 전달합니다.
                    targetHealth.TakeDamage(hit);
                    // --- 모든 피해 흡혈 로직 ---
                    if (ownerStats.omnivamp > 0)
                    {
                        var ownerHealth = ownerStats.GetComponent<Health>();
                        if (ownerHealth != null)
                        {
                            float healAmount = hit.amount * (ownerStats.omnivamp / 100f);
                            ownerHealth.Heal(healAmount);
                        }
                    }
                    // --- 처치 시 생명력 회복 로직 ---
                    if (targetHealth.IsDead && ownerStats.lifeOnKill > 0)
                    {
                        var ownerHealth = ownerStats.GetComponent<Health>();
                        if (ownerHealth != null)
                        {
                            ownerHealth.Heal(ownerStats.lifeOnKill);
                            Debug.Log($"{ownerStats.name}이(가) 적을 처치하고 체력을 {ownerStats.lifeOnKill} 회복했습니다.");
                        }
                    }
                    break;
            }

            // --- 로직 끝 ---

            // 단일 타격 무기이므로, 유효한 대상(피격이든 회피든)을 한 명이라도 찾으면 공격을 멈춥니다.
            break;
        }
    }

    // Helpers
    bool IsOwnerAllyByLayer(int layer)
    {
        var name = LayerMask.LayerToName(layer);
        return !string.IsNullOrEmpty(name) && name == allyLayerName;
    }

    bool IsOwnerEnemyByLayer(int layer)
    {
        var name = LayerMask.LayerToName(layer);
        return !string.IsNullOrEmpty(name) && name == enemyLayerName;
    }

    bool IsOwnerAllyByField(UnitStats st)
    {
        try
        {
            var t = st.GetType();
            var f = t.GetField("team", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null)
            {
                var v = f.GetValue(st);
                if (v == null) return false;
                if (v is int i) return i == 0;
                return v.ToString() == "Ally";
            }
            var p = t.GetProperty("team", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanRead)
            {
                var v = p.GetValue(st);
                if (v == null) return false;
                if (v is int i) return i == 0;
                return v.ToString() == "Ally";
            }
        }
        catch { }
        return false;
    }

    static bool IsSameTeam(UnitStats a, UnitStats b)
    {
        var la = LayerMask.LayerToName(a.gameObject.layer);
        var lb = LayerMask.LayerToName(b.gameObject.layer);
        if (!string.IsNullOrEmpty(la) && !string.IsNullOrEmpty(lb))
            if (la == lb && (la == "Ally" || la == "Enemy")) return true;

        try
        {
            var fa = a.GetType().GetField("team"); var fb = b.GetType().GetField("team");
            if (fa != null && fb != null)
            {
                var va = fa.GetValue(a); var vb = fb.GetValue(b);
                if (va != null && vb != null) return Equals(va, vb);
            }
            var pa = a.GetType().GetProperty("team"); var pb = b.GetType().GetProperty("team");
            if (pa != null && pb != null && pa.CanRead && pb.CanRead)
            {
                var va = pa.GetValue(a); var vb = pb.GetValue(b);
                if (va != null && vb != null) return Equals(va, vb);
            }
        }
        catch { }
        return false;
    }

    static string MaskToString(LayerMask mask)
    {
        if (mask.value == 0) return "0 (none)";
        System.Text.StringBuilder sb = new();
        for (int i = 0; i < 32; i++)
        {
            if ((mask.value & (1 << i)) != 0)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(LayerMask.LayerToName(i)); sb.Append('[').Append(i).Append(']');
            }
        }
        return sb.ToString();
    }
    public void UpdateTargetMaskByTeam(Team ownerTeam)
    {
        if (ownerTeam == Team.Ally)
        {
            targetMask = LayerMask.GetMask("Enemy");
        }
        else // ownerTeam == Team.Enemy
        {
            targetMask = LayerMask.GetMask("Ally");
        }
    }

    void OnDrawGizmosSelected()
    {
        var owner = GetComponentInParent<UnitStats>();
        var center = owner ? owner.transform.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, radius);
    }
}
