using System;
using System.Linq.Expressions;

namespace TurboMapper
{
    public interface IMappingExpression<TSource, TTarget>
    {
        IMappingExpression<TSource, TTarget> ForMember<TValue>(Expression<Func<TTarget, TValue>> targetMember, Expression<Func<TSource, TValue>> sourceMember);

        IMappingExpression<TSource, TTarget> Ignore<TValue>(Expression<Func<TTarget, TValue>> targetMember);

        IMappingExpression<TSource, TTarget> When<TValue>(Expression<Func<TTarget, TValue>> targetMember, Func<TSource, bool> condition);

        IMappingExpression<TSource, TTarget> MapWith<TSourceValue, TTargetValue>(Expression<Func<TTarget, TTargetValue>> targetMember, Func<TSourceValue, TTargetValue> transformFunction);
    }
}