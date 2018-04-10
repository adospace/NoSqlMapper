using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace NoSqlMapper.Query
{
    public class QueryUnary : Query
    {
        [NotNull] public string Field { get; }
        public UnaryOperator Op { get; }
        [NotNull] public object Value { get; }

        public QueryUnary([NotNull] string field, UnaryOperator op, [NotNull] object value)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(field, nameof(field));
            Validate.NotNull(value, nameof(value));

            Field = field;
            Op = op;
            Value = value;
        }

        public override string ToString()
        {
            return $"( {Field} {Op} {Value} )";
        }
    }
}
