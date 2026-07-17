using UnityEngine;

namespace MB
{
    /// Flying pest: bobs in place until the player is near, then swoops at
    /// them with a sine-wave wobble. Ignores walls.
    [RequireComponent(typeof(Rigidbody2D))]
    public class FlyerEnemy : EnemyBase
    {
        public float aggroRange = 7.5f;
        public float speed = 2.8f;

        Rigidbody2D rb;
        bool aggro;
        float clock;
        Vector3 home;

        protected override void Awake()
        {
            base.Awake();
            maxHp = 1;
            contactDamage = 3;
            rb = GetComponent<Rigidbody2D>();
            home = transform.position;
        }

        protected override void Update()
        {
            base.Update();
            if (IsDead) return;

            clock += Time.deltaTime;
            var player = PlayerT;

            if (!aggro)
            {
                rb.linearVelocity = new Vector2(0, Mathf.Sin(clock * 4f) * 0.7f);
                if (player != null && Vector2.Distance(player.position, transform.position) < aggroRange)
                    aggro = true;
            }
            else if (player != null)
            {
                Vector2 target = (Vector2)player.position + Vector2.up * 0.8f;
                Vector2 dir = (target - (Vector2)transform.position).normalized;
                rb.linearVelocity = dir * speed + Vector2.up * Mathf.Sin(clock * 9f) * 1.6f;
            }
            else rb.linearVelocity = Vector2.zero;

            bool flip = rb.linearVelocity.x < 0;
            SetSprite(Mathf.Repeat(clock, 0.2f) < 0.1f ? "flyer_0" : "flyer_1", flip);
        }
    }
}
