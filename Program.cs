using Scalar.AspNetCore;
using TmsApi.Exceptions;
using TmsApi.Middleware;
using TmsApi.Options;
using TmsApi.Services;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using Microsoft.Extensions.Logging;
using TmsApi.Filters;



var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});


builder.Services.AddAuthentication();
builder.Services.AddAuthorization();


builder.Services.AddOpenApi();


builder.Services.AddProblemDetails();


builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();


builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<ICourseService, CourseService>();


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


if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();

    await DataSeeder.SeedAsync(context);
}


app.Run();