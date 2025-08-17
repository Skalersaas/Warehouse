using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Utilities.DataManipulation;

public static class QueryMaster<T>
{
    private static readonly Type Type = typeof(T);
    private static readonly MethodInfo ToStringMethod = typeof(object).GetMethod(nameof(ToString))!;
    private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;

    // Cache for properties and compiled expressions
    private static readonly ConcurrentDictionary<string, PropertyInfo> PropertyCache = new();
    private static readonly ConcurrentDictionary<string, Func<IQueryable<T>, IQueryable<T>>> OrderExpressionCache = new();

    /// <summary>
    /// Gets a cached property info for a given field name, ignoring case.
    /// </summary>
    public static PropertyInfo GetProperty(string fieldName) =>
        PropertyCache.GetOrAdd(fieldName.ToLowerInvariant(),
            _ => Type.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                ?? throw new ArgumentException($"Property '{fieldName}' not found on type '{Type.Name}'."));

    /// <summary>
    /// Orders an IQueryable collection by a specified field with caching.
    /// </summary>
    public static IQueryable<T> OrderByField(IQueryable<T> source, string? fieldName, bool descending = true)
    {
        fieldName = string.IsNullOrWhiteSpace(fieldName) ? "id" : fieldName;
        var cacheKey = $"{fieldName.ToLowerInvariant()}_{descending}";

        var orderFunc = OrderExpressionCache.GetOrAdd(cacheKey, _ => CreateOrderExpression(fieldName, descending));
        return orderFunc(source);
    }

    /// <summary>
    /// Filters an IQueryable collection based on field-value pairs with optimized expression building.
    /// </summary>
    public static IQueryable<T> FilterByFields(IQueryable<T> source, Dictionary<string, string>? filters)
    {
        if (filters?.Count > 0)
        {
            var expression = BuildFilterExpression(filters);
            if (expression != null)
                source = source.Where(expression);
        }
        return source;
    }

    /// <summary>
    /// Filters by date range with optimized expression building.
    /// </summary>
    public static IQueryable<T> FilterByDateRange(IQueryable<T> source, string fieldName, DateTime? dateFrom, DateTime? dateTo)
    {
        if (string.IsNullOrWhiteSpace(fieldName) ||
            (!IsValidDate(dateFrom) && !IsValidDate(dateTo)))
            return source;

        var expression = BuildDateRangeExpression(fieldName, dateFrom, dateTo);
        return expression != null ? source.Where(expression) : source;
    }

    /// <summary>
    /// Combines field and date filtering in a single operation.
    /// </summary>
    public static IQueryable<T> FilterByFieldsAndDate(IQueryable<T> source,
        Dictionary<string, string>? filters,
        string? dateField,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        var expressions = new List<Expression<Func<T, bool>>>();

        // Build field filter expression
        var fieldExpression = BuildFilterExpression(filters);
        if (fieldExpression != null)
            expressions.Add(fieldExpression);

        // Build date range expression
        if (!string.IsNullOrWhiteSpace(dateField))
        {
            var dateExpression = BuildDateRangeExpression(dateField, dateFrom, dateTo);
            if (dateExpression != null)
                expressions.Add(dateExpression);
        }

        // Combine all expressions
        return expressions.Count switch
        {
            0 => source,
            1 => source.Where(expressions[0]),
            _ => source.Where(CombineExpressions(expressions))
        };
    }

    #region Private Methods

    private static Func<IQueryable<T>, IQueryable<T>> CreateOrderExpression(string fieldName, bool descending)
    {
        var parameter = Expression.Parameter(Type, "x");
        var property = Expression.Property(parameter, GetProperty(fieldName));
        var lambda = Expression.Lambda(property, parameter);

        var methodName = descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(Type, property.Type);

        return source => (IQueryable<T>)method.Invoke(null, [source, lambda])!;
    }

    private static Expression<Func<T, bool>>? BuildFilterExpression(Dictionary<string, string>? filters)
    {
        if (filters?.Count == 0) return null;

        var parameter = Expression.Parameter(Type, "x");
        var conditions = new List<Expression>();

        foreach (var (fieldName, value) in filters!)
        {
            if (!TryGetProperty(fieldName, out var property)) continue;

            var condition = CreateFieldCondition(parameter, property, value);
            if (condition != null)
                conditions.Add(condition);
        }

        return conditions.Count == 0 ? null :
            Expression.Lambda<Func<T, bool>>(CombineWithAnd(conditions), parameter);
    }

    private static Expression<Func<T, bool>>? BuildDateRangeExpression(string fieldName, DateTime? dateFrom, DateTime? dateTo)
    {
        if (!TryGetProperty(fieldName, out var property) || !IsDateTimeProperty(property))
            return null;

        var parameter = Expression.Parameter(Type, "x");
        var propertyExpression = Expression.Property(parameter, property);
        var conditions = new List<Expression>();

        var effectiveDateFrom = IsValidDate(dateFrom) ? dateFrom : null;
        var effectiveDateTo = IsValidDate(dateTo) ? dateTo : null;

        if (effectiveDateFrom.HasValue)
            conditions.Add(CreateDateCondition(propertyExpression, property.PropertyType, effectiveDateFrom.Value, true));

        if (effectiveDateTo.HasValue)
            conditions.Add(CreateDateCondition(propertyExpression, property.PropertyType, effectiveDateTo.Value, false));

        return conditions.Count == 0 ? null :
            Expression.Lambda<Func<T, bool>>(CombineWithAnd(conditions), parameter);
    }

    private static Expression? CreateFieldCondition(ParameterExpression parameter, PropertyInfo property, string value)
    {
        // Split comma-separated values and trim whitespace
        var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(v => v.Trim())
                          .Where(v => !string.IsNullOrEmpty(v))
                          .ToList();

        if (values.Count == 0) return null;

        Expression propertyExpression = Expression.Property(parameter, property);

        // Handle boolean types
        if (IsBooleanProperty(property))
        {
            var boolConditions = new List<Expression>();

            foreach (var val in values)
            {
                if (bool.TryParse(val, out var boolValue))
                {
                    boolConditions.Add(Expression.Equal(propertyExpression, Expression.Constant(boolValue, property.PropertyType)));
                }
            }

            return boolConditions.Count == 0 ? null : CombineWithOr(boolConditions);
        }

        // Handle string contains for other types
        var stringPropertyExpression = property.PropertyType != typeof(string)
            ? Expression.Call(propertyExpression, ToStringMethod)
            : propertyExpression;

        // Create OR conditions for each value
        var orConditions = values.Select(val =>
            Expression.Call(stringPropertyExpression, ContainsMethod, Expression.Constant(val))
        ).Cast<Expression>().ToList();

        return CombineWithOr(orConditions);
    }

    private static BinaryExpression CreateDateCondition(Expression propertyExpression, Type propertyType, DateTime date, bool isFrom)
    {
        var dateConstant = Expression.Constant(date, typeof(DateTime));
        var isNullable = propertyType == typeof(DateTime?);

        if (isNullable)
        {
            var hasValue = Expression.Property(propertyExpression, "HasValue");
            var value = Expression.Property(propertyExpression, "Value");
            var comparison = isFrom
                ? Expression.GreaterThanOrEqual(value, dateConstant)
                : Expression.LessThanOrEqual(value, dateConstant);
            return Expression.AndAlso(hasValue, comparison);
        }

        return isFrom
            ? Expression.GreaterThanOrEqual(propertyExpression, dateConstant)
            : Expression.LessThanOrEqual(propertyExpression, dateConstant);
    }

    private static Expression CombineWithAnd(List<Expression> conditions) =>
        conditions.Aggregate(Expression.AndAlso);

    private static Expression CombineWithOr(List<Expression> conditions) =>
        conditions.Count == 1 ? conditions[0] : conditions.Aggregate(Expression.OrElse);

    private static Expression<Func<T, bool>> CombineExpressions(List<Expression<Func<T, bool>>> expressions)
    {
        var parameter = Expression.Parameter(Type, "x");

        // Replace parameters in each expression with our common parameter
        var replacedExpressions = expressions.Select(expr =>
        {
            var replacer = new ParameterReplacer(expr.Parameters[0], parameter);
            return replacer.Visit(expr.Body);
        }).ToList();

        var combined = replacedExpressions.Aggregate((left, right) => Expression.AndAlso(left, right));
        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    // Helper class to replace parameters in expressions
    private class ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter) : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter = oldParameter;
        private readonly ParameterExpression _newParameter = newParameter;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }

    private static bool TryGetProperty(string fieldName, out PropertyInfo property)
    {
        try
        {
            property = GetProperty(fieldName);
            return true;
        }
        catch
        {
            property = null!;
            return false;
        }
    }

    private static bool IsValidDate(DateTime? date) =>
        date.HasValue && date.Value != DateTime.MinValue;

    private static bool IsBooleanProperty(PropertyInfo property) =>
        property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?);

    private static bool IsDateTimeProperty(PropertyInfo property) =>
        property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?);

    #endregion
}