// Assets/Game/Scripts/Services/PoolService.cs
using UnityEngine;
using System.Collections.Generic;

namespace Game.Services
{
    public class PoolService : MonoBehaviour
    {
        readonly Dictionary<int, Queue<GameObject>> _pool = new();

        public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            int id = prefab.GetInstanceID();
            if (!_pool.TryGetValue(id, out var q) || q.Count == 0)
                return Instantiate(prefab, pos, rot);

            var go = q.Dequeue();
            go.transform.SetPositionAndRotation(pos, rot);
            go.SetActive(true);
            return go;
        }

        public void Despawn(GameObject prefab, GameObject instance)
        {
            int id = prefab.GetInstanceID();
            if (!_pool.TryGetValue(id, out var q)) _pool[id] = q = new Queue<GameObject>();
            instance.SetActive(false);
            _pool[id].Enqueue(instance);
        }
    }
}
