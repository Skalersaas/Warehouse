using FluentAssertions;
using Tests.Helpers;
using Xunit;

namespace Tests.IntegrationTests;

public class DomainModelIntegrationTests
{
    [Fact]
    public void AllDomainEntities_ShouldImplementIModel()
    {
        // Arrange
        var domainAssembly = typeof(Domain.Models.Entities.Client).Assembly;
        var entityTypes = domainAssembly.GetTypes()
            .Where(t => t.Namespace == "Domain.Models.Entities" && t.IsClass && !t.IsAbstract)
            .ToList();

        // Act & Assert
        foreach (var entityType in entityTypes)
        {
            entityType.ShouldImplementInterface<Domain.Models.Interfaces.IModel>();
            entityType.ShouldHaveIModelProperties();
        }

        // Ensure we found some entities
        entityTypes.Should().NotBeEmpty("Domain should contain entity classes");
    }

    [Fact]
    public void ArchivableEntities_ShouldImplementIArchivable()
    {
        // Arrange
        var archivableEntities = new[]
        {
            typeof(Domain.Models.Entities.Client),
            typeof(Domain.Models.Entities.Resource),
            typeof(Domain.Models.Entities.Unit)
        };

        // Act & Assert
        foreach (var entityType in archivableEntities)
        {
            entityType.ShouldImplementInterface<Domain.Models.Interfaces.IArchivable>();
            entityType.ShouldHaveIArchivableProperties();
        }
    }

    [Fact]
    public void DocumentEntities_ShouldHaveAuditTrail()
    {
        // Arrange
        var documentEntities = new[]
        {
            typeof(Domain.Models.Entities.ReceiptDocument),
            typeof(Domain.Models.Entities.ShipmentDocument)
        };

        // Act & Assert
        foreach (var entityType in documentEntities)
        {
            entityType.ShouldFollowAuditTrailPattern(shouldHaveUpdatedAt: true);
        }
    }

    [Fact]
    public void ItemEntities_ShouldHaveRequiredForeignKeys()
    {
        // Arrange & Act & Assert
        typeof(Domain.Models.Entities.ReceiptItem).ShouldHaveForeignKeyFor("Document");
        typeof(Domain.Models.Entities.ReceiptItem).ShouldHaveForeignKeyFor("Resource");
        typeof(Domain.Models.Entities.ReceiptItem).ShouldHaveForeignKeyFor("Unit");

        typeof(Domain.Models.Entities.ShipmentItem).ShouldHaveForeignKeyFor("Document");
        typeof(Domain.Models.Entities.ShipmentItem).ShouldHaveForeignKeyFor("Resource");
        typeof(Domain.Models.Entities.ShipmentItem).ShouldHaveForeignKeyFor("Unit");

        typeof(Domain.Models.Entities.Balance).ShouldHaveForeignKeyFor("Resource");
        typeof(Domain.Models.Entities.Balance).ShouldHaveForeignKeyFor("Unit");

        typeof(Domain.Models.Entities.ShipmentDocument).ShouldHaveForeignKeyFor("Client");
    }

    [Fact]
    public void NavigationProperties_ShouldBeProperlyConfigured()
    {
        // Arrange & Act & Assert
        
        // Receipt Document navigation
        typeof(Domain.Models.Entities.ReceiptDocument)
            .ShouldHaveNavigationProperty<System.Collections.Generic.ICollection<Domain.Models.Entities.ReceiptItem>>("Items");

        // Shipment Document navigation
        typeof(Domain.Models.Entities.ShipmentDocument)
            .ShouldHaveNavigationProperty<Domain.Models.Entities.Client>("Client");
        typeof(Domain.Models.Entities.ShipmentDocument)
            .ShouldHaveNavigationProperty<System.Collections.Generic.ICollection<Domain.Models.Entities.ShipmentItem>>("Items");

        // Resource navigation
        typeof(Domain.Models.Entities.Resource)
            .ShouldHaveNavigationProperty<System.Collections.Generic.ICollection<Domain.Models.Entities.ReceiptItem>>("ReceiptItems");
        typeof(Domain.Models.Entities.Resource)
            .ShouldHaveNavigationProperty<System.Collections.Generic.ICollection<Domain.Models.Entities.ShipmentItem>>("ShipmentItems");
        typeof(Domain.Models.Entities.Resource)
            .ShouldHaveNavigationProperty<System.Collections.Generic.ICollection<Domain.Models.Entities.Balance>>("Balances");

        // Unit navigation
        typeof(Domain.Models.Entities.Unit)
            .ShouldHaveNavigationProperty<System.Collections.Generic.ICollection<Domain.Models.Entities.ReceiptItem>>("ReceiptItems");
        typeof(Domain.Models.Entities.Unit)
            .ShouldHaveNavigationProperty<System.Collections.Generic.ICollection<Domain.Models.Entities.ShipmentItem>>("ShipmentItems");
        typeof(Domain.Models.Entities.Unit)
            .ShouldHaveNavigationProperty<System.Collections.Generic.ICollection<Domain.Models.Entities.Balance>>("Balances");

        // Item navigation
        typeof(Domain.Models.Entities.ReceiptItem)
            .ShouldHaveNavigationProperty<Domain.Models.Entities.ReceiptDocument>("Document");
        typeof(Domain.Models.Entities.ReceiptItem)
            .ShouldHaveNavigationProperty<Domain.Models.Entities.Resource>("Resource");
        typeof(Domain.Models.Entities.ReceiptItem)
            .ShouldHaveNavigationProperty<Domain.Models.Entities.Unit>("Unit");

        typeof(Domain.Models.Entities.ShipmentItem)
            .ShouldHaveNavigationProperty<Domain.Models.Entities.ShipmentDocument>("Document");
        typeof(Domain.Models.Entities.ShipmentItem)
            .ShouldHaveNavigationProperty<Domain.Models.Entities.Resource>("Resource");
        typeof(Domain.Models.Entities.ShipmentItem)
            .ShouldHaveNavigationProperty<Domain.Models.Entities.Unit>("Unit");

        // Balance navigation
        typeof(Domain.Models.Entities.Balance)
            .ShouldHaveNavigationProperty<Domain.Models.Entities.Resource>("Resource");
        typeof(Domain.Models.Entities.Balance)
            .ShouldHaveNavigationProperty<Domain.Models.Entities.Unit>("Unit");
    }

    [Fact]
    public void AllEntities_ShouldBeSerializable()
    {
        // Arrange
        var entities = new object[]
        {
            TestDataBuilder.CreateValidClient(),
            TestDataBuilder.CreateValidResource(),
            TestDataBuilder.CreateValidUnit(),
            TestDataBuilder.CreateValidReceiptDocument(),
            TestDataBuilder.CreateValidShipmentDocument(),
            TestDataBuilder.CreateValidReceiptItem(),
            TestDataBuilder.CreateValidShipmentItem(),
            TestDataBuilder.CreateValidBalance()
        };

        // Act & Assert
        foreach (var entity in entities)
        {
            entity.ShouldBeSerializable();
        }
    }

    [Fact]
    public void DocumentWithItems_ShouldMaintainRelationshipIntegrity()
    {
        // Arrange
        var receiptDocument = TestDataBuilder.CreateReceiptDocumentWithItems(3);
        var shipmentDocument = TestDataBuilder.CreateShipmentDocumentWithItems(2);

        // Act & Assert
        receiptDocument.Items.Should().HaveCount(3);
        receiptDocument.Items.All(item => item.DocumentId == receiptDocument.Id).Should().BeTrue();

        shipmentDocument.Items.Should().HaveCount(2);
        shipmentDocument.Items.All(item => item.DocumentId == shipmentDocument.Id).Should().BeTrue();
    }

    [Fact]
    public void ResourceWithRelatedEntities_ShouldMaintainRelationshipIntegrity()
    {
        // Arrange & Act
        var resource = TestDataBuilder.CreateResourceWithRelatedEntities();

        // Assert
        resource.ReceiptItems.Should().HaveCount(2);
        resource.ShipmentItems.Should().HaveCount(2);
        resource.Balances.Should().HaveCount(2);

        resource.ReceiptItems.All(item => item.ResourceId == resource.Id).Should().BeTrue();
        resource.ShipmentItems.All(item => item.ResourceId == resource.Id).Should().BeTrue();
        resource.Balances.All(balance => balance.ResourceId == resource.Id).Should().BeTrue();
    }
}
