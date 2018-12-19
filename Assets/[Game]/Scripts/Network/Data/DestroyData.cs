using System;

namespace Game.Network.Data
{
    [Serializable]
    public class DestroyData : NetworkData
    {
        // TODO: Track actor ID

        public DestroyData(int ownerID) : base(ownerID)
        {

        }
    }
}