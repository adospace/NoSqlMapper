using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace NoSqlMapper.Query
{
    public class SortDescription
    {
        [NotNull] public string Field { get; }
        public SortOrder Order { get; }

        public SortDescription([NotNull] string field, SortOrder order)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));

            Field = field;
            Order = order;
        }

        public static SortDescription[] OrderBy([NotNull] string field)
        {
            return new[] {new SortDescription(field, SortOrder.Ascending)};
        }

        public static SortDescription[] OrderByDescending([NotNull] string field)
        {
            return new[] { new SortDescription(field, SortOrder.Descending)};
        }
    }
}
