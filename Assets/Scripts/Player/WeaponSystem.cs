using System.Collections.Generic;
using UnityEngine;

namespace MB
{
    /// Tracks the equipped weapon and energy (energy is stored on GameManager so
    /// it persists across scenes, like the NES games).
    public class WeaponSystem : MonoBehaviour
    {
        public WeaponId Current { get; private set; } = WeaponId.Buster;

        public List<WeaponId> UnlockedOrdered
        {
            get
            {
                var list = new List<WeaponId>();
                foreach (WeaponId w in System.Enum.GetValues(typeof(WeaponId)))
                    if (GameManager.I.Unlocked.Contains(w)) list.Add(w);
                return list;
            }
        }

        public int CurrentEnergy => GameManager.I.GetEnergy(Current);
        public Color CurrentTint => GameConfig.TintFor(Current);

        public void Cycle(int dir)
        {
            var list = UnlockedOrdered;
            if (list.Count <= 1) return;
            int i = list.IndexOf(Current);
            i = (i + dir + list.Count) % list.Count;
            Current = list[i];
            Sfx.Play(SfxId.Cursor);
        }

        public void Select(WeaponId w)
        {
            if (!GameManager.I.Unlocked.Contains(w)) return;
            Current = w;
        }

        public bool Consume(int cost)
        {
            if (Current == WeaponId.Buster) return true;
            if (GameManager.I.GetEnergy(Current) < cost) return false;
            GameManager.I.AddEnergy(Current, -cost);
            return true;
        }

        /// Weapon energy pickups refill the current weapon (wasted on the buster,
        /// just like the NES).
        public void AddEnergyPickup(int amount)
        {
            if (Current == WeaponId.Buster) return;
            GameManager.I.AddEnergy(Current, amount);
        }
    }
}
