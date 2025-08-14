using Domain.Models.Entities;
using FluentAssertions;
using Tests.Helpers;
using Xunit;

namespace Tests.UnitTests.Domain.Entities;

public class EntityRelationshipTests
{
    [Fact]
    public void ReceiptDocument_WithItems_ShouldMaintainConsistentRelationships()
    {
        // Arrange
        var document = TestDataBuilder.CreateValidReceiptDocument();
        var resource = TestDataBuilder.CreateValidResource();
        var unit = TestDataBuilder.CreateValidUnit();

        var item1 = new ReceiptItem
        {
            Id = 1,
            DocumentId = document.Id,
            ResourceId = resource.Id,
            UnitId = unit.Id,
            Quantity = 10.5m,
            Document = document,
            Resource = resource,
            Unit = unit
        };

        var item2 = new ReceiptItem
        {
            Id = 2,
            DocumentId = document.Id,
            ResourceId = resource.Id,
            UnitId = unit.Id,
            Quantity = 5.25m,
            Document = document,
            Resource = resource,
            Unit = unit
        };

        // Act
        document.Items.Add(item1);
        document.Items.Add(item2);

        // Assert
        document.Items.Should().HaveCount(2);
        document.Items.Should().Contain(item1);
        document.Items.Should().Contain(item2);
        
        // Verify bidirectional relationships
        item1.Document.Should().BeSameAs(document);
        item2.Document.Should().BeSameAs(document);
        
        // Verify foreign key consistency
        item1.DocumentId.Should().Be(document.Id);
        item2.DocumentId.Should().Be(document.Id);
    }

    [Fact]
    public void ShipmentDocument_WithClientAndItems_ShouldMaintainConsistentRelationships()
    {
        // Arrange
        var client = TestDataBuilder.CreateValidClient();
        var document = TestDataBuilder.CreateValidShipmentDocument(clientId: client.Id);
        var resource = TestDataBuilder.CreateValidResource();
        var unit = TestDataBuilder.CreateValidUnit();

        document.Client = client;

        var item = new ShipmentItem
        {
            Id = 1,
            DocumentId = document.Id,
            ResourceId = resource.Id,
            UnitId = unit.Id,
            Quantity = 15.75m,
            Document = document,
            Resource = resource,
            Unit = unit
        };

        // Act
        document.Items.Add(item);

        // Assert
        document.Client.Should().BeSameAs(client);
        document.ClientId.Should().Be(client.Id);
        
        document.Items.Should().HaveCount(1);
        document.Items.Should().Contain(item);
        
        item.Document.Should().BeSameAs(document);
        item.DocumentId.Should().Be(document.Id);
    }

    [Fact]
    public void Resource_WithMultipleRelatedEntities_ShouldMaintainAllRelationships()
    {
        // Arrange
        var resource = TestDataBuilder.CreateValidResource();
        var unit = TestDataBuilder.CreateValidUnit();
        
        var receiptItem = TestDataBuilder.CreateValidReceiptItem(resourceId: resource.Id, unitId: unit.Id);
        var shipmentItem = TestDataBuilder.CreateValidShipmentItem(resourceId: resource.Id, unitId: unit.Id);
        var balance = TestDataBuilder.CreateValidBalance(resourceId: resource.Id, unitId: unit.Id);

        receiptItem.Resource = resource;
        receiptItem.Unit = unit;
        shipmentItem.Resource = resource;
        shipmentItem.Unit = unit;
        balance.Resource = resource;
        balance.Unit = unit;

        // Act
        resource.ReceiptItems = new List<ReceiptItem> { receiptItem };
        resource.ShipmentItems = new List<ShipmentItem> { shipmentItem };
        resource.Balances = new List<Balance> { balance };

        unit.ReceiptItems = new List<ReceiptItem> { receiptItem };
        unit.ShipmentItems = new List<ShipmentItem> { shipmentItem };
        unit.Balances = new List<Balance> { balance };

        // Assert
        // Resource relationships
        resource.ReceiptItems.Should().HaveCount(1);
        resource.ReceiptItems.Should().Contain(receiptItem);
        resource.ShipmentItems.Should().HaveCount(1);
        resource.ShipmentItems.Should().Contain(shipmentItem);
        resource.Balances.Should().HaveCount(1);
        resource.Balances.Should().Contain(balance);

        // Unit relationships
        unit.ReceiptItems.Should().HaveCount(1);
        unit.ReceiptItems.Should().Contain(receiptItem);
        unit.ShipmentItems.Should().HaveCount(1);
        unit.ShipmentItems.Should().Contain(shipmentItem);
        unit.Balances.Should().HaveCount(1);
        unit.Balances.Should().Contain(balance);

        // Bidirectional verification
        receiptItem.Resource.Should().BeSameAs(resource);
        receiptItem.Unit.Should().BeSameAs(unit);
        shipmentItem.Resource.Should().BeSameAs(resource);
        shipmentItem.Unit.Should().BeSameAs(unit);
        balance.Resource.Should().BeSameAs(resource);
        balance.Unit.Should().BeSameAs(unit);

        // Foreign key consistency
        receiptItem.ResourceId.Should().Be(resource.Id);
        receiptItem.UnitId.Should().Be(unit.Id);
        shipmentItem.ResourceId.Should().Be(resource.Id);
        shipmentItem.UnitId.Should().Be(unit.Id);
        balance.ResourceId.Should().Be(resource.Id);
        balance.UnitId.Should().Be(unit.Id);
    }

    [Fact]
    public void Balance_ShouldRepresentCurrentStock()
    {
        // Arrange
        var resource = TestDataBuilder.CreateValidResource("Test Product");
        var unit = TestDataBuilder.CreateValidUnit("kg");
        
        var balance = new Balance
        {
            Id = 1,
            ResourceId = resource.Id,
            UnitId = unit.Id,
            Quantity = 100.50m,
            Resource = resource,
            Unit = unit
        };

        // Act & Assert
        balance.Quantity.Should().Be(100.50m);
        balance.Resource.Should().BeSameAs(resource);
        balance.Unit.Should().BeSameAs(unit);
        balance.ResourceId.Should().Be(resource.Id);
        balance.UnitId.Should().Be(unit.Id);
    }

    [Fact]
    public void Documents_ShouldSupportComplexScenarios()
    {
        // Arrange - Create a complex scenario with multiple documents sharing resources
        var client1 = TestDataBuilder.CreateValidClient("Client A");
        var client2 = TestDataBuilder.CreateValidClient("Client B");
        
        var resource1 = TestDataBuilder.CreateValidResource("Product 1");
        var resource2 = TestDataBuilder.CreateValidResource("Product 2");
        
        var unit = TestDataBuilder.CreateValidUnit("pieces");

        // Receipt document with items
        var receiptDoc = TestDataBuilder.CreateValidReceiptDocument("REC-001");
        var receiptItem1 = new ReceiptItem
        {
            Id = 1,
            DocumentId = receiptDoc.Id,
            ResourceId = resource1.Id,
            UnitId = unit.Id,
            Quantity = 100m
        };
        var receiptItem2 = new ReceiptItem
        {
            Id = 2,
            DocumentId = receiptDoc.Id,
            ResourceId = resource2.Id,
            UnitId = unit.Id,
            Quantity = 50m
        };

        // Shipment documents
        var shipmentDoc1 = TestDataBuilder.CreateValidShipmentDocument("SHIP-001", client1.Id);
        var shipmentItem1 = new ShipmentItem
        {
            Id = 1,
            DocumentId = shipmentDoc1.Id,
            ResourceId = resource1.Id,
            UnitId = unit.Id,
            Quantity = 30m
        };

        var shipmentDoc2 = TestDataBuilder.CreateValidShipmentDocument("SHIP-002", client2.Id);
        var shipmentItem2 = new ShipmentItem
        {
            Id = 2,
            DocumentId = shipmentDoc2.Id,
            ResourceId = resource1.Id,
            UnitId = unit.Id,
            Quantity = 20m
        };

        // Act
        receiptDoc.Items.Add(receiptItem1);
        receiptDoc.Items.Add(receiptItem2);
        shipmentDoc1.Items.Add(shipmentItem1);
        shipmentDoc2.Items.Add(shipmentItem2);

        // Assert
        // Receipt document
        receiptDoc.Items.Should().HaveCount(2);
        receiptDoc.Items.Sum(i => i.Quantity).Should().Be(150m);

        // Shipment documents
        shipmentDoc1.Items.Should().HaveCount(1);
        shipmentDoc1.Items.Sum(i => i.Quantity).Should().Be(30m);
        
        shipmentDoc2.Items.Should().HaveCount(1);
        shipmentDoc2.Items.Sum(i => i.Quantity).Should().Be(20m);

        // Resource 1 should have total movements
        var resource1TotalReceived = receiptDoc.Items.Where(i => i.ResourceId == resource1.Id).Sum(i => i.Quantity);
        var resource1TotalShipped = new[] { shipmentDoc1, shipmentDoc2 }
            .SelectMany(d => d.Items)
            .Where(i => i.ResourceId == resource1.Id)
            .Sum(i => i.Quantity);

        resource1TotalReceived.Should().Be(100m);
        resource1TotalShipped.Should().Be(50m); // 30 + 20

        // Theoretical balance for resource1 would be 50m (100 - 50)
        var theoreticalBalance = resource1TotalReceived - resource1TotalShipped;
        theoreticalBalance.Should().Be(50m);
    }

    [Fact]
    public void ArchivableEntities_ShouldSupportArchivingWorkflow()
    {
        // Arrange
        var client = TestDataBuilder.CreateValidClient();
        var resource = TestDataBuilder.CreateValidResource();
        var unit = TestDataBuilder.CreateValidUnit();

        // Act - Archive entities
        client.IsArchived = true;
        resource.IsArchived = true;
        unit.IsArchived = true;

        // Assert
        client.IsArchived.Should().BeTrue();
        resource.IsArchived.Should().BeTrue();
        unit.IsArchived.Should().BeTrue();

        // Verify they still maintain their other properties
        client.Name.Should().NotBeNullOrEmpty();
        resource.Name.Should().NotBeNullOrEmpty();
        unit.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void EntityCollections_ShouldSupportLINQOperations()
    {
        // Arrange
        var document = TestDataBuilder.CreateReceiptDocumentWithItems(5);
        
        // Act & Assert - LINQ operations should work
        var totalQuantity = document.Items.Sum(i => i.Quantity);
        var itemCount = document.Items.Count();
        var firstItem = document.Items.FirstOrDefault();
        var hasItems = document.Items.Any();

        totalQuantity.Should().BeGreaterThan(0);
        itemCount.Should().Be(5);
        firstItem.Should().NotBeNull();
        hasItems.Should().BeTrue();
    }
}
