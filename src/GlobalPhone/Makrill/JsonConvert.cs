using Newtonsoft.Json;

namespace Makrill
{

    /// <summary>
    /// Makrill wrapper for Newtonsoft.Json.JsonConvert
    /// </summary>
    internal class JsonConvert
    {

        /// <summary>
        /// Using array and dictionary converter to deserialize into arrays and dictionaries
        /// </summary>
        public T Deserialize<T>(string val)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ArrayAndDictionaryConverter());
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(val, settings);
        }

        /// <summary>
        /// Serializes the specified object o a JSON string
        /// </summary>
        public string Serialize<T>(T obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

    }

}