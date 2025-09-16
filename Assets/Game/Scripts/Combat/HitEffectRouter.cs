// Assets/Game/Scripts/Combat/HitEffectRouter.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List 사용을 위해 추가
using UnityEngine.Rendering;
using UnityEngine.U2D;
using Game.Combat;
using Game.Services;

[RequireComponent(typeof(Health))]
[DisallowMultipleComponent]
public sealed class HitEffectRouter : MonoBehaviour
{
    [Header("Effect Map")]
    public HitEffectSO physicalEffect;
    public HitEffectSO magicalEffect;
    public HitEffectSO trueEffect;

    [Header("Anchors")]
    public Transform vfxAnchor;
    public SimpleCameraShaker cameraShaker;

    [Header("Sorting (옵션)")]
    public bool matchSortingGroupForVfx = true;
    public int vfxOrderOffset = +10;

    private Health _hp;
    private SortingGroup _sg;
    private SpriteRenderer[] _groupSprites;
    private MultiSpriteFlashOnHit _multiFlash;
    private SpriteFlashOnHit _singleFlash;
    private PixelPerfectCamera _ppc;

    void Awake()
    {
        _hp = GetComponent<Health>();
        _sg = GetComponent<SortingGroup>()
           ?? GetComponentInChildren<SortingGroup>(true)
           ?? GetComponentInParent<SortingGroup>(true);
        _multiFlash = GetComponentInChildren<MultiSpriteFlashOnHit>(true);
        _singleFlash = GetComponentInChildren<SpriteFlashOnHit>(true);
        _groupSprites = CollectGroupSprites();
        if (!cameraShaker) cameraShaker = FindFirstObjectByType<SimpleCameraShaker>();
        _ppc = FindFirstObjectByType<PixelPerfectCamera>();
    }

    // [수정] Health 스크립트의 변경된 이벤트 이름(OnHitReceived)을 구독합니다.
    void OnEnable() { _hp.OnHitReceived += OnHitReceived; }
    void OnDisable() { _hp.OnHitReceived -= OnHitReceived; }

    // [수정] 이벤트에 맞춰 함수 이름도 OnDamaged에서 OnHitReceived로 변경합니다.
    void OnHitReceived(HitInfo hit)
    {
        var def = Select(hit.damageType);
        if (def == null) return;

        var pos = ResolveVfxWorldPosition(hit, def);

        // ===== VFX =====
        if (def.vfxPrefab)
        {
            var vfx = ObjectPool.I.Rent(def.vfxPrefab, pos, Quaternion.identity);
            if (matchSortingGroupForVfx) ApplySortingToVfx(vfx);
            vfx.transform.localScale = Vector3.one * Mathf.Max(0.01f, def.vfxScale);
            StartCoroutine(ReturnLater(vfx, def.vfxLifetime));
        }

        // ===== SFX =====
        if (def.sfx)
            AudioOneShot.PlayAt(def.sfx, pos, def.volume, def.pitch, def.pitchJitter, def.mixerGroup, def.maxInstances, def.instanceCooldown);

        // ===== Flash =====
        if (def.flashDuration > 0f)
        {
            if (_multiFlash) _multiFlash.Flash(def.flashColor, def.flashDuration);
            else if (_singleFlash) _singleFlash.Flash(def.flashColor, def.flashDuration);
        }

        // ===== Hitstop / Shake =====
        if (def.hitstop > 0f) StartCoroutine(Hitstop(def.hitstop));
        if (cameraShaker && def.shakeAmplitude > 0f)
            cameraShaker.Shake(def.shakeAmplitude, def.shakeDuration);

        // ===== Damage number =====
        if (def.damageNumberPrefab && hit.amount > 0)
        {
            // TODO: 데미지 폰트를 생성하는 로직을 이곳으로 옮겨와야 합니다.
            // 예시:
            // var dnGO = ObjectPool.I.Rent(def.damageNumberPrefab, pos, Quaternion.identity);
            // var dnScript = dnGO.GetComponent<DamageNumber>();
            // if (dnScript != null) dnScript.Show(hit.amount);
            // StartCoroutine(ReturnLater(dnGO, def.damageNumberLifetime));
        }
    }

    HitEffectSO Select(DamageType t) =>
        t == DamageType.Magical ? magicalEffect :
        t == DamageType.True ? trueEffect :
                                physicalEffect;

    Vector3 ResolveVfxWorldPosition(in HitInfo hit, HitEffectSO def)
    {
        Vector3 basePos;
        if (vfxAnchor) basePos = vfxAnchor.position;
        else if (hit.point != Vector3.zero) basePos = hit.point;
        else basePos = BoundsCenterOrTransform();
        basePos += new Vector3(def.vfxOffset.x, def.vfxOffset.y, 0f);
        if (def.snapOffsetToPixel && _ppc && _ppc.assetsPPU > 0)
        {
            float u = 1f / _ppc.assetsPPU;
            basePos.x = Mathf.Round(basePos.x / u) * u;
            basePos.y = Mathf.Round(basePos.y / u) * u;
        }
        return basePos;
    }

    Vector3 BoundsCenterOrTransform()
    {
        var sprites = _groupSprites;
        if (sprites != null && sprites.Length > 0)
        {
            bool found = false;
            Bounds b = default;
            for (int i = 0; i < sprites.Length; i++)
            {
                var r = sprites[i];
                if (!r || !r.enabled || r.sprite == null) continue;
                if (!found) { b = r.bounds; found = true; }
                else b.Encapsulate(r.bounds);
            }
            if (found) return b.center;
        }
        return transform.position;
    }

    SpriteRenderer[] CollectGroupSprites()
    {
        var all = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        if (_sg == null) return all;

        var list = new List<SpriteRenderer>(all.Length);
        for (int i = 0; i < all.Length; i++)
        {
            var sr = all[i];
            if (!sr) continue;
            if (sr.GetComponentInParent<SortingGroup>() == _sg)
                list.Add(sr);
        }
        return list.ToArray();
    }

    void ApplySortingToVfx(GameObject vfx)
    {
        if (!_sg || !vfx) return;
        int layerId = _sg.sortingLayerID;
        int order = _sg.sortingOrder + vfxOrderOffset;

        var renderers = vfx.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            r.sortingLayerID = layerId;
            r.sortingOrder = order;
        }
    }

    IEnumerator Hitstop(float sec)
    {
        var prev = Time.timeScale;
        Time.timeScale = 0f; // 히트스톱 효과를 더 강하게
        yield return new WaitForSecondsRealtime(sec);
        Time.timeScale = prev;
    }

    IEnumerator ReturnLater(GameObject go, float sec)
    {
        yield return new WaitForSecondsRealtime(sec);
        if (go) ObjectPool.I.Return(go);
    }
}