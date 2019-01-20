using UnityEngine;

namespace Game.Scenes
{
    public class RequiredScenes : MonoBehaviour
    {

//#if !UNITY_EDITOR
        private void Awake()
        {
            SceneController.LoadSceneAdditive("ConsoleScene");
            SceneController.LoadSceneAdditive("SurfacePainterPlayground");
        }
//#endif
    }
}