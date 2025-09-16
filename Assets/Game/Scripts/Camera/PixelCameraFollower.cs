// Assets/Game/Scripts/Camera/PixelCameraFollower.cs
using UnityEngine;
using UnityEngine.U2D; // PixelPerfectCamera
using System.Collections;
using Game.Combat;     // Team

[DisallowMultipleComponent]
public sealed class PixelCameraFollower : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("���� �����ϸ� �ڵ� Ž���� �켱���� �ʽ��ϴ�.")]
    public Transform manualTarget;
    [Tooltip("�ڵ� Ž�� �� ������ ��")]
    public Team targetTeam = Team.Ally;
    [Tooltip("���� ���� �ڵ� Ž���� �õ��մϴ�.")]
    public bool autoAcquire = true;
    [Tooltip("���� �ڵ� Ž�� ����(���� �Ϸ� ���)")]
    public float firstAcquireDelay = 0.25f;
    [Tooltip("��Ž�� �ֱ�(Ÿ���� ���ų� ��� ��)")]
    public float reacquireInterval = 0.5f;

    [Header("Follow")]
    [Tooltip("ī�޶� Ÿ�� ��ġ�� �����ϴ� �ӵ�(Ŭ���� ����)")]
    public float followLerp = 10f;
    [Tooltip("Ÿ�� ���� ������(���� ����). �� ���� ���� ��鸲�� ����")]
    public Vector2 deadZone = new Vector2(0.1f, 0.06f);
    [Tooltip("Ÿ�� ���� �߰� ������(���� ����)")]
    public Vector2 followOffset = new Vector2(0f, 0.5f);
    [Tooltip("Time.timeScale ��ȭ(��Ʈ���� ��)�� ������ ���� �ʰ� �ε巴�� �̵�")]
    public bool useUnscaledTime = true;

    [Header("Pixel Perfect")]
    [Tooltip("�ȼ� ����Ʈ ī�޶�(���� �ڵ� �˻�)")]
    public PixelPerfectCamera pixelPerfect;
    [Tooltip("ī�޶� ��ġ�� �ȼ� �׸��忡 ����")]
    public bool snapToPixels = true;

    [Header("Bounds (�ɼ�)")]
    [Tooltip("���� ���� ī�޶� Ŭ�����մϴ�(���簢��).")]
    public bool clampToBounds = false;
    public Rect worldBounds = new Rect(-100, -100, 200, 200);

    Transform _target;
    Health _targetHealth; // ������ ��ȹ��
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

        // ��� ����(�ʱ� ��ġ Ƣ�� �� ����)
        if (_target)
            transform.position = ComputeSnappedPosition(GetDesiredPosition());
    }

    void OnTargetDeath()
    {
        UnsubscribeDeath();
        _target = null; // ���� �������� �ڵ� ��ȹ��
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

    // PixelCameraFollower.cs - FindFirstAliveOfTeam ��ü
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

        // ���� Ÿ�� �켱
        if (manualTarget && _target != manualTarget)
            SetTarget(manualTarget);

        if (!_target) return;

        Vector3 targetPos = GetDesiredPosition();
        Vector3 current = transform.position;

        // ������(���� ��鸲 ����)
        Vector2 delta = new Vector2(targetPos.x - current.x, targetPos.y - current.y);
        if (Mathf.Abs(delta.x) < deadZone.x) targetPos.x = current.x;
        if (Mathf.Abs(delta.y) < deadZone.y) targetPos.y = current.y;

        // �ε巴�� �̵�(���� ����)
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float k = 1f - Mathf.Exp(-followLerp * dt);
        Vector3 blended = Vector3.Lerp(current, targetPos, Mathf.Clamp01(k));

        // �ȼ� ���� + ��� Ŭ����
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
