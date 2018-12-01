using System;

namespace Game.Network.Data
{
    [Serializable]
    public struct PositionData
    {
        public int clientID;

        // TODO: Create serializable vector (check out System.Numerics.Vector3)
        public float x;
        public float y;
        public float z;
    }
}
