using System;
using System.Collections.Generic;

namespace Factoring.Core.Entities;

/// <summary>
/// Línea de Crédito del Cliente
/// Tabla: LCR (migrado de F_LC1100.frm, F_LC1400.frm)
/// </summary>
public class LineaCredito
{
    public int LcrRutCli { get; set; }              // RUT Cliente
    public int LcrNumLin { get; set; }              // Número Línea (correlativo)
    public int LcrTipMon { get; set; }              // Tipo Moneda (1=Peso, 2=Dólar, 3=UF)
    public DateTime? LcrFecSol { get; set; }         // Fecha Solicitud
    public DateTime? LcrFecRes { get; set; }        // Fecha Resolución
    public DateTime? LcrFviDes { get; set; }        // Fecha Vigencia Desde
    public DateTime? LcrFviHas { get; set; }        // Fecha Vigencia Hasta
    public int LcrEstSol { get; set; }              // Estado Solicitud (1=Creada, 2=Aprobada, 3=Rechazada)
    public decimal LcrTasNeg { get; set; }          // Tasa Negocio
    public decimal LcrSprEad { get; set; }          // Spread
    public decimal LcrMonSol { get; set; }          // Monto Solicitado
    public decimal LcrMonApr { get; set; }          // Monto Aprobado
    public decimal LcrMonDis { get; set; }          // Monto Disponible
    public int LcrEstLin { get; set; }              // Estado Línea (1=Creada, 2=Vigente, 3=Aprobada, 4=Vencida)
    public int? LcrNumCon { get; set; }             // Número Contrato
    public int? LcrTipCon { get; set; }             // Tipo Contrato
    public DateTime? LcrFecCon { get; set; }        // Fecha Contrato
    public DateTime? LcrVtoCon { get; set; }        // Vencimiento Contrato
    public int? LcrEstCon { get; set; }             // Estado Contrato
    public DateTime? LcrFesCon { get; set; }        // Fecha Estado Contrato
    public string? LcrObsCon { get; set; }          // Observaciones Contrato
    public int? LcrCodNot { get; set; }             // Código Notaría
    public string? LcrUsrMod { get; set; }          // Usuario Modificación
    public DateTime? LcrFecMod { get; set; }        // Fecha Modificación
    public string? LcrHorMod { get; set; }          // Hora Modificación
    public decimal? LcrTasMor { get; set; }         // Tasa Mora
    public int? LcrDesMor { get; set; }             // Días Mora
    public int? LcrCalMor { get; set; }             // Cálculo Mora (0/1)

    // Navegación
    public string? NombreCliente { get; set; }
    public string? DescripcionEstadoLinea { get; set; }
    public string? DescripcionEstadoSolicitud { get; set; }
    public string? DescripcionMoneda { get; set; }
    public string? NombreNotaria { get; set; }
}

/// <summary>
/// Sublínea por Producto (Anticipos)
/// Tabla: ANT (migrado de F_LC1100.frm)
/// </summary>
public class SublineaProducto
{
    public int AntRutCli { get; set; }              // RUT Cliente
    public int AntNumLin { get; set; }              // Número Línea
    public int AntTipDoc { get; set; }              // Tipo Documento (FAC, CHE, LET)
    public int AntTipMon { get; set; }              // Tipo Moneda
    public decimal AntPorAnt { get; set; }          // Porcentaje Anticipo
    public decimal AntMonSol { get; set; }          // Monto Solicitado
    public decimal AntMonApr { get; set; }          // Monto Aprobado
    public decimal AntMonDis { get; set; }          // Monto Disponible
    public int? AntNumCon { get; set; }             // Número Contrato
    public DateTime? AntFecCon { get; set; }        // Fecha Contrato
    public DateTime? AntVtoCon { get; set; }        // Vencimiento Contrato
    public int? AntEstCon { get; set; }             // Estado Contrato
    public DateTime? AntFesCon { get; set; }        // Fecha Estado Contrato
    public int? AntPlaVto { get; set; }             // Plazo Vencimiento
    public int? AntPlaCes { get; set; }             // Plazo Cesión
    public decimal? AntComMin { get; set; }         // Comisión Mínima
    public decimal? AntComMax { get; set; }         // Comisión Máxima
    public decimal? AntComPor { get; set; }         // Comisión Porcentual

    // Navegación
    public string? DescripcionTipoDocumento { get; set; }
    public string? DescripcionMoneda { get; set; }
    public string? DescripcionEstadoContrato { get; set; }
}

/// <summary>
/// Sublínea por Deudor
/// Tabla: SUB (migrado de F_LC1100.frm)
/// </summary>
public class SublineaDeudor
{
    public int SubRutCli { get; set; }              // RUT Cliente
    public int SubNumLin { get; set; }              // Número Línea
    public int SubRutDeu { get; set; }              // RUT Deudor
    public int SubTipDoc { get; set; }              // Tipo Documento
    public int SubTipMon { get; set; }              // Tipo Moneda
    public decimal SubPorAnt { get; set; }          // Porcentaje Anticipo
    public decimal SubMonSol { get; set; }          // Monto Solicitado
    public decimal SubMonApr { get; set; }          // Monto Aprobado
    public decimal SubMonDis { get; set; }          // Monto Disponible
    public DateTime? SubFecSol { get; set; }         // Fecha Solicitud
    public DateTime? SubFecRes { get; set; }        // Fecha Resolución
    public DateTime? SubFecVto { get; set; }        // Fecha Vencimiento
    public int SubEstLin { get; set; }              // Estado Sublínea
    public bool SubVerif { get; set; }              // Verificado

    // Navegación
    public string? NombreDeudor { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public string? DescripcionEstado { get; set; }
}

/// <summary>
/// Parámetros de Línea de Crédito (Comisiones)
/// Tabla: LCR_PAR (migrado de F_LC1400.frm)
/// </summary>
public class LineaCreditoParametro
{
    public int LcpNroLcr { get; set; }              // Número Línea
    public int LcpRutCli { get; set; }              // RUT Cliente
    public int LcpTipPar { get; set; }              // Tipo Parámetro (1=Apertura, 2=Renovación, 3=Exceso)
    public decimal LcpValPar { get; set; }          // Valor Parámetro (porcentaje)

    // Navegación
    public string? DescripcionTipo { get; set; }
}

/// <summary>
/// Eventos de Línea de Crédito
/// Tabla: LCR_EVE (migrado de F_LC1400.frm)
/// </summary>
public class LineaCreditoEvento
{
    public int LceNroLcr { get; set; }              // Número Línea
    public int LceRutCli { get; set; }              // RUT Cliente
    public int LceTipEve { get; set; }              // Tipo Evento (1=Apertura, 2=Renovación, 3=Exceso)
    public DateTime? LceFecEve { get; set; }         // Fecha Evento
    public int LceEstEve { get; set; }              // Estado Evento
    public decimal LcePorEve { get; set; }          // Porcentaje Comisión
    public decimal LceMonEve { get; set; }          // Monto Comisión

    // Navegación
    public string? DescripcionTipoEvento { get; set; }
}

/// <summary>
/// Notaría para contratos
/// Tabla: NTA
/// </summary>
public class Notaria
{
    public int NtaCodNot { get; set; }              // Código Notaría
    public string NtaNomNot { get; set; } = string.Empty;  // Nombre Notaría
    public string? NtaDesNot { get; set; }          // Descripción/Dirección
}

/// <summary>
/// Resumen de endeudamiento del cliente
/// Vista calculada
/// </summary>
public class ResumenEndeudamiento
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
}

/// <summary>
/// Consulta de líneas de crédito con filtros
/// </summary>
public class ConsultaLineaCredito
{
    public int NumeroLinea { get; set; }
    public int RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime? FechaSolicitud { get; set; }
    public DateTime? FechaVigenciaDesde { get; set; }
    public DateTime? FechaVigenciaHasta { get; set; }
    public string EstadoSolicitud { get; set; } = string.Empty;
    public decimal Spread { get; set; }
    public decimal MontoSolicitado { get; set; }
    public decimal MontoAprobado { get; set; }
    public string EstadoLinea { get; set; } = string.Empty;
    public string TipoMoneda { get; set; } = string.Empty;
    public string? CategoriaRiesgo { get; set; }
}

#region Enums

/// <summary>
/// Estado de la solicitud de línea
/// </summary>
public enum EstadoSolicitudLineaEnum
{
    Creada = 1,
    Aprobada = 2,
    Rechazada = 3
}

/// <summary>
/// Estado de la línea de crédito
/// </summary>
public enum EstadoLineaCreditoEnum
{
    Creada = 1,
    Vigente = 2,
    Aprobada = 3,
    Vencida = 4,
    Suspendida = 5
}

/// <summary>
/// Tipo de evento de línea
/// </summary>
public enum TipoEventoLineaEnum
{
    Apertura = 1,
    Renovacion = 2,
    Exceso = 3
}

/// <summary>
/// Tipo de moneda
/// </summary>
public enum TipoMonedaEnum
{
    Peso = 1,
    Dolar = 2,
    UF = 3
}

/// <summary>
/// Estado del contrato
/// </summary>
public enum EstadoContratoEnum
{
    Vigente = 1,
    Vencido = 2,
    Renovado = 3,
    Anulado = 4
}

#endregion
