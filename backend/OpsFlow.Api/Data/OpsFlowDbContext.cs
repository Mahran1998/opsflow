using Microsoft.EntityFrameworkCore;
using OpsFlow.Api.Domain;

namespace OpsFlow.Api.Data;

public sealed class OpsFlowDbContext : DbContext
{
    public OpsFlowDbContext(DbContextOptions<OpsFlowDbContext> options) : base(options) { }

    public DbSet<RequestItem> Requests => Set<RequestItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<RequestItem>();

        e.ToTable("Requests");
        e.HasKey(x => x.Id);

        e.Property(x => x.Title)
            .HasMaxLength(120)
            .IsRequired();

        e.Property(x => x.Description)
            .HasMaxLength(2000);

        e.Property(x => x.Notes)
            .HasMaxLength(2000);

        // enums as strings (readable in DB)
        e.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        e.Property(x => x.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        e.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSDATETIMEOFFSET()");

        e.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("SYSDATETIMEOFFSET()");
    }
}
