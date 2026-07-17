using System.Collections;
using UnityEngine;

namespace MB
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public int HP { get; private set; } = GameConfig.MaxHealth;
        public bool IsDead { get; private set; }
        public float InvulnTimer { get; private set; }

        PlayerController controller;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
        }

        void Update()
        {
            if (InvulnTimer > 0) InvulnTimer -= Time.deltaTime;
        }

        public DamageResult TakeDamage(in DamageInfo info)
        {
            if (IsDead || InvulnTimer > 0f) return DamageResult.Ignored;
            if (controller != null && controller.State == PlayerController.PState.Frozen) return DamageResult.Ignored;

            HP -= info.amount;
            InvulnTimer = GameConfig.InvulnDuration;
            Sfx.Play(SfxId.Hurt);
            controller?.Hurt(info.from);

            if (HP <= 0)
            {
                HP = 0;
                Die();
                return DamageResult.Killed;
            }
            return DamageResult.Hit;
        }

        /// Spikes and pits ignore i-frames, classic style.
        public void InstantKill()
        {
            if (IsDead) return;
            HP = 0;
            Die();
        }

        void Die()
        {
            IsDead = true;
            controller?.MarkDead();
            Sfx.Play(SfxId.ExplodeBig);
            EntityFactory.CreateDeathOrbs(transform.position + Vector3.up * 0.7f, GameConfig.TintFor(WeaponId.Buster));
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
            StageController.I?.OnPlayerDeath();
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            StartCoroutine(HealGradual(amount));
        }

        IEnumerator HealGradual(int amount)
        {
            while (amount > 0 && HP < GameConfig.MaxHealth)
            {
                HP++;
                amount--;
                Sfx.Play(SfxId.Energy, 0.3f);
                yield return new WaitForSecondsRealtime(0.035f);
            }
        }

        /// Full refill used by E-Tanks (runs on unscaled time so it works while paused).
        public IEnumerator RefillRoutine()
        {
            Sfx.Play(SfxId.ETank);
            while (HP < GameConfig.MaxHealth)
            {
                HP++;
                Sfx.Play(SfxId.Energy, 0.3f);
                yield return new WaitForSecondsRealtime(0.035f);
            }
        }

        // contact damage from enemy hitboxes
        void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.layer != Layers.Enemy) return;
            var enemy = other.GetComponentInParent<EnemyBase>();
            if (enemy == null || enemy.IsDead) return;
            TakeDamage(new DamageInfo(enemy.contactDamage, WeaponId.Buster, other.bounds.center));
        }
    }
}
