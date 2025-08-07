using System.Linq.Expressions;
using System.Reflection;

namespace Utilities.DataManipulation;
public static class QueryMaster<T>
{
    private static readonly Type Type = typeof(T);
    private static readonly Type StringType = typeof(string);
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

        return (IQueryable<T>)method.Invoke(null, [source, lambda])!;
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
}
