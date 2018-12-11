using System;
using UnityEngine;

namespace Game.Network.Data
{
    [Serializable]
    public class RotationNetworkData : NetworkData
    {
        private readonly float x;
        private readonly float y;
        private readonly float z;
        private readonly float w;

        public Quaternion Rotation => new Quaternion(x, y, z, w);

        public RotationNetworkData(int ownerID, Quaternion rotation) : base(ownerID)
        {
            x = rotation.x;
            y = rotation.y;
            z = rotation.z;
            w = rotation.w;
        }
    }
}
