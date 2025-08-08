using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;
public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.Address)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(e => e.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        // Indexes
        builder.HasIndex(e => e.Name)
            .HasDatabaseName("IX_Clients_Name");
        builder.HasIndex(e => e.IsArchived)
            .HasDatabaseName("IX_Clients_IsArchived");
            
        // Relationships
        builder.HasMany(e => e.ShipmentDocuments)
            .WithOne(e => e.Client)
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
