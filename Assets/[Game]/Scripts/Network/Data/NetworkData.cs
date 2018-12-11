using System;

namespace Game.Network.Data
{
    [Serializable]
    public class NetworkData
    {
        /// <summary>
        /// The ID of the Actor owning the component sending this data
        /// </summary>
        public readonly int ownerID;

        public NetworkData(int ownerID)
        {
            this.ownerID = ownerID;
        }
    }
}
