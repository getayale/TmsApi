using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace TmsApi.Infrastructure.Persistence;

public class TmsDbContext : IdentityDbContext<TmsUser>
{
    public TmsDbContext(
        DbContextOptions<TmsDbContext> options)
        : base(options){}
    public DbSet<Student> Students => Set<Student>();

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<Assessment> Assessments => Set<Assessment>();

    public DbSet<Certificate> Certificates => Set<Certificate>();




       protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TmsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}