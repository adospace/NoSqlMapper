using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NoSqlMapper.Query;

namespace NoSqlMapper
{
    public interface ISqlDatabaseProvider : IDisposable
    {
        #region Connection
        [NotNull]
        Task EnsureConnectionAsync();

        [NotNull]
        Task CloseConnectionAsync();
        #endregion

        #region DDL
        [NotNull]
        Task CreateDatabaseIfNotExistsAsync([NotNull] string databaseName);

        [NotNull]
        Task DeleteDatabaseAsync([NotNull] string databaseName);

        [NotNull]
        Task EnsureTableAsync([NotNull] string databaseName, [NotNull] string tableName, ObjectIdType objectIdType = ObjectIdType.Guid);

        [NotNull]
        Task EnsureIndexAsync([NotNull] string databaseName, [NotNull] string tableName, [NotNull] string field, bool unique = false,
            bool ascending = true);

        [NotNull]
        Task DeleteIndexAsync([NotNull] string databaseName, [NotNull] string tableName, [NotNull] string field);

        [NotNull]
        Task DeleteTableAsync([NotNull] string databaseName, [NotNull] string tableName);
        #endregion

        #region Read

        [ItemNotNull]
        Task<IEnumerable<T>> QueryAsync<T>([NotNull] string databaseName, [NotNull] string tableName, [NotNull] Query.Query query,
            SortDescription[] sorts = null, int skip = 0, int take = 0);

        [ItemCanBeNull]
        Task<T> QueryFirstAsync<T>([NotNull] string databaseName, [NotNull] string tableName, 
            [NotNull] Query.Query query,
            SortDescription[] sorts = null);

        [NotNull]
        Task<int> CountAsync([NotNull] string databaseName, [NotNull] string tableName, [NotNull] Query.Query query);
        #endregion

        #region Create & Update & Delete
        [NotNull]
        Task<object> InsertAsync([NotNull] string databaseName, [NotNull] string tableName, [NotNull] string json, object id = null);

        [NotNull]
        Task UpdateAsync([NotNull] string databaseName, [NotNull] string tableName, [NotNull] string json, [NotNull] object id);

        [NotNull]
        Task UpsertAsync([NotNull] string databaseName, [NotNull] string tableName, [NotNull] string json, [NotNull] object id);

        [NotNull]
        Task DeleteAsync([NotNull] string databaseName, [NotNull] string tableName, object id);
        #endregion

    }
}
