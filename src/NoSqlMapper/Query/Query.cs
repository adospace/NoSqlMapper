using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace NoSqlMapper.Query
{
    public abstract class Query
    {
        public static Query Eq([NotNull] string field, [NotNull] object value)
        {
            return new QueryUnary(field, UnaryOperator.EqualTo, value);
        }

        public static Query Neq([NotNull] string field, [NotNull] object value)
        {
            return new QueryUnary(field, UnaryOperator.NotEqualTo, value);
        }

        public static Query And([NotNull] Query left, [NotNull] Query right)
        {
            return new QueryBinary(left, right, LogicalOperator.And);
        }

        public static Query Or([NotNull] Query left, [NotNull] Query right)
        {
            return new QueryBinary(left, right, LogicalOperator.Or);
        }
    }
}
