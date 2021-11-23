/// ---------------------------
/// Author: Szilveszter Dezsi
/// Created: 2019-12-22
/// Modified: n/a
/// ---------------------------

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Robothello.DAL
{
    /// <summary>
    /// Utility class containing generic methods for binary and xml serialization to and from file.
    /// </summary>
    public class RobothelloSerialization
    {
        /// <summary>
        /// BinarySerialize any type of object to file.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="obj">Object.</param>
        /// <param name="filePath">Path to file.</param>
        public static void BinarySerializeToFile<T>(T obj, string filePath)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                formatter.Serialize(stream, obj);
        }

        /// <summary>
        /// BinaryDeserialize any files serialized using BinarySerializeToFile&lt;T&gt;.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="filePath">Path to file.</param>
        /// <returns></returns>
        public static T BinaryDeserializeFromFile<T>(string filePath)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
                return (T)formatter.Deserialize(stream);
        }
    }
}
