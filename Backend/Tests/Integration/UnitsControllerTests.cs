using Application.DTOs.Unit;
using FluentAssertions;
using System.Net;
using Tests.Infrastructure;
using Utilities.Responses;
using Xunit;

namespace Tests.Integration;

public class UnitsControllerTests : IClassFixture<WarehouseTestFactory>
{
    private readonly HttpClient _client;
    private readonly WarehouseTestFactory _factory;

    public UnitsControllerTests(WarehouseTestFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateUnit_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateUnitDto
        {
            Name = "pounds"
        };

        // Act
        var response = await _client.PostAsync("/api/units", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<UnitResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(createDto.Name);
        result.Data.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task CreateUnit_DuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateUnitDto
        {
            Name = "kg" // This exists in seed data
        };

        // Act
        var response = await _client.PostAsync("/api/units", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateUnit_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateUnitDto
        {
            Name = "" // Invalid name
        };

        // Act
        var response = await _client.PostAsync("/api/units", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUnit_ExistingId_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/units/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<UnitResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Name.Should().Be("kg");
    }

    [Fact]
    public async Task GetUnit_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/units/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllUnits_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/units");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<UnitResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().NotBeEmpty();
        result.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateUnit_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateUnitDto
        {
            Id = 1,
            Name = "kilograms",
            IsArchived = false
        };

        // Act
        var response = await _client.PutAsync("/api/units", TestHelpers.CreateJsonContent(updateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<UnitResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(updateDto.Name);
    }

    [Fact]
    public async Task UpdateUnit_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateUnitDto
        {
            Id = 999,
            Name = "Non-existing Unit",
            IsArchived = false
        };

        // Act
        var response = await _client.PutAsync("/api/units", TestHelpers.CreateJsonContent(updateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ArchiveUnit_ExistingUnit_ReturnsOk()
    {
        // Act
        var response = await _client.PatchAsync("/api/units/1/archive", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Contain("archived successfully");
    }

    [Fact]
    public async Task UnarchiveUnit_ExistingUnit_ReturnsOk()
    {
        // First archive the unit
        await _client.PatchAsync("/api/units/1/archive", null);

        // Act
        var response = await _client.PatchAsync("/api/units/1/unarchive", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Contain("unarchived successfully");
    }

    [Fact]
    public async Task DeleteUnit_NonExistingUnit_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/units/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllUnits_WithSearch_ReturnsFilteredResults()
    {
        // Arrange - Create a unit with specific name first
        var createDto = new CreateUnitDto { Name = "SearchableUnit" };
        await _client.PostAsync("/api/units", TestHelpers.CreateJsonContent(createDto));

        // Act
        var response = await _client.GetAsync("/api/units?searchTerm=Searchable");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<UnitResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().Contain(u => u.Name.Contains("SearchableUnit"));
    }
}
