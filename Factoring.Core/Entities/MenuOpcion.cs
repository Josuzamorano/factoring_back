namespace Factoring.Core.Entities;

/// <summary>
/// Entidad Opción de Menú - Migrado de tabla PUE (Puertas) en VB6
/// Los iconos del menú principal se cargan según permisos del perfil
/// </summary>
public class MenuOpcion
{
    public int PueCodPue { get; set; }
    public string PueTipObj { get; set; } = string.Empty; // 'I' = Icono
    public int PueNumPue { get; set; }
    public string PueNombre { get; set; } = string.Empty;
    public string? PueDescripcion { get; set; }
    public int? PuePadre { get; set; } // Para submenús
    public string? PueRuta { get; set; } // Ruta en Angular
    public string? PueIcono { get; set; } // Icono Material
    public int PueOrden { get; set; }
    public bool PueActivo { get; set; }
}
