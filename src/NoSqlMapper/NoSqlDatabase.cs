using System;
using System.Threading.Tasks;

namespace NoSqlMapper
{
    public class NoSqlDatabase
    {
        public string Name { get; }
        private readonly NoSqlConnection _connection;
        private Design.Database _databaseDesign = null;

        internal NoSqlDatabase(NoSqlConnection connection, string name)
        {
            Name = name;
            _connection = connection;
        }

        private async Task EnsureDatabase()
        {
            if (_databaseDesign == null)
            {
                var serializedDatabaseDesign = await _connection.SqlDatabaseProvider.EnsureDatabaseAsync(Name);

                if (serializedDatabaseDesign == null)
                {
                    serializedDatabaseDesign = _connection.JsonProvider.Serialize(_databaseDesign = new Design.Database());
                    await _connection.SqlDatabaseProvider.EnsureDatabaseAsync(Name, serializedDatabaseDesign);
                }
                else
                    _databaseDesign = _connection.JsonProvider.Deserialize<Design.Database>(serializedDatabaseDesign);
            }
        }

        public NoSqlCollection<T> GetCollection<T>(string collectionName) where T : class 
        {
            Validate.NotNullOrEmptyOrWhiteSpace(collectionName, nameof(collectionName));

            return new NoSqlCollection<T>(this, collectionName);
        }

    }
}
