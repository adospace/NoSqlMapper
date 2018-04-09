using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace NoSqlMapper.SqlServer
{
    public static class NsConnectionExtensions
    {
        public static NsConnection UseSqlServer(this NsConnection connection, string connectionString)
        {
            connection.SqlDatabaseProvider = new SqlServerDatabaseProvider(connectionString);
            return connection;
        }

        public static NsConnection UseSqlServer(this NsConnection connection, SqlConnection sqlConnection, bool ownConnection = false)
        {
            connection.SqlDatabaseProvider = new SqlServerDatabaseProvider(sqlConnection, ownConnection);
            return connection;
        }

    }
}
