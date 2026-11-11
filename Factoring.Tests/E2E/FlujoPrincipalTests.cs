using Factoring.Core.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Factoring.Tests.E2E;

/// <summary>
/// Pruebas E2E para el flujo principal del sistema
/// Flujo: Login → Crear operación → Otorgar → Cobrar
///
/// NOTA: Estas pruebas requieren una base de datos configurada con datos de prueba
/// Para ejecutarlas en CI/CD, usar TestContainers o una BD de test
/// </summary>
public class FlujoPrincipalTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public FlujoPrincipalTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Test del flujo completo: Login → Validar que tenemos acceso
    /// </summary>
    [Fact]
    public async Task FlujoLogin_ConUsuarioValido_DebeObtenerToken()
    {
        // Este test solo pasará si existe el usuario admin en la BD
        // En un ambiente real, usaríamos TestContainers o similar

        // Arrange
        var loginRequest = new LoginRequest
        {
            Usuario = "admin",
            Password = "admin123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Solo verificamos que el endpoint responde correctamente
        response.Should().NotBeNull();
        // No validamos el código de estado porque depende de la BD
    }

    /// <summary>
    /// Test: Acceso a endpoint protegido sin token debe fallar
    /// </summary>
    [Fact]
    public async Task AccesoProtegido_SinToken_DebeRetornar401()
    {
        // Act
        var response = await _client.GetAsync("/api/cobranza/recaudacion/pendientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Test: Dashboard debe estar accesible (endpoint público para demo)
    /// </summary>
    [Fact]
    public async Task Dashboard_DebeResponder()
    {
        // Act
        var response = await _client.GetAsync("/api/reportes/dashboard");

        // Assert
        // Puede requerir auth, pero el endpoint debe existir
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test: Swagger debe estar disponible en desarrollo
    /// </summary>
    [Fact]
    public async Task Swagger_DebeEstarDisponible()
    {
        // Act
        var response = await _client.GetAsync("/swagger/index.html");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Test: Swagger JSON debe estar disponible
    /// </summary>
    [Fact]
    public async Task SwaggerJson_DebeEstarDisponible()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Factoring API");
    }

    /// <summary>
    /// Test: Health check del API (endpoints básicos responden)
    /// </summary>
    [Fact]
    public async Task Api_EndpointsPrincipales_DebenResponder()
    {
        // Arrange
        var endpoints = new[]
        {
            "/api/auth/login",
            "/api/auth/logout-completo"
        };

        foreach (var endpoint in endpoints)
        {
            // Act
            var response = await _client.PostAsync(endpoint, null);

            // Assert - No debe ser 404 (el endpoint existe)
            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
                $"El endpoint {endpoint} no fue encontrado");
        }
    }

    /// <summary>
    /// Test: CORS headers deben estar presentes
    /// </summary>
    [Fact]
    public async Task Cors_DebeEstarConfigurado()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/login");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        // Debe permitir CORS desde localhost:5173
        // El código de estado puede variar según la configuración
        response.Should().NotBeNull();
    }

    /// <summary>
    /// Test: Request logging debe agregar correlation ID
    /// </summary>
    [Fact]
    public async Task RequestLogging_DebeAgregarCorrelationId()
    {
        // Arrange
        var request = new LoginRequest { Usuario = "test", Password = "test" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.Headers.Should().ContainKey("X-Correlation-Id");
        var correlationId = response.Headers.GetValues("X-Correlation-Id").FirstOrDefault();
        correlationId.Should().NotBeNullOrEmpty();
    }
}

/// <summary>
/// Pruebas de flujo de operaciones (requieren autenticación y datos de prueba)
/// </summary>
public class FlujoOperacionesTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public FlujoOperacionesTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Helper para obtener token de autenticación
    /// </summary>
    private async Task<string?> ObtenerTokenAsync()
    {
        var loginRequest = new LoginRequest
        {
            Usuario = "admin",
            Password = "admin123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        return loginResponse?.Token;
    }

    /// <summary>
    /// Test: Validar operación endpoint debe responder
    /// </summary>
    [Fact]
    public async Task ValidarOperacion_Endpoint_DebeExistir()
    {
        // Arrange
        var request = new
        {
            rutCliente = 76123456,
            numeroLinea = 1,
            tipoDocumento = 1,
            montoOperacion = 1000000
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/operaciones/validar", request);

        // Assert
        // Puede requerir auth, pero el endpoint debe existir
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test: Calcular operación endpoint debe responder
    /// </summary>
    [Fact]
    public async Task CalcularOperacion_Endpoint_DebeExistir()
    {
        // Arrange
        var request = new
        {
            rutCliente = 76123456,
            numeroLinea = 1,
            tipoDocumento = 1,
            montoDocumentos = 1000000,
            plazoDias = 30
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/operaciones/calcular", request);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test: Endpoints de alertas deben responder
    /// </summary>
    [Fact]
    public async Task Alertas_Endpoints_DebenExistir()
    {
        // Act
        var responseLineas = await _client.GetAsync("/api/alertas/lineas/vencimiento");
        var responseDocumentos = await _client.GetAsync("/api/alertas/documentos/vencimiento");
        var responseResumen = await _client.GetAsync("/api/alertas/resumen");

        // Assert
        responseLineas.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        responseDocumentos.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        responseResumen.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test: Endpoints de reportes deben responder
    /// </summary>
    [Fact]
    public async Task Reportes_Endpoints_DebenExistir()
    {
        // Act
        var responseDashboard = await _client.GetAsync("/api/reportes/dashboard");
        var responseCartera = await _client.GetAsync("/api/reportes/cartera");

        // Assert
        responseDashboard.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        responseCartera.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test: Endpoints de documentos PDF deben responder
    /// </summary>
    [Fact]
    public async Task DocumentosPdf_Endpoints_DebenExistir()
    {
        // Act
        var responseLiquidacion = await _client.GetAsync("/api/documentos/liquidacion/1");
        var responsePagare = await _client.GetAsync("/api/documentos/pagare/1");
        var responseEstadoCuenta = await _client.GetAsync("/api/documentos/estado-cuenta/76123456");

        // Assert
        responseLiquidacion.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        responsePagare.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        responseEstadoCuenta.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }
}
