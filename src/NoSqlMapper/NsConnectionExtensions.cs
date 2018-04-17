using System;
using System.Collections.Generic;
using System.Text;

namespace NoSqlMapper
{
    public static class NsConnectionExtensions
    {
        public static NsConnection LogTo(this NsConnection connection, Action<string> logger)
        {
            Validate.NotNull(connection, nameof(connection));
            Validate.NotNull(logger, nameof(logger));

            connection.Log = logger;
            return connection;
        }
    }
}
