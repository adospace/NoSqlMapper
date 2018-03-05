using System;

namespace NoSqlMapper
{
    public class NoSqlDatabase
    {
        public string Name { get; }
        private readonly NoSqlConnection _connection;

        internal NoSqlDatabase(NoSqlConnection connection, string name)
        {
            Name = name;
            _connection = connection;
        }

        public NoSqlCollection<T> GetCollection<T>(string collectionName) where T : class 
        {
            Validate.NotNullOrEmptyOrWhiteSpace(collectionName, nameof(collectionName));

            return new NoSqlCollection<T>(this, collectionName);
        }

    }
}
