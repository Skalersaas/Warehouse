using System.Collections;
using System.Reflection;

namespace Utilities.DataManipulation;

/// <summary>
/// Provides functionality for mapping between DTO (Data Transfer Object) and domain model objects.
/// This class handles the automatic mapping of properties between objects of different types.
/// </summary>
public static class Mapper
{
    /// <summary>
    /// Maps properties from a DTO object to a new instance of the destination type.
    /// </summary>
    /// <typeparam name="TDestination">The type of object to create and map to.</typeparam>
    /// <typeparam name="TSource">The type of the source DTO object.</typeparam>
    /// <param name="dto">The source DTO object to map from.</param>
    /// <returns>A new instance of TDestination with properties mapped from the source DTO.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dto parameter is null.</exception>
    /// <remarks>
    /// Maps properties with matching names and compatible types, preserving original values and handling only writable properties.
    /// </remarks>
    public static TDestination FromDTO<TDestination, TSource>(TSource dto)
        where TDestination : new()
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        return (TDestination)MapObject(dto, typeof(TSource), typeof(TDestination));
    }

    /// <summary>
    /// Maps properties from a source object to an existing destination object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TDestination">The type of the destination object.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <param name="destination">The existing destination object to map to.</param>
    /// <param name="skipNullValues">If true, null values from source won't overwrite destination values.</param>
    /// <exception cref="ArgumentNullException">Thrown when source or destination is null.</exception>
    /// <remarks>
    /// Updates the existing destination object with values from the source object.
    /// Only maps properties that exist in both objects and are writable in the destination.
    /// </remarks>
    public static void MapToExisting<TSource, TDestination>(TSource source, TDestination destination, bool skipNullValues = false)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (destination == null) throw new ArgumentNullException(nameof(destination));

        MapToExistingObject(source, destination, typeof(TSource), typeof(TDestination), skipNullValues);
    }

    /// <summary>
    /// Maps properties from a source object to an existing destination object with type inference.
    /// </summary>
    /// <param name="source">The source object to map from.</param>
    /// <param name="destination">The existing destination object to map to.</param>
    /// <param name="skipNullValues">If true, null values from source won't overwrite destination values.</param>
    /// <exception cref="ArgumentNullException">Thrown when source or destination is null.</exception>
    public static void MapToExisting(object source, object destination, bool skipNullValues = false)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (destination == null) throw new ArgumentNullException(nameof(destination));

        MapToExistingObject(source, destination, source.GetType(), destination.GetType(), skipNullValues);
    }

    private static object MapObject(object source, Type sourceType, Type destinationType)
    {
        if (source == null) return null;

        var destination = Activator.CreateInstance(destinationType);
        var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destPropsDict = destinationType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name);

        foreach (var sourceProp in sourceProps)
        {
            if (!destPropsDict.TryGetValue(sourceProp.Name, out var destProp))
                continue;

            var sourceValue = sourceProp.GetValue(source);
            if (sourceValue == null)
            {
                destProp.SetValue(destination, null);
                continue;
            }

            var sourcePropertyType = sourceProp.PropertyType;
            var destPropertyType = destProp.PropertyType;

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
        }

        return destination;
    }

    private static void MapToExistingObject(object source, object destination, Type sourceType, Type destinationType, bool skipNullValues)
    {
        var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destPropsDict = destinationType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name);

        foreach (var sourceProp in sourceProps)
        {
            if (!destPropsDict.TryGetValue(sourceProp.Name, out var destProp))
                continue;

            var sourceValue = sourceProp.GetValue(source);

            // Skip null values if requested
            if (skipNullValues && sourceValue == null)
                continue;

            if (sourceValue == null)
            {
                destProp.SetValue(destination, null);
                continue;
            }

            var sourcePropertyType = sourceProp.PropertyType;
            var destPropertyType = destProp.PropertyType;

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
            // Handle nested objects (DTOs) - map to existing nested object if it exists
            else if (IsComplexType(sourcePropertyType) && IsComplexType(destPropertyType))
            {
                var existingNestedObject = destProp.GetValue(destination);
                if (existingNestedObject != null)
                {
                    // Map to existing nested object
                    MapToExistingObject(sourceValue, existingNestedObject, sourcePropertyType, destPropertyType, skipNullValues);
                }
                else
                {
                    // Create new nested object
                    var mappedObject = MapObject(sourceValue, sourcePropertyType, destPropertyType);
                    destProp.SetValue(destination, mappedObject);
                }
            }
        }
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

    private static Type GetEnumerableElementType(Type type)
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
}