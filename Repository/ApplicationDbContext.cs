using BackendService.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataBase;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public virtual DbSet<WorkTask> WorkTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WorkTask>(entity =>
        {
            entity.HasKey(e => e.ID);

            entity.Property(nt => nt.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(nt => nt.Description)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(nt => nt.Status)
                .IsRequired();

            entity.Property(nt => nt.AssignedTo)
                .IsRequired(false)
                .HasMaxLength(200);
        });
    }
}