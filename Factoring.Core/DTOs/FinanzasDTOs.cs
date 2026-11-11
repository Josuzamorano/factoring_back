using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Factoring.Core.DTOs;

#region DTOs de Tasa Plazo (TYP)

/// <summary>
/// DTO para Tasa y Plazo
/// </summary>
public class TasaPlazoDto
{
    public DateTime? FechaVigencia { get; set; }
    public int TipoTasa { get; set; }
    public string? DescripcionTipoTasa { get; set; }
    public int TipoMoneda { get; set; }
    public string? DescripcionMoneda { get; set; }
    public int DiasTramo { get; set; }
    public decimal TasaBase { get; set; }
    public decimal TasaMora { get; set; }
    public decimal TasaMaximaConvencional { get; set; }
    public decimal TasaAuxiliar { get; set; }
}

/// <summary>
/// DTO para crear Tasa Plazo
/// </summary>
public class CreateTasaPlazoDto
{
    [Required(ErrorMessage = "La fecha de vigencia es requerida")]
    public DateTime? FechaVigencia { get; set; }
    
    [Required(ErrorMessage = "El tipo de tasa es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El tipo de tasa debe ser mayor a 0")]
    public int TipoTasa { get; set; }
    
    [Required(ErrorMessage = "El tipo de moneda es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El tipo de moneda debe ser mayor a 0")]
    public int TipoMoneda { get; set; }
    
    [Required(ErrorMessage = "Los días del tramo son requeridos")]
    [Range(1, 360, ErrorMessage = "Los días deben estar entre 1 y 360")]
    public int DiasTramo { get; set; }
    
    [Range(0, 100, ErrorMessage = "La tasa base debe estar entre 0 y 100")]
    public decimal TasaBase { get; set; }
    
    [Range(0, 100, ErrorMessage = "La tasa mora debe estar entre 0 y 100")]
    public decimal TasaMora { get; set; }
    
    [Range(0, 100, ErrorMessage = "La tasa máxima convencional debe estar entre 0 y 100")]
    public decimal TasaMaximaConvencional { get; set; }
    
    [Range(0, 100, ErrorMessage = "La tasa auxiliar debe estar entre 0 y 100")]
    public decimal TasaAuxiliar { get; set; }
}

/// <summary>
/// DTO para actualizar Tasa Plazo
/// </summary>
public class UpdateTasaPlazoDto
{
    [Range(0, 100, ErrorMessage = "La tasa base debe estar entre 0 y 100")]
    public decimal TasaBase { get; set; }
    
    [Range(0, 100, ErrorMessage = "La tasa mora debe estar entre 0 y 100")]
    public decimal TasaMora { get; set; }
    
    [Range(0, 100, ErrorMessage = "La tasa máxima convencional debe estar entre 0 y 100")]
    public decimal TasaMaximaConvencional { get; set; }
    
    [Range(0, 100, ErrorMessage = "La tasa auxiliar debe estar entre 0 y 100")]
    public decimal TasaAuxiliar { get; set; }
}

/// <summary>
/// DTO para filtrar Tasas Plazo
/// </summary>
public class FiltroTasaPlazoDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? TipoMoneda { get; set; }
    public int? TipoTasa { get; set; }
    public int? DiasDesde { get; set; }
    public int? DiasHasta { get; set; }
}

#endregion

#region DTOs de Tasa Base

/// <summary>
/// DTO para Tasa Base
/// </summary>
public class TasaBaseDto
{
    public int Id { get; set; }
    public string TipoPeriodo { get; set; } = string.Empty;
    public string? DescripcionPeriodo { get; set; }
    public DateTime? FechaVigencia { get; set; }
    public int CodigoMoneda { get; set; }
    public string? DescripcionMoneda { get; set; }
    public decimal ValorTasa { get; set; }
    public decimal TasaMora { get; set; }
    public decimal TasaMaximaLegal { get; set; }
    public decimal TasaAuxiliar { get; set; }
}

/// <summary>
/// DTO para crear Tasa Base
/// </summary>
public class CreateTasaBaseDto
{
    [Required(ErrorMessage = "El tipo de período es requerido")]
    public string TipoPeriodo { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La fecha de vigencia es requerida")]
    public DateTime? FechaVigencia { get; set; }
    
    [Required(ErrorMessage = "El código de moneda es requerido")]
    public int CodigoMoneda { get; set; }
    
    [Range(0, 100, ErrorMessage = "El valor de tasa debe estar entre 0 y 100")]
    public decimal ValorTasa { get; set; }
    
    [Range(0, 100, ErrorMessage = "La tasa mora debe estar entre 0 y 100")]
    public decimal TasaMora { get; set; }
    
    [Range(0, 100, ErrorMessage = "La tasa máxima legal debe estar entre 0 y 100")]
    public decimal TasaMaximaLegal { get; set; }
}

/// <summary>
/// DTO para filtrar Tasas Base
/// </summary>
public class FiltroTasaBaseDto
{
    public DateTime? Fecha { get; set; }
    public int? CodigoMoneda { get; set; }
    public string? TipoPeriodo { get; set; }
}

#endregion

#region DTOs de Simulación

/// <summary>
/// DTO para solicitar simulación de tasa
/// </summary>
public class SimulacionTasaRequestDto
{
    [Required(ErrorMessage = "La fecha es requerida")]
    public DateTime? Fecha { get; set; }
    
    [Required(ErrorMessage = "El tipo de moneda es requerido")]
    public int TipoMoneda { get; set; }
    
    [Required(ErrorMessage = "Los días de operación son requeridos")]
    [Range(1, 360, ErrorMessage = "Los días deben estar entre 1 y 360")]
    public int DiasOperacion { get; set; }
    
    [Required(ErrorMessage = "El monto de operación es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal MontoOperacion { get; set; }
    
    /// <summary>
    /// Spread adicional del cliente (opcional)
    /// </summary>
    public decimal? Spread { get; set; }
}

/// <summary>
/// DTO de respuesta de simulación de tasa
/// </summary>
public class SimulacionTasaResponseDto
{
    public DateTime? Fecha { get; set; }
    public int TipoMoneda { get; set; }
    public string? DescripcionMoneda { get; set; }
    public int DiasOperacion { get; set; }
    public decimal MontoOperacion { get; set; }
    
    // Tasas
    public decimal TasaBase { get; set; }
    public decimal TasaMora { get; set; }
    public decimal TasaMaximaConvencional { get; set; }
    public decimal Spread { get; set; }
    public decimal TasaFinal { get; set; }
    
    // Cálculos
    public decimal InteresNeto { get; set; }
    public decimal InteresMora { get; set; }
    public decimal TotalDescuento { get; set; }
    public decimal MontoLiquido { get; set; }
    
    // Información adicional
    public string? TramoAplicado { get; set; }
    public string? Observaciones { get; set; }
}

#endregion

#region DTOs de Histórico y Resumen

/// <summary>
/// DTO para histórico de tasas
/// </summary>
public class HistoricoTasaDto
{
    public DateTime? Fecha { get; set; }
    public int TipoMoneda { get; set; }
    public string? DescripcionMoneda { get; set; }
    public int DiasTramo { get; set; }
    public decimal TasaBase { get; set; }
    public decimal TasaMora { get; set; }
    public decimal TasaMaximaConvencional { get; set; }
}

/// <summary>
/// DTO para filtrar histórico de tasas
/// </summary>
public class FiltroHistoricoTasaDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? TipoMoneda { get; set; }
    public int? DiasTramo { get; set; }
}

/// <summary>
/// DTO para resumen de tasas
/// </summary>
public class ResumenTasasDto
{
    public DateTime? Fecha { get; set; }
    public int TipoMoneda { get; set; }
    public string? DescripcionMoneda { get; set; }
    public int CantidadTramos { get; set; }
    public decimal TasaBasePromedio { get; set; }
    public decimal TasaBaseMinima { get; set; }
    public decimal TasaBaseMaxima { get; set; }
    public decimal TasaMoraPromedio { get; set; }
}

#endregion

#region DTOs de Spread Cliente

/// <summary>
/// DTO para Spread de Cliente
/// </summary>
public class SpreadClienteDto
{
    public int RutCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int TipoMoneda { get; set; }
    public string? DescripcionMoneda { get; set; }
    public decimal SpreadBase { get; set; }
    public decimal SpreadAdicional { get; set; }
    public decimal SpreadTotal { get; set; }
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public bool Activo { get; set; }
}

/// <summary>
/// DTO para crear Spread de Cliente
/// </summary>
public class CreateSpreadClienteDto
{
    [Required(ErrorMessage = "El RUT del cliente es requerido")]
    public int RutCliente { get; set; }
    
    [Required(ErrorMessage = "El tipo de moneda es requerido")]
    public int TipoMoneda { get; set; }
    
    [Range(0, 100, ErrorMessage = "El spread base debe estar entre 0 y 100")]
    public decimal SpreadBase { get; set; }
    
    [Range(0, 100, ErrorMessage = "El spread adicional debe estar entre 0 y 100")]
    public decimal SpreadAdicional { get; set; }
    
    [Required(ErrorMessage = "La fecha de vigencia es requerida")]
    public DateTime? VigenciaDesde { get; set; }
    
    public DateTime? VigenciaHasta { get; set; }
}

#endregion

#region DTOs de Indicador Económico

/// <summary>
/// DTO para Indicador Económico
/// </summary>
public class IndicadorEconomicoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public DateTime? Fecha { get; set; }
    public decimal Valor { get; set; }
    public decimal? VariacionDiaria { get; set; }
    public decimal? VariacionMensual { get; set; }
    public decimal? VariacionAnual { get; set; }
}

/// <summary>
/// DTO para filtrar Indicadores Económicos
/// </summary>
public class FiltroIndicadorDto
{
    public string? Codigo { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}

#endregion

#region DTOs de Copia de Tasas

/// <summary>
/// DTO para copiar tasas de una fecha a otra
/// </summary>
public class CopiarTasasDto
{
    [Required(ErrorMessage = "La fecha origen es requerida")]
    public DateTime? FechaOrigen { get; set; }
    
    [Required(ErrorMessage = "La fecha destino es requerida")]
    public DateTime? FechaDestino { get; set; }
    
    [Required(ErrorMessage = "El tipo de moneda es requerido")]
    public int TipoMoneda { get; set; }
    
    /// <summary>
    /// Ajuste porcentual a aplicar (ej: 0.5 para aumentar 0.5%)
    /// </summary>
    public decimal? AjustePorcentual { get; set; }
}

/// <summary>
/// Resultado de copia de tasas
/// </summary>
public class ResultadoCopiaDto
{
    public bool Exitoso { get; set; }
    public int TasasCopiadas { get; set; }
    public string? Mensaje { get; set; }
    public List<string>? Errores { get; set; }
}

#endregion
