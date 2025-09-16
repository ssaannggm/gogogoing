// Assets/Game/Scripts/Core/Targeter.cs
using Game.Combat;
using UnityEngine;

[RequireComponent(typeof(UnitStats))]
public class Targeter : MonoBehaviour
{
    private UnitStats _selfStats;
    public LayerMask targetMask;

    void Awake()
    {
        _selfStats = GetComponent<UnitStats>();
    }

    public Transform FindNearestEnemy()
    {
        // 씬의 모든 UnitStats를 찾습니다.
        UnitStats[] allUnits = FindObjectsByType<UnitStats>(FindObjectsSortMode.None);

        float bestDistanceSqr = float.MaxValue;
        Transform pick = null;

        foreach (var unit in allUnits)
        {
            // --- ✨ 여기가 핵심 수정 부분 ✨ ---

            // 1. 상대방의 Health 컴포넌트를 가져옵니다.
            var unitHealth = unit.GetComponent<Health>();

            // 2. Health 컴포넌트가 없거나, 이미 죽었거나, 같은 팀이면 건너뜁니다.
            if (unitHealth == null || unitHealth.IsDead || unit.team == _selfStats.team)
            {
                continue;
            }

            // --- 로직 끝 ---

            float distanceSqr = (unit.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                pick = unit.transform;
            }
        }
        return pick;
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
}