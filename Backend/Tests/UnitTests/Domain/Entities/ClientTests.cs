using AutoFixture;
using AutoFixture.Xunit2;
using Domain.Models.Entities;
using Domain.Models.Interfaces;
using FluentAssertions;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Domain.Entities;

public class ClientTests
{
    private readonly Fixture _fixture = FixtureExtensions.CreateDomainFixture();

    [Fact]
    public void Client_ShouldImplementIModel()
    {
        // Arrange & Act
        var client = new Client();

        // Assert
        client.Should().BeAssignableTo<IModel>();
    }

    [Fact]
    public void Client_ShouldImplementIArchivable()
    {
        // Arrange & Act
        var client = new Client();

        // Assert
        client.Should().BeAssignableTo<IArchivable>();
    }

    [Fact]
    public void Client_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var client = new Client();

        // Assert
        client.Id.Should().Be(0);
        client.Name.Should().BeNull();
        client.Address.Should().BeNull();
        client.IsArchived.Should().BeFalse();
        client.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        client.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void Client_ShouldSetPropertiesCorrectly(int id, string name, string address, bool isArchived)
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        var client = new Client
        {
            Id = id,
            Name = name,
            Address = address,
            IsArchived = isArchived,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        client.Id.Should().Be(id);
        client.Name.Should().Be(name);
        client.Address.Should().Be(address);
        client.IsArchived.Should().Be(isArchived);
        client.CreatedAt.Should().Be(createdAt);
        client.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void Client_CreatedAt_ShouldBeSetToUtcNow_WhenUsingDefaultConstructor()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var client = new Client();
        var afterCreation = DateTime.UtcNow;

        // Assert
        client.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        client.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Client_ShouldAcceptInvalidNameValues(string invalidName)
    {
        // Arrange & Act
        var client = new Client { Name = invalidName };

        // Assert
        // Note: The entity doesn't enforce validation at this level
        // Validation should be handled at the business logic layer
        client.Name.Should().Be(invalidName);
    }

    [Fact]
    public void Client_ShouldSupportArchiving()
    {
        // Arrange
        var client = _fixture.Create<Client>();
        client.IsArchived = false;

        // Act
        client.IsArchived = true;

        // Assert
        client.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Client_ShouldSupportUpdatedAtTracking()
    {
        // Arrange
        var client = _fixture.Create<Client>();
        var originalUpdatedAt = client.UpdatedAt;
        var newUpdatedAt = DateTime.UtcNow;

        // Act
        client.UpdatedAt = newUpdatedAt;

        // Assert
        client.UpdatedAt.Should().Be(newUpdatedAt);
        client.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }
}
