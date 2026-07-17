using UnityEngine;

namespace MB
{
    /// Forwards trigger events from a child collider to a parent handler.
    public class TriggerRelay : MonoBehaviour
    {
        public System.Action<Collider2D> onEnter;
        void OnTriggerEnter2D(Collider2D other) => onEnter?.Invoke(other);
    }

    /// Health/weapon energy pellets, E-Tanks, W-Tanks and 1-Ups.
    public class Pickup : MonoBehaviour
    {
        public PickupType type;
        public bool despawns;   // true for enemy drops

        float life = 6f;
        SpriteRenderer sr;
        bool taken;

        void Awake() { sr = GetComponent<SpriteRenderer>(); }

        void Update()
        {
            if (!despawns) return;
            life -= Time.deltaTime;
            if (life <= 0f) { Destroy(gameObject); return; }
            if (life < 1.5f && sr != null)
                sr.enabled = Mathf.Repeat(Time.time, 0.15f) < 0.1f;
        }

        void OnTriggerEnter2D(Collider2D other) => TryCollect(other);

        public void TryCollect(Collider2D other)
        {
            if (taken || other.gameObject.layer != Layers.Player) return;
            var health = other.GetComponentInParent<PlayerHealth>();
            var weapons = other.GetComponentInParent<WeaponSystem>();
            if (health == null || health.IsDead) return;
            taken = true;

            var gm = GameManager.I;
            switch (type)
            {
                case PickupType.HealthSmall: health.Heal(2); Sfx.Play(SfxId.Pickup); break;
                case PickupType.HealthLarge: health.Heal(10); Sfx.Play(SfxId.Pickup); break;
                case PickupType.EnergySmall: weapons?.AddEnergyPickup(2); Sfx.Play(SfxId.Pickup); break;
                case PickupType.EnergyLarge: weapons?.AddEnergyPickup(10); Sfx.Play(SfxId.Pickup); break;
                case PickupType.ETank:
                    if (gm.ETanks < GameConfig.MaxTanks) gm.ETanks++;
                    gm.Save();
                    Sfx.Play(SfxId.ETank);
                    break;
                case PickupType.WTank:
                    if (gm.WTanks < GameConfig.MaxTanks) gm.WTanks++;
                    gm.Save();
                    Sfx.Play(SfxId.ETank);
                    break;
                case PickupType.OneUp:
                    gm.Lives++;
                    Sfx.Play(SfxId.OneUp);
                    break;
            }
            Destroy(gameObject);
        }
    }
}
