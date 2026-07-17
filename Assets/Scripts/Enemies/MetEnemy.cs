using UnityEngine;

namespace MB
{
    /// Classic hard-hat: invincible while hiding, pops up to fire a 3-way
    /// spread when the player is close, then hides again.
    public class MetEnemy : EnemyBase
    {
        public float range = 6.5f;

        enum MState { Closed, Opening, Open }
        MState state = MState.Closed;
        float timer;
        float cooldown;
        bool flip;

        protected override void Awake()
        {
            base.Awake();
            maxHp = 1;
            contactDamage = 3;
            invincible = true;
        }

        protected override void Update()
        {
            base.Update();
            if (IsDead) return;

            var player = PlayerT;
            if (player != null)
                flip = player.position.x < transform.position.x;

            if (cooldown > 0f) cooldown -= Time.deltaTime;

            switch (state)
            {
                case MState.Closed:
                    invincible = true;
                    SetSprite("met_closed", flip);
                    if (player != null && cooldown <= 0f &&
                        Vector2.Distance(player.position, transform.position) < range &&
                        Mathf.Abs(player.position.y - transform.position.y) < 3f)
                    {
                        state = MState.Opening;
                        timer = 0.4f;
                    }
                    break;

                case MState.Opening:
                    invincible = false;
                    SetSprite("met_open", flip);
                    timer -= Time.deltaTime;
                    if (timer <= 0f)
                    {
                        Shoot();
                        state = MState.Open;
                        timer = 0.55f;
                    }
                    break;

                case MState.Open:
                    invincible = false;
                    SetSprite("met_open", flip);
                    timer -= Time.deltaTime;
                    if (timer <= 0f)
                    {
                        state = MState.Closed;
                        cooldown = 1.4f;
                    }
                    break;
            }
        }

        void Shoot()
        {
            float dir = flip ? -1f : 1f;
            Vector2 origin = (Vector2)transform.position + new Vector2(dir * 0.4f, 0.45f);
            foreach (var ang in new[] { -25f, 0f, 25f })
            {
                var rad = ang * Mathf.Deg2Rad;
                var v = new Vector2(Mathf.Cos(rad) * dir, Mathf.Sin(rad)) * 7.5f;
                EntityFactory.CreateEnemyShot(origin, v, 2);
            }
            Sfx.Play(SfxId.Shoot, 0.25f);
        }
    }
}
