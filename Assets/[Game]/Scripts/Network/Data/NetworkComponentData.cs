using System;

namespace Game.Network.Data
{
    [Serializable]
    public class NetworkComponentData
    {
        /// <summary>
        /// The ID of the Actor owning the component sending this data
        /// </summary>
        public readonly int ownerID;

        public NetworkComponentData(int ownerID)
        {
            this.ownerID = ownerID;
        }
    }
}
