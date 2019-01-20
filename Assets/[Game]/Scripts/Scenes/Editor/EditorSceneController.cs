using Game.Utilities.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Game.Scenes.Editor
{
    public static class EditorSceneController
    {
        /// <summary>
        /// Open a scene in editor mode
        /// </summary>
        /// <param name="sceneAsset"></param>
        /// <param name="openSceneMode"></param>
        /// <param name="setSceneActive"></param>
        /// <returns></returns>
        public static Scene OpenSceneInEditor(SceneAsset sceneAsset, OpenSceneMode openSceneMode = OpenSceneMode.Single, 
            bool setSceneActive = false)
        {
            string scenePath = AssetUtility.GetAssetPath(sceneAsset);   
            Scene scene = EditorSceneManager.OpenScene(scenePath, openSceneMode);

            if (setSceneActive)
            {
                // ReSharper disable once AccessToStaticMemberViaDerivedType
                EditorSceneManager.SetActiveScene(scene);
            }

            return scene;
        }
        
        /// <summary>
        /// Close a scene in editor mode
        /// </summary>
        /// <param name="name"></param>
        /// <param name="removeScene"></param>
        /// <returns></returns>
        public static bool CloseSceneInEditor(string name, bool removeScene = true)
        {
            // ReSharper disable once AccessToStaticMemberViaDerivedType
            Scene scene = EditorSceneManager.GetSceneByName(name);

            return EditorSceneManager.CloseScene(scene, removeScene);
        }

        /// <summary>
        /// Close all scenes in editor mode
        /// </summary>
        public static void CloseAllScenesInEditor()
        {
            Scene[] scenes = GetAllScenesInEditor();
            
            for (int i = 0; i < scenes.Length; i++)
            {
                EditorSceneManager.CloseScene(scenes[i], true);
            }
        }

        public static Scene[] GetAllScenesInEditor()
        {
            // ReSharper disable once AccessToStaticMemberViaDerivedType
            Scene[] scenes = new Scene[EditorSceneManager.sceneCount];
            
            // ReSharper disable once AccessToStaticMemberViaDerivedType
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                // ReSharper disable once AccessToStaticMemberViaDerivedType
                scenes[i] = EditorSceneManager.GetSceneAt(i);
            }

            return scenes;
        }
    }
}