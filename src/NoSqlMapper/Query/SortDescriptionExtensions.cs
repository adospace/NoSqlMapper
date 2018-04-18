using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoSqlMapper.Query
{
    public static class SortDescriptionExtensions
    {
        public static SortDescription[] ThenBy(this SortDescription[] sorts, string field)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));

            return sorts.Concat(new[] {new SortDescription(field, SortOrder.Ascending)}).ToArray();
        }

        public static SortDescription[] ThenByDescending(this SortDescription[] sorts, string field)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));

            return sorts.Concat(new[] { new SortDescription(field, SortOrder.Descending) }).ToArray();
        }
    }
}
