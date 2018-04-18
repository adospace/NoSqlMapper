using System;
using System.Collections.Generic;
using System.Linq;
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
            return await collection.FindAsync(sorts: sorts ?? SortDescription.OrderById());
        }

        public static async Task<T> FindFirstOrDefaultAsync<T>(this NsCollection<T> collection, params SortDescription[] sorts)
            where T : class
        {
            return (await collection.FindAsync(sorts: sorts ?? SortDescription.OrderById(), skip: 0, take: 1)).FirstOrDefault();
        }

        public static async Task<T> FindFirstOrDefaultAsync<T>(this NsCollection<T> collection, Query.Query query, params SortDescription[] sorts)
            where T : class
        {
            return (await collection.FindAsync(query:query, sorts: sorts ?? SortDescription.OrderById(), skip: 0, take: 1)).FirstOrDefault();
        }
    }
}
