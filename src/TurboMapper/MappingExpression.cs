using System;
using System.Linq.Expressions;

namespace TurboMapper
{
    internal class MappingExpression<TSource, TTarget> : IMappingExpression<TSource, TTarget>
    {
        internal readonly System.Collections.Generic.List<PropertyMapping> Mappings = new System.Collections.Generic.List<PropertyMapping>();

        public IMappingExpression<TSource, TTarget> ForMember<TValue>(
            Expression<Func<TTarget, TValue>> targetMember,
            Expression<Func<TSource, TValue>> sourceMember)
        {
            var targetPath = GetMemberPath<TValue>(targetMember);
            var sourcePath = GetMemberPath(sourceMember);

            Mappings.Add(new PropertyMapping
            {
                SourcePropertyPath = sourcePath,
                TargetPropertyPath = targetPath,
                SourceProperty = GetLastPropertyName(targetPath),
                TargetProperty = GetLastPropertyName(targetPath)
            });

            return this;
        }

        private string GetMemberPath<TValue>(Expression<Func<TTarget, TValue>> expression)
        {
            var path = new System.Collections.Generic.List<string>();
            var memberExpression = expression.Body as MemberExpression;

            while (memberExpression != null)
            {
                path.Add(memberExpression.Member.Name);
                memberExpression = memberExpression.Expression as MemberExpression;
            }

            path.Reverse();
            return string.Join(".", path);
        }

        private string GetMemberPath<TSourceValue, TValue>(Expression<Func<TSourceValue, TValue>> expression)
        {
            var path = new System.Collections.Generic.List<string>();
            var memberExpression = expression.Body as MemberExpression;

            while (memberExpression != null)
            {
                path.Add(memberExpression.Member.Name);
                memberExpression = memberExpression.Expression as MemberExpression;
            }

            path.Reverse();
            return string.Join(".", path);
        }

        private string GetLastPropertyName(string path)
        {
            var parts = path.Split('.');
            return parts[parts.Length - 1];
        }
    }
}