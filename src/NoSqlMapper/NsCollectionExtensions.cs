using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NoSqlMapper.Query;

namespace NoSqlMapper
{
    public static class NsCollectionExtensions
    {
        public static async Task<T[]> FindAllAsync<T>(this NsCollection<T> collection, params SortDescription[] sorts)
            where T : class
        {
            Validate.NotNull(collection, nameof(collection));
            return await collection.FindAsync(sorts: sorts ?? SortDescription.OrderById());
        }

        public static async Task<T> FindFirstOrDefaultAsync<T>(this NsCollection<T> collection, params SortDescription[] sorts)
            where T : class
        {
            Validate.NotNull(collection, nameof(collection));
            return (await collection.FindAsync(sorts: sorts ?? SortDescription.OrderById(), skip: 0, take: 1)).FirstOrDefault();
        }

        public static async Task<T> FindFirstOrDefaultAsync<T>(this NsCollection<T> collection, Query.Query query, params SortDescription[] sorts)
            where T : class
        {
            Validate.NotNull(collection, nameof(collection));
            Validate.NotNull(query, nameof(query));
            return (await collection.FindAsync(query:query, sorts: sorts ?? SortDescription.OrderById(), skip: 0, take: 1)).FirstOrDefault();
        }

        public static async Task<T[]> FindAsync<T>(this NsCollection<T> collection, Expression<Func<T, bool>> query, params SortDescription[] sorts) where T: class 
        {
            Validate.NotNull(collection, nameof(collection));
            Validate.NotNull(query, nameof(query));

            return await collection.FindAsync(QueryBuilder<T>.Build(query), sorts);
        }

        public static async Task<int> CountAsync<T>(this NsCollection<T> collection, Expression<Func<T, bool>> query) where T : class
        {
            Validate.NotNull(collection, nameof(collection));
            Validate.NotNull(query, nameof(query));

            return await collection.CountAsync(QueryBuilder<T>.Build(query));
        }

        public static async Task<T> FindFirstOrDefaultAsync<T>(this NsCollection<T> collection, Expression<Func<T, bool>> query, params SortDescription[] sorts)
            where T : class
        {
            Validate.NotNull(collection, nameof(collection));
            Validate.NotNull(query, nameof(query));
            return (await collection.FindAsync(query: QueryBuilder<T>.Build(query), sorts: sorts ?? SortDescription.OrderById(), skip: 0, take: 1)).FirstOrDefault();
        }

        public static async Task EnsureIndexAsync<T>(this NsCollection<T> collection,
            Expression<Func<T, object>> query, bool unique = false, bool ascending = true)
            where T : class
        {
            await collection.EnsureIndexAsync(QueryBuilder<T>.BuildPath(query), unique, ascending);
        }
    }
}
