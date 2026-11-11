using Factoring.Core.Entities;

namespace Factoring.Core.Interfaces;

/// <summary>
/// Repositorio para autenticacion avanzada
/// </summary>
public interface IAuthRepository
{
    // ==================== REFRESH TOKENS ====================

    /// <summary>
    /// Guarda un nuevo refresh token
    /// </summary>
    Task<int> GuardarRefreshTokenAsync(RefreshToken token);

    /// <summary>
    /// Obtiene un refresh token por su valor
    /// </summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string token);

    /// <summary>
    /// Revoca un refresh token
    /// </summary>
    Task RevocarRefreshTokenAsync(string token, string? tokenReemplazo = null);

    /// <summary>
    /// Revoca todos los refresh tokens de un usuario
    /// </summary>
    Task RevocarTodosRefreshTokensAsync(string usuarioLogin);

    /// <summary>
    /// Limpia refresh tokens expirados
    /// </summary>
    Task LimpiarRefreshTokensExpiradosAsync();

    // ==================== SESIONES ====================

    /// <summary>
    /// Registra una nueva sesion
    /// </summary>
    Task<int> RegistrarSesionAsync(SesionUsuario sesion);

    /// <summary>
    /// Obtiene sesiones activas de un usuario
    /// </summary>
    Task<IEnumerable<SesionUsuario>> GetSesionesActivasAsync(string usuarioLogin);

    /// <summary>
    /// Cierra una sesion especifica
    /// </summary>
    Task CerrarSesionAsync(int sesionId);

    /// <summary>
    /// Cierra todas las sesiones de un usuario excepto la actual
    /// </summary>
    Task CerrarOtrasSesionesAsync(string usuarioLogin, string tokenJtiActual);

    /// <summary>
    /// Cierra todas las sesiones de un usuario
    /// </summary>
    Task CerrarTodasSesionesAsync(string usuarioLogin);

    /// <summary>
    /// Cuenta sesiones activas de un usuario
    /// </summary>
    Task<int> ContarSesionesActivasAsync(string usuarioLogin);

    // ==================== 2FA ====================

    /// <summary>
    /// Obtiene configuracion 2FA de un usuario
    /// </summary>
    Task<ConfiguracionTwoFactor?> GetConfiguracion2FAAsync(string usuarioLogin);

    /// <summary>
    /// Guarda o actualiza configuracion 2FA
    /// </summary>
    Task GuardarConfiguracion2FAAsync(ConfiguracionTwoFactor config);

    /// <summary>
    /// Desactiva 2FA para un usuario
    /// </summary>
    Task Desactivar2FAAsync(string usuarioLogin);

    /// <summary>
    /// Actualiza codigos de recuperacion
    /// </summary>
    Task ActualizarCodigosRecuperacionAsync(string usuarioLogin, string codigosJson);

    // ==================== INTENTOS DE LOGIN ====================

    /// <summary>
    /// Registra un intento de login
    /// </summary>
    Task RegistrarIntentoLoginAsync(IntentoLogin intento);

    /// <summary>
    /// Cuenta intentos fallidos en los ultimos N minutos
    /// </summary>
    Task<int> ContarIntentosFallidosAsync(string usuarioLogin, int minutos);

    /// <summary>
    /// Obtiene fecha del ultimo intento fallido
    /// </summary>
    Task<DateTime?> GetUltimoIntentoFallidoAsync(string usuarioLogin);

    /// <summary>
    /// Limpia intentos de login antiguos
    /// </summary>
    Task LimpiarIntentosAntiguosAsync(int diasAntiguedad);

    // ==================== POLITICAS DE PASSWORD ====================

    /// <summary>
    /// Obtiene las politicas de password activas
    /// </summary>
    Task<PoliticaPassword> GetPoliticaPasswordAsync();

    /// <summary>
    /// Actualiza las politicas de password
    /// </summary>
    Task ActualizarPoliticaPasswordAsync(PoliticaPassword politica);

    // ==================== HISTORIAL DE PASSWORD ====================

    /// <summary>
    /// Guarda password en historial
    /// </summary>
    Task GuardarPasswordEnHistorialAsync(string usuarioLogin, string passwordHash);

    /// <summary>
    /// Verifica si password esta en historial reciente
    /// </summary>
    Task<bool> PasswordEnHistorialAsync(string usuarioLogin, string passwordHash, int ultimasN);

    /// <summary>
    /// Limpia historial antiguo
    /// </summary>
    Task LimpiarHistorialAntiguoAsync(string usuarioLogin, int mantenerUltimasN);

    // ==================== BLOQUEO TEMPORAL ====================

    /// <summary>
    /// Registra bloqueo temporal de usuario
    /// </summary>
    Task RegistrarBloqueoTemporalAsync(string usuarioLogin, int minutosBloqueo);

    /// <summary>
    /// Verifica si usuario esta bloqueado temporalmente
    /// </summary>
    Task<(bool Bloqueado, DateTime? FinBloqueo)> VerificarBloqueoTemporalAsync(string usuarioLogin);

    /// <summary>
    /// Limpia bloqueos temporales expirados
    /// </summary>
    Task LimpiarBloqueosExpiradosAsync();
}
