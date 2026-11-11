// =============================================
// Servicio de Alertas - Vencimientos y Notificaciones
// =============================================

using Dapper;
using Factoring.Core.Interfaces;

namespace Factoring.Application.Services;

/// <summary>
/// Alerta de vencimiento de línea de crédito
/// </summary>
public class AlertaVencimientoLinea
{
    public int NumeroLinea { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
    public int DiasParaVencer { get; set; }
    public decimal MontoLinea { get; set; }
    public decimal CupoDisponible { get; set; }
    public decimal CupoUtilizado { get; set; }
    public int EstadoLinea { get; set; }
    public string TipoAlerta { get; set; } = string.Empty; // "ProximoVencer", "Vencida"
}

/// <summary>
/// Alerta de documentos por vencer
/// </summary>
public class AlertaDocumentoVencer
{
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public string TipoDocumentoNombre { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
    public int DiasParaVencer { get; set; }
    public decimal MontoPendiente { get; set; }
}

/// <summary>
/// Resumen de alertas del sistema
/// </summary>
public class ResumenAlertas
{
    public int LineasProximasVencer { get; set; }
    public int LineasVencidas { get; set; }
    public int DocumentosVencer7Dias { get; set; }
    public int DocumentosVencer15Dias { get; set; }
    public int DocumentosVencer30Dias { get; set; }
    public int DocumentosMorosos { get; set; }
    public decimal MontoMorosoTotal { get; set; }
    public List<AlertaVencimientoLinea> AlertasLineas { get; set; } = new();
    public List<AlertaDocumentoVencer> AlertasDocumentos { get; set; } = new();
}

/// <summary>
/// Servicio de alertas y notificaciones
/// </summary>
public class AlertaService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AlertaService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Obtiene alertas de líneas próximas a vencer o vencidas
    /// </summary>
    public async Task<IEnumerable<AlertaVencimientoLinea>> GetAlertasLineasVencimientoAsync(int diasAnticipacion = 30)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                L.LCR_NUM_LIN as NumeroLinea,
                L.LCR_RUT_CLI as RutCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                L.LCR_FEC_VTO as FechaVencimiento,
                DATEDIFF(DAY, GETDATE(), L.LCR_FEC_VTO) as DiasParaVencer,
                L.LCR_MON_LIN as MontoLinea,
                L.LCR_CUP_DIS as CupoDisponible,
                (L.LCR_MON_LIN - ISNULL(L.LCR_CUP_DIS, L.LCR_MON_LIN)) as CupoUtilizado,
                L.LCR_EST_LIN as EstadoLinea,
                CASE
                    WHEN L.LCR_FEC_VTO < GETDATE() THEN 'Vencida'
                    ELSE 'ProximoVencer'
                END as TipoAlerta
            FROM LCR L
            INNER JOIN CLI C ON L.LCR_RUT_CLI = C.CLI_RUT_CLI
            WHERE L.LCR_EST_LIN = 1  -- Solo activas
              AND L.LCR_FEC_VTO <= DATEADD(DAY, @DiasAnticipacion, GETDATE())
            ORDER BY L.LCR_FEC_VTO ASC";

        return await connection.QueryAsync<AlertaVencimientoLinea>(sql, new { DiasAnticipacion = diasAnticipacion });
    }

    /// <summary>
    /// Obtiene alertas de documentos próximos a vencer
    /// </summary>
    public async Task<IEnumerable<AlertaDocumentoVencer>> GetAlertasDocumentosVencerAsync(int diasAnticipacion = 30)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT
                F.FAO_NUM_OPE as NumeroOperacion,
                1 as TipoDocumento,
                'Factura' as TipoDocumentoNombre,
                F.FAO_NUM_FAC as NumeroDocumento,
                F.FAO_RUT_CLI as RutCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                F.FAO_RUT_DEU as RutDeudor,
                D.DEU_RAZ_SOC as NombreDeudor,
                F.FAO_FEC_VTO as FechaVencimiento,
                DATEDIFF(DAY, GETDATE(), F.FAO_FEC_VTO) as DiasParaVencer,
                ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) as MontoPendiente
            FROM FAO F
            INNER JOIN CLI C ON F.FAO_RUT_CLI = C.CLI_RUT_CLI
            LEFT JOIN DEU D ON F.FAO_RUT_DEU = D.DEU_RUT_DEU AND F.FAO_RUT_CLI = D.DEU_RUT_CLI
            WHERE F.FAO_EST_FAC IN (1, 2) -- Vigente o prorrogado
              AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0
              AND F.FAO_FEC_VTO BETWEEN GETDATE() AND DATEADD(DAY, @DiasAnticipacion, GETDATE())

            UNION ALL

            SELECT
                CH.CHO_NUM_OPE,
                2,
                'Cheque',
                CH.CHO_NUM_FAC,
                CH.CHO_RUT_CLI,
                C.CLI_RAZ_SOC,
                CH.CHO_RUT_DEU,
                D.DEU_RAZ_SOC,
                CH.CHO_FEC_VTO,
                DATEDIFF(DAY, GETDATE(), CH.CHO_FEC_VTO),
                ISNULL(CH.CHO_SAL_PEN, CH.CHO_MON_FAC)
            FROM CHO CH
            INNER JOIN CLI C ON CH.CHO_RUT_CLI = C.CLI_RUT_CLI
            LEFT JOIN DEU D ON CH.CHO_RUT_DEU = D.DEU_RUT_DEU AND CH.CHO_RUT_CLI = D.DEU_RUT_CLI
            WHERE CH.CHO_EST_FAC IN (1, 2)
              AND ISNULL(CH.CHO_SAL_PEN, CH.CHO_MON_FAC) > 0
              AND CH.CHO_FEC_VTO BETWEEN GETDATE() AND DATEADD(DAY, @DiasAnticipacion, GETDATE())

            ORDER BY FechaVencimiento ASC";

        return await connection.QueryAsync<AlertaDocumentoVencer>(sql, new { DiasAnticipacion = diasAnticipacion });
    }

    /// <summary>
    /// Obtiene resumen completo de alertas del sistema
    /// </summary>
    public async Task<ResumenAlertas> GetResumenAlertasAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var resumen = new ResumenAlertas();

        // Alertas de líneas
        const string sqlLineas = @"
            SELECT
                SUM(CASE WHEN LCR_FEC_VTO >= GETDATE() AND LCR_FEC_VTO <= DATEADD(DAY, 30, GETDATE()) THEN 1 ELSE 0 END) as ProximasVencer,
                SUM(CASE WHEN LCR_FEC_VTO < GETDATE() THEN 1 ELSE 0 END) as Vencidas
            FROM LCR WHERE LCR_EST_LIN = 1";

        var statsLineas = await connection.QueryFirstOrDefaultAsync<dynamic>(sqlLineas);
        resumen.LineasProximasVencer = statsLineas?.ProximasVencer ?? 0;
        resumen.LineasVencidas = statsLineas?.Vencidas ?? 0;

        // Alertas de documentos
        const string sqlDocs = @"
            SELECT
                SUM(CASE WHEN FAO_FEC_VTO BETWEEN GETDATE() AND DATEADD(DAY, 7, GETDATE()) THEN 1 ELSE 0 END) as Vencer7,
                SUM(CASE WHEN FAO_FEC_VTO BETWEEN GETDATE() AND DATEADD(DAY, 15, GETDATE()) THEN 1 ELSE 0 END) as Vencer15,
                SUM(CASE WHEN FAO_FEC_VTO BETWEEN GETDATE() AND DATEADD(DAY, 30, GETDATE()) THEN 1 ELSE 0 END) as Vencer30,
                SUM(CASE WHEN FAO_FEC_VTO < GETDATE() AND FAO_EST_FAC NOT IN (4, 7) THEN 1 ELSE 0 END) as Morosos,
                SUM(CASE WHEN FAO_FEC_VTO < GETDATE() AND FAO_EST_FAC NOT IN (4, 7) THEN ISNULL(FAO_SAL_PEN, FAO_MON_FAC) ELSE 0 END) as MontoMoroso
            FROM FAO
            WHERE FAO_EST_FAC NOT IN (4, 7)
              AND ISNULL(FAO_SAL_PEN, FAO_MON_FAC) > 0";

        var statsDocs = await connection.QueryFirstOrDefaultAsync<dynamic>(sqlDocs);
        resumen.DocumentosVencer7Dias = statsDocs?.Vencer7 ?? 0;
        resumen.DocumentosVencer15Dias = statsDocs?.Vencer15 ?? 0;
        resumen.DocumentosVencer30Dias = statsDocs?.Vencer30 ?? 0;
        resumen.DocumentosMorosos = statsDocs?.Morosos ?? 0;
        resumen.MontoMorosoTotal = statsDocs?.MontoMoroso ?? 0m;

        // Obtener detalles
        resumen.AlertasLineas = (await GetAlertasLineasVencimientoAsync(30)).ToList();
        resumen.AlertasDocumentos = (await GetAlertasDocumentosVencerAsync(7)).Take(10).ToList();

        return resumen;
    }

    /// <summary>
    /// Bloquea automáticamente líneas vencidas
    /// </summary>
    public async Task<int> BloquearLineasVencidasAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var rowsAffected = await connection.ExecuteAsync(@"
            UPDATE LCR SET
                LCR_EST_LIN = 3,  -- Bloqueada
                LCR_FEC_MOD = GETDATE(),
                LCR_OBS_LIN = ISNULL(LCR_OBS_LIN, '') + ' [Bloqueada automáticamente por vencimiento ' + CONVERT(VARCHAR, GETDATE(), 103) + ']'
            WHERE LCR_EST_LIN = 1
              AND LCR_FEC_VTO < GETDATE()");

        return rowsAffected;
    }

    /// <summary>
    /// Marca documentos como morosos después de X días de vencidos
    /// </summary>
    public async Task<int> MarcarDocumentosMorososAsync(int diasGracia = 30)
    {
        using var connection = _connectionFactory.CreateConnection();

        var rowsFacturas = await connection.ExecuteAsync(@"
            UPDATE FAO SET
                FAO_EST_FAC = 5,  -- Moroso
                FAO_FEC_MOD = GETDATE()
            WHERE FAO_EST_FAC = 1
              AND FAO_FEC_VTO < DATEADD(DAY, -@DiasGracia, GETDATE())
              AND ISNULL(FAO_SAL_PEN, FAO_MON_FAC) > 0",
            new { DiasGracia = diasGracia });

        var rowsCheques = await connection.ExecuteAsync(@"
            UPDATE CHO SET
                CHO_EST_FAC = 5,  -- Moroso
                CHO_FEC_MOD = GETDATE()
            WHERE CHO_EST_FAC = 1
              AND CHO_FEC_VTO < DATEADD(DAY, -@DiasGracia, GETDATE())
              AND ISNULL(CHO_SAL_PEN, CHO_MON_FAC) > 0",
            new { DiasGracia = diasGracia });

        return rowsFacturas + rowsCheques;
    }

    /// <summary>
    /// Obtiene clientes con morosidad para bloqueo de línea
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetClientesConMorosidadAsync(int diasMoroso = 30)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT DISTINCT
                F.FAO_RUT_CLI as RutCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                COUNT(*) as CantidadDocumentosMorosos,
                SUM(ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC)) as MontoMorosoTotal,
                MIN(F.FAO_FEC_VTO) as FechaVencimientoMasAntigua,
                MAX(DATEDIFF(DAY, F.FAO_FEC_VTO, GETDATE())) as DiasMaxMorosidad
            FROM FAO F
            INNER JOIN CLI C ON F.FAO_RUT_CLI = C.CLI_RUT_CLI
            WHERE F.FAO_EST_FAC IN (1, 5)
              AND F.FAO_FEC_VTO < DATEADD(DAY, -@DiasMoroso, GETDATE())
              AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0
            GROUP BY F.FAO_RUT_CLI, C.CLI_RAZ_SOC
            HAVING COUNT(*) > 0
            ORDER BY MontoMorosoTotal DESC";

        return await connection.QueryAsync(sql, new { DiasMoroso = diasMoroso });
    }

    /// <summary>
    /// Bloquea línea de un cliente por morosidad
    /// </summary>
    public async Task<bool> BloquearLineaPorMorosidadAsync(long rutCliente, string usuario, string motivo)
    {
        using var connection = _connectionFactory.CreateConnection();

        var rowsAffected = await connection.ExecuteAsync(@"
            UPDATE LCR SET
                LCR_EST_LIN = 4,  -- Bloqueada por morosidad
                LCR_FEC_MOD = GETDATE(),
                LCR_USR_MOD = @Usuario,
                LCR_OBS_LIN = ISNULL(LCR_OBS_LIN, '') + ' [Bloqueada por morosidad: ' + @Motivo + ' - ' + CONVERT(VARCHAR, GETDATE(), 103) + ']'
            WHERE LCR_RUT_CLI = @RutCliente
              AND LCR_EST_LIN = 1",
            new { RutCliente = rutCliente, Usuario = usuario, Motivo = motivo });

        return rowsAffected > 0;
    }
}
