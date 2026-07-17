using System.Collections;
using UnityEngine;

namespace MB
{
    /// Stage boss. Sleeps invisibly until Activate() (called by the boss door
    /// sequence): drops from the ceiling, poses while its health bar fills,
    /// then fights with jump + spread-shot patterns. Weak to the weapon
    /// configured on the current StageData.
    [RequireComponent(typeof(Rigidbody2D))]
    public class BossController : EnemyBase
    {
        public string bossName = "BOSS";
        public WeaponId weakness = WeaponId.Bomber;
        public Color tint = Color.white;

        /// HP shown by the HUD (animated during the intro fill).
        public int DisplayHP { get; private set; }
        public int MaxHP => maxHp;
        public bool FightActive { get; private set; }

        Rigidbody2D rb;
        BoxCollider2D body;
        float invulnTimer;
        bool activated;

        protected override void Awake()
        {
            base.Awake();
            maxHp = 28;
            contactDamage = 6;
            dropsItems = false;
            rb = GetComponent<Rigidbody2D>();
            body = GetComponent<BoxCollider2D>();
            baseTint = tint;
        }

        protected override void Start()
        {
            base.Start();
            var stage = GameManager.I.CurrentStage;
            if (stage != null)
            {
                bossName = stage.bossName;
                weakness = stage.bossWeakness;
                baseTint = tint = (Color)stage.theme;
            }
            // sleep until activated
            sr.enabled = false;
            rb.simulated = false;
            SetHitboxActive(false);
            sr.color = baseTint;
            DisplayHP = 0;
        }

        void SetHitboxActive(bool on)
        {
            foreach (Transform child in transform)
                if (child.gameObject.layer == Layers.Enemy)
                    child.gameObject.SetActive(on);
        }

        public IEnumerator IntroRoutine(Vector2 dropTo)
        {
            activated = true;
            transform.position = new Vector3(dropTo.x, dropTo.y + 10f, 0);
            sr.enabled = true;
            sr.sprite = SpriteFactory.Get("boss_idle");

            // drop in
            while (transform.position.y > dropTo.y + 0.05f)
            {
                transform.position += Vector3.down * 22f * Time.deltaTime;
                if (transform.position.y < dropTo.y) transform.position = new Vector3(dropTo.x, dropTo.y, 0);
                yield return null;
            }
            Sfx.Play(SfxId.Land);
            yield return new WaitForSeconds(0.6f);

            // health fill
            for (int i = 0; i < maxHp; i++)
            {
                DisplayHP = i + 1;
                if (i % 2 == 0) Sfx.Play(SfxId.Energy, 0.35f);
                yield return new WaitForSeconds(0.045f);
            }
            yield return new WaitForSeconds(0.35f);

            rb.simulated = true;
            SetHitboxActive(true);
            FightActive = true;
            StartCoroutine(FightRoutine());
        }

        public override DamageResult TakeDamage(in DamageInfo info)
        {
            if (IsDead) return DamageResult.Ignored;
            if (!FightActive || invulnTimer > 0f) return DamageResult.Blocked;

            int amount = info.amount;
            if (info.weapon == weakness) amount *= 3;

            hp -= amount;
            DisplayHP = Mathf.Max(0, hp);
            invulnTimer = 0.5f;
            Sfx.Play(SfxId.BossHit);
            StartCoroutine(HitFlash());

            if (hp <= 0)
            {
                Die();
                return DamageResult.Killed;
            }
            return DamageResult.Hit;
        }

        IEnumerator HitFlash()
        {
            for (int i = 0; i < 5; i++)
            {
                sr.color = Color.white;
                yield return new WaitForSeconds(0.05f);
                sr.color = baseTint;
                yield return new WaitForSeconds(0.05f);
            }
        }

        protected override void Die()
        {
            if (IsDead) return;
            FightActive = false;
            StopAllCoroutines();
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            SetHitboxActive(false);
            // don't call base.Die(): it would destroy this object immediately.
            // StageController owns the victory sequence and destroys us at the end.
            IsDead = true;
            StageController.I?.OnBossDefeated(this);
        }

        protected override void Update()
        {
            base.Update();
            if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;
            if (FightActive && sr != null)
            {
                var player = PlayerT;
                if (player != null) sr.flipX = player.position.x < transform.position.x;
            }
        }

        IEnumerator FightRoutine()
        {
            var wait = new WaitForSeconds(0.02f);
            while (!IsDead)
            {
                yield return new WaitForSeconds(Random.Range(0.4f, 0.9f));
                if (IsDead) break;

                var player = PlayerT;
                if (player == null) continue;

                float dx = player.position.x - transform.position.x;
                int dir = dx >= 0 ? 1 : -1;

                int action = Random.Range(0, 3);
                if (action == 0)
                {
                    // big jump toward player
                    rb.linearVelocity = new Vector2(dir * Mathf.Min(Mathf.Abs(dx), 6f), 16f);
                    yield return new WaitForSeconds(0.4f);
                    // fire one shot at apex
                    ShootAt(player.position);
                    while (!Grounded() && !IsDead) yield return wait;
                    Sfx.Play(SfxId.Land, 0.3f);
                    rb.linearVelocity = Vector2.zero;
                }
                else if (action == 1)
                {
                    // short hop + 3-way spread
                    rb.linearVelocity = new Vector2(dir * 2f, 10f);
                    while (!Grounded() && !IsDead) yield return wait;
                    rb.linearVelocity = Vector2.zero;
                    SpreadShot(dir);
                }
                else
                {
                    SpreadShot(dir);
                }
            }
        }

        void ShootAt(Vector2 target)
        {
            Vector2 origin = (Vector2)transform.position + Vector2.up * 1.2f;
            Vector2 v = (target - origin).normalized * 8.5f;
            EntityFactory.CreateEnemyShot(origin, v, 3);
            Sfx.Play(SfxId.Shoot, 0.3f);
        }

        void SpreadShot(int dir)
        {
            Vector2 origin = (Vector2)transform.position + new Vector2(dir * 0.6f, 1.1f);
            foreach (var ang in new[] { -20f, 0f, 20f })
            {
                var rad = ang * Mathf.Deg2Rad;
                var v = new Vector2(Mathf.Cos(rad) * dir, Mathf.Sin(rad)) * 8f;
                EntityFactory.CreateEnemyShot(origin, v, 3);
            }
            Sfx.Play(SfxId.Shoot, 0.3f);
        }

        bool Grounded()
        {
            var hit = Physics2D.BoxCast((Vector2)transform.position + Vector2.up * 0.1f,
                new Vector2(0.9f, 0.1f), 0, Vector2.down, 0.15f, Layers.GroundMask);
            return hit.collider != null && rb.linearVelocity.y <= 0.1f;
        }
    }
}
