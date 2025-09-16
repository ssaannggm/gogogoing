// StatTestRunner.cs (새 파일)
using UnityEngine;
using Game.Combat;

public class StatTestRunner : MonoBehaviour
{
    [SerializeField] private UnitStats ally;
    [SerializeField] private UnitStats enemy;

    void Start()
    {
        // 1. 각 유닛의 Health 컴포넌트 초기화
        //var allyHealth = ally.GetComponent<Health>();
        //var enemyHealth = enemy.GetComponent<Health>();

        //allyHealth.Initialize(Mathf.RoundToInt(ally.maxHp));
        //enemyHealth.Initialize(Mathf.RoundToInt(enemy.maxHp));

        // 2. 각 유닛의 타겟 레이어 설정
        var allyWeapon = ally.GetComponentInChildren<MeleeWeapon>();
        var allyTargeter = ally.GetComponent<Targeter>();
        if (allyWeapon) allyWeapon.UpdateTargetMaskByTeam(ally.team);
        if (allyTargeter) allyTargeter.UpdateTargetMaskByTeam(ally.team);

        var enemyWeapon = enemy.GetComponentInChildren<MeleeWeapon>();
        var enemyTargeter = enemy.GetComponent<Targeter>();
        if (enemyWeapon) enemyWeapon.UpdateTargetMaskByTeam(enemy.team);
        if (enemyTargeter) enemyTargeter.UpdateTargetMaskByTeam(enemy.team);
    }
}