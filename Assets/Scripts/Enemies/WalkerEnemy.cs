using UnityEngine;

namespace MB
{
    /// Ground patroller: walks until it hits a wall or a ledge, then turns.
    [RequireComponent(typeof(Rigidbody2D))]
    public class WalkerEnemy : EnemyBase
    {
        public float speed = 2.2f;
        public int dir = -1;

        Rigidbody2D rb;
        BoxCollider2D body;
        float animClock;

        protected override void Awake()
        {
            base.Awake();
            maxHp = 3;
            contactDamage = 3;
            rb = GetComponent<Rigidbody2D>();
            body = GetComponent<BoxCollider2D>();
        }

        protected override void Update()
        {
            base.Update();
            if (IsDead) return;

            // wall ahead?
            Vector2 origin = (Vector2)transform.position + new Vector2(0, 0.35f);
            var wall = Physics2D.Raycast(origin, new Vector2(dir, 0), 0.55f, Layers.GroundMask);
            // ledge ahead?
            var ground = Physics2D.Raycast(origin + new Vector2(dir * 0.45f, 0), Vector2.down, 0.9f, Layers.GroundMask);

            if (wall.collider != null || ground.collider == null)
                dir = -dir;

            rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

            animClock += Time.deltaTime;
            SetSprite(Mathf.Repeat(animClock, 0.3f) < 0.15f ? "walker_0" : "walker_1", dir < 0);
        }
    }
}
