using System;
using UnityEditor;
using UnityEngine;

namespace Game.Building
{
    public class BuildEditorWindow : EditorWindow
    {
        private const string BUILD_TARGET_KEY = "buildTarget";
        private const string BUILD_OPTIONS_KEY = "buildOptions";
        private const string BUILD_PATH_KEY = "buildPath";

        private BuildTarget buildTarget;

        private BuildOptions buildOptions;

        private string buildPath;

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
            if (EditorPrefs.HasKey(BUILD_TARGET_KEY))
            {
                buildTarget = (BuildTarget)EditorPrefs.GetInt(BUILD_TARGET_KEY);
            }
            else
            {
                buildTarget = EditorUserBuildSettings.activeBuildTarget;
            }

            if (EditorPrefs.HasKey(BUILD_OPTIONS_KEY))
            {
                buildOptions = (BuildOptions)Enum.Parse(typeof(BuildOptions),
                    EditorPrefs.GetString(BUILD_OPTIONS_KEY));
            }
            else
            {
                buildOptions = BuildOptions.None;
            }

            buildPath = EditorPrefs.GetString(BUILD_PATH_KEY);

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

            EditorGUILayout.LabelField("Current Path", buildPath);

            if (GUILayout.Button("Select Path"))
            {
                buildPath = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("SELECT BUILD TYPE", style);

            EditorGUILayout.Space();

            if (GUILayout.Button("Build Server"))
            {
                SaveSettings();

                Builder.Build(buildPath + "/Server/Winter_Server.exe", buildTarget, buildOptions,
                    "SERVER_BUILD");
            }

            if (GUILayout.Button("Build Client"))
            {
                SaveSettings();

                Builder.Build(buildPath + "/Client/Winter_Client.exe", buildTarget, buildOptions,
                    "CLIENT_BUILD");
            }
        }

        private void SaveSettings()
        {
            EditorPrefs.SetInt(BUILD_TARGET_KEY, (int)buildTarget);
            EditorPrefs.SetString(BUILD_OPTIONS_KEY, buildOptions.ToString());
            EditorPrefs.SetString(BUILD_PATH_KEY, buildPath);
        }
    }
}
