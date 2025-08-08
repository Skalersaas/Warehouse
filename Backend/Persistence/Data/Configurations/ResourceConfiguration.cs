using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;
public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Resources");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        // Indexes
        builder.HasIndex(e => e.Name)
            .HasDatabaseName("IX_Resources_Name");
        builder.HasIndex(e => e.IsArchived)
            .HasDatabaseName("IX_Resources_IsArchived");
            
        // Relationships
        builder.HasMany(e => e.ReceiptItems)
            .WithOne(e => e.Resource)
            .HasForeignKey(e => e.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(e => e.ShipmentItems)
            .WithOne(e => e.Resource)
            .HasForeignKey(e => e.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(e => e.Balances)
            .WithOne(e => e.Resource)
            .HasForeignKey(e => e.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
