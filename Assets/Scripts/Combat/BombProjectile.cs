using UnityEngine;

namespace MB
{
    /// Lobbed bomb: arcs with gravity, explodes on contact with ground or an
    /// enemy (or when the fuse runs out) and deals area damage.
    [RequireComponent(typeof(Rigidbody2D))]
    public class BombProjectile : MonoBehaviour
    {
        public int damage = 4;
        public float radius = 1.7f;
        public float fuse = 2.5f;

        bool exploded;

        void Update()
        {
            fuse -= Time.deltaTime;
            if (fuse <= 0f) Explode();
            transform.Rotate(0, 0, 360f * Time.deltaTime);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == Layers.Ground ||
                other.gameObject.layer == Layers.Enemy)
                Explode();
        }

        void Explode()
        {
            if (exploded) return;
            exploded = true;

            Sfx.Play(SfxId.ExplodeSmall);
            EntityFactory.CreateExplosion(transform.position, true);

            var hits = Physics2D.OverlapCircleAll(transform.position, radius, Layers.EnemyMask);
            foreach (var h in hits)
            {
                var e = h.GetComponentInParent<EnemyBase>();
                if (e != null && !e.IsDead)
                    e.TakeDamage(new DamageInfo(damage, WeaponId.Bomber, transform.position));
            }
            Destroy(gameObject);
        }
    }
}
