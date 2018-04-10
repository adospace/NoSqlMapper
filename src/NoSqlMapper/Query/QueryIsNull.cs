using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace NoSqlMapper.Query
{
    public class QueryIsNull : Query
    {
        [NotNull] public string Field { get; }

        public QueryIsNull([NotNull] string field)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));

            Field = field;
        }

        public override string ToString()
        {
            return $"( {Field} IS NULL )";
        }
    }
}
