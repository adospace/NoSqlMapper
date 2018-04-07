using System;
using System.Collections.Generic;
using System.Text;

namespace NoSqlMapper.SqlServer
{
    public static class NsConnectionExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static NsConnection UseSqlServer(this NsConnection connection, string connectionString)
        {
            connection.SqlDatabaseProvider = new SqlServerDatabaseProvider(connectionString);
            return connection;
        }
    }
}
