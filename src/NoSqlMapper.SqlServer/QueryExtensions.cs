using System;
using System.Collections.Generic;
using System.Text;
using NoSqlMapper.Query;

namespace NoSqlMapper.SqlServer
{
    internal static class QueryExtensions
    {
        public static string ConvertToSql(this Query.Query query, List<KeyValuePair<int, object>> parameters)
        {
            if (query is QueryUnary unary)
                return ConvertToSql(unary, parameters);

            throw new NotSupportedException();
        }

        public static string ConvertToSql(this Query.QueryUnary queryUnary, List<KeyValuePair<int, object>> parameters)
        {
            parameters.Add(new KeyValuePair<int, object>(parameters.Count + 1, queryUnary.Value));

            if (queryUnary.Value is string)
                return $"(JSON_VALUE(_document,'$.{queryUnary.Field}') {ConvertToSql(queryUnary.Op)} @{parameters.Count})";

            return $"(CAST(JSON_VALUE(_document,'$.{queryUnary.Field}') AS {ConvertToSql(queryUnary.Field.GetType())}) {ConvertToSql(queryUnary.Op)} @{parameters.Count})";
        }

        public static string ConvertToSql(UnaryOperator op)
        {
            switch (op)
            {
                case UnaryOperator.EqualTo:
                    return "=";
                case UnaryOperator.NotEqualTo:
                    return "!=";
                case UnaryOperator.GreaterOrEqualTo:
                    return ">=";
                case UnaryOperator.GreaterThan:
                    return ">";
                case UnaryOperator.LessOrEqualTo:
                    return "<=";
                case UnaryOperator.LessThan:
                    return "<";
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }

        public static string ConvertToSql(Type type)
        {
            if (type == typeof(int))
                return "INT";

            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

    }
}
