using Application.DTOs.ReceiptDocument;
using Application.DTOs.ReceiptItem;
using FluentAssertions;
using System.Net;
using Tests.Infrastructure;
using Utilities.Responses;
using Xunit;

namespace Tests.Integration;

public class ReceiptDocumentsControllerTests : IClassFixture<WarehouseTestFactory>
{
    private readonly HttpClient _client;
    private readonly WarehouseTestFactory _factory;

    public ReceiptDocumentsControllerTests(WarehouseTestFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateReceiptDocument_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateReceiptDocumentDto
        {
            Number = "RCP-TEST-001",
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 100 }
            }
        };

        // Act
        var response = await _client.PostAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<ReceiptDocumentResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Number.Should().Be(createDto.Number);
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().Quantity.Should().Be(100);
    }

    [Fact]
    public async Task CreateReceiptDocument_DuplicateNumber_ReturnsBadRequest()
    {
        // Arrange - First create a receipt
        var firstDto = new CreateReceiptDocumentDto
        {
            Number = "RCP-DUPLICATE",
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 50 }
            }
        };
        await _client.PostAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(firstDto));

        // Try to create another with same number
        var duplicateDto = new CreateReceiptDocumentDto
        {
            Number = "RCP-DUPLICATE", // Same number
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 75 }
            }
        };

        // Act
        var response = await _client.PostAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(duplicateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateReceiptDocument_InvalidResource_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateReceiptDocumentDto
        {
            Number = "RCP-INVALID-RESOURCE",
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>
            {
                new() { ResourceId = 999, UnitId = 1, Quantity = 100 } // Invalid resource
            }
        };

        // Act
        var response = await _client.PostAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("Resource with ID 999 not found");
    }

    [Fact]
    public async Task CreateReceiptDocument_InvalidUnit_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateReceiptDocumentDto
        {
            Number = "RCP-INVALID-UNIT",
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>
            {
                new() { ResourceId = 1, UnitId = 999, Quantity = 100 } // Invalid unit
            }
        };

        // Act
        var response = await _client.PostAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("Unit with ID 999 not found");
    }

    [Fact]
    public async Task CreateReceiptDocument_EmptyItems_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateReceiptDocumentDto
        {
            Number = "RCP-EMPTY",
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>() // Empty items
        };

        // Act
        var response = await _client.PostAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(createDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetReceiptDocument_AfterCreation_ReturnsCorrectData()
    {
        // Arrange - Create a receipt first
        var createDto = new CreateReceiptDocumentDto
        {
            Number = "RCP-GET-TEST",
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 150 }
            }
        };

        var createResponse = await _client.PostAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(createDto));
        var createResult = await TestHelpers.DeserializeResponse<ApiResponse<ReceiptDocumentResponseDto>>(createResponse);
        var receiptId = createResult!.Data!.Id;

        // Act
        var response = await _client.GetAsync($"/api/receiptdocuments/{receiptId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<ReceiptDocumentResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(receiptId);
        result.Data.Number.Should().Be("RCP-GET-TEST");
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().Quantity.Should().Be(150);
    }

    [Fact]
    public async Task GetReceiptDocument_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/receiptdocuments/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllReceiptDocuments_ReturnsOk()
    {
        // Arrange - Create a receipt to ensure we have data
        var createDto = new CreateReceiptDocumentDto
        {
            Number = "RCP-GETALL-TEST",
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 200 }
            }
        };
        await _client.PostAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(createDto));

        // Act
        var response = await _client.GetAsync("/api/receiptdocuments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<ReceiptDocumentResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().NotBeEmpty();
        result.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateReceiptDocument_ValidData_ReturnsOk()
    {
        // Arrange - Create a receipt first
        var createDto = new CreateReceiptDocumentDto
        {
            Number = "RCP-UPDATE-TEST",
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 100 }
            }
        };

        var createResponse = await _client.PostAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(createDto));
        var createResult = await TestHelpers.DeserializeResponse<ApiResponse<ReceiptDocumentResponseDto>>(createResponse);
        var receiptId = createResult!.Data!.Id;
        var itemId = createResult.Data.Items.First().Id;

        var updateDto = new UpdateReceiptDocumentDto
        {
            Id = receiptId,
            Number = "RCP-UPDATE-TEST-MODIFIED",
            Date = DateTime.Today.AddDays(1),
            Items = new List<UpdateReceiptItemDto>
            {
                new() { Id = itemId, ResourceId = 1, UnitId = 1, Quantity = 200 }
            }
        };

        // Act
        var response = await _client.PutAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(updateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<ReceiptDocumentResponseDto>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Number.Should().Be("RCP-UPDATE-TEST-MODIFIED");
        result.Data.Items.First().Quantity.Should().Be(200);
    }

    [Fact]
    public async Task UpdateReceiptDocument_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateReceiptDocumentDto
        {
            Id = 999,
            Number = "RCP-NON-EXISTING",
            Date = DateTime.Today,
            Items = new List<UpdateReceiptItemDto>
            {
                new() { Id = 1, ResourceId = 1, UnitId = 1, Quantity = 100 }
            }
        };

        // Act
        var response = await _client.PutAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(updateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteReceiptDocument_WithSufficientBalance_ReturnsOk()
    {
        // Arrange - Create a small receipt that can be safely deleted
        var createDto = new CreateReceiptDocumentDto
        {
            Number = "RCP-DELETE-TEST",
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto>
            {
                new() { ResourceId = 1, UnitId = 1, Quantity = 1 } // Small quantity
            }
        };

        var createResponse = await _client.PostAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(createDto));
        var createResult = await TestHelpers.DeserializeResponse<ApiResponse<ReceiptDocumentResponseDto>>(createResponse);
        var receiptId = createResult!.Data!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/receiptdocuments/{receiptId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<object>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteReceiptDocument_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/receiptdocuments/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllReceiptDocuments_WithDateFilter_ReturnsFilteredResults()
    {
        // Arrange - Create receipts with different dates
        var todayDto = new CreateReceiptDocumentDto
        {
            Number = "RCP-TODAY",
            Date = DateTime.Today,
            Items = new List<CreateReceiptItemDto> { new() { ResourceId = 1, UnitId = 1, Quantity = 10 } }
        };

        var yesterdayDto = new CreateReceiptDocumentDto
        {
            Number = "RCP-YESTERDAY",
            Date = DateTime.Today.AddDays(-1),
            Items = new List<CreateReceiptItemDto> { new() { ResourceId = 1, UnitId = 1, Quantity = 10 } }
        };

        await _client.PostAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(todayDto));
        await _client.PostAsync("/api/receiptdocuments", TestHelpers.CreateJsonContent(yesterdayDto));

        // Act - Filter for today only
        var response = await _client.GetAsync($"/api/receiptdocuments?dateFrom={DateTime.Today:yyyy-MM-dd}&dateTo={DateTime.Today:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await TestHelpers.DeserializeResponse<ApiResponse<IEnumerable<ReceiptDocumentResponseDto>>>(response);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        
        // Should contain today's receipt but not yesterday's
        result.Data!.Should().Contain(r => r.Number == "RCP-TODAY");
        result.Data.Should().NotContain(r => r.Number == "RCP-YESTERDAY");
    }
}
