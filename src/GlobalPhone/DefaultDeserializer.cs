using Newtonsoft.Json;
using System.IO;
using Makrill;
using JsonConvert = Newtonsoft.Json.JsonConvert;

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
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ArrayAndDictionaryConverter());
            return JsonConvert.DeserializeObject<object[]>(text, settings);
        }

        public T Deserialize<T>(Stream stream)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ArrayAndDictionaryConverter());
            var deserializer = JsonSerializer.Create(settings);
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