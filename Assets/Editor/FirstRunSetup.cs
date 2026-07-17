using UnityEditor;
using UnityEngine;

namespace MB.EditorTools
{
    /// Offers to generate the game scenes the first time the project opens.
    [InitializeOnLoad]
    static class FirstRunSetup
    {
        const string Flag = "mb_first_run_prompted";

        static FirstRunSetup()
        {
            EditorApplication.delayCall += Check;
        }

        static void Check()
        {
            if (SessionState.GetBool(Flag, false)) return;
            SessionState.SetBool(Flag, true);
            if (System.IO.File.Exists("Assets/Scenes/Title.unity")) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            if (EditorUtility.DisplayDialog("MegaBot – first run",
                "This project generates its scenes and placeholder content from code.\n\n" +
                "Generate the Title, Stage Select and Demo Stage scenes now?\n" +
                "(You can always do it later via Tools > Mega Man > Setup Project.)",
                "Generate", "Later"))
            {
                SceneBuilder.BuildAll();
            }
        }
    }
}
