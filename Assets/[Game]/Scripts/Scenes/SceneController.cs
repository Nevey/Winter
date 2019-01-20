using System;
using UnityEngine.SceneManagement;

namespace Game.Scenes
{
    public static class SceneController
    {
        private static bool setSceneActive;

        public static event Action<Scene> SceneLoadedEvent;

        private static void LoadScene(string sceneName, LoadSceneMode loadSceneMode, bool reloadScene)
        {
            if (!reloadScene)
            {
                // First check if the scene was already loaded
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);

                    if (scene.name != sceneName)
                    {
                        continue;
                    }

                    SceneLoadedEvent?.Invoke(scene);
                    return;
                }
            }

            SceneManager.sceneLoaded += OnSceneLoaded;

            SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (setSceneActive)
            {
                SceneManager.SetActiveScene(scene);

                setSceneActive = false;
            }

            SceneLoadedEvent?.Invoke(scene);
        }

        /// <summary>
        /// Load given scene and close all other open scenes during runtime
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="reloadScene"></param>
        public static void LoadSceneSingle(string sceneName, bool reloadScene = false)
        {
            LoadScene(sceneName, LoadSceneMode.Single, reloadScene);
        }

        /// <summary>
        /// Load given scene additively to other open scenes during runtime
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="setActive"></param>
        /// <param name="reloadScene"></param>
        public static void LoadSceneAdditive(string sceneName, bool setActive = false, bool reloadScene = false)
        {
            setSceneActive = setActive;

            LoadScene(sceneName, LoadSceneMode.Additive, reloadScene);
        }

        /// <summary>
        /// Close all open scenes during runtime
        /// </summary>
        public static void UnloadAllScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                SceneManager.UnloadSceneAsync(i);
            }
        }

        /// <summary>
        /// Close given scene during runtime
        /// </summary>
        /// <param name="sceneName"></param>
        public static void UnLoadScene(string sceneName)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }

        public static Scene[] GetActiveScenes()
        {
            Scene[] activeScenes = new Scene[SceneManager.sceneCount];

            for (int i = 0; i < activeScenes.Length; i++)
            {
                activeScenes[i] = SceneManager.GetSceneAt(i);
            }

            return activeScenes;
        }
    }
}