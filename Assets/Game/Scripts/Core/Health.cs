// Health.cs - 최종 수정본 (복사/붙여넣기)
using System;
using UnityEngine;
using Game.Combat;

[RequireComponent(typeof(UnitStats))]
public sealed class Health : MonoBehaviour
{
    // 이벤트: (현재 체력, 최대 체력)
    public event Action<float, float> OnHealthChanged;
    public event Action<HitInfo> OnHitReceived;
    public event Action OnDeath;

    public float CurrentHP { get; private set; }
    public float MaxHP { get; private set; } // [추가] 최대 체력도 저장
    public bool IsDead => _isDead;

    private UnitStats _stats;
    private AnimBridge _anim; // SPUM 애니메이션 브릿지
    private bool _isDead = false;

    void Awake()
    {
        _stats = GetComponent<UnitStats>();
        _anim = GetComponentInChildren<AnimBridge>(true);
    }

    /// <summary>
    /// [수정] BattleManager가 호출할 초기화 함수. Start() 대신 사용합니다.
    /// </summary>
    public void Initialize(float maxHp)
    {
        MaxHP = maxHp;
        CurrentHP = MaxHP;
        _isDead = false;
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void TakeDamage(float damageAmount)
    {
        var basicHit = new HitInfo
        {
            amount = Mathf.RoundToInt(damageAmount)
        };
        TakeDamage(basicHit);
    }

    public void TakeDamage(HitInfo hit)
    {
        if (_isDead) return;

        float damageAmount = hit.amount;
        CurrentHP = Mathf.Max(0f, CurrentHP - damageAmount);

        OnHitReceived?.Invoke(hit);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP); // [수정] _stats.CurrentMaxHp -> MaxHP

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (_isDead || healAmount <= 0) return;

        // [수정] _stats.CurrentMaxHp -> MaxHP
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + healAmount);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        OnDeath?.Invoke();

        // PlayerState Enum에 맞게 수정이 필요할 수 있습니다.
        // 예: PlayerState.DEATH -> PlayerState.Die
        _anim?.PlayAnimation(PlayerState.DEATH, 0);
    }
}