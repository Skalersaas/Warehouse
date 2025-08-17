using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Utilities.DataManipulation;

/// <summary>
/// High-performance universal query builder for LINQ with support for:
/// - Nested object filtering (User.Profile.Name)
/// - Collection filtering (Orders.Status)
/// - Date range filtering (CreatedDate.from/to)
/// - Multiple value filtering (Status => "Active,Pending")
/// - Type-safe expression compilation with caching
/// </summary>
public static class QueryMaster<T>
{
    #region Constants and Static Fields

    private static readonly Type EntityType = typeof(T);
    private static readonly MethodInfo StringContainsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
    private static readonly MethodInfo ObjectToStringMethod = typeof(object).GetMethod(nameof(ToString))!;
    private static readonly MethodInfo EnumerableAnyMethod = typeof(Enumerable).GetMethods()
        .First(m => m.Name == "Any" && m.GetParameters().Length == 2);

    // Thread-safe caches for performance
    private static readonly ConcurrentDictionary<string, PropertyPathInfo> PropertyPathCache = new();
    private static readonly ConcurrentDictionary<string, Func<IQueryable<T>, IQueryable<T>>> OrderExpressionCache = new();
    private static readonly ConcurrentDictionary<string, Expression<Func<T, bool>>> FilterExpressionCache = new();

    // Supported date formats for parsing (UTC-optimized)
    private static readonly string[] SupportedDateFormats =
    [
        "yyyy-MM-dd",                    // 2024-08-18 (treated as UTC)
        "yyyy-MM-ddTHH:mm:ss",          // 2024-08-18T14:30:00 (treated as UTC)
        "yyyy-MM-ddTHH:mm:ssZ",         // 2024-08-18T14:30:00Z (explicit UTC)
        "yyyy-MM-ddTHH:mm:ss.fffZ",     // 2024-08-18T14:30:00.123Z
        "yyyy-MM-ddTHH:mm:ss.fffffffZ", // 2024-08-18T14:30:00.1234567Z
        "yyyy-MM-dd HH:mm:ss",          // 2024-08-18 14:30:00 (treated as UTC)
        "dd.MM.yyyy",                   // 18.08.2024 (treated as UTC)
        "dd/MM/yyyy"                    // 18/08/2024 (treated as UTC)
    ];

    #endregion

    #region Public API

    /// <summary>
    /// Applies universal filtering to IQueryable with automatic type detection
    /// </summary>
    public static IQueryable<T> ApplyFilters(IQueryable<T> source, Dictionary<string, string>? filters)
    {
        if (filters == null || filters.Count == 0)
            return source;

        var filterGroups = GroupFilters(filters);
        var expressions = new List<Expression<Func<T, bool>>>();

        // Process regular field filters
        foreach (var (fieldPath, values) in filterGroups.FieldFilters)
        {
            var expression = CreateFieldFilterExpression(fieldPath, values);
            if (expression != null)
                expressions.Add(expression);
        }

        // Process date range filters
        foreach (var (fieldPath, dateRange) in filterGroups.DateRangeFilters)
        {
            var expression = CreateDateRangeExpression(fieldPath, dateRange.From, dateRange.To);
            if (expression != null)
                expressions.Add(expression);
        }

        return expressions.Count == 0 ? source : source.Where(CombineExpressionsWithAnd(expressions));
    }

    /// <summary>
    /// Applies ordering with support for nested properties and caching
    /// </summary>
    public static IQueryable<T> ApplyOrdering(IQueryable<T> source, string? fieldPath, bool descending = true)
    {
        if (string.IsNullOrWhiteSpace(fieldPath))
            fieldPath = "Id";

        var cacheKey = $"{fieldPath.ToLowerInvariant()}_{descending}";
        var orderFunc = OrderExpressionCache.GetOrAdd(cacheKey, _ => CreateOrderingFunction(fieldPath, descending));

        return orderFunc(source);
    }

    #endregion

    #region Core Data Structures

    private record PropertyPathInfo(
        PropertyInfo[] Properties,
        Type FinalType,
        bool IsCollection,
        int CollectionIndex = -1);

    private record DateRange(DateTime? From, DateTime? To);

    private record FilterGroups(
        Dictionary<string, List<string>> FieldFilters,
        Dictionary<string, DateRange> DateRangeFilters);

    #endregion

    #region Filter Processing

    private static FilterGroups GroupFilters(Dictionary<string, string> filters)
    {
        var fieldFilters = new Dictionary<string, List<string>>();
        var dateRangeFilters = new Dictionary<string, DateRange>();
        var processedDateFields = new HashSet<string>();

        foreach (var (key, value) in filters)
        {
            if (TryParseDateRangeKey(key, out var fieldPath, out var isFrom))
            {
                if (processedDateFields.Add(fieldPath)) // Add returns true if not already present
                {
                    var fromValue = TryParseDate(filters.GetValueOrDefault($"{fieldPath}.from"));
                    var toValue = TryParseDate(filters.GetValueOrDefault($"{fieldPath}.to"));

                    if (fromValue.HasValue || toValue.HasValue)
                    {
                        dateRangeFilters[fieldPath] = new DateRange(fromValue, toValue);
                    }
                }
            }
            else if (!IsDateRangeRelatedKey(key, processedDateFields))
            {
                var values = ParseMultipleValues(value);
                if (values.Count > 0)
                {
                    fieldFilters[key] = values;
                }
            }
        }

        return new FilterGroups(fieldFilters, dateRangeFilters);
    }

    private static bool TryParseDateRangeKey(string key, out string fieldPath, out bool isFrom)
    {
        if (key.EndsWith(".from", StringComparison.OrdinalIgnoreCase))
        {
            fieldPath = key[..^5]; // Remove ".from"
            isFrom = true;
            return true;
        }

        if (key.EndsWith(".to", StringComparison.OrdinalIgnoreCase))
        {
            fieldPath = key[..^3]; // Remove ".to"
            isFrom = false;
            return true;
        }

        fieldPath = string.Empty;
        isFrom = false;
        return false;
    }

    private static bool IsDateRangeRelatedKey(string key, HashSet<string> processedDateFields)
    {
        return processedDateFields.Any(field =>
            key.StartsWith($"{field}.", StringComparison.OrdinalIgnoreCase));
    }

    private static List<string> ParseMultipleValues(string value)
    {
        return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(v => v.Trim())
                   .Where(v => !string.IsNullOrEmpty(v))
                   .ToList();
    }

    #endregion

    #region Expression Building

    private static Expression<Func<T, bool>>? CreateFieldFilterExpression(string fieldPath, List<string> values)
    {
        var cacheKey = $"field_{fieldPath}_{string.Join("|", values)}";
        return FilterExpressionCache.GetOrAdd(cacheKey, _ => BuildFieldFilterExpression(fieldPath, values));
    }

    private static Expression<Func<T, bool>>? BuildFieldFilterExpression(string fieldPath, List<string> values)
    {
        if (!TryGetPropertyPath(fieldPath, out var pathInfo))
            return null;

        var parameter = Expression.Parameter(EntityType, "x");
        var condition = pathInfo.IsCollection
            ? CreateCollectionCondition(parameter, pathInfo, values)
            : CreateSimplePropertyCondition(parameter, pathInfo, values);

        return condition != null ? Expression.Lambda<Func<T, bool>>(condition, parameter) : null;
    }

    private static Expression<Func<T, bool>>? CreateDateRangeExpression(string fieldPath, DateTime? from, DateTime? to)
    {
        if (!TryGetPropertyPath(fieldPath, out var pathInfo) || !IsDateTimeType(pathInfo.FinalType))
            return null;

        var parameter = Expression.Parameter(EntityType, "x");
        var condition = pathInfo.IsCollection
            ? CreateCollectionDateCondition(parameter, pathInfo, from, to)
            : CreateSimpleDateCondition(parameter, pathInfo, from, to);

        return condition != null ? Expression.Lambda<Func<T, bool>>(condition, parameter) : null;
    }

    private static Expression? CreateSimplePropertyCondition(ParameterExpression parameter, PropertyPathInfo pathInfo, List<string> values)
    {
        var propertyExpression = BuildPropertyAccessExpression(parameter, pathInfo.Properties);
        var finalType = pathInfo.FinalType;

        return finalType switch
        {
            _ when IsBooleanType(finalType) => CreateBooleanConditions(propertyExpression, finalType, values),
            _ when IsDateTimeType(finalType) => CreateDateConditions(propertyExpression, finalType, values),
            _ when IsNumericType(finalType) => CreateNumericConditions(propertyExpression, finalType, values),
            _ => CreateStringContainsConditions(propertyExpression, finalType, values)
        };
    }

    private static Expression? CreateCollectionCondition(ParameterExpression parameter, PropertyPathInfo pathInfo, List<string> values)
    {
        var collectionExpression = BuildPropertyAccessExpression(parameter, pathInfo.Properties[..pathInfo.CollectionIndex]);
        var collectionProperty = pathInfo.Properties[pathInfo.CollectionIndex - 1];
        var targetProperty = pathInfo.Properties[^1];

        var elementType = GetCollectionElementType(collectionProperty.PropertyType);
        if (elementType == null) return null;

        var itemParameter = Expression.Parameter(elementType, "item");
        var itemPropertyExpression = BuildPropertyAccessExpression(itemParameter, pathInfo.Properties[pathInfo.CollectionIndex..]);

        var conditions = new List<Expression>();
        var targetType = targetProperty.PropertyType;

        foreach (var value in values)
        {
            var condition = targetType switch
            {
                _ when IsBooleanType(targetType) => CreateSingleBooleanCondition(itemPropertyExpression, targetType, value),
                _ when IsDateTimeType(targetType) => CreateSingleDateCondition(itemPropertyExpression, targetType, value),
                _ when IsNumericType(targetType) => CreateSingleNumericCondition(itemPropertyExpression, targetType, value),
                _ => CreateSingleStringCondition(itemPropertyExpression, targetType, value)
            };

            if (condition != null)
                conditions.Add(condition);
        }

        if (conditions.Count == 0) return null;

        var combinedCondition = CombineExpressionsWithOr(conditions);
        var lambda = Expression.Lambda(combinedCondition, itemParameter);
        var anyMethod = EnumerableAnyMethod.MakeGenericMethod(elementType);

        return Expression.Call(anyMethod, collectionExpression, lambda);
    }

    #endregion

    #region Type-Specific Condition Creators

    private static Expression? CreateBooleanConditions(Expression propertyExpression, Type propertyType, List<string> values)
    {
        var conditions = new List<Expression>();

        foreach (var value in values)
        {
            var condition = CreateSingleBooleanCondition(propertyExpression, propertyType, value);
            if (condition != null)
                conditions.Add(condition);
        }

        return conditions.Count == 0 ? null : CombineExpressionsWithOr(conditions);
    }

    private static Expression? CreateSingleBooleanCondition(Expression propertyExpression, Type propertyType, string value)
    {
        if (!bool.TryParse(value, out var boolValue))
            return null;

        var constant = Expression.Constant(boolValue, propertyType);
        return Expression.Equal(propertyExpression, constant);
    }

    private static Expression? CreateDateConditions(Expression propertyExpression, Type propertyType, List<string> values)
    {
        var conditions = new List<Expression>();

        foreach (var value in values)
        {
            var condition = CreateSingleDateCondition(propertyExpression, propertyType, value);
            if (condition != null)
                conditions.Add(condition);
        }

        return conditions.Count == 0 ? null : CombineExpressionsWithOr(conditions);
    }

    private static Expression? CreateSingleDateCondition(Expression propertyExpression, Type propertyType, string value)
    {
        var date = TryParseDate(value);
        if (!date.HasValue) return null;

        return CreateDateEqualityCondition(propertyExpression, propertyType, date.Value);
    }

    private static Expression? CreateNumericConditions(Expression propertyExpression, Type propertyType, List<string> values)
    {
        var conditions = new List<Expression>();

        foreach (var value in values)
        {
            var condition = CreateSingleNumericCondition(propertyExpression, propertyType, value);
            if (condition != null)
                conditions.Add(condition);
        }

        return conditions.Count == 0 ? null : CombineExpressionsWithOr(conditions);
    }

    private static Expression? CreateSingleNumericCondition(Expression propertyExpression, Type propertyType, string value)
    {
        // For numeric types, convert to string and use contains for partial matching
        var stringExpression = Expression.Call(propertyExpression, ObjectToStringMethod);
        var constant = Expression.Constant(value);
        return Expression.Call(stringExpression, StringContainsMethod, constant);
    }

    private static Expression? CreateStringContainsConditions(Expression propertyExpression, Type propertyType, List<string> values)
    {
        var conditions = values.Select(value => CreateSingleStringCondition(propertyExpression, propertyType, value))
                              .Where(c => c != null)
                              .ToList();

        return conditions.Count == 0 ? null : CombineExpressionsWithOr(conditions!);
    }

    private static Expression? CreateSingleStringCondition(Expression propertyExpression, Type propertyType, string value)
    {
        var stringExpression = propertyType == typeof(string)
            ? propertyExpression
            : Expression.Call(propertyExpression, ObjectToStringMethod);

        var constant = Expression.Constant(value);
        return Expression.Call(stringExpression, StringContainsMethod, constant);
    }

    #endregion

    #region Date Range Handling

    private static Expression? CreateSimpleDateCondition(ParameterExpression parameter, PropertyPathInfo pathInfo, DateTime? from, DateTime? to)
    {
        var propertyExpression = BuildPropertyAccessExpression(parameter, pathInfo.Properties);
        var conditions = new List<Expression>();

        if (from.HasValue)
            conditions.Add(CreateDateComparisonCondition(propertyExpression, pathInfo.FinalType, from.Value, true));

        if (to.HasValue)
            conditions.Add(CreateDateComparisonCondition(propertyExpression, pathInfo.FinalType, to.Value, false));

        return conditions.Count == 0 ? null : CombineExpressionsWithAnd(conditions);
    }

    private static Expression? CreateCollectionDateCondition(ParameterExpression parameter, PropertyPathInfo pathInfo, DateTime? from, DateTime? to)
    {
        var collectionExpression = BuildPropertyAccessExpression(parameter, pathInfo.Properties[..pathInfo.CollectionIndex]);
        var collectionProperty = pathInfo.Properties[pathInfo.CollectionIndex - 1];
        var targetProperty = pathInfo.Properties[^1];

        var elementType = GetCollectionElementType(collectionProperty.PropertyType);
        if (elementType == null) return null;

        var itemParameter = Expression.Parameter(elementType, "item");
        var itemPropertyExpression = BuildPropertyAccessExpression(itemParameter, pathInfo.Properties[pathInfo.CollectionIndex..]);
        var conditions = new List<Expression>();

        if (from.HasValue)
            conditions.Add(CreateDateComparisonCondition(itemPropertyExpression, targetProperty.PropertyType, from.Value, true));

        if (to.HasValue)
            conditions.Add(CreateDateComparisonCondition(itemPropertyExpression, targetProperty.PropertyType, to.Value, false));

        if (conditions.Count == 0) return null;

        var combinedCondition = CombineExpressionsWithAnd(conditions);
        var lambda = Expression.Lambda(combinedCondition, itemParameter);
        var anyMethod = EnumerableAnyMethod.MakeGenericMethod(elementType);

        return Expression.Call(anyMethod, collectionExpression, lambda);
    }

    private static Expression CreateDateEqualityCondition(Expression propertyExpression, Type propertyType, DateTime date)
    {
        // Ensure date is UTC for database compatibility
        var utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        var dateConstant = Expression.Constant(utcDate, typeof(DateTime));

        if (IsNullableType(propertyType))
        {
            var hasValue = Expression.Property(propertyExpression, "HasValue");
            var value = Expression.Property(propertyExpression, "Value");
            var dateComparison = Expression.Equal(
                Expression.Property(value, "Date"),
                Expression.Property(dateConstant, "Date"));
            return Expression.AndAlso(hasValue, dateComparison);
        }

        return Expression.Equal(
            Expression.Property(propertyExpression, "Date"),
            Expression.Property(dateConstant, "Date"));
    }

    private static Expression CreateDateComparisonCondition(Expression propertyExpression, Type propertyType, DateTime date, bool isGreaterThanOrEqual)
    {
        // Ensure date is UTC for database compatibility
        var utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        var dateConstant = Expression.Constant(utcDate, typeof(DateTime));

        if (IsNullableType(propertyType))
        {
            var hasValue = Expression.Property(propertyExpression, "HasValue");
            var value = Expression.Property(propertyExpression, "Value");
            var comparison = isGreaterThanOrEqual
                ? Expression.GreaterThanOrEqual(value, dateConstant)
                : Expression.LessThanOrEqual(value, dateConstant);
            return Expression.AndAlso(hasValue, comparison);
        }

        return isGreaterThanOrEqual
            ? Expression.GreaterThanOrEqual(propertyExpression, dateConstant)
            : Expression.LessThanOrEqual(propertyExpression, dateConstant);
    }

    #endregion

    #region Property Path Resolution

    private static bool TryGetPropertyPath(string fieldPath, out PropertyPathInfo pathInfo)
    {
        try
        {
            pathInfo = PropertyPathCache.GetOrAdd(fieldPath.ToLowerInvariant(), _ => ResolvePropertyPath(fieldPath));
            return true;
        }
        catch
        {
            pathInfo = null!;
            return false;
        }
    }

    private static PropertyPathInfo ResolvePropertyPath(string fieldPath)
    {
        var parts = fieldPath.Split('.');
        var properties = new List<PropertyInfo>();
        var currentType = EntityType;
        var collectionIndex = -1;

        for (int i = 0; i < parts.Length; i++)
        {
            var property = FindProperty(currentType, parts[i]);
            if (property == null)
                throw new ArgumentException($"Property '{parts[i]}' not found on type '{currentType.Name}' in path '{fieldPath}'");

            properties.Add(property);

            if (IsCollectionType(property.PropertyType) && property.PropertyType != typeof(string))
            {
                collectionIndex = i + 1; // Index after collection property
                currentType = GetCollectionElementType(property.PropertyType)
                    ?? throw new ArgumentException($"Cannot determine element type for collection '{parts[i]}'");
            }
            else
            {
                currentType = property.PropertyType;
            }
        }

        var finalType = properties[^1].PropertyType;
        var isCollection = collectionIndex != -1;

        return new PropertyPathInfo(properties.ToArray(), finalType, isCollection, collectionIndex);
    }

    private static PropertyInfo? FindProperty(Type type, string name)
    {
        return type.GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
    }

    private static Expression BuildPropertyAccessExpression(Expression parameter, PropertyInfo[] properties)
    {
        return properties.Aggregate(parameter, Expression.Property);
    }

    #endregion

    #region Ordering

    private static Func<IQueryable<T>, IQueryable<T>> CreateOrderingFunction(string fieldPath, bool descending)
    {
        if (!TryGetPropertyPath(fieldPath, out var pathInfo) || pathInfo.IsCollection)
            throw new InvalidOperationException($"Cannot order by '{fieldPath}'. Collections are not supported for ordering.");

        var parameter = Expression.Parameter(EntityType, "x");
        var propertyExpression = BuildPropertyAccessExpression(parameter, pathInfo.Properties);
        var lambda = Expression.Lambda(propertyExpression, parameter);

        var methodName = descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(EntityType, pathInfo.FinalType);

        return source => (IQueryable<T>)method.Invoke(null, [source, lambda])!;
    }

    #endregion

    #region Expression Combinators

    private static Expression<Func<T, bool>> CombineExpressionsWithAnd(List<Expression<Func<T, bool>>> expressions)
    {
        if (expressions.Count == 1)
            return expressions[0];

        var parameter = Expression.Parameter(EntityType, "x");
        var visitor = new ParameterReplacementVisitor(parameter);

        var bodies = expressions.Select(expr => visitor.Visit(expr.Body)).ToList();
        var combined = bodies.Aggregate(Expression.AndAlso);

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    private static Expression CombineExpressionsWithOr(List<Expression> expressions)
    {
        return expressions.Count == 1 ? expressions[0] : expressions.Aggregate(Expression.OrElse);
    }

    private static Expression CombineExpressionsWithAnd(List<Expression> expressions)
    {
        return expressions.Count == 1 ? expressions[0] : expressions.Aggregate(Expression.AndAlso);
    }

    private class ParameterReplacementVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _newParameter;

        public ParameterReplacementVisitor(ParameterExpression newParameter)
        {
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node.Type == _newParameter.Type ? _newParameter : base.VisitParameter(node);
        }
    }

    #endregion

    #region Utility Methods

    private static DateTime? TryParseDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        // Try exact formats first
        foreach (var format in SupportedDateFormats)
        {
            if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactDate))
            {
                // Ensure the DateTime is treated as UTC
                return DateTime.SpecifyKind(exactDate, DateTimeKind.Utc);
            }
        }

        // Fallback to general parsing
        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var generalDate))
        {
            // Ensure UTC kind
            return DateTime.SpecifyKind(generalDate, DateTimeKind.Utc);
        }

        return null;
    }

    private static bool IsCollectionType(Type type)
    {
        if (type == typeof(string)) return false;

        return type.IsArray ||
               (type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition())) ||
               typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }

    private static Type? GetCollectionElementType(Type collectionType)
    {
        if (collectionType.IsArray)
            return collectionType.GetElementType();

        if (collectionType.IsGenericType && collectionType.GetGenericArguments().Length == 1)
            return collectionType.GetGenericArguments()[0];

        var enumerableInterface = collectionType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableInterface?.GetGenericArguments()[0];
    }

    private static bool IsBooleanType(Type type) =>
        type == typeof(bool) || type == typeof(bool?);

    private static bool IsDateTimeType(Type type) =>
        type == typeof(DateTime) || type == typeof(DateTime?);

    private static bool IsNumericType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType == typeof(int) || underlyingType == typeof(long) ||
               underlyingType == typeof(short) || underlyingType == typeof(byte) ||
               underlyingType == typeof(uint) || underlyingType == typeof(ulong) ||
               underlyingType == typeof(ushort) || underlyingType == typeof(sbyte) ||
               underlyingType == typeof(decimal) || underlyingType == typeof(double) ||
               underlyingType == typeof(float);
    }

    private static bool IsNullableType(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

    #endregion
}