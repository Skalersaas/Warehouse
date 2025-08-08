using Application.DTOs.Balance;
using FluentAssertions;
using System.Net;
using Tests.Infrastructure;
using Utilities.Responses;
using Xunit;

namespace Tests.Integration;

public class BalanceControllerTests : IClassFixture<WarehouseTestFactory>
{
    private readonly HttpClient _client;
    private readonly WarehouseTestFactory _factory;

    public BalanceControllerTests(WarehouseTestFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetCurrentBalance_ExistingResourceAndUnit_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/balance/current?resourceId=1&unitId=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<decimal>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetCurrentBalance_NonExistingResource_ReturnsZero()
    {
        // Act
        var response = await _client.GetAsync("/api/balance/current?resourceId=999&unitId=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<decimal>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().Be(0);
    }

    [Fact]
    public async Task GetCurrentBalance_NonExistingUnit_ReturnsZero()
    {
        // Act
        var response = await _client.GetAsync("/api/balance/current?resourceId=1&unitId=999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<decimal>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().Be(0);
    }

    [Fact]
    public async Task GetCurrentBalance_MissingParameters_ReturnsBadRequest()
    {
        // Act - Missing resourceId
        var response1 = await _client.GetAsync("/api/balance/current?unitId=1");
        
        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act - Missing unitId
        var response2 = await _client.GetAsync("/api/balance/current?resourceId=1");
        
        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act - Missing both parameters
        var response3 = await _client.GetAsync("/api/balance/current");
        
        // Assert
        response3.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllBalances_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/balance");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<BalanceResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetAllBalances_WithResourceFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/balance?resourceIds=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<BalanceResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        // If there are results, they should all be for resource 1
        if (result.Data!.Any())
        {
            result.Data.Should().OnlyContain(b => b.ResourceId == 1);
        }
    }

    [Fact]
    public async Task GetAllBalances_WithUnitFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/balance?unitIds=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<BalanceResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        // If there are results, they should all be for unit 1
        if (result.Data!.Any())
        {
            result.Data.Should().OnlyContain(b => b.UnitId == 1);
        }
    }

    [Fact]
    public async Task GetAllBalances_WithMinimumQuantityFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/balance?minimumQuantity=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<BalanceResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        // If there are results, they should all have quantity >= 50
        if (result.Data!.Any())
        {
            result.Data.Should().OnlyContain(b => b.Quantity >= 50);
        }
    }

    [Fact]
    public async Task GetAllBalances_WithSearchTerm_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/balance?searchTerm=Steel");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<BalanceResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        // If there are results, they should contain "Steel" in resource name
        if (result.Data!.Any())
        {
            result.Data.Should().OnlyContain(b => b.ResourceName.Contains("Steel", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task GetAllBalances_WithPagination_ReturnsPagedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/balance?page=1&size=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<BalanceResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        // Should return at most 5 items
        result.Data!.Should().HaveCountLessOrEqualTo(5);
    }

    [Fact]
    public async Task CheckSufficientBalance_SufficientQuantity_ReturnsTrue()
    {
        // Act
        var response = await _client.GetAsync("/api/balance/sufficient?resourceId=1&unitId=1&requiredQuantity=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<bool>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        // The result depends on actual balance, but should be a valid boolean
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task CheckSufficientBalance_LargeQuantity_ReturnsFalse()
    {
        // Act - Check for a very large quantity that's unlikely to be available
        var response = await _client.GetAsync("/api/balance/sufficient?resourceId=1&unitId=1&requiredQuantity=999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<bool>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().BeFalse(); // Very unlikely to have such a large quantity
    }

    [Fact]
    public async Task CheckSufficientBalance_MissingParameters_ReturnsBadRequest()
    {
        // Act - Missing requiredQuantity
        var response1 = await _client.GetAsync("/api/balance/sufficient?resourceId=1&unitId=1");
        
        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act - Missing resourceId
        var response2 = await _client.GetAsync("/api/balance/sufficient?unitId=1&requiredQuantity=10");
        
        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act - Missing unitId
        var response3 = await _client.GetAsync("/api/balance/sufficient?resourceId=1&requiredQuantity=10");
        
        // Assert
        response3.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CheckSufficientBalance_InvalidParameters_ReturnsBadRequest()
    {
        // Act - Negative quantity
        var response1 = await _client.GetAsync("/api/balance/sufficient?resourceId=1&unitId=1&requiredQuantity=-10");
        
        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act - Zero or negative resourceId
        var response2 = await _client.GetAsync("/api/balance/sufficient?resourceId=0&unitId=1&requiredQuantity=10");
        
        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act - Zero or negative unitId
        var response3 = await _client.GetAsync("/api/balance/sufficient?resourceId=1&unitId=0&requiredQuantity=10");
        
        // Assert
        response3.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
