namespace Factoring.Core.DTOs;

/// <summary>
/// Request para autenticacion de usuario
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Nombre de usuario (login)
    /// </summary>
    public string Usuario { get; set; } = string.Empty;

    /// <summary>
    /// Contrasena del usuario
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Indica si debe cambiar la contrasena
    /// </summary>
    public bool CambiarPassword { get; set; }

    /// <summary>
    /// Nueva contrasena (si CambiarPassword es true)
    /// </summary>
    public string? NuevoPassword { get; set; }

    /// <summary>
    /// Indica si debe recordar la sesion (genera refresh token de larga duracion)
    /// </summary>
    public bool RecordarSesion { get; set; }
}

/// <summary>
/// Request para refrescar el token JWT usando refresh token
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Token de refresco obtenido en el login con RecordarSesion=true
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
