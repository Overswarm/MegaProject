using UnityEngine;

namespace MB
{
    /// Spikes and pits. Instant kill ignores i-frames, NES style.
    public class Hazard : MonoBehaviour
    {
        public bool instantKill = true;
        public int damage = 4;

        void OnTriggerEnter2D(Collider2D other) { Hit(other); }
        void OnTriggerStay2D(Collider2D other) { Hit(other); }

        void Hit(Collider2D other)
        {
            if (other.gameObject.layer != Layers.Player) return;
            var hp = other.GetComponentInParent<PlayerHealth>();
            if (hp == null || hp.IsDead) return;
            if (instantKill) hp.InstantKill();
            else hp.TakeDamage(new DamageInfo(damage, WeaponId.Buster, transform.position));
        }
    }
}
