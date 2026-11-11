using Factoring.Core.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Factoring.Tests.Integration.Api;

/// <summary>
/// Pruebas de integración para AuthController
/// Estas pruebas verifican el comportamiento completo del endpoint de autenticación
/// </summary>
public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task Login_ConCredencialesVacias_DebeRetornar401()
    {
        // Arrange
        var request = new LoginRequest { Usuario = "", Password = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ConUsuarioInexistente_DebeRetornar401()
    {
        // Arrange
        var request = new LoginRequest
        {
            Usuario = "usuarioquenoexiste",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);

        loginResponse.Should().NotBeNull();
        loginResponse!.Exitoso.Should().BeFalse();
    }

    [Fact]
    public async Task CambiarPassword_SinNuevoPassword_DebeRetornar400()
    {
        // Arrange
        var request = new LoginRequest
        {
            Usuario = "admin",
            Password = "admin123"
            // NuevoPassword no especificado
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/cambiar-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_SinAutenticacion_DebeRetornar401()
    {
        // Act
        var response = await _client.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ConTokenInvalido_DebeRetornar401()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "tokeninvalidoquenoexiste" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LogoutCompleto_SinToken_DebeRetornar200()
    {
        // Arrange - logout completo permite usuarios anónimos

        // Act
        var response = await _client.PostAsync("/api/auth/logout-completo", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_EndpointExiste_DebeResponder()
    {
        // Arrange
        var request = new LoginRequest { Usuario = "test", Password = "test" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        // Debe responder (no 404), aunque sea con error
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }
}
