namespace Factoring.Core.Entities;

/// <summary>
/// Entidad Bitácora de acceso - Migrado de tabla BIT_IOS en VB6
/// </summary>
public class BitacoraAcceso
{
    public string BitCodUsr { get; set; } = string.Empty;
    public string BitNomMaq { get; set; } = string.Empty;
    public int BitEstAdo { get; set; }
    public string? BitIp { get; set; }
    public DateTime BitFecIns { get; set; }
    public string? BitHorIns { get; set; }
    public DateTime? BitFecOus { get; set; }
    public string? BitHorOus { get; set; }
    public string? BitObservacion { get; set; }
    public string? BitResultado { get; set; }
}
