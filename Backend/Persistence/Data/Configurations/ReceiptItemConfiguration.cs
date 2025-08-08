using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;
public class ReceiptItemConfiguration : IEntityTypeConfiguration<ReceiptItem>
{
    public void Configure(EntityTypeBuilder<ReceiptItem> builder)
    {
        builder.ToTable("ReceiptItems");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Quantity)
            .IsRequired()
            .HasColumnType("decimal(18,3)");
            
        // Indexes
        builder.HasIndex(e => new { e.DocumentId, e.ResourceId, e.UnitId })
            .HasDatabaseName("IX_ReceiptItems_Document_Resource_Unit");
            
        // Relationships
        builder.HasOne(e => e.Document)
            .WithMany(e => e.Items)
            .HasForeignKey(e => e.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(e => e.Resource)
            .WithMany(e => e.ReceiptItems)
            .HasForeignKey(e => e.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(e => e.Unit)
            .WithMany(e => e.ReceiptItems)
            .HasForeignKey(e => e.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
