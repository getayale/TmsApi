using Scalar.AspNetCore;
using TmsApi.Exceptions;
using TmsApi.Middleware;
using TmsApi.Options;
using TmsApi.Services;


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


app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing");
});



app.Run();