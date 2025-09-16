// Assets/Game/Scripts/Core/AIController.cs
using UnityEngine;
using Game.Combat;
using Game.Battle; // BattleManager를 사용하기 위해 추가
public enum AIState { Idle, Chase, Attack, Recover, Death }

[RequireComponent(typeof(UnitStats), typeof(Health))]
public class AIController : MonoBehaviour
{
    public Team targetTeam = Team.Enemy;

    [Header("Targeting")]
    public float retargetInterval = 0.3f;

    [Header("Facing")]
    public Transform visualRoot;
    public bool autoCaptureBaseScale = true;
    public float faceThreshold = 0.01f;
    public bool invertFacing = false;

    // --- ���� ���� ���� ---
    private UnitStats _stats;
    private Health _health;
    private Mover2D _mover;
    private Targeter _targeter;
    private AnimBridge _animBridge;
    private IWeapon _weapon;
    private Rigidbody2D _rb;

    private Transform _target;
    private AIState _state = AIState.Idle;

    // [����] ���� ��ٿ�(cd)�� AIController�� ���� �����մϴ�.
    private float _attackCooldownTimer;

    private bool _wantsToMove;
    private Vector2 _moveTarget;

    private Vector3 _visualBaseScale = Vector3.one;
    private int _faceSign = 1;

    private float _regenTimer;

    void Awake()
    {
        // ������Ʈ ���� ��������
        _stats = GetComponent<UnitStats>();
        _health = GetComponent<Health>();
        _mover = GetComponent<Mover2D>();
        _targeter = GetComponent<Targeter>();
        _animBridge = GetComponentInChildren<AnimBridge>(true);
        _weapon = GetComponentInChildren<IWeapon>(true);
        _rb = GetComponent<Rigidbody2D>();

        if (!visualRoot && _animBridge) visualRoot = _animBridge.transform;
        if (!visualRoot) visualRoot = transform;
        if (autoCaptureBaseScale && visualRoot) _visualBaseScale = visualRoot.localScale;

        // �̺�Ʈ ����
        if (_health != null) _health.OnDeath += OnDeath;
        if (_animBridge != null) _animBridge.OnHitEvent += OnHit;

        InvokeRepeating(nameof(Retarget), 0f, retargetInterval);
    }

    void OnDestroy()
    {
        if (_health != null) _health.OnDeath -= OnDeath;
        if (_animBridge != null) _animBridge.OnHitEvent -= OnHit;
    }

    void Start()
    {
        Retarget();
    }

    void Retarget()
    {
        if (_state == AIState.Death) return;
        _target = _targeter ? _targeter.FindNearestEnemy() : null;
    }

    void Update()
    {
        // --- ✨ 여기가 핵심! '신호등' 추가 ---
        // BattleManager가 없거나, 상태가 'Fighting'이 아니면 아무것도 하지 않고 대기
        if (BattleManager.Instance == null || BattleManager.Instance.State != BattleState.Fighting)
        {
            _mover?.Stop(); // 혹시 움직이고 있었다면 멈추게 함
            return;
        }
        // --- 수정 끝 ---

        if (_state == AIState.Death) return;

        // ���� ��ٿ� Ÿ�̸� ����
        if (_attackCooldownTimer > 0f)
        {
            _attackCooldownTimer -= Time.deltaTime;
        }

        _wantsToMove = false;

        // ���� �ӽ�
        switch (_state)
        {
            case AIState.Idle:
                _animBridge?.SetMove(false);
                if (_target) _state = AIState.Chase;
                break;

            case AIState.Chase:
                if (!_target)
                {
                    _state = AIState.Idle;
                    break;
                }

                FaceTo(_target.position - transform.position);

                // [����] UnitStats�� ���� ���ǵ� CurrentAttackRange�� ����մϴ�.
                float rangeSqr = _stats.CurrentAttackRange * _stats.CurrentAttackRange;
                float distSqr = (_target.position - transform.position).sqrMagnitude;

                if (distSqr > rangeSqr)
                {
                    _wantsToMove = true;
                    _moveTarget = _target.position;
                    _animBridge?.SetMove(true);
                }
                else
                {
                    EnterAttack();
                }
                break;

            case AIState.Attack:
                // ���� �ִϸ��̼��� ���� ������ ��� (OnAttackAnimationEnd �Լ��� Recover ���·� ��ȯ)
                if (_target) FaceTo(_target.position - transform.position);
                break;

            case AIState.Recover:
                _animBridge?.SetMove(false);

                // ��ٿ��� ������ ���� �ൿ ����
                if (_attackCooldownTimer <= 0f)
                {
                    if (!_target)
                    {
                        _state = AIState.Idle;
                    }
                    else
                    {
                        // ��Ÿ� �ȿ� ������ �ٽ� ����, �ƴϸ� �߰�
                        float currentRangeSqr = _stats.CurrentAttackRange * _stats.CurrentAttackRange;
                        if ((_target.position - transform.position).sqrMagnitude <= currentRangeSqr)
                        {
                            EnterAttack();
                        }
                        else
                        {
                            _state = AIState.Chase;
                        }
                    }
                }
                break;
        }

        // --- ü�� ��� ���� ---
        if (_state != AIState.Death && _stats.CurrentHealthRegen > 0)
        {
            _regenTimer += Time.deltaTime;
            if (_regenTimer >= 1f)
            {
                _health.Heal(_stats.CurrentHealthRegen);
                _regenTimer -= 1f;
            }
        }
    }

    void FixedUpdate()
    {
        if (_state == AIState.Death) return;
        if (_wantsToMove) _mover?.MoveTowards(_moveTarget);
        else _mover?.Stop();
    }

    void EnterAttack()
    {
        if (_attackCooldownTimer > 0f)
        {
            _state = AIState.Recover;
            return;
        }

        _wantsToMove = false;
        _mover?.Stop();
        if (_rb) _rb.linearVelocity = Vector2.zero;

        if (_target) FaceTo(_target.position - transform.position);

        _animBridge?.SetMove(false);

        // ����Ʈ�� ��ǥ�� ������ �� �ְ� ����
        var emitter = GetComponentInChildren<AttackVfxEmitter>(true);
        if (emitter) emitter.SetAimTarget(_target);

        // �ִϸ��̼� ���
        _animBridge?.PlayAnimation(PlayerState.ATTACK, 0); // TODO: ���� �ε��� �ʿ� �� �߰�
        _state = AIState.Attack;

        // [����] ���� ���� ��ٿ� ��� (attackSpeed�� 2�̸� 0.5��)
        _attackCooldownTimer = 1f / _stats.CurrentAttackSpeed;
    }

    void FaceTo(Vector2 dir)
    {
        if (!visualRoot) return;
        if (Mathf.Abs(dir.x) < faceThreshold) return;

        int desired = dir.x >= 0f ? 1 : -1;
        if (invertFacing) desired *= -1;
        if (desired == _faceSign) return;

        _faceSign = desired;
        var s = _visualBaseScale;
        s.x = Mathf.Abs(s.x) * _faceSign;
        visualRoot.localScale = s;
    }

    void OnHit()
    {
        _weapon?.TryHit(_stats, _target);
    }

    public void OnAttackAnimationEnd()
    {
        _state = AIState.Recover;
    }

    void OnDeath()
    {
        if (_state == AIState.Death) return;
        _state = AIState.Death;
        _wantsToMove = false;
        _mover?.Stop();
        if (_rb) _rb.linearVelocity = Vector2.zero;

        // �浹ü ��Ȱ��ȭ
        var cols = GetComponentsInChildren<Collider2D>();
        foreach (var col in cols) col.enabled = false;
        if (_rb) _rb.simulated = false;

        _animBridge?.PlayAnimation(PlayerState.DEATH, 0);

        // TODO: ������Ʈ Ǯ���� ����Ѵٸ� ReturnToPool ȣ��
        Destroy(gameObject, 2.0f);
        enabled = false;
    }

    void OnDisable()
    {
        CancelInvoke();
        _wantsToMove = false;
        _mover?.Stop();
    }
}