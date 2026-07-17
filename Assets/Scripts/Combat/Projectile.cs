using UnityEngine;

namespace MB
{
    /// Straight-flying shot. Player shots pass through walls (classic buster
    /// behavior); enemy shots pass through walls too. Blocked hits deflect
    /// with the iconic 'ping'.
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        public int damage = 1;
        public WeaponId weapon = WeaponId.Buster;
        public bool fromPlayer = true;
        public bool pierce;
        public bool dieOnGround;
        public float lifetime = 3f;

        protected Rigidbody2D rb;
        protected bool deflected;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Launch(Vector2 velocity)
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            rb.linearVelocity = velocity;
        }

        protected virtual void Update()
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f) { Destroy(gameObject); return; }

            var cam = Camera.main;
            if (cam != null)
            {
                var v = cam.WorldToViewportPoint(transform.position);
                if (v.x < -0.2f || v.x > 1.2f || v.y < -0.3f || v.y > 1.3f)
                    Destroy(gameObject);
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (deflected) return;

            if (other.gameObject.layer == Layers.Ground)
            {
                if (dieOnGround) Destroy(gameObject);
                return;
            }

            if (fromPlayer && other.gameObject.layer == Layers.Enemy)
            {
                var target = other.GetComponentInParent<EnemyBase>();
                if (target == null || target.IsDead) return;
                var result = target.TakeDamage(new DamageInfo(damage, weapon, transform.position));
                if (result == DamageResult.Blocked) Deflect();
                else if (!pierce) Destroy(gameObject);
            }
            else if (!fromPlayer && other.gameObject.layer == Layers.Player)
            {
                var hp = other.GetComponentInParent<PlayerHealth>();
                if (hp == null || hp.IsDead) return;
                var res = hp.TakeDamage(new DamageInfo(damage, weapon, transform.position));
                if (res != DamageResult.Ignored) Destroy(gameObject);
            }
        }

        protected void Deflect()
        {
            deflected = true;
            Sfx.Play(SfxId.Deflect);
            rb.linearVelocity = new Vector2(-Mathf.Sign(rb.linearVelocity.x) * 6f, 9f);
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            lifetime = 0.6f;
        }
    }
}
