// Assets/Game/Scripts/Battle/BattlePrewarm.cs
using UnityEngine;
using Game.Services;
using Game.Combat;

public sealed class BattlePrewarm : MonoBehaviour
{
    [SerializeField] HitEffectSO[] effects;

    void Start()
    {
        if (effects == null) return;
        foreach (var e in effects)
        {
            if (!e) continue;
            if (e.vfxPrefab) ObjectPool.I.Prewarm(e.vfxPrefab, 16);
            if (e.damageNumberPrefab) ObjectPool.I.Prewarm(e.damageNumberPrefab, 8);
        }
    }
}
