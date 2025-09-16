using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Visual
{
    [DisallowMultipleComponent]
    public sealed class EquipmentSpriteGroups : MonoBehaviour
    {
        [Serializable]
        public sealed class SpriteGroup
        {
            public string key;
            public SpriteRenderer[] renderers;
            [HideInInspector] public Sprite[] defaults;
        }

        [Header("������ �׷�(���ϴ� ��ŭ �߰�)")]
        public SpriteGroup[] groups;

        [Header("�ɼ�")]
        [Tooltip("������ ��������Ʈ�� null�̸� ���� ��������Ʈ�� �����մϴ�.")]
        public bool keepOriginalIfNull = true;

        [Tooltip("Clear �� ĸó�ص� �⺻ ��������Ʈ�� �ǵ����ϴ�.")]
        public bool revertToDefaultOnClear = true;

        [Tooltip("����/���� ���� �� �α�")]
        public bool verboseLogging = false;

        Dictionary<string, SpriteGroup> _map;

        void Awake()
        {
            RebuildMap();
        }

        public void RebuildMap()
        {
            _map = new Dictionary<string, SpriteGroup>(StringComparer.Ordinal);
            if (groups == null) return;

            for (int i = 0; i < groups.Length; i++)
            {
                var g = groups[i];
                if (g == null || string.IsNullOrEmpty(g.key)) continue;
                _map[g.key] = g;

                if (g.renderers != null)
                {
                    g.defaults = new Sprite[g.renderers.Length];
                    for (int r = 0; r < g.renderers.Length; r++)
                        g.defaults[r] = g.renderers[r] ? g.renderers[r].sprite : null;
                }
            }
            if (verboseLogging)
                Debug.Log($"[EquipGroups] RebuildMap: {_map.Count} groups registered on '{name}'");
        }

        void EnsureMap()
        {
            if (_map == null) RebuildMap();
        }

        public bool HasGroup(string key)
        {
            EnsureMap();
            return !string.IsNullOrEmpty(key) && _map.ContainsKey(key);
        }

        public void Apply(string key, Sprite[] sprites)
        {
            EnsureMap();
            if (string.IsNullOrEmpty(key) || !_map.TryGetValue(key, out var g) || g.renderers == null) return;

            int changed = 0;
            int kept = 0;
            for (int i = 0; i < g.renderers.Length; i++)
            {
                var r = g.renderers[i];
                if (!r) continue;

                Sprite next = (sprites != null && i < sprites.Length) ? sprites[i] : null;
                if (next == null && keepOriginalIfNull)
                {
                    next = r.sprite;
                    kept++;
                }

                if (r.sprite != next)
                {
                    r.sprite = next;
                    changed++;
                }
            }
            if (verboseLogging)
                Debug.Log($"[EquipGroups] Apply key='{key}' renderers={g.renderers.Length} input={(sprites?.Length ?? 0)} changed={changed} kept={kept}");
        }

        public void Clear(string key)
        {
            EnsureMap();
            if (string.IsNullOrEmpty(key) || !_map.TryGetValue(key, out var g) || g.renderers == null) return;

            int reverted = 0;
            for (int i = 0; i < g.renderers.Length; i++)
            {
                var r = g.renderers[i];
                if (!r) continue;

                if (revertToDefaultOnClear && g.defaults != null && i < g.defaults.Length)
                {
                    if (r.sprite != g.defaults[i])
                    {
                        r.sprite = g.defaults[i];
                        reverted++;
                    }
                }
            }
            if (verboseLogging)
                Debug.Log($"[EquipGroups] Clear key='{key}' reverted={reverted}");
        }

        /// <summary>
        /// [�߰�] ��ϵ� ��� ��������Ʈ �׷��� �⺻ ���·� �ǵ����ϴ�.
        /// </summary>
        public void ClearAll()
        {
            EnsureMap();
            if (_map == null) return;

            foreach (var group in _map.Values)
            {
                if (group?.renderers == null) continue;

                for (int i = 0; i < group.renderers.Length; i++)
                {
                    var r = group.renderers[i];
                    if (!r) continue;

                    if (revertToDefaultOnClear && group.defaults != null && i < group.defaults.Length)
                    {
                        r.sprite = group.defaults[i];
                    }
                    else
                    {
                        r.sprite = null;
                    }
                }
            }
            if (verboseLogging) Debug.Log($"[EquipGroups] Cleared all groups.");
        }


#if UNITY_EDITOR
        [ContextMenu("DEV/Print Groups")]
        void DevPrint()
        {
            EnsureMap();
            foreach (var kv in _map)
            {
                var g = kv.Value;
                Debug.Log($"[EquipGroups] key='{kv.Key}' renderers={g.renderers?.Length ?? 0}");
            }
        }
#endif
    }
}