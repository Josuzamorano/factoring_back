namespace Factoring.Core.DTOs;

/// <summary>
/// Respuesta del proceso de autenticacion
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Indica si la autenticacion fue exitosa
    /// </summary>
    public bool Exitoso { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string? Mensaje { get; set; }

    /// <summary>
    /// Token JWT para autenticacion de requests
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Refresh token para renovar el JWT (solo si RecordarSesion=true)
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Fecha de expiracion del token JWT
    /// </summary>
    public DateTime? TokenExpira { get; set; }

    /// <summary>
    /// Fecha de expiracion del refresh token
    /// </summary>
    public DateTime? RefreshTokenExpira { get; set; }

    /// <summary>
    /// Datos del usuario autenticado
    /// </summary>
    public UsuarioDto? Usuario { get; set; }

    /// <summary>
    /// Menu de opciones autorizadas para el usuario
    /// </summary>
    public List<MenuOpcionDto>? Menu { get; set; }

    /// <summary>
    /// Indica si el usuario debe cambiar su contrasena
    /// </summary>
    public bool RequiereCambioPassword { get; set; }
}

public class UsuarioDto
{
    public string Login { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int? CodigoPerfil { get; set; }
    public int? CodigoEjecutivo { get; set; }
    public int? CodigoSucursal { get; set; }
}

public class MenuOpcionDto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Ruta { get; set; }
    public string? Icono { get; set; }
    public int Orden { get; set; }
    public List<MenuOpcionDto>? SubMenu { get; set; }
}
