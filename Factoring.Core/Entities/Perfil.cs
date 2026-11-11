namespace Factoring.Core.Entities;

/// <summary>
/// Entidad Perfil de usuario - glbPerfil en VB6
/// </summary>
public class Perfil
{
    public int PerCodPer { get; set; }
    public string PerNomPer { get; set; } = string.Empty;
    public string? PerDescripcion { get; set; }
    public bool PerActivo { get; set; }
}
