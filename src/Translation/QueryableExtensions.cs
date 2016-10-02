using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Translation
{
    public static class QueryableExtensions
    {
        public static IQueryable<TResult> Join<TOuter, TInner, TResult>(
            this IQueryable<TOuter> outer,
            IQueryable<TInner> inner,
            Expression<Func<TOuter, TInner,bool>> joinCondition,
            Expression<Func<TOuter, TInner, TResult>> resultSelector,
            JoinType joinType = JoinType.Inner)
        {   
            var method = typeof(QueryableExtensions).GetMethod("Join");
            var callExpression = Expression.Call(
                null,
                method.MakeGenericMethod(new []
                    { 
                        typeof(TOuter),
                        typeof(TInner),
                        typeof(TResult) 
                    }),
                new Expression[] 
                    { 
                        outer.Expression, 
                        inner.Expression,
                        Expression.Quote(joinCondition),
                        Expression.Quote(resultSelector),
                        Expression.Constant(joinType)
                    });
                    
            return outer.Provider.CreateQuery<TResult>(callExpression);
        }
    }
}