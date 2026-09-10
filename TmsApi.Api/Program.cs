using Asp.Versioning;
using FluentValidation;
using MediatR;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

using TmsApi.Api.ExceptionHandlers;
using TmsApi.Api.Filters;
using TmsApi.Api.Hubs;
using TmsApi.Api.Middleware;
using TmsApi.Api.Notifications;
using TmsApi.Api.Options;
using TmsApi.Api.RateLimiting;

using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Notifications;
using TmsApi.Application.Transcripts;

using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Transcripts;
using TmsApi.Infrastructure.Workers;

using Microsoft.AspNetCore.Antiforgery;
using TmsApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);

    options.AssumeDefaultVersionWhenUnspecified = true;

    options.ReportApiVersions = true;

    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";

    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v1";
});

builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v2";
});

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(EnrollStudentCommand).Assembly);

    cfg.AddOpenBehavior(
        typeof(LoggingBehavior<,>));

    cfg.AddOpenBehavior(
        typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(
    typeof(EnrollStudentValidator).Assembly);

builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

builder.Services.AddSingleton<
    ITranscriptStatusStore,
    InMemoryTranscriptStatusStore>();

builder.Services.AddSingleton<
    Channel<TranscriptRequest>>(sp =>
    Channel.CreateBounded<TranscriptRequest>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));

builder.Services.AddHostedService<TranscriptWorker>();

builder.Services.AddSignalR();

builder.Services.AddSingleton<
    ITranscriptNotificationService,
    SignalRTranscriptNotificationService>();

builder.Services.AddDbContext<TmsDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase"));

    options
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging();
});

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
            {
                var (partitionKey, tier) =
                    ApiKeyResolver.Resolve(httpContext);

                return tier switch
                {
                    ApiKeyTier.Paid =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            $"paid:{partitionKey}",
                            _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 200,
                                TokensPerPeriod = 100,
                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            }),

                    ApiKeyTier.Free =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            $"free:{partitionKey}",
                            _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 30,
                                TokensPerPeriod = 10,
                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            }),

                    _ =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            $"anon:{partitionKey}",
                            _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 10,
                                TokensPerPeriod = 5,
                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            })
                };
            });

    options.AddConcurrencyLimiter(
        "transcripts",
        opt =>
        {
            opt.PermitLimit = 5;
            opt.QueueLimit = 20;
            opt.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;
        });

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = "10";

        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter,
            out var retry))
        {
            retryAfter =
                ((int)retry.TotalSeconds).ToString();
        }

        context.HttpContext.Response.Headers.RetryAfter =
            retryAfter;

        context.HttpContext.Response.ContentType =
            "application/problem+json";

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                title = "Rate limit exceeded",
                detail =
                    $"Too many requests. Retry after {retryAfter} seconds.",
                status = 429,
                type =
                    "https://tms.local/errors/rate_limit_exceeded"
            },
            ct);
    };
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});



builder.Services.AddIdentityCore<TmsUser>(options =>
{
 options.Password.RequiredLength = 8;
options.Password.RequireUppercase = true;
options.Password.RequireDigit = true;
options.Password.RequireNonAlphanumeric = true;

// Brute-Force Lockout Protection
options.Lockout.MaxFailedAccessAttempts = 5;
options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
options.Lockout.AllowedForNewUsers = true;
}).AddRoles<IdentityRole>()
.AddEntityFrameworkStores<TmsDbContext>();

builder.Services.AddScoped<TokenService>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer =
                builder.Configuration["Jwt:Issuer"],

            ValidAudience =
                builder.Configuration["Jwt:Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]!))
        };
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("TMS API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(
                ScalarTarget.CSharp,
                ScalarClient.HttpClient);

        options
            .AddDocument("v1", "API Version 1.0")
            .AddDocument("v2", "API Version 2.0");
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<V1DeprecationMiddleware>();

app.UseStatusCodePages();

app.UseRouting();

app.UseCors("TmsClient");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();
app.UseAuthentication();

app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true ||
        context.Request.Cookies.ContainsKey("tms_auth"))
    {
        var antiforgery = context.RequestServices
            .GetRequiredService<IAntiforgery>();

        var tokens = antiforgery.GetAndStoreTokens(context);

        context.Response.Cookies.Append(
            "XSRF-TOKEN",
            tokens.RequestToken!,
            new CookieOptions
            {
                HttpOnly = false,
                Secure = !builder.Environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict
            });
    }

    await next(context);
});
app.MapHub<TmsHub>("/hubs/tms").RequireCors("TmsClient");

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();

    await DataSeeder.SeedAsync(context);
}

app.Run();