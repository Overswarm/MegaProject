using UnityEngine;

namespace MB.EditorTools
{
    /// The hand-tuned demo stage. Layout in tile units (1 unit = 1 tile):
    ///
    ///   Zone 1 (ground, y=0):  two pits, Mets + a Walker, W-Tank on a pit platform
    ///   Zone 2 (climb):        ladder up to the y=8 path (checkpoint 1)
    ///   Zone 3 (upper path):   spike gap, mandatory slide corridor, E-Tank ledge, 1-Up
    ///   Zone 4 (descent):      cliff back to ground (checkpoint 2)
    ///   Boss:                  double shutter doors, corridor, boss room
    ///
    /// Everything is parented under organizational empties at the origin:
    ///   Level    - terrain, hazards, ladders, checkpoints, boss doors
    ///   Enemies  - enemy spawners and the boss
    ///   Pickups  - tanks, pellets, 1-ups
    ///   Doodads  - non-interactive background decoration (empty for now)
    public static class DemoStageLayout
    {
        static readonly Color BlockTint = new Color(0.72f, 0.72f, 0.80f);

        static Transform level, enemies, pickups;

        public static void Populate()
        {
            // ---- stage controller + camera; camera stops at x=92 until the
            //      final door pans it into the boss room ----
            EntityFactory.CreateStageEssentials(new Rect(-2, -3, 94, 23),
                new Color(0.35f, 0.4f, 0.55f));

            level = EntityFactory.GetGroup("Level");
            enemies = EntityFactory.GetGroup("Enemies");
            pickups = EntityFactory.GetGroup("Pickups");
            EntityFactory.GetGroup("Doodads");

            // ---- terrain ----
            Block(-3, -3, 1, 21, "WallLeft");
            Block(-2, -3, 14, 3, "Ground1");          // start, top y=0
            Block(15, -3, 9, 3, "Ground2");           // after pit 1 (x12-15)
            Block(25, 1, 2, 1, "PitPlatform");        // hop across pit 2 (x24-28)
            Block(28, -3, 18, 3, "Ground3");          // ladder base area
            Block(38, 5, 3, 3, "UpperFloorA");        // upper path y=8, ladder gap at x41-42
            Block(42, 5, 4, 3, "UpperFloorB");
            Block(46, -3, 24, 7, "Mass");             // solid under upper path, top y=4
            Block(46, 4, 6, 4, "UpperFloorC");        // spike gap at x52-55
            Block(55, 4, 15, 4, "UpperFloorD");
            Block(60, 9, 5, 2, "SlideCeiling");       // 1-tile slide corridor at y=8..9
            Block(66, 9, 3, 1, "ETankLedge");
            Block(70, -3, 40, 3, "Ground4");          // boss approach + boss room floor
            Block(84, 4, 1, 7, "AboveDoor1");
            Block(85, 4, 6, 1, "CorridorCeiling");
            Block(91, 4, 1, 7, "AboveDoor2");
            Block(91, 11, 19, 3, "BossCeiling");
            Block(108, 0, 3, 11, "WallRight");

            // ---- hazards ----
            InLevel(EntityFactory.CreateSpikes(new Vector2(52, 4), 3));
            InLevel(EntityFactory.CreateKillZone(new Rect(-4, -9, 118, 2)));

            // ---- ladder + checkpoints ----
            InLevel(EntityFactory.CreateLadder(41.5f, 0f, 8f));
            InLevel(EntityFactory.CreateCheckpoint(new Vector2(2, 0), 0));     // spawn
            InLevel(EntityFactory.CreateCheckpoint(new Vector2(44, 8), 1));    // top of ladder
            InLevel(EntityFactory.CreateCheckpoint(new Vector2(74, 0), 2));    // before boss

            // ---- enemies (spawners respawn them NES-style) ----
            Spawn(EnemyKind.Met, 8, 0);
            Spawn(EnemyKind.Met, 20, 0);
            Spawn(EnemyKind.Walker, 33, 0);
            Spawn(EnemyKind.Flyer, 47, 10);
            Spawn(EnemyKind.Walker, 49, 8);
            Spawn(EnemyKind.Met, 57, 8);
            Spawn(EnemyKind.Met, 76, 0);

            // ---- items ----
            Item(PickupType.HealthSmall, 16, 0);
            Item(PickupType.EnergySmall, 30, 0);
            Item(PickupType.WTank, 26, 2);            // on the pit platform
            Item(PickupType.HealthLarge, 44.8f, 8);
            Item(PickupType.HealthSmall, 58, 8);
            Item(PickupType.ETank, 67.5f, 10);        // past the slide corridor
            Item(PickupType.OneUp, 69, 8);            // at the cliff edge
            Item(PickupType.EnergyLarge, 88, 0);      // boss corridor

            // ---- boss ----
            InLevel(EntityFactory.CreateBossDoor(new Vector2(84, 0), false));
            var finalDoorGo = EntityFactory.CreateBossDoor(new Vector2(91, 0), true);
            InLevel(finalDoorGo);
            var finalDoor = finalDoorGo.GetComponent<BossDoor>();
            finalDoor.roomBounds = new Rect(92, 0, 16, 12);
            var boss = EntityFactory.CreateBoss(new Vector2(100, 0));
            boss.transform.SetParent(enemies, true);
            finalDoor.boss = boss.GetComponent<BossController>();
        }

        static void Block(float x, float y, float w, float h, string name)
        {
            InLevel(EntityFactory.CreateBlock(new Rect(x, y, w, h), BlockTint, name));
        }

        static void Spawn(EnemyKind kind, float x, float y)
        {
            EntityFactory.CreateSpawner(kind, new Vector2(x, y), -1)
                .transform.SetParent(enemies, true);
        }

        static void Item(PickupType type, float x, float y)
        {
            EntityFactory.CreatePickup(type, new Vector2(x, y), false)
                .transform.SetParent(pickups, true);
        }

        static void InLevel(GameObject go) => go.transform.SetParent(level, true);
    }
}
