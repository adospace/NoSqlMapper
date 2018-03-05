using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NoSqlMapper
{
    public interface ISqlDatabaseProvider
    {
        [ItemCanBeNull]
        Task<string> EnsureDatabaseAsync([NotNull] string databaseName, string databaseDesign = null);

        [NotNull]
        Task DeleteDatabaseAsync([NotNull] string databaseName);

    }
}
