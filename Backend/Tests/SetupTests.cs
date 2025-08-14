using Tests.Helpers;
using Xunit;

namespace Tests;

public class SetupTests
{
    [Fact]
    public void TestFramework_ShouldBeWorkingCorrectly()
    {
        // Arrange
        var expected = true;
        
        // Act
        var actual = true;
        
        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FluentAssertions_ShouldBeWorkingCorrectly()
    {
        // Arrange
        var testString = "Hello World";
        
        // Act & Assert
        testString.Should().NotBeNull();
        testString.Should().Contain("World");
        testString.Should().StartWith("Hello");
    }

    [Fact]
    public void AutoFixture_ShouldBeWorkingCorrectly()
    {
        // Arrange
        var fixture = FixtureExtensions.CreateSimpleFixture();
        
        // Act
        var randomString = fixture.Create<string>();
        var randomInt = fixture.Create<int>();
        
        // Assert
        randomString.Should().NotBeNullOrEmpty();
        randomInt.Should().NotBe(0);
    }
}
