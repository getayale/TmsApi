using Scalar.AspNetCore;
using TmsApi.Exceptions;
using TmsApi.Middleware;
using TmsApi.Options;
using TmsApi.Services;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


builder.Services.AddAuthentication();
builder.Services.AddAuthorization();


builder.Services.AddOpenApi();


builder.Services.AddProblemDetails();


builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();


builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase"))
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging());


var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler();
}


app.UseMiddleware<RequestLoggingMiddleware>();


app.UseStatusCodePages();


app.UseHttpsRedirection();


app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();

    context.Database.Migrate();

    if (!context.Students.Any())
    {
        var students = new List<Student>
        {
            new()
            {
                RegistrationNumber = "TMS-2026-0001",
                Name = "Alice Smith",
                GPA = 3.8m,
                IsActive = true
            },
            new()
            {
                RegistrationNumber = "TMS-2026-0002",
                Name = "Bob Jones",
                GPA = 2.9m,
                IsActive = true
            }
        };

        context.Students.AddRange(students);

        var courses = new List<Course>
        {
            new()
            {
                Code="CS-101",
                Title="Introduction to Computer Science",
                Capacity=30
            }
        };

        context.Courses.AddRange(courses);

        context.SaveChanges();
    }
}



app.Run();