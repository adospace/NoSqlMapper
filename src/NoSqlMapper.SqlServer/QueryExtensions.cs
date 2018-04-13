using System;
using System.Collections.Generic;
using System.Text;
using NoSqlMapper.Query;

namespace NoSqlMapper.SqlServer
{
    internal static class QueryExtensions
    {

        public static string ConvertToSql(this Query.Query query, TypeReflector typeReflector, string tableName, List<KeyValuePair<int, object>> parameters)
        {
            var crossApplyPaths = new List<string>();

            var whereClause = query.ConvertToSqlWhere(typeReflector, parameters, crossApplyPaths);

            var sql = new List<string>();
            sql.Append($"SELECT _id, _document FROM [dbo].[{tableName}]")
               .Append($"WHERE ({whereClause})");

            return string.Join(Environment.NewLine, sql);
        }


        private static string ConvertToSqlWhere(this Query.Query query, TypeReflector typeReflector, List<KeyValuePair<int, object>> parameters, List<string> crossApplyPath)
        {
            switch (query)
            {
                case QueryUnary queryUnary:
                    return ConvertToSqlWhere(queryUnary, typeReflector, parameters);
                case QueryIsNull queryIsNull:
                    return ConvertToSqlWhere(queryIsNull, typeReflector, parameters);
                case QueryIsNotNull queryIsNotNull:
                    return ConvertToSqlWhere(queryIsNotNull, typeReflector, parameters);
                case QueryBinary queryBinary:
                    return ConvertToSqlWhere(queryBinary, typeReflector, parameters, crossApplyPath);
            }

            throw new NotSupportedException();
        }

        public static string ConvertToSqlWhere(this Query.QueryUnary queryUnary, TypeReflector typeReflector,
            List<KeyValuePair<int, object>> parameters)
        {
            parameters.Add(new KeyValuePair<int, object>(parameters.Count + 1, queryUnary.Value));

            var reflectedType = typeReflector.Navigate(queryUnary.Field);
            if (reflectedType == null)
                throw new InvalidOperationException(
                    $"Unable to find property '{queryUnary.Field}' on type '{typeReflector}'");

            if (queryUnary.Op == UnaryOperator.Contains)
            {
                if (reflectedType.Type.IsArray)
                    return $"(@{parameters.Count} IN (SELECT value FROM OPENJSON(_document, '$.{queryUnary.Field}')))";
            }
            else
            {
                if (reflectedType.Is(typeof(string)))
                    return
                        $"JSON_VALUE(_document,'$.{queryUnary.Field}') {ConvertToSql(queryUnary.Op)} @{parameters.Count}";
            }

            return
                $"CAST(JSON_VALUE(_document,'$.{queryUnary.Field}') AS {ConvertToSql(reflectedType.Type)}) {ConvertToSql(queryUnary.Op)} @{parameters.Count}";
        }
    

        public static string ConvertToSqlWhere(this Query.QueryIsNull queryIsNull, TypeReflector typeReflector, List<KeyValuePair<int, object>> parameters)
        {
            var reflectedType = typeReflector.Navigate(queryIsNull.Field);
            if (reflectedType == null)
                throw new InvalidOperationException($"Unable to find property '{queryIsNull.Field}' on type '{typeReflector}'");

            return $"JSON_VALUE(_document,'$.{queryIsNull.Field}') IS NULL";
        }

        public static string ConvertToSqlWhere(this Query.QueryIsNotNull queryIsNotNull, TypeReflector typeReflector, List<KeyValuePair<int, object>> parameters)
        {
            var reflectedType = typeReflector.Navigate(queryIsNotNull.Field);
            if (reflectedType == null)
                throw new InvalidOperationException($"Unable to find property '{queryIsNotNull.Field}' on type '{typeReflector}'");
            return $"JSON_VALUE(_document,'$.{queryIsNotNull.Field}') IS NOT NULL";
        }

        public static string ConvertToSqlWhere(this Query.QueryBinary queryBinary, TypeReflector typeReflector, List<KeyValuePair<int, object>> parameters, List<string> crossApplyPath)
        {
            Validate.NotNull(queryBinary.Left, nameof(queryBinary), "Left");
            Validate.NotNull(queryBinary.Right, nameof(queryBinary), "Right");

            return $"({ConvertToSqlWhere(queryBinary.Left, typeReflector, parameters, crossApplyPath)}) " +
                   $"{(queryBinary.Op == LogicalOperator.And ? "AND" : "OR")}  " +
                   $"({ConvertToSqlWhere(queryBinary.Right, typeReflector, parameters, crossApplyPath)})";
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
