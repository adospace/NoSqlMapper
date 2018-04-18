using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace NoSqlMapper.Query
{
    public class QueryBinary : Query
    {
        [NotNull] public Query Left { get; }
        [NotNull] public Query Right { get; }
        public LogicalOperator Op { get; }

        internal QueryBinary([NotNull] Query left, [NotNull] Query right, LogicalOperator op)
        {
            Validate.NotNull(left, nameof(left));
            Validate.NotNull(right, nameof(right));
            Left = left;
            Right = right;
            Op = op;
        }

        public override string ToString()
        {
            return $"( {Left} {Op} {Right} )";
        }
    }
}
