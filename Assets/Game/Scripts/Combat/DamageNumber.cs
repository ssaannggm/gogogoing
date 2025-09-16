// Assets/Game/Scripts/Combat/DamageNumber.cs
using UnityEngine;
using TMPro;
using Game.Services;
using Poolable = Game.Services.IPoolable;  // ← 별칭으로 충돌 회피

[RequireComponent(typeof(RectTransform))]
public sealed class DamageNumber : MonoBehaviour, Poolable
{
    [SerializeField] TMP_Text label;
    [SerializeField] float rise = 1.0f;
    [SerializeField] float life = 0.8f;
    [SerializeField] AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    float _t; Vector3 _start;

    void Awake() { if (!label) label = GetComponentInChildren<TMP_Text>(true); }

    public void Show(int amount, bool crit)
    {
        label.text = crit ? $"<b>{amount}</b>" : amount.ToString();
        label.color = crit ? new Color(1f, 0.9f, 0.2f, 1f) : new Color(1f, 0.2f, 0.2f, 1f);
        _t = 0f; _start = transform.position;
    }

    void Update()
    {
        _t += Time.deltaTime;
        float k = Mathf.Clamp01(_t / life);
        transform.position = _start + Vector3.up * rise * k;
        var c = label.color; c.a = alphaCurve.Evaluate(k); label.color = c;
        if (_t >= life) ObjectPool.I.Return(gameObject);
    }

    public void OnSpawned() { }
    public void OnDespawned() { }
}
