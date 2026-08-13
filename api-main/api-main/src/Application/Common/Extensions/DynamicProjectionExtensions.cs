using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Extensions;

public static class DynamicProjectionExtensions
{
    /// <summary>
    /// Projects the queryable to a dictionary of field values based on the provided field selectors.
    /// </summary>
    public static async Task<List<Dictionary<string, object?>>> ProjectToDynamicAsync<TEntity>(
        this IQueryable<TEntity> query,
        Dictionary<string, Expression<Func<TEntity, object?>>> fieldSelectors,
        CancellationToken cancellationToken = default)
    {
        // shared parameter for all projections
        var parameter = Expression.Parameter(typeof(TEntity), "x");

        var fieldKeys = fieldSelectors.Keys.ToList();
        var projections = new List<Expression>();

        foreach (var selector in fieldSelectors.Values)
        {
            // Replace the parameter in each selector to use our shared parameter
            var body = ParameterReplacer.Replace(selector.Body, selector.Parameters[0], parameter);

            // Ensure the expression returns object (box value types)
            if (body.Type.IsValueType)
            {
                body = Expression.Convert(body, typeof(object));
            }

            projections.Add(body);
        }

        // array initializer, example: new object[] { x.Id, x.Name }
        var arrayInit = Expression.NewArrayInit(typeof(object), projections);
        var lambda = Expression.Lambda<Func<TEntity, object[]>>(arrayInit, parameter);

        var results = await query.Select(lambda).ToListAsync(cancellationToken);

        return results.Select(values =>
        {
            var dict = new Dictionary<string, object?>(fieldKeys.Count);

            for (var i = 0; i < fieldKeys.Count; i++)
            {
                dict[fieldKeys[i]] = values[i];
            }

            return dict;
        }).ToList();
    }

    /// <summary>
    /// Helper class to replace parameters in expressions
    /// </summary>
    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly Expression _newExpression;

        private ParameterReplacer(ParameterExpression oldParameter, Expression newExpression)
        {
            _oldParameter = oldParameter;
            _newExpression = newExpression;
        }

        public static Expression Replace(Expression expression, ParameterExpression oldParameter, Expression newExpression)
        {
            return new ParameterReplacer(oldParameter, newExpression).Visit(expression);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newExpression : base.VisitParameter(node);
        }
    }
}