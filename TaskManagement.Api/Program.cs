using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text;
using Serilog;
using AspNetCoreRateLimit;
using Asp.Versioning;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;
using TaskManagement.Core.Settings;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Services;
using TaskManagement.Api.Middleware;
using TaskManagement.Api.Services;

// Configure Serilog with proper configuration pipeline
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting Task Management API...");

    var builder = WebApplication.CreateBuilder(args);

    // Configuration Pipeline (automatically loaded by WebApplicationBuilder):
    // 1. appsettings.json
    // 2. appsettings.{Environment}.json (e.g., appsettings.Production.json)
    // 3. User Secrets (Development environment only)
    // 4. Environment Variables
    // 5. Command-line arguments

    // Add Serilog
    builder.Host.UseSerilog();

// Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("TaskManagement.Infrastructure")
    ));

// Rate Limiting Configuration
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Identity Configuration
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var secretKey = jwtSettings.Get<JwtSettings>()!.SecretKey;
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Get<JwtSettings>()!.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Get<JwtSettings>()!.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Register Services
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IChecklistRepository, ChecklistRepository>();
builder.Services.AddScoped<IChecklistService, ChecklistService>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IDashboardService, TaskManagement.Api.Services.DashboardService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Email Service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Notification Service
builder.Services.AddScoped<INotificationService, TaskManagement.Api.Services.NotificationService>();

// File Services
builder.Services.AddScoped<IFileService, TaskManagement.Api.Services.FileService>();
// File Cleanup Service - Runs every 15 minutes to delete old attachments
builder.Services.AddHostedService<TaskManagement.Api.Services.FileCleanupService>();

// Time Tracking Service
builder.Services.AddScoped<ITimeTrackingService, TaskManagement.Api.Services.TimeTrackingService>();

// Task Dependency Service
builder.Services.AddScoped<ITaskDependencyService, TaskManagement.Api.Services.TaskDependencyService>();

// Calendar Service
builder.Services.AddScoped<ICalendarService, TaskManagement.Api.Services.GoogleCalendarService>();

// Shared Link Service
builder.Services.AddScoped<ISharedLinkService, TaskManagement.Api.Services.SharedLinkService>();

// Task Assignment Service
builder.Services.AddScoped<ITaskAssignmentService, TaskManagement.Api.Services.TaskAssignmentService>();

// Repositories
builder.Services.AddScoped<IProjectRepository, TaskManagement.Infrastructure.Repositories.ProjectRepository>();
builder.Services.AddScoped<ICommentRepository, TaskManagement.Infrastructure.Repositories.CommentRepository>();
builder.Services.AddScoped<IAttachmentRepository, TaskManagement.Infrastructure.Repositories.AttachmentRepository>();

// OTP Service
builder.Services.AddScoped<IOtpService, TaskManagement.Api.Services.OtpService>();

// SignalR
builder.Services.AddSignalR();

// Controllers & Validation
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining(typeof(Program));

// CORS Configuration
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", 
                "http://localhost:3000", 
                "http://localhost:5174",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:3000"
            )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "https://zenith1-ochre.vercel.app",
                "http://zenith.runasp.net",
                "https://zenith.runasp.net"
            )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("database", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database is healthy"));

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// SignalR
builder.Services.AddSignalR();

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Management API",
        Version = "v1",
        Description = "A comprehensive Task Management System API with Authentication"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Apply pending migrations automatically on startup
// Auto-Migrate (Create DB tables on MonsterASP)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        Log.Information("Checking for pending database migrations...");
        db.Database.Migrate();
        Log.Information("✅ Database migrated successfully");
        Console.WriteLine("✅ Database migrated successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ An error occurred while migrating the database");
        Console.WriteLine($"❌ Migration Error: {ex.Message}");
        // Don't throw in production - let the app start and log the error
    }
}

// Add Global Exception Handler Middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure HTTP request pipeline
// Enable Swagger in all environments for testing
app.UseSwagger();
app.UseSwaggerUI();

// DO NOT enable HTTPS redirection on MonsterASP
// app.UseHttpsRedirection();

// Serve static files (for uploaded images)
app.UseStaticFiles();

// CORS
app.UseCors("AllowAll"); // Use "Production" in production environment

// Rate Limiting
app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

// Root Endpoint (required by MonsterASP)
app.MapGet("/", () => Results.Ok("Task Management API is running..."));

app.MapControllers();
app.MapHub<TaskManagement.Api.Hubs.NotificationsHub>("/hubs/notifications");
app.MapHealthChecks("/health");

    Log.Information("Task Management API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
