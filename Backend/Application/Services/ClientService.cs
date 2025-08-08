using Application.DTOs.Client;
using Application.Interfaces;
using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Data.Interfaces;
using Utilities.DataManipulation;

namespace Application.Services;

public class ClientService : IClientService
{
    private readonly IArchivableRepository<Client> _clientRepository;
    private readonly ApplicationContext _context;
    private readonly ILogger<ClientService> _logger;

    public ClientService(IArchivableRepository<Client> clientRepository, ApplicationContext context, ILogger<ClientService> logger)
    {
        _clientRepository = clientRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, ClientResponseDto? Data, string? ErrorMessage)> CreateAsync(CreateClientDto dto)
    {
        try
        {
            // Business validation
            if (await ExistsByNameAsync(dto.Name))
            {
                return (false, null, "A client with this name already exists");
            }

            var client = new Client
            {
                Name = dto.Name.Trim(),
                Address = dto.Address.Trim(),
                IsArchived = false
            };

            var created = await _clientRepository.CreateAsync(client);
            if (created == null)
            {
                return (false, null, "Failed to create client");
            }

            var response = MapToResponseDto(created);
            _logger.LogInformation("Client created successfully with ID: {ClientId}", created.Id);
            
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client with name: {Name}", dto.Name);
            return (false, null, "An error occurred while creating the client");
        }
    }

    public async Task<(bool Success, ClientResponseDto? Data, string? ErrorMessage)> GetByIdAsync(int id)
    {
        try
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
            {
                return (false, null, "Client not found");
            }

            var response = MapToResponseDto(client);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving client with ID: {ClientId}", id);
            return (false, null, "An error occurred while retrieving the client");
        }
    }

    public async Task<(IEnumerable<ClientResponseDto> Data, int TotalCount)> GetAllAsync(SearchModel? searchModel = null)
    {
        try
        {
            var (clients, totalCount) = await _clientRepository.QueryBy(searchModel);
            var response = clients.Select(MapToResponseDto);
            return (response, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clients");
            return (Enumerable.Empty<ClientResponseDto>(), 0);
        }
    }

    public async Task<(bool Success, ClientResponseDto? Data, string? ErrorMessage)> UpdateAsync(UpdateClientDto dto)
    {
        try
        {
            var existing = await _clientRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                return (false, null, "Client not found");
            }

            // Business validation
            if (await ExistsByNameAsync(dto.Name, dto.Id))
            {
                return (false, null, "A client with this name already exists");
            }

            existing.Name = dto.Name.Trim();
            existing.Address = dto.Address.Trim();
            existing.IsArchived = dto.IsArchived;
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _clientRepository.UpdateAsync(existing);
            var response = MapToResponseDto(updated);
            
            _logger.LogInformation("Client updated successfully with ID: {ClientId}", dto.Id);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating client with ID: {ClientId}", dto.Id);
            return (false, null, "An error occurred while updating the client");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        try
        {
            var success = await _clientRepository.DeleteAsync(id);
            if (!success)
            {
                return (false, "Client not found");
            }

            _logger.LogInformation("Client deleted successfully with ID: {ClientId}", id);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting client with ID: {ClientId}", id);
            return (false, "An error occurred while deleting the client");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ArchiveAsync(int id)
    {
        try
        {
            var success = await _clientRepository.ArchiveAsync(id);
            if (!success)
            {
                return (false, "Client not found");
            }

            _logger.LogInformation("Client archived successfully with ID: {ClientId}", id);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving client with ID: {ClientId}", id);
            return (false, "An error occurred while archiving the client");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> UnarchiveAsync(int id)
    {
        try
        {
            var success = await _clientRepository.UnarchiveAsync(id);
            if (!success)
            {
                return (false, "Client not found");
            }

            _logger.LogInformation("Client unarchived successfully with ID: {ClientId}", id);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unarchiving client with ID: {ClientId}", id);
            return (false, "An error occurred while unarchiving the client");
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _clientRepository.GetCountAsync(c => c.Id == id) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if client exists with ID: {ClientId}", id);
            return false;
        }
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        try
        {
            var normalizedName = name.Trim().ToLowerInvariant();
            return excludeId.HasValue
                ? await _context.Clients.IgnoreQueryFilters()
                    .AnyAsync(c => c.Name.ToLower() == normalizedName && c.Id != excludeId.Value)
                : await _context.Clients.IgnoreQueryFilters()
                    .AnyAsync(c => c.Name.ToLower() == normalizedName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if client exists with name: {Name}", name);
            return false;
        }
    }

    private static ClientResponseDto MapToResponseDto(Client client)
    {
        return new ClientResponseDto
        {
            Id = client.Id,
            Name = client.Name,
            Address = client.Address,
            IsArchived = client.IsArchived,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        };
    }
}
