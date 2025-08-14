using Domain.Models.Enums;
using FluentAssertions;
using Xunit;

namespace Tests.UnitTests.Domain.Enums;

public class ShipmentStatusTests
{
    [Fact]
    public void ShipmentStatus_ShouldHaveExpectedValues()
    {
        // Arrange & Act
        var draftValue = ShipmentStatus.Draft;
        var signedValue = ShipmentStatus.Signed;

        // Assert
        ((int)draftValue).Should().Be(0);
        ((int)signedValue).Should().Be(1);
    }

    [Fact]
    public void ShipmentStatus_ShouldContainAllExpectedMembers()
    {
        // Arrange
        var expectedValues = new[] { "Draft", "Signed" };

        // Act
        var actualValues = Enum.GetNames(typeof(ShipmentStatus));

        // Assert
        actualValues.Should().BeEquivalentTo(expectedValues);
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft, "Draft")]
    [InlineData(ShipmentStatus.Signed, "Signed")]
    public void ShipmentStatus_ToString_ShouldReturnCorrectString(ShipmentStatus status, string expectedString)
    {
        // Act
        var result = status.ToString();

        // Assert
        result.Should().Be(expectedString);
    }

    [Fact]
    public void ShipmentStatus_ShouldSupportConversionFromString()
    {
        // Arrange & Act
        var draftFromString = Enum.Parse<ShipmentStatus>("Draft");
        var signedFromString = Enum.Parse<ShipmentStatus>("Signed");

        // Assert
        draftFromString.Should().Be(ShipmentStatus.Draft);
        signedFromString.Should().Be(ShipmentStatus.Signed);
    }

    [Fact]
    public void ShipmentStatus_ShouldSupportConversionFromInt()
    {
        // Arrange & Act
        var draftFromInt = (ShipmentStatus)0;
        var signedFromInt = (ShipmentStatus)1;

        // Assert
        draftFromInt.Should().Be(ShipmentStatus.Draft);
        signedFromInt.Should().Be(ShipmentStatus.Signed);
    }

    [Fact]
    public void ShipmentStatus_GetValues_ShouldReturnAllValues()
    {
        // Arrange
        var expectedValues = new[] { ShipmentStatus.Draft, ShipmentStatus.Signed };

        // Act
        var actualValues = Enum.GetValues<ShipmentStatus>();

        // Assert
        actualValues.Should().BeEquivalentTo(expectedValues);
    }
}
