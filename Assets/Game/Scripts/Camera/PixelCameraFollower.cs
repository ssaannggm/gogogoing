// Assets/Game/Scripts/Camera/PixelCameraFollower.cs
using UnityEngine;
using UnityEngine.U2D; // PixelPerfectCamera
using System.Collections;
using Game.Combat;     // Team

[DisallowMultipleComponent]
public sealed class PixelCameraFollower : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("직접 지정하면 자동 탐색을 우선하지 않습니다.")]
    public Transform manualTarget;
    [Tooltip("자동 탐색 시 추적할 팀")]
    public Team targetTeam = Team.Ally;
    [Tooltip("스폰 직후 자동 탐색을 시도합니다.")]
    public bool autoAcquire = true;
    [Tooltip("최초 자동 탐색 지연(스폰 완료 대기)")]
    public float firstAcquireDelay = 0.25f;
    [Tooltip("재탐색 주기(타깃이 없거나 사망 시)")]
    public float reacquireInterval = 0.5f;

    [Header("Follow")]
    [Tooltip("카메라가 타깃 위치에 수렴하는 속도(클수록 빠름)")]
    public float followLerp = 10f;
    [Tooltip("타깃 주위 데드존(월드 유닛). 이 범위 안의 흔들림은 무시")]
    public Vector2 deadZone = new Vector2(0.1f, 0.06f);
    [Tooltip("타깃 기준 추가 오프셋(월드 유닛)")]
    public Vector2 followOffset = new Vector2(0f, 0.5f);
    [Tooltip("Time.timeScale 변화(히트스톱 등)의 영향을 받지 않고 부드럽게 이동")]
    public bool useUnscaledTime = true;

    [Header("Pixel Perfect")]
    [Tooltip("픽셀 퍼펙트 카메라(비우면 자동 검색)")]
    public PixelPerfectCamera pixelPerfect;
    [Tooltip("카메라 위치를 픽셀 그리드에 스냅")]
    public bool snapToPixels = true;

    [Header("Bounds (옵션)")]
    [Tooltip("월드 경계로 카메라를 클램프합니다(직사각형).")]
    public bool clampToBounds = false;
    public Rect worldBounds = new Rect(-100, -100, 200, 200);

    Transform _target;
    Health _targetHealth; // 죽으면 재획득
    Camera _cam;

    void Awake()
    {
        _cam = GetComponentInChildren<Camera>() ?? Camera.main;
        if (!pixelPerfect) pixelPerfect = FindFirstObjectByType<PixelPerfectCamera>();
    }

    void OnEnable()
    {
        if (autoAcquire)
            StartCoroutine(Co_AutoAcquireLoop());
        else
            SetTarget(manualTarget ? manualTarget : _target);
    }

    void OnDisable()
    {
        UnsubscribeDeath();
    }

    public void SetTarget(Transform t)
    {
        if (_target == t) return;

        UnsubscribeDeath();

        _target = t;
        _targetHealth = _target ? _target.GetComponentInParent<Health>() : null;
        if (_targetHealth != null)
            _targetHealth.OnDeath += OnTargetDeath;

        // 즉시 스냅(초기 위치 튀는 것 방지)
        if (_target)
            transform.position = ComputeSnappedPosition(GetDesiredPosition());
    }

    void OnTargetDeath()
    {
        UnsubscribeDeath();
        _target = null; // 다음 루프에서 자동 재획득
    }

    void UnsubscribeDeath()
    {
        if (_targetHealth != null)
            _targetHealth.OnDeath -= OnTargetDeath;
        _targetHealth = null;
    }

    IEnumerator Co_AutoAcquireLoop()
    {
        if (firstAcquireDelay > 0f)
            yield return new WaitForSeconds(firstAcquireDelay);

        var wait = new WaitForSeconds(reacquireInterval);
        while (enabled)
        {
            if (!_target)
            {
                var found = FindFirstAliveOfTeam(targetTeam);
                if (found) SetTarget(found);
            }
            yield return wait;
        }
    }

    // PixelCameraFollower.cs - FindFirstAliveOfTeam 교체
    Transform FindFirstAliveOfTeam(Team team)
    {
        var st = UnitRegistry.FirstAlive(team);
        return st ? GetCameraAnchor(st.transform) : null;
    }


    Transform GetCameraAnchor(Transform root)
    {
        if (!root) return null;
        var anchor = root.Find("CameraTargetAnchor");
        return anchor != null ? anchor : root;
    }

    void LateUpdate()
    {
        if (!_cam) return;

        // 수동 타깃 우선
        if (manualTarget && _target != manualTarget)
            SetTarget(manualTarget);

        if (!_target) return;

        Vector3 targetPos = GetDesiredPosition();
        Vector3 current = transform.position;

        // 데드존(작은 흔들림 무시)
        Vector2 delta = new Vector2(targetPos.x - current.x, targetPos.y - current.y);
        if (Mathf.Abs(delta.x) < deadZone.x) targetPos.x = current.x;
        if (Mathf.Abs(delta.y) < deadZone.y) targetPos.y = current.y;

        // 부드럽게 이동(지수 보간)
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float k = 1f - Mathf.Exp(-followLerp * dt);
        Vector3 blended = Vector3.Lerp(current, targetPos, Mathf.Clamp01(k));

        // 픽셀 스냅 + 경계 클램프
        Vector3 snapped = ComputeSnappedPosition(blended);
        if (clampToBounds) snapped = ClampToWorldBounds(snapped);

        transform.position = snapped;
    }

    Vector3 GetDesiredPosition()
    {
        if (!_target) return transform.position;

        Vector3 pos = _target.position + (Vector3)followOffset;
        pos.z = (_cam && _cam.orthographic) ? _cam.transform.position.z : transform.position.z;
        return pos;
    }

    Vector3 ComputeSnappedPosition(Vector3 pos)
    {
        if (!snapToPixels || pixelPerfect == null || pixelPerfect.assetsPPU <= 0)
            return pos;

        float u = 1f / pixelPerfect.assetsPPU;
        pos.x = Mathf.Round(pos.x / u) * u;
        pos.y = Mathf.Round(pos.y / u) * u;
        return pos;
    }

    Vector3 ClampToWorldBounds(Vector3 pos)
    {
        if (!_cam || !_cam.orthographic) return pos;

        float halfH = _cam.orthographicSize;
        float halfW = halfH * _cam.aspect;

        float minX = worldBounds.xMin + halfW;
        float maxX = worldBounds.xMax - halfW;
        float minY = worldBounds.yMin + halfH;
        float maxY = worldBounds.yMax - halfH;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }
}
