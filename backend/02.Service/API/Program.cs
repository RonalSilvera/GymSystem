using System.Reflection;
using API;
using API.Middleware;
using System;
using API.Swagger;
using BusinessLogic.Ports;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Filters;
using Infrastructure.Factory;
using Infrastructure.Interfaces;
using Infrastructure.Mapper;
using Infrastructure.Data;
using Infrastructure.TenantConnection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.Extensions.Http;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Serilog;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseWebRoot("wwwroot");
builder.WebHost.UseStaticWebAssets();


// Configuración de Serilog
builder.Host.UseSerilog((context, config) => { config.ReadFrom.Configuration(context.Configuration); });

// Configuración general
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile("responsemessage.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true,
        true); // Esto carga PathBase en producción

// Registrar proveedor de conexión tenant (lee connection strings de appsettings)
builder.Services.AddSingleton<ITenantConnectionProvider, TenantConnectionProvider>();

// Registrar fábrica para crear DbContext dinámicamente por tenant
builder.Services.AddScoped<IAppDbContextFactory, AppDbContextFactory>();

var defaultTenant = builder.Configuration.GetValue<string>("DefaultTenant") ?? "Tenant1";
var conn = builder.Configuration.GetConnectionString(defaultTenant) ??
          builder.Configuration.GetConnectionString("Tenant1") ??
          "Server=localhost;Port=3306;Database=gymsystem;User=root;Password=;SslMode=None;AllowPublicKeyRetrieval=True;";

var runtimeServerVersion = DetectServerVersion(conn);
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseMySql(conn, runtimeServerVersion, mySqlOptions =>
    {
        mySqlOptions.MigrationsHistoryTable(AppDbContext.MigrationsHistoryTableName);
    }));


// CORS
var corsSection = builder.Configuration.GetSection("Cors");
var useCredentials = corsSection.GetValue<bool>("UseCredentials");
var configuredOrigins = corsSection.GetSection("AllowedOrigins").Get<string[]>();
if (configuredOrigins == null || configuredOrigins.Length == 0)
{
    var legacyOrigins = builder.Configuration.GetValue<string>("Origins");
    if (!string.IsNullOrWhiteSpace(legacyOrigins))
    {
        configuredOrigins = legacyOrigins
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}

configuredOrigins ??= Array.Empty<string>();
var allowAllOrigins = configuredOrigins.Length == 0 || configuredOrigins.Contains("*");

builder.Services.AddCors(options =>
{
    options.AddPolicy("DynamicCors", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod();

        if (useCredentials)
        {
            if (allowAllOrigins)
            {
                policy.SetIsOriginAllowed(_ => true);
            }
            else
            {
                policy.WithOrigins(configuredOrigins);
            }

            policy.AllowCredentials();
        }
        else
        {
            if (allowAllOrigins)
            {
                policy.AllowAnyOrigin();
            }
            else
            {
                policy.WithOrigins(configuredOrigins);
            }
        }

        var exposedHeaders = corsSection.GetValue<string>("ExposedHeaders");
        if (!string.IsNullOrWhiteSpace(exposedHeaders))
        {
            var headers = exposedHeaders.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (headers.Length > 0)
            {
                policy.WithExposedHeaders(headers);
            }
        }
    });
});

// AutoMapper y DI
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IFactoryLogic, FactoryLogic>();
builder.Services.AddScoped<API.Services.IJwtTokenService, API.Services.JwtTokenService>();
builder.Services.AddScoped<API.Services.IPasswordService, API.Services.PasswordService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

// Polly
builder.Services.AddSingleton<AsyncPolicy>(serviceProvider =>
    Policy.Handle<HttpRequestException>()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
builder.Services.AddHttpClient("ExternalServiceClient")
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt)));

// Controllers y validaciones
builder.Services.AddControllers(options => { options.Filters.Add<GlobalExceptionFilter>(); })
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    })
    .ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = true; });
builder.Services.AddMvc(options => { options.Filters.Add<ValidationFilter>(); });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GymSystem API", Version = "v1" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.OperationFilter<TenantHeaderOperationFilter>();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and your token"
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

// Redirección HTTPS configurable por entorno
var enforceHttpsRedirection = builder.Configuration.GetValue<bool?>("Security:EnforceHttpsRedirection")
                          ?? builder.Environment.IsProduction();

if (enforceHttpsRedirection)
    builder.Services.AddHttpsRedirection(options => { options.HttpsPort = 44322; });


// ==================== BUILD ====================
var app = builder.Build();

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Local"))
{
    var pathBase = app.Configuration["PathBase"];
    if (!string.IsNullOrEmpty(pathBase))
    {
        app.UsePathBase(pathBase);
        app.Use((context, next) =>
        {
            context.Request.PathBase = new PathString(pathBase);
            return next();
        });
    }
}

// Middleware base
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "GymSystem API v1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();

app.UseCors("DynamicCors");

app.Use(async (context, next) =>
{
    if (HttpMethods.IsOptions(context.Request.Method))
    {
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }

    await next();
});

if (enforceHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.UseMiddleware<TenantMiddleware>();
app.MapControllers().AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory);
app.Run();

static ServerVersion DetectServerVersion(string connectionString)
{
    try
    {
        return ServerVersion.AutoDetect(connectionString);
    }
    catch (MySqlConnector.MySqlException)
    {
        return MySqlServerVersion.LatestSupportedServerVersion;
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Unable to connect", StringComparison.OrdinalIgnoreCase))
    {
        return MySqlServerVersion.LatestSupportedServerVersion;
    }
}
