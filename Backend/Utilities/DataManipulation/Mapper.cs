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

        var destination = new TDestination();

        var sourceProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destPropsDict = typeof(TDestination)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name);

        foreach (var sourceProp in sourceProps)
        {
            if (destPropsDict.TryGetValue(sourceProp.Name, out var destProp) &&
                destProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
            {
                var value = sourceProp.GetValue(dto);
                destProp.SetValue(destination, value);
            }
        }

        return destination;
    }
}