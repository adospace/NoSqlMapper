using Npgsql;

namespace NoSqlMapper.PostgreSQL
{
    public static class NsConnectionExtensions
    {
        public static NsConnection UsePostgreSQL(this NsConnection connection, string connectionString)
        {
            Validate.NotNull(connection, nameof(connection));
            Validate.NotNullOrEmptyOrWhiteSpace(connectionString, nameof(connectionString));

            connection.SqlDatabaseProvider = new NpgSqlDatabaseProvider(connection, connectionString);
            return connection;
        }

        public static NsConnection UsePostgreSQL(this NsConnection connection, NpgsqlConnection sqlConnection, bool ownConnection = false)
        {
            Validate.NotNull(connection, nameof(connection));
            Validate.NotNull(sqlConnection, nameof(sqlConnection));

            connection.SqlDatabaseProvider = new NpgSqlDatabaseProvider(connection, sqlConnection, ownConnection);
            return connection;
        }

    }
}
