using UnityEngine;

namespace MB
{
    public enum WeaponId { Buster = 0, Slicer = 1, Bomber = 2 }
    public enum EnemyKind { Met, Walker, Flyer }
    public enum PickupType { HealthSmall, HealthLarge, EnergySmall, EnergyLarge, ETank, WTank, OneUp }
    public enum DamageResult { Ignored, Hit, Killed, Blocked }

    public struct DamageInfo
    {
        public int amount;
        public WeaponId weapon;
        public Vector2 from;

        public DamageInfo(int amount, WeaponId weapon, Vector2 from)
        {
            this.amount = amount;
            this.weapon = weapon;
            this.from = from;
        }
    }

    public interface IDamageable
    {
        DamageResult TakeDamage(in DamageInfo info);
    }

    /// Central tuning values. Units: 1 unit = 1 tile = 16 pixels.
    /// Numbers are derived from NES Mega Man frame data (60fps, px/frame -> units/second).
    public static class GameConfig
    {
        public const int MaxHealth = 28;        // NES bars are 28 ticks
        public const int MaxWeaponEnergy = 28;
        public const int MaxTanks = 9;
        public const int StartingLives = 2;     // spare lives, NES style

        // Player movement
        public const float RunSpeed = 4.9f;      // ~1.3 px/frame
        public const float JumpVelocity = 18.4f; // ~3 tile jump apex
        public const float Gravity = 56f;        // units/s^2, ~0.25 px/frame^2
        public const float MaxFallSpeed = 24f;
        public const float SlideSpeed = 8.8f;
        public const float SlideDuration = 0.42f;
        public const float ClimbSpeed = 4.2f;
        public const float HurtDuration = 0.35f;
        public const float InvulnDuration = 1.2f;
        public const float KnockbackX = 2.4f;

        // Buster charge thresholds (seconds held)
        public const float ChargeTier1 = 0.32f;
        public const float ChargeTier2 = 1.05f;

        public const int MaxBusterShots = 3;     // classic 3 pellets on screen

        public static Color TintFor(WeaponId w)
        {
            switch (w)
            {
                case WeaponId.Slicer: return new Color(1f, 0.96f, 0.78f);
                case WeaponId.Bomber: return new Color(0.55f, 0.92f, 0.55f);
                default: return new Color(0.47f, 0.75f, 1f);
            }
        }

        public static string NameOf(WeaponId w)
        {
            switch (w)
            {
                case WeaponId.Slicer: return "SLICE BOOMERANG";
                case WeaponId.Bomber: return "BLAST BOMB";
                default: return "MEGA BUSTER";
            }
        }

        public static string LetterOf(WeaponId w)
        {
            switch (w)
            {
                case WeaponId.Slicer: return "S";
                case WeaponId.Bomber: return "B";
                default: return "P";
            }
        }
    }

    [System.Serializable]
    public class StageData
    {
        public int slot;
        public string stageName;
        public string bossName;
        public string sceneName;
        public WeaponId reward;
        public WeaponId bossWeakness;
        public Color32 theme;
        public bool available;

        public StageData(int slot, string stageName, string bossName, string sceneName,
                         WeaponId reward, WeaponId weakness, Color32 theme, bool available)
        {
            this.slot = slot; this.stageName = stageName; this.bossName = bossName;
            this.sceneName = sceneName; this.reward = reward; this.bossWeakness = weakness;
            this.theme = theme; this.available = available;
        }
    }

    public static class Stages
    {
        // 8 stage-select slots. Slots 0 and 1 are playable in this build (both load the
        // demo stage with a different boss config); the rest are wired but locked.
        public static readonly StageData[] All =
        {
            new StageData(0, "SLICE STAGE", "SLICE MAN", "DemoStage", WeaponId.Slicer, WeaponId.Bomber, new Color32(216, 120, 64, 255), true),
            new StageData(1, "BLAST STAGE", "BLAST MAN", "DemoStage", WeaponId.Bomber, WeaponId.Slicer, new Color32(96, 176, 96, 255), true),
            new StageData(2, "????", "????", "", WeaponId.Buster, WeaponId.Buster, new Color32(90, 90, 110, 255), false),
            new StageData(3, "????", "????", "", WeaponId.Buster, WeaponId.Buster, new Color32(90, 90, 110, 255), false),
            new StageData(4, "????", "????", "", WeaponId.Buster, WeaponId.Buster, new Color32(90, 90, 110, 255), false),
            new StageData(5, "????", "????", "", WeaponId.Buster, WeaponId.Buster, new Color32(90, 90, 110, 255), false),
            new StageData(6, "????", "????", "", WeaponId.Buster, WeaponId.Buster, new Color32(90, 90, 110, 255), false),
            new StageData(7, "????", "????", "", WeaponId.Buster, WeaponId.Buster, new Color32(90, 90, 110, 255), false),
        };

        public static StageData BySlot(int slot)
        {
            foreach (var s in All) if (s.slot == slot) return s;
            return null;
        }
    }
}
