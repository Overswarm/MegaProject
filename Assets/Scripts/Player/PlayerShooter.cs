using System.Collections.Generic;
using UnityEngine;

namespace MB
{
    /// Mega Buster (pellet + MM4-style charge) and special weapon firing.
    public class PlayerShooter : MonoBehaviour
    {
        /// 0 = not charged, 1 = mid charge, 2 = full charge (for visuals)
        public int ChargeTier
        {
            get
            {
                if (!charging) return 0;
                if (chargeTime >= GameConfig.ChargeTier2) return 2;
                if (chargeTime >= GameConfig.ChargeTier1) return 1;
                return 0;
            }
        }

        PlayerController controller;
        WeaponSystem weapons;
        readonly List<GameObject> busterShots = new List<GameObject>();
        readonly List<GameObject> specials = new List<GameObject>();
        bool charging;
        float chargeTime;
        int lastTier;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
            weapons = GetComponent<WeaponSystem>();
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;
            if (controller.State == PlayerController.PState.Dead ||
                controller.State == PlayerController.PState.Frozen)
            {
                charging = false; chargeTime = 0;
                return;
            }

            // weapon quick-swap (bumpers / Q,E)
            if (GameInput.PrevPressed) weapons.Cycle(-1);
            if (GameInput.NextPressed) weapons.Cycle(+1);

            bool canShoot = controller.State == PlayerController.PState.Normal ||
                            controller.State == PlayerController.PState.Climb;

            if (weapons.Current == WeaponId.Buster)
                TickBuster(canShoot);
            else
                TickSpecial(canShoot);
        }

        void TickBuster(bool canShoot)
        {
            if (GameInput.FirePressed)
            {
                charging = true;
                chargeTime = 0f;
                lastTier = 0;
                if (canShoot) FireBuster(0);
            }

            if (charging && GameInput.FireHeld)
            {
                chargeTime += Time.deltaTime;
                int tier = ChargeTier;
                if (tier == 2 && lastTier < 2) Sfx.Play(SfxId.ChargeFull, 0.35f);
                lastTier = tier;
            }

            if (charging && GameInput.FireReleased)
            {
                int tier = ChargeTier;
                charging = false;
                chargeTime = 0f;
                if (tier > 0 && canShoot) FireBuster(tier);
            }
        }

        void FireBuster(int tier)
        {
            busterShots.RemoveAll(s => s == null);
            if (tier == 0 && busterShots.Count >= GameConfig.MaxBusterShots) return;

            var shot = EntityFactory.CreateBusterShot(Muzzle(), controller.Facing, tier);
            if (tier == 0) busterShots.Add(shot);
            Sfx.Play(SfxId.Shoot);
        }

        void TickSpecial(bool canShoot)
        {
            if (!GameInput.FirePressed || !canShoot) return;
            specials.RemoveAll(s => s == null);

            switch (weapons.Current)
            {
                case WeaponId.Slicer:
                    if (specials.Count >= 1) return;
                    if (!weapons.Consume(1)) { Sfx.Play(SfxId.Buzz, 0.2f); return; }
                    specials.Add(EntityFactory.CreateCutter(Muzzle(), controller.Facing, transform));
                    Sfx.Play(SfxId.Shoot);
                    break;

                case WeaponId.Bomber:
                    if (specials.Count >= 2) return;
                    if (!weapons.Consume(2)) { Sfx.Play(SfxId.Buzz, 0.2f); return; }
                    specials.Add(EntityFactory.CreateBomb(Muzzle(), controller.Facing));
                    Sfx.Play(SfxId.Shoot);
                    break;
            }
        }

        Vector2 Muzzle()
        {
            return (Vector2)transform.position + new Vector2(0.75f * controller.Facing, 0.95f);
        }
    }
}
