namespace Factoring.Core.Entities;

/// <summary>
/// Entidad Permiso de Menú - Migrado de tabla NPP en VB6
/// Relaciona perfiles con opciones de menú autorizadas
/// </summary>
public class PermisoMenu
{
    public int NppCodPer { get; set; } // Código Perfil
    public int NppCodPue { get; set; } // Código Puerta/Menú
}
