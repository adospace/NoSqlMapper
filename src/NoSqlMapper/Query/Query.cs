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
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));
            Validate.NotNull(value, nameof(value));

            return new QueryUnary(field, UnaryOperator.EqualTo, value);
        }

        public static Query Neq([NotNull] string field, [NotNull] object value)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));
            Validate.NotNull(value, nameof(value));

            return new QueryUnary(field, UnaryOperator.NotEqualTo, value);
        }

        public static Query And([NotNull] Query left, [NotNull] Query right)
        {
            Validate.NotNull(left, nameof(left));
            Validate.NotNull(right, nameof(right));

            return new QueryBinary(left, right, LogicalOperator.And);
        }

        public static Query Or([NotNull] Query left, [NotNull] Query right)
        {
            Validate.NotNull(left, nameof(left));
            Validate.NotNull(right, nameof(right));

            return new QueryBinary(left, right, LogicalOperator.Or);
        }

        public static Query Contains([NotNull] string field, [NotNull] object value)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));
            Validate.NotNull(value, nameof(value));

            return new QueryUnary(field, UnaryOperator.Contains, value);
        }

        public static Query NotContains([NotNull] string field, [NotNull] object value)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));
            Validate.NotNull(value, nameof(value));

            return new QueryUnary(field, UnaryOperator.NotContains, value);
        }

        public static Query IsNull([NotNull] string field)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));

            return new QueryIsNull(field);
        }

        public static Query IsNotNull([NotNull] string field)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));

            return new QueryIsNotNull(field);
        }
    }
}
