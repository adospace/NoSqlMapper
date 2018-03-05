using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NoSqlMapper
{
    public interface ISqlDatabaseProvider
    {
        [ItemNotNull]
        Task<NoSqlDatabase> GetDatabaseAsync([NotNull] string databaseName);
    }
}
