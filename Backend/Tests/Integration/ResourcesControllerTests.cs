using Application.DTOs.Resource;
using FluentAssertions;
using System.Net;
using Tests.Infrastructure;
using Utilities.Responses;
using Xunit;

namespace Tests.Integration;

public class ResourcesControllerTests : IClassFixture<WarehouseTestFactory>
{
    private readonly HttpClient _client;
    private readonly WarehouseTestFactory _factory;

    public ResourcesControllerTests(WarehouseTestFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateResource_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateResourceDto
        {
            Name = "Integration Test Resource"
        };

        // Act
        var response = await _client.PostAsync("/api/resources", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<ResourceResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(createDto.Name);
        result.Data.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task CreateResource_DuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateResourceDto
        {
            Name = "Steel Bars" // This exists in seed data
        };

        // Act
        var response = await _client.PostAsync("/api/resources", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateResource_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateResourceDto
        {
            Name = "" // Invalid name
        };

        // Act
        var response = await _client.PostAsync("/api/resources", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetResource_ExistingId_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/resources/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<ResourceResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Name.Should().Be("Steel Bars");
    }

    [Fact]
    public async Task GetResource_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/resources/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllResources_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/resources");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<ResourceResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().NotBeEmpty();
        result.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateResource_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateResourceDto
        {
            Id = 1,
            Name = "Updated Steel Bars",
            IsArchived = false
        };

        // Act
        var response = await _client.PutAsync("/api/resources", TestHelpers.CreateJsonContent(updateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<ResourceResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(updateDto.Name);
    }

    [Fact]
    public async Task UpdateResource_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateResourceDto
        {
            Id = 999,
            Name = "Non-existing Resource",
            IsArchived = false
        };

        // Act
        var response = await _client.PutAsync("/api/resources", TestHelpers.CreateJsonContent(updateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ArchiveResource_ExistingResource_ReturnsOk()
    {
        // Act
        var response = await _client.PatchAsync("/api/resources/1/archive", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Contain("archived successfully");
    }

    [Fact]
    public async Task UnarchiveResource_ExistingResource_ReturnsOk()
    {
        // First archive the resource
        await _client.PatchAsync("/api/resources/1/archive", null);

        // Act
        var response = await _client.PatchAsync("/api/resources/1/unarchive", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Contain("unarchived successfully");
    }

    [Fact]
    public async Task DeleteResource_NonExistingResource_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/resources/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
