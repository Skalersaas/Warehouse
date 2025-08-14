using AutoFixture;
using AutoFixture.Kernel;

namespace Tests.Helpers;

/// <summary>
/// Configures AutoFixture to handle domain entities with circular references
/// </summary>
public static class FixtureExtensions
{
    /// <summary>
    /// Creates a fixture configured for domain entities with circular reference handling
    /// </summary>
    public static Fixture CreateDomainFixture()
    {
        var fixture = new Fixture();
        
        // Remove ThrowingRecursionBehavior and add OmitOnRecursionBehavior
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Configure specific customizations for domain entities
        fixture.Customize<Domain.Models.Entities.Client>(c => c
            .Without(x => x.Id) // Let EF handle IDs
            .Do(x => x.CreatedAt = DateTime.UtcNow)
            .Do(x => x.UpdatedAt = null));

        fixture.Customize<Domain.Models.Entities.Resource>(c => c
            .Without(x => x.Id)
            .Without(x => x.ReceiptItems) // Avoid circular references
            .Without(x => x.ShipmentItems)
            .Without(x => x.Balances)
            .Do(x => x.CreatedAt = DateTime.UtcNow)
            .Do(x => x.UpdatedAt = DateTime.UtcNow));

        fixture.Customize<Domain.Models.Entities.Unit>(c => c
            .Without(x => x.Id)
            .Without(x => x.ReceiptItems) // Avoid circular references
            .Without(x => x.ShipmentItems)
            .Without(x => x.Balances)
            .Do(x => x.CreatedAt = DateTime.UtcNow)
            .Do(x => x.UpdatedAt = null));

        fixture.Customize<Domain.Models.Entities.ReceiptDocument>(c => c
            .Without(x => x.Id)
            .Without(x => x.Items) // Avoid circular references
            .Do(x => x.CreatedAt = DateTime.UtcNow)
            .Do(x => x.UpdatedAt = null)
            .Do(x => x.Items = new List<Domain.Models.Entities.ReceiptItem>()));

        fixture.Customize<Domain.Models.Entities.ShipmentDocument>(c => c
            .Without(x => x.Id)
            .Without(x => x.Client) // Avoid circular references
            .Without(x => x.Items)
            .Do(x => x.CreatedAt = DateTime.UtcNow)
            .Do(x => x.UpdatedAt = null)
            .Do(x => x.Items = new List<Domain.Models.Entities.ShipmentItem>()));

        fixture.Customize<Domain.Models.Entities.ReceiptItem>(c => c
            .Without(x => x.Id)
            .Without(x => x.Document) // Avoid circular references
            .Without(x => x.Resource)
            .Without(x => x.Unit)
            .With(x => x.Quantity, () => Math.Abs(fixture.Create<decimal>())));

        fixture.Customize<Domain.Models.Entities.ShipmentItem>(c => c
            .Without(x => x.Id)
            .Without(x => x.Document) // Avoid circular references
            .Without(x => x.Resource)
            .Without(x => x.Unit)
            .With(x => x.Quantity, () => Math.Abs(fixture.Create<decimal>())));

        fixture.Customize<Domain.Models.Entities.Balance>(c => c
            .Without(x => x.Id)
            .Without(x => x.Resource) // Avoid circular references
            .Without(x => x.Unit)
            .With(x => x.Quantity, () => Math.Abs(fixture.Create<decimal>())));

        return fixture;
    }

    /// <summary>
    /// Creates a fixture with minimal configuration for simple scenarios
    /// </summary>
    public static Fixture CreateSimpleFixture()
    {
        var fixture = new Fixture();
        
        // Remove ThrowingRecursionBehavior and add OmitOnRecursionBehavior
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }
}
