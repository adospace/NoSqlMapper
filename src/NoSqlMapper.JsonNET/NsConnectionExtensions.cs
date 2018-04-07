using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NoSqlMapper.JsonNET
{
    public static class NsConnectionExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static NsConnection UseJsonNET(this NsConnection connection)
        {
            connection.JsonSerializer = new JsonSerializer();
            return connection;
        }

        // ReSharper disable once InconsistentNaming
        public static NsConnection UseJsonNET(this NsConnection connection, JsonSerializerSettings settings)
        {
            connection.JsonSerializer = new JsonSerializer(settings);
            return connection;
        }
    }
}
