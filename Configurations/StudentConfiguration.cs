using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.RegistrationNumber)
            .IsRequired();

        builder.Property(s => s.Name)
            .IsRequired();

        builder.Property(s => s.GPA)
            .HasPrecision(3, 2);


       
        builder.Property<DateTime>("LastUpdated")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");


       
        builder.Property(s => s.Version)
            .IsRowVersion();
    }
}