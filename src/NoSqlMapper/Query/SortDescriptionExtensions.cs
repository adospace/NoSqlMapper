using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;

namespace NoSqlMapper.Query
{
    public static class SortDescriptionExtensions
    {
        public static SortDescription[] ThenBy([NotNull] this SortDescription[] sorts, [NotNull] string field)
        {
            Validate.NotNull(sorts, nameof(sorts));
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));

            return sorts.Concat(new[] {new SortDescription(field, SortOrder.Ascending)}).ToArray();
        }

        public static SortDescription[] ThenByDescending([NotNull] this SortDescription[] sorts, [NotNull]string field)
        {
            Validate.NotNull(sorts, nameof(sorts));
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));

            return sorts.Concat(new[] { new SortDescription(field, SortOrder.Descending) }).ToArray();
        }

        public static SortDescription[] OrderBy<T>([NotNull] Expression<Func<T, object>> field) where T : class 
        {
            Validate.NotNull(field, nameof(field));

            return new[] { new SortDescription(QueryBuilder<T>.BuildPath(field), SortOrder.Ascending) };
        }

        public static SortDescription[] OrderByDescending<T>([NotNull] Expression<Func<T, object>> field) where T : class
        {
            Validate.NotNull(field, nameof(field));

            return new[] { new SortDescription(QueryBuilder<T>.BuildPath(field), SortOrder.Descending) };
        }

        public static SortDescription[] ThenBy<T>([NotNull] this SortDescription[] sorts, [NotNull] Expression<Func<T, object>> field)
        {
            Validate.NotNull(sorts, nameof(sorts));
            Validate.NotNull(field, nameof(field));

            return sorts.Concat(new[] { new SortDescription(QueryBuilder<T>.BuildPath(field), SortOrder.Ascending) }).ToArray();
        }

        public static SortDescription[] ThenByDescending<T>([NotNull] this SortDescription[] sorts, [NotNull] Expression<Func<T, object>> field)
        {
            Validate.NotNull(sorts, nameof(sorts));
            Validate.NotNull(field, nameof(field));

            return sorts.Concat(new[] { new SortDescription(QueryBuilder<T>.BuildPath(field), SortOrder.Descending) }).ToArray();
        }
    }
}
