using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoSqlMapper.Query;

namespace NoSqlMapper.SqlServer
{
    internal static class SqlUtils
    {
        private class CrossApplyDefinition
        {
            private CrossApplyDefinition _parent;

            public CrossApplyDefinition Parent
            {
                get { return _parent; }
                set
                {
#if DEBUG
                    if (_parent != null || (value != null && value.Parent != null))
                        throw new InvalidOperationException();
#endif
                    _parent = value;
                    _parent?._children.Add(this);
                }
            }

            private readonly List<CrossApplyDefinition> _children = new List<CrossApplyDefinition>();
            public IEnumerable<CrossApplyDefinition> Children => _children;

            public string Name { get; set; }

            public string Path => Parent == null ? Name : string.Concat(Parent.Path, ".", Name);
        }

        private class FieldType
        {
            public TypeReflector Type { get; set; }
            public string Path { get; set; }
            public string ParentField { get; set; }
        }

        public static string ConvertToSql(List<string> sqlLines, Query.Query query, TypeReflector typeReflector, string tableName, List<KeyValuePair<int, object>> parameters)
        {
            var crossApplyPaths = new Dictionary<string, CrossApplyDefinition>();

            var whereClause = BuildSqlWhere(query, typeReflector, parameters, crossApplyPaths);

            //select *, JSON_VALUE(Replies.value, '$.Content') from [dbo].[posts] _doc
            //cross apply openjson(_doc._document, '$.Comments') WITH (Comments nvarchar(MAX) '$'  AS JSON)
            //cross apply openjson(Comments, '$.Replies') Replies
            //where JSON_VALUE(Comments, '$.Author.Username') = 'admin'

            sqlLines.Append($"SELECT{(crossApplyPaths.Any() ? " DISTINCT" : string.Empty)} _id, _document FROM [dbo].[{tableName}] _doc");

            foreach (var crossApplyDefinition in crossApplyPaths.Where(_=>_.Value.Parent == null).OrderBy(_=>_.Key))
            {
                AppendCrossJoinApply(sqlLines, crossApplyDefinition.Value);
            }

            sqlLines.Append($"WHERE ({whereClause})");

            return string.Join(Environment.NewLine, sqlLines);
        }

        private static void AppendCrossJoinApply(List<string> sqlLines, CrossApplyDefinition definition)
        {
            sqlLines.Append(
                $"CROSS APPLY OPENJSON({(definition.Parent == null ? "_doc._document" : "[" + definition.Parent.Name + "]")}, '$.{definition.Name}') WITH ([{definition.Path}] nvarchar(MAX) '$' AS JSON)");

            foreach (var childDefinition in definition.Children)
            {
                AppendCrossJoinApply(sqlLines, childDefinition);
            }
        }

        private static string BuildSqlWhere(Query.Query query, 
            TypeReflector typeReflector, 
            List<KeyValuePair<int, object>> parameters,
            IDictionary<string, CrossApplyDefinition> crossApplyDefinitions)
        {
            switch (query)
            {
                case QueryUnary queryUnary:
                    return ConvertToSqlWhere(queryUnary, typeReflector, parameters, crossApplyDefinitions);
                case QueryIsNull queryIsNull:
                    return ConvertToSqlWhere(queryIsNull, typeReflector, parameters, crossApplyDefinitions);
                case QueryIsNotNull queryIsNotNull:
                    return ConvertToSqlWhere(queryIsNotNull, typeReflector, parameters, crossApplyDefinitions);
                case QueryBinary queryBinary:
                    return ConvertToSqlWhere(queryBinary, typeReflector, parameters, crossApplyDefinitions);
            }

            throw new NotSupportedException();
        }

        private static FieldType ResolveField(string originalPath, TypeReflector typeReflector, IDictionary<string, CrossApplyDefinition> crossApplyDefinitions)
        {
            var fieldTypes = typeReflector.Navigate(originalPath).ToList();
            var resultingPath = new List<string>();
            var tempPath = new List<string>();
            CrossApplyDefinition lastCrossApplyDefinition = null;
            foreach (var fieldType in fieldTypes)
            {
                if (fieldType.IsObjectArray)
                {
                    if (fieldType == fieldTypes.Last())
                        throw new InvalidOperationException();

                    tempPath.Add(fieldType.Name);

                    lastCrossApplyDefinition = new CrossApplyDefinition
                    {
                        Parent = fieldType.Parent?.Path != null ? crossApplyDefinitions[fieldType.Parent.Path] : null,
                        Name = string.Join(".", tempPath)
                    };

                    crossApplyDefinitions[lastCrossApplyDefinition.Name] = lastCrossApplyDefinition;
                    tempPath.Clear();
                }
                else
                {
                    resultingPath.Add(fieldType.Name);
                    tempPath.Add(fieldType.Name);
                }
            }

            return new FieldType()
            {
                Type = fieldTypes.Last(),
                Path = string.Join(".", resultingPath),
                ParentField = lastCrossApplyDefinition?.Path ?? "_document"
            };
        }

        private static string ConvertToSqlWhere(QueryUnary queryUnary, TypeReflector typeReflector,
            List<KeyValuePair<int, object>> parameters, IDictionary<string, CrossApplyDefinition> crossApplyDefinitions)
        {
            parameters.Add(new KeyValuePair<int, object>(parameters.Count + 1, queryUnary.Value));

            var reflectedType = ResolveField(queryUnary.Field, typeReflector, crossApplyDefinitions);

            if (queryUnary.Op == UnaryOperator.Contains ||
                queryUnary.Op == UnaryOperator.NotContains)
            {
                if (reflectedType.Type.IsArray)
                    return
                        $"(@{parameters.Count}{(queryUnary.Op == UnaryOperator.NotContains ? " NOT" : string.Empty)} IN (SELECT value FROM OPENJSON(_document, '$.{queryUnary.Field}')))";
            }

            if (reflectedType.Type.Is(typeof(string)))
                return
                    $"JSON_VALUE([{reflectedType.ParentField}],'$.{reflectedType.Path}') {ConvertToSql(queryUnary.Op)} @{parameters.Count}";

            return
                $"CAST(JSON_VALUE([{reflectedType.ParentField}],'$.{reflectedType.Path}') AS {ConvertToSql(reflectedType.Type.Type)}) {ConvertToSql(queryUnary.Op)} @{parameters.Count}";
        }


        private static string ConvertToSqlWhere(QueryIsNull queryIsNull, TypeReflector typeReflector, List<KeyValuePair<int, object>> parameters, IDictionary<string, CrossApplyDefinition> crossApplyDefinitions)
        {
            var reflectedType = typeReflector.Navigate(queryIsNull.Field);
            if (reflectedType == null)
                throw new InvalidOperationException($"Unable to find property '{queryIsNull.Field}' on type '{typeReflector}'");

            return $"JSON_VALUE(_document,'$.{queryIsNull.Field}') IS NULL";
        }

        private static string ConvertToSqlWhere(QueryIsNotNull queryIsNotNull, TypeReflector typeReflector, List<KeyValuePair<int, object>> parameters, IDictionary<string, CrossApplyDefinition> crossApplyDefinitions)
        {
            var reflectedType = typeReflector.Navigate(queryIsNotNull.Field);
            if (reflectedType == null)
                throw new InvalidOperationException($"Unable to find property '{queryIsNotNull.Field}' on type '{typeReflector}'");
            return $"JSON_VALUE(_document,'$.{queryIsNotNull.Field}') IS NOT NULL";
        }

        private static string ConvertToSqlWhere(QueryBinary queryBinary, TypeReflector typeReflector, List<KeyValuePair<int, object>> parameters, IDictionary<string, CrossApplyDefinition> crossApplyDefinitions)
        {
            Validate.NotNull(queryBinary.Left, nameof(queryBinary), "Left");
            Validate.NotNull(queryBinary.Right, nameof(queryBinary), "Right");

            return $"({BuildSqlWhere(queryBinary.Left, typeReflector, parameters, crossApplyDefinitions)})" +
                   $" {(queryBinary.Op == LogicalOperator.And ? "AND" : "OR")} " +
                   $"({BuildSqlWhere(queryBinary.Right, typeReflector, parameters, crossApplyDefinitions)})";
        }

        private static string ConvertToSql(UnaryOperator op)
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

        private static string ConvertToSql(Type type)
        {
            if (type == typeof(int))
                return "INT";

            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

    }
}
