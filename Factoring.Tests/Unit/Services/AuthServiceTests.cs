using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;
using Factoring.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Factoring.Tests.Unit.Services;

/// <summary>
/// Pruebas unitarias para AuthService
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IBitacoraRepository> _bitacoraRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _bitacoraRepositoryMock = new Mock<IBitacoraRepository>();
        _configurationMock = new Mock<IConfiguration>();

        // Configurar valores por defecto
        _configurationMock.Setup(c => c.GetSection("Auth:MaxIntentosLogin").Value).Returns("3");
        _configurationMock.Setup(c => c.GetSection("Auth:DiasCambioPassword").Value).Returns("90");
        _configurationMock.Setup(c => c.GetSection("Auth:HorasExpiracionToken").Value).Returns("8");
        _configurationMock.Setup(c => c.GetSection("Auth:DiasExpiracionRefreshToken").Value).Returns("30");
        _configurationMock.Setup(c => c["Jwt:Key"]).Returns("TestSecretKey12345678901234567890!");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        _authService = new AuthService(
            _usuarioRepositoryMock.Object,
            _bitacoraRepositoryMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ConCredencialesVacias_DebeRetornarError()
    {
        // Arrange
        var request = new LoginRequest { Usuario = "", Password = "" };

        // Act
        var result = await _authService.LoginAsync(request, "127.0.0.1", "TestMachine");

        // Assert
        result.Exitoso.Should().BeFalse();
        result.Mensaje.Should().Be("Debe ingresar usuario y contraseña");
    }

    [Fact]
    public async Task LoginAsync_ConUsuarioInexistente_DebeRetornarAccesoDenegado()
    {
        // Arrange
        var request = new LoginRequest { Usuario = "usuarioinexistente", Password = "password123" };
        _usuarioRepositoryMock
            .Setup(r => r.GetByLoginAsync(It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _authService.LoginAsync(request, "127.0.0.1", "TestMachine");

        // Assert
        result.Exitoso.Should().BeFalse();
        result.Mensaje.Should().Be("Acceso denegado");
        _bitacoraRepositoryMock.Verify(
            b => b.RegistrarAccesoAsync(It.Is<BitacoraAcceso>(ba => ba.BitObservacion == "Usuario no existe")),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ConUsuarioBloqueado_DebeRetornarClaveBloqueada()
    {
        // Arrange
        var request = new LoginRequest { Usuario = "usuariobloqueado", Password = "password123" };
        var usuarioBloqueado = new Usuario
        {
            UsrLogUsr = "usuariobloqueado",
            UsrNomUsr = "Usuario Bloqueado",
            UsrPasUsr = "hashedpassword",
            UsrEstPas = 3 // Bloqueado
        };

        _usuarioRepositoryMock
            .Setup(r => r.GetByLoginAsync("usuariobloqueado"))
            .ReturnsAsync(usuarioBloqueado);

        // Act
        var result = await _authService.LoginAsync(request, "127.0.0.1", "TestMachine");

        // Assert
        result.Exitoso.Should().BeFalse();
        result.Mensaje.Should().Be("Clave bloqueada, contáctese con el Administrador");
    }

    [Fact]
    public async Task LoginAsync_RequiereCambioPassword_DebeIndicarRequiereCambio()
    {
        // Arrange
        var request = new LoginRequest { Usuario = "usuario", Password = "password123" };
        var usuario = new Usuario
        {
            UsrLogUsr = "usuario",
            UsrNomUsr = "Usuario Test",
            UsrPasUsr = "hashedpassword",
            UsrEstPas = 2 // Primer acceso
        };

        _usuarioRepositoryMock
            .Setup(r => r.GetByLoginAsync("usuario"))
            .ReturnsAsync(usuario);

        // Act
        var result = await _authService.LoginAsync(request, "127.0.0.1", "TestMachine");

        // Assert
        result.Exitoso.Should().BeFalse();
        result.RequiereCambioPassword.Should().BeTrue();
        result.Mensaje.Should().Contain("primer acceso");
    }

    [Fact]
    public async Task LoginAsync_ConUsuarioExpirado_DebeRetornarPermisoExpirado()
    {
        // Arrange
        var request = new LoginRequest { Usuario = "usuarioexpirado", Password = "password123" };
        var usuarioExpirado = new Usuario
        {
            UsrLogUsr = "usuarioexpirado",
            UsrNomUsr = "Usuario Expirado",
            UsrPasUsr = "hashedpassword",
            UsrEstPas = 0,
            UsrFecExp = DateTime.Today.AddDays(-1) // Expirado ayer
        };

        _usuarioRepositoryMock
            .Setup(r => r.GetByLoginAsync("usuarioexpirado"))
            .ReturnsAsync(usuarioExpirado);

        // Act
        var result = await _authService.LoginAsync(request, "127.0.0.1", "TestMachine");

        // Assert
        result.Exitoso.Should().BeFalse();
        result.Mensaje.Should().Be("Permiso de acceso ha expirado");
    }

    [Fact]
    public async Task LoginAsync_ConPasswordIncorrecta_DebeIncrementarIntentos()
    {
        // Arrange
        var request = new LoginRequest { Usuario = "usuario", Password = "passwordincorrecta" };
        var usuario = new Usuario
        {
            UsrLogUsr = "usuario",
            UsrNomUsr = "Usuario Test",
            UsrPasUsr = BCrypt.Net.BCrypt.HashPassword("passwordcorrecta"),
            UsrEstPas = 0
        };

        _usuarioRepositoryMock
            .Setup(r => r.GetByLoginAsync("usuario"))
            .ReturnsAsync(usuario);
        _usuarioRepositoryMock
            .Setup(r => r.IncrementarIntentosErroneos("usuario"))
            .ReturnsAsync(1);

        // Act
        var result = await _authService.LoginAsync(request, "127.0.0.1", "TestMachine");

        // Assert
        result.Exitoso.Should().BeFalse();
        result.Mensaje.Should().Be("Acceso denegado");
        _usuarioRepositoryMock.Verify(r => r.IncrementarIntentosErroneos("usuario"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ConPasswordIncorrectaTresVeces_DebeBloquearUsuario()
    {
        // Arrange
        var request = new LoginRequest { Usuario = "usuario", Password = "passwordincorrecta" };
        var usuario = new Usuario
        {
            UsrLogUsr = "usuario",
            UsrNomUsr = "Usuario Test",
            UsrPasUsr = BCrypt.Net.BCrypt.HashPassword("passwordcorrecta"),
            UsrEstPas = 0
        };

        _usuarioRepositoryMock
            .Setup(r => r.GetByLoginAsync("usuario"))
            .ReturnsAsync(usuario);
        _usuarioRepositoryMock
            .Setup(r => r.IncrementarIntentosErroneos("usuario"))
            .ReturnsAsync(3); // Tercer intento

        // Act
        var result = await _authService.LoginAsync(request, "127.0.0.1", "TestMachine");

        // Assert
        result.Exitoso.Should().BeFalse();
        result.Mensaje.Should().Be("Usuario ha sido bloqueado");
        _usuarioRepositoryMock.Verify(r => r.BloquearUsuarioAsync("usuario"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ConCredencialesCorrectas_DebeRetornarTokenYUsuario()
    {
        // Arrange
        var passwordCorrecto = "password123";
        var request = new LoginRequest { Usuario = "usuario", Password = passwordCorrecto };
        var usuario = new Usuario
        {
            UsrLogUsr = "usuario",
            UsrNomUsr = "Usuario Test",
            UsrPasUsr = BCrypt.Net.BCrypt.HashPassword(passwordCorrecto),
            UsrEstPas = 0,
            UsrCodPer = 1,
            UsrCodEje = 1,
            UsrCodSuc = 1
        };

        _usuarioRepositoryMock
            .Setup(r => r.GetByLoginAsync("usuario"))
            .ReturnsAsync(usuario);
        _usuarioRepositoryMock
            .Setup(r => r.GetMenuAutorizadoAsync(1))
            .ReturnsAsync(new List<MenuOpcion>());

        // Act
        var result = await _authService.LoginAsync(request, "127.0.0.1", "TestMachine");

        // Assert
        result.Exitoso.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.TokenExpira.Should().BeAfter(DateTime.Now);
        result.Usuario.Should().NotBeNull();
        result.Usuario!.Login.Should().Be("usuario");
        result.Usuario.Nombre.Should().Be("Usuario Test");
        _usuarioRepositoryMock.Verify(r => r.ResetearIntentosAsync("usuario"), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ConRecordarSesion_DebeRetornarRefreshToken()
    {
        // Arrange
        var passwordCorrecto = "password123";
        var request = new LoginRequest
        {
            Usuario = "usuario",
            Password = passwordCorrecto,
            RecordarSesion = true
        };
        var usuario = new Usuario
        {
            UsrLogUsr = "usuario",
            UsrNomUsr = "Usuario Test",
            UsrPasUsr = BCrypt.Net.BCrypt.HashPassword(passwordCorrecto),
            UsrEstPas = 0
        };

        _usuarioRepositoryMock
            .Setup(r => r.GetByLoginAsync("usuario"))
            .ReturnsAsync(usuario);
        _usuarioRepositoryMock
            .Setup(r => r.GetMenuAutorizadoAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<MenuOpcion>());

        // Act
        var result = await _authService.LoginAsync(request, "127.0.0.1", "TestMachine");

        // Assert
        result.Exitoso.Should().BeTrue();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshTokenExpira.Should().BeAfter(DateTime.Now);
    }

    [Fact]
    public async Task LoginAsync_SinRecordarSesion_NoDebeRetornarRefreshToken()
    {
        // Arrange
        var passwordCorrecto = "password123";
        var request = new LoginRequest
        {
            Usuario = "usuario",
            Password = passwordCorrecto,
            RecordarSesion = false
        };
        var usuario = new Usuario
        {
            UsrLogUsr = "usuario",
            UsrNomUsr = "Usuario Test",
            UsrPasUsr = BCrypt.Net.BCrypt.HashPassword(passwordCorrecto),
            UsrEstPas = 0
        };

        _usuarioRepositoryMock
            .Setup(r => r.GetByLoginAsync("usuario"))
            .ReturnsAsync(usuario);
        _usuarioRepositoryMock
            .Setup(r => r.GetMenuAutorizadoAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<MenuOpcion>());

        // Act
        var result = await _authService.LoginAsync(request, "127.0.0.1", "TestMachine");

        // Assert
        result.Exitoso.Should().BeTrue();
        result.RefreshToken.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task LogoutAsync_DebeRegistrarSalidaEnBitacora()
    {
        // Arrange
        var usuario = "testuser";
        var maquina = "TestMachine";

        // Act
        await _authService.LogoutAsync(usuario, maquina);

        // Assert
        _bitacoraRepositoryMock.Verify(
            b => b.RegistrarSalidaAsync(usuario, maquina),
            Times.Once);
    }

    [Fact]
    public void RevocarRefreshToken_DebeEliminarToken()
    {
        // Este test verifica que no lance excepción al revocar un token
        // Arrange
        var refreshToken = "tokenquenoexiste";

        // Act & Assert - No debe lanzar excepción
        var exception = Record.Exception(() => _authService.RevocarRefreshToken(refreshToken));
        exception.Should().BeNull();
    }
}
