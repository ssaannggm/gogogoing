// Assets/Game/Scripts/Combat/Health.cs (수정 완료된 최종 코드)
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

    // 외부에서 현재 체력을 안전하게 읽을 수 있는 public 프로퍼티
    public float CurrentHP { get; private set; }
    public bool IsDead => _isDead;

    private UnitStats _stats;
    private AnimBridge _anim;
    private bool _isDead = false;

    void Awake()
    {
        _stats = GetComponent<UnitStats>();
        _anim = GetComponentInChildren<AnimBridge>(true);
    }

    // Start에서 UnitStats의 최종 계산된 maxHp를 가져와 초기화합니다.
    void Start()
    {
        // UnitStats의 Awake -> RecalculateStats()가 먼저 실행된 후,
        // Start에서 그 최종값을 가져옵니다.
        CurrentHP = _stats.CurrentMaxHp;
        OnHealthChanged?.Invoke(CurrentHP, _stats.CurrentMaxHp);
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
        OnHealthChanged?.Invoke(CurrentHP, _stats.CurrentMaxHp);

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (_isDead || healAmount <= 0) return;

        // 최대 체력을 넘지 않도록 _stats.CurrentMaxHp를 기준으로 제한합니다.
        CurrentHP = Mathf.Min(_stats.CurrentMaxHp, CurrentHP + healAmount);

        OnHealthChanged?.Invoke(CurrentHP, _stats.CurrentMaxHp);
    }

    private void Die()
    {
        if (_isDead) return; // 중복 실행 방지
        _isDead = true;

        OnDeath?.Invoke();
        _anim?.PlayAnimation(PlayerState.DEATH, 0);
    }
}