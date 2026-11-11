namespace Factoring.Core.Entities;

/// <summary>
/// Entidad para almacenar refresh tokens en la base de datos
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UsuarioLogin { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public bool Revocado { get; set; }
    public DateTime? FechaRevocacion { get; set; }
    public string? TokenReemplazo { get; set; }
    public string? DireccionIp { get; set; }
    public string? Dispositivo { get; set; }
    public bool EstaActivo => !Revocado && FechaExpiracion > DateTime.Now;
}

/// <summary>
/// Entidad para sesiones activas del usuario
/// </summary>
public class SesionUsuario
{
    public int Id { get; set; }
    public string UsuarioLogin { get; set; } = string.Empty;
    public string TokenJti { get; set; } = string.Empty; // JWT ID unico
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string DireccionIp { get; set; } = string.Empty;
    public string Dispositivo { get; set; } = string.Empty;
    public bool Activa { get; set; }
}

/// <summary>
/// Configuracion de 2FA para un usuario
/// </summary>
public class ConfiguracionTwoFactor
{
    public string UsuarioLogin { get; set; } = string.Empty;
    public bool Habilitado { get; set; }
    public string? SecretoTotp { get; set; }
    public DateTime? FechaActivacion { get; set; }
    public string? CodigosRecuperacion { get; set; } // JSON array de codigos de respaldo
}

/// <summary>
/// Registro de intentos de login para bloqueo temporal
/// </summary>
public class IntentoLogin
{
    public int Id { get; set; }
    public string UsuarioLogin { get; set; } = string.Empty;
    public DateTime FechaIntento { get; set; }
    public bool Exitoso { get; set; }
    public string DireccionIp { get; set; } = string.Empty;
    public string? MotivoFallo { get; set; }
}

/// <summary>
/// Politicas de contrasena configurables
/// </summary>
public class PoliticaPassword
{
    public int Id { get; set; }
    public int LongitudMinima { get; set; } = 8;
    public int LongitudMaxima { get; set; } = 128;
    public bool RequiereMayusculas { get; set; } = true;
    public bool RequiereMinusculas { get; set; } = true;
    public bool RequiereNumeros { get; set; } = true;
    public bool RequiereCaracteresEspeciales { get; set; } = true;
    public int DiasVigencia { get; set; } = 90;
    public int HistorialNoRepetir { get; set; } = 5; // No repetir ultimas N passwords
    public int MaxIntentosAntesBloqueo { get; set; } = 3;
    public int MinutosBloqueoTemporal { get; set; } = 15;
    public int MaxSesionesConcurrentes { get; set; } = 3;
    public bool Activa { get; set; } = true;
}

/// <summary>
/// Historial de passwords para evitar reutilizacion
/// </summary>
public class HistorialPassword
{
    public int Id { get; set; }
    public string UsuarioLogin { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime FechaCambio { get; set; }
}
