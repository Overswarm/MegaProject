using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MB.EditorTools
{
    /// Generates the game's scenes (Title, StageSelect, DemoStage) and
    /// registers them in Build Settings. Everything inside the scenes is
    /// created through EntityFactory, the same code the Tools menu uses.
    public static class SceneBuilder
    {
        const string Dir = "Assets/Scenes";

        public static void BuildAll()
        {
            if (!AssetDatabase.IsValidFolder(Dir))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            string title = BuildSimple("Title", typeof(TitleScreen));
            string select = BuildSimple("StageSelect", typeof(StageSelectController));
            string demo = BuildDemoStage();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(title, true),
                new EditorBuildSettingsScene(select, true),
                new EditorBuildSettingsScene(demo, true),
            };

            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(title);
            EditorUtility.DisplayDialog("Mega Man Setup",
                "Generated scenes:\n  Title\n  StageSelect\n  DemoStage\n\n" +
                "All three were added to Build Settings.\n\n" +
                "Press Play in the Title scene to start.", "OK");
        }

        static Scene FreshScene()
        {
            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        static string BuildSimple(string name, System.Type controller)
        {
            var scene = FreshScene();

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            cam.transform.position = new Vector3(0, 0, -10);
            camGo.AddComponent<AudioListener>();

            new GameObject(name).AddComponent(controller);

            string path = $"{Dir}/{name}.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        static string BuildDemoStage()
        {
            var scene = FreshScene();
            DemoStageLayout.Populate();
            string path = $"{Dir}/DemoStage.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }
    }
}
