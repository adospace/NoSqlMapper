using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NoSqlMapper.SqlServer
{
    internal static class StringListExtensions
    {
        public static List<string> Append(this List<string> list, params string[] items)
        {
            Validate.NotNull(list, nameof(list));
            Validate.NotNull(items, nameof(items));
            list.AddRange(items);
            return list;
        }
    }
}
