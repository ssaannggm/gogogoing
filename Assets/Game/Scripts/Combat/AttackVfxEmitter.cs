using UnityEngine;
using System.Collections;
using Game.Items;
using Game.Services; // ObjectPool, AudioOneShot 등을 사용하기 위해

[DisallowMultipleComponent]
public sealed class AttackVfxEmitter : MonoBehaviour
{
    [Header("Anchors (비우면 자기 자신)")]
    public Transform rightAnchor;
    public Transform leftAnchor;

    [Header("Aim (선택)")]
    [Tooltip("공격 시점을 향할 목표. 없으면 속도/페이싱으로 Z각을 결정합니다.")]
    public Transform aimTarget;
    [Tooltip("속도로 좌/우를 추정하고 싶을 때(없으면 무시).")]
    public Rigidbody2D ownerRb;

    [Header("Runtime (Loadout에서 주입)")]
    public ItemSO.AttackEffectDef rightDef = ItemSO.AttackEffectDef.Default;
    public ItemSO.AttackEffectDef leftDef = ItemSO.AttackEffectDef.Default;

    [Header("Culling")]
    public bool skipWhenOffscreen = true;
    public float maxEmitDistance = 30f;

    private Camera _cam;

    void Awake()
    {
        if (!_cam) _cam = Camera.main;
        AutoWire();
    }

    void AutoWire()
    {
        if (!rightAnchor) rightAnchor = transform;
        if (!leftAnchor) leftAnchor = transform;
    }

    public void SetAimTarget(Transform t) => aimTarget = t;

    public void EmitRight() => Emit(in rightDef, rightAnchor);
    public void EmitLeft() => Emit(in leftDef, leftAnchor);

    // AttackVfxEmitter.cs의 Emit 함수를 이걸로 교체
    void Emit(in ItemSO.AttackEffectDef def, Transform anchor)
    {
        Debug.Log("1. Emit 함수 시작됨.");

        if (skipWhenOffscreen && _cam)
        {
            Vector3 p = (anchor ? anchor.position : transform.position);
            if ((p - _cam.transform.position).sqrMagnitude > maxEmitDistance * maxEmitDistance)
            {
                Debug.LogWarning("-> 중단 이유: 화면 밖(maxEmitDistance)으로 판단되어 컬링됨.");
                return;
            }
        }

        Debug.Log("2. 화면 컬링 통과.");

        if (def.vfxPrefab == null && def.sfx == null)
        {
            Debug.LogWarning("-> 중단 이유: ItemSO의 AttackEffect에 vfxPrefab과 sfx가 모두 비어있음.");
            return;
        }

        Debug.Log("3. 데이터 존재 확인 통과.");

        var parent = anchor ? anchor : transform;

        // ── VFX ──
        if (def.vfxPrefab != null)
        {
            // ObjectPool이 준비되었는지 최종 확인
            if (ObjectPool.I == null)
            {
                Debug.LogError("-> 중단 이유: ObjectPool.I가 준비되지 않았습니다! 씬에 ObjectPool이 있는지, 실행 순서가 맞는지 확인하세요.");
                return;
            }

            Debug.Log($"4. vfxPrefab({def.vfxPrefab.name})이 존재함 -> ObjectPool.Rent 시도!");

            GameObject vfxInstance = ObjectPool.I.Rent(def.vfxPrefab, parent.position, parent.rotation);
            if (vfxInstance != null)
            {
                Debug.Log("<color=green>5. VFX 인스턴스 생성 성공!</color>");
            }
            else
            {
                Debug.LogError("-> 중단 이유: ObjectPool.I.Rent가 null을 반환했습니다. 풀에 문제가 있을 수 있습니다.");
                return;
            }

            Transform t = vfxInstance.transform;

            if (def.attachToAnchor)
            {
                t.SetParent(parent, true);
            }
            else
            {
                if (ObjectPool.I) t.SetParent(ObjectPool.I.transform, true);
                else t.SetParent(null, true);
            }

            t.localPosition = def.localPosition;
            t.localScale = def.localScale;

            float zAngle = 0f;
            if (def.alignToAim)
            {
                Vector2 dir = ResolveAimDirection(parent);
                if (dir.sqrMagnitude < 1e-6f) dir = Vector2.right * Mathf.Sign(parent.lossyScale.x);
                zAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + def.angleOffset;
            }
            t.localRotation = Quaternion.Euler(def.localEuler.x, def.localEuler.y, zAngle + def.localEuler.z);

            if (def.matchSortingGroup)
            {
                var sg = GetComponentInParent<UnityEngine.Rendering.SortingGroup>();
                if (sg)
                {
                    var renderers = vfxInstance.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        r.sortingLayerID = sg.sortingLayerID;
                        r.sortingOrder = sg.sortingOrder + def.orderOffset;
                    }
                }
            }

            var ps = vfxInstance.GetComponentInChildren<ParticleSystem>(true);
            if (ps)
            {
                ps.Clear(true);
                ps.Play(true);
            }

            if (!def.attachToAnchor && def.vfxLifetime > 0f)
            {
                StartCoroutine(ReturnToPoolAfter(vfxInstance, def.vfxLifetime));
            }
        }
        else
        {
            Debug.LogWarning("-> VFX 생성 안 됨: vfxPrefab이 비어있어서 SFX만 재생합니다.");
        }

        // ── SFX ──
        if (def.sfx)
        {
            Debug.Log("SFX 재생 시도!");
            var pos = (anchor ? anchor.position : transform.position);
            AudioOneShot.PlayAt(def.sfx, pos, def.volume, def.pitch, def.pitchJitter,
                                def.mixerGroup, def.maxInstances, def.instanceCooldown);
        }
    }

    Vector2 ResolveAimDirection(Transform anchor)
    {
        Vector3 origin = anchor ? anchor.position : transform.position;
        if (aimTarget)
        {
            Vector2 d = (Vector2)(aimTarget.position - origin);
            if (d.sqrMagnitude > 1e-6f) return d.normalized;
        }
        if (ownerRb && ownerRb.linearVelocity.sqrMagnitude > 1e-6f)
            return ownerRb.linearVelocity.normalized;

        float sign = Mathf.Sign((anchor ? anchor.lossyScale.x : transform.lossyScale.x));
        return new Vector2(sign >= 0f ? 1f : -1f, 0f);
    }

    IEnumerator ReturnToPoolAfter(GameObject go, float sec)
    {
        yield return new WaitForSeconds(sec);
        if (go) ObjectPool.I.Return(go);
    }
}