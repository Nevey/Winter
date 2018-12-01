using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Game.Utilities
{
    public static class ByteArrayUtility
    {
        public static byte[] ObjectToByteArray(object obj)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }

        public static T ByteArrayToObject<T>(byte[] byteArray)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                memoryStream.Write(byteArray, 0, byteArray.Length);

                memoryStream.Seek(0, SeekOrigin.Begin);

                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}
