using System;
using UnityEngine;

namespace Game.Network.Data
{
    [Serializable]
    public class PositionNetworkData : NetworkData
    {
        // TODO: Create serializable vector (check out System.Numerics.Vector3)
        public readonly float x;
        public readonly float y;
        public readonly float z;

        public PositionNetworkData(int ownerID, Vector3 position) : base(ownerID)
        {
            x = position.x;
            y = position.y;
            z = position.z;
        }
    }
}
