// ====================================================
// DTOs del Módulo Comercial
// Migrado de: F_CM1000.frm, F_CM1200.frm, F_CM1400.frm, 
//             F_CM1500.frm, F_CM1700.frm, F_CM1800.frm
// ====================================================

using System.ComponentModel.DataAnnotations;

namespace Factoring.Core.DTOs;

#region Excedentes DTOs

/// <summary>
/// DTO para mostrar excedente
/// </summary>
public class ExcedenteDto
{
    public int NumeroExcedente { get; set; }
    public long RutCliente { get; set; }
    public string RutClienteFormateado { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public int TipoDocumento { get; set; }
    public string TipoDocumentoDescripcion { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public DateTime? FechaGeneracion { get; set; }
    public decimal MontoExcedente { get; set; }
    public decimal SaldoExcedente { get; set; }
    public int Estado { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
}

/// <summary>
/// DTO para crear/aplicar excedente
/// </summary>
public class AplicarExcedenteDto
{
    [Required]
    public int NumeroExcedente { get; set; }
    
    [Required]
    public string NumeroDocumento { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal MontoAplicar { get; set; }
    
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para filtrar excedentes
/// </summary>
public class FiltroExcedenteDto
{
    public long? RutCliente { get; set; }
    public int? TipoDocumento { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? Estado { get; set; }
    public bool SoloConSaldo { get; set; } = true;
}

/// <summary>
/// DTO resumen de excedentes por cliente
/// </summary>
public class ResumenExcedenteDto
{
    public long RutCliente { get; set; }
    public string RutClienteFormateado { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public decimal MontoTotalExcedentes { get; set; }
    public decimal SaldoTotalExcedentes { get; set; }
    public int CantidadExcedentes { get; set; }
}

/// <summary>
/// DTO detalle excedente generado
/// </summary>
public class ExcedenteDetalleDto
{
    public int NumeroExcedente { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public DateTime? FechaGeneracion { get; set; }
    public decimal MontoExcedente { get; set; }
    public decimal SaldoExcedente { get; set; }
    public bool Seleccionado { get; set; }
}

#endregion

#region Prórroga DTOs

/// <summary>
/// DTO para mostrar prórroga
/// </summary>
public class ProrrogaDto
{
    public int NumeroProrrroga { get; set; }
    public int TipoDocumento { get; set; }
    public string TipoDocumentoDescripcion { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public int NumeroOperacion { get; set; }
    public long RutCliente { get; set; }
    public string RutClienteFormateado { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string RutDeudorFormateado { get; set; } = string.Empty;
    public string NombreDeudor { get; set; } = string.Empty;
    public DateTime? FechaVencimientoOriginal { get; set; }
    public DateTime? FechaVencimientoNueva { get; set; }
    public int DiasProrrroga { get; set; }
    public decimal PorcentajeProrrroga { get; set; }
    public decimal MontoAbono { get; set; }
    public int TipoAbono { get; set; }
    public string? Observaciones { get; set; }
    public int Estado { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
    public DateTime? FechaSolicitud { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? UsuarioSolicita { get; set; }
    public string? UsuarioAprueba { get; set; }
}

/// <summary>
/// DTO para crear prórroga
/// </summary>
public class CreateProrrogaDto
{
    [Required]
    public int TipoDocumento { get; set; }
    
    [Required]
    public string NumeroDocumento { get; set; } = string.Empty;
    
    [Required]
    public int NumeroOperacion { get; set; }
    
    [Required]
    public DateTime? FechaVencimientoNueva { get; set; }
    
    [Range(1, 365, ErrorMessage = "Los días de prórroga deben estar entre 1 y 365")]
    public int DiasProrrroga { get; set; }
    
    [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
    public decimal PorcentajeProrrroga { get; set; }
    
    public decimal MontoAbono { get; set; }
    
    public int TipoAbono { get; set; }
    
    [MaxLength(500)]
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para filtrar prórrogas
/// </summary>
public class FiltroProrrogaDto
{
    public long? RutCliente { get; set; }
    public long? RutDeudor { get; set; }
    public int? TipoDocumento { get; set; }
    public int? Estado { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}

/// <summary>
/// DTO para aprobar/rechazar prórroga
/// </summary>
public class AccionProrrogaDto
{
    [Required]
    public int NumeroProrrroga { get; set; }
    
    [Required]
    public bool Aprobar { get; set; }
    
    public string? Observaciones { get; set; }
}

#endregion

#region Renovación DTOs

/// <summary>
/// DTO para mostrar renovación
/// </summary>
public class RenovacionDto
{
    public int NumeroRenovacion { get; set; }
    public int TipoDocumentoOriginal { get; set; }
    public string TipoDocumentoOriginalDescripcion { get; set; } = string.Empty;
    public string NumeroDocumentoOriginal { get; set; } = string.Empty;
    public int NumeroOperacionOriginal { get; set; }
    public int TipoDocumentoNuevo { get; set; }
    public string TipoDocumentoNuevoDescripcion { get; set; } = string.Empty;
    public string NumeroDocumentoNuevo { get; set; } = string.Empty;
    public int? NumeroOperacionNueva { get; set; }
    public long RutCliente { get; set; }
    public string RutClienteFormateado { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string RutDeudorFormateado { get; set; } = string.Empty;
    public string NombreDeudor { get; set; } = string.Empty;
    public decimal MontoOriginal { get; set; }
    public decimal MontoNuevo { get; set; }
    public DateTime? FechaVencimientoOriginal { get; set; }
    public DateTime? FechaVencimientoNueva { get; set; }
    public int DiasPlazo { get; set; }
    public string? Observaciones { get; set; }
    public int Estado { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
    public DateTime? FechaRenovacion { get; set; }
    public string? UsuarioRegistra { get; set; }
}

/// <summary>
/// DTO para crear renovación
/// </summary>
public class CreateRenovacionDto
{
    [Required]
    public int TipoDocumentoOriginal { get; set; }
    
    [Required]
    public string NumeroDocumentoOriginal { get; set; } = string.Empty;
    
    [Required]
    public int NumeroOperacionOriginal { get; set; }
    
    [Required]
    public int TipoDocumentoNuevo { get; set; }
    
    [Required]
    public string NumeroDocumentoNuevo { get; set; } = string.Empty;
    
    [Required]
    public decimal MontoNuevo { get; set; }
    
    [Required]
    public DateTime? FechaVencimientoNueva { get; set; }
    
    [Range(1, 365)]
    public int DiasPlazo { get; set; }
    
    [MaxLength(500)]
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para filtrar renovaciones
/// </summary>
public class FiltroRenovacionDto
{
    public long? RutCliente { get; set; }
    public long? RutDeudor { get; set; }
    public int? TipoDocumento { get; set; }
    public int? Estado { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}

#endregion

#region Informe Documentos DTOs

/// <summary>
/// DTO para informe de documentos por cliente
/// </summary>
public class DocumentoClienteDto
{
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public string TipoDocumentoDescripcion { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public decimal MontoDocumento { get; set; }
    public decimal SaldoDocumento { get; set; }
    public DateTime? FechaOperacion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int DiasVencido { get; set; }
    public int Estado { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
}

/// <summary>
/// DTO para filtrar documentos cliente
/// </summary>
public class FiltroDocumentoClienteDto
{
    [Required]
    public long RutCliente { get; set; }
    
    public int? TipoDocumento { get; set; }
    public DateTime? FechaOperacionDesde { get; set; }
    public DateTime? FechaOperacionHasta { get; set; }
    public DateTime? FechaVencimientoDesde { get; set; }
    public DateTime? FechaVencimientoHasta { get; set; }
    public int? Estado { get; set; }
    public bool? SoloVencidos { get; set; }
}

/// <summary>
/// DTO resumen documentos cliente
/// </summary>
public class ResumenDocumentoClienteDto
{
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public int TotalDocumentos { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal SaldoTotal { get; set; }
    public int DocumentosVencidos { get; set; }
    public decimal MontoVencido { get; set; }
    public int DocumentosPorVencer { get; set; }
    public decimal MontoPorVencer { get; set; }
}

#endregion

#region Consulta Otorgamiento DTOs

/// <summary>
/// DTO para consulta de otorgamiento
/// </summary>
public class ConsultaOtorgamientoDto
{
    public int NumeroOperacion { get; set; }
    public long RutCliente { get; set; }
    public string RutClienteFormateado { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime? FechaOperacion { get; set; }
    public DateTime? FechaOtorgamiento { get; set; }
    public decimal MontoOperacion { get; set; }
    public decimal MontoOtorgado { get; set; }
    public int EstadoOperacion { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
    public int TipoMoneda { get; set; }
    public string MonedaDescripcion { get; set; } = string.Empty;
    public int CantidadDocumentos { get; set; }
}

/// <summary>
/// DTO para filtrar consulta otorgamiento
/// </summary>
public class FiltroOtorgamientoDto
{
    public long? RutCliente { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? Estado { get; set; }
    public int? TipoMoneda { get; set; }
}

#endregion

#region Cartola Cliente DTOs

/// <summary>
/// DTO para cartola de cliente
/// </summary>
public class CartolaClienteDto
{
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime? Fecha { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public string TipoMovimientoDescripcion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public decimal Cargo { get; set; }
    public decimal Abono { get; set; }
    public decimal Saldo { get; set; }
}

/// <summary>
/// DTO para filtrar cartola cliente
/// </summary>
public class FiltroCartolaClienteDto
{
    [Required]
    public long RutCliente { get; set; }
    
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public string? TipoMovimiento { get; set; }
}

/// <summary>
/// DTO resumen cartola cliente
/// </summary>
public class ResumenCartolaClienteDto
{
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public decimal TotalCargos { get; set; }
    public decimal TotalAbonos { get; set; }
    public decimal SaldoActual { get; set; }
    public int CantidadMovimientos { get; set; }
}

#endregion

#region Pizarra Comercial DTOs

/// <summary>
/// DTO para pizarra comercial (consulta de giros)
/// </summary>
public class PizarraComercialDto
{
    public int NumeroGiro { get; set; }
    public int NumeroOperacion { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime? FechaGiro { get; set; }
    public decimal MontoGiro { get; set; }
    public int TipoPago { get; set; }
    public string TipoPagoDescripcion { get; set; } = string.Empty;
    public int Estado { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
}

/// <summary>
/// DTO para filtrar pizarra comercial
/// </summary>
public class FiltroPizarraDto
{
    public long? RutCliente { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? Estado { get; set; }
    public int? TipoPago { get; set; }
}

#endregion
