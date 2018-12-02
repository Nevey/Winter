using UnityEditor;
using UnityEngine;

namespace Game.Building
{
    public class BuildEditorWindow : EditorWindow
    {
        private BuildTarget buildTarget;

        private BuildOptions buildOptions;

        private string path;

        private string previousSymbols;

        private GUIStyle style;

        [MenuItem("Winter/Build")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            BuildEditorWindow window = (BuildEditorWindow)GetWindow(typeof(BuildEditorWindow));
            window.Show();

            window.minSize = new Vector2(500, 300);
            window.maxSize = window.minSize;

            window.name = "Build Window";
            window.titleContent = new GUIContent("Build Window");
        }

        private void Awake()
        {
            buildTarget = EditorUserBuildSettings.activeBuildTarget;

            style = new GUIStyle();

            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 14;

            EditorGUILayout.LabelField("WINTER BUILD WINDOW", style);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            style.fontStyle = FontStyle.Normal;
            style.fontSize = 12;

            EditorGUILayout.LabelField("BUILD TARGET SETTINGS", style);

            EditorGUILayout.Space();

            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", buildTarget);

            buildOptions = (BuildOptions)EditorGUILayout.EnumFlagsField("Build Options", buildOptions);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("BUILD PATH SETTINGS", style);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Current Path", path);

            if (GUILayout.Button("Select Path"))
            {
                path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("SELECT BUILD TYPE", style);

            EditorGUILayout.Space();

            if (GUILayout.Button("Build Server"))
            {
                Build(path + "/Server/Winter_Server.exe", "SERVER_BUILD");
            }

            if (GUILayout.Button("Build Client"))
            {
                Build(path + "/Client/Winter_Client.exe", "CLIENT_BUILD");
            }
        }

        private string GetScriptingDefineSymbols()
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

        private void RestoreScriptingDefineSymbols()
        {
            if (previousSymbols == null)
            {
                return;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup, previousSymbols);
        }

        private void SwitchTarget()
        {
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);

            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
        }

        private void Build(string path, string customSymbols = null)
        {
            // Switch to build target platform
            SwitchTarget();

            if (customSymbols != null)
            {
                // Get current scripting define symbols
                string symbols = GetScriptingDefineSymbols();
                symbols += customSymbols;

                // Set define symbols
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    symbols);
            }

            // Make build
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path, buildTarget, buildOptions);

            // Restore to previous scripting define symbols
            RestoreScriptingDefineSymbols();

            // Save changes to modified assets
            AssetDatabase.SaveAssets();
        }
    }
}
