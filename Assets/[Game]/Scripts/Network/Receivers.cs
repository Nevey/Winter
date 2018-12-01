namespace Game.Network
{
    /// <summary>
    /// Used to select which clients will receive a message from the server
    /// </summary>
    public enum Receivers
    {
        /// <summary>
        /// Every client will receive the message
        /// </summary>
        All,

        /// <summary>
        /// Every client but the sender will receive the message
        /// </summary>
        Others,

        /// <summary>
        /// Only sender client will receive the message
        /// </summary>
        Self
    }
}
