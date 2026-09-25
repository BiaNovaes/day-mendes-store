using System.Text;
using DayMendesStore.API.Middleware;
using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Interfaces;
using DayMendesStore.Application.Services;
using DayMendesStore.Infrastructure.Configuration;
using DayMendesStore.Infrastructure.Data;
using DayMendesStore.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration (MySQL)
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? builder.Configuration["DB_SERVER"];
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? builder.Configuration["DB_PORT"];
var dbDatabase = Environment.GetEnvironmentVariable("DB_DATABASE") ?? builder.Configuration["DB_DATABASE"];
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? builder.Configuration["DB_USER"];
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? builder.Configuration["DB_PASSWORD"];

string connectionString;

if (!string.IsNullOrWhiteSpace(dbServer) || !string.IsNullOrWhiteSpace(dbPassword))
{
    var server = !string.IsNullOrWhiteSpace(dbServer) ? dbServer : "localhost";
    var port = !string.IsNullOrWhiteSpace(dbPort) ? dbPort : "3306";
    var database = !string.IsNullOrWhiteSpace(dbDatabase) ? dbDatabase : "day_mendes_store";
    var user = !string.IsNullOrWhiteSpace(dbUser) ? dbUser : "root";
    var password = dbPassword ?? "";

    connectionString = $"Server={server};Port={port};Database={database};User={user};Password={password};";
}
else
{
    var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

    if (!string.IsNullOrWhiteSpace(defaultConn) && !defaultConn.Contains("YOUR_DATABASE_PASSWORD"))
    {
        connectionString = defaultConn;
    }
    else
    {
        connectionString = "Server=localhost;Port=3306;Database=day_mendes_store;User=root;Password=;";
    }
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
    options.UseMySql(connectionString, serverVersion);
});

// 2. JWT Settings & Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

var envJwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? builder.Configuration["JWT_SECRET_KEY"];
if (!string.IsNullOrWhiteSpace(envJwtSecret))
{
    jwtSettings.SecretKey = envJwtSecret;
}

var envJwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["JWT_ISSUER"];
if (!string.IsNullOrWhiteSpace(envJwtIssuer))
{
    jwtSettings.Issuer = envJwtIssuer;
}

var envJwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["JWT_AUDIENCE"];
if (!string.IsNullOrWhiteSpace(envJwtAudience))
{
    jwtSettings.Audience = envJwtAudience;
}

var envJwtExp = Environment.GetEnvironmentVariable("JWT_EXPIRATION_HOURS") ?? builder.Configuration["JWT_EXPIRATION_HOURS"];
if (int.TryParse(envJwtExp, out var expHours) && expHours > 0)
{
    jwtSettings.ExpirationHours = expHours;
}

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
{
    throw new InvalidOperationException("A chave JWT ('JWT_SECRET_KEY') não foi configurada ou possui tamanho inferior a 32 bytes (256 bits). Configure uma chave segura através de variáveis de ambiente.");
}

builder.Services.AddSingleton(jwtSettings);

var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

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
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 3. Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILojaService, LojaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IVendaService, VendaService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();

// 4. Controllers & JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 5. CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:5173", "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 6. Swagger / OpenAPI with JWT Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Day Mendes Store API",
        Version = "v1",
        Description = "API de Gestão da Day Mendes Store"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Autenticação automática ao fazer login em /api/auth/login. Para autorizar manualmente, digite apenas o token (sem a palavra 'Bearer')."
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

// Mantém o schema atualizado quando a aplicação inicia, inclusive no Docker.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Pipeline Configuration
app.UseMiddleware<ExceptionMiddleware>();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Day Mendes Store API v1");
        c.InjectJavascript("/swagger-custom.js");
    });
}

app.UseHttpsRedirection();

app.UseCors("DefaultCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
