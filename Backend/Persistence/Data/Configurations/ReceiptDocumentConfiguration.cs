using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;
public class ReceiptDocumentConfiguration : IEntityTypeConfiguration<ReceiptDocument>
{
    public void Configure(EntityTypeBuilder<ReceiptDocument> builder)
    {
        builder.ToTable("ReceiptDocuments");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Number)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.Date)
            .IsRequired();
            
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        // Indexes
        builder.HasIndex(e => e.Number)
            .IsUnique()
            .HasDatabaseName("IX_ReceiptDocuments_Number");
        builder.HasIndex(e => e.Date)
            .HasDatabaseName("IX_ReceiptDocuments_Date");
            
        // Relationships
        builder.HasMany(e => e.Items)
            .WithOne(e => e.Document)
            .HasForeignKey(e => e.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
