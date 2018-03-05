using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NoSqlMapper
{
    public interface IJsonSerializer
    {
        string Serialize<T>(T objectToSerialize);

        T Deserialize<T>(string serializedObject);
    }
}
