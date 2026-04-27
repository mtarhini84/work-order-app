using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
namespace WorkOrderApp.Helpers.Paging
{
    public static class IQueryableExtensions
    {

        public static IQueryable<T> ApplyFilters<T>( this IQueryable<T> source, Dictionary<string, string[]> filters) where T : class
        {
            if (filters == null || filters.Count == 0)
                return source;

            var entityType = typeof(T);
            var param = Expression.Parameter(entityType, "x");

            foreach (var kv in filters)
            {
                // 1) find the real property, case‑insensitive
                var propInfo = entityType
                    .GetProperty(kv.Key,
                                 BindingFlags.IgnoreCase
                               | BindingFlags.Public
                               | BindingFlags.Instance);
                if (propInfo == null)
                    continue;

                // 2) build x.Prop
                var propertyAccess = Expression.Property(param, propInfo);

                // 3) for each filter value, convert to the right type and == compare
                Expression? orExpr = null;
                foreach (var stringVal in kv.Value)
                {
                    object? typedVal;
                    try
                    {
                        typedVal = Convert.ChangeType(stringVal, propInfo.PropertyType);
                    }
                    catch
                    {
                        // skip invalid conversions
                        continue;
                    }

                    var constant = Expression.Constant(typedVal, propInfo.PropertyType);
                    var equals = Expression.Equal(propertyAccess, constant);
                    orExpr = orExpr == null
                           ? equals
                           : Expression.OrElse(orExpr, equals);
                }

                if (orExpr == null)
                    continue;

                var lambda = Expression.Lambda<Func<T, bool>>(orExpr, param);
                source = source.Where(lambda);
            }

            return source;
        }

        public static IQueryable<T> ApplySearch<T>( this IQueryable<T> source, string searchTerm, string[] searchColumns)where T : class
        {
            if (string.IsNullOrWhiteSpace(searchTerm)
             || searchColumns.Length == 0)
                return source;

            var entityType = typeof(T);
            var param = Expression.Parameter(entityType, "x");
            var lowerTerm = searchTerm.ToLower();

            Expression? orExpr = null;
            foreach (var rawCol in searchColumns)
            {
                var propInfo = entityType
                    .GetProperty(rawCol,
                                 BindingFlags.IgnoreCase
                               | BindingFlags.Public
                               | BindingFlags.Instance);
                if (propInfo == null
                 || propInfo.PropertyType != typeof(string))
                    continue;

                // EF.Property<string>(x, "Name")
                var propAccess = Expression.Call(
                    typeof(EF).GetMethod(nameof(EF.Property))!
                               .MakeGenericMethod(typeof(string)),
                    param,
                    Expression.Constant(propInfo.Name)
                );

                // .ToLower()
                var toLower = Expression.Call(
                    propAccess,
                    nameof(string.ToLower),
                    Type.EmptyTypes
                );

                // .Contains(lowerTerm)
                var contains = Expression.Call(
                    toLower,
                    nameof(string.Contains),
                    Type.EmptyTypes,
                    Expression.Constant(lowerTerm)
                );

                orExpr = orExpr == null
                       ? contains
                       : Expression.OrElse(orExpr, contains);
            }

            if (orExpr == null)
                return source;

            var lambda = Expression.Lambda<Func<T, bool>>(orExpr, param);
            return source.Where(lambda);
        }

        public static IQueryable<T> ApplySorting<T>( this IQueryable<T> source, string? sortBy, bool sortDescending = false) where T : class
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return source;

            var entityType = typeof(T);
            var param = Expression.Parameter(entityType, "x");

            var propInfo = entityType
                .GetProperty(sortBy,
                             BindingFlags.IgnoreCase
                           | BindingFlags.Public
                           | BindingFlags.Instance);

            if (propInfo == null)
                return source;

            var propertyAccess = Expression.Property(param, propInfo);
            var lambda = Expression.Lambda(propertyAccess, param);

            var methodName = sortDescending ? "OrderByDescending" : "OrderBy";

            var methodCall = Expression.Call(
                typeof(Queryable),
                methodName,
                new[] { entityType, propInfo.PropertyType },
                source.Expression,
                lambda
            );

            return source.Provider.CreateQuery<T>(methodCall);
        }

        public static IQueryable<T> ApplyMultipleSorting<T>( this IQueryable<T> source,List<(string PropertyName, bool Descending)> sortColumns) where T : class
        {
            if (sortColumns == null || sortColumns.Count == 0)
                return source;

            var entityType = typeof(T);
            IOrderedQueryable<T>? orderedQuery = null;

            for (int i = 0; i < sortColumns.Count; i++)
            {
                var (propertyName, descending) = sortColumns[i];
                var param = Expression.Parameter(entityType, "x");

                var propInfo = entityType
                    .GetProperty(propertyName,
                                 BindingFlags.IgnoreCase
                               | BindingFlags.Public
                               | BindingFlags.Instance);

                if (propInfo == null)
                    continue; // Skip invalid properties

                var propertyAccess = Expression.Property(param, propInfo);
                var lambda = Expression.Lambda(propertyAccess, param);

                string methodName;
                if (i == 0) // First sort
                {
                    methodName = descending ? "OrderByDescending" : "OrderBy";
                }
                else // Subsequent sorts
                {
                    methodName = descending ? "ThenByDescending" : "ThenBy";
                }

                var methodCall = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new[] { entityType, propInfo.PropertyType },
                    (orderedQuery?.Expression ?? source.Expression),
                    lambda
                );

                orderedQuery = (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(methodCall);
            }

            return orderedQuery ?? source;
        }

        public static async Task<PagedResult<T>> ToPagedDictionaryAsync<T>( this IQueryable<T> source, Func<T, string> keySelector, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var total = await source.CountAsync(ct);

            var list = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var dict = list.ToDictionary(keySelector);

            return new PagedResult<T>(dict, total, pageNumber, pageSize);
        }

        public static async Task<PagedResult<T>> ToPagedListAsync<T>(this IQueryable<T> source, Func<T, string> keySelector, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            var total = await source.CountAsync(ct);

            var list = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var dict = list.ToDictionary(keySelector);

            return new PagedResult<T>(dict, total, pageNumber, pageSize);
        }
    }
}
