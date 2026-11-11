using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Factoring.API.Controllers;

/// <summary>
/// Controlador de Autenticacion Avanzada
/// - Refresh tokens con rotacion
/// - Two-Factor Authentication (TOTP)
/// - Bloqueo por intentos fallidos
/// - Politicas de password configurables
/// - Control de sesiones concurrentes
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Autenticacion avanzada y gestion de sesiones")]
public class AuthController : ControllerBase
{
    private readonly AuthAdvancedService _authService;

    public AuthController(AuthAdvancedService authService)
    {
        _authService = authService;
    }

    private string GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    private string GetDispositivo() => Request.Headers["X-Machine-Name"].FirstOrDefault()
        ?? Request.Headers["User-Agent"].FirstOrDefault()?.Substring(0, Math.Min(100, Request.Headers["User-Agent"].FirstOrDefault()?.Length ?? 0))
        ?? "Unknown";
    private string GetUsuarioActual() => User.Identity?.Name ?? "";
    private string GetJtiActual() => User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value ?? "";

    // ==================== LOGIN ====================

    /// <summary>
    /// Login de usuario - Obtener token JWT
    /// </summary>
    /// <remarks>
    /// ## Proceso de Login
    ///
    /// 1. Si el login es exitoso y 2FA no esta habilitado, retorna el token JWT
    /// 2. Si el usuario tiene 2FA habilitado, retorna `Requiere2FA=true` y un `TokenTemporal2FA`
    /// 3. Con el token temporal, llamar a `/api/auth/verificar-2fa` con el codigo TOTP
    ///
    /// ## Bloqueo de Cuenta
    ///
    /// - Despues de N intentos fallidos (configurable), la cuenta se bloquea temporalmente
    /// - El bloqueo temporal dura M minutos (configurable)
    /// - Bloqueo permanente: contactar administrador
    ///
    /// ## Recordar Sesion
    ///
    /// Si `recordarSesion=true`, se incluye un refresh token para renovar el JWT sin credenciales.
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Autenticar usuario", Description = "Valida credenciales y retorna token JWT")]
    [ProducesResponseType(typeof(LoginAdvancedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginAdvancedResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginAdvancedResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request, GetIp(), GetDispositivo());

        if (!response.Exitoso && !response.RequiereCambioPassword && !response.Requiere2FA)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Verificar codigo 2FA
    /// </summary>
    /// <remarks>
    /// Usar despues de login cuando `Requiere2FA=true`.
    ///
    /// Se acepta:
    /// - Codigo TOTP de 6 digitos de la app autenticadora
    /// - Codigo de recuperacion de 10 caracteres
    /// </remarks>
    [HttpPost("verificar-2fa")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Verificar 2FA", Description = "Completa el login verificando el codigo 2FA")]
    [ProducesResponseType(typeof(LoginAdvancedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginAdvancedResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginAdvancedResponse>> Verificar2FA([FromBody] Verificar2FARequest request)
    {
        var response = await _authService.Verificar2FAAsync(request, GetIp(), GetDispositivo());

        if (!response.Exitoso)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Renovar token JWT usando refresh token
    /// </summary>
    /// <remarks>
    /// ## Seguridad
    ///
    /// - El refresh token se rota en cada uso (el anterior se invalida)
    /// - Si un refresh token revocado se usa, todas las sesiones del usuario se cierran
    /// - Si el token expiro, el usuario debe hacer login nuevamente
    /// </remarks>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Renovar token", Description = "Renueva el JWT usando refresh token con rotacion")]
    [ProducesResponseType(typeof(LoginAdvancedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginAdvancedResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginAdvancedResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken, GetIp(), GetDispositivo());

        if (!response.Exitoso)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Cerrar sesion
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [SwaggerOperation(Summary = "Cerrar sesion", Description = "Cierra la sesion actual")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromQuery] string? refreshToken = null)
    {
        await _authService.LogoutAsync(GetUsuarioActual(), GetDispositivo(), refreshToken, GetJtiActual());
        return Ok(new { mensaje = "Sesion cerrada correctamente" });
    }

    /// <summary>
    /// Cambiar contrasena
    /// </summary>
    /// <remarks>
    /// La nueva password debe cumplir con las politicas configuradas.
    /// Usar endpoint GET /api/auth/politica-password para conocer los requisitos.
    /// </remarks>
    [HttpPost("cambiar-password")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Cambiar password", Description = "Cambia la password validando politicas")]
    [ProducesResponseType(typeof(LoginAdvancedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LoginAdvancedResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginAdvancedResponse>> CambiarPassword([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.NuevoPassword))
        {
            return BadRequest(new { mensaje = "Debe proporcionar la nueva password" });
        }

        request.CambiarPassword = true;
        var response = await _authService.LoginAsync(request, GetIp(), GetDispositivo());

        if (!response.Exitoso)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Validar password contra politicas
    /// </summary>
    [HttpPost("validar-password")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Validar password", Description = "Valida una password contra las politicas sin guardarla")]
    [ProducesResponseType(typeof(ValidacionPasswordResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidacionPasswordResponse>> ValidarPassword([FromBody] ValidarPasswordRequest request)
    {
        var resultado = await _authService.ValidarPasswordAsync(request.Password);
        return Ok(resultado);
    }

    // ==================== 2FA ====================

    /// <summary>
    /// Generar configuracion 2FA
    /// </summary>
    /// <remarks>
    /// Retorna el secreto TOTP y codigos de recuperacion.
    /// El usuario debe escanear el QR con una app como Google Authenticator.
    /// Luego llamar a POST /api/auth/2fa/habilitar con un codigo generado para activar.
    ///
    /// **IMPORTANTE**: Los codigos de recuperacion solo se muestran una vez.
    /// </remarks>
    [HttpPost("2fa/setup")]
    [Authorize]
    [SwaggerOperation(Summary = "Configurar 2FA", Description = "Genera secreto TOTP y codigos de recuperacion")]
    [ProducesResponseType(typeof(Setup2FAResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<Setup2FAResponse>> Setup2FA()
    {
        var response = await _authService.GenerarConfiguracion2FAAsync(GetUsuarioActual());
        return Ok(response);
    }

    /// <summary>
    /// Habilitar 2FA
    /// </summary>
    /// <remarks>
    /// Despues de escanear el QR, enviar un codigo generado para activar 2FA.
    /// </remarks>
    [HttpPost("2fa/habilitar")]
    [Authorize]
    [SwaggerOperation(Summary = "Habilitar 2FA", Description = "Activa 2FA verificando un codigo TOTP")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Habilitar2FA([FromBody] Habilitar2FARequest request)
    {
        var exito = await _authService.Habilitar2FAAsync(GetUsuarioActual(), request.CodigoVerificacion);

        if (!exito)
        {
            return BadRequest(new { mensaje = "Codigo incorrecto. Verifique la configuracion." });
        }

        return Ok(new { mensaje = "2FA habilitado correctamente" });
    }

    /// <summary>
    /// Deshabilitar 2FA
    /// </summary>
    /// <remarks>
    /// Requiere la password actual y opcionalmente un codigo de recuperacion.
    /// </remarks>
    [HttpPost("2fa/deshabilitar")]
    [Authorize]
    [SwaggerOperation(Summary = "Deshabilitar 2FA", Description = "Desactiva 2FA con verificacion de password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deshabilitar2FA([FromBody] Deshabilitar2FARequest request)
    {
        var exito = await _authService.Deshabilitar2FAAsync(GetUsuarioActual(), request.Password, request.CodigoRecuperacion);

        if (!exito)
        {
            return BadRequest(new { mensaje = "No se pudo deshabilitar 2FA. Verifique su password." });
        }

        return Ok(new { mensaje = "2FA deshabilitado correctamente" });
    }

    /// <summary>
    /// Estado de 2FA del usuario
    /// </summary>
    [HttpGet("2fa/estado")]
    [Authorize]
    [SwaggerOperation(Summary = "Estado 2FA", Description = "Obtiene el estado de 2FA del usuario")]
    [ProducesResponseType(typeof(Estado2FAResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<Estado2FAResponse>> Estado2FA()
    {
        var estado = await _authService.GetEstado2FAAsync(GetUsuarioActual());
        return Ok(estado);
    }

    // ==================== SESIONES ====================

    /// <summary>
    /// Listar sesiones activas
    /// </summary>
    [HttpGet("sesiones")]
    [Authorize]
    [SwaggerOperation(Summary = "Listar sesiones", Description = "Obtiene todas las sesiones activas del usuario")]
    [ProducesResponseType(typeof(IEnumerable<SesionActivaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SesionActivaDto>>> GetSesiones()
    {
        var sesiones = await _authService.GetSesionesActivasAsync(GetUsuarioActual(), GetJtiActual());
        return Ok(sesiones);
    }

    /// <summary>
    /// Cerrar una sesion especifica
    /// </summary>
    [HttpDelete("sesiones/{sesionId}")]
    [Authorize]
    [SwaggerOperation(Summary = "Cerrar sesion", Description = "Cierra una sesion especifica por ID")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CerrarSesion(int sesionId)
    {
        await _authService.CerrarSesionAsync(sesionId);
        return Ok(new { mensaje = "Sesion cerrada" });
    }

    /// <summary>
    /// Cerrar todas las otras sesiones
    /// </summary>
    [HttpPost("sesiones/cerrar-otras")]
    [Authorize]
    [SwaggerOperation(Summary = "Cerrar otras sesiones", Description = "Cierra todas las sesiones excepto la actual")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CerrarOtrasSesiones()
    {
        await _authService.CerrarOtrasSesionesAsync(GetUsuarioActual(), GetJtiActual());
        return Ok(new { mensaje = "Otras sesiones cerradas" });
    }

    // ==================== POLITICAS ====================

    /// <summary>
    /// Obtener politicas de password
    /// </summary>
    [HttpGet("politica-password")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Politicas de password", Description = "Obtiene las politicas de password vigentes")]
    [ProducesResponseType(typeof(PoliticaPasswordDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PoliticaPasswordDto>> GetPoliticaPassword()
    {
        var politica = await _authService.GetPoliticaPasswordAsync();
        return Ok(politica);
    }

    /// <summary>
    /// Actualizar politicas de password (solo admin)
    /// </summary>
    [HttpPut("politica-password")]
    [Authorize]
    [SwaggerOperation(Summary = "Actualizar politicas", Description = "Actualiza las politicas de password (requiere perfil admin)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActualizarPoliticaPassword([FromBody] PoliticaPassword politica)
    {
        // Verificar que sea administrador (perfil 1)
        var perfilClaim = User.Claims.FirstOrDefault(c => c.Type == "perfil")?.Value;
        if (perfilClaim != "1")
        {
            return Forbid();
        }

        await _authService.ActualizarPoliticaPasswordAsync(politica);
        return Ok(new { mensaje = "Politicas actualizadas" });
    }

    // ==================== BLOQUEO ====================

    /// <summary>
    /// Estado de bloqueo de una cuenta
    /// </summary>
    [HttpGet("bloqueo/{usuario}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Estado de bloqueo", Description = "Verifica si una cuenta esta bloqueada")]
    [ProducesResponseType(typeof(EstadoBloqueoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EstadoBloqueoDto>> GetEstadoBloqueo(string usuario)
    {
        var estado = await _authService.GetEstadoBloqueoAsync(usuario);
        return Ok(estado);
    }

    /// <summary>
    /// Desbloquear usuario (solo admin)
    /// </summary>
    [HttpPost("desbloquear/{usuario}")]
    [Authorize]
    [SwaggerOperation(Summary = "Desbloquear usuario", Description = "Desbloquea un usuario (requiere perfil admin)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DesbloquearUsuario(string usuario)
    {
        var perfilClaim = User.Claims.FirstOrDefault(c => c.Type == "perfil")?.Value;
        if (perfilClaim != "1")
        {
            return Forbid();
        }

        await _authService.DesbloquearUsuarioAsync(usuario);
        return Ok(new { mensaje = "Usuario desbloqueado" });
    }
}
