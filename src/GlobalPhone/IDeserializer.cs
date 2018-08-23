using System.IO;

namespace GlobalPhone
{

    public interface IDeserializer
    {

        object[] Deserialize(string text);

        T Deserialize<T>(Stream stream);

    }

}