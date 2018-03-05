using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NoSqlMapper
{
    public class NoSqlConnection
    {

        public NoSqlConnection()
        {
        }

        private ISqlDatabaseProvider _sqlDatabaseProvider;
        public ISqlDatabaseProvider SqlDatabaseProvider
        {
            get => _sqlDatabaseProvider ?? throw new InvalidOperationException("SqlDatabase provider not specified");
            set
            {
                if (_sqlDatabaseProvider != null)
                    throw new InvalidOperationException("SqlDatabase provider already set");

                _sqlDatabaseProvider = value;
            }
        }

        private IJsonSerializer _jsonProvider;
        public IJsonSerializer JsonProvider
        {
            get => _jsonProvider ?? throw new InvalidOperationException("Json serializer not specified");
            set
            {
                if (_jsonProvider != null)
                    throw new InvalidOperationException("Json serializer already set");

                _jsonProvider = value;
            }
        }

        public ILoggerProvider LoggerProvider { get; set; }

        

        public async Task<NoSqlDatabase> GetDatabaseAsync(string databaseName)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(databaseName, nameof(databaseName));

            return new NoSqlDatabase(this, databaseName);
        }

    }
}
