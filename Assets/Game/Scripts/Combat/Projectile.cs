// Assets/Game/Scripts/Battle/Projectile.cs
using UnityEngine;
using Game.Services;
using Poolable = Game.Services.IPoolable;  // �� ��Ī

namespace Game.Battle
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Projectile : MonoBehaviour, Poolable
    {
        public float speed = 12f;
        public float lifeTime = 2.5f;
        public int damage = 1;
        public LayerMask hitMask;

        Rigidbody2D _rb;
        float _life;

        void Awake() { _rb = GetComponent<Rigidbody2D>(); }

        public void Fire(Vector2 origin, Vector2 dir)
        {
            transform.position = origin;
            transform.right = dir == Vector2.zero ? transform.right : dir.normalized;
            _rb.linearVelocity = transform.right * speed;
            _life = lifeTime;
        }

        void Update()
        {
            _life -= Time.deltaTime;
            if (_life <= 0f) Despawn();
        }

        void OnTriggerEnter2D(Collider2D col)
        {
            if (((1 << col.gameObject.layer) & hitMask) == 0) return;

            var hp = col.GetComponent<Health>();
            if (hp) hp.TakeDamage(damage); // HitInfo �����̸� �װɷ� �ٲ㵵 ��
            Despawn();
        }

        public void OnSpawned()
        {
            _rb.simulated = true;
            _rb.linearVelocity = Vector2.zero;
            var trail = GetComponentInChildren<TrailRenderer>();
            if (trail) trail.Clear();
            _life = lifeTime;
        }

        public void OnDespawned()
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated = false;
        }

        void Despawn()
        {
            ObjectPool.I.Return(gameObject);
        }
    }
}
