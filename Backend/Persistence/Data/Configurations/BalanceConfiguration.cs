using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Data.Configurations;
public class BalanceConfiguration : IEntityTypeConfiguration<Balance>
{
    public void Configure(EntityTypeBuilder<Balance> builder)
    {
        builder.ToTable("Balances");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Quantity)
            .IsRequired()
            .HasColumnType("decimal(18,3)");
            
        // Unique constraint: one balance per resource+unit combination
        builder.HasIndex(e => new { e.ResourceId, e.UnitId })
            .IsUnique()
            .HasDatabaseName("IX_Balances_Resource_Unit");
            
        // Relationships
        builder.HasOne(e => e.Resource)
            .WithMany(e => e.Balances)
            .HasForeignKey(e => e.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(e => e.Unit)
            .WithMany(e => e.Balances)
            .HasForeignKey(e => e.UnitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
