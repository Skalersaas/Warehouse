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
        };

        // Assert
        client.Id.Should().Be(id);
        client.Name.Should().Be(name);
        client.Address.Should().Be(address);
        client.IsArchived.Should().Be(isArchived);
        client.CreatedAt.Should().Be(createdAt);
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
    public void Client_ArchiveWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var client = TestDataBuilder.CreateValidClient("Active Client", "Active Address");
        client.IsArchived.Should().BeFalse();

        // Act - Archive
        client.IsArchived = true;

        // Assert
        client.IsArchived.Should().BeTrue();

        // Act - Unarchive
        client.IsArchived = false;

        // Assert
        client.IsArchived.Should().BeFalse();
    }

    [Theory]
    [InlineData("Client A", "Address A")]
    [InlineData("Client B", "Address B")]
    [InlineData("Very Long Client Name That Exceeds Normal Length", "Very Long Address That Also Exceeds Normal Length For Testing")]
    public void Client_WithVariousNameAndAddressCombinations_ShouldWork(string name, string address)
    {
        // Arrange & Act
        var client = new Client
        {
            Name = name,
            Address = address
        };

        // Assert
        client.Name.Should().Be(name);
        client.Address.Should().Be(address);
    }

    [Fact]
    public void Client_ShouldSupportCompleteEntityLifecycle()
    {
        // Arrange - Creation
        var client = new Client
        {
            Name = "Lifecycle Test Client",
            Address = "Initial Address"
        };
        var creationTime = client.CreatedAt;

        // Act - Update
        client.Name = "Updated Client Name";
        client.Address = "Updated Address";

        // Act - Archive
        client.IsArchived = true;

        // Assert
        client.Name.Should().Be("Updated Client Name");
        client.Address.Should().Be("Updated Address");
        client.IsArchived.Should().BeTrue();
        client.CreatedAt.Should().Be(creationTime);
    }

    [Theory]
    [InlineData("Test Client", "123 Main St", false)]
    [InlineData("Archived Client", "456 Old St", true)]
    public void Client_ShouldSupportDifferentArchiveStates(string name, string address, bool isArchived)
    {
        // Arrange & Act
        var client = new Client
        {
            Name = name,
            Address = address,
            IsArchived = isArchived
        };

        // Assert
        client.Name.Should().Be(name);
        client.Address.Should().Be(address);
        client.IsArchived.Should().Be(isArchived);
    }

    [Fact]
    public void Client_TimestampProperties_ShouldFollowExpectedPattern()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var client = new Client
        {
            Name = "Timestamp Test Client",
            Address = "Test Address"
        };
        var afterCreation = DateTime.UtcNow;

        // Assert
        client.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        client.CreatedAt.Should().BeOnOrBefore(afterCreation);

        // Act - Set UpdatedAt
        var updateTime = DateTime.UtcNow;
    }
}
