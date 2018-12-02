using UnityEditor;

namespace Game.Building
{
    public static class Builder
    {
        private static string GetScriptingDefineSymbols(out string previousSymbols)
        {
            // Get and store current scripting define symbols
            string symbols = previousSymbols =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings
                    .selectedBuildTargetGroup);

            if (!string.IsNullOrEmpty(symbols))
            {
                symbols += ";";
            }

            return symbols;
        }

        private static void RestoreScriptingDefineSymbols(string previousSymbols)
        {
            if (previousSymbols == null)
            {
                return;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup, previousSymbols);
        }

        private static void SwitchTarget(BuildTarget buildTarget)
        {
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);

            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
        }

        public static void Build(string path, BuildTarget buildTarget, BuildOptions buildOptions,
            string customSymbols = null)
        {
            // Switch to build target platform
            SwitchTarget(buildTarget);

            string previousSymbols = null;

            if (customSymbols != null)
            {
                // Get current scripting define symbols
                string symbols = GetScriptingDefineSymbols(out previousSymbols);
                symbols += customSymbols;

                // Set define symbols
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    symbols);
            }

            // Make build
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path, buildTarget, buildOptions);

            if (previousSymbols != null)
            {
                // Restore to previous scripting define symbols
                RestoreScriptingDefineSymbols(previousSymbols);
            }

            // Save changes to modified assets
            AssetDatabase.SaveAssets();
        }
    }
}
