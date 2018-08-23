using Newtonsoft.Json;
using System.IO;

namespace GlobalPhone
{

    /// <summary>
    /// Using JavaScriptSerializer to deserialize the internal data.
    /// </summary>
    public class DefaultDeserializer
        : IDeserializer
    {

        public object[] Deserialize(string text)
        {
            return JsonConvert.DeserializeObject<object[]>(text);
        }

        public T Deserialize<T>(Stream stream)
        {
            var deserializer = new JsonSerializer();
            using (TextReader tr = new StreamReader(stream))
            {
                using (var jtr = new JsonTextReader(tr))
                {
                    return deserializer.Deserialize<T>(jtr);
                }
            }
        }

    }

}