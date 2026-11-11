using System;
using System.ComponentModel.DataAnnotations;

namespace Factoring.Core.DTOs;

#region Línea de Crédito DTOs

/// <summary>
/// DTO para Línea de Crédito
/// </summary>
public class LineaCreditoDto
{
    public int RutCliente { get; set; }
    public int NumeroLinea { get; set; }
    public int TipoMoneda { get; set; }
    public string DescripcionMoneda { get; set; } = string.Empty;
    public DateTime? FechaSolicitud { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public int EstadoSolicitud { get; set; }
    public string DescripcionEstadoSolicitud { get; set; } = string.Empty;
    public decimal TasaNegocio { get; set; }
    public decimal Spread { get; set; }
    public decimal MontoSolicitado { get; set; }
    public decimal MontoAprobado { get; set; }
    public decimal MontoDisponible { get; set; }
    public int EstadoLinea { get; set; }
    public string DescripcionEstadoLinea { get; set; } = string.Empty;
    public string? NombreCliente { get; set; }
    public string? CategoriaRiesgo { get; set; }
    
    // Contrato
    public int? NumeroContrato { get; set; }
    public int? TipoContrato { get; set; }
    public DateTime? FechaContrato { get; set; }
    public DateTime? VencimientoContrato { get; set; }
    public int? EstadoContrato { get; set; }
    public string? ObservacionesContrato { get; set; }
    public int? CodigoNotaria { get; set; }
    public string? NombreNotaria { get; set; }
    
    // Mora
    public decimal? TasaMora { get; set; }
    public int? DiasMora { get; set; }
    public int? CalculoMora { get; set; }
    
    // Calculados
    public decimal MontoUtilizado => MontoAprobado - MontoDisponible;
    public decimal PorcentajeUtilizado => MontoAprobado > 0 ? Math.Round((MontoUtilizado / MontoAprobado) * 100, 2) : 0;
}

/// <summary>
/// DTO para crear línea de crédito
/// </summary>
public class CreateLineaCreditoDto
{
    [Required]
    public int RutCliente { get; set; }
    
    [Required]
    public int TipoMoneda { get; set; } = 1; // Default: Peso
    
    [Required]
    public DateTime? FechaSolicitud { get; set; } = DateTime.Today;
    
    public DateTime? FechaResolucion { get; set; }
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    
    [Required]
    public int EstadoSolicitud { get; set; } = 1; // Default: Creada
    
    [Range(0, 100)]
    public decimal TasaNegocio { get; set; }
    
    [Range(0, 100)]
    public decimal Spread { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal MontoSolicitado { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal MontoAprobado { get; set; }
    
    [Required]
    public int EstadoLinea { get; set; } = 1; // Default: Creada
    
    // Contrato
    public int? TipoContrato { get; set; }
    public DateTime? FechaContrato { get; set; }
    public DateTime? VencimientoContrato { get; set; }
    public int? EstadoContrato { get; set; }
    public string? ObservacionesContrato { get; set; }
    public int? CodigoNotaria { get; set; }
    
    // Mora
    public decimal? TasaMora { get; set; }
    public int? DiasMora { get; set; }
    public int? CalculoMora { get; set; }
    
    // Comisiones (para gerencia)
    public decimal? ComisionApertura { get; set; }
    public decimal? ComisionRenovacion { get; set; }
    public decimal? ComisionExceso { get; set; }
}

/// <summary>
/// DTO para actualizar línea de crédito
/// </summary>
public class UpdateLineaCreditoDto
{
    public DateTime? FechaResolucion { get; set; }
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public int? EstadoSolicitud { get; set; }
    
    [Range(0, 100)]
    public decimal? TasaNegocio { get; set; }
    
    [Range(0, 100)]
    public decimal? Spread { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MontoSolicitado { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MontoAprobado { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? MontoDisponible { get; set; }
    
    public int? EstadoLinea { get; set; }
    
    // Contrato
    public int? NumeroContrato { get; set; }
    public int? TipoContrato { get; set; }
    public DateTime? FechaContrato { get; set; }
    public DateTime? VencimientoContrato { get; set; }
    public int? EstadoContrato { get; set; }
    public string? ObservacionesContrato { get; set; }
    public int? CodigoNotaria { get; set; }
    
    // Mora
    public decimal? TasaMora { get; set; }
    public int? DiasMora { get; set; }
    public int? CalculoMora { get; set; }
    
    // Comisiones
    public decimal? ComisionApertura { get; set; }
    public decimal? ComisionRenovacion { get; set; }
    public decimal? ComisionExceso { get; set; }
}

/// <summary>
/// DTO para filtrar líneas de crédito
/// </summary>
public class FiltroLineaCreditoDto
{
    public int? RutCliente { get; set; }
    public int? EstadoLinea { get; set; }
    public int? EstadoSolicitud { get; set; }
    public int? TipoMoneda { get; set; }
    public DateTime? FechaSolicitudDesde { get; set; }
    public DateTime? FechaSolicitudHasta { get; set; }
    public DateTime? FechaVigenciaDesde { get; set; }
    public DateTime? FechaVigenciaHasta { get; set; }
    public DateTime? FechaAprobacionDesde { get; set; }
    public DateTime? FechaAprobacionHasta { get; set; }
    public DateTime? FechaVencimientoDesde { get; set; }
    public DateTime? FechaVencimientoHasta { get; set; }
    public string? TipoFiltroFecha { get; set; } // "solicitud", "aprobacion", "vencimiento"
}

/// <summary>
/// DTO para aprobar/rechazar línea
/// </summary>
public class AccionLineaCreditoDto
{
    [Required]
    public int RutCliente { get; set; }
    
    [Required]
    public int NumeroLinea { get; set; }
    
    [Required]
    public string Accion { get; set; } = string.Empty; // "aprobar", "rechazar", "suspender", "renovar"
    
    public decimal? MontoAprobado { get; set; }
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public string? Observaciones { get; set; }
    
    // Comisiones para renovación
    public decimal? ComisionRenovacion { get; set; }
}

#endregion

#region Sublínea Producto DTOs

/// <summary>
/// DTO para Sublínea por Producto
/// </summary>
public class SublineaProductoDto
{
    public int RutCliente { get; set; }
    public int NumeroLinea { get; set; }
    public int TipoDocumento { get; set; }
    public string DescripcionTipoDocumento { get; set; } = string.Empty;
    public int TipoMoneda { get; set; }
    public string DescripcionMoneda { get; set; } = string.Empty;
    public decimal PorcentajeAnticipo { get; set; }
    public decimal MontoSolicitado { get; set; }
    public decimal MontoAprobado { get; set; }
    public decimal MontoDisponible { get; set; }
    
    // Contrato
    public int? NumeroContrato { get; set; }
    public DateTime? FechaContrato { get; set; }
    public DateTime? VencimientoContrato { get; set; }
    public int? EstadoContrato { get; set; }
    public string? DescripcionEstadoContrato { get; set; }
    
    // Plazos
    public int? PlazoVencimiento { get; set; }
    public int? PlazoCesion { get; set; }
    
    // Comisiones
    public decimal? ComisionMinima { get; set; }
    public decimal? ComisionMaxima { get; set; }
    public decimal? ComisionPorcentual { get; set; }
}

/// <summary>
/// DTO para crear sublínea por producto
/// </summary>
public class CreateSublineaProductoDto
{
    [Required]
    public int RutCliente { get; set; }
    
    [Required]
    public int NumeroLinea { get; set; }
    
    [Required]
    public int TipoDocumento { get; set; }
    
    [Required]
    public int TipoMoneda { get; set; } = 1;
    
    [Range(0.01, 100)]
    public decimal PorcentajeAnticipo { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal MontoSolicitado { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal MontoAprobado { get; set; }
    
    // Contrato
    public int? NumeroContrato { get; set; }
    public DateTime? FechaContrato { get; set; }
    public DateTime? VencimientoContrato { get; set; }
    public int? EstadoContrato { get; set; }
    
    // Plazos
    public int? PlazoVencimiento { get; set; }
    public int? PlazoCesion { get; set; }
    
    // Comisiones
    public decimal? ComisionMinima { get; set; }
    public decimal? ComisionMaxima { get; set; }
    public decimal? ComisionPorcentual { get; set; }
}

#endregion

#region Sublínea Deudor DTOs

/// <summary>
/// DTO para Sublínea por Deudor
/// </summary>
public class SublineaDeudorDto
{
    public int RutCliente { get; set; }
    public int NumeroLinea { get; set; }
    public int RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public int TipoDocumento { get; set; }
    public string DescripcionTipoDocumento { get; set; } = string.Empty;
    public int TipoMoneda { get; set; }
    public string DescripcionMoneda { get; set; } = string.Empty;
    public decimal PorcentajeAnticipo { get; set; }
    public decimal MontoSolicitado { get; set; }
    public decimal MontoAprobado { get; set; }
    public decimal MontoDisponible { get; set; }
    public DateTime? FechaSolicitud { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int EstadoLinea { get; set; }
    public string DescripcionEstado { get; set; } = string.Empty;
    public bool Verificado { get; set; }
}

/// <summary>
/// DTO para crear sublínea por deudor
/// </summary>
public class CreateSublineaDeudorDto
{
    [Required]
    public int RutCliente { get; set; }
    
    [Required]
    public int NumeroLinea { get; set; }
    
    [Required]
    public int RutDeudor { get; set; }
    
    [Required]
    public int TipoDocumento { get; set; }
    
    [Required]
    public int TipoMoneda { get; set; } = 1;
    
    [Range(0.01, 100)]
    public decimal PorcentajeAnticipo { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal MontoSolicitado { get; set; }
    
    [Required]
    public DateTime? FechaSolicitud { get; set; } = DateTime.Today;
    
    public int EstadoLinea { get; set; } = 1;
    
    public bool Verificado { get; set; }
}

#endregion

#region Parámetros y Eventos DTOs

/// <summary>
/// DTO para parámetros de línea (comisiones)
/// </summary>
public class LineaCreditoParametroDto
{
    public int NumeroLinea { get; set; }
    public int RutCliente { get; set; }
    public int TipoParametro { get; set; }
    public string DescripcionTipo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

/// <summary>
/// DTO para eventos de línea
/// </summary>
public class LineaCreditoEventoDto
{
    public int NumeroLinea { get; set; }
    public int RutCliente { get; set; }
    public int TipoEvento { get; set; }
    public string DescripcionTipoEvento { get; set; } = string.Empty;
    public DateTime? FechaEvento { get; set; }
    public int EstadoEvento { get; set; }
    public decimal PorcentajeComision { get; set; }
    public decimal MontoComision { get; set; }
}

/// <summary>
/// DTO para registrar evento
/// </summary>
public class CreateLineaCreditoEventoDto
{
    [Required]
    public int NumeroLinea { get; set; }
    
    [Required]
    public int RutCliente { get; set; }
    
    [Required]
    public int TipoEvento { get; set; }
    
    [Range(0, 100)]
    public decimal PorcentajeComision { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal MontoComision { get; set; }
}

#endregion

#region Resumen y Consultas DTOs

/// <summary>
/// DTO para resumen de endeudamiento
/// </summary>
public class ResumenEndeudamientoDto
{
    public int RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public decimal TotalSolicitado { get; set; }
    public decimal TotalAprobado { get; set; }
    public decimal TotalDisponible { get; set; }
    public decimal TotalUtilizado { get; set; }
    public decimal PorcentajeUtilizado { get; set; }
    public int CantidadLineas { get; set; }
    public int LineasVigentes { get; set; }
    public int LineasVencidas { get; set; }
    public string? CategoriaRiesgo { get; set; }
}

/// <summary>
/// DTO para consulta de líneas
/// </summary>
public class ConsultaLineaCreditoDto
{
    public int NumeroLinea { get; set; }
    public int RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime? FechaSolicitud { get; set; }
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public string EstadoSolicitud { get; set; } = string.Empty;
    public decimal Spread { get; set; }
    public decimal MontoSolicitado { get; set; }
    public decimal MontoAprobado { get; set; }
    public string EstadoLinea { get; set; } = string.Empty;
    public string TipoMoneda { get; set; } = string.Empty;
    public string? CategoriaRiesgo { get; set; }
    public bool EstaVencida { get; set; }
    public int? DiasParaVencer { get; set; }
}

/// <summary>
/// DTO para notaría
/// </summary>
public class NotariaDto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

/// <summary>
/// DTO para resumen de sublíneas
/// </summary>
public class ResumenSublineasDto
{
    public int RutCliente { get; set; }
    public int NumeroLinea { get; set; }
    
    // Por Producto
    public int CantidadSublineasProducto { get; set; }
    public decimal TotalMontoProducto { get; set; }
    public decimal TotalDisponibleProducto { get; set; }
    
    // Por Deudor
    public int CantidadSublineasDeudor { get; set; }
    public decimal TotalMontoDeudor { get; set; }
    public decimal TotalDisponibleDeudor { get; set; }
    
    // Máximo crédito por deudor
    public decimal MaximoCreditoDeudor { get; set; }
}

#endregion
