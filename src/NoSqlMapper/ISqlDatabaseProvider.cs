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
        Task DeleteDatabaseAsync([NotNull] NsDatabase database);

        [NotNull]
        Task EnsureTableAsync([NotNull] NsDatabase database, [NotNull] string tableName, ObjectIdType objectIdType = ObjectIdType.Guid);

        [NotNull]
        Task EnsureIndexAsync([NotNull] NsDatabase database, [NotNull] string tableName, [NotNull] string field, bool unique = false,
            bool ascending = true);

        [NotNull]
        Task DeleteIndexAsync([NotNull] NsDatabase database, [NotNull] string tableName, [NotNull] string field);

        [NotNull]
        Task DeleteTableAsync([NotNull] NsDatabase database, [NotNull] string tableName);
        #endregion

        #region Read

        [ItemNotNull]
        Task<IEnumerable<NsDocument>> QueryAsync([NotNull] NsDatabase database, [NotNull] string tableName, TypeReflector typeReflector, [NotNull] Query.Query query,
            SortDescription[] sorts = null, int skip = 0, int take = 0);

        [ItemCanBeNull]
        Task<NsDocument> QueryFirstAsync([NotNull] NsDatabase database, [NotNull] string tableName, TypeReflector typeReflector, 
            [NotNull] Query.Query query,
            SortDescription[] sorts = null);

        [ItemCanBeNull]
        Task<NsDocument> FindAsync([NotNull] NsDatabase database, [NotNull] string tableName, [NotNull] object id);

        [NotNull]
        Task<int> CountAsync([NotNull] NsDatabase database, [NotNull] string tableName, TypeReflector typeReflector, [NotNull] Query.Query query);
        #endregion

        #region Create & Update & Delete
        [NotNull]
        Task<object> InsertAsync([NotNull] NsDatabase database, [NotNull] string tableName, [NotNull] string json, object id = null);

        [NotNull]
        Task UpdateAsync([NotNull] NsDatabase database, [NotNull] string tableName, [NotNull] string json, [NotNull] object id);

        [NotNull]
        Task UpsertAsync([NotNull] NsDatabase database, [NotNull] string tableName, [NotNull] string json, [NotNull] object id);

        [NotNull]
        Task DeleteAsync([NotNull] NsDatabase database, [NotNull] string tableName, object id);
        #endregion

    }
}
