using System.Linq.Expressions;
using System.Reflection;

namespace Utilities.DataManipulation;
public static class QueryMaster<T>
{
    private static readonly Type Type = typeof(T);
    private static readonly Type StringType = typeof(string);
    private static readonly Type DateTimeType = typeof(DateTime);
    private static readonly Type NullableDateTimeType = typeof(DateTime?);
    private static readonly MethodInfo ToStringMethod = typeof(object).GetMethod(nameof(ToString))!;
    private static readonly MethodInfo ContainsMethod = StringType.GetMethod(nameof(string.Contains), [StringType])!;

    /// <summary>
    /// Gets a property info for a given field name, ignoring case.
    /// </summary>
    /// <param name="fieldName">The name of the property to find.</param>
    /// <returns>The PropertyInfo for the specified field.</returns>
    /// <exception cref="ArgumentException">Thrown when the property is not found on the type.</exception>
    public static PropertyInfo GetProperty(string fieldName) =>
        Type.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
        ?? throw new ArgumentException($"Property '{fieldName}' not found on type '{Type.Name}'.");

    /// <summary>
    /// Orders an IQueryable collection by a specified field.
    /// </summary>
    /// <param name="source">The source IQueryable collection.</param>
    /// <param name="fieldName">The name of the field to order by.</param>
    /// <param name="ascending">Whether to sort in ascending order (true) or descending order (false).</param>
    /// <returns>An ordered IQueryable collection.</returns>
    /// <remarks>
    /// Returns the source collection unchanged if fieldName is null or empty.
    /// </remarks>
    public static IQueryable<T> OrderByField(IQueryable<T> source, string fieldName, bool ascending)
    {
        if (string.IsNullOrWhiteSpace(fieldName)) return source;

        var parameter = Expression.Parameter(Type, "x");
        var property = Expression.Property(parameter, GetProperty(fieldName));
        var lambda = Expression.Lambda(property, parameter);

        string methodName = ascending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);

        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(Type, property.Type);

        return (IQueryable<T>)method.Invoke(null, new object[] { source, lambda })!;
    }

    /// <summary>
    /// Filters an IQueryable collection based on a dictionary of field-value pairs.
    /// </summary>
    /// <param name="source">The source IQueryable collection.</param>
    /// <param name="filters">Dictionary of field names and their corresponding filter values.</param>
    /// <returns>A filtered IQueryable collection.</returns>
    /// <remarks>
    /// Combines all filters using AND logic, with case-insensitive string matching and automatic type conversion.
    /// </remarks>
    public static IQueryable<T> FilterByFields(IQueryable<T> source, Dictionary<string, string>? filters)
    {
        if (filters is null || filters.Count == 0) return source;

        var parameter = Expression.Parameter(Type, "x");
        Expression? combinedExpression = null;

        foreach (var (fieldName, value) in filters)
        {
            var property = Type.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null) continue;

            Expression propertyExpression = Expression.Property(parameter, property);

            if (property.PropertyType != StringType)
            {
                propertyExpression = Expression.Call(propertyExpression, ToStringMethod);
            }

            var constant = Expression.Constant(value);
            var condition = Expression.Call(propertyExpression, ContainsMethod, constant);

            combinedExpression = combinedExpression == null
                ? condition
                : Expression.AndAlso(combinedExpression, condition);
        }

        if (combinedExpression == null) return source;

        var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        return source.Where(lambda);
    }

    /// <summary>
    /// Filters an IQueryable collection by a date range on a specified field.
    /// </summary>
    /// <param name="source">The source IQueryable collection.</param>
    /// <param name="fieldName">The name of the date field to filter by.</param>
    /// <param name="dateFrom">The start date (inclusive). If null or DateTime.Min, no lower bound is applied.</param>
    /// <param name="dateTo">The end date (inclusive). If null or DateTime.Min, no upper bound is applied.</param>
    /// <returns>A filtered IQueryable collection.</returns>
    /// <remarks>
    /// Returns the source collection unchanged if both dateFrom and dateTo are null or DateTime.Min.
    /// Supports both DateTime and DateTime? properties.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when the specified field is not found or is not a date type.</exception>
    public static IQueryable<T> FilterByDateRange(IQueryable<T> source, string fieldName, DateTime? dateFrom, DateTime? dateTo)
    {
        // Treat DateTime.Min as null (no filter)
        var effectiveDateFrom = dateFrom.HasValue && dateFrom.Value != DateTime.MinValue ? dateFrom : null;
        var effectiveDateTo = dateTo.HasValue && dateTo.Value != DateTime.MinValue ? dateTo : null;

        if (!effectiveDateFrom.HasValue && !effectiveDateTo.HasValue) return source;
        if (string.IsNullOrWhiteSpace(fieldName)) return source;

        var property = Type.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
            throw new ArgumentException($"Property '{fieldName}' not found on type '{Type.Name}'.");

        if (property.PropertyType != DateTimeType && property.PropertyType != NullableDateTimeType)
            throw new ArgumentException($"Property '{fieldName}' is not a DateTime or DateTime? type.");

        var parameter = Expression.Parameter(Type, "x");
        var propertyExpression = Expression.Property(parameter, property);
        Expression? combinedExpression = null;

        // Handle DateFrom (greater than or equal)
        if (effectiveDateFrom.HasValue)
        {
            var fromConstant = Expression.Constant(effectiveDateFrom.Value, DateTimeType);
            Expression fromCondition;

            if (property.PropertyType == NullableDateTimeType)
            {
                // For nullable DateTime: x.Date.HasValue && x.Date.Value >= dateFrom
                var hasValueProperty = Expression.Property(propertyExpression, "HasValue");
                var valueProperty = Expression.Property(propertyExpression, "Value");
                var dateComparison = Expression.GreaterThanOrEqual(valueProperty, fromConstant);
                fromCondition = Expression.AndAlso(hasValueProperty, dateComparison);
            }
            else
            {
                // For non-nullable DateTime: x.Date >= dateFrom
                fromCondition = Expression.GreaterThanOrEqual(propertyExpression, fromConstant);
            }

            combinedExpression = fromCondition;
        }

        // Handle DateTo (less than or equal)
        if (effectiveDateTo.HasValue)
        {
            var toConstant = Expression.Constant(effectiveDateTo.Value, DateTimeType);
            Expression toCondition;

            if (property.PropertyType == NullableDateTimeType)
            {
                // For nullable DateTime: x.Date.HasValue && x.Date.Value <= dateTo
                var hasValueProperty = Expression.Property(propertyExpression, "HasValue");
                var valueProperty = Expression.Property(propertyExpression, "Value");
                var dateComparison = Expression.LessThanOrEqual(valueProperty, toConstant);
                toCondition = Expression.AndAlso(hasValueProperty, dateComparison);
            }
            else
            {
                // For non-nullable DateTime: x.Date <= dateTo
                toCondition = Expression.LessThanOrEqual(propertyExpression, toConstant);
            }

            combinedExpression = combinedExpression == null
                ? toCondition
                : Expression.AndAlso(combinedExpression, toCondition);
        }

        if (combinedExpression == null) return source;

        var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        return source.Where(lambda);
    }

    /// <summary>
    /// Applies both field filters and date range filter to an IQueryable collection.
    /// </summary>
    /// <param name="source">The source IQueryable collection.</param>
    /// <param name="filters">Dictionary of field names and their corresponding filter values.</param>
    /// <param name="dateField">The name of the date field to filter by.</param>
    /// <param name="dateFrom">The start date (inclusive). If null, no lower bound is applied.</param>
    /// <param name="dateTo">The end date (inclusive). If null, no upper bound is applied.</param>
    /// <returns>A filtered IQueryable collection.</returns>
    /// <remarks>
    /// Combines field filters and date range filter using AND logic.
    /// </remarks>
    public static IQueryable<T> FilterByFieldsAndDate(IQueryable<T> source,
        Dictionary<string, string>? filters,
        string? dateField,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        // Apply field filters first
        source = FilterByFields(source, filters);

        // Apply date range filter if date field is specified
        if (!string.IsNullOrWhiteSpace(dateField))
        {
            source = FilterByDateRange(source, dateField, dateFrom, dateTo);
        }

        return source;
    }
}