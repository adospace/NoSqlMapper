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
            Validate.NotNull(connection, nameof(connection));
            Validate.NotNullOrEmptyOrWhiteSpace(connectionString, nameof(connectionString));

            connection.SqlDatabaseProvider = new SqlServerDatabaseProvider(connection, connectionString);
            return connection;
        }

        public static NsConnection UseSqlServer(this NsConnection connection, SqlConnection sqlConnection, bool ownConnection = false)
        {
            Validate.NotNull(connection, nameof(connection));
            Validate.NotNull(sqlConnection, nameof(sqlConnection));

            connection.SqlDatabaseProvider = new SqlServerDatabaseProvider(connection, sqlConnection, ownConnection);
            return connection;
        }

    }
}
