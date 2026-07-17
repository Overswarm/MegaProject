using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MB.EditorTools
{
    /// Creates and activates a URP asset with the 2D Renderer (the "Universal
    /// 2D" template setup). Assets are generated through Unity's own APIs so
    /// they always match the installed URP version.
    ///
    /// Note: the game's placeholder sprites use the unlit Sprites/Default
    /// material, so they render identically with or without 2D lights. To use
    /// 2D lighting later, switch sprite materials to Sprite-Lit-Default and
    /// add a Global Light 2D to the scene.
    public static class UrpSetup
    {
        const string Dir = "Assets/Settings";
        const string RendererPath = Dir + "/Renderer2D.asset";
        const string PipelinePath = Dir + "/URP-2D-Pipeline.asset";

        [MenuItem("Tools/Mega Man/Setup URP 2D Pipeline", false, 4)]
        static void EnsureInteractive()
        {
            bool changed = Ensure();
            EditorUtility.DisplayDialog("Mega Man - URP 2D",
                changed
                    ? "URP with the 2D Renderer is now the active render pipeline.\n\n" +
                      "Assets created in Assets/Settings/."
                    : "URP 2D was already the active render pipeline - nothing to do.",
                "OK");
        }

        /// Returns true if the pipeline was created/activated, false if it was
        /// already in place. Called automatically by Setup Project.
        public static bool Ensure()
        {
            bool alreadyActive = GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset;

            if (!AssetDatabase.IsValidFolder(Dir))
                AssetDatabase.CreateFolder("Assets", "Settings");

            var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>(RendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<Renderer2DData>();
                AssetDatabase.CreateAsset(renderer, RendererPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            if (alreadyActive && GraphicsSettings.defaultRenderPipeline == pipeline)
                return false;

            GraphicsSettings.defaultRenderPipeline = pipeline;

            // clear per-quality-level overrides so the default applies everywhere
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(current, false);

            AssetDatabase.SaveAssets();
            Debug.Log("[MegaMan] URP 2D pipeline created and activated.");
            return true;
        }
    }
}
