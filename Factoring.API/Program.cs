using System.Reflection;
using System.Text;
using Factoring.Infrastructure;
using Factoring.API.Services;
using Factoring.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

// Configurar Serilog antes de crear el builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/factoring-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Iniciando Factoring API...");

var builder = WebApplication.CreateBuilder(args);

// Usar Serilog como proveedor de logging
builder.Host.UseSerilog();

// Add services to the container.

// Registrar Infrastructure (Dapper, Repositorios, Servicios)
builder.Services.AddInfrastructure(builder.Configuration);

// HttpContextAccessor para servicios que necesitan acceso al contexto HTTP
builder.Services.AddHttpContextAccessor();

// Registrar servicios de API
builder.Services.AddScoped<PdfGeneratorService>();

// Configurar JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "FactoringSystemSecretKey2024!@#$%";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "FactoringSystem",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "FactoringClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();

// CORS para React
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Swagger con soporte JWT y documentacion XML completa
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Factoring API",
        Version = "v1",
        Description = @"
## API para Sistema de Factoring - Migrado de VB6

### Modulos disponibles:
- **Auth**: Autenticacion y gestion de sesiones
- **Clientes**: Mantencion de clientes (CRUD completo)
- **Deudores**: Mantencion de deudores
- **Operaciones**: Ingreso, otorgamiento y anulacion de operaciones
- **Cobranza**: Pagos, recaudacion y verificacion de documentos
- **Alertas**: Alertas de vencimiento y morosidad
- **Reportes**: Dashboard, exportaciones Excel
- **Documentos**: Generacion de PDFs (Liquidacion, Pagare, Estado de Cuenta)
- **Lineas de Credito**: Gestion de lineas y sublineas
- **Tesoreria**: Movimientos de caja y bancos

### Autenticacion:
Usar el endpoint `/api/auth/login` para obtener un token JWT.
Incluir el token en el header `Authorization: Bearer {token}`

### Codigos de Estado:
- `200`: Operacion exitosa
- `201`: Recurso creado exitosamente
- `400`: Error de validacion o datos incorrectos
- `401`: No autenticado
- `403`: No autorizado
- `404`: Recurso no encontrado
- `500`: Error interno del servidor
",
        Contact = new OpenApiContact
        {
            Name = "Soporte Factoring",
            Email = "soporte@factoring.cl"
        },
        License = new OpenApiLicense
        {
            Name = "Uso Interno"
        }
    });

    // Cargar documentacion XML
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Agregar ejemplos y descripciones adicionales
    c.EnableAnnotations();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header usando el esquema Bearer.
Ingrese 'Bearer' [espacio] y luego su token.
Ejemplo: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
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

    // Ordenar endpoints por tag
    c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Middlewares de seguridad (orden importante - van primero)
app.UseSecurityHeaders(options =>
{
    options.EnableHsts = !app.Environment.IsDevelopment();
    options.EnableContentSecurityPolicy = true;
});

app.UseRateLimiting(options =>
{
    options.DefaultMaxRequests = 100;
    options.DefaultWindowSeconds = 60;
    // Limites especificos
    options.EndpointRules.Add(new RateLimitRule
    {
        PathPrefix = "/api/auth/login",
        MaxRequests = 5,
        WindowSeconds = 60
    });
    options.EndpointRules.Add(new RateLimitRule
    {
        PathPrefix = "/api/reportes",
        MaxRequests = 30,
        WindowSeconds = 60
    });
});

app.UseInputValidation(options =>
{
    options.DetectSqlInjection = true;
    options.DetectXss = true;
    options.DetectPathTraversal = true;
    options.RevealThreatType = app.Environment.IsDevelopment();
});

// Middlewares personalizados de logging
app.UseGlobalExceptionHandler(); // Captura todas las excepciones
app.UseRequestLogging();         // Logging de requests

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Log.Information("Factoring API iniciada correctamente en {Environment}", app.Environment.EnvironmentName);

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicacion fallo al iniciar");
}
finally
{
    Log.CloseAndFlush();
}

// Hacer Program accesible para pruebas de integración
public partial class Program { }
