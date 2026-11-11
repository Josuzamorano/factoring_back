using System;
using System.Collections.Generic;

namespace Factoring.Core.Entities;

#region Enums

/// <summary>
/// Tipo de moneda para tasas
/// </summary>
public enum TipoMonedaTasaEnum
{
    Peso = 1,
    Dolar = 2,
    UF = 3,
    Euro = 4
}

/// <summary>
/// Tipo de período para tasas
/// </summary>
public enum TipoPeriodoTasaEnum
{
    Diario = 1,
    Semanal = 7,
    Quincenal = 15,
    Mensual = 30,
    Trimestral = 90,
    Semestral = 180,
    Anual = 360
}

/// <summary>
/// Tipo de rango de montos
/// </summary>
public enum TipoRangoMontoEnum
{
    Hasta5Millones = 1,
    De5a20Millones = 2,
    De20a50Millones = 3,
    De50a100Millones = 4,
    MasDe100Millones = 5
}

#endregion

#region Entidades Principales

/// <summary>
/// Tasa y plazo por tramo - Tabla TYP
/// Migrado de F_FI1100.frm - Tasas y Plazos por Tramos
/// </summary>
public class TasaPlazo
{
    // Clave compuesta: TYP_FEC_TAS + TYP_TIP_TAS + TYP_TIP_MON + TYP_DIA_TAS
    
    /// <summary>
    /// Fecha de vigencia de la tasa (TYP_FEC_TAS)
    /// </summary>
    public DateTime? FechaVigencia { get; set; }
    
    /// <summary>
    /// Tipo de tasa/rango de monto (TYP_TIP_TAS) - FK a PAR TAB_MONTORANGO
    /// </summary>
    public int TipoTasa { get; set; }
    
    /// <summary>
    /// Descripción del tipo de tasa/rango
    /// </summary>
    public string? DescripcionTipoTasa { get; set; }
    
    /// <summary>
    /// Tipo de moneda (TYP_TIP_MON) - FK a PAR TAB_MONEDA
    /// </summary>
    public int TipoMoneda { get; set; }
    
    /// <summary>
    /// Descripción del tipo de moneda
    /// </summary>
    public string? DescripcionMoneda { get; set; }
    
    /// <summary>
    /// Cantidad de días del tramo (TYP_DIA_TAS)
    /// </summary>
    public int DiasTramo { get; set; }
    
    /// <summary>
    /// Tasa base - Costo de fondo (TYP_VTA_NEG)
    /// </summary>
    public decimal TasaBase { get; set; }
    
    /// <summary>
    /// Tasa de mora (TYP_VAL_TMO)
    /// </summary>
    public decimal TasaMora { get; set; }
    
    /// <summary>
    /// Tasa máxima convencional (TYP_MAX_CON)
    /// </summary>
    public decimal TasaMaximaConvencional { get; set; }
    
    /// <summary>
    /// Tasa auxiliar (TYP_TAS_AUX)
    /// </summary>
    public decimal TasaAuxiliar { get; set; }
    
    // Campos de auditoría
    public DateTime? FechaCreacion { get; set; }
    public string? UsuarioCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
}

/// <summary>
/// Tasa base externa - Tabla TASASBASES (conexión ABN)
/// Migrado de F_FI1200.frm
/// </summary>
public class TasaBase
{
    /// <summary>
    /// ID único
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Tipo de período/tasa (TipoDeTasa)
    /// </summary>
    public string TipoPeriodo { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción del período
    /// </summary>
    public string? DescripcionPeriodo { get; set; }
    
    /// <summary>
    /// Fecha de vigencia (FechaValor)
    /// </summary>
    public DateTime? FechaVigencia { get; set; }
    
    /// <summary>
    /// Código de moneda (MonedaSCORE)
    /// </summary>
    public int CodigoMoneda { get; set; }
    
    /// <summary>
    /// Descripción de la moneda
    /// </summary>
    public string? DescripcionMoneda { get; set; }
    
    /// <summary>
    /// Valor de la tasa base (ValorTasa)
    /// </summary>
    public decimal ValorTasa { get; set; }
    
    /// <summary>
    /// Tasa de mora (TasaMor)
    /// </summary>
    public decimal TasaMora { get; set; }
    
    /// <summary>
    /// Tasa máxima legal (TasaMaxLegal)
    /// </summary>
    public decimal TasaMaximaLegal { get; set; }
    
    /// <summary>
    /// Fecha de actualización
    /// </summary>
    public DateTime? FechaActualizacion { get; set; }
    
    /// <summary>
    /// Tasa auxiliar calculada (ValorTasa / 12)
    /// </summary>
    public decimal TasaAuxiliar => ValorTasa / 12;
}

/// <summary>
/// Simulación de tasa para operación
/// </summary>
public class SimulacionTasa
{
    public DateTime? Fecha { get; set; }
    public int TipoMoneda { get; set; }
    public string? DescripcionMoneda { get; set; }
    public int DiasOperacion { get; set; }
    public decimal MontoOperacion { get; set; }
    
    // Tasas aplicables
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
}

/// <summary>
/// Histórico de tasas para gráficos y análisis
/// </summary>
public class HistoricoTasa
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
/// Resumen de tasas por moneda y fecha
/// </summary>
public class ResumenTasas
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

/// <summary>
/// Configuración de spread por cliente/tipo
/// </summary>
public class SpreadCliente
{
    public int RutCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int TipoMoneda { get; set; }
    public string? DescripcionMoneda { get; set; }
    public decimal SpreadBase { get; set; }
    public decimal SpreadAdicional { get; set; }
    public decimal SpreadTotal => SpreadBase + SpreadAdicional;
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public bool Activo { get; set; }
}

/// <summary>
/// Indicador económico (UF, Dólar, etc.)
/// </summary>
public class IndicadorEconomico
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

#endregion
