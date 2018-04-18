using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;

namespace NoSqlMapper.Query
{
    internal static class QueryBuilder<T>
    {
        public static Query Build(Expression<Func<T, bool>> expression)
        {
            Validate.NotNull(expression, nameof(expression));

            return Build(expression.Body);
        }

        public static string BuildPath(Expression<Func<T, object>> expression)
        {
            Validate.NotNull(expression, nameof(expression));

            return BuildPath(expression.Body);
        }


        private static Query Build(Expression expression)
        {
            if (expression is BinaryExpression binaryExpression)
                return Build(binaryExpression);

            throw new NotSupportedException();
        }

        private static object BuildConstant(Expression expression)
        {
            if (expression is ConstantExpression constantExpression)
                return BuildConstant(constantExpression);
            if (expression is MemberExpression memberExpression)
                return BuildConstant(memberExpression);

            throw new NotSupportedException();
        }

        private static object BuildConstant(ConstantExpression expression)
        {
            return expression.Value;
        }

        private static object BuildConstant(MemberExpression expression)
        {
            if (expression.Expression is ConstantExpression constantExpression)
            {
                return constantExpression.Value.GetType().GetField(expression.Member.Name).GetValue(constantExpression.Value);
            }

            throw new NotSupportedException();
        }

        private static string BuildPath(Expression expression)
        {
            if (expression is MemberExpression memberExpression)
                return BuildPath(memberExpression);
            if (expression is ParameterExpression parameterExpression)
                return BuildPath(parameterExpression);
            if (expression is MethodCallExpression methodCallExpression)
                return BuildPath(methodCallExpression);
            if (expression is BinaryExpression binaryExpression)
                return BuildPath(binaryExpression);
            if (expression is UnaryExpression unaryExpression)
                return BuildPath(unaryExpression);

            throw new NotSupportedException();
        }

        private static string BuildPath(MemberExpression expression)
        {
            var innerPath = BuildPath(expression.Expression);
            if (innerPath == null)
                return expression.Member.Name;

            return string.Concat(innerPath, ".", expression.Member.Name);
        }

        private static string BuildPath(ParameterExpression expression)
        {
            return null;
        }

        private static string BuildPath(MethodCallExpression expression)
        {
            if (expression.Object is MemberExpression memberExpression)
                return BuildPath(memberExpression);

            throw new NotSupportedException();
        }

        private static string BuildPath(BinaryExpression expression)
        {
            return BuildPath(expression.Left);
        }

        private static string BuildPath(UnaryExpression expression)
        {
            return BuildPath(expression.Operand);
        }

        private static Query Build(BinaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Equal)
            {
                var path = BuildPath(expression.Left);
                var constant = BuildConstant(expression.Right);
                return constant == null ? Query.IsNull(path) : Query.Eq(path, constant);
            }
            if (expression.NodeType == ExpressionType.NotEqual)
            {
                var path = BuildPath(expression.Left);
                var constant = BuildConstant(expression.Right);
                return constant == null ? Query.IsNotNull(path) : Query.Neq(path, constant);
            }
            if (expression.NodeType == ExpressionType.OrElse)
                return Query.Or(Build(expression.Left), Build(expression.Right));
            if (expression.NodeType == ExpressionType.AndAlso)
                return Query.And(Build(expression.Left), Build(expression.Right));

            if (expression.NodeType == ExpressionType.LessThan)
                return Query.Lt(BuildPath(expression.Left), BuildConstant(expression.Right));
            if (expression.NodeType == ExpressionType.LessThanOrEqual)
                return Query.Lte(BuildPath(expression.Left), BuildConstant(expression.Right));

            if (expression.NodeType == ExpressionType.GreaterThan)
                return Query.Gt(BuildPath(expression.Left), BuildConstant(expression.Right));
            if (expression.NodeType == ExpressionType.GreaterThanOrEqual)
                return Query.Gte(BuildPath(expression.Left), BuildConstant(expression.Right));

            throw new NotSupportedException();
        }

    }
}
