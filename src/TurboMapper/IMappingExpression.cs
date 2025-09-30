using System;
using System.Linq.Expressions;

namespace TurboMapper
{
    public interface IMappingExpression<TSource, TTarget>
    {
        IMappingExpression<TSource, TTarget> ForMember<TValue>(Expression<Func<TTarget, TValue>> targetMember, Expression<Func<TSource, TValue>> sourceMember);
    }
}