using Application.DTOs.Client;
using FluentAssertions;
using System.Net;
using Tests.Infrastructure;
using Utilities.Responses;
using Xunit;

namespace Tests.Integration;

public class ClientsControllerTests : IClassFixture<WarehouseTestFactory>
{
    private readonly HttpClient _client;
    private readonly WarehouseTestFactory _factory;

    public ClientsControllerTests(WarehouseTestFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateClient_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateClientDto
        {
            Name = "Integration Test Client",
            Address = "123 Integration Test Street"
        };

        // Act
        var response = await _client.PostAsync("/api/clients", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<ClientResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(createDto.Name);
        result.Data.Address.Should().Be(createDto.Address);
        result.Data.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task CreateClient_DuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateClientDto
        {
            Name = "Test Client", // This exists in seed data
            Address = "456 Different Street"
        };

        // Act
        var response = await _client.PostAsync("/api/clients", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task GetClient_ExistingId_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/clients/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<ClientResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Name.Should().Be("Test Client");
    }

    [Fact]
    public async Task GetClient_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/clients/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllClients_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/clients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<ClientResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().NotBeEmpty();
        result.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateClient_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateClientDto
        {
            Id = 1,
            Name = "Updated Test Client",
            Address = "Updated Address",
            IsArchived = false
        };

        // Act
        var response = await _client.PutAsync("/api/clients", TestHelpers.CreateJsonContent(updateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<ClientResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(updateDto.Name);
        result.Data.Address.Should().Be(updateDto.Address);
    }

    [Fact]
    public async Task ArchiveClient_ExistingClient_ReturnsOk()
    {
        // Act
        var response = await _client.PatchAsync("/api/clients/1/archive", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Contain("archived successfully");
    }

    [Fact]
    public async Task UnarchiveClient_ExistingClient_ReturnsOk()
    {
        // First archive the client
        await _client.PatchAsync("/api/clients/1/archive", null);

        // Act
        var response = await _client.PatchAsync("/api/clients/1/unarchive", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Contain("unarchived successfully");
    }
}
