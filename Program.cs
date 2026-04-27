using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using WorkOrderApp.AppDbContext;
using WorkOrderApp.Data.Seeders;
using WorkOrderApp.Helpers;
using WorkOrderApp.Helpers.AST;
using WorkOrderApp.Helpers.Auth;
using WorkOrderApp.Helpers.Notifications;
using WorkOrderApp.Helpers.Queues;
using WorkOrderApp.Middleware;
using WorkOrderApp.Services;
using WorkOrderApp.Services.Email;
using WorkOrderApp.Services.Interfaces;
using WorkOrderApp.Services.OTP;
using WorkOrderApp.Settings;
using WorkOrderApp.AutoMapper;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Typed settings ────────────────────────────────────────────────────────────
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection("CorsSettings"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<AdminSeedSettings>(builder.Configuration.GetSection("AdminSeed"));

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// ── CORS ──────────────────────────────────────────────────────────────────────
var corsOrigins = builder.Configuration
    .GetSection("CorsSettings:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy("AppCors", policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()));

// ── Controllers + OpenAPI ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new();
        document.Info = new()
        {
            Title       = appSettings.Title,
            Version     = appSettings.Version,
            Description = appSettings.Description
        };
        return Task.CompletedTask;
    });
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings section is missing from configuration.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime    = true,
            ValidIssuer              = jwtSettings.Issuer,
            ValidAudience            = jwtSettings.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtService,        JwtService>();
builder.Services.AddScoped<IUserService,       UserService>();
builder.Services.AddScoped<IEnumService,       EnumService>();
builder.Services.AddScoped<ILocationService,   LocationService>();
builder.Services.AddScoped<IRequestService,    RequestService>();
builder.Services.AddScoped<IWorkOrderService,  WorkOrderService>();
builder.Services.AddScoped<ICostService,       CostService>();
builder.Services.AddScoped<IPartService,       PartService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<OTPService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<BlobService>();
builder.Services.AddScoped<NotificationService>();

// ── Infrastructure services ───────────────────────────────────────────────────
builder.Services.AddScoped<IAzureTableService, AzureTableService>();
builder.Services.AddScoped<IQueueService,      AstQueueService>();
builder.Services.AddScoped<QueueManager>();

// ── Background services (uncomment as needed) ─────────────────────────────────
// builder.Services.AddHostedService<QueueServiceListner>();
// builder.Services.AddHostedService<DelayedNotificationsListener>();

// ── Seeders ───────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IDataSeeder, AdminSeeder>();
builder.Services.AddScoped<IDataSeeder, LocationSeeder>();
builder.Services.AddScoped<IDataSeeder, UserSeeder>();
builder.Services.AddScoped<IDataSeeder, RequestSeeder>();
builder.Services.AddScoped<IDataSeeder, WorkOrderSeeder>();
builder.Services.AddScoped<IDataSeeder, CostPartAttachmentSeeder>();

// ── AutoMapper ────────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
}, typeof(MappingProfile));

builder.Services.AddMemoryCache();

// ═════════════════════════════════════════════════════════════════════════════
var app = builder.Build();

app.UseCors("AppCors");
app.UseMiddleware<ExceptionHandler>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", $"{jwtSettings.Issuer} API v1");
    options.RoutePrefix = "swagger";
});

app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new();
    options.Title              = appSettings.Title;
    options.Theme              = ScalarTheme.Purple;
    options.DefaultHttpClient  = new(ScalarTarget.Http, ScalarClient.Http11);
});

// ── Run seeders ───────────────────────────────────────────────────────────────
await SeederRunner.RunAsync(app.Services);

app.Run();

public partial class Program { }
