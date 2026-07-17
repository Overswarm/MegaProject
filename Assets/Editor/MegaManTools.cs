using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MB.EditorTools
{
    /// The Tools > Mega Man menu: one-click project setup plus creators for
    /// every important game object, so nothing has to be assembled by hand.
    /// New objects are placed at the Scene view pivot, snapped to the tile grid.
    public static class MegaManTools
    {
        // ---------- setup ----------

        [MenuItem("Tools/Mega Man/Setup Project (Generate Scenes)", false, 0)]
        static void Setup() => SceneBuilder.BuildAll();

        [MenuItem("Tools/Mega Man/Open Demo Stage", false, 1)]
        static void OpenDemo()
        {
            const string path = "Assets/Scenes/DemoStage.unity";
            if (System.IO.File.Exists(path)) EditorSceneManager.OpenScene(path);
            else if (EditorUtility.DisplayDialog("Mega Man",
                "DemoStage.unity doesn't exist yet. Generate the scenes now?", "Generate", "Cancel"))
                SceneBuilder.BuildAll();
        }

        [MenuItem("Tools/Mega Man/Fix Input Handler (enable Input System)", false, 2)]
        static void FixInputHandler()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0) return;
            var so = new SerializedObject(assets[0]);
            var prop = so.FindProperty("activeInputHandler");
            if (prop != null)
            {
                // 1 = Input System Package only. 'Both' is known to cause
                // InvalidCastException spam in the Input System's event processing.
                prop.intValue = 1;
                so.ApplyModifiedProperties();
                EditorUtility.DisplayDialog("Mega Man",
                    "Active Input Handling set to 'Input System Package (New)'. " +
                    "Restart the editor for it to take effect.", "OK");
            }
        }

        [MenuItem("Tools/Mega Man/Fix Object Scales In Scene", false, 5)]
        static void FixScales()
        {
            int count = 0;
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (t.localScale == Vector3.one) continue;
                bool gameplayObject = t.GetComponent<Collider2D>() != null ||
                                      t.GetComponent<AutoSprite>() != null ||
                                      t.GetComponent<EnemySpawner>() != null;
                if (!gameplayObject) continue;
                Undo.RecordObject(t, "Fix Scale");
                t.localScale = Vector3.one;
                count++;
                EditorSceneManager.MarkSceneDirty(t.gameObject.scene);
            }
            EditorUtility.DisplayDialog("Mega Man - Fix Scales",
                count == 0
                    ? "All gameplay objects already have a scale of (1,1,1)."
                    : $"Reset scale to (1,1,1) on {count} object(s).\n\n" +
                      "Colliders and tiled sprites in this project are sized via " +
                      "component values, so transform scale must stay at 1.",
                "OK");
        }

        [MenuItem("Tools/Mega Man/Validate Project", false, 3)]
        static void Validate()
        {
            var report = new System.Text.StringBuilder();
            bool ok = true;

            // 1. physics layers from TagManager.asset
            var expected = new (int index, string name)[]
            {
                (6, "Ground"), (7, "Player"), (8, "Enemy"), (9, "PlayerShot"),
                (10, "EnemyShot"), (11, "Item"), (12, "Ladder"), (13, "Hazard"), (14, "Body"),
            };
            var missing = new System.Collections.Generic.List<string>();
            foreach (var (index, name) in expected)
                if (LayerMask.LayerToName(index) != name) missing.Add($"{index}={name}");
            if (missing.Count == 0)
                report.AppendLine("[OK] Physics layers 6-14 are set up.");
            else
            {
                ok = false;
                report.AppendLine("[FAIL] Missing layers: " + string.Join(", ", missing));
                report.AppendLine("       TagManager.asset was not loaded as shipped. Re-add these");
                report.AppendLine("       names at these indices in Project Settings > Tags and Layers.");
            }

            // 2. active input handler (0 = old only, 1 = new only, 2 = both)
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            var prop = assets.Length > 0
                ? new SerializedObject(assets[0]).FindProperty("activeInputHandler") : null;
            if (prop == null || prop.intValue == 0)
            {
                ok = false;
                report.AppendLine("[FAIL] Active Input Handling excludes the Input System.");
                report.AppendLine("       Run Tools > Mega Man > Fix Input Handler, then restart.");
            }
            else if (prop.intValue == 2)
            {
                report.AppendLine("[WARN] Active Input Handling is 'Both' - known to cause");
                report.AppendLine("       InvalidCastException spam. Run Fix Input Handler (sets New only).");
            }
            else report.AppendLine("[OK] Active Input Handling = Input System only.");

            // 3. render pipeline
            if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null)
                report.AppendLine("[OK] URP 2D render pipeline is active.");
            else
            {
                ok = false;
                report.AppendLine("[FAIL] No render pipeline asset is assigned.");
                report.AppendLine("       Run Tools > Mega Man > Setup URP 2D Pipeline.");
            }

            // 4. non-unit scales on physics objects in the OPEN scene
            int scaled = 0;
            foreach (var col in Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None))
                if ((col.transform.lossyScale - Vector3.one).sqrMagnitude > 1e-6f) scaled++;
            if (scaled == 0)
                report.AppendLine("[OK] All colliders in the open scene are at scale (1,1,1).");
            else
            {
                ok = false;
                report.AppendLine($"[FAIL] {scaled} collider object(s) have a non-unit scale.");
                report.AppendLine("       Run Tools > Mega Man > Fix Object Scales In Scene.");
            }

            // 5. scenes generated and in build settings
            string[] scenes = { "Title", "StageSelect", "DemoStage" };
            var inBuild = new System.Collections.Generic.HashSet<string>();
            foreach (var s in EditorBuildSettings.scenes)
                inBuild.Add(System.IO.Path.GetFileNameWithoutExtension(s.path));
            foreach (var s in scenes)
            {
                bool exists = System.IO.File.Exists($"Assets/Scenes/{s}.unity");
                if (exists && inBuild.Contains(s))
                    report.AppendLine($"[OK] Scene {s} exists and is in Build Settings.");
                else
                {
                    ok = false;
                    report.AppendLine(exists
                        ? $"[FAIL] Scene {s} is not in Build Settings."
                        : $"[FAIL] Scene {s} has not been generated.");
                }
            }
            if (!ok && !inBuild.Contains("Title"))
                report.AppendLine("       Run Tools > Mega Man > Setup Project to fix scene issues.");

            report.AppendLine(ok ? "\nAll good - open Assets/Scenes/Title.unity and press Play."
                                 : "\nFix the items above, then validate again.");
            EditorUtility.DisplayDialog("Mega Man - Validate Project", report.ToString(), "OK");
            Debug.Log("[MegaMan Validate]\n" + report);
        }

        // ---------- placement helpers ----------

        static Vector2 SpawnPos()
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv == null) return Vector2.zero;
            var p = sv.pivot;
            return new Vector2(Mathf.Round(p.x), Mathf.Round(p.y));
        }

        static void Finish(GameObject go, string label, string group = null)
        {
            if (group != null)
                go.transform.SetParent(EntityFactory.GetGroup(group), true);
            Undo.RegisterCreatedObjectUndo(go, label);
            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(go.scene);
        }

        // ---------- stage ----------

        [MenuItem("Tools/Mega Man/Create/Stage Essentials (Controller + Camera)", false, 20)]
        static void CreateStage()
        {
            var go = EntityFactory.CreateStageEssentials(
                new Rect(-2, -4, 100, 24), new Color(0.35f, 0.4f, 0.55f));
            Finish(go, "Stage Essentials");
        }

        [MenuItem("Tools/Mega Man/Create/Player", false, 21)]
        static void CreatePlayer() => Finish(EntityFactory.CreatePlayer(SpawnPos()), "Player");

        // ---------- level pieces ----------

        [MenuItem("Tools/Mega Man/Create/Level/Platform Block (4x1)", false, 40)]
        static void CreateBlock() => Finish(EntityFactory.CreateBlock(
            new Rect(SpawnPos(), new Vector2(4, 1)), new Color(0.72f, 0.72f, 0.8f)), "Block", "Level");

        [MenuItem("Tools/Mega Man/Create/Level/Ground Slab (8x3)", false, 41)]
        static void CreateSlab() => Finish(EntityFactory.CreateBlock(
            new Rect(SpawnPos() - new Vector2(0, 3), new Vector2(8, 3)), new Color(0.72f, 0.72f, 0.8f)), "Ground", "Level");

        [MenuItem("Tools/Mega Man/Create/Level/Ladder (8 tall)", false, 42)]
        static void CreateLadder()
        {
            var p = SpawnPos();
            Finish(EntityFactory.CreateLadder(p.x + 0.5f, p.y, 8f), "Ladder", "Level");
        }

        [MenuItem("Tools/Mega Man/Create/Level/Spikes (3 wide)", false, 43)]
        static void CreateSpikes() => Finish(EntityFactory.CreateSpikes(SpawnPos(), 3), "Spikes", "Level");

        [MenuItem("Tools/Mega Man/Create/Level/Kill Zone (pit, 8 wide)", false, 44)]
        static void CreateKillZone() => Finish(EntityFactory.CreateKillZone(
            new Rect(SpawnPos() - new Vector2(4, 1), new Vector2(8, 2))), "Kill Zone", "Level");

        [MenuItem("Tools/Mega Man/Create/Level/Checkpoint", false, 45)]
        static void CreateCheckpoint()
        {
            int next = 1;
            foreach (var c in Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None))
                next = Mathf.Max(next, c.index + 1);
            Finish(EntityFactory.CreateCheckpoint(SpawnPos(), next), "Checkpoint", "Level");
        }

        [MenuItem("Tools/Mega Man/Create/Level/Boss Door", false, 46)]
        static void CreateDoor() => Finish(EntityFactory.CreateBossDoor(SpawnPos(), false), "Boss Door", "Level");

        [MenuItem("Tools/Mega Man/Create/Level/Boss Door (Final, opens boss room)", false, 47)]
        static void CreateFinalDoor()
        {
            var go = EntityFactory.CreateBossDoor(SpawnPos(), true);
            var door = go.GetComponent<BossDoor>();
            door.roomBounds = new Rect(SpawnPos().x + 1, SpawnPos().y, 16, 12);
            Finish(go, "Boss Door (Final)", "Level");
            Debug.Log("Final boss door created. Set its Room Bounds and drag the Boss reference in the Inspector.");
        }

        [MenuItem("Tools/Mega Man/Create/Organization Groups", false, 48)]
        static void CreateGroups()
        {
            EntityFactory.GetGroup("Level");
            EntityFactory.GetGroup("Enemies");
            EntityFactory.GetGroup("Pickups");
            var doodads = EntityFactory.GetGroup("Doodads");
            Selection.activeGameObject = doodads.gameObject;
            EditorSceneManager.MarkSceneDirty(doodads.gameObject.scene);
        }

        // ---------- enemies ----------

        [MenuItem("Tools/Mega Man/Create/Enemies/Met Spawner", false, 60)]
        static void CreateMet() => Finish(EntityFactory.CreateSpawner(EnemyKind.Met, SpawnPos(), -1), "Met", "Enemies");

        [MenuItem("Tools/Mega Man/Create/Enemies/Walker Spawner", false, 61)]
        static void CreateWalker() => Finish(EntityFactory.CreateSpawner(EnemyKind.Walker, SpawnPos(), -1), "Walker", "Enemies");

        [MenuItem("Tools/Mega Man/Create/Enemies/Flyer Spawner", false, 62)]
        static void CreateFlyer() => Finish(EntityFactory.CreateSpawner(EnemyKind.Flyer, SpawnPos(), -1), "Flyer", "Enemies");

        [MenuItem("Tools/Mega Man/Create/Enemies/Boss", false, 63)]
        static void CreateBoss() => Finish(EntityFactory.CreateBoss(SpawnPos()), "Boss", "Enemies");

        // ---------- items ----------

        [MenuItem("Tools/Mega Man/Create/Items/E-Tank", false, 80)]
        static void CreateETank() => Finish(EntityFactory.CreatePickup(PickupType.ETank, SpawnPos(), false), "E-Tank", "Pickups");

        [MenuItem("Tools/Mega Man/Create/Items/W-Tank", false, 81)]
        static void CreateWTank() => Finish(EntityFactory.CreatePickup(PickupType.WTank, SpawnPos(), false), "W-Tank", "Pickups");

        [MenuItem("Tools/Mega Man/Create/Items/Health (Small)", false, 82)]
        static void CreateHpS() => Finish(EntityFactory.CreatePickup(PickupType.HealthSmall, SpawnPos(), false), "Health", "Pickups");

        [MenuItem("Tools/Mega Man/Create/Items/Health (Large)", false, 83)]
        static void CreateHpL() => Finish(EntityFactory.CreatePickup(PickupType.HealthLarge, SpawnPos(), false), "Health", "Pickups");

        [MenuItem("Tools/Mega Man/Create/Items/Weapon Energy (Small)", false, 84)]
        static void CreateEnS() => Finish(EntityFactory.CreatePickup(PickupType.EnergySmall, SpawnPos(), false), "Energy", "Pickups");

        [MenuItem("Tools/Mega Man/Create/Items/Weapon Energy (Large)", false, 85)]
        static void CreateEnL() => Finish(EntityFactory.CreatePickup(PickupType.EnergyLarge, SpawnPos(), false), "Energy", "Pickups");

        [MenuItem("Tools/Mega Man/Create/Items/1-Up", false, 86)]
        static void CreateOneUp() => Finish(EntityFactory.CreatePickup(PickupType.OneUp, SpawnPos(), false), "1-Up", "Pickups");
    }
}
