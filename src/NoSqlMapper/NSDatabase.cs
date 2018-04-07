using System;
using System.Threading.Tasks;

namespace NoSqlMapper
{
    public class NsDatabase
    {
        public NsConnection Connection { get; }
        public string Name { get; }

        internal NsDatabase(NsConnection connection, string name)
        {
            Connection = connection;
            Name = name;
        }

        public async Task<NsCollection<T>> GetCollectionAsync<T>(string collectionName) where T : class 
        {
            Validate.NotNullOrEmptyOrWhiteSpace(collectionName, nameof(collectionName));

            var collection = new NsCollection<T>(this, collectionName);

            await collection.EnsureTableAsync();

            return collection;
        }

        public void DeleteCollectionAsync(string collectionName)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(collectionName, nameof(collectionName));

            Connection.SqlDatabaseProvider.DeleteTableAsync(Name, collectionName);
        }

    }
}
