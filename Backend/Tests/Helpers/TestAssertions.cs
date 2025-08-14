using FluentAssertions;
using System.Reflection;

namespace Tests.Helpers;

public static class TestAssertions
{
    /// <summary>
    /// Asserts that a type implements a specific interface
    /// </summary>
    public static void ShouldImplementInterface<TInterface>(this Type type)
    {
        type.Should().BeAssignableTo<TInterface>();
    }

    /// <summary>
    /// Asserts that an object has all required properties with correct types
    /// </summary>
    public static void ShouldHaveProperty<T>(this Type type, string propertyName, Type expectedType)
    {
        var property = type.GetProperty(propertyName);
        property.Should().NotBeNull($"Property {propertyName} should exist");
        property!.PropertyType.Should().Be(expectedType, $"Property {propertyName} should be of type {expectedType.Name}");
    }

    /// <summary>
    /// Asserts that an object has all required properties for an IModel entity
    /// </summary>
    public static void ShouldHaveIModelProperties(this Type type)
    {
        type.ShouldHaveProperty<int>("Id", typeof(int));
    }

    /// <summary>
    /// Asserts that an object has all required properties for an IArchivable entity
    /// </summary>
    public static void ShouldHaveIArchivableProperties(this Type type)
    {
        type.ShouldHaveProperty<bool>("IsArchived", typeof(bool));
    }

    /// <summary>
    /// Asserts that a DateTime property is close to the expected time
    /// </summary>
    public static void ShouldBeCloseToNow(this DateTime dateTime, TimeSpan? precision = null)
    {
        var actualPrecision = precision ?? TimeSpan.FromSeconds(5);
        dateTime.Should().BeCloseTo(DateTime.UtcNow, actualPrecision);
    }

    /// <summary>
    /// Asserts that all navigation properties are properly configured
    /// </summary>
    public static void ShouldHaveNavigationProperty<T>(this Type type, string propertyName)
    {
        var property = type.GetProperty(propertyName);
        property.Should().NotBeNull($"Navigation property {propertyName} should exist");
        
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(ICollection<>))
        {
            property!.PropertyType.Should().BeAssignableTo<T>($"Property {propertyName} should be assignable to {typeof(T).Name}");
        }
        else
        {
            property!.PropertyType.Should().Be(typeof(T), $"Property {propertyName} should be of type {typeof(T).Name}");
        }
    }

    /// <summary>
    /// Verifies that an entity follows the audit trail pattern
    /// </summary>
    public static void ShouldFollowAuditTrailPattern(this Type type, bool shouldHaveUpdatedAt = true)
    {
        type.ShouldHaveProperty<DateTime>("CreatedAt", typeof(DateTime));
        
        if (shouldHaveUpdatedAt)
        {
            type.ShouldHaveProperty<DateTime?>("UpdatedAt", typeof(DateTime?));
        }
    }

    /// <summary>
    /// Asserts that a collection contains exactly the specified number of items
    /// </summary>
    public static void ShouldContainExactly<T>(this IEnumerable<T> collection, int expectedCount)
    {
        collection.Should().HaveCount(expectedCount);
    }

    /// <summary>
    /// Asserts that a decimal value is positive
    /// </summary>
    public static void ShouldBePositive(this decimal value)
    {
        value.Should().BePositive();
    }

    /// <summary>
    /// Asserts that a decimal value is non-negative (>= 0)
    /// </summary>
    public static void ShouldBeNonNegative(this decimal value)
    {
        value.Should().BeGreaterOrEqualTo(0);
    }

    /// <summary>
    /// Verifies that an entity can be properly serialized/deserialized
    /// </summary>
    public static void ShouldBeSerializable<T>(this T entity) where T : class
    {
        entity.Should().NotBeNull();
        
        // Check that all public properties have getters and setters
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var property in properties)
        {
            property.CanRead.Should().BeTrue($"Property {property.Name} should have a getter");
            
            // Allow read-only properties for calculated fields or collections
            if (property.PropertyType.IsValueType || property.PropertyType == typeof(string))
            {
                property.CanWrite.Should().BeTrue($"Property {property.Name} should have a setter");
            }
        }
    }

    /// <summary>
    /// Verifies that foreign key properties exist for navigation properties
    /// </summary>
    public static void ShouldHaveForeignKeyFor(this Type type, string navigationPropertyName, string? foreignKeyName = null)
    {
        var expectedForeignKey = foreignKeyName ?? $"{navigationPropertyName}Id";
        type.ShouldHaveProperty<int>(expectedForeignKey, typeof(int));
    }
}
