using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoSqlMapper.Query
{
    public static class Extensions
    {
        public static SortDescription[] ThenBy(this SortDescription[] descriptions, string field)
        {
            return descriptions.Concat(new[] {new SortDescription(field, SortOrder.Ascending),}).ToArray();
        }

        public static SortDescription[] ThenByDescending(this SortDescription[] descriptions, string field)
        {
            return descriptions.Concat(new[] { new SortDescription(field, SortOrder.Descending), }).ToArray();
        }
    }
}
