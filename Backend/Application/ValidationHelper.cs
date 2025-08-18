using Domain.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Utilities.Responses;

namespace Application;

public static class DocumentValidationHelper
{
    /// <summary>
    /// Validates items for existence and archive status
    /// </summary>
    /// <typeparam name="TItemDto">The item DTO type</typeparam>
    /// <param name="context">Database context</param>
    /// <param name="items">Collection of items to validate</param>
    /// <param name="getResourceId">Function to extract ResourceId from item</param>
    /// <param name="getUnitId">Function to extract UnitId from item</param>
    /// <param name="getQuantity">Function to extract Quantity from item (optional)</param>
    /// <returns>Validation result</returns>
    public static async Task<Result> ValidateItemsAsync<TItemDto>(
        ApplicationContext context,
        IEnumerable<TItemDto>? items,
        Func<TItemDto, int> getResourceId,
        Func<TItemDto, int> getUnitId,
        Func<TItemDto, decimal>? getQuantity = null)
    {
        if (items == null || !items.Any())
            return Result.ErrorResult("Document must contain at least one item");

        var itemsList = items.ToList();

        // Validate quantities if function provided
        if (getQuantity != null)
        {
            var quantityValidation = ValidateItemQuantities(itemsList, getQuantity);
            if (!quantityValidation.Success)
                return quantityValidation;
        }

        // Get all unique resource and unit IDs
        var resourceIds = itemsList.Select(getResourceId).Distinct().ToList();
        var unitIds = itemsList.Select(getUnitId).Distinct().ToList();

        // Validate resources exist and are not archived
        var resourceValidation = await ValidateResourcesAsync(context, resourceIds);
        if (!resourceValidation.Success)
            return resourceValidation;

        // Validate units exist and are not archived
        var unitValidation = await ValidateUnitsAsync(context, unitIds);
        if (!unitValidation.Success)
            return unitValidation;

        return Result.SuccessResult();
    }

    /// <summary>
    /// Validates that all item quantities are positive
    /// </summary>
    public static Result ValidateItemQuantities<TItemDto>(
        List<TItemDto> items,
        Func<TItemDto, decimal> getQuantity)
    {
        for (int i = 0; i < items.Count; i++)
        {
            var quantity = getQuantity(items[i]);
            if (quantity <= 0)
                return Result.ErrorResult($"Item {i + 1}: Quantity must be greater than 0");
        }
        return Result.SuccessResult();
    }

    /// <summary>
    /// Validates resources exist and are not archived
    /// </summary>
    public static async Task<Result> ValidateResourcesAsync(ApplicationContext context, List<int> resourceIds)
    {
        if (!resourceIds.Any()) return Result.SuccessResult();

        var resources = await context.Resources
            .Where(r => resourceIds.Contains(r.Id))
            .ToListAsync();

        // Check for non-existing resources
        var existingResourceIds = resources.Select(r => r.Id).ToHashSet();
        var missingResourceIds = resourceIds.Where(id => !existingResourceIds.Contains(id)).ToList();

        if (missingResourceIds.Any())
            return Result.ErrorResult($"Resources not found: {string.Join(", ", missingResourceIds)}");

        // Check for archived resources
        var archivedResources = resources
            .Where(r => r is IArchivable archivable && archivable.IsArchived)
            .Select(r => r.Name)
            .ToList();

        if (archivedResources.Any())
            return Result.ErrorResult($"Cannot use archived resources: {string.Join(", ", archivedResources)}");

        return Result.SuccessResult();
    }

    /// <summary>
    /// Validates units exist and are not archived
    /// </summary>
    public static async Task<Result> ValidateUnitsAsync(ApplicationContext context, List<int> unitIds)
    {
        if (!unitIds.Any()) return Result.SuccessResult();

        var units = await context.Units
            .Where(u => unitIds.Contains(u.Id))
            .ToListAsync();

        // Check for non-existing units
        var existingUnitIds = units.Select(u => u.Id).ToHashSet();
        var missingUnitIds = unitIds.Where(id => !existingUnitIds.Contains(id)).ToList();

        if (missingUnitIds.Any())
            return Result.ErrorResult($"Units not found: {string.Join(", ", missingUnitIds)}");

        // Check for archived units
        var archivedUnits = units
            .Where(u => u is IArchivable archivable && archivable.IsArchived)
            .Select(u => u.Name)
            .ToList();

        if (archivedUnits.Any())
            return Result.ErrorResult($"Cannot use archived units: {string.Join(", ", archivedUnits)}");

        return Result.SuccessResult();
    }

    /// <summary>
    /// Validates client exists and is not archived
    /// </summary>
    public static async Task<Result> ValidateClientAsync(ApplicationContext context, int clientId)
    {
        if (clientId <= 0)
            return Result.ErrorResult("Invalid client");

        var client = await context.Clients
            .FirstOrDefaultAsync(c => c.Id == clientId);

        if (client == null)
            return Result.ErrorResult($"Client with ID {clientId} not found");

        if (client is IArchivable archivable && archivable.IsArchived)
            return Result.ErrorResult($"Client: {client.Name} is archived");

        return Result.SuccessResult();
    }

    /// <summary>
    /// Optimized validation for resources and units in single query
    /// </summary>
    public static async Task<Result> ValidateResourcesAndUnitsAsync(
        ApplicationContext context,
        List<int> resourceIds,
        List<int> unitIds)
    {
        var errors = new List<string>();

        // Validate resources and units in parallel
        var resourceTask = ValidateResourcesAsync(context, resourceIds);
        var unitTask = ValidateUnitsAsync(context, unitIds);

        var results = await Task.WhenAll(resourceTask, unitTask);

        foreach (var result in results)
        {
            if (!result.Success)
                errors.Add(result.Message);
        }

        return errors.Any()
            ? Result.ErrorResult(string.Join("; ", errors))
            : Result.SuccessResult();
    }
}