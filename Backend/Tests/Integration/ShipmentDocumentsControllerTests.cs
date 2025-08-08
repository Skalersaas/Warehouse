using Application.DTOs.ShipmentDocument;
using Application.DTOs.ShipmentItem;
using Domain.Models.Enums;
using FluentAssertions;
using System.Net;
using Tests.Infrastructure;
using Utilities.Responses;
using Xunit;

namespace Tests.Integration;

public class ShipmentDocumentsControllerTests : IClassFixture<WarehouseTestFactory>
{
    private readonly HttpClient _client;
    private readonly WarehouseTestFactory _factory;

    public ShipmentDocumentsControllerTests(WarehouseTestFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateShipmentDocument_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateShipmentDocumentDto
        {
            Number = "SHP-TEST-001",
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 50 }
            }
        };

        // Act
        var response = await _client.PostAsync("/api/shipmentdocuments", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<ShipmentDocumentResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Number.Should().Be(createDto.Number);
        result.Data.Status.Should().Be(ShipmentStatus.Draft);
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().Quantity.Should().Be(50);
    }

    [Fact]
    public async Task CreateShipmentDocument_DuplicateNumber_ReturnsBadRequest()
    {
        // Arrange - First create a shipment
        var firstDto = new CreateShipmentDocumentDto
        {
            Number = "SHP-DUPLICATE",
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 25 }
            }
        };
        await _client.PostAsync("/api/shipmentdocuments", TestHelpers.CreateJsonContent(firstDto));

        // Try to create another with same number
        var duplicateDto = new CreateShipmentDocumentDto
        {
            Number = "SHP-DUPLICATE", // Same number
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 30 }
            }
        };

        // Act
        var response = await _client.PostAsync("/api/shipmentdocuments", TestHelpers.CreateJsonContent(duplicateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateShipmentDocument_InvalidClient_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateShipmentDocumentDto
        {
            Number = "SHP-INVALID-CLIENT",
            ClientId = 999, // Invalid client
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 50 }
            }
        };

        // Act
        var response = await _client.PostAsync("/api/shipmentdocuments", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("Client not found");
    }

    [Fact]
    public async Task CreateShipmentDocument_EmptyItems_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateShipmentDocumentDto
        {
            Number = "SHP-EMPTY",
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>() // Empty items
        };

        // Act
        var response = await _client.PostAsync("/api/shipmentdocuments", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("at least one item");
    }

    [Fact]
    public async Task GetShipmentDocument_AfterCreation_ReturnsCorrectData()
    {
        // Arrange - Create a shipment first
        var createDto = new CreateShipmentDocumentDto
        {
            Number = "SHP-GET-TEST",
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 75 }
            }
        };

        var createResponse = await _client.PostAsync("/api/shipmentdocuments", TestHelpers.CreateJsonContent(createDto));
        var createResult = await TestHelpers.DeserializeResponse<ApiResponse<ShipmentDocumentResponseDto>>(createResponse);
        var shipmentId = createResult!.Data!.Id;

        // Act
        var response = await _client.GetAsync($"/api/shipmentdocuments/{shipmentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<ShipmentDocumentResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(shipmentId);
        result.Data.Number.Should().Be("SHP-GET-TEST");
        result.Data.Status.Should().Be(ShipmentStatus.Draft);
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().Quantity.Should().Be(75);
    }

    [Fact]
    public async Task GetShipmentDocument_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/shipmentdocuments/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllShipmentDocuments_ReturnsOk()
    {
        // Arrange - Create a shipment to ensure we have data
        var createDto = new CreateShipmentDocumentDto
        {
            Number = "SHP-GETALL-TEST",
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 100 }
            }
        };
        await _client.PostAsync("/api/shipmentdocuments", TestHelpers.CreateJsonContent(createDto));

        // Act
        var response = await _client.GetAsync("/api/shipmentdocuments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<ShipmentDocumentResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().NotBeEmpty();
        result.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateShipmentDocument_DraftStatus_ValidData_ReturnsOk()
    {
        // Arrange - Create a draft shipment first
        var createDto = new CreateShipmentDocumentDto
        {
            Number = "SHP-UPDATE-TEST",
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 50 }
            }
        };

        var createResponse = await _client.PostAsync("/api/shipmentdocuments", TestHelpers.CreateJsonContent(createDto));
        var createResult = await TestHelpers.DeserializeResponse<ApiResponse<ShipmentDocumentResponseDto>>(createResponse);
        var shipmentId = createResult!.Data!.Id;
        var itemId = createResult.Data.Items.First().Id;

        var updateDto = new UpdateShipmentDocumentDto
        {
            Id = shipmentId,
            Number = "SHP-UPDATE-TEST-MODIFIED",
            ClientId = 1,
            Date = DateTime.Today.AddDays(1),
            Items = new List<UpdateShipmentItemDto>
            {
                new() { Id = itemId, ResourceId = 1, UnitId = 1, Quantity = 100 }
            }
        };

        // Act
        var response = await _client.PutAsync("/api/shipmentdocuments", TestHelpers.CreateJsonContent(updateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<ShipmentDocumentResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Number.Should().Be("SHP-UPDATE-TEST-MODIFIED");
        result.Data.Items.First().Quantity.Should().Be(100);
    }

    [Fact]
    public async Task DeleteShipmentDocument_DraftStatus_ReturnsOk()
    {
        // Arrange - Create a draft shipment
        var createDto = new CreateShipmentDocumentDto
        {
            Number = "SHP-DELETE-TEST",
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 25 }
            }
        };

        var createResponse = await _client.PostAsync("/api/shipmentdocuments", TestHelpers.CreateJsonContent(createDto));
        var createResult = await TestHelpers.DeserializeResponse<ApiResponse<ShipmentDocumentResponseDto>>(createResponse);
        var shipmentId = createResult!.Data!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/shipmentdocuments/{shipmentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteShipmentDocument_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/shipmentdocuments/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SignShipmentDocument_WithoutSufficientBalance_ReturnsBadRequest()
    {
        // Arrange - Create a shipment with large quantity (more than available balance)
        var createDto = new CreateShipmentDocumentDto
        {
            Number = "SHP-SIGN-INSUFFICIENT",
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 10000 } // Very large quantity
            }
        };

        var createResponse = await _client.PostAsync("/api/shipmentdocuments", TestHelpers.CreateJsonContent(createDto));
        var createResult = await TestHelpers.DeserializeResponse<ApiResponse<ShipmentDocumentResponseDto>>(createResponse);
        var shipmentId = createResult!.Data!.Id;

        // Act
        var response = await _client.PatchAsync($"/api/shipmentdocuments/{shipmentId}/sign", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("Insufficient balance");
    }

    [Fact]
    public async Task SignShipmentDocument_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.PatchAsync("/api/shipmentdocuments/999/sign", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RevokeShipmentDocument_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.PatchAsync("/api/shipmentdocuments/999/revoke", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllShipmentDocuments_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange - Create shipments with different statuses
        var draftDto = new CreateShipmentDocumentDto
        {
            Number = "SHP-DRAFT-FILTER",
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto> { new() { ResourceId = 1, UnitId = 1, Quantity = 10 } }
        };

        await _client.PostAsync("/api/shipmentdocuments", TestHelpers.CreateJsonContent(draftDto));

        // Act - Filter for draft status only
        var response = await _client.GetAsync($"/api/shipmentdocuments?statuses={ShipmentStatus.Draft}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<ShipmentDocumentResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        // All returned shipments should be draft status
        result.Data!.Should().OnlyContain(s => s.Status == ShipmentStatus.Draft);
        result.Data.Should().Contain(s => s.Number == "SHP-DRAFT-FILTER");
    }

    [Fact]
    public async Task GetAllShipmentDocuments_WithClientFilter_ReturnsFilteredResults()
    {
        // Arrange - Create a shipment for specific client
        var createDto = new CreateShipmentDocumentDto
        {
            Number = "SHP-CLIENT-FILTER",
            ClientId = 1,
            Date = DateTime.Today,
            Items = new List<CreateShipmentItemDto> { new() { ResourceId = 1, UnitId = 1, Quantity = 15 } }
        };

        await _client.PostAsync("/api/shipmentdocuments", TestHelpers.CreateJsonContent(createDto));

        // Act - Filter for specific client
        var response = await _client.GetAsync("/api/shipmentdocuments?clientIds=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<ShipmentDocumentResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        // All returned shipments should be for client 1
        result.Data!.Should().OnlyContain(s => s.ClientId == 1);
        result.Data.Should().Contain(s => s.Number == "SHP-CLIENT-FILTER");
    }
}
