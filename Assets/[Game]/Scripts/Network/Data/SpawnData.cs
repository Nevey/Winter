using System;
using UnityEngine;

namespace Game.Network.Data
{
    [Serializable]
    public struct SpawnData
    {
        // Private
        private readonly int ownerID;

        private readonly float x;
        private readonly float y;
        private readonly float z;

        // TODO: Add actor type

        // Public
        public int OwnerID => ownerID;

        // TODO: Fix performance
        public Vector3 Position => new Vector3(x, y, z);

        public SpawnData(int ownerID, float x, float y, float z)
        {
            this.ownerID = ownerID;
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
