using System;
using UnityEngine;

namespace Game.Network.Data
{
    [Serializable]
    public class SpawnData : NetworkComponentData
    {
        public readonly float x;
        public readonly float y;
        public readonly float z;

        // TODO: Add actor type reference

        public SpawnData(int ownerID, Vector3 position) : base(ownerID)
        {
            x = position.x;
            y = position.y;
            z = position.z;
        }
    }
}
