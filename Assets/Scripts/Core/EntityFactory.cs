using UnityEngine;

namespace MB
{
    /// Creates every game object in the game -- used at runtime (projectiles,
    /// drops, effects) AND by the editor Tools menu (players, enemies, level
    /// pieces, whole stages). Keeping construction here means scenes never
    /// depend on prefabs or serialized sprite assets.
    public static class EntityFactory
    {
        static GameObject NewGO(string name, Vector2 pos, int layer)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one;   // colliders assume unit scale
            go.layer = layer;
            return go;
        }

        /// Find-or-create an organizational root (Level / Enemies / Pickups /
        /// Doodads) at the origin with identity scale. Parenting under these
        /// is safe because they never move or scale.
        public static Transform GetGroup(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) go = new GameObject(name);
            go.transform.position = Vector3.zero;
            go.transform.localScale = Vector3.one;
            return go.transform;
        }

        static GameObject AddHitbox(GameObject root, Vector2 size, Vector2 offset)
        {
            var child = new GameObject("Hitbox");
            child.layer = Layers.Enemy;
            child.transform.SetParent(root.transform, false);
            var box = child.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = size;
            box.offset = offset;
            return child;
        }

        // ---------------- player ----------------

        public static GameObject CreatePlayer(Vector2 pos)
        {
            var go = NewGO("Player", pos, Layers.Player);
            go.tag = "Player";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.75f, 1.35f);
            box.offset = new Vector2(0f, 0.675f);

            AutoSprite.Attach(go, "player_idle", Color.white, 10);

            go.AddComponent<PlayerController>();
            go.AddComponent<PlayerHealth>();
            go.AddComponent<WeaponSystem>();
            go.AddComponent<PlayerShooter>();
            go.AddComponent<PlayerVisual>();
            return go;
        }

        // ---------------- enemies ----------------

        public static GameObject CreateEnemy(EnemyKind kind, Vector2 pos, int facing)
        {
            switch (kind)
            {
                case EnemyKind.Walker: return CreateWalker(pos, facing);
                case EnemyKind.Flyer: return CreateFlyer(pos);
                default: return CreateMet(pos);
            }
        }

        static GameObject CreateMet(Vector2 pos)
        {
            var go = NewGO("Met", pos, 0);
            AutoSprite.Attach(go, "met_closed", Color.white, 8);
            AddHitbox(go, new Vector2(0.8f, 0.6f), new Vector2(0f, 0.3f));
            go.AddComponent<MetEnemy>();
            return go;
        }

        static GameObject CreateWalker(Vector2 pos, int facing)
        {
            var go = NewGO("Walker", pos, Layers.Body);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 3f;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.7f, 0.55f);
            box.offset = new Vector2(0f, 0.3f);
            AutoSprite.Attach(go, "walker_0", Color.white, 8);
            AddHitbox(go, new Vector2(0.85f, 0.65f), new Vector2(0f, 0.35f));
            var w = go.AddComponent<WalkerEnemy>();
            w.dir = facing == 0 ? -1 : facing;
            return go;
        }

        static GameObject CreateFlyer(Vector2 pos)
        {
            var go = NewGO("Flyer", pos, Layers.Body);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            AutoSprite.Attach(go, "flyer_0", Color.white, 8);
            AddHitbox(go, new Vector2(0.7f, 0.5f), Vector2.zero);
            go.AddComponent<FlyerEnemy>();
            return go;
        }

        public static GameObject CreateBoss(Vector2 pos)
        {
            var go = NewGO("Boss", pos, Layers.Body);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 4.5f;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1.0f, 1.85f);
            box.offset = new Vector2(0f, 0.95f);
            AutoSprite.Attach(go, "boss_idle", Color.white, 9);
            AddHitbox(go, new Vector2(1.0f, 1.8f), new Vector2(0f, 0.95f));
            go.AddComponent<BossController>();
            return go;
        }

        public static GameObject CreateSpawner(EnemyKind kind, Vector2 pos, int facing)
        {
            var go = NewGO("Spawner_" + kind, pos, 0);
            string preview = kind == EnemyKind.Walker ? "walker_0"
                : kind == EnemyKind.Flyer ? "flyer_0" : "met_closed";
            AutoSprite.Attach(go, preview, new Color(1, 1, 1, 0.55f), 8);
            var sp = go.AddComponent<EnemySpawner>();
            sp.kind = kind;
            sp.facing = facing;
            return go;
        }

        // ---------------- projectiles ----------------

        static GameObject NewShot(string name, Vector2 pos, int layer, string key, Color tint, float radius)
        {
            var go = NewGO(name, pos, layer);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = radius;
            AutoSprite.Attach(go, key, tint, 12);
            return go;
        }

        public static GameObject CreateBusterShot(Vector2 pos, int facing, int tier)
        {
            string key = tier == 2 ? "bullet_full" : tier == 1 ? "bullet_mid" : "bullet";
            Color tint = tier == 0 ? new Color(1f, 0.92f, 0.55f) : new Color(0.6f, 0.88f, 1f);
            float radius = tier == 2 ? 0.4f : tier == 1 ? 0.25f : 0.15f;
            int dmg = tier == 2 ? 3 : tier == 1 ? 2 : 1;
            float speed = 15f + tier;

            var go = NewShot("BusterShot", pos, Layers.PlayerShot, key, tint, radius);
            var p = go.AddComponent<Projectile>();
            p.damage = dmg;
            p.weapon = WeaponId.Buster;
            p.fromPlayer = true;
            p.Launch(new Vector2(speed * facing, 0));
            return go;
        }

        public static GameObject CreateCutter(Vector2 pos, int facing, Transform owner)
        {
            var go = NewShot("Cutter", pos, Layers.PlayerShot, "cutter", Color.white, 0.3f);
            var c = go.AddComponent<CutterProjectile>();
            c.damage = 3;
            c.weapon = WeaponId.Slicer;
            c.fromPlayer = true;
            c.pierce = true;
            c.lifetime = 5f;
            c.owner = owner;
            c.Launch(new Vector2(10.5f * facing, 2.5f));
            return go;
        }

        public static GameObject CreateBomb(Vector2 pos, int facing)
        {
            var go = NewGO("Bomb", pos, Layers.PlayerShot);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3.5f;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.28f;
            AutoSprite.Attach(go, "bomb", Color.white, 12);
            go.AddComponent<BombProjectile>();
            rb.linearVelocity = new Vector2(6.5f * facing, 9.5f);
            return go;
        }

        public static GameObject CreateEnemyShot(Vector2 pos, Vector2 velocity, int dmg)
        {
            var go = NewShot("EnemyShot", pos, Layers.EnemyShot, "eshot", Color.white, 0.18f);
            var p = go.AddComponent<Projectile>();
            p.damage = dmg;
            p.fromPlayer = false;
            p.lifetime = 4f;
            p.Launch(velocity);
            return go;
        }

        // ---------------- effects & drops ----------------

        public static void CreateExplosion(Vector3 pos, bool big)
        {
            var go = NewGO("Explosion", pos, 0);
            AutoSprite.Attach(go, "orb", new Color(1f, 0.65f, 0.25f), 15);
            var fx = go.AddComponent<ExplosionFx>();
            if (big) { fx.duration = 0.5f; fx.startScale = 1f; fx.endScale = 3.5f; }
        }

        public static void CreateDeathOrbs(Vector3 pos, Color color)
        {
            for (int i = 0; i < 8; i++) SpawnOrb(pos, i * 45f, 4.5f, color);
            for (int i = 0; i < 4; i++) SpawnOrb(pos, i * 90f + 45f, 2.2f, color);
        }

        static void SpawnOrb(Vector3 pos, float angleDeg, float speed, Color color)
        {
            var go = NewGO("Orb", pos, 0);
            AutoSprite.Attach(go, "orb", color, 15);
            var d = go.AddComponent<DeathOrb>();
            float rad = angleDeg * Mathf.Deg2Rad;
            d.velocity = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * speed;
        }

        public static void SpawnDrop(Vector3 pos)
        {
            float r = Random.value;
            PickupType type;
            if (r < 0.40f) return;
            else if (r < 0.62f) type = PickupType.HealthSmall;
            else if (r < 0.72f) type = PickupType.HealthLarge;
            else if (r < 0.87f) type = PickupType.EnergySmall;
            else if (r < 0.95f) type = PickupType.EnergyLarge;
            else if (r < 0.98f) return;
            else type = PickupType.OneUp;
            CreatePickup(type, pos, true);
        }

        public static GameObject CreatePickup(PickupType type, Vector2 pos, bool dropped)
        {
            string key; Color tint;
            switch (type)
            {
                case PickupType.HealthSmall: key = "hp_small"; tint = new Color(1f, 0.8f, 0.75f); break;
                case PickupType.HealthLarge: key = "hp_big"; tint = new Color(1f, 0.8f, 0.75f); break;
                case PickupType.EnergySmall: key = "en_small"; tint = new Color(1f, 1f, 0.45f); break;
                case PickupType.EnergyLarge: key = "en_big"; tint = new Color(1f, 1f, 0.45f); break;
                case PickupType.ETank: key = "etank"; tint = new Color(0.55f, 0.75f, 1f); break;
                case PickupType.WTank: key = "wtank"; tint = new Color(1f, 0.7f, 0.35f); break;
                default: key = "oneup"; tint = Color.white; break;
            }

            GameObject go;
            if (dropped)
            {
                go = NewGO("Drop_" + type, pos, Layers.Body);
                var rb = go.AddComponent<Rigidbody2D>();
                rb.freezeRotation = true;
                rb.gravityScale = 3f;
                var solid = go.AddComponent<CircleCollider2D>();
                solid.radius = 0.22f;
                solid.offset = new Vector2(0f, 0.22f);

                var trig = new GameObject("Trigger");
                trig.layer = Layers.Item;
                trig.transform.SetParent(go.transform, false);
                var tb = trig.AddComponent<BoxCollider2D>();
                tb.isTrigger = true;
                tb.size = new Vector2(0.7f, 0.7f);
                tb.offset = new Vector2(0f, 0.3f);

                var pk = go.AddComponent<Pickup>();
                pk.type = type;
                pk.despawns = true;
                trig.AddComponent<TriggerRelay>().onEnter = pk.TryCollect;
            }
            else
            {
                go = NewGO("Pickup_" + type, pos, Layers.Item);
                var tb = go.AddComponent<BoxCollider2D>();
                tb.isTrigger = true;
                tb.size = new Vector2(0.7f, 0.8f);
                tb.offset = new Vector2(0f, 0.4f);
                var pk = go.AddComponent<Pickup>();
                pk.type = type;
                pk.despawns = false;
            }

            AutoSprite.Attach(go, key, tint, 5);
            return go;
        }

        // ---------------- level pieces ----------------

        public static GameObject CreateBlock(Rect rect, Color tint, string name = "Block")
        {
            var go = NewGO(name, rect.min, Layers.Ground);
            AutoSprite.Attach(go, "block", tint, 0, new Vector2(rect.width, rect.height));
            var box = go.AddComponent<BoxCollider2D>();
            box.size = rect.size;
            box.offset = rect.size / 2f;
            return go;
        }

        public static GameObject CreateSpikes(Vector2 leftBase, int tiles)
        {
            var go = NewGO("Spikes", leftBase + new Vector2(tiles / 2f, 0), Layers.Hazard);
            AutoSprite.Attach(go, "spike", Color.white, 4, new Vector2(tiles, 0.5f));
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(tiles - 0.2f, 0.3f);
            box.offset = new Vector2(0f, 0.15f);
            go.AddComponent<Hazard>().instantKill = true;
            return go;
        }

        public static GameObject CreateKillZone(Rect rect)
        {
            var go = NewGO("KillZone", rect.center, Layers.Hazard);
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = rect.size;
            go.AddComponent<Hazard>().instantKill = true;
            return go;
        }

        public static GameObject CreateLadder(float centerX, float baseY, float height)
        {
            var go = NewGO("Ladder", new Vector2(centerX, baseY), Layers.Ladder);
            AutoSprite.Attach(go, "ladder", new Color(0.95f, 0.8f, 0.55f), 1, new Vector2(1f, height));
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(0.9f, height + 0.7f);
            box.offset = new Vector2(0f, height / 2f + 0.3f);

            var ladder = go.AddComponent<Ladder>();
            ladder.height = height;

            var top = new GameObject("LadderTop");
            top.layer = Layers.Ground;
            top.transform.SetParent(go.transform, false);
            top.transform.localPosition = new Vector3(0f, height, 0f);
            var tb = top.AddComponent<BoxCollider2D>();
            tb.size = new Vector2(1.1f, 0.12f);
            tb.offset = new Vector2(0f, -0.06f);
            tb.usedByEffector = true;
            var eff = top.AddComponent<PlatformEffector2D>();
            eff.useOneWay = true;
            eff.surfaceArc = 170f;
            ladder.TopPlatform = tb;
            return go;
        }

        public static GameObject CreateCheckpoint(Vector2 pos, int index)
        {
            var go = NewGO("Checkpoint_" + index, pos, Layers.Item);
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1f, 3f);
            box.offset = new Vector2(0f, 1.5f);
            go.AddComponent<Checkpoint>().index = index;
            return go;
        }

        public static GameObject CreateBossDoor(Vector2 basePos, bool isFinal)
        {
            var go = NewGO(isFinal ? "BossDoor_Final" : "BossDoor", basePos, Layers.Ground);
            AutoSprite.Attach(go, "door", Color.white, 6, new Vector2(1f, 4f));
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1f, 4f);
            box.offset = new Vector2(0.5f, 2f);

            var door = go.AddComponent<BossDoor>();
            door.isFinal = isFinal;

            var trig = new GameObject("DoorTrigger");
            trig.layer = Layers.Item;
            trig.transform.SetParent(go.transform, false);
            trig.transform.localPosition = new Vector3(-0.5f, 2f, 0f);
            var tb = trig.AddComponent<BoxCollider2D>();
            tb.isTrigger = true;
            tb.size = new Vector2(1.0f, 3.6f);
            trig.AddComponent<BossDoorTrigger>();
            return go;
        }

        public static GameObject CreateStageEssentials(Rect bounds, Color theme)
        {
            var go = new GameObject("StageController");
            var sc = go.AddComponent<StageController>();
            sc.stageBounds = bounds;
            sc.theme = theme;

            var cam = Camera.main;
            if (cam == null)
            {
                var cgo = new GameObject("Main Camera");
                cgo.tag = "MainCamera";
                cam = cgo.AddComponent<Camera>();
                cgo.AddComponent<AudioListener>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            var bg = theme * 0.16f; bg.a = 1f;
            cam.backgroundColor = bg;
            cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
            var cf = cam.GetComponent<CameraFollow>();
            if (cf == null) cf = cam.gameObject.AddComponent<CameraFollow>();
            cf.bounds = bounds;
            return go;
        }
    }
}
