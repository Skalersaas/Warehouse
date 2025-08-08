using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;
public class ShipmentDocumentConfiguration : IEntityTypeConfiguration<ShipmentDocument>
{
    public void Configure(EntityTypeBuilder<ShipmentDocument> builder)
    {
        builder.ToTable("ShipmentDocuments");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Number)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.Date)
            .IsRequired();
            
        builder.Property(e => e.Status)
            .IsRequired()
            .HasDefaultValue(Domain.Models.Enums.ShipmentStatus.Draft);
            
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        // Indexes
        builder.HasIndex(e => e.Number)
            .IsUnique()
            .HasDatabaseName("IX_ShipmentDocuments_Number");
        builder.HasIndex(e => e.Date)
            .HasDatabaseName("IX_ShipmentDocuments_Date");
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_ShipmentDocuments_Status");
            
        // Relationships
        builder.HasOne(e => e.Client)
            .WithMany(e => e.ShipmentDocuments)
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(e => e.Items)
            .WithOne(e => e.Document)
            .HasForeignKey(e => e.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
