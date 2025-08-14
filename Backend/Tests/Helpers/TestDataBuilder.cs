using AutoFixture;
using Domain.Models.Entities;

namespace Tests.Helpers;

public static class TestDataBuilder
{
    private static readonly Fixture _fixture = FixtureExtensions.CreateDomainFixture();

    public static Client CreateValidClient(
        string? name = null,
        string? address = null,
        bool isArchived = false)
    {
        return new Client
        {
            Id = _fixture.Create<int>(),
            Name = name ?? _fixture.Create<string>(),
            Address = address ?? _fixture.Create<string>(),
            IsArchived = isArchived,
            CreatedAt = DateTime.UtcNow.AddDays(-_fixture.Create<int>() % 30),
            UpdatedAt = _fixture.Create<bool>() ? DateTime.UtcNow : null
        };
    }

    public static Resource CreateValidResource(
        string? name = null,
        bool isArchived = false)
    {
        return new Resource
        {
            Id = _fixture.Create<int>(),
            Name = name ?? _fixture.Create<string>(),
            IsArchived = isArchived,
            CreatedAt = DateTime.UtcNow.AddDays(-_fixture.Create<int>() % 30),
            UpdatedAt = DateTime.UtcNow.AddDays(-_fixture.Create<int>() % 15),
            ReceiptItems = new List<ReceiptItem>(),
            ShipmentItems = new List<ShipmentItem>(),
            Balances = new List<Balance>()
        };
    }

    public static Unit CreateValidUnit(
        string? name = null,
        bool isArchived = false)
    {
        return new Unit
        {
            Id = _fixture.Create<int>(),
            Name = name ?? _fixture.Create<string>(),
            IsArchived = isArchived,
            CreatedAt = DateTime.UtcNow.AddDays(-_fixture.Create<int>() % 30),
            UpdatedAt = _fixture.Create<bool>() ? DateTime.UtcNow : null,
            ReceiptItems = new List<ReceiptItem>(),
            ShipmentItems = new List<ShipmentItem>(),
            Balances = new List<Balance>()
        };
    }

    public static ReceiptDocument CreateValidReceiptDocument(
        string? number = null,
        DateTime? date = null)
    {
        return new ReceiptDocument
        {
            Id = _fixture.Create<int>(),
            Number = number ?? _fixture.Create<string>(),
            Date = date ?? _fixture.Create<DateTime>(),
            CreatedAt = DateTime.UtcNow.AddDays(-_fixture.Create<int>() % 30),
            UpdatedAt = _fixture.Create<bool>() ? DateTime.UtcNow : null,
            Items = new List<ReceiptItem>()
        };
    }

    public static ShipmentDocument CreateValidShipmentDocument(
        string? number = null,
        int? clientId = null,
        DateTime? date = null)
    {
        return new ShipmentDocument
        {
            Id = _fixture.Create<int>(),
            Number = number ?? _fixture.Create<string>(),
            ClientId = clientId ?? _fixture.Create<int>(),
            Date = date ?? _fixture.Create<DateTime>(),
            Status = _fixture.Create<Domain.Models.Enums.ShipmentStatus>(),
            CreatedAt = DateTime.UtcNow.AddDays(-_fixture.Create<int>() % 30),
            UpdatedAt = _fixture.Create<bool>() ? DateTime.UtcNow : null,
            Items = new List<ShipmentItem>()
        };
    }

    public static ReceiptItem CreateValidReceiptItem(
        int? documentId = null,
        int? resourceId = null,
        int? unitId = null,
        decimal? quantity = null)
    {
        return new ReceiptItem
        {
            Id = _fixture.Create<int>(),
            DocumentId = documentId ?? _fixture.Create<int>(),
            ResourceId = resourceId ?? _fixture.Create<int>(),
            UnitId = unitId ?? _fixture.Create<int>(),
            Quantity = quantity ?? _fixture.Create<decimal>()
        };
    }

    public static ShipmentItem CreateValidShipmentItem(
        int? documentId = null,
        int? resourceId = null,
        int? unitId = null,
        decimal? quantity = null)
    {
        return new ShipmentItem
        {
            Id = _fixture.Create<int>(),
            DocumentId = documentId ?? _fixture.Create<int>(),
            ResourceId = resourceId ?? _fixture.Create<int>(),
            UnitId = unitId ?? _fixture.Create<int>(),
            Quantity = quantity ?? _fixture.Create<decimal>()
        };
    }

    public static Balance CreateValidBalance(
        int? resourceId = null,
        int? unitId = null,
        decimal? quantity = null)
    {
        return new Balance
        {
            Id = _fixture.Create<int>(),
            ResourceId = resourceId ?? _fixture.Create<int>(),
            UnitId = unitId ?? _fixture.Create<int>(),
            Quantity = quantity ?? _fixture.Create<decimal>()
        };
    }

    public static ReceiptDocument CreateReceiptDocumentWithItems(int itemCount = 3)
    {
        var document = CreateValidReceiptDocument();
        var items = new List<ReceiptItem>();

        for (int i = 0; i < itemCount; i++)
        {
            items.Add(CreateValidReceiptItem(documentId: document.Id));
        }

        document.Items = items;
        return document;
    }

    public static ShipmentDocument CreateShipmentDocumentWithItems(int itemCount = 3)
    {
        var document = CreateValidShipmentDocument();
        var items = new List<ShipmentItem>();

        for (int i = 0; i < itemCount; i++)
        {
            items.Add(CreateValidShipmentItem(documentId: document.Id));
        }

        document.Items = items;
        return document;
    }

    public static Resource CreateResourceWithRelatedEntities()
    {
        var resource = CreateValidResource();
        
        resource.ReceiptItems = new List<ReceiptItem>
        {
            CreateValidReceiptItem(resourceId: resource.Id),
            CreateValidReceiptItem(resourceId: resource.Id)
        };

        resource.ShipmentItems = new List<ShipmentItem>
        {
            CreateValidShipmentItem(resourceId: resource.Id),
            CreateValidShipmentItem(resourceId: resource.Id)
        };

        resource.Balances = new List<Balance>
        {
            CreateValidBalance(resourceId: resource.Id),
            CreateValidBalance(resourceId: resource.Id)
        };

        return resource;
    }
}
