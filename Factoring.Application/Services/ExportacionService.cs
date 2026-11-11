// =============================================
// Servicio de Exportacion - Excel y reportes
// =============================================

using Dapper;
using Factoring.Core.Interfaces;

namespace Factoring.Application.Services;

/// <summary>
/// Datos para exportacion de cartera
/// </summary>
public class CarteraExportDto
{
    public int NumeroOperacion { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public DateTime FechaOperacion { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public int DiasVencido { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal MontoAnticipado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Ejecutivo { get; set; } = string.Empty;
}

/// <summary>
/// Datos para exportacion de operaciones
/// </summary>
public class OperacionExportDto
{
    public int NumeroOperacion { get; set; }
    public DateTime FechaOperacion { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public int NumeroLinea { get; set; }
    public int TipoOperacion { get; set; }
    public string TipoOperacionNombre { get; set; } = string.Empty;
    public decimal MontoDocumentos { get; set; }
    public decimal MontoAnticipo { get; set; }
    public decimal MontoIntereses { get; set; }
    public decimal MontoComision { get; set; }
    public decimal MontoGastos { get; set; }
    public decimal MontoIVA { get; set; }
    public decimal MontoGiro { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Ejecutivo { get; set; } = string.Empty;
}

/// <summary>
/// Datos para exportacion de pagos
/// </summary>
public class PagoExportDto
{
    public int NumeroPago { get; set; }
    public DateTime FechaPago { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public int NumeroOperacion { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public decimal MontoPago { get; set; }
    public decimal SaldoAnterior { get; set; }
    public decimal SaldoNuevo { get; set; }
    public string TipoPago { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

/// <summary>
/// Datos para exportacion de lineas de credito
/// </summary>
public class LineaCreditoExportDto
{
    public int NumeroLinea { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime FechaAprobacion { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public int DiasParaVencer { get; set; }
    public decimal MontoLinea { get; set; }
    public decimal CupoUtilizado { get; set; }
    public decimal CupoDisponible { get; set; }
    public decimal PorcentajeUtilizacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Ejecutivo { get; set; } = string.Empty;
}

/// <summary>
/// Datos para dashboard
/// </summary>
public class DashboardMetricas
{
    // Cartera
    public decimal CarteraTotal { get; set; }
    public decimal CarteraVigente { get; set; }
    public decimal CarteraMorosa { get; set; }
    public decimal PorcentajeMorosidad { get; set; }
    public int TotalDocumentos { get; set; }
    public int DocumentosVigentes { get; set; }
    public int DocumentosMorosos { get; set; }

    // Operaciones
    public int OperacionesMes { get; set; }
    public decimal MontoOperadoMes { get; set; }
    public decimal InteresesMes { get; set; }
    public decimal ComisionesMes { get; set; }

    // Lineas
    public int LineasActivas { get; set; }
    public decimal CupoTotalAprobado { get; set; }
    public decimal CupoTotalUtilizado { get; set; }
    public decimal CupoTotalDisponible { get; set; }

    // Cobranza
    public decimal RecaudacionMes { get; set; }
    public int PagosMes { get; set; }
    public decimal ExcedentesPendientes { get; set; }

    // Vencimientos proximos
    public List<DocumentoProximoVencer> DocumentosProximosVencer { get; set; } = new();
    public List<LineaProximaVencer> LineasProximasVencer { get; set; } = new();
}

public class DocumentoProximoVencer
{
    public int NumeroOperacion { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public string NombreDeudor { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
    public int DiasParaVencer { get; set; }
    public decimal Monto { get; set; }
}

public class LineaProximaVencer
{
    public int NumeroLinea { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
    public int DiasParaVencer { get; set; }
    public decimal CupoUtilizado { get; set; }
}

/// <summary>
/// Datos de concentracion por deudor
/// </summary>
public class ConcentracionDeudorDto
{
    public long RutDeudor { get; set; }
    public string DvDeudor { get; set; } = string.Empty;
    public string NombreDeudor { get; set; } = string.Empty;
    public int CantidadDocumentos { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public decimal PorcentajeCartera { get; set; }
    public decimal PorcentajeAcumulado { get; set; }
    public int CantidadClientes { get; set; }
    public string Clasificacion { get; set; } = string.Empty; // A, B, C
    public decimal MontoMoroso { get; set; }
    public decimal PorcentajeMorosidad { get; set; }
}

/// <summary>
/// Datos de concentracion por cliente
/// </summary>
public class ConcentracionClienteDto
{
    public long RutCliente { get; set; }
    public string DvCliente { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public int CantidadDocumentos { get; set; }
    public int CantidadDeudores { get; set; }
    public decimal MontoCartera { get; set; }
    public decimal SaldoPendiente { get; set; }
    public decimal PorcentajeCartera { get; set; }
    public decimal PorcentajeAcumulado { get; set; }
    public decimal MontoLineaCredito { get; set; }
    public decimal CupoUtilizado { get; set; }
    public decimal PorcentajeUtilizacion { get; set; }
    public string Clasificacion { get; set; } = string.Empty;
    public decimal MontoMoroso { get; set; }
    public decimal PorcentajeMorosidad { get; set; }
}

/// <summary>
/// Resumen de concentracion
/// </summary>
public class ResumenConcentracion
{
    public decimal CarteraTotal { get; set; }
    public int TotalDeudores { get; set; }
    public int TotalClientes { get; set; }

    // Top concentracion
    public decimal Top5Porcentaje { get; set; }
    public decimal Top10Porcentaje { get; set; }
    public decimal Top20Porcentaje { get; set; }

    // Indice Herfindahl-Hirschman (HHI)
    public decimal IndiceHHI { get; set; }
    public string NivelConcentracion { get; set; } = string.Empty; // Bajo, Moderado, Alto

    // Por clasificacion
    public int DeudoresClaseA { get; set; }
    public decimal MontoClaseA { get; set; }
    public int DeudoresClaseB { get; set; }
    public decimal MontoClaseB { get; set; }
    public int DeudoresClaseC { get; set; }
    public decimal MontoClaseC { get; set; }
}

/// <summary>
/// Datos de rentabilidad por cliente
/// </summary>
public class RentabilidadClienteDto
{
    public long RutCliente { get; set; }
    public string DvCliente { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;

    // Operaciones
    public int CantidadOperaciones { get; set; }
    public decimal MontoOperado { get; set; }
    public decimal MontoAnticipado { get; set; }

    // Ingresos
    public decimal InteresesGanados { get; set; }
    public decimal ComisionesGanadas { get; set; }
    public decimal GastosRecuperados { get; set; }
    public decimal TotalIngresos { get; set; }

    // Metricas
    public decimal TasaPromedioMensual { get; set; }
    public decimal SpreadPromedio { get; set; }
    public int PlazoPonderadoPromedio { get; set; }
    public decimal TicketPromedio { get; set; }

    // Riesgo
    public decimal MontoMoroso { get; set; }
    public decimal PorcentajeMorosidad { get; set; }
    public decimal ProvisionEstimada { get; set; }

    // Rentabilidad neta
    public decimal IngresosNetos { get; set; }
    public decimal RentabilidadSobreCartera { get; set; }
    public decimal RentabilidadAnualizada { get; set; }

    // Ranking
    public int Ranking { get; set; }
    public string Clasificacion { get; set; } = string.Empty; // Premium, Estandar, Bajo
}

/// <summary>
/// Datos de evolucion de cartera mensual
/// </summary>
public class EvolucionCarteraMensualDto
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string NombreMes { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty; // "2024-01"

    // Cartera
    public decimal CarteraTotal { get; set; }
    public decimal CarteraVigente { get; set; }
    public decimal CarteraMorosa { get; set; }
    public decimal PorcentajeMorosidad { get; set; }
    public int TotalDocumentos { get; set; }
    public int DocumentosVigentes { get; set; }
    public int DocumentosMorosos { get; set; }

    // Operaciones
    public int OperacionesNuevas { get; set; }
    public decimal MontoOperado { get; set; }
    public decimal MontoAnticipado { get; set; }

    // Ingresos
    public decimal InteresesGanados { get; set; }
    public decimal ComisionesGanadas { get; set; }
    public decimal TotalIngresos { get; set; }

    // Recaudacion
    public decimal RecaudacionMes { get; set; }
    public int PagosRegistrados { get; set; }

    // Variaciones
    public decimal VariacionCartera { get; set; }
    public decimal VariacionCarteraPorcentaje { get; set; }
    public decimal VariacionMorosidad { get; set; }
}

/// <summary>
/// Resumen de evolucion de cartera
/// </summary>
public class ResumenEvolucionCartera
{
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public int TotalMeses { get; set; }

    // Cartera
    public decimal CarteraInicial { get; set; }
    public decimal CarteraFinal { get; set; }
    public decimal CrecimientoCartera { get; set; }
    public decimal CrecimientoPorcentaje { get; set; }

    // Promedios
    public decimal CarteraPromedio { get; set; }
    public decimal MorosidadPromedio { get; set; }
    public decimal OperacionesMesPromedio { get; set; }
    public decimal IngresoMensualPromedio { get; set; }

    // Maximos y Minimos
    public string MesMayorCartera { get; set; } = string.Empty;
    public decimal MayorCartera { get; set; }
    public string MesMenorMorosidad { get; set; } = string.Empty;
    public decimal MenorMorosidad { get; set; }
    public string MesMayorIngreso { get; set; } = string.Empty;
    public decimal MayorIngreso { get; set; }

    // Totales del periodo
    public decimal TotalOperado { get; set; }
    public decimal TotalIntereses { get; set; }
    public decimal TotalComisiones { get; set; }
    public decimal TotalRecaudado { get; set; }
}

/// <summary>
/// Datos comparativo año anterior
/// </summary>
public class ComparativoAnualDto
{
    public string Metrica { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;

    // Año actual
    public decimal ValorActual { get; set; }
    public int CantidadActual { get; set; }

    // Año anterior
    public decimal ValorAnterior { get; set; }
    public int CantidadAnterior { get; set; }

    // Variaciones
    public decimal VariacionAbsoluta { get; set; }
    public decimal VariacionPorcentaje { get; set; }
    public string Tendencia { get; set; } = string.Empty; // Subió, Bajó, Estable
}

/// <summary>
/// Detalle mensual comparativo
/// </summary>
public class ComparativoMensualDto
{
    public int Mes { get; set; }
    public string NombreMes { get; set; } = string.Empty;

    // Año actual
    public decimal CarteraActual { get; set; }
    public int OperacionesActual { get; set; }
    public decimal IngresosActual { get; set; }
    public decimal MorosidadActual { get; set; }

    // Año anterior
    public decimal CarteraAnterior { get; set; }
    public int OperacionesAnterior { get; set; }
    public decimal IngresosAnterior { get; set; }
    public decimal MorosidadAnterior { get; set; }

    // Variaciones
    public decimal VariacionCartera { get; set; }
    public decimal VariacionOperaciones { get; set; }
    public decimal VariacionIngresos { get; set; }
    public decimal VariacionMorosidad { get; set; }
}

/// <summary>
/// Resumen comparativo anual
/// </summary>
public class ResumenComparativoAnual
{
    public int AnioActual { get; set; }
    public int AnioAnterior { get; set; }

    // Cartera
    public decimal CarteraActual { get; set; }
    public decimal CarteraAnterior { get; set; }
    public decimal CrecimientoCartera { get; set; }
    public decimal CrecimientoCarteraPorcentaje { get; set; }

    // Operaciones
    public int OperacionesActual { get; set; }
    public int OperacionesAnterior { get; set; }
    public decimal CrecimientoOperaciones { get; set; }

    // Montos
    public decimal MontoOperadoActual { get; set; }
    public decimal MontoOperadoAnterior { get; set; }
    public decimal CrecimientoMonto { get; set; }

    // Ingresos
    public decimal IngresosActual { get; set; }
    public decimal IngresosAnterior { get; set; }
    public decimal CrecimientoIngresos { get; set; }
    public decimal CrecimientoIngresosPorcentaje { get; set; }

    // Morosidad
    public decimal MorosidadPromedioActual { get; set; }
    public decimal MorosidadPromedioAnterior { get; set; }
    public decimal VariacionMorosidad { get; set; }

    // Mejor/Peor mes
    public string MejorMesActual { get; set; } = string.Empty;
    public decimal MejorIngresoActual { get; set; }
    public string MejorMesAnterior { get; set; } = string.Empty;
    public decimal MejorIngresoAnterior { get; set; }
}

/// <summary>
/// Datos de proyección de flujo de caja diario
/// </summary>
public class FlujoCajaDiarioDto
{
    public DateTime Fecha { get; set; }
    public string DiaSemana { get; set; } = string.Empty;
    public bool EsHabil { get; set; } = true;

    // Entradas proyectadas
    public decimal CobranzaProyectada { get; set; }
    public int DocumentosVencen { get; set; }
    public decimal CobranzaAjustada { get; set; } // Aplicando tasa de recuperación

    // Salidas proyectadas
    public decimal GirosProyectados { get; set; }
    public int OperacionesProyectadas { get; set; }

    // Flujo neto
    public decimal FlujoNeto { get; set; }
    public decimal SaldoAcumulado { get; set; }
}

/// <summary>
/// Datos de proyección semanal
/// </summary>
public class FlujoCajaSemanalDto
{
    public int NumeroSemana { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Periodo { get; set; } = string.Empty;

    // Entradas
    public decimal CobranzaProyectada { get; set; }
    public int DocumentosVencen { get; set; }

    // Salidas
    public decimal GirosProyectados { get; set; }
    public int OperacionesProyectadas { get; set; }

    // Flujo
    public decimal FlujoNeto { get; set; }
    public decimal SaldoAcumulado { get; set; }
}

/// <summary>
/// Resumen de proyección de flujo de caja
/// </summary>
public class ResumenProyeccionFlujo
{
    public DateTime FechaProyeccion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int DiasProyectados { get; set; }

    // Saldo inicial
    public decimal SaldoInicial { get; set; }

    // Totales proyectados
    public decimal TotalCobranzaProyectada { get; set; }
    public decimal TotalCobranzaAjustada { get; set; }
    public int TotalDocumentosVencen { get; set; }
    public decimal TotalGirosProyectados { get; set; }
    public int TotalOperacionesProyectadas { get; set; }

    // Flujo neto
    public decimal FlujoNetoTotal { get; set; }
    public decimal SaldoFinalProyectado { get; set; }

    // Métricas
    public decimal TasaRecuperacionHistorica { get; set; }
    public decimal PromedioGiroDiario { get; set; }
    public decimal PromedioCobranzaDiaria { get; set; }

    // Alertas
    public int DiasConFlujoNegativo { get; set; }
    public decimal MinimoSaldoProyectado { get; set; }
    public DateTime? FechaMinimoSaldo { get; set; }

    // Por semana
    public decimal MejorSemanaPorFlujo { get; set; }
    public int NumeroMejorSemana { get; set; }
    public decimal PeorSemanaPorFlujo { get; set; }
    public int NumeroPeorSemana { get; set; }
}

/// <summary>
/// Resumen de rentabilidad
/// </summary>
public class ResumenRentabilidad
{
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }

    // Totales
    public decimal TotalIntereses { get; set; }
    public decimal TotalComisiones { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal TotalIngresos { get; set; }

    // Promedios
    public decimal TasaPromedioGeneral { get; set; }
    public int PlazoPromedioGeneral { get; set; }
    public decimal TicketPromedioGeneral { get; set; }

    // Cartera
    public decimal CarteraPromedio { get; set; }
    public decimal RentabilidadCartera { get; set; }

    // Por clasificacion
    public int ClientesPremium { get; set; }
    public decimal IngresosPremium { get; set; }
    public int ClientesEstandar { get; set; }
    public decimal IngresosEstandar { get; set; }
    public int ClientesBajo { get; set; }
    public decimal IngresosBajo { get; set; }
}

/// <summary>
/// Servicio de exportacion y dashboard
/// </summary>
public class ExportacionService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ExportacionService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Obtiene datos de cartera para exportar
    /// </summary>
    public async Task<IEnumerable<CarteraExportDto>> GetCarteraParaExportarAsync(
        long? rutCliente = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null,
        bool soloMorosos = false, bool soloVigentes = false)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT
                F.FAO_NUM_OPE as NumeroOperacion,
                'Factura' as TipoDocumento,
                F.FAO_NUM_FAC as NumeroDocumento,
                F.FAO_RUT_CLI as RutCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                F.FAO_RUT_DEU as RutDeudor,
                ISNULL(D.DEU_RAZ_SOC, 'Sin Deudor') as NombreDeudor,
                O.OPE_FEC_OPE as FechaOperacion,
                F.FAO_FEC_VTO as FechaVencimiento,
                DATEDIFF(DAY, F.FAO_FEC_VTO, GETDATE()) as DiasVencido,
                F.FAO_MON_FAC as MontoDocumento,
                ISNULL(F.FAO_MON_ANT, 0) as MontoAnticipado,
                ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) as SaldoPendiente,
                CASE F.FAO_EST_FAC
                    WHEN 1 THEN 'Vigente'
                    WHEN 2 THEN 'Prorrogado'
                    WHEN 3 THEN 'Renovado'
                    WHEN 4 THEN 'Cancelado'
                    WHEN 5 THEN 'Moroso'
                    WHEN 6 THEN 'Protestado'
                    WHEN 7 THEN 'Anulado'
                    ELSE 'Otro'
                END as Estado,
                ISNULL(U.USR_NOM_USR, 'Sistema') as Ejecutivo
            FROM FAO F
            INNER JOIN CLI C ON F.FAO_RUT_CLI = C.CLI_RUT_CLI
            INNER JOIN OPE O ON F.FAO_NUM_OPE = O.OPE_NUM_OPE
            LEFT JOIN DEU D ON F.FAO_RUT_DEU = D.DEU_RUT_DEU AND F.FAO_RUT_CLI = D.DEU_RUT_CLI
            LEFT JOIN USR U ON O.OPE_USR_CRE = U.USR_COD_USR
            WHERE F.FAO_EST_FAC NOT IN (7) -- Excluir anulados
              AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0";

        var parameters = new DynamicParameters();

        if (rutCliente.HasValue)
        {
            sql += " AND F.FAO_RUT_CLI = @RutCliente";
            parameters.Add("RutCliente", rutCliente.Value);
        }

        if (fechaDesde.HasValue)
        {
            sql += " AND F.FAO_FEC_VTO >= @FechaDesde";
            parameters.Add("FechaDesde", fechaDesde.Value);
        }

        if (fechaHasta.HasValue)
        {
            sql += " AND F.FAO_FEC_VTO <= @FechaHasta";
            parameters.Add("FechaHasta", fechaHasta.Value);
        }

        if (soloMorosos)
        {
            sql += " AND F.FAO_FEC_VTO < GETDATE()";
        }

        if (soloVigentes)
        {
            sql += " AND F.FAO_FEC_VTO >= GETDATE()";
        }

        sql += " ORDER BY F.FAO_FEC_VTO ASC";

        return await connection.QueryAsync<CarteraExportDto>(sql, parameters);
    }

    /// <summary>
    /// Obtiene datos de operaciones para exportar
    /// </summary>
    public async Task<IEnumerable<OperacionExportDto>> GetOperacionesParaExportarAsync(
        long? rutCliente = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null,
        int? estado = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT
                O.OPE_NUM_OPE as NumeroOperacion,
                O.OPE_FEC_OPE as FechaOperacion,
                O.OPE_RUT_CLI as RutCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                O.OPE_NUM_LIN as NumeroLinea,
                O.OPE_TIP_OPE as TipoOperacion,
                CASE O.OPE_TIP_OPE
                    WHEN 1 THEN 'Facturas'
                    WHEN 2 THEN 'Cheques'
                    WHEN 3 THEN 'Letras'
                    ELSE 'Otro'
                END as TipoOperacionNombre,
                ISNULL(O.OPE_TOT_DOC, 0) as MontoDocumentos,
                ISNULL(O.OPE_TOT_ANT, 0) as MontoAnticipo,
                ISNULL(O.OPE_TOT_INT, 0) as MontoIntereses,
                ISNULL(O.OPE_TOT_COM, 0) as MontoComision,
                ISNULL(O.OPE_TOT_GAS, 0) as MontoGastos,
                ISNULL(O.OPE_TOT_IVA, 0) as MontoIVA,
                ISNULL(O.OPE_TOT_GIR, 0) as MontoGiro,
                CASE O.OPE_EST_OPE
                    WHEN 1 THEN 'Ingresada'
                    WHEN 2 THEN 'Aprobada'
                    WHEN 3 THEN 'Otorgada'
                    WHEN 4 THEN 'Cerrada'
                    WHEN 5 THEN 'Anulada'
                    ELSE 'Otro'
                END as Estado,
                ISNULL(U.USR_NOM_USR, 'Sistema') as Ejecutivo
            FROM OPE O
            INNER JOIN CLI C ON O.OPE_RUT_CLI = C.CLI_RUT_CLI
            LEFT JOIN USR U ON O.OPE_USR_CRE = U.USR_COD_USR
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (rutCliente.HasValue)
        {
            sql += " AND O.OPE_RUT_CLI = @RutCliente";
            parameters.Add("RutCliente", rutCliente.Value);
        }

        if (fechaDesde.HasValue)
        {
            sql += " AND O.OPE_FEC_OPE >= @FechaDesde";
            parameters.Add("FechaDesde", fechaDesde.Value);
        }

        if (fechaHasta.HasValue)
        {
            sql += " AND O.OPE_FEC_OPE <= @FechaHasta";
            parameters.Add("FechaHasta", fechaHasta.Value);
        }

        if (estado.HasValue)
        {
            sql += " AND O.OPE_EST_OPE = @Estado";
            parameters.Add("Estado", estado.Value);
        }

        sql += " ORDER BY O.OPE_FEC_OPE DESC";

        return await connection.QueryAsync<OperacionExportDto>(sql, parameters);
    }

    /// <summary>
    /// Obtiene datos de lineas de credito para exportar
    /// </summary>
    public async Task<IEnumerable<LineaCreditoExportDto>> GetLineasCreditoParaExportarAsync(
        long? rutCliente = null, bool soloActivas = false)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT
                L.LCR_NUM_LIN as NumeroLinea,
                L.LCR_RUT_CLI as RutCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                L.LCR_FEC_APR as FechaAprobacion,
                L.LCR_FEC_VTO as FechaVencimiento,
                DATEDIFF(DAY, GETDATE(), L.LCR_FEC_VTO) as DiasParaVencer,
                L.LCR_MON_LIN as MontoLinea,
                (L.LCR_MON_LIN - ISNULL(L.LCR_CUP_DIS, L.LCR_MON_LIN)) as CupoUtilizado,
                ISNULL(L.LCR_CUP_DIS, L.LCR_MON_LIN) as CupoDisponible,
                CASE WHEN L.LCR_MON_LIN > 0
                    THEN ((L.LCR_MON_LIN - ISNULL(L.LCR_CUP_DIS, L.LCR_MON_LIN)) / L.LCR_MON_LIN) * 100
                    ELSE 0
                END as PorcentajeUtilizacion,
                CASE L.LCR_EST_LIN
                    WHEN 1 THEN 'Activa'
                    WHEN 2 THEN 'Suspendida'
                    WHEN 3 THEN 'Bloqueada'
                    WHEN 4 THEN 'Bloqueada Morosidad'
                    WHEN 5 THEN 'Cerrada'
                    ELSE 'Otro'
                END as Estado,
                ISNULL(U.USR_NOM_USR, 'Sistema') as Ejecutivo
            FROM LCR L
            INNER JOIN CLI C ON L.LCR_RUT_CLI = C.CLI_RUT_CLI
            LEFT JOIN USR U ON L.LCR_USR_CRE = U.USR_COD_USR
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (rutCliente.HasValue)
        {
            sql += " AND L.LCR_RUT_CLI = @RutCliente";
            parameters.Add("RutCliente", rutCliente.Value);
        }

        if (soloActivas)
        {
            sql += " AND L.LCR_EST_LIN = 1";
        }

        sql += " ORDER BY L.LCR_FEC_VTO ASC";

        return await connection.QueryAsync<LineaCreditoExportDto>(sql, parameters);
    }

    /// <summary>
    /// Obtiene metricas para el dashboard
    /// </summary>
    public async Task<DashboardMetricas> GetDashboardMetricasAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var metricas = new DashboardMetricas();

        // Metricas de cartera
        var carteraStats = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT
                SUM(ISNULL(FAO_SAL_PEN, FAO_MON_FAC)) as CarteraTotal,
                SUM(CASE WHEN FAO_FEC_VTO >= GETDATE() THEN ISNULL(FAO_SAL_PEN, FAO_MON_FAC) ELSE 0 END) as CarteraVigente,
                SUM(CASE WHEN FAO_FEC_VTO < GETDATE() THEN ISNULL(FAO_SAL_PEN, FAO_MON_FAC) ELSE 0 END) as CarteraMorosa,
                COUNT(*) as TotalDocumentos,
                SUM(CASE WHEN FAO_FEC_VTO >= GETDATE() THEN 1 ELSE 0 END) as DocumentosVigentes,
                SUM(CASE WHEN FAO_FEC_VTO < GETDATE() THEN 1 ELSE 0 END) as DocumentosMorosos
            FROM FAO
            WHERE FAO_EST_FAC NOT IN (4, 7)
              AND ISNULL(FAO_SAL_PEN, FAO_MON_FAC) > 0");

        if (carteraStats != null)
        {
            metricas.CarteraTotal = carteraStats.CarteraTotal ?? 0;
            metricas.CarteraVigente = carteraStats.CarteraVigente ?? 0;
            metricas.CarteraMorosa = carteraStats.CarteraMorosa ?? 0;
            metricas.TotalDocumentos = carteraStats.TotalDocumentos ?? 0;
            metricas.DocumentosVigentes = carteraStats.DocumentosVigentes ?? 0;
            metricas.DocumentosMorosos = carteraStats.DocumentosMorosos ?? 0;
            metricas.PorcentajeMorosidad = metricas.CarteraTotal > 0
                ? (metricas.CarteraMorosa / metricas.CarteraTotal) * 100
                : 0;
        }

        // Metricas de operaciones del mes
        var operacionesStats = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT
                COUNT(*) as OperacionesMes,
                ISNULL(SUM(OPE_TOT_DOC), 0) as MontoOperadoMes,
                ISNULL(SUM(OPE_TOT_INT), 0) as InteresesMes,
                ISNULL(SUM(OPE_TOT_COM), 0) as ComisionesMes
            FROM OPE
            WHERE OPE_FEC_OPE >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
              AND OPE_EST_OPE IN (2, 3, 4)");

        if (operacionesStats != null)
        {
            metricas.OperacionesMes = operacionesStats.OperacionesMes ?? 0;
            metricas.MontoOperadoMes = operacionesStats.MontoOperadoMes ?? 0;
            metricas.InteresesMes = operacionesStats.InteresesMes ?? 0;
            metricas.ComisionesMes = operacionesStats.ComisionesMes ?? 0;
        }

        // Metricas de lineas
        var lineasStats = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT
                COUNT(*) as LineasActivas,
                ISNULL(SUM(LCR_MON_LIN), 0) as CupoTotalAprobado,
                ISNULL(SUM(LCR_MON_LIN - ISNULL(LCR_CUP_DIS, LCR_MON_LIN)), 0) as CupoTotalUtilizado,
                ISNULL(SUM(ISNULL(LCR_CUP_DIS, LCR_MON_LIN)), 0) as CupoTotalDisponible
            FROM LCR
            WHERE LCR_EST_LIN = 1");

        if (lineasStats != null)
        {
            metricas.LineasActivas = lineasStats.LineasActivas ?? 0;
            metricas.CupoTotalAprobado = lineasStats.CupoTotalAprobado ?? 0;
            metricas.CupoTotalUtilizado = lineasStats.CupoTotalUtilizado ?? 0;
            metricas.CupoTotalDisponible = lineasStats.CupoTotalDisponible ?? 0;
        }

        // Metricas de cobranza del mes
        var existePag = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PAG'");

        if (existePag > 0)
        {
            var cobranzaStats = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT
                    ISNULL(SUM(PAG_MTO_ABO), 0) as RecaudacionMes,
                    COUNT(*) as PagosMes
                FROM PAG
                WHERE PAG_FEC_PAG >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
                  AND PAG_EST_PAG = 2");

            if (cobranzaStats != null)
            {
                metricas.RecaudacionMes = cobranzaStats.RecaudacionMes ?? 0;
                metricas.PagosMes = cobranzaStats.PagosMes ?? 0;
            }
        }

        // Excedentes pendientes
        var existeExc = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EXC'");

        if (existeExc > 0)
        {
            metricas.ExcedentesPendientes = await connection.ExecuteScalarAsync<decimal>(@"
                SELECT ISNULL(SUM(EXC_MON_EXC), 0) FROM EXC WHERE EXC_EST_EXC = 1");
        }

        // Documentos proximos a vencer (7 dias)
        metricas.DocumentosProximosVencer = (await connection.QueryAsync<DocumentoProximoVencer>(@"
            SELECT TOP 10
                FAO_NUM_OPE as NumeroOperacion,
                FAO_NUM_FAC as NumeroDocumento,
                ISNULL(D.DEU_RAZ_SOC, 'Sin Deudor') as NombreDeudor,
                FAO_FEC_VTO as FechaVencimiento,
                DATEDIFF(DAY, GETDATE(), FAO_FEC_VTO) as DiasParaVencer,
                ISNULL(FAO_SAL_PEN, FAO_MON_FAC) as Monto
            FROM FAO F
            LEFT JOIN DEU D ON F.FAO_RUT_DEU = D.DEU_RUT_DEU AND F.FAO_RUT_CLI = D.DEU_RUT_CLI
            WHERE FAO_EST_FAC IN (1, 2)
              AND FAO_FEC_VTO BETWEEN GETDATE() AND DATEADD(DAY, 7, GETDATE())
              AND ISNULL(FAO_SAL_PEN, FAO_MON_FAC) > 0
            ORDER BY FAO_FEC_VTO ASC")).ToList();

        // Lineas proximas a vencer (30 dias)
        metricas.LineasProximasVencer = (await connection.QueryAsync<LineaProximaVencer>(@"
            SELECT TOP 5
                L.LCR_NUM_LIN as NumeroLinea,
                C.CLI_RAZ_SOC as NombreCliente,
                L.LCR_FEC_VTO as FechaVencimiento,
                DATEDIFF(DAY, GETDATE(), L.LCR_FEC_VTO) as DiasParaVencer,
                (L.LCR_MON_LIN - ISNULL(L.LCR_CUP_DIS, L.LCR_MON_LIN)) as CupoUtilizado
            FROM LCR L
            INNER JOIN CLI C ON L.LCR_RUT_CLI = C.CLI_RUT_CLI
            WHERE L.LCR_EST_LIN = 1
              AND L.LCR_FEC_VTO BETWEEN GETDATE() AND DATEADD(DAY, 30, GETDATE())
            ORDER BY L.LCR_FEC_VTO ASC")).ToList();

        return metricas;
    }

    /// <summary>
    /// Obtiene datos de pagos para exportar
    /// </summary>
    public async Task<IEnumerable<PagoExportDto>> GetPagosParaExportarAsync(
        DateTime? fechaDesde = null, DateTime? fechaHasta = null, long? rutCliente = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var existePag = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PAG'");

        if (existePag == 0)
            return Enumerable.Empty<PagoExportDto>();

        var sql = @"
            SELECT
                P.PAG_NUM_PAG as NumeroPago,
                P.PAG_FEC_PAG as FechaPago,
                CASE P.PAG_TIP_DOC
                    WHEN 1 THEN 'Factura'
                    WHEN 2 THEN 'Cheque'
                    WHEN 3 THEN 'Letra'
                    ELSE 'Otro'
                END as TipoDocumento,
                P.PAG_NUM_DOC as NumeroDocumento,
                P.PAG_NUM_OPE as NumeroOperacion,
                P.PAG_RUT_CLI as RutCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                P.PAG_RUT_DEU as RutDeudor,
                ISNULL(D.DEU_RAZ_SOC, 'Sin Deudor') as NombreDeudor,
                P.PAG_MTO_PAG as MontoPago,
                P.PAG_SAL_DOC as SaldoAnterior,
                (P.PAG_SAL_DOC - P.PAG_MTO_ABO) as SaldoNuevo,
                CASE P.PAG_TIP_PAG
                    WHEN 1 THEN 'Pago Deudor'
                    WHEN 2 THEN 'Pago Cliente'
                    WHEN 3 THEN 'Deposito'
                    ELSE 'Otro'
                END as TipoPago,
                CASE P.PAG_EST_PAG
                    WHEN 1 THEN 'Pendiente'
                    WHEN 2 THEN 'Aprobado'
                    WHEN 3 THEN 'Rechazado'
                    ELSE 'Otro'
                END as Estado
            FROM PAG P
            INNER JOIN CLI C ON P.PAG_RUT_CLI = C.CLI_RUT_CLI
            LEFT JOIN DEU D ON P.PAG_RUT_DEU = D.DEU_RUT_DEU AND P.PAG_RUT_CLI = D.DEU_RUT_CLI
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (fechaDesde.HasValue)
        {
            sql += " AND P.PAG_FEC_PAG >= @FechaDesde";
            parameters.Add("FechaDesde", fechaDesde.Value);
        }

        if (fechaHasta.HasValue)
        {
            sql += " AND P.PAG_FEC_PAG <= @FechaHasta";
            parameters.Add("FechaHasta", fechaHasta.Value);
        }

        if (rutCliente.HasValue)
        {
            sql += " AND P.PAG_RUT_CLI = @RutCliente";
            parameters.Add("RutCliente", rutCliente.Value);
        }

        sql += " ORDER BY P.PAG_FEC_PAG DESC";

        return await connection.QueryAsync<PagoExportDto>(sql, parameters);
    }

    /// <summary>
    /// Obtiene concentracion de cartera por deudor
    /// </summary>
    public async Task<(ResumenConcentracion Resumen, IEnumerable<ConcentracionDeudorDto> Detalle)>
        GetConcentracionPorDeudorAsync(long? rutCliente = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClause = rutCliente.HasValue ? "AND F.FAO_RUT_CLI = @RutCliente" : "";
        var parameters = new DynamicParameters();
        if (rutCliente.HasValue)
            parameters.Add("RutCliente", rutCliente.Value);

        // Obtener concentracion por deudor
        var sql = $@"
            WITH CarteraDeudor AS (
                SELECT
                    F.FAO_RUT_DEU as RutDeudor,
                    ISNULL(D.DEU_RAZ_SOC, 'DEUDOR ' + CAST(F.FAO_RUT_DEU AS VARCHAR)) as NombreDeudor,
                    COUNT(*) as CantidadDocumentos,
                    SUM(F.FAO_MON_FAC) as MontoTotal,
                    SUM(ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC)) as SaldoPendiente,
                    SUM(CASE WHEN F.FAO_FEC_VTO < GETDATE() THEN ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) ELSE 0 END) as MontoMoroso,
                    COUNT(DISTINCT F.FAO_RUT_CLI) as CantidadClientes
                FROM FAO F
                LEFT JOIN DEU D ON F.FAO_RUT_DEU = D.DEU_RUT_DEU
                WHERE F.FAO_EST_FAC NOT IN (4, 7)
                  AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0
                  {whereClause}
                GROUP BY F.FAO_RUT_DEU, D.DEU_RAZ_SOC
            ),
            CarteraTotal AS (
                SELECT SUM(SaldoPendiente) as Total FROM CarteraDeudor
            )
            SELECT
                CD.RutDeudor,
                '' as DvDeudor,
                CD.NombreDeudor,
                CD.CantidadDocumentos,
                CD.MontoTotal,
                CD.SaldoPendiente,
                CASE WHEN CT.Total > 0 THEN (CD.SaldoPendiente / CT.Total) * 100 ELSE 0 END as PorcentajeCartera,
                0 as PorcentajeAcumulado,
                CD.CantidadClientes,
                '' as Clasificacion,
                CD.MontoMoroso,
                CASE WHEN CD.SaldoPendiente > 0 THEN (CD.MontoMoroso / CD.SaldoPendiente) * 100 ELSE 0 END as PorcentajeMorosidad
            FROM CarteraDeudor CD
            CROSS JOIN CarteraTotal CT
            ORDER BY CD.SaldoPendiente DESC";

        var deudores = (await connection.QueryAsync<ConcentracionDeudorDto>(sql, parameters)).ToList();

        // Calcular porcentaje acumulado y clasificacion
        decimal acumulado = 0;
        foreach (var deudor in deudores)
        {
            acumulado += deudor.PorcentajeCartera;
            deudor.PorcentajeAcumulado = Math.Round(acumulado, 2);

            // Clasificacion ABC: A = 80%, B = 15%, C = 5%
            deudor.Clasificacion = acumulado <= 80 ? "A" : acumulado <= 95 ? "B" : "C";
        }

        // Calcular resumen
        var resumen = new ResumenConcentracion
        {
            CarteraTotal = deudores.Sum(d => d.SaldoPendiente),
            TotalDeudores = deudores.Count,
            TotalClientes = deudores.Sum(d => d.CantidadClientes)
        };

        // Top concentracion
        var top5 = deudores.Take(5).Sum(d => d.PorcentajeCartera);
        var top10 = deudores.Take(10).Sum(d => d.PorcentajeCartera);
        var top20 = deudores.Take(20).Sum(d => d.PorcentajeCartera);
        resumen.Top5Porcentaje = Math.Round(top5, 2);
        resumen.Top10Porcentaje = Math.Round(top10, 2);
        resumen.Top20Porcentaje = Math.Round(top20, 2);

        // Indice HHI (Herfindahl-Hirschman)
        // HHI = suma de (participacion%)^2, max 10000
        resumen.IndiceHHI = Math.Round(deudores.Sum(d => (d.PorcentajeCartera / 100) * (d.PorcentajeCartera / 100)) * 10000, 0);
        resumen.NivelConcentracion = resumen.IndiceHHI < 1500 ? "Bajo" :
                                      resumen.IndiceHHI < 2500 ? "Moderado" : "Alto";

        // Por clasificacion
        resumen.DeudoresClaseA = deudores.Count(d => d.Clasificacion == "A");
        resumen.MontoClaseA = deudores.Where(d => d.Clasificacion == "A").Sum(d => d.SaldoPendiente);
        resumen.DeudoresClaseB = deudores.Count(d => d.Clasificacion == "B");
        resumen.MontoClaseB = deudores.Where(d => d.Clasificacion == "B").Sum(d => d.SaldoPendiente);
        resumen.DeudoresClaseC = deudores.Count(d => d.Clasificacion == "C");
        resumen.MontoClaseC = deudores.Where(d => d.Clasificacion == "C").Sum(d => d.SaldoPendiente);

        return (resumen, deudores);
    }

    /// <summary>
    /// Obtiene concentracion de cartera por cliente
    /// </summary>
    public async Task<(ResumenConcentracion Resumen, IEnumerable<ConcentracionClienteDto> Detalle)>
        GetConcentracionPorClienteAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            WITH CarteraCliente AS (
                SELECT
                    F.FAO_RUT_CLI as RutCliente,
                    C.CLI_RAZ_SOC as NombreCliente,
                    COUNT(*) as CantidadDocumentos,
                    COUNT(DISTINCT F.FAO_RUT_DEU) as CantidadDeudores,
                    SUM(F.FAO_MON_FAC) as MontoCartera,
                    SUM(ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC)) as SaldoPendiente,
                    SUM(CASE WHEN F.FAO_FEC_VTO < GETDATE() THEN ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) ELSE 0 END) as MontoMoroso,
                    ISNULL(L.LCR_MON_LIN, 0) as MontoLineaCredito,
                    ISNULL(L.LCR_MON_LIN, 0) - ISNULL(L.LCR_CUP_DIS, ISNULL(L.LCR_MON_LIN, 0)) as CupoUtilizado
                FROM FAO F
                INNER JOIN CLI C ON F.FAO_RUT_CLI = C.CLI_RUT_CLI
                LEFT JOIN LCR L ON F.FAO_RUT_CLI = L.LCR_RUT_CLI AND L.LCR_EST_LIN = 1
                WHERE F.FAO_EST_FAC NOT IN (4, 7)
                  AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0
                GROUP BY F.FAO_RUT_CLI, C.CLI_RAZ_SOC, L.LCR_MON_LIN, L.LCR_CUP_DIS
            ),
            CarteraTotal AS (
                SELECT SUM(SaldoPendiente) as Total FROM CarteraCliente
            )
            SELECT
                CC.RutCliente,
                '' as DvCliente,
                CC.NombreCliente,
                CC.CantidadDocumentos,
                CC.CantidadDeudores,
                CC.MontoCartera,
                CC.SaldoPendiente,
                CASE WHEN CT.Total > 0 THEN (CC.SaldoPendiente / CT.Total) * 100 ELSE 0 END as PorcentajeCartera,
                0 as PorcentajeAcumulado,
                CC.MontoLineaCredito,
                CC.CupoUtilizado,
                CASE WHEN CC.MontoLineaCredito > 0 THEN (CC.CupoUtilizado / CC.MontoLineaCredito) * 100 ELSE 0 END as PorcentajeUtilizacion,
                '' as Clasificacion,
                CC.MontoMoroso,
                CASE WHEN CC.SaldoPendiente > 0 THEN (CC.MontoMoroso / CC.SaldoPendiente) * 100 ELSE 0 END as PorcentajeMorosidad
            FROM CarteraCliente CC
            CROSS JOIN CarteraTotal CT
            ORDER BY CC.SaldoPendiente DESC";

        var clientes = (await connection.QueryAsync<ConcentracionClienteDto>(sql)).ToList();

        // Calcular porcentaje acumulado y clasificacion
        decimal acumulado = 0;
        foreach (var cliente in clientes)
        {
            acumulado += cliente.PorcentajeCartera;
            cliente.PorcentajeAcumulado = Math.Round(acumulado, 2);
            cliente.Clasificacion = acumulado <= 80 ? "A" : acumulado <= 95 ? "B" : "C";
        }

        // Calcular resumen
        var resumen = new ResumenConcentracion
        {
            CarteraTotal = clientes.Sum(c => c.SaldoPendiente),
            TotalClientes = clientes.Count,
            TotalDeudores = clientes.Sum(c => c.CantidadDeudores)
        };

        resumen.Top5Porcentaje = Math.Round(clientes.Take(5).Sum(c => c.PorcentajeCartera), 2);
        resumen.Top10Porcentaje = Math.Round(clientes.Take(10).Sum(c => c.PorcentajeCartera), 2);
        resumen.Top20Porcentaje = Math.Round(clientes.Take(20).Sum(c => c.PorcentajeCartera), 2);

        resumen.IndiceHHI = Math.Round(clientes.Sum(c => (c.PorcentajeCartera / 100) * (c.PorcentajeCartera / 100)) * 10000, 0);
        resumen.NivelConcentracion = resumen.IndiceHHI < 1500 ? "Bajo" :
                                      resumen.IndiceHHI < 2500 ? "Moderado" : "Alto";

        resumen.DeudoresClaseA = clientes.Count(c => c.Clasificacion == "A");
        resumen.MontoClaseA = clientes.Where(c => c.Clasificacion == "A").Sum(c => c.SaldoPendiente);
        resumen.DeudoresClaseB = clientes.Count(c => c.Clasificacion == "B");
        resumen.MontoClaseB = clientes.Where(c => c.Clasificacion == "B").Sum(c => c.SaldoPendiente);
        resumen.DeudoresClaseC = clientes.Count(c => c.Clasificacion == "C");
        resumen.MontoClaseC = clientes.Where(c => c.Clasificacion == "C").Sum(c => c.SaldoPendiente);

        return (resumen, clientes);
    }

    /// <summary>
    /// Obtiene rentabilidad por cliente
    /// </summary>
    public async Task<(ResumenRentabilidad Resumen, IEnumerable<RentabilidadClienteDto> Detalle)>
        GetRentabilidadPorClienteAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var desde = fechaDesde ?? DateTime.Today.AddMonths(-12);
        var hasta = fechaHasta ?? DateTime.Today;

        var sql = @"
            WITH OperacionesCliente AS (
                SELECT
                    O.OPE_RUT_CLI as RutCliente,
                    C.CLI_RAZ_SOC as NombreCliente,
                    COUNT(*) as CantidadOperaciones,
                    SUM(ISNULL(O.OPE_TOT_DOC, 0)) as MontoOperado,
                    SUM(ISNULL(O.OPE_TOT_ANT, 0)) as MontoAnticipado,
                    SUM(ISNULL(O.OPE_TOT_INT, 0)) as InteresesGanados,
                    SUM(ISNULL(O.OPE_TOT_COM, 0)) as ComisionesGanadas,
                    SUM(ISNULL(O.OPE_TOT_GAS, 0)) as GastosRecuperados,
                    AVG(ISNULL(O.OPE_TAS_INT, 0)) as TasaPromedioMensual,
                    AVG(ISNULL(O.OPE_PLA_PRO, 30)) as PlazoPonderadoPromedio
                FROM OPE O
                INNER JOIN CLI C ON O.OPE_RUT_CLI = C.CLI_RUT_CLI
                WHERE O.OPE_FEC_OPE BETWEEN @FechaDesde AND @FechaHasta
                  AND O.OPE_EST_OPE IN (2, 3, 4)
                GROUP BY O.OPE_RUT_CLI, C.CLI_RAZ_SOC
            ),
            MorosidadCliente AS (
                SELECT
                    F.FAO_RUT_CLI as RutCliente,
                    SUM(ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC)) as CarteraActual,
                    SUM(CASE WHEN F.FAO_FEC_VTO < GETDATE() THEN ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) ELSE 0 END) as MontoMoroso
                FROM FAO F
                WHERE F.FAO_EST_FAC NOT IN (4, 7)
                  AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0
                GROUP BY F.FAO_RUT_CLI
            )
            SELECT
                OC.RutCliente,
                '' as DvCliente,
                OC.NombreCliente,
                OC.CantidadOperaciones,
                OC.MontoOperado,
                OC.MontoAnticipado,
                OC.InteresesGanados,
                OC.ComisionesGanadas,
                OC.GastosRecuperados,
                (OC.InteresesGanados + OC.ComisionesGanadas + OC.GastosRecuperados) as TotalIngresos,
                OC.TasaPromedioMensual,
                0 as SpreadPromedio,
                CAST(OC.PlazoPonderadoPromedio AS INT) as PlazoPonderadoPromedio,
                CASE WHEN OC.CantidadOperaciones > 0 THEN OC.MontoOperado / OC.CantidadOperaciones ELSE 0 END as TicketPromedio,
                ISNULL(MC.MontoMoroso, 0) as MontoMoroso,
                CASE WHEN ISNULL(MC.CarteraActual, 0) > 0
                    THEN (ISNULL(MC.MontoMoroso, 0) / MC.CarteraActual) * 100
                    ELSE 0 END as PorcentajeMorosidad,
                ISNULL(MC.MontoMoroso, 0) * 0.02 as ProvisionEstimada,
                (OC.InteresesGanados + OC.ComisionesGanadas + OC.GastosRecuperados) - (ISNULL(MC.MontoMoroso, 0) * 0.02) as IngresosNetos,
                CASE WHEN OC.MontoAnticipado > 0
                    THEN ((OC.InteresesGanados + OC.ComisionesGanadas) / OC.MontoAnticipado) * 100
                    ELSE 0 END as RentabilidadSobreCartera,
                CASE WHEN OC.MontoAnticipado > 0 AND OC.PlazoPonderadoPromedio > 0
                    THEN ((OC.InteresesGanados + OC.ComisionesGanadas) / OC.MontoAnticipado) * (365.0 / OC.PlazoPonderadoPromedio) * 100
                    ELSE 0 END as RentabilidadAnualizada,
                0 as Ranking,
                '' as Clasificacion
            FROM OperacionesCliente OC
            LEFT JOIN MorosidadCliente MC ON OC.RutCliente = MC.RutCliente
            ORDER BY (OC.InteresesGanados + OC.ComisionesGanadas + OC.GastosRecuperados) DESC";

        var clientes = (await connection.QueryAsync<RentabilidadClienteDto>(sql, new { FechaDesde = desde, FechaHasta = hasta })).ToList();

        // Calcular ranking y clasificacion
        int ranking = 1;
        var totalIngresos = clientes.Sum(c => c.TotalIngresos);
        foreach (var cliente in clientes)
        {
            cliente.Ranking = ranking++;

            // Clasificacion por rentabilidad: Premium (top 20%), Estandar (medio 60%), Bajo (bottom 20%)
            var percentil = (double)cliente.Ranking / clientes.Count * 100;
            cliente.Clasificacion = percentil <= 20 ? "Premium" : percentil <= 80 ? "Estandar" : "Bajo";
        }

        // Calcular resumen
        var resumen = new ResumenRentabilidad
        {
            FechaDesde = desde,
            FechaHasta = hasta,
            TotalIntereses = clientes.Sum(c => c.InteresesGanados),
            TotalComisiones = clientes.Sum(c => c.ComisionesGanadas),
            TotalGastos = clientes.Sum(c => c.GastosRecuperados),
            TotalIngresos = clientes.Sum(c => c.TotalIngresos),
            TasaPromedioGeneral = clientes.Average(c => c.TasaPromedioMensual),
            PlazoPromedioGeneral = (int)clientes.Average(c => c.PlazoPonderadoPromedio),
            TicketPromedioGeneral = clientes.Average(c => c.TicketPromedio),
            CarteraPromedio = clientes.Average(c => c.MontoAnticipado),
            RentabilidadCartera = clientes.Average(c => c.RentabilidadSobreCartera),
            ClientesPremium = clientes.Count(c => c.Clasificacion == "Premium"),
            IngresosPremium = clientes.Where(c => c.Clasificacion == "Premium").Sum(c => c.TotalIngresos),
            ClientesEstandar = clientes.Count(c => c.Clasificacion == "Estandar"),
            IngresosEstandar = clientes.Where(c => c.Clasificacion == "Estandar").Sum(c => c.TotalIngresos),
            ClientesBajo = clientes.Count(c => c.Clasificacion == "Bajo"),
            IngresosBajo = clientes.Where(c => c.Clasificacion == "Bajo").Sum(c => c.TotalIngresos)
        };

        return (resumen, clientes);
    }

    /// <summary>
    /// Obtiene evolucion de cartera mensual
    /// </summary>
    public async Task<(ResumenEvolucionCartera Resumen, IEnumerable<EvolucionCarteraMensualDto> Detalle)>
        GetEvolucionCarteraMensualAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? meses = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Si se especifica cantidad de meses, calcular fechas
        var hasta = fechaHasta ?? DateTime.Today;
        var desde = fechaDesde ?? (meses.HasValue
            ? DateTime.Today.AddMonths(-meses.Value + 1)
            : DateTime.Today.AddMonths(-11));

        // Ajustar al primer dia del mes
        desde = new DateTime(desde.Year, desde.Month, 1);
        hasta = new DateTime(hasta.Year, hasta.Month, DateTime.DaysInMonth(hasta.Year, hasta.Month));

        var nombresMeses = new[] { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
            "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

        // Obtener cartera por mes (foto al cierre de cada mes)
        var sqlCartera = @"
            WITH MesesPeriodo AS (
                SELECT
                    YEAR(DATEADD(MONTH, n, @FechaDesde)) as Anio,
                    MONTH(DATEADD(MONTH, n, @FechaDesde)) as Mes,
                    EOMONTH(DATEADD(MONTH, n, @FechaDesde)) as FechaFin
                FROM (SELECT TOP 24 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 as n
                      FROM sys.objects) nums
                WHERE DATEADD(MONTH, n, @FechaDesde) <= @FechaHasta
            ),
            CarteraMes AS (
                SELECT
                    M.Anio,
                    M.Mes,
                    SUM(CASE
                        WHEN F.FAO_FEC_VTO >= M.FechaFin AND F.FAO_EST_FAC NOT IN (4, 7)
                            AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0
                        THEN ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) ELSE 0 END) as CarteraVigente,
                    SUM(CASE
                        WHEN F.FAO_FEC_VTO < M.FechaFin AND F.FAO_EST_FAC NOT IN (4, 7)
                            AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0
                        THEN ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) ELSE 0 END) as CarteraMorosa,
                    COUNT(CASE WHEN F.FAO_FEC_VTO >= M.FechaFin AND F.FAO_EST_FAC NOT IN (4, 7)
                        AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0 THEN 1 END) as DocsVigentes,
                    COUNT(CASE WHEN F.FAO_FEC_VTO < M.FechaFin AND F.FAO_EST_FAC NOT IN (4, 7)
                        AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0 THEN 1 END) as DocsMorosos
                FROM MesesPeriodo M
                LEFT JOIN FAO F ON F.FAO_FEC_VTO <= M.FechaFin
                GROUP BY M.Anio, M.Mes
            )
            SELECT * FROM CarteraMes ORDER BY Anio, Mes";

        var carteraPorMes = await connection.QueryAsync<dynamic>(sqlCartera, new { FechaDesde = desde, FechaHasta = hasta });

        // Obtener operaciones por mes
        var sqlOperaciones = @"
            SELECT
                YEAR(OPE_FEC_OPE) as Anio,
                MONTH(OPE_FEC_OPE) as Mes,
                COUNT(*) as OperacionesNuevas,
                ISNULL(SUM(OPE_TOT_DOC), 0) as MontoOperado,
                ISNULL(SUM(OPE_TOT_ANT), 0) as MontoAnticipado,
                ISNULL(SUM(OPE_TOT_INT), 0) as InteresesGanados,
                ISNULL(SUM(OPE_TOT_COM), 0) as ComisionesGanadas
            FROM OPE
            WHERE OPE_FEC_OPE BETWEEN @FechaDesde AND @FechaHasta
              AND OPE_EST_OPE IN (2, 3, 4)
            GROUP BY YEAR(OPE_FEC_OPE), MONTH(OPE_FEC_OPE)
            ORDER BY Anio, Mes";

        var operacionesPorMes = (await connection.QueryAsync<dynamic>(sqlOperaciones,
            new { FechaDesde = desde, FechaHasta = hasta }))
            .ToDictionary(x => $"{x.Anio}-{x.Mes}", x => x);

        // Obtener recaudacion por mes (si existe tabla PAG)
        Dictionary<string, dynamic> recaudacionPorMes = new();
        var existePag = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PAG'");

        if (existePag > 0)
        {
            var sqlRecaudacion = @"
                SELECT
                    YEAR(PAG_FEC_PAG) as Anio,
                    MONTH(PAG_FEC_PAG) as Mes,
                    ISNULL(SUM(PAG_MTO_ABO), 0) as RecaudacionMes,
                    COUNT(*) as PagosRegistrados
                FROM PAG
                WHERE PAG_FEC_PAG BETWEEN @FechaDesde AND @FechaHasta
                  AND PAG_EST_PAG = 2
                GROUP BY YEAR(PAG_FEC_PAG), MONTH(PAG_FEC_PAG)";

            recaudacionPorMes = (await connection.QueryAsync<dynamic>(sqlRecaudacion,
                new { FechaDesde = desde, FechaHasta = hasta }))
                .ToDictionary(x => $"{x.Anio}-{x.Mes}", x => x);
        }

        // Construir resultado mes a mes
        var resultado = new List<EvolucionCarteraMensualDto>();
        decimal carteraAnterior = 0;
        decimal morosidadAnterior = 0;

        var fechaActual = desde;
        while (fechaActual <= hasta)
        {
            var anio = fechaActual.Year;
            var mes = fechaActual.Month;
            var key = $"{anio}-{mes}";

            var cartera = carteraPorMes.FirstOrDefault(c => (int)c.Anio == anio && (int)c.Mes == mes);
            operacionesPorMes.TryGetValue(key, out var ops);
            recaudacionPorMes.TryGetValue(key, out var rec);

            var carteraVigente = cartera?.CarteraVigente ?? 0m;
            var carteraMorosa = cartera?.CarteraMorosa ?? 0m;
            var carteraTotal = carteraVigente + carteraMorosa;
            var porcentajeMorosidad = carteraTotal > 0 ? (carteraMorosa / carteraTotal) * 100 : 0;

            var item = new EvolucionCarteraMensualDto
            {
                Anio = anio,
                Mes = mes,
                NombreMes = nombresMeses[mes],
                Periodo = $"{anio}-{mes:D2}",

                CarteraTotal = carteraTotal,
                CarteraVigente = carteraVigente,
                CarteraMorosa = carteraMorosa,
                PorcentajeMorosidad = Math.Round(porcentajeMorosidad, 2),
                TotalDocumentos = (cartera?.DocsVigentes ?? 0) + (cartera?.DocsMorosos ?? 0),
                DocumentosVigentes = cartera?.DocsVigentes ?? 0,
                DocumentosMorosos = cartera?.DocsMorosos ?? 0,

                OperacionesNuevas = ops?.OperacionesNuevas ?? 0,
                MontoOperado = ops?.MontoOperado ?? 0m,
                MontoAnticipado = ops?.MontoAnticipado ?? 0m,
                InteresesGanados = ops?.InteresesGanados ?? 0m,
                ComisionesGanadas = ops?.ComisionesGanadas ?? 0m,
                TotalIngresos = (ops?.InteresesGanados ?? 0m) + (ops?.ComisionesGanadas ?? 0m),

                RecaudacionMes = rec?.RecaudacionMes ?? 0m,
                PagosRegistrados = rec?.PagosRegistrados ?? 0,

                VariacionCartera = carteraTotal - carteraAnterior,
                VariacionCarteraPorcentaje = carteraAnterior > 0
                    ? Math.Round(((carteraTotal - carteraAnterior) / carteraAnterior) * 100, 2)
                    : 0,
                VariacionMorosidad = Math.Round(porcentajeMorosidad - morosidadAnterior, 2)
            };

            resultado.Add(item);
            carteraAnterior = carteraTotal;
            morosidadAnterior = porcentajeMorosidad;

            fechaActual = fechaActual.AddMonths(1);
        }

        // Calcular resumen
        var resumen = new ResumenEvolucionCartera
        {
            FechaDesde = desde,
            FechaHasta = hasta,
            TotalMeses = resultado.Count
        };

        if (resultado.Any())
        {
            resumen.CarteraInicial = resultado.First().CarteraTotal;
            resumen.CarteraFinal = resultado.Last().CarteraTotal;
            resumen.CrecimientoCartera = resumen.CarteraFinal - resumen.CarteraInicial;
            resumen.CrecimientoPorcentaje = resumen.CarteraInicial > 0
                ? Math.Round((resumen.CrecimientoCartera / resumen.CarteraInicial) * 100, 2)
                : 0;

            resumen.CarteraPromedio = Math.Round(resultado.Average(r => r.CarteraTotal), 0);
            resumen.MorosidadPromedio = Math.Round(resultado.Average(r => r.PorcentajeMorosidad), 2);
            resumen.OperacionesMesPromedio = Math.Round((decimal)resultado.Average(r => r.OperacionesNuevas), 1);
            resumen.IngresoMensualPromedio = Math.Round(resultado.Average(r => r.TotalIngresos), 0);

            var mesMayorCartera = resultado.OrderByDescending(r => r.CarteraTotal).First();
            resumen.MesMayorCartera = $"{mesMayorCartera.NombreMes} {mesMayorCartera.Anio}";
            resumen.MayorCartera = mesMayorCartera.CarteraTotal;

            var mesMenorMorosidad = resultado.Where(r => r.CarteraTotal > 0).OrderBy(r => r.PorcentajeMorosidad).FirstOrDefault();
            if (mesMenorMorosidad != null)
            {
                resumen.MesMenorMorosidad = $"{mesMenorMorosidad.NombreMes} {mesMenorMorosidad.Anio}";
                resumen.MenorMorosidad = mesMenorMorosidad.PorcentajeMorosidad;
            }

            var mesMayorIngreso = resultado.OrderByDescending(r => r.TotalIngresos).First();
            resumen.MesMayorIngreso = $"{mesMayorIngreso.NombreMes} {mesMayorIngreso.Anio}";
            resumen.MayorIngreso = mesMayorIngreso.TotalIngresos;

            resumen.TotalOperado = resultado.Sum(r => r.MontoOperado);
            resumen.TotalIntereses = resultado.Sum(r => r.InteresesGanados);
            resumen.TotalComisiones = resultado.Sum(r => r.ComisionesGanadas);
            resumen.TotalRecaudado = resultado.Sum(r => r.RecaudacionMes);
        }

        return (resumen, resultado);
    }

    /// <summary>
    /// Obtiene comparativo con año anterior
    /// </summary>
    public async Task<(ResumenComparativoAnual Resumen, IEnumerable<ComparativoMensualDto> DetalleMensual, IEnumerable<ComparativoAnualDto> Metricas)>
        GetComparativoAnioAnteriorAsync(int? anio = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var anioActual = anio ?? DateTime.Today.Year;
        var anioAnterior = anioActual - 1;

        var nombresMeses = new[] { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
            "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

        // Obtener operaciones por mes para ambos años
        var sqlOperaciones = @"
            SELECT
                YEAR(OPE_FEC_OPE) as Anio,
                MONTH(OPE_FEC_OPE) as Mes,
                COUNT(*) as CantidadOperaciones,
                ISNULL(SUM(OPE_TOT_DOC), 0) as MontoOperado,
                ISNULL(SUM(OPE_TOT_INT), 0) as Intereses,
                ISNULL(SUM(OPE_TOT_COM), 0) as Comisiones,
                ISNULL(SUM(OPE_TOT_INT), 0) + ISNULL(SUM(OPE_TOT_COM), 0) as TotalIngresos
            FROM OPE
            WHERE YEAR(OPE_FEC_OPE) IN (@AnioActual, @AnioAnterior)
              AND OPE_EST_OPE IN (2, 3, 4)
            GROUP BY YEAR(OPE_FEC_OPE), MONTH(OPE_FEC_OPE)
            ORDER BY Anio, Mes";

        var operaciones = await connection.QueryAsync<dynamic>(sqlOperaciones,
            new { AnioActual = anioActual, AnioAnterior = anioAnterior });

        var opsPorMesActual = operaciones.Where(o => (int)o.Anio == anioActual)
            .ToDictionary(o => (int)o.Mes, o => o);
        var opsPorMesAnterior = operaciones.Where(o => (int)o.Anio == anioAnterior)
            .ToDictionary(o => (int)o.Mes, o => o);

        // Obtener cartera por mes (snapshot al cierre de cada mes)
        var sqlCartera = @"
            SELECT
                YEAR(FechaCorte) as Anio,
                MONTH(FechaCorte) as Mes,
                SUM(Cartera) as CarteraTotal,
                SUM(Morosa) as CarteraMorosa
            FROM (
                SELECT
                    EOMONTH(DATEFROMPARTS(@AnioActual, n.Mes, 1)) as FechaCorte,
                    ISNULL(SUM(CASE WHEN F.FAO_EST_FAC NOT IN (4,7) AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0
                        THEN ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) ELSE 0 END), 0) as Cartera,
                    ISNULL(SUM(CASE WHEN F.FAO_EST_FAC NOT IN (4,7) AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0
                        AND F.FAO_FEC_VTO < EOMONTH(DATEFROMPARTS(@AnioActual, n.Mes, 1))
                        THEN ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) ELSE 0 END), 0) as Morosa
                FROM (SELECT 1 as Mes UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5 UNION SELECT 6
                      UNION SELECT 7 UNION SELECT 8 UNION SELECT 9 UNION SELECT 10 UNION SELECT 11 UNION SELECT 12) n
                LEFT JOIN FAO F ON 1=1
                GROUP BY n.Mes
                UNION ALL
                SELECT
                    EOMONTH(DATEFROMPARTS(@AnioAnterior, n.Mes, 1)) as FechaCorte,
                    ISNULL(SUM(CASE WHEN F.FAO_EST_FAC NOT IN (4,7) AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0
                        THEN ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) ELSE 0 END), 0) as Cartera,
                    ISNULL(SUM(CASE WHEN F.FAO_EST_FAC NOT IN (4,7) AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0
                        AND F.FAO_FEC_VTO < EOMONTH(DATEFROMPARTS(@AnioAnterior, n.Mes, 1))
                        THEN ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) ELSE 0 END), 0) as Morosa
                FROM (SELECT 1 as Mes UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5 UNION SELECT 6
                      UNION SELECT 7 UNION SELECT 8 UNION SELECT 9 UNION SELECT 10 UNION SELECT 11 UNION SELECT 12) n
                LEFT JOIN FAO F ON 1=1
                GROUP BY n.Mes
            ) sub
            GROUP BY YEAR(FechaCorte), MONTH(FechaCorte)";

        var cartera = await connection.QueryAsync<dynamic>(sqlCartera,
            new { AnioActual = anioActual, AnioAnterior = anioAnterior });

        var carteraPorMesActual = cartera.Where(c => (int)c.Anio == anioActual)
            .ToDictionary(c => (int)c.Mes, c => c);
        var carteraPorMesAnterior = cartera.Where(c => (int)c.Anio == anioAnterior)
            .ToDictionary(c => (int)c.Mes, c => c);

        // Construir detalle mensual
        var detalleMensual = new List<ComparativoMensualDto>();
        for (int mes = 1; mes <= 12; mes++)
        {
            opsPorMesActual.TryGetValue(mes, out var opsAct);
            opsPorMesAnterior.TryGetValue(mes, out var opsAnt);
            carteraPorMesActual.TryGetValue(mes, out var cartAct);
            carteraPorMesAnterior.TryGetValue(mes, out var cartAnt);

            var carteraActual = cartAct?.CarteraTotal ?? 0m;
            var carteraAnteriorMes = cartAnt?.CarteraTotal ?? 0m;
            var morosidadActual = carteraActual > 0 ? ((cartAct?.CarteraMorosa ?? 0m) / carteraActual) * 100 : 0;
            var morosidadAnteriorMes = carteraAnteriorMes > 0 ? ((cartAnt?.CarteraMorosa ?? 0m) / carteraAnteriorMes) * 100 : 0;
            var ingresosActual = opsAct?.TotalIngresos ?? 0m;
            var ingresosAnteriorMes = opsAnt?.TotalIngresos ?? 0m;
            var operacionesActual = opsAct?.CantidadOperaciones ?? 0;
            var operacionesAnteriorMes = opsAnt?.CantidadOperaciones ?? 0;

            detalleMensual.Add(new ComparativoMensualDto
            {
                Mes = mes,
                NombreMes = nombresMeses[mes],
                CarteraActual = carteraActual,
                OperacionesActual = operacionesActual,
                IngresosActual = ingresosActual,
                MorosidadActual = Math.Round(morosidadActual, 2),
                CarteraAnterior = carteraAnteriorMes,
                OperacionesAnterior = operacionesAnteriorMes,
                IngresosAnterior = ingresosAnteriorMes,
                MorosidadAnterior = Math.Round(morosidadAnteriorMes, 2),
                VariacionCartera = carteraAnteriorMes > 0
                    ? Math.Round(((carteraActual - carteraAnteriorMes) / carteraAnteriorMes) * 100, 2) : 0,
                VariacionOperaciones = operacionesAnteriorMes > 0
                    ? Math.Round(((decimal)(operacionesActual - operacionesAnteriorMes) / operacionesAnteriorMes) * 100, 2) : 0,
                VariacionIngresos = ingresosAnteriorMes > 0
                    ? Math.Round(((ingresosActual - ingresosAnteriorMes) / ingresosAnteriorMes) * 100, 2) : 0,
                VariacionMorosidad = Math.Round(morosidadActual - morosidadAnteriorMes, 2)
            });
        }

        // Calcular métricas agregadas
        var metricas = new List<ComparativoAnualDto>();

        // Agregar métricas
        void AgregarMetrica(string categoria, string metrica, decimal valorActual, decimal valorAnterior, int cantActual = 0, int cantAnterior = 0)
        {
            var variacion = valorAnterior != 0 ? ((valorActual - valorAnterior) / Math.Abs(valorAnterior)) * 100 : 0;
            metricas.Add(new ComparativoAnualDto
            {
                Categoria = categoria,
                Metrica = metrica,
                ValorActual = valorActual,
                ValorAnterior = valorAnterior,
                CantidadActual = cantActual,
                CantidadAnterior = cantAnterior,
                VariacionAbsoluta = valorActual - valorAnterior,
                VariacionPorcentaje = Math.Round(variacion, 2),
                Tendencia = variacion > 1 ? "Subió" : variacion < -1 ? "Bajó" : "Estable"
            });
        }

        // Totales del año
        var totalOpsActual = opsPorMesActual.Values.Sum(o => (int)o.CantidadOperaciones);
        var totalOpsAnterior = opsPorMesAnterior.Values.Sum(o => (int)o.CantidadOperaciones);
        var totalMontoActual = opsPorMesActual.Values.Sum(o => (decimal)o.MontoOperado);
        var totalMontoAnterior = opsPorMesAnterior.Values.Sum(o => (decimal)o.MontoOperado);
        var totalIngresosActual = opsPorMesActual.Values.Sum(o => (decimal)o.TotalIngresos);
        var totalIngresosAnterior = opsPorMesAnterior.Values.Sum(o => (decimal)o.TotalIngresos);
        var totalInteresesActual = opsPorMesActual.Values.Sum(o => (decimal)o.Intereses);
        var totalInteresesAnterior = opsPorMesAnterior.Values.Sum(o => (decimal)o.Intereses);
        var totalComisionesActual = opsPorMesActual.Values.Sum(o => (decimal)o.Comisiones);
        var totalComisionesAnterior = opsPorMesAnterior.Values.Sum(o => (decimal)o.Comisiones);

        AgregarMetrica("Operaciones", "Cantidad de Operaciones", totalOpsActual, totalOpsAnterior, totalOpsActual, totalOpsAnterior);
        AgregarMetrica("Operaciones", "Monto Total Operado", totalMontoActual, totalMontoAnterior);
        AgregarMetrica("Ingresos", "Total Ingresos", totalIngresosActual, totalIngresosAnterior);
        AgregarMetrica("Ingresos", "Intereses Ganados", totalInteresesActual, totalInteresesAnterior);
        AgregarMetrica("Ingresos", "Comisiones Ganadas", totalComisionesActual, totalComisionesAnterior);

        // Promedios mensuales
        var promedioCarteraActual = detalleMensual.Where(d => d.CarteraActual > 0).DefaultIfEmpty().Average(d => d?.CarteraActual ?? 0);
        var promedioCarteraAnterior = detalleMensual.Where(d => d.CarteraAnterior > 0).DefaultIfEmpty().Average(d => d?.CarteraAnterior ?? 0);
        var promedioMorosidadActual = detalleMensual.Where(d => d.CarteraActual > 0).DefaultIfEmpty().Average(d => d?.MorosidadActual ?? 0);
        var promedioMorosidadAnterior = detalleMensual.Where(d => d.CarteraAnterior > 0).DefaultIfEmpty().Average(d => d?.MorosidadAnterior ?? 0);

        AgregarMetrica("Cartera", "Cartera Promedio Mensual", promedioCarteraActual, promedioCarteraAnterior);
        AgregarMetrica("Cartera", "Morosidad Promedio (%)", promedioMorosidadActual, promedioMorosidadAnterior);

        // Calcular resumen
        var mejorMesActual = detalleMensual.OrderByDescending(d => d.IngresosActual).First();
        var mejorMesAnterior = detalleMensual.OrderByDescending(d => d.IngresosAnterior).First();

        var resumen = new ResumenComparativoAnual
        {
            AnioActual = anioActual,
            AnioAnterior = anioAnterior,
            CarteraActual = promedioCarteraActual,
            CarteraAnterior = promedioCarteraAnterior,
            CrecimientoCartera = promedioCarteraActual - promedioCarteraAnterior,
            CrecimientoCarteraPorcentaje = promedioCarteraAnterior > 0
                ? Math.Round(((promedioCarteraActual - promedioCarteraAnterior) / promedioCarteraAnterior) * 100, 2) : 0,
            OperacionesActual = totalOpsActual,
            OperacionesAnterior = totalOpsAnterior,
            CrecimientoOperaciones = totalOpsAnterior > 0
                ? Math.Round(((decimal)(totalOpsActual - totalOpsAnterior) / totalOpsAnterior) * 100, 2) : 0,
            MontoOperadoActual = totalMontoActual,
            MontoOperadoAnterior = totalMontoAnterior,
            CrecimientoMonto = totalMontoAnterior > 0
                ? Math.Round(((totalMontoActual - totalMontoAnterior) / totalMontoAnterior) * 100, 2) : 0,
            IngresosActual = totalIngresosActual,
            IngresosAnterior = totalIngresosAnterior,
            CrecimientoIngresos = totalIngresosActual - totalIngresosAnterior,
            CrecimientoIngresosPorcentaje = totalIngresosAnterior > 0
                ? Math.Round(((totalIngresosActual - totalIngresosAnterior) / totalIngresosAnterior) * 100, 2) : 0,
            MorosidadPromedioActual = Math.Round(promedioMorosidadActual, 2),
            MorosidadPromedioAnterior = Math.Round(promedioMorosidadAnterior, 2),
            VariacionMorosidad = Math.Round(promedioMorosidadActual - promedioMorosidadAnterior, 2),
            MejorMesActual = mejorMesActual.NombreMes,
            MejorIngresoActual = mejorMesActual.IngresosActual,
            MejorMesAnterior = mejorMesAnterior.NombreMes,
            MejorIngresoAnterior = mejorMesAnterior.IngresosAnterior
        };

        return (resumen, detalleMensual, metricas);
    }

    /// <summary>
    /// Obtiene proyección de flujo de caja
    /// </summary>
    public async Task<(ResumenProyeccionFlujo Resumen, IEnumerable<FlujoCajaDiarioDto> DetalleDiario, IEnumerable<FlujoCajaSemanalDto> DetalleSemanal)>
        GetProyeccionFlujoCajaAsync(int dias = 30, decimal? saldoInicial = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var fechaInicio = DateTime.Today;
        var fechaFin = fechaInicio.AddDays(dias);
        var diasSemana = new[] { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };

        // Calcular tasa de recuperación histórica (últimos 90 días)
        var sqlRecuperacion = @"
            SELECT
                ISNULL(SUM(CASE WHEN PAG_EST_PAG = 2 THEN PAG_MTO_ABO ELSE 0 END), 0) as Cobrado,
                ISNULL((SELECT SUM(ISNULL(FAO_SAL_PEN, FAO_MON_FAC))
                        FROM FAO WHERE FAO_FEC_VTO BETWEEN DATEADD(DAY, -90, GETDATE()) AND GETDATE()
                        AND FAO_EST_FAC NOT IN (4, 7)), 1) as Vencido
            FROM PAG
            WHERE PAG_FEC_PAG >= DATEADD(DAY, -90, GETDATE())";

        decimal tasaRecuperacion = 0.85m; // Default 85%
        var existePag = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PAG'");

        if (existePag > 0)
        {
            var recuperacion = await connection.QueryFirstOrDefaultAsync<dynamic>(sqlRecuperacion);
            if (recuperacion != null && recuperacion.Vencido > 0)
            {
                tasaRecuperacion = Math.Min(1, Math.Max(0.5m, (decimal)recuperacion.Cobrado / (decimal)recuperacion.Vencido));
            }
        }

        // Obtener documentos que vencen por día
        var sqlVencimientos = @"
            SELECT
                FAO_FEC_VTO as FechaVencimiento,
                COUNT(*) as CantidadDocumentos,
                SUM(ISNULL(FAO_SAL_PEN, FAO_MON_FAC)) as MontoTotal
            FROM FAO
            WHERE FAO_FEC_VTO BETWEEN @FechaInicio AND @FechaFin
              AND FAO_EST_FAC NOT IN (4, 7)
              AND ISNULL(FAO_SAL_PEN, FAO_MON_FAC) > 0
            GROUP BY FAO_FEC_VTO
            ORDER BY FAO_FEC_VTO";

        var vencimientos = (await connection.QueryAsync<dynamic>(sqlVencimientos,
            new { FechaInicio = fechaInicio, FechaFin = fechaFin }))
            .ToDictionary(v => ((DateTime)v.FechaVencimiento).Date, v => v);

        // Calcular promedio de giros diarios (últimos 30 días hábiles)
        var sqlGirosHistoricos = @"
            SELECT
                AVG(GiroDiario) as PromedioGiro,
                AVG(Operaciones) as PromedioOperaciones
            FROM (
                SELECT
                    OPE_FEC_OPE as Fecha,
                    SUM(ISNULL(OPE_TOT_GIR, 0)) as GiroDiario,
                    COUNT(*) as Operaciones
                FROM OPE
                WHERE OPE_FEC_OPE >= DATEADD(DAY, -45, GETDATE())
                  AND OPE_EST_OPE IN (3, 4)
                  AND DATEPART(WEEKDAY, OPE_FEC_OPE) NOT IN (1, 7)
                GROUP BY OPE_FEC_OPE
            ) sub";

        var girosHistoricos = await connection.QueryFirstOrDefaultAsync<dynamic>(sqlGirosHistoricos);
        var promedioGiroDiario = girosHistoricos?.PromedioGiro ?? 0m;
        var promedioOpsDiarias = girosHistoricos?.PromedioOperaciones ?? 0;

        // Obtener saldo inicial (excedentes disponibles o usar parámetro)
        decimal saldoInicialCalculado = saldoInicial ?? 0;
        if (!saldoInicial.HasValue)
        {
            var existeExc = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EXC'");
            if (existeExc > 0)
            {
                saldoInicialCalculado = await connection.ExecuteScalarAsync<decimal>(
                    "SELECT ISNULL(SUM(EXC_MON_EXC), 0) FROM EXC WHERE EXC_EST_EXC = 1");
            }
        }

        // Construir proyección diaria
        var detalleDiario = new List<FlujoCajaDiarioDto>();
        decimal saldoAcumulado = saldoInicialCalculado;

        for (var fecha = fechaInicio; fecha <= fechaFin; fecha = fecha.AddDays(1))
        {
            var diaSemanaNum = (int)fecha.DayOfWeek;
            var esHabil = diaSemanaNum != 0 && diaSemanaNum != 6; // No domingo ni sábado

            vencimientos.TryGetValue(fecha.Date, out var venc);

            var cobranzaProyectada = venc?.MontoTotal ?? 0m;
            var documentosVencen = venc?.CantidadDocumentos ?? 0;
            var cobranzaAjustada = esHabil ? cobranzaProyectada * tasaRecuperacion : 0;

            // Giros solo en días hábiles, con variación aleatoria basada en día
            var factorDia = 0.8m + (diaSemanaNum * 0.05m); // Lunes menos, viernes más
            var girosProyectados = esHabil ? promedioGiroDiario * factorDia : 0;
            var operacionesProyectadas = esHabil ? (int)(promedioOpsDiarias * factorDia) : 0;

            var flujoNeto = cobranzaAjustada - girosProyectados;
            saldoAcumulado += flujoNeto;

            detalleDiario.Add(new FlujoCajaDiarioDto
            {
                Fecha = fecha,
                DiaSemana = diasSemana[diaSemanaNum],
                EsHabil = esHabil,
                CobranzaProyectada = cobranzaProyectada,
                DocumentosVencen = documentosVencen,
                CobranzaAjustada = Math.Round(cobranzaAjustada, 0),
                GirosProyectados = Math.Round(girosProyectados, 0),
                OperacionesProyectadas = operacionesProyectadas,
                FlujoNeto = Math.Round(flujoNeto, 0),
                SaldoAcumulado = Math.Round(saldoAcumulado, 0)
            });
        }

        // Agrupar por semana
        var detalleSemanal = detalleDiario
            .GroupBy(d => GetWeekNumber(d.Fecha))
            .Select((g, idx) =>
            {
                var items = g.ToList();
                return new FlujoCajaSemanalDto
                {
                    NumeroSemana = idx + 1,
                    FechaInicio = items.Min(i => i.Fecha),
                    FechaFin = items.Max(i => i.Fecha),
                    Periodo = $"{items.Min(i => i.Fecha):dd/MM} - {items.Max(i => i.Fecha):dd/MM}",
                    CobranzaProyectada = items.Sum(i => i.CobranzaAjustada),
                    DocumentosVencen = items.Sum(i => i.DocumentosVencen),
                    GirosProyectados = items.Sum(i => i.GirosProyectados),
                    OperacionesProyectadas = items.Sum(i => i.OperacionesProyectadas),
                    FlujoNeto = items.Sum(i => i.FlujoNeto),
                    SaldoAcumulado = items.Last().SaldoAcumulado
                };
            }).ToList();

        // Calcular resumen
        var minimoSaldo = detalleDiario.MinBy(d => d.SaldoAcumulado);
        var mejorSemana = detalleSemanal.OrderByDescending(s => s.FlujoNeto).FirstOrDefault();
        var peorSemana = detalleSemanal.OrderBy(s => s.FlujoNeto).FirstOrDefault();

        var resumen = new ResumenProyeccionFlujo
        {
            FechaProyeccion = DateTime.Now,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            DiasProyectados = dias,
            SaldoInicial = saldoInicialCalculado,
            TotalCobranzaProyectada = detalleDiario.Sum(d => d.CobranzaProyectada),
            TotalCobranzaAjustada = detalleDiario.Sum(d => d.CobranzaAjustada),
            TotalDocumentosVencen = detalleDiario.Sum(d => d.DocumentosVencen),
            TotalGirosProyectados = detalleDiario.Sum(d => d.GirosProyectados),
            TotalOperacionesProyectadas = detalleDiario.Sum(d => d.OperacionesProyectadas),
            FlujoNetoTotal = detalleDiario.Sum(d => d.FlujoNeto),
            SaldoFinalProyectado = detalleDiario.Last().SaldoAcumulado,
            TasaRecuperacionHistorica = Math.Round(tasaRecuperacion * 100, 1),
            PromedioGiroDiario = Math.Round(promedioGiroDiario, 0),
            PromedioCobranzaDiaria = Math.Round(detalleDiario.Where(d => d.EsHabil).Average(d => d.CobranzaAjustada), 0),
            DiasConFlujoNegativo = detalleDiario.Count(d => d.FlujoNeto < 0),
            MinimoSaldoProyectado = minimoSaldo?.SaldoAcumulado ?? 0,
            FechaMinimoSaldo = minimoSaldo?.Fecha,
            MejorSemanaPorFlujo = mejorSemana?.FlujoNeto ?? 0,
            NumeroMejorSemana = mejorSemana?.NumeroSemana ?? 0,
            PeorSemanaPorFlujo = peorSemana?.FlujoNeto ?? 0,
            NumeroPeorSemana = peorSemana?.NumeroSemana ?? 0
        };

        return (resumen, detalleDiario, detalleSemanal);
    }

    private static int GetWeekNumber(DateTime date)
    {
        var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }
}
