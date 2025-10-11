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
            var targetPath = GetMemberPathForTarget(targetMember);
            var sourcePath = GetMemberPathForSource(sourceMember);

            Mappings.Add(new PropertyMapping
            {
                SourcePropertyPath = sourcePath,
                TargetPropertyPath = targetPath,
                SourceProperty = GetLastPropertyName(targetPath),
                TargetProperty = GetLastPropertyName(targetPath)
            });

            return this;
        }

        private string GetMemberPathForTarget<TValue>(Expression<Func<TTarget, TValue>> expression)
        {
            return GetMemberPathInternal(expression);
        }

        private string GetMemberPathForSource<TValue>(Expression<Func<TSource, TValue>> expression)
        {
            return GetMemberPathInternal(expression);
        }

        private static string GetMemberPathInternal<TExpressionValue, TValue>(Expression<Func<TExpressionValue, TValue>> expression)
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

        public IMappingExpression<TSource, TTarget> Ignore<TValue>(Expression<Func<TTarget, TValue>> targetMember)
        {
            var targetPath = GetMemberPathForTarget(targetMember);
            var targetProperty = GetLastPropertyName(targetPath);

            // Add an ignored property mapping
            Mappings.Add(new PropertyMapping
            {
                TargetProperty = targetProperty,
                TargetPropertyPath = targetPath,
                IsIgnored = true
            });

            return this;
        }

        public IMappingExpression<TSource, TTarget> When<TValue>(Expression<Func<TTarget, TValue>> targetMember, Func<TSource, bool> condition)
        {
            var targetPath = GetMemberPathForTarget(targetMember);
            var targetProperty = GetLastPropertyName(targetPath);

            // Add a conditional property mapping
            Mappings.Add(new PropertyMapping
            {
                TargetProperty = targetProperty,
                TargetPropertyPath = targetPath,
                Condition = source => condition((TSource)source)
            });

            return this;
        }

        public IMappingExpression<TSource, TTarget> MapWith<TSourceValue, TTargetValue>(Expression<Func<TTarget, TTargetValue>> targetMember, Func<TSourceValue, TTargetValue> transformFunction)
        {
            var targetPath = GetMemberPathForTarget(targetMember);
            var targetProperty = GetLastPropertyName(targetPath);

            // For MapWith, we'll create a property mapping where the source property has the same name
            // as the target property, but we'll apply the transformation function
            var sourcePath = targetPath; // Assuming same property name for simplicity

            // Add a transformation property mapping
            Mappings.Add(new PropertyMapping
            {
                SourceProperty = targetProperty, // Same name for source and target
                TargetProperty = targetProperty,
                SourcePropertyPath = sourcePath,
                TargetPropertyPath = targetPath,
                TransformFunction = transformFunction
            });

            return this;
        }
    }
}