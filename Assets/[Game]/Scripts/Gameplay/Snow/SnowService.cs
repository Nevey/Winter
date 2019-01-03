using System.Collections.Generic;
using Game.Services;
using UnityEngine;

namespace Game.Gameplay.Snow
{
    public class SnowService : Service<SnowService>
    {
        private List<SnowDrawer> snowDrawers = new List<SnowDrawer>();

        public void RegisterSnowDrawer(SnowDrawer snowDrawer)
        {
            snowDrawers.Add(snowDrawer);
        }

        public void UnregisterSnowDrawer(SnowDrawer snowDrawer)
        {
            snowDrawers.Remove(snowDrawer);
        }

        public void DrawStamp(Transform transform, float stampSize, float stampStrength)
        {
            for (int i = 0; i < snowDrawers.Count; i++)
            {
                snowDrawers[i].DrawStamp(transform, stampSize, stampStrength);
            }
        }
    }
}