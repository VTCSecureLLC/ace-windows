using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;

namespace com.vtcsecure.ace.windows.Utilities
{
    public static class JsonDeserializer
    {
        public static T JsonDeserialize<T>(string jsonString)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(jsonString);
            MemoryStream stream = new MemoryStream(byteArray);
            try
            {

                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                T item = (T)serializer.ReadObject(stream);

                return item;
            }
            catch (Exception ex)
            {
                string message = "Error during deserialization: JsonString: " + jsonString + " Error Details: " + ex.Message;
                throw new Exception(message);
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }
        }

        public static string Serialize<T>(this T obj)
        {
            MemoryStream stream = new MemoryStream();

            try
            {
                //serialize data to a stream, then to a JSON string
                DataContractJsonSerializer jsSerializer = new DataContractJsonSerializer(typeof(T));
                jsSerializer.WriteObject(stream, obj);

                return Encoding.UTF8.GetString(stream.ToArray());
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }
        }
    }
}
