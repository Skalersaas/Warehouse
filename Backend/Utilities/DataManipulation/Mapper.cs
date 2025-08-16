using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Utilities.DataManipulation;

/// <summary>
/// Provides functionality for mapping between DTO and domain model objects with registration support.
/// </summary>
public static class Mapper
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();
    private static readonly ConcurrentDictionary<string, Func<object, object>> CompiledAccessors = new();

    // Registration storage
    private static readonly ConcurrentDictionary<string, object> RegisteredMappings = new();

    #region Registration Methods

    /// <summary>
    /// Registers a mapping configuration between two types.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="propertyMappings">Property mapping configuration</param>
    public static void RegisterMapping<TSource, TDestination>(
        Action<PropertyMappingBuilder<TSource, TDestination>> propertyMappings)
        where TDestination : new()
    {
        var key = GetMappingKey<TSource, TDestination>();
        var builder = new PropertyMappingBuilder<TSource, TDestination>();
        propertyMappings(builder);
        RegisteredMappings[key] = builder.GetMappings();
    }

    /// <summary>
    /// Registers a simple mapping that just uses property name matching (no custom mappings).
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    public static void RegisterMapping<TSource, TDestination>()
        where TDestination : new()
    {
        var key = GetMappingKey<TSource, TDestination>();
        RegisteredMappings[key] = new Dictionary<string, Func<object, object>>();
    }

    /// <summary>
    /// Checks if a mapping is registered for the given types.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <returns>True if mapping is registered</returns>
    public static bool IsMappingRegistered<TSource, TDestination>()
    {
        var key = GetMappingKey<TSource, TDestination>();
        return RegisteredMappings.ContainsKey(key);
    }

    /// <summary>
    /// Removes a registered mapping.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    public static void UnregisterMapping<TSource, TDestination>()
    {
        var key = GetMappingKey<TSource, TDestination>();
        RegisteredMappings.TryRemove(key, out _);
    }

    /// <summary>
    /// Clears all registered mappings.
    /// </summary>
    public static void ClearAllMappings()
    {
        RegisteredMappings.Clear();
    }

    private static string GetMappingKey<TSource, TDestination>()
    {
        return $"{typeof(TSource).FullName}->{typeof(TDestination).FullName}";
    }

    #endregion

    #region Mapping Methods with Registration Support

    /// <summary>
    /// Maps properties from a DTO object to a new instance of the destination type using lambda expressions.
    /// </summary>
    /// <typeparam name="TDestination">The type of object to create and map to.</typeparam>
    /// <typeparam name="TSource">The type of the source DTO object.</typeparam>
    /// <param name="dto">The source DTO object to map from.</param>
    /// <param name="propertyMappings">Optional lambda-based property mappings.</param>
    /// <returns>A new instance of TDestination with properties mapped from the source DTO.</returns>
    private static TDestination FromDTO<TDestination, TSource>(TSource dto,
        Action<PropertyMappingBuilder<TSource, TDestination>>? propertyMappings = null)
        where TDestination : new()
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var mappings = GetMappingsFromBuilder<TSource, TDestination>(propertyMappings);
        return (TDestination)MapObject(dto, typeof(TSource), typeof(TDestination), mappings);
    }

    /// <summary>
    /// Maps properties from a source object to an existing destination object using lambda expressions.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TDestination">The type of the destination object.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <param name="destination">The existing destination object to map to.</param>
    /// <param name="skipNullValues">If true, null values from source won't overwrite destination values.</param>
    /// <param name="propertyMappings">Optional lambda-based property mappings.</param>
    private static void MapToExisting<TSource, TDestination>(TSource source, TDestination destination,
        bool skipNullValues = false,
        Action<PropertyMappingBuilder<TSource, TDestination>>? propertyMappings = null)
        where TDestination : new()
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (destination == null) throw new ArgumentNullException(nameof(destination));

        var mappings = GetMappingsFromBuilder(propertyMappings);
        MapToExistingObject(source, destination, typeof(TSource), typeof(TDestination),
            skipNullValues, mappings);
    }

    /// <summary>
    /// Auto-maps using registered mappings if available, otherwise falls back to property name matching.
    /// </summary>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <param name="source">Source object</param>
    /// <returns>Mapped destination object</returns>
    public static TDestination AutoMap<TDestination, TSource>(TSource source)
        where TDestination : new()
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var key = GetMappingKey<TSource, TDestination>();
        if (RegisteredMappings.TryGetValue(key, out var mappingsObj))
        {
            // Use registered mapping
            var mappings = (Dictionary<string, Func<object, object>>)mappingsObj;
            return (TDestination)MapObject(source, typeof(TSource), typeof(TDestination), mappings);
        }
        else
        {
            // Fall back to automatic property name matching
            return FromDTO<TDestination, TSource>(source, null);
        }
    }

    /// <summary>
    /// Auto-maps to existing object using registered mappings if available, otherwise falls back to property name matching.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source object</param>
    /// <param name="destination">Destination object</param>
    /// <param name="skipNullValues">Skip null values</param>
    public static void AutoMapToExisting<TSource, TDestination>(TSource source, TDestination destination,
        bool skipNullValues = false)
        where TDestination : new()
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (destination == null) throw new ArgumentNullException(nameof(destination));

        var key = GetMappingKey<TSource, TDestination>();
        if (RegisteredMappings.TryGetValue(key, out var mappingsObj))
        {
            // Use registered mapping
            var mappings = (Dictionary<string, Func<object, object>>)mappingsObj;
            MapToExistingObject(source, destination, typeof(TSource), typeof(TDestination), skipNullValues, mappings);
        }
        else
        {
            // Fall back to automatic property name matching
            MapToExisting(source, destination, skipNullValues, null);
        }
    }

    private static Dictionary<string, Func<object, object>>? GetMappingsFromBuilder<TSource, TDestination>(
        Action<PropertyMappingBuilder<TSource, TDestination>>? propertyMappings)
        where TDestination : new()
    {
        if (propertyMappings != null)
        {
            var builder = new PropertyMappingBuilder<TSource, TDestination>();
            propertyMappings(builder);
            return builder.GetMappings();
        }

        return null;
    }

    #endregion

    #region Original Mapping Implementation
    // ... (keeping all your existing PropertyMappingBuilder and private methods unchanged)

    /// <summary>
    /// Builder class for creating property mappings using lambda expressions.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    public class PropertyMappingBuilder<TSource, TDestination>
    {
        private readonly Dictionary<string, Func<TSource, object>> _mappings = new();

        /// <summary>
        /// Maps a destination property to a source expression with automatic type conversion.
        /// </summary>
        /// <typeparam name="TDestProperty">The type of the destination property</typeparam>
        /// <param name="destinationProperty">Expression pointing to the destination property</param>
        /// <param name="sourceExpression">Expression defining how to get the value from source</param>
        /// <returns>The builder for chaining</returns>
        public PropertyMappingBuilder<TSource, TDestination> Map<TDestProperty>(
            Expression<Func<TDestination, TDestProperty>> destinationProperty,
            Expression<Func<TSource, object>> sourceExpression)
        {
            var destPropertyName = GetPropertyName(destinationProperty);
            var compiledExpression = sourceExpression.Compile();

            _mappings[destPropertyName] = source =>
            {
                var value = compiledExpression((TSource)source);
                return ConvertValue(value, typeof(TDestProperty));
            };
            return this;
        }

        /// <summary>
        /// Maps a destination property using a custom function.
        /// </summary>
        /// <typeparam name="TProperty">The type of the destination property</typeparam>
        /// <param name="destinationProperty">Expression pointing to the destination property</param>
        /// <param name="valueSelector">Custom function to calculate the value</param>
        /// <returns>The builder for chaining</returns>
        public PropertyMappingBuilder<TSource, TDestination> MapWith<TProperty>(
            Expression<Func<TDestination, TProperty>> destinationProperty,
            Func<TSource, TProperty> valueSelector)
        {
            var destPropertyName = GetPropertyName(destinationProperty);
            _mappings[destPropertyName] = source => valueSelector(source);
            return this;
        }

        internal Dictionary<string, Func<object, object>> GetMappings()
        {
            return _mappings.ToDictionary(
                kvp => kvp.Key,
                kvp => new Func<object, object>(source => kvp.Value((TSource)source))
            );
        }

        private static string GetPropertyName<T>(Expression<Func<TDestination, T>> expression)
        {
            return expression.Body switch
            {
                MemberExpression member => member.Member.Name,
                UnaryExpression unary when unary.Operand is MemberExpression memberExpr => memberExpr.Member.Name,
                _ => throw new ArgumentException("Expression must be a property access", nameof(expression))
            };
        }
    }

    // ... (all your existing private methods remain the same)
    private static object MapObject(object source, Type sourceType, Type destinationType,
        Dictionary<string, Func<object, object>>? customMappings = null)
    {
        if (source == null) return null;

        var destination = Activator.CreateInstance(destinationType);
        var sourceProps = GetCachedProperties(sourceType);
        var destPropsDict = GetCachedProperties(destinationType)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name);

        // Handle standard property mappings
        foreach (var sourceProp in sourceProps)
        {
            if (!destPropsDict.TryGetValue(sourceProp.Name, out var destProp))
                continue;

            var sourceValue = sourceProp.GetValue(source);
            SetPropertyValue(destination, destProp, sourceValue, sourceProp.PropertyType, destProp.PropertyType);
        }

        // Handle custom mappings
        if (customMappings != null)
        {
            foreach (var mapping in customMappings)
            {
                var destPropertyName = mapping.Key;
                var valueSelector = mapping.Value;

                if (!destPropsDict.TryGetValue(destPropertyName, out var destProp))
                    continue;

                try
                {
                    var value = valueSelector(source);
                    var convertedValue = ConvertValue(value, destProp.PropertyType);
                    destProp.SetValue(destination, convertedValue);
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    throw new InvalidOperationException(
                        $"Failed to map property '{destPropertyName}': {ex.Message}", ex);
                }
            }
        }

        return destination;
    }

    private static void MapToExistingObject(object source, object destination, Type sourceType, Type destinationType,
        bool skipNullValues, Dictionary<string, Func<object, object>>? customMappings = null)
    {
        var sourceProps = GetCachedProperties(sourceType);
        var destPropsDict = GetCachedProperties(destinationType)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name);

        // Handle standard property mappings
        foreach (var sourceProp in sourceProps)
        {
            if (!destPropsDict.TryGetValue(sourceProp.Name, out var destProp))
                continue;

            var sourceValue = sourceProp.GetValue(source);

            if (skipNullValues && sourceValue == null)
                continue;

            SetPropertyValue(destination, destProp, sourceValue, sourceProp.PropertyType, destProp.PropertyType);
        }

        // Handle custom mappings
        if (customMappings != null)
        {
            foreach (var mapping in customMappings)
            {
                var destPropertyName = mapping.Key;
                var valueSelector = mapping.Value;

                if (!destPropsDict.TryGetValue(destPropertyName, out var destProp))
                    continue;

                try
                {
                    var value = valueSelector(source);

                    if (skipNullValues && value == null)
                        continue;

                    var convertedValue = ConvertValue(value, destProp.PropertyType);
                    destProp.SetValue(destination, convertedValue);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to map property '{destPropertyName}': {ex.Message}", ex);
                }
            }
        }
    }

    private static void SetPropertyValue(object destination, PropertyInfo destProp, object sourceValue,
        Type sourcePropertyType, Type destPropertyType)
    {
        if (sourceValue == null)
        {
            destProp.SetValue(destination, null);
            return;
        }

        // Direct assignment for compatible types
        if (destPropertyType.IsAssignableFrom(sourcePropertyType))
        {
            destProp.SetValue(destination, sourceValue);
        }
        // Handle collections (IEnumerable<T>)
        else if (IsEnumerableType(sourcePropertyType) && IsEnumerableType(destPropertyType))
        {
            var mappedCollection = MapCollection(sourceValue, sourcePropertyType, destPropertyType);
            destProp.SetValue(destination, mappedCollection);
        }
        // Handle nested objects (DTOs)
        else if (IsComplexType(sourcePropertyType) && IsComplexType(destPropertyType))
        {
            var mappedObject = MapObject(sourceValue, sourcePropertyType, destPropertyType);
            destProp.SetValue(destination, mappedObject);
        }
        else
        {
            // Try to convert the value
            var convertedValue = ConvertValue(sourceValue, destPropertyType);
            destProp.SetValue(destination, convertedValue);
        }
    }

    private static object ConvertValue(object value, Type targetType)
    {
        if (value == null)
            return null;

        var sourceType = value.GetType();

        // If types are compatible, return as-is
        if (targetType.IsAssignableFrom(sourceType))
            return value;

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Try to convert using Convert.ChangeType
        try
        {
            return Convert.ChangeType(value, underlyingType);
        }
        catch
        {
            // If conversion fails, return the original value or default
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }

    private static PropertyInfo[] GetCachedProperties(Type type)
    {
        return PropertyCache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
    }

    private static object MapCollection(object sourceCollection, Type sourceType, Type destType)
    {
        if (sourceCollection == null) return null;

        var sourceElementType = GetEnumerableElementType(sourceType);
        var destElementType = GetEnumerableElementType(destType);

        if (sourceElementType == null || destElementType == null)
            return sourceCollection;

        var sourceList = ((IEnumerable)sourceCollection).Cast<object>().ToList();
        var destListType = typeof(List<>).MakeGenericType(destElementType);
        var destList = (IList)Activator.CreateInstance(destListType);

        foreach (var sourceItem in sourceList)
        {
            if (sourceItem == null)
            {
                destList.Add(null);
                continue;
            }

            // Direct assignment for compatible types
            if (destElementType.IsAssignableFrom(sourceElementType))
            {
                destList.Add(sourceItem);
            }
            // Map complex types
            else if (IsComplexType(sourceElementType) && IsComplexType(destElementType))
            {
                var mappedItem = MapObject(sourceItem, sourceElementType, destElementType);
                destList.Add(mappedItem);
            }
        }

        // Convert to destination collection type if needed
        if (destType.IsArray)
        {
            var array = Array.CreateInstance(destElementType, destList.Count);
            destList.CopyTo(array, 0);
            return array;
        }

        return destList;
    }

    private static bool IsEnumerableType(Type type)
    {
        return type != typeof(string) &&
               typeof(IEnumerable).IsAssignableFrom(type);
    }

    private static Type? GetEnumerableElementType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType)
        {
            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length == 1)
                return genericArgs[0];
        }

        // Check implemented interfaces for IEnumerable<T>
        foreach (var interfaceType in type.GetInterfaces())
        {
            if (interfaceType.IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return interfaceType.GetGenericArguments()[0];
            }
        }

        return null;
    }

    private static bool IsComplexType(Type type)
    {
        return !type.IsPrimitive &&
               !type.IsEnum &&
               type != typeof(string) &&
               type != typeof(DateTime) &&
               type != typeof(TimeSpan) &&
               type != typeof(Guid) &&
               type != typeof(decimal) &&
               !type.IsValueType;
    }

    #endregion
}