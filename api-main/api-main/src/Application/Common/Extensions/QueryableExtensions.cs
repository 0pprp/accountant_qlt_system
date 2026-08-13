using System.Linq.Expressions;
using Application.Common.Models.KeysetPagination;
using Application.Common.Models.Sorting;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> When<T>(this IQueryable<T> query, bool criteria, Expression<Func<T, bool>> predicate) =>
        criteria ? query.Where(predicate) : query;

    public static IEnumerable<T> When<T>(this IEnumerable<T> query, bool criteria, Func<T, bool> predicate) =>
        criteria ? query.Where(predicate) : query;

    public static IQueryable<T> ExcludeSoftDelete<T>(this IQueryable<T> query) where T : ISoftDelete
    {
        return query.Where(x => x.DeletedBy == null && x.DeletedAt == null);
    }

    /// <summary>
    /// Applies keyset pagination to an IQueryable using a specified key selector
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <typeparam name="TKey">The key type used for pagination</typeparam>
    /// <param name="query">The queryable to paginate</param>
    /// <param name="key">Expression to select the pagination key</param>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="descending">Whether to order in descending order (default: true)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Paginated queryable</returns>
    public static async Task<List<TEntity>> KeysetPaginateAsync<TEntity, TKey>(
        this IQueryable<TEntity> query,
        Expression<Func<TEntity, TKey>> key,
        KeysetPagination<TKey> pagination,
        bool descending = true,
        CancellationToken cancellationToken = default)
    {
        var orderedQuery = descending
            ? query.OrderByDescending(key)
            : query.OrderBy(key);

        if (pagination.LastId == null)
            return await orderedQuery
                .Take(pagination.Size)
                .ToListAsync(cancellationToken);

        var filteredQuery = descending
            ? orderedQuery.Where(BuildComparisonExpression(key, pagination.LastId, ExpressionType.LessThan))
            : orderedQuery.Where(BuildComparisonExpression(key, pagination.LastId, ExpressionType.GreaterThan));

        return await filteredQuery
            .Take(pagination.Size)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Builds a comparison expression for the keyset pagination filter
    /// </summary>
    private static Expression<Func<TEntity, bool>> BuildComparisonExpression<TEntity, TKey>(
        Expression<Func<TEntity, TKey>> key,
        TKey value,
        ExpressionType comparisonType)
    {
        var parameter = key.Parameters[0];
        var constantValue = Expression.Constant(value, typeof(TKey));

        var comparison = Expression.MakeBinary(comparisonType, key.Body, constantValue);
        return Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
    }

    /// <summary>
    /// Applies dynamic sorting if criteria are provided, otherwise sorts by the specified default expression in descending order.
    /// </summary>
    public static IQueryable<T> SortOrDefault<T>(
        this IQueryable<T> query,
        List<SortCriterion>? sortCriteria,
        Expression<Func<T, object>> defaultSort)
    {
        if (sortCriteria == null || sortCriteria.Count == 0)
        {
            return query.OrderByDescending(defaultSort);
        }

        return query.Sort(sortCriteria);
    }

    /// <summary>
    /// Applies dynamic sorting to the query based on property names and directions.
    /// Supports multiple sort fields using OrderBy/ThenBy pattern.
    /// </summary>
    public static IQueryable<T> Sort<T>(
        this IQueryable<T> query,
        List<SortCriterion>? sortCriteria)
    {
        if (sortCriteria is null || sortCriteria.Count == 0)
            return query;

        var queryType = typeof(T);
        var properties = queryType.GetProperties();

        var isFirstRound = true;

        foreach (var sortCriterion in sortCriteria)
        {
            var property = properties.FirstOrDefault(x => x.Name.Equals(sortCriterion.Property, StringComparison.OrdinalIgnoreCase));
            if (property is null)
                continue;

            var parameter = Expression.Parameter(queryType, "x");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            var methodName = isFirstRound
                ? sortCriterion.Direction == SortDirection.Ascending ? "OrderBy" : "OrderByDescending"
                : sortCriterion.Direction == SortDirection.Ascending
                    ? "ThenBy"
                    : "ThenByDescending";

            var orderingExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                [typeof(T), property.PropertyType],
                query.Expression,
                Expression.Quote(lambda)
            );

            query = query.Provider.CreateQuery<T>(orderingExpression);
            isFirstRound = false;
        }

        return query;
    }
}