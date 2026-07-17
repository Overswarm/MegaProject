using UnityEngine;

namespace MB
{
    /// Layer indices match ProjectSettings/TagManager.asset.
    /// The 2D collision matrix is configured in code at startup so no manual
    /// Physics2D settings are needed.
    public static class Layers
    {
        public const int Ground = 6;
        public const int Player = 7;
        public const int Enemy = 8;       // enemy hurt/hit boxes (triggers)
        public const int PlayerShot = 9;
        public const int EnemyShot = 10;
        public const int Item = 11;       // pickup triggers
        public const int Ladder = 12;
        public const int Hazard = 13;
        public const int Body = 14;       // solid bodies of enemies/pickups: collide with Ground only

        public static int GroundMask => 1 << Ground;
        public static int EnemyMask => 1 << Enemy;
        public static int PlayerMask => 1 << Player;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Apply()
        {
            int[] ours = { Ground, Player, Enemy, PlayerShot, EnemyShot, Item, Ladder, Hazard, Body };
            foreach (var a in ours)
            {
                for (int b = 0; b < 32; b++)
                    Physics2D.IgnoreLayerCollision(a, b, true);
            }

            Allow(Ground, Player);
            Allow(Ground, Body);
            Allow(Ground, PlayerShot);   // trigger events only; buster ignores, bombs explode
            Allow(Player, Enemy);        // contact damage
            Allow(Player, EnemyShot);
            Allow(Player, Item);
            Allow(Player, Ladder);
            Allow(Player, Hazard);
            Allow(Enemy, PlayerShot);
        }

        static void Allow(int a, int b) => Physics2D.IgnoreLayerCollision(a, b, false);
    }
}
