// Assets/Game/Scripts/Services/ObjectPool.cs
using System.Collections.Generic;
using UnityEngine;

namespace Game.Services
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }

    public sealed class PoolToken : MonoBehaviour
    {
        public GameObject prefab;
    }

    public sealed class ObjectPool : MonoBehaviour
    {
        static ObjectPool _i;
        public static ObjectPool I => _i;

        readonly Dictionary<GameObject, Stack<GameObject>> _pool = new();

        void Awake()
        {
            if (_i && _i != this) { Destroy(gameObject); return; }
            _i = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Prewarm(GameObject prefab, int count, Transform parent = null)
        {
            if (!prefab || count <= 0) return;
            if (!_pool.TryGetValue(prefab, out var stack)) { stack = new Stack<GameObject>(); _pool[prefab] = stack; }
            for (int i = 0; i < count; i++)
            {
                var go = Create(prefab, parent);
                go.SetActive(false);
                stack.Push(go);
            }
        }

        public GameObject Rent(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent = null)
        {
            if (!prefab) return null;
            if (!_pool.TryGetValue(prefab, out var stack)) { stack = new Stack<GameObject>(); _pool[prefab] = stack; }

            GameObject go = stack.Count > 0 ? stack.Pop() : Create(prefab, parent);
            if (parent && go.transform.parent != parent) go.transform.SetParent(parent, false);

            var t = go.transform;
            t.SetPositionAndRotation(pos, rot);
            go.SetActive(true);

            // 완전 수식으로 충돌 회피
            foreach (var p in go.GetComponentsInChildren<Game.Services.IPoolable>(true)) p.OnSpawned();
            return go;
        }

        public void Return(GameObject instance)
        {
            if (!instance) return;
            var token = instance.GetComponent<PoolToken>();
            if (!token || !token.prefab) { Destroy(instance); return; }

            foreach (var p in instance.GetComponentsInChildren<Game.Services.IPoolable>(true)) p.OnDespawned();

            instance.SetActive(false);
            if (!_pool.TryGetValue(token.prefab, out var stack)) { stack = new Stack<GameObject>(); _pool[token.prefab] = stack; }
            stack.Push(instance);
        }

        GameObject Create(GameObject prefab, Transform parent)
        {
            var go = Instantiate(prefab, parent);
            go.name = $"{prefab.name}_Pooled";
            var token = go.GetComponent<PoolToken>() ?? go.AddComponent<PoolToken>();
            token.prefab = prefab;
            return go;
        }
    }
}
