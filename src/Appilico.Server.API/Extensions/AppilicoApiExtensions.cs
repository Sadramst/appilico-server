using System.Text;
using AspNetCoreRateLimit;
using CloudinaryDotNet;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Appilico.Server.API.Middleware;
using Appilico.Server.API.Swagger;
using Appilico.Server.Business.Behaviors;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Business.Mappings;
using Appilico.Server.Business.Options;
using Appilico.Server.Business.Services;
using Appilico.Server.DataAccess.Data;
using Appilico.Server.DataAccess.Repositories;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;

namespace Appilico.Server.API.Extensions;

/// <summary>Application composition helpers for the API host.</summary>
public static class AppilicoApiExtensions
{
    /// <summary>Configures structured logging.</summary>
    public static ConfigureHostBuilder UseAppilicoLogging(this ConfigureHostBuilder host, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        host.UseSerilog();
        return host;
    }

    /// <summary>Adds persistence, Identity, application services, and API infrastructure.</summary>
    public static IServiceCollection AddAppilicoApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddPersistence(configuration, environment);
        services.AddValidatedOptions(configuration);
        services.AddIdentityAndJwt(configuration);
        services.AddApplicationServices(configuration, environment);
        services.AddApiInfrastructure(configuration);
        return services;
    }

    /// <summary>Configures the HTTP request pipeline.</summary>
    public static WebApplication UseAppilicoPipeline(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        var swaggerEnabled = app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled");
        if (swaggerEnabled)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Appilico API v1"));
        }

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
        });

        if (app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseCors("AllowFrontend");
        app.UseIpRateLimiting();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    /// <summary>Maps controllers and lightweight health endpoints.</summary>
    public static WebApplication MapAppilicoEndpoints(this WebApplication app)
    {
        app.MapControllers();

        app.MapGet("/", () => Results.Ok(new
        {
            service = "Appilico E-Commerce API",
            status = "healthy",
            version = "v1",
            swagger = "/swagger/index.html",
            timestamp = DateTime.UtcNow
        }));

        app.MapGet("/health/live", () => Results.Ok(new
        {
            status = "healthy",
            service = "Appilico API",
            timestamp = DateTime.UtcNow
        }));

        app.MapGet("/health/ready", async (
            AppDbContext db,
            IOptions<StripeOptions> stripeOptions,
            IOptions<AzureStorageOptions> azureStorageOptions,
            IOptions<EmailOptions> emailOptions,
            IWebHostEnvironment environment) =>
        {
            var checks = await BuildReadinessChecksAsync(db, stripeOptions.Value, azureStorageOptions.Value, emailOptions.Value, environment);
            var healthy = checks.All(check => check.Value == "healthy" || check.Value == "disabled" || check.Value == "local");
            var response = new
            {
                status = healthy ? "healthy" : "degraded",
                checks,
                version = "v1",
                timestamp = DateTime.UtcNow
            };

            return healthy ? Results.Ok(response) : Results.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable);
        });

        app.MapGet("/health", async (AppDbContext db) =>
        {
            try
            {
                var canConnect = await db.Database.CanConnectAsync();
                return Results.Ok(new
                {
                    status = canConnect ? "healthy" : "degraded",
                    db = canConnect ? "connected" : "unreachable",
                    version = "v1",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new
                {
                    status = "degraded",
                    db = "error",
                    error = ex.Message,
                    version = "v1",
                    timestamp = DateTime.UtcNow
                });
            }
        });

        return app;
    }

    private static async Task<Dictionary<string, string>> BuildReadinessChecksAsync(
        AppDbContext db,
        StripeOptions stripeOptions,
        AzureStorageOptions azureStorageOptions,
        EmailOptions emailOptions,
        IWebHostEnvironment environment)
    {
        var checks = new Dictionary<string, string>
        {
            ["database"] = "unknown",
            ["stripe"] = stripeOptions.Enabled ? "healthy" : "disabled",
            ["fileStorage"] = environment.IsDevelopment()
                ? "local"
                : azureStorageOptions.Enabled && azureStorageOptions.HasRequiredSettings ? "healthy" : "degraded",
            ["email"] = emailOptions.Enabled ? "healthy" : "disabled"
        };

        try
        {
            checks["database"] = await db.Database.CanConnectAsync() ? "healthy" : "degraded";
        }
        catch
        {
            checks["database"] = "degraded";
        }

        return checks;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        if (environment.IsProduction() && IsPlaceholder(defaultConnection))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured for production startup.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(defaultConnection));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddValidatedOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => options.HasStrongSecret, "JWT:Secret must be configured, non-placeholder, and at least 32 characters.")
            .Validate(options => options.HasRequiredIssuerAudience, "JWT:Issuer and JWT:Audience must be configured.")
            .ValidateOnStart();

        services.AddOptions<StripeOptions>()
            .Bind(configuration.GetSection(StripeOptions.SectionName))
            .Validate(options => options.HasRequiredSettings, "Stripe settings must be configured when Stripe:Enabled is true.")
            .ValidateOnStart();

        services.AddOptions<AzureStorageOptions>()
            .Bind(configuration.GetSection(AzureStorageOptions.SectionName))
            .Validate(options => options.HasRequiredSettings, "AzureStorage settings must be configured when AzureStorage:Enabled is true.")
            .ValidateOnStart();

        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .Validate(options => options.HasRequiredSettings, "Email SMTP settings must be configured when Email:Enabled is true.")
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddIdentityAndJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<AppUser, AppRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        var jwtSettings = configuration.GetSection("JWT");
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT:Secret not configured"))),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();
        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var emailEnabled = configuration.GetValue<bool>("Email:Enabled");
        if (environment.IsDevelopment() || !emailEnabled)
            services.AddScoped<IEmailService, NullEmailService>();
        else
            services.AddScoped<IEmailService, SmtpEmailService>();

        services.AddSingleton<IEmailWorkQueue, BackgroundEmailQueue>();
        services.AddHostedService<QueuedEmailHostedService>();

        if (environment.IsDevelopment())
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        else
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();

        services.AddScoped<IStripeService, StripePaymentService>();
        services.AddScoped<IAccessControlService, AccessControlService>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<IVoucherService, VoucherService>();
        services.AddScoped<ISpecialOfferService, SpecialOfferService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<IWaitlistService, WaitlistService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IVisualService, VisualService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<INewsletterService, NewsletterService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ValidationBehaviour<,>).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

        var cloudinaryConfig = configuration.GetSection("Cloudinary");
        var cloudinaryAccount = new Account(
            cloudinaryConfig["CloudName"],
            cloudinaryConfig["ApiKey"],
            cloudinaryConfig["ApiSecret"]);
        services.AddSingleton(new Cloudinary(cloudinaryAccount));
        services.AddScoped<IImageService, CloudinaryImageService>();

        services.AddAutoMapper(_ => { }, typeof(MappingProfile));
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<MappingProfile>();

        return services;
    }

    private static IServiceCollection AddApiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        var allowedOrigins = new[]
        {
            "http://localhost:3000",
            "http://localhost:3001",
            "http://localhost:3002",
            "https://appilico-client-aif9.vercel.app",
            "https://appilico-client.vercel.app",
            "https://appilico-web.vercel.app",
            "https://appilico.com",
            "https://www.appilico.com",
            "https://appilico.store",
            "https://www.appilico.store",
            "https://appilico.com.au",
            "https://www.appilico.com.au"
        };

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.SetIsOriginAllowed(origin =>
                        allowedOrigins.Contains(origin) ||
                        new Uri(origin).Host.EndsWith(".vercel.app"))
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Appilico E-Commerce API",
                Version = "v1",
                Description = "Appilico API v1. Routes are stable under /api and use JWT bearer authentication for protected resources."
            });

            var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            options.OperationFilter<DefaultResponsesOperationFilter>();
        });

        return services;
    }

    private static bool IsPlaceholder(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            || value.Contains("will-be-overridden", StringComparison.OrdinalIgnoreCase)
            || value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase);
    }
}
