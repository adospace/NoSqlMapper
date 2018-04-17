using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using NoSqlMapper.Query;

namespace NoSqlMapper.SqlServer
{
    public class SqlServerDatabaseProvider : ISqlDatabaseProvider
    {
        public string ConnectionString { get; }
        //public string InitialCatalog { get; }
        public string DataSource { get; }

        public SqlServerDatabaseProvider(NsConnection connection, string connectionString)
        {
            Validate.NotNull(connection, nameof(connection));
            Validate.NotNullOrEmptyOrWhiteSpace(connectionString, nameof(connectionString));

            _connection = connection;

            Validate.NotNullOrEmptyOrWhiteSpace(connectionString, nameof(connectionString));
            ConnectionString = connectionString;
            try
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
                //InitialCatalog = connectionStringBuilder.InitialCatalog;
                DataSource = connectionStringBuilder.DataSource;
            }
            catch (Exception ex)
            {
                throw  new InvalidOperationException("Connection string to Sql Server seems invalid", ex);
            }
        }

        public SqlServerDatabaseProvider(NsConnection connection, SqlConnection sqlConnection, bool ownConnection = false)
        {
            Validate.NotNull(connection, nameof(connection));
            Validate.NotNull(sqlConnection, nameof(sqlConnection));

            _connection = connection;
            _sqlConnection = sqlConnection;
            _disposeConnection = ownConnection;
            ConnectionString = sqlConnection.ConnectionString;
        }

        private void Log(string message)
        {
            _connection.Log?.Invoke(message);
        }

        #region Access to Sql Server

        private readonly NsConnection _connection;
        private SqlConnection _sqlConnection;
        private readonly bool _disposeConnection = true;

        public async Task EnsureConnectionAsync()
        {
            if (_sqlConnection == null)
                _sqlConnection = new SqlConnection(ConnectionString);

            if (_sqlConnection.State != ConnectionState.Open)
            {
                Log($"Opening connection to '{DataSource}'...");
                await _sqlConnection.OpenAsync();
                Log("Connection opened");
            }
        }

        public Task CloseConnectionAsync()
        {
            if (_sqlConnection != null && _sqlConnection.State != ConnectionState.Closed)
            {
                _sqlConnection?.Close();
                Log("Connection closed");
            }

            return Task.CompletedTask;
        }

        private async Task ExecuteNonQueryAsync(params string[] sqlLines)
        {
            await EnsureConnectionAsync();
            try
            {
                var sql = string.Join(Environment.NewLine, sqlLines);
                Log($"ExecuteNonQueryAsync(){Environment.NewLine}" +
                    $"{sql}{Environment.NewLine}");
                using (var cmd = new SqlCommand(sql, _sqlConnection))
                    await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log($"ExecuteNonQueryAsync() Exception{Environment.NewLine}{e}");
                throw;
            }
            finally
            {
                Log($"ExecuteNonQueryAsync() Completed");
            }
        }

        private async Task<object> ExecuteNonQueryAsync(string[] sqlLines, IDictionary<string, object> parameters,
            bool generateIndetity = false)
        {
            await EnsureConnectionAsync();
            try
            {
                var sql = string.Join(Environment.NewLine, sqlLines);
                Log($"ExecuteNonQueryAsync(){Environment.NewLine}" +
                    $"{sql}{Environment.NewLine}" +
                    $"{(string.Join(Environment.NewLine, parameters.Select(_ => string.Concat(_.Key, "=", _.Value))))}");

                using (var cmd = new SqlCommand(sql, _sqlConnection))
                {
                    foreach (var paramEntry in parameters)
                        cmd.Parameters.AddWithValue(paramEntry.Key, paramEntry.Value);

                    if (generateIndetity)
                        return await cmd.ExecuteScalarAsync();

                    await cmd.ExecuteNonQueryAsync();
                }

                return null;
            }
            catch (Exception e)
            {
                Log($"ExecuteNonQueryAsync() Exception{Environment.NewLine}{e}");
                throw;
            }
            finally
            {
                Log($"ExecuteNonQueryAsync() Completed");
            }
        }

        private async Task<IEnumerable<NsDocument>> ExecuteReaderAsync(string[] sqlLines, IDictionary<string, object> parameters)
        {
            await EnsureConnectionAsync();

            var documents = new List<NsDocument>();
            try
            {
                var sql = string.Join(Environment.NewLine, sqlLines);
                Log($"ExecuteReaderAsync(){Environment.NewLine}" +
                    $"{sql}{Environment.NewLine}" +
                    $"{(string.Join(Environment.NewLine, parameters.Select(_ => string.Concat(_.Key, "=", _.Value))))}");

                using (var cmd = new SqlCommand(sql, _sqlConnection))
                {
                    foreach (var paramEntry in parameters)
                        cmd.Parameters.AddWithValue(paramEntry.Key, paramEntry.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            documents.Add(new NsDocument(reader["_id"], (string) reader["_document"]));
                        }
                    }

                    return documents;
                }
            }
            catch (Exception e)
            {
                Log($"ExecuteReaderAsync() Exception{Environment.NewLine}{e}");
                throw;
            }
            finally
            {
                Log($"ExecuteReaderAsync() Completed");
            }
        }
        #endregion

        #region Provider Implementation

        public async Task CreateDatabaseIfNotExistsAsync(string databaseName)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(databaseName, nameof(databaseName));

            await ExecuteNonQueryAsync(
                $"IF (db_id(N'{databaseName}') IS NULL)",
                "BEGIN",
                $"    CREATE DATABASE [{databaseName}]",
                "END;");
        }

        public async Task DeleteDatabaseAsync(NsDatabase database)
        {
            Validate.NotNull(database, nameof(database));

            await ExecuteNonQueryAsync($"DROP DATABASE {database.Name}");
        }

        public async Task EnsureTableAsync(NsDatabase database, string tableName, ObjectIdType objectIdType = ObjectIdType.Guid)
        {
            Validate.NotNull(database, nameof(database));
            Validate.NotNullOrEmptyOrWhiteSpace(tableName, nameof(tableName));

            await ExecuteNonQueryAsync( 
                $"USE [{database.Name}]",
                $"IF NOT EXISTS (select * from sysobjects where name='{tableName}' and xtype='U')",
                $"BEGIN",
                $"CREATE TABLE [dbo].[{tableName}](",
                objectIdType == ObjectIdType.Guid ? "[_id] [uniqueidentifier] NOT NULL," : "[_id] [int] IDENTITY(1,1) NOT NULL",
                $"[_document] [nvarchar](max) NOT NULL,",
                $"CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED ",
                $"(",
                $"[_id] ASC",
                $") WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]",
                $") ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]",
                objectIdType == ObjectIdType.Guid ? $"ALTER TABLE [dbo].[{tableName}] ADD  CONSTRAINT [DF_{tableName}__id]  DEFAULT (newid()) FOR [_id]" : string.Empty,
                $"END");
        }

        public async Task EnsureIndexAsync(NsDatabase database, string tableName, string field, bool unique = false, bool ascending = true)
        {
            Validate.NotNull(database, nameof(database));
            Validate.NotNullOrEmptyOrWhiteSpace(tableName, nameof(tableName));
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));

            var columnName = field.Replace(".", "_");
            await ExecuteNonQueryAsync( 
                $"USE [{database.Name}]",
                $"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N\'[dbo].[{tableName}]\') AND name = \'{columnName}\')",
                $"BEGIN",
                $"ALTER TABLE [dbo].[Posts]",
                $"ADD [{columnName}] AS (json_value([__document],\'$.{field}\'))",
                $"END",
                $"IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = \'IDX_{columnName}\' AND object_id = OBJECT_ID(\'{tableName}\'))",
                $"BEGIN"+
                $"CREATE {(unique ? "UNIQUE" : string.Empty)} NONCLUSTERED INDEX [IDX_{columnName}] ON [dbo].[{tableName}]",
                $"( [{columnName}] {(ascending ? "ASC" : "DESC")} )",
                $"WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]",
                $"END");
        }

        public async Task DeleteIndexAsync(NsDatabase database, string tableName, string field)
        {
            Validate.NotNull(database, nameof(database));
            var columnName = field.Replace(".", "_");
            await ExecuteNonQueryAsync( 
                $"USE [{database.Name}]",
                $"DROP INDEX [IDX_{columnName}] ON [dbo].[{tableName}]");
        }

        public async Task DeleteTableAsync(NsDatabase database, string tableName)
        {
            Validate.NotNull(database, nameof(database));
            await ExecuteNonQueryAsync( 
                $"USE [{database.Name}]",
                $"IF EXISTS (select * from sysobjects where name='{tableName}' and xtype='U')",
                $"BEGIN",
                $"DROP TABLE [dbo].[{tableName}]",
                $"END");
        }

        public async Task<IEnumerable<NsDocument>> QueryAsync(NsDatabase database, string tableName, TypeReflector typeReflector, Query.Query query, SortDescription[] sorts = null, int skip = 0, int take = 0)
        {
            Validate.NotNull(database, nameof(database));
            Validate.NotNullOrEmptyOrWhiteSpace(tableName, nameof(tableName));
            Validate.NotNull(query, nameof(query));

            var sql = new List<string>();
            var parameters = new List<KeyValuePair<int, object>>();

            sql .Append($"USE [{database.Name}]")
                .Append(query.ConvertToSql(typeReflector, tableName, parameters));
                
            if (skip > 0)
                sql.Append($"OFFSET {skip} ROWS");

            if (take < int.MaxValue)
                sql.Append($"FETCH NEXT {take} ROWS ONLY");

            return await ExecuteReaderAsync(sql.ToArray(), parameters.ToDictionary(_ => $"@{_.Key}", _ => _.Value));
        }

        public Task<NsDocument> QueryFirstAsync(NsDatabase database, string tableName, TypeReflector typeReflector, Query.Query query, SortDescription[] sorts = null)
        {
            throw new NotImplementedException();
        }

        public async Task<NsDocument> FindAsync(NsDatabase database, string tableName, object id)
        {
            Validate.NotNull(database, nameof(database));
            Validate.NotNullOrEmptyOrWhiteSpace(tableName, nameof(tableName));

            var sql = new List<string>();

            sql.Append($"USE [{database.Name}]")
                .Append($"SELECT _id, _document FROM [dbo].[{tableName}]")
                .Append($"WHERE (_id = @1)");

            return (await ExecuteReaderAsync(sql.ToArray(), new Dictionary<string, object>() {{"@1", id}}))
                .FirstOrDefault();
        }

        public Task<int> CountAsync(NsDatabase database, string tableName, TypeReflector typeReflector, Query.Query query)
        {
            throw new NotImplementedException();
        }

        public async Task<object> InsertAsync(NsDatabase database, string tableName, string json, object id)
        {
            Validate.NotNull(database, nameof(database));
            Validate.NotNullOrEmptyOrWhiteSpace(tableName, nameof(tableName));
            Validate.NotNull(json, nameof(json));

            if (id != null)
            {
                await ExecuteNonQueryAsync(new[]
                           {
                               $"USE [{database.Name}]",
                               $"INSERT INTO [dbo].[{tableName}]",
                               $"           ([_id]",
                               $"           ,[_document])",
                               $"     VALUES",
                               $"           (@id",
                               $"           ,@document)",
                               $"SELECT SCOPE_IDENTITY();"
                           },
                           new Dictionary<string, object>()
                           {
                               {"@id", id},
                               {"@document", json}
                           },
                           generateIndetity: false);

                return id;
            }

            return await ExecuteNonQueryAsync(new[]
                       {
                           $"USE [{database.Name}]",
                           $"INSERT INTO [dbo].[{tableName}]",
                           $"           ([_document])" +
                           $"output INSERTED._id",
                           $"     VALUES",
                           $"           (@document)"
                       },
                       new Dictionary<string, object>()
                       {
                           {"@document", json}
                       },
                       generateIndetity: true);
        }

        public Task UpdateAsync(NsDatabase database, string tableName, string json, object id)
        {
            Validate.NotNull(database, nameof(database));
            Validate.NotNullOrEmptyOrWhiteSpace(tableName, nameof(tableName));
            Validate.NotNull(json, nameof(json));
            Validate.NotNull(id, nameof(id));

            throw new NotImplementedException();
        }

        public Task UpsertAsync(NsDatabase database, string tableName, string json, object id)
        {
            Validate.NotNull(database, nameof(database));
            Validate.NotNull(json, nameof(json));
            Validate.NotNull(id, nameof(id));

            throw new NotImplementedException();
        }

        public Task DeleteAsync(NsDatabase database, string tableName, object id)
        {
            throw new NotImplementedException();
        }

        

        #endregion


        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_disposeConnection)
                        _sqlConnection?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NsConnection() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
