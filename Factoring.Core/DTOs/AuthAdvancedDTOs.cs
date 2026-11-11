namespace Factoring.Core.DTOs;

// ==================== REQUEST DTOs ====================

/// <summary>
/// Request para verificar codigo 2FA
/// </summary>
public class Verificar2FARequest
{
    public string Usuario { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string? TokenTemporal { get; set; } // Token de sesion pendiente de 2FA
}

/// <summary>
/// Request para habilitar 2FA
/// </summary>
public class Habilitar2FARequest
{
    public string CodigoVerificacion { get; set; } = string.Empty;
}

/// <summary>
/// Request para deshabilitar 2FA
/// </summary>
public class Deshabilitar2FARequest
{
    public string Password { get; set; } = string.Empty;
    public string? CodigoRecuperacion { get; set; }
}

/// <summary>
/// Request para validar politicas de password
/// </summary>
public class ValidarPasswordRequest
{
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request para cerrar otras sesiones
/// </summary>
public class CerrarSesionesRequest
{
    public bool TodasMenosActual { get; set; } = true;
    public List<int>? SesionesEspecificas { get; set; }
}

// ==================== RESPONSE DTOs ====================

/// <summary>
/// Respuesta extendida de login con soporte 2FA
/// </summary>
public class LoginAdvancedResponse : LoginResponse
{
    /// <summary>
    /// Indica si se requiere verificacion 2FA
    /// </summary>
    public bool Requiere2FA { get; set; }

    /// <summary>
    /// Token temporal para completar 2FA (valido por 5 minutos)
    /// </summary>
    public string? TokenTemporal2FA { get; set; }

    /// <summary>
    /// ID de la sesion actual
    /// </summary>
    public int? SesionId { get; set; }

    /// <summary>
    /// Indica si el usuario esta bloqueado temporalmente
    /// </summary>
    public bool BloqueadoTemporalmente { get; set; }

    /// <summary>
    /// Minutos restantes de bloqueo temporal
    /// </summary>
    public int? MinutosRestantesBloqueo { get; set; }

    /// <summary>
    /// Intentos restantes antes del bloqueo
    /// </summary>
    public int? IntentosRestantes { get; set; }
}

/// <summary>
/// Respuesta al generar configuracion 2FA
/// </summary>
public class Setup2FAResponse
{
    public bool Exitoso { get; set; }
    public string? Mensaje { get; set; }

    /// <summary>
    /// Secreto TOTP en formato base32
    /// </summary>
    public string? Secreto { get; set; }

    /// <summary>
    /// URL para generar QR code (otpauth://)
    /// </summary>
    public string? QrCodeUrl { get; set; }

    /// <summary>
    /// Codigos de recuperacion (mostrar solo una vez)
    /// </summary>
    public List<string>? CodigosRecuperacion { get; set; }
}

/// <summary>
/// Estado de 2FA del usuario
/// </summary>
public class Estado2FAResponse
{
    public bool Habilitado { get; set; }
    public DateTime? FechaActivacion { get; set; }
    public int CodigosRecuperacionRestantes { get; set; }
}

/// <summary>
/// Resultado de validacion de password
/// </summary>
public class ValidacionPasswordResponse
{
    public bool EsValida { get; set; }
    public List<string> Errores { get; set; } = new();
    public int Fortaleza { get; set; } // 0-100
    public string NivelFortaleza { get; set; } = string.Empty; // Debil, Media, Fuerte, Muy Fuerte
}

/// <summary>
/// Lista de sesiones activas del usuario
/// </summary>
public class SesionActivaDto
{
    public int Id { get; set; }
    public DateTime FechaInicio { get; set; }
    public string DireccionIp { get; set; } = string.Empty;
    public string Dispositivo { get; set; } = string.Empty;
    public bool EsSesionActual { get; set; }
    public string TiempoActiva { get; set; } = string.Empty;
}

/// <summary>
/// Politicas de password actuales
/// </summary>
public class PoliticaPasswordDto
{
    public int LongitudMinima { get; set; }
    public int LongitudMaxima { get; set; }
    public bool RequiereMayusculas { get; set; }
    public bool RequiereMinusculas { get; set; }
    public bool RequiereNumeros { get; set; }
    public bool RequiereCaracteresEspeciales { get; set; }
    public int DiasVigencia { get; set; }
    public int MaxSesionesConcurrentes { get; set; }
}

/// <summary>
/// Informacion de bloqueo de cuenta
/// </summary>
public class EstadoBloqueoDto
{
    public bool Bloqueado { get; set; }
    public string TipoBloqueo { get; set; } = string.Empty; // "Temporal" o "Permanente"
    public DateTime? FinBloqueo { get; set; }
    public int? MinutosRestantes { get; set; }
    public int IntentosRealizados { get; set; }
    public int IntentosMaximos { get; set; }
}
