#pragma warning disable 0649

using UnityEngine;

namespace DS.Example
{
    public class RollTheBallManager : MonoBehaviour
    {
        #region Refs
        [SerializeField] private DeformableSurface _surface;
        #endregion

        #region Private
        private RenderTexture _savedBaseMapRt;
        #endregion


        void Update()
        {
            if (_surface != null)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                    OnRestoreClick();
                if (Input.GetKeyDown(KeyCode.F2))
                    OnSaveClick();
                if (Input.GetKeyDown(KeyCode.F3))
                    OnLoadClick();
            }
        }

        void OnRestoreClick()
        {
            //Restore to initial state
            _surface.Restore();
        }

        void OnSaveClick()
        {
            //Create new render texture
            _savedBaseMapRt = new RenderTexture(_surface.BaseMapRt.width, _surface.BaseMapRt.height, 0, _surface.BaseMapRt.format);
            _savedBaseMapRt.Create();

            //Blit from BaseMapRt
            Graphics.Blit(_surface.BaseMapRt, _savedBaseMapRt);

            //Now we can convert _savedBaseMapRt to Texture2D and save it as PNG file on disk. 
            //When the game starts again load this PNG file as Texture2D and blit it back to _surface.BaseMapRt 
        }

        void OnLoadClick()
        {
            if (_savedBaseMapRt == null)
                return;

            //Blit from _savedBaseMapRt or loaded Texture2D from disk
            Graphics.Blit(_savedBaseMapRt, _surface.BaseMapRt);
        }
    }
}

