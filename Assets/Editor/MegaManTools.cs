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
                prop.intValue = 2; // Both
                so.ApplyModifiedProperties();
                EditorUtility.DisplayDialog("Mega Man",
                    "Active Input Handling set to 'Both'. Restart the editor for it to take effect.", "OK");
            }
        }

        // ---------- placement helpers ----------

        static Vector2 SpawnPos()
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv == null) return Vector2.zero;
            var p = sv.pivot;
            return new Vector2(Mathf.Round(p.x), Mathf.Round(p.y));
        }

        static void Finish(GameObject go, string label)
        {
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
            new Rect(SpawnPos(), new Vector2(4, 1)), new Color(0.72f, 0.72f, 0.8f)), "Block");

        [MenuItem("Tools/Mega Man/Create/Level/Ground Slab (8x3)", false, 41)]
        static void CreateSlab() => Finish(EntityFactory.CreateBlock(
            new Rect(SpawnPos() - new Vector2(0, 3), new Vector2(8, 3)), new Color(0.72f, 0.72f, 0.8f)), "Ground");

        [MenuItem("Tools/Mega Man/Create/Level/Ladder (8 tall)", false, 42)]
        static void CreateLadder()
        {
            var p = SpawnPos();
            Finish(EntityFactory.CreateLadder(p.x + 0.5f, p.y, 8f), "Ladder");
        }

        [MenuItem("Tools/Mega Man/Create/Level/Spikes (3 wide)", false, 43)]
        static void CreateSpikes() => Finish(EntityFactory.CreateSpikes(SpawnPos(), 3), "Spikes");

        [MenuItem("Tools/Mega Man/Create/Level/Kill Zone (pit, 8 wide)", false, 44)]
        static void CreateKillZone() => Finish(EntityFactory.CreateKillZone(
            new Rect(SpawnPos() - new Vector2(4, 1), new Vector2(8, 2))), "Kill Zone");

        [MenuItem("Tools/Mega Man/Create/Level/Checkpoint", false, 45)]
        static void CreateCheckpoint()
        {
            int next = 1;
            foreach (var c in Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None))
                next = Mathf.Max(next, c.index + 1);
            Finish(EntityFactory.CreateCheckpoint(SpawnPos(), next), "Checkpoint");
        }

        [MenuItem("Tools/Mega Man/Create/Level/Boss Door", false, 46)]
        static void CreateDoor() => Finish(EntityFactory.CreateBossDoor(SpawnPos(), false), "Boss Door");

        [MenuItem("Tools/Mega Man/Create/Level/Boss Door (Final, opens boss room)", false, 47)]
        static void CreateFinalDoor()
        {
            var go = EntityFactory.CreateBossDoor(SpawnPos(), true);
            var door = go.GetComponent<BossDoor>();
            door.roomBounds = new Rect(SpawnPos().x + 1, SpawnPos().y, 16, 12);
            Finish(go, "Boss Door (Final)");
            Debug.Log("Final boss door created. Set its Room Bounds and drag the Boss reference in the Inspector.");
        }

        // ---------- enemies ----------

        [MenuItem("Tools/Mega Man/Create/Enemies/Met Spawner", false, 60)]
        static void CreateMet() => Finish(EntityFactory.CreateSpawner(EnemyKind.Met, SpawnPos(), -1), "Met");

        [MenuItem("Tools/Mega Man/Create/Enemies/Walker Spawner", false, 61)]
        static void CreateWalker() => Finish(EntityFactory.CreateSpawner(EnemyKind.Walker, SpawnPos(), -1), "Walker");

        [MenuItem("Tools/Mega Man/Create/Enemies/Flyer Spawner", false, 62)]
        static void CreateFlyer() => Finish(EntityFactory.CreateSpawner(EnemyKind.Flyer, SpawnPos(), -1), "Flyer");

        [MenuItem("Tools/Mega Man/Create/Enemies/Boss", false, 63)]
        static void CreateBoss() => Finish(EntityFactory.CreateBoss(SpawnPos()), "Boss");

        // ---------- items ----------

        [MenuItem("Tools/Mega Man/Create/Items/E-Tank", false, 80)]
        static void CreateETank() => Finish(EntityFactory.CreatePickup(PickupType.ETank, SpawnPos(), false), "E-Tank");

        [MenuItem("Tools/Mega Man/Create/Items/W-Tank", false, 81)]
        static void CreateWTank() => Finish(EntityFactory.CreatePickup(PickupType.WTank, SpawnPos(), false), "W-Tank");

        [MenuItem("Tools/Mega Man/Create/Items/Health (Small)", false, 82)]
        static void CreateHpS() => Finish(EntityFactory.CreatePickup(PickupType.HealthSmall, SpawnPos(), false), "Health");

        [MenuItem("Tools/Mega Man/Create/Items/Health (Large)", false, 83)]
        static void CreateHpL() => Finish(EntityFactory.CreatePickup(PickupType.HealthLarge, SpawnPos(), false), "Health");

        [MenuItem("Tools/Mega Man/Create/Items/Weapon Energy (Small)", false, 84)]
        static void CreateEnS() => Finish(EntityFactory.CreatePickup(PickupType.EnergySmall, SpawnPos(), false), "Energy");

        [MenuItem("Tools/Mega Man/Create/Items/Weapon Energy (Large)", false, 85)]
        static void CreateEnL() => Finish(EntityFactory.CreatePickup(PickupType.EnergyLarge, SpawnPos(), false), "Energy");

        [MenuItem("Tools/Mega Man/Create/Items/1-Up", false, 86)]
        static void CreateOneUp() => Finish(EntityFactory.CreatePickup(PickupType.OneUp, SpawnPos(), false), "1-Up");
    }
}
