using System;
using System.Linq.Expressions;

namespace TurboMapper
{
    /// <summary>
    /// Defines a mapping expression for configuring property mappings between source and target types.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    public interface IMappingExpression<TSource, TTarget>
    {
        /// <summary>
        /// Configures a mapping for a specific member from the source type to the target type.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="targetMember"></param>
        /// <param name="sourceMember"></param>
        /// <returns></returns>
        IMappingExpression<TSource, TTarget> ForMember<TValue>(Expression<Func<TTarget, TValue>> targetMember, Expression<Func<TSource, TValue>> sourceMember);

        /// <summary>
        /// Ignores a specific member in the target type during the mapping process.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="targetMember"></param>
        /// <returns></returns>
        IMappingExpression<TSource, TTarget> Ignore<TValue>(Expression<Func<TTarget, TValue>> targetMember);

        /// <summary>
        /// Applies a condition to a specific member in the target type, determining whether it should be mapped based on the provided condition function.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="targetMember"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        IMappingExpression<TSource, TTarget> When<TValue>(Expression<Func<TTarget, TValue>> targetMember, Func<TSource, bool> condition);

        /// <summary>
        /// Maps a target member using a custom transformation function that takes a source value and produces a target value.
        /// </summary>
        /// <typeparam name="TSourceValue"></typeparam>
        /// <typeparam name="TTargetValue"></typeparam>
        /// <param name="targetMember"></param>
        /// <param name="transformFunction"></param>
        /// <returns></returns>
        IMappingExpression<TSource, TTarget> MapWith<TSourceValue, TTargetValue>(Expression<Func<TTarget, TTargetValue>> targetMember, Func<TSourceValue, TTargetValue> transformFunction);
    }
}