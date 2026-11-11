namespace Factoring.Core.Entities;

/// <summary>
/// Entidad Usuario - Migrado de tabla USR en VB6
/// </summary>
public class Usuario
{
    public string UsrLogUsr { get; set; } = string.Empty; // Login usuario
    public string UsrPasUsr { get; set; } = string.Empty; // Password
    public string UsrNomUsr { get; set; } = string.Empty; // Nombre
    public int UsrEstPas { get; set; } // Estado password: 0=Normal, 1=Admin solicita cambio, 2=Primer acceso, 3=Bloqueado
    public int UsrIntErr { get; set; } // Intentos erróneos
    public DateTime? UsrFecExp { get; set; } // Fecha expiración
    public DateTime? UsrUltCambio { get; set; } // Último cambio password
    public int? UsrCodPer { get; set; } // Código perfil
    public int? UsrCodEje { get; set; } // Código ejecutivo
    public int? UsrCodSuc { get; set; } // Código sucursal
}
