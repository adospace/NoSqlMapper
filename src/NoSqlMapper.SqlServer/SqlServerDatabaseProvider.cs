using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NoSqlMapper.Query;

namespace NoSqlMapper.SqlServer
{
    public class SqlServerDatabaseProvider : ISqlDatabaseProvider
    {
        public string ConnectionString { get; }
        public string InitialCatalog { get; }
        public string DataSource { get; }

        public SqlServerDatabaseProvider(string connectionString)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(connectionString, nameof(connectionString));
            ConnectionString = connectionString;

            try
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
                InitialCatalog = connectionStringBuilder.InitialCatalog;
                DataSource = connectionStringBuilder.DataSource;
            }
            catch (Exception ex)
            {
                throw  new InvalidOperationException("Connection string to Sql Server seems invalid", ex);
            }
        }

        private SqlConnection _connection;
        public async Task EnsureConnectionAsync()
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(ConnectionString);
                await _connection.OpenAsync();
            }
        }

        public Task CloseConnectionAsync()
        {
            _connection?.Close();
            return Task.CompletedTask;
        }

        private async Task ExecuteNonQueryAsync(params string[] sqlLines)
        {
            await EnsureConnectionAsync();

            using (var cmd = new SqlCommand(string.Join(Environment.NewLine, sqlLines), _connection))
                await cmd.ExecuteNonQueryAsync();
        }

        private async Task<object> ExecuteNonQueryAsync(string[] sqlLines, IDictionary<string, object> parameters, bool generateIndetity = false)
        {
            await EnsureConnectionAsync();

            using (var cmd = new SqlCommand(string.Join(Environment.NewLine, sqlLines), _connection))
            {
                foreach (var paramEntry in parameters)
                    cmd.Parameters.AddWithValue(paramEntry.Key, paramEntry.Value);

                if (generateIndetity)
                    return await cmd.ExecuteScalarAsync();

                await cmd.ExecuteNonQueryAsync();
                return null;
            }
        }

        public async Task CreateDatabaseIfNotExistsAsync(string databaseName)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(databaseName, nameof(databaseName));

            await ExecuteNonQueryAsync($"IF (db_id(N'{databaseName}') IS NULL)",
                                       "BEGIN",
                                       $"    CREATE DATABASE [{databaseName}]",
                                       "END;");
        }

        public async Task DeleteDatabaseAsync(string databaseName)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(databaseName, nameof(databaseName));

            await ExecuteNonQueryAsync($"DROP DATABASE {databaseName}");
        }

        public async Task EnsureTableAsync(string databaseName, string tableName, ObjectIdType objectIdType = ObjectIdType.Guid)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(databaseName, nameof(databaseName));
            Validate.NotNullOrEmptyOrWhiteSpace(tableName, nameof(tableName));

            await ExecuteNonQueryAsync($"USE [{databaseName}]",
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

        public async Task EnsureIndexAsync(string databaseName, string tableName, string field, bool unique = false, bool ascending = true)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(databaseName, nameof(databaseName));
            Validate.NotNullOrEmptyOrWhiteSpace(tableName, nameof(tableName));
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));

            var columnName = field.Replace(".", "_");
            await ExecuteNonQueryAsync($"USE [{databaseName}]",
                                       $"GO",
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

        public async Task DeleteIndexAsync(string databaseName, string tableName, string field)
        {
            var columnName = field.Replace(".", "_");
            await ExecuteNonQueryAsync($"USE [{databaseName}]",
                                       $"GO",
                                       $"DROP INDEX [IDX_{columnName}] ON [dbo].[{tableName}]",
                                       $"GO");
        }

        public Task DeleteTableAsync(string databaseName, string tableName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> QueryAsync<T>(string databaseName, string tableName, Query.Query query, SortDescription[] sorts = null, int skip = 0, int take = 0)
        {
            throw new NotImplementedException();
        }

        public Task<T> QueryFirstAsync<T>(string databaseName, string tableName, Query.Query query, SortDescription[] sorts = null)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(string databaseName, string tableName, Query.Query query)
        {
            throw new NotImplementedException();
        }

        public async Task<object> InsertAsync(string databaseName, string tableName, string json, object id)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(databaseName, nameof(databaseName));
            Validate.NotNullOrEmptyOrWhiteSpace(tableName, nameof(tableName));
            Validate.NotNull(json, nameof(json));

            if (id != null)
            {
                await ExecuteNonQueryAsync(new[]
                           {
                               $"USE [{databaseName}]",
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
                           $"USE [{databaseName}]",
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

        public Task UpdateAsync(string databaseName, string tableName, string json, object id)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(databaseName, nameof(databaseName));
            Validate.NotNullOrEmptyOrWhiteSpace(tableName, nameof(tableName));
            Validate.NotNull(json, nameof(json));
            Validate.NotNull(id, nameof(id));

            throw new NotImplementedException();
        }

        public Task UpsertAsync(string databaseName, string tableName, string json, object id)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(databaseName, nameof(databaseName));
            Validate.NotNullOrEmptyOrWhiteSpace(tableName, nameof(tableName));
            Validate.NotNull(json, nameof(json));
            Validate.NotNull(id, nameof(id));

            throw new NotImplementedException();
        }

        public Task DeleteAsync(string databaseName, string tableName, object id)
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _connection?.Dispose();
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
