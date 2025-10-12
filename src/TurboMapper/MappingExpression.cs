using System;
using System.Linq.Expressions;

namespace TurboMapper
{
    /// <summary>
    /// Defines a mapping expression for configuring property mappings between source and target types.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    internal class MappingExpression<TSource, TTarget> : IMappingExpression<TSource, TTarget>
    {
        /// <summary>
        /// Holds the list of property mappings configured for this mapping expression.
        /// </summary>
        internal readonly System.Collections.Generic.List<PropertyMapping> Mappings = new System.Collections.Generic.List<PropertyMapping>();

        /// <summary>
        /// Configures a mapping for a specific member in the target type, specifying the corresponding member in the source type.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="targetMember"></param>
        /// <param name="sourceMember"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Extracts the member path from a given expression for the target type, supporting nested properties.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        private string GetMemberPathForTarget<TValue>(Expression<Func<TTarget, TValue>> expression)
        {
            return GetMemberPathInternal(expression);
        }
        /// <summary>
        /// Extracts the member path from a given expression for the source type, supporting nested properties.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        private string GetMemberPathForSource<TValue>(Expression<Func<TSource, TValue>> expression)
        {
            return GetMemberPathInternal(expression);
        }
        /// <summary>
        /// Extracts the member path from a given expression, supporting nested properties.
        /// </summary>
        /// <typeparam name="TExpressionValue"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Ignores a specific member in the target type during the mapping process.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="targetMember"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Applies a condition to a specific member in the target type, determining whether it should be mapped based on the provided condition function.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="targetMember"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Maps a target member using a custom transformation function that takes a source value and produces a target value.
        /// </summary>
        /// <typeparam name="TSourceValue"></typeparam>
        /// <typeparam name="TTargetValue"></typeparam>
        /// <param name="targetMember"></param>
        /// <param name="transformFunction"></param>
        /// <returns></returns>
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