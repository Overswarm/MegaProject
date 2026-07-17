using UnityEngine;

namespace MB
{
    /// Base enemy: HP, contact damage, white damage-flash, death explosion and
    /// random item drops. Enemy hurt/hit boxes live on a child trigger collider
    /// on the Enemy layer; solid bodies (if any) are on the Body layer.
    public class EnemyBase : MonoBehaviour, IDamageable
    {
        public int maxHp = 2;
        public int contactDamage = 3;
        public bool invincible;
        public bool dropsItems = true;

        public bool IsDead { get; protected set; }
        public System.Action onDeath;

        protected SpriteRenderer sr;
        protected Color baseTint = Color.white;
        float flashTimer;

        protected Transform PlayerT
        {
            get
            {
                var sc = StageController.I;
                return sc != null && sc.Player != null ? sc.Player.transform : null;
            }
        }

        protected virtual void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 8;
        }

        protected int hp;

        protected virtual void Start()
        {
            hp = maxHp;
        }

        public virtual DamageResult TakeDamage(in DamageInfo info)
        {
            if (IsDead) return DamageResult.Ignored;
            if (invincible) return DamageResult.Blocked;

            hp -= info.amount;
            flashTimer = 0.08f;
            Sfx.Play(SfxId.Hit, 0.35f);

            if (hp <= 0)
            {
                Die();
                return DamageResult.Killed;
            }
            return DamageResult.Hit;
        }

        protected virtual void Die()
        {
            if (IsDead) return;
            IsDead = true;
            Sfx.Play(SfxId.ExplodeSmall, 0.4f);
            EntityFactory.CreateExplosion(transform.position + Vector3.up * 0.4f, false);
            if (dropsItems) EntityFactory.SpawnDrop(transform.position + Vector3.up * 0.3f);
            onDeath?.Invoke();
            Destroy(gameObject);
        }

        protected void SetSprite(string key, bool flipX)
        {
            if (sr == null) return;
            sr.sprite = SpriteFactory.Get(key);
            sr.flipX = flipX;
        }

        protected virtual void Update()
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                sr.color = Color.white * 2f;
                if (flashTimer <= 0f) sr.color = baseTint;
            }
        }
    }
}
