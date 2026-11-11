// ====================================================
// Implementación de Repositorios del Módulo Comercial
// Migrado de: F_CM1000.frm, F_CM1200.frm, F_CM1400.frm, 
//             F_CM1500.frm, F_CM1700.frm, F_CM1800.frm
// ====================================================

using System.Data;
using Dapper;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;

namespace Factoring.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestión de excedentes
/// Migrado de: F_CM1200.frm
/// </summary>
public class ExcedenteRepository : IExcedenteRepository
{
    private readonly IDbConnection _connection;

    public ExcedenteRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<ExcedenteDto>> GetExcedentesAsync(FiltroExcedenteDto filtro)
    {
        // Query migrada de F_CM1200.frm Bot_Seleccionar_Click
        var sql = @"
            SELECT 
                e.EXC_NUM_EXC AS NumeroExcedente,
                e.EXC_RUT_CLI AS RutCliente,
                CONVERT(VARCHAR, e.EXC_RUT_CLI) + '-' + dbo.Fnc_DigitoRut(e.EXC_RUT_CLI) AS RutClienteFormateado,
                RTRIM(c.CLI_RAZ_SOC) + ' ' + RTRIM(ISNULL(c.CLI_APE_PAT, '')) + ' ' + RTRIM(ISNULL(c.CLI_APE_MAT, '')) AS NombreCliente,
                e.EXC_TIP_DOC AS TipoDocumento,
                ISNULL(p.PAR_COD_AUX, 'N/A') AS TipoDocumentoDescripcion,
                e.EXC_NUM_DOC AS NumeroDocumento,
                e.EXC_FEC_GEN AS FechaGeneracion,
                e.EXC_MON_EXC AS MontoExcedente,
                e.EXC_SAL_EXC - ISNULL((
                    SELECT SUM(VVE_MTO_ABO) FROM VVE 
                    WHERE VVE_NUM_EXC = e.EXC_NUM_EXC 
                    AND VVE_EST_ADO IN (1, 2)
                ), 0) AS SaldoExcedente,
                e.EXC_EST_EXC AS Estado,
                CASE e.EXC_EST_EXC 
                    WHEN 1 THEN 'Generado'
                    WHEN 2 THEN 'Preparado'
                    WHEN 5 THEN 'Retenido'
                    WHEN 6 THEN 'Rechazado'
                    ELSE 'Desconocido'
                END AS EstadoDescripcion
            FROM EXC e
            INNER JOIN CLI c ON e.EXC_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN PAR p ON p.PAR_COD_TAB = 31 AND p.PAR_COD_PAR = e.EXC_TIP_DOC
            WHERE e.EXC_EST_EXC IN (5, 6, 1)
            AND (@RutCliente IS NULL OR e.EXC_RUT_CLI = @RutCliente)
            AND (@TipoDocumento IS NULL OR e.EXC_TIP_DOC = @TipoDocumento)
            AND (@FechaDesde IS NULL OR e.EXC_FEC_GEN >= @FechaDesde)
            AND (@FechaHasta IS NULL OR e.EXC_FEC_GEN <= @FechaHasta)
            AND (@Estado IS NULL OR e.EXC_EST_EXC = @Estado)
            AND (@SoloConSaldo = 0 OR ROUND(e.EXC_SAL_EXC, 0, 1) > 0)
            ORDER BY e.EXC_FEC_GEN DESC";

        return await _connection.QueryAsync<ExcedenteDto>(sql, new
        {
            filtro.RutCliente,
            filtro.TipoDocumento,
            filtro.FechaDesde,
            filtro.FechaHasta,
            filtro.Estado,
            SoloConSaldo = filtro.SoloConSaldo ? 1 : 0
        });
    }

    public async Task<ExcedenteDto?> GetExcedenteByIdAsync(int numeroExcedente)
    {
        var sql = @"
            SELECT 
                e.EXC_NUM_EXC AS NumeroExcedente,
                e.EXC_RUT_CLI AS RutCliente,
                CONVERT(VARCHAR, e.EXC_RUT_CLI) + '-' + dbo.Fnc_DigitoRut(e.EXC_RUT_CLI) AS RutClienteFormateado,
                RTRIM(c.CLI_RAZ_SOC) + ' ' + RTRIM(ISNULL(c.CLI_APE_PAT, '')) AS NombreCliente,
                e.EXC_TIP_DOC AS TipoDocumento,
                e.EXC_NUM_DOC AS NumeroDocumento,
                e.EXC_FEC_GEN AS FechaGeneracion,
                e.EXC_MON_EXC AS MontoExcedente,
                e.EXC_SAL_EXC AS SaldoExcedente,
                e.EXC_EST_EXC AS Estado
            FROM EXC e
            INNER JOIN CLI c ON e.EXC_RUT_CLI = c.CLI_RUT_CLI
            WHERE e.EXC_NUM_EXC = @NumeroExcedente";

        return await _connection.QueryFirstOrDefaultAsync<ExcedenteDto>(sql, new { NumeroExcedente = numeroExcedente });
    }

    public async Task<IEnumerable<ResumenExcedenteDto>> GetResumenExcedentesAsync(FiltroExcedenteDto filtro)
    {
        // Query migrada de F_CM1200.frm SqlStr2
        var sql = @"
            SELECT 
                e.EXC_RUT_CLI AS RutCliente,
                CONVERT(VARCHAR, e.EXC_RUT_CLI) + '-' + dbo.Fnc_DigitoRut(e.EXC_RUT_CLI) AS RutClienteFormateado,
                RTRIM(c.CLI_RAZ_SOC) + ' ' + RTRIM(ISNULL(c.CLI_APE_PAT, '')) + ' ' + RTRIM(ISNULL(c.CLI_APE_MAT, '')) AS NombreCliente,
                SUM(e.EXC_MON_EXC) AS MontoTotalExcedentes,
                SUM(e.EXC_SAL_EXC) - ISNULL((
                    SELECT SUM(VVE_MTO_ABO) FROM VVE v2, EXC e2
                    WHERE v2.VVE_NUM_EXC = e2.EXC_NUM_EXC 
                    AND v2.VVE_EST_ADO IN (1, 2)
                    AND e2.EXC_EST_EXC IN (5, 6, 1)
                    AND ROUND(e2.EXC_SAL_EXC, 0, 1) > 0
                    AND e2.EXC_RUT_CLI = e.EXC_RUT_CLI
                ), 0) AS SaldoTotalExcedentes,
                COUNT(*) AS CantidadExcedentes
            FROM EXC e
            INNER JOIN CLI c ON e.EXC_RUT_CLI = c.CLI_RUT_CLI
            WHERE e.EXC_EST_EXC IN (5, 6)
            AND ROUND(e.EXC_SAL_EXC, 0, 1) > 0
            AND (@RutCliente IS NULL OR e.EXC_RUT_CLI = @RutCliente)
            AND (@FechaDesde IS NULL OR e.EXC_FEC_GEN >= @FechaDesde)
            AND (@FechaHasta IS NULL OR e.EXC_FEC_GEN <= @FechaHasta)
            GROUP BY e.EXC_RUT_CLI, c.CLI_RAZ_SOC, c.CLI_APE_PAT, c.CLI_APE_MAT
            ORDER BY NombreCliente";

        return await _connection.QueryAsync<ResumenExcedenteDto>(sql, new
        {
            filtro.RutCliente,
            filtro.FechaDesde,
            filtro.FechaHasta
        });
    }

    public async Task<IEnumerable<ExcedenteDetalleDto>> GetExcedentesDisponiblesAsync(long rutCliente)
    {
        // Query migrada de F_CM1200.frm Bot_Seleccionar_Click
        var sql = @"
            SELECT 
                exc.EXC_NUM_EXC AS NumeroExcedente,
                excgen.EXC_NUM_DOC AS NumeroDocumento,
                exc.EXC_FEC_GEN AS FechaGeneracion,
                excgen.EXC_SAL_INI AS MontoExcedente,
                excgen.EXC_SAL_FIN - ISNULL((
                    SELECT SUM(DET_MTO_APL) FROM DET_EXC 
                    WHERE DET_NUM_EXC = excgen.EXC_NUM_EXC 
                    AND DET_NUM_DOC = excgen.EXC_NUM_DOC 
                    AND DET_EST_EXC = 1
                ), 0) - ISNULL((
                    SELECT SUM(DET_MTO_APL) FROM TMP_DET_EXC 
                    WHERE DET_NUM_EXC = excgen.EXC_NUM_EXC 
                    AND DET_NUM_DOC = excgen.EXC_NUM_DOC
                ), 0) AS SaldoExcedente,
                0 AS Seleccionado
            FROM EXC exc
            INNER JOIN EXCGEN excgen ON exc.EXC_NUM_EXC = excgen.EXC_NUM_EXC
            WHERE exc.EXC_RUT_CLI = @RutCliente
            AND exc.EXC_EST_EXC IN (5, 6)
            AND exc.EXC_SAL_EXC > 0
            AND exc.EXC_NUM_EXC IN (
                SELECT FAO_NUM_EXC FROM FAO WHERE FAO_NUM_OPE IN (
                    SELECT OPE_NUM_OPE FROM OPE WHERE OPE_RUT_CLI = @RutCliente
                )
                UNION ALL
                SELECT CHO_NUM_EXC FROM CHO WHERE CHO_NUM_OPE IN (
                    SELECT OPE_NUM_OPE FROM OPE WHERE OPE_RUT_CLI = @RutCliente
                )
                UNION ALL
                SELECT LEO_NUM_EXC FROM LEO WHERE LEO_NUM_OPE IN (
                    SELECT OPE_NUM_OPE FROM OPE WHERE OPE_RUT_CLI = @RutCliente
                )
                UNION ALL
                SELECT OTO_NUM_EXC FROM OTO WHERE OTO_NUM_OPE IN (
                    SELECT OPE_NUM_OPE FROM OPE WHERE OPE_RUT_CLI = @RutCliente
                )
            )
            ORDER BY exc.EXC_FEC_GEN";

        return await _connection.QueryAsync<ExcedenteDetalleDto>(sql, new { RutCliente = rutCliente });
    }

    public async Task<bool> AplicarExcedenteAsync(AplicarExcedenteDto dto, string usuario)
    {
        var sql = @"
            INSERT INTO DET_EXC (DET_NUM_EXC, DET_NUM_DOC, DET_MTO_APL, DET_EST_EXC, DET_FEC_APL, DET_USR_APL)
            VALUES (@NumeroExcedente, @NumeroDocumento, @MontoAplicar, 1, GETDATE(), @Usuario);
            
            Update EXC 
            SET EXC_SAL_EXC = EXC_SAL_EXC - @MontoAplicar
            WHERE EXC_NUM_EXC = @NumeroExcedente;";

        var result = await _connection.ExecuteAsync(sql, new
        {
            dto.NumeroExcedente,
            dto.NumeroDocumento,
            dto.MontoAplicar,
            Usuario = usuario
        });

        return result > 0;
    }

    public async Task<decimal> GetSaldoExcedenteAsync(int numeroExcedente)
    {
        var sql = @"
            SELECT EXC_SAL_EXC - ISNULL((
                SELECT SUM(VVE_MTO_ABO) FROM VVE 
                WHERE VVE_NUM_EXC = @NumeroExcedente 
                AND VVE_EST_ADO IN (1, 2)
            ), 0)
            FROM EXC WHERE EXC_NUM_EXC = @NumeroExcedente";

        return await _connection.QueryFirstOrDefaultAsync<decimal>(sql, new { NumeroExcedente = numeroExcedente });
    }
}

/// <summary>
/// Repositorio para gestión de prórrogas
/// Migrado de: F_CM1400.frm, F_CM1800.frm
/// </summary>
public class ProrrogaRepository : IProrrogaRepository
{
    private readonly IDbConnection _connection;

    public ProrrogaRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<ProrrogaDto>> GetProrrogasAsync(FiltroProrrogaDto filtro)
    {
        var sql = @"
            SELECT 
                p.PRO_NUM_PRO AS NumeroProrrroga,
                p.PRO_TIP_DOC AS TipoDocumento,
                CASE p.PRO_TIP_DOC 
                    WHEN 1 THEN 'Factura'
                    WHEN 2 THEN 'Cheque'
                    WHEN 3 THEN 'Letra'
                    ELSE 'Otro'
                END AS TipoDocumentoDescripcion,
                p.PRO_NUM_DOC AS NumeroDocumento,
                p.PRO_NUM_OPE AS NumeroOperacion,
                p.PRO_RUT_CLI AS RutCliente,
                CONVERT(VARCHAR, p.PRO_RUT_CLI) + '-' + dbo.Fnc_DigitoRut(p.PRO_RUT_CLI) AS RutClienteFormateado,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                p.PRO_RUT_DEU AS RutDeudor,
                CONVERT(VARCHAR, p.PRO_RUT_DEU) + '-' + dbo.Fnc_DigitoRut(p.PRO_RUT_DEU) AS RutDeudorFormateado,
                RTRIM(d.DEU_RAZ_SOC) AS NombreDeudor,
                p.PRO_FEC_VTO_ORI AS FechaVencimientoOriginal,
                p.PRO_FEC_VTO_NUE AS FechaVencimientoNueva,
                p.PRO_DIA_PRO AS DiasProrrroga,
                p.PRO_POR_PRO AS PorcentajeProrrroga,
                p.PRO_MTO_ABO AS MontoAbono,
                p.PRO_TIP_ABO AS TipoAbono,
                p.PRO_OBS AS Observaciones,
                p.PRO_EST AS Estado,
                CASE p.PRO_EST 
                    WHEN 1 THEN 'Pendiente'
                    WHEN 2 THEN 'Aprobada'
                    WHEN 3 THEN 'Rechazada'
                    WHEN 4 THEN 'Contabilizada'
                    ELSE 'Desconocido'
                END AS EstadoDescripcion,
                p.PRO_FEC_SOL AS FechaSolicitud,
                p.PRO_FEC_APR AS FechaAprobacion,
                p.PRO_USR_SOL AS UsuarioSolicita,
                p.PRO_USR_APR AS UsuarioAprueba
            FROM PRO p
            INNER JOIN CLI c ON p.PRO_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON p.PRO_RUT_DEU = d.DEU_RUT_DEU
            WHERE (@RutCliente IS NULL OR p.PRO_RUT_CLI = @RutCliente)
            AND (@RutDeudor IS NULL OR p.PRO_RUT_DEU = @RutDeudor)
            AND (@TipoDocumento IS NULL OR p.PRO_TIP_DOC = @TipoDocumento)
            AND (@Estado IS NULL OR p.PRO_EST = @Estado)
            AND (@FechaDesde IS NULL OR p.PRO_FEC_SOL >= @FechaDesde)
            AND (@FechaHasta IS NULL OR p.PRO_FEC_SOL <= @FechaHasta)
            ORDER BY p.PRO_FEC_SOL DESC";

        return await _connection.QueryAsync<ProrrogaDto>(sql, filtro);
    }

    public async Task<ProrrogaDto?> GetProrrogaByIdAsync(int numeroProrrroga)
    {
        var sql = @"
            SELECT 
                p.PRO_NUM_PRO AS NumeroProrrroga,
                p.PRO_TIP_DOC AS TipoDocumento,
                p.PRO_NUM_DOC AS NumeroDocumento,
                p.PRO_NUM_OPE AS NumeroOperacion,
                p.PRO_RUT_CLI AS RutCliente,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                p.PRO_RUT_DEU AS RutDeudor,
                RTRIM(d.DEU_RAZ_SOC) AS NombreDeudor,
                p.PRO_FEC_VTO_ORI AS FechaVencimientoOriginal,
                p.PRO_FEC_VTO_NUE AS FechaVencimientoNueva,
                p.PRO_DIA_PRO AS DiasProrrroga,
                p.PRO_POR_PRO AS PorcentajeProrrroga,
                p.PRO_MTO_ABO AS MontoAbono,
                p.PRO_TIP_ABO AS TipoAbono,
                p.PRO_OBS AS Observaciones,
                p.PRO_EST AS Estado
            FROM PRO p
            INNER JOIN CLI c ON p.PRO_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON p.PRO_RUT_DEU = d.DEU_RUT_DEU
            WHERE p.PRO_NUM_PRO = @NumeroProrrroga";

        return await _connection.QueryFirstOrDefaultAsync<ProrrogaDto>(sql, new { NumeroProrrroga = numeroProrrroga });
    }

    public async Task<IEnumerable<ProrrogaDto>> GetProrrogasPendientesAsync()
    {
        return await GetProrrogasAsync(new FiltroProrrogaDto { Estado = 1 });
    }

    public async Task<int> CreateProrrogaAsync(CreateProrrogaDto dto, string usuario)
    {
        var sql = @"
            DECLARE @NumPro INT;
            SELECT @NumPro = ISNULL(MAX(PRO_NUM_PRO), 0) + 1 FROM PRO;
            
            INSERT INTO PRO (
                PRO_NUM_PRO, PRO_TIP_DOC, PRO_NUM_DOC, PRO_NUM_OPE,
                PRO_FEC_VTO_NUE, PRO_DIA_PRO, PRO_POR_PRO, PRO_MTO_ABO,
                PRO_TIP_ABO, PRO_OBS, PRO_EST, PRO_FEC_SOL, PRO_USR_SOL
            )
            SELECT 
                @NumPro, @TipoDocumento, @NumeroDocumento, @NumeroOperacion,
                @FechaVencimientoNueva, @DiasProrrroga, @PorcentajeProrrroga, @MontoAbono,
                @TipoAbono, @Observaciones, 1, GETDATE(), @Usuario;
            
            SELECT @NumPro;";

        return await _connection.QueryFirstAsync<int>(sql, new
        {
            dto.TipoDocumento,
            dto.NumeroDocumento,
            dto.NumeroOperacion,
            dto.FechaVencimientoNueva,
            dto.DiasProrrroga,
            dto.PorcentajeProrrroga,
            dto.MontoAbono,
            dto.TipoAbono,
            dto.Observaciones,
            Usuario = usuario
        });
    }

    public async Task<bool> ProcesarProrrogaAsync(AccionProrrogaDto dto, string usuario)
    {
        var nuevoEstado = dto.Aprobar ? 2 : 3;
        var sql = @"
            Update PRO 
            SET PRO_EST = @Estado,
                PRO_FEC_APR = GETDATE(),
                PRO_USR_APR = @Usuario,
                PRO_OBS = ISNULL(PRO_OBS, '') + ' | ' + ISNULL(@Observaciones, '')
            WHERE PRO_NUM_PRO = @NumeroProrrroga AND PRO_EST = 1";

        var result = await _connection.ExecuteAsync(sql, new
        {
            Estado = nuevoEstado,
            Usuario = usuario,
            dto.Observaciones,
            dto.NumeroProrrroga
        });

        return result > 0;
    }

    public async Task<bool> ContabilizarProrrogaAsync(int numeroProrrroga, string usuario)
    {
        var sql = @"
            Update PRO 
            SET PRO_EST = 4,
                PRO_FEC_CTB = GETDATE(),
                PRO_USR_CTB = @Usuario
            WHERE PRO_NUM_PRO = @NumeroProrrroga AND PRO_EST = 2";

        var result = await _connection.ExecuteAsync(sql, new
        {
            NumeroProrrroga = numeroProrrroga,
            Usuario = usuario
        });

        return result > 0;
    }

    public async Task<DocumentoClienteDto?> GetDocumentoParaProrrogaAsync(int tipoDocumento, string numeroDocumento)
    {
        var tableName = tipoDocumento switch
        {
            1 => "FAO",
            2 => "CHO",
            3 => "LEO",
            _ => "OTO"
        };
        var prefix = tipoDocumento switch
        {
            1 => "FAO",
            2 => "CHO",
            3 => "LEO",
            _ => "OTO"
        };

        var sql = $@"
            SELECT 
                d.{prefix}_NUM_OPE AS NumeroOperacion,
                {tipoDocumento} AS TipoDocumento,
                d.{prefix}_NUM_DOC AS NumeroDocumento,
                o.OPE_RUT_CLI AS RutCliente,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                d.{prefix}_RUT_DEU AS RutDeudor,
                RTRIM(de.DEU_RAZ_SOC) AS NombreDeudor,
                d.{prefix}_MTO_DOC AS MontoDocumento,
                d.{prefix}_SAL_DOC AS SaldoDocumento,
                o.OPE_FEC_OPE AS FechaOperacion,
                d.{prefix}_FEC_VTO AS FechaVencimiento,
                DATEDIFF(DAY, d.{prefix}_FEC_VTO, GETDATE()) AS DiasVencido,
                d.{prefix}_EST_DOC AS Estado
            FROM {tableName} d
            INNER JOIN OPE o ON d.{prefix}_NUM_OPE = o.OPE_NUM_OPE
            INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU de ON d.{prefix}_RUT_DEU = de.DEU_RUT_DEU
            WHERE d.{prefix}_NUM_DOC = @NumeroDocumento";

        return await _connection.QueryFirstOrDefaultAsync<DocumentoClienteDto>(sql, new { NumeroDocumento = numeroDocumento });
    }
}

/// <summary>
/// Repositorio para informe de documentos por cliente
/// Migrado de: F_CM1700.frm
/// </summary>
public class DocumentoClienteRepository : IDocumentoClienteRepository
{
    private readonly IDbConnection _connection;

    public DocumentoClienteRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<DocumentoClienteDto>> GetDocumentosClienteAsync(FiltroDocumentoClienteDto filtro)
    {
        var sql = @"
            -- Facturas
            SELECT 
                f.FAO_NUM_OPE AS NumeroOperacion,
                1 AS TipoDocumento,
                'Factura' AS TipoDocumentoDescripcion,
                f.FAO_NUM_DOC AS NumeroDocumento,
                o.OPE_RUT_CLI AS RutCliente,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                f.FAO_RUT_DEU AS RutDeudor,
                RTRIM(d.DEU_RAZ_SOC) AS NombreDeudor,
                f.FAO_MTO_DOC AS MontoDocumento,
                f.FAO_SAL_DOC AS SaldoDocumento,
                o.OPE_FEC_OPE AS FechaOperacion,
                f.FAO_FEC_VTO AS FechaVencimiento,
                DATEDIFF(DAY, f.FAO_FEC_VTO, GETDATE()) AS DiasVencido,
                f.FAO_EST_DOC AS Estado,
                CASE f.FAO_EST_DOC 
                    WHEN 1 THEN 'Vigente'
                    WHEN 2 THEN 'Pagado'
                    WHEN 3 THEN 'Protestado'
                    ELSE 'Otro'
                END AS EstadoDescripcion
            FROM FAO f
            INNER JOIN OPE o ON f.FAO_NUM_OPE = o.OPE_NUM_OPE
            INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON f.FAO_RUT_DEU = d.DEU_RUT_DEU
            WHERE o.OPE_RUT_CLI = @RutCliente
            AND (@TipoDocumento IS NULL OR @TipoDocumento = 1)
            AND (@FechaOperacionDesde IS NULL OR o.OPE_FEC_OPE >= @FechaOperacionDesde)
            AND (@FechaOperacionHasta IS NULL OR o.OPE_FEC_OPE <= @FechaOperacionHasta)
            AND (@Estado IS NULL OR f.FAO_EST_DOC = @Estado)
            AND (@SoloVencidos IS NULL OR @SoloVencidos = 0 OR f.FAO_FEC_VTO < GETDATE())
            
            UNION ALL
            
            -- Cheques
            SELECT 
                ch.CHO_NUM_OPE AS NumeroOperacion,
                2 AS TipoDocumento,
                'Cheque' AS TipoDocumentoDescripcion,
                ch.CHO_NUM_DOC AS NumeroDocumento,
                o.OPE_RUT_CLI AS RutCliente,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                ch.CHO_RUT_DEU AS RutDeudor,
                RTRIM(d.DEU_RAZ_SOC) AS NombreDeudor,
                ch.CHO_MTO_DOC AS MontoDocumento,
                ch.CHO_SAL_DOC AS SaldoDocumento,
                o.OPE_FEC_OPE AS FechaOperacion,
                ch.CHO_FEC_VTO AS FechaVencimiento,
                DATEDIFF(DAY, ch.CHO_FEC_VTO, GETDATE()) AS DiasVencido,
                ch.CHO_EST_DOC AS Estado,
                CASE ch.CHO_EST_DOC 
                    WHEN 1 THEN 'Vigente'
                    WHEN 2 THEN 'Pagado'
                    WHEN 3 THEN 'Protestado'
                    ELSE 'Otro'
                END AS EstadoDescripcion
            FROM CHO ch
            INNER JOIN OPE o ON ch.CHO_NUM_OPE = o.OPE_NUM_OPE
            INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON ch.CHO_RUT_DEU = d.DEU_RUT_DEU
            WHERE o.OPE_RUT_CLI = @RutCliente
            AND (@TipoDocumento IS NULL OR @TipoDocumento = 2)
            AND (@FechaOperacionDesde IS NULL OR o.OPE_FEC_OPE >= @FechaOperacionDesde)
            AND (@FechaOperacionHasta IS NULL OR o.OPE_FEC_OPE <= @FechaOperacionHasta)
            AND (@Estado IS NULL OR ch.CHO_EST_DOC = @Estado)
            AND (@SoloVencidos IS NULL OR @SoloVencidos = 0 OR ch.CHO_FEC_VTO < GETDATE())
            
            UNION ALL
            
            -- Letras
            SELECT 
                l.LEO_NUM_OPE AS NumeroOperacion,
                3 AS TipoDocumento,
                'Letra' AS TipoDocumentoDescripcion,
                l.LEO_NUM_DOC AS NumeroDocumento,
                o.OPE_RUT_CLI AS RutCliente,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                l.LEO_RUT_DEU AS RutDeudor,
                RTRIM(d.DEU_RAZ_SOC) AS NombreDeudor,
                l.LEO_MTO_DOC AS MontoDocumento,
                l.LEO_SAL_DOC AS SaldoDocumento,
                o.OPE_FEC_OPE AS FechaOperacion,
                l.LEO_FEC_VTO AS FechaVencimiento,
                DATEDIFF(DAY, l.LEO_FEC_VTO, GETDATE()) AS DiasVencido,
                l.LEO_EST_DOC AS Estado,
                CASE l.LEO_EST_DOC 
                    WHEN 1 THEN 'Vigente'
                    WHEN 2 THEN 'Pagado'
                    WHEN 3 THEN 'Protestado'
                    ELSE 'Otro'
                END AS EstadoDescripcion
            FROM LEO l
            INNER JOIN OPE o ON l.LEO_NUM_OPE = o.OPE_NUM_OPE
            INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON l.LEO_RUT_DEU = d.DEU_RUT_DEU
            WHERE o.OPE_RUT_CLI = @RutCliente
            AND (@TipoDocumento IS NULL OR @TipoDocumento = 3)
            AND (@FechaOperacionDesde IS NULL OR o.OPE_FEC_OPE >= @FechaOperacionDesde)
            AND (@FechaOperacionHasta IS NULL OR o.OPE_FEC_OPE <= @FechaOperacionHasta)
            AND (@Estado IS NULL OR l.LEO_EST_DOC = @Estado)
            AND (@SoloVencidos IS NULL OR @SoloVencidos = 0 OR l.LEO_FEC_VTO < GETDATE())
            
            ORDER BY FechaVencimiento DESC";

        return await _connection.QueryAsync<DocumentoClienteDto>(sql, filtro);
    }

    public async Task<ResumenDocumentoClienteDto> GetResumenDocumentosAsync(long rutCliente)
    {
        var sql = @"
            SELECT 
                @RutCliente AS RutCliente,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                (SELECT COUNT(*) FROM VW_DOCUMENTOS_OPERACION WHERE RutCliente = @RutCliente) AS TotalDocumentos,
                (SELECT ISNULL(SUM(MontoDocumento), 0) FROM VW_DOCUMENTOS_OPERACION WHERE RutCliente = @RutCliente) AS MontoTotal,
                (SELECT ISNULL(SUM(SaldoDocumento), 0) FROM VW_DOCUMENTOS_OPERACION WHERE RutCliente = @RutCliente) AS SaldoTotal,
                (SELECT COUNT(*) FROM VW_DOCUMENTOS_OPERACION WHERE RutCliente = @RutCliente AND FechaVencimiento < GETDATE()) AS DocumentosVencidos,
                (SELECT ISNULL(SUM(SaldoDocumento), 0) FROM VW_DOCUMENTOS_OPERACION WHERE RutCliente = @RutCliente AND FechaVencimiento < GETDATE()) AS MontoVencido,
                (SELECT COUNT(*) FROM VW_DOCUMENTOS_OPERACION WHERE RutCliente = @RutCliente AND FechaVencimiento >= GETDATE()) AS DocumentosPorVencer,
                (SELECT ISNULL(SUM(SaldoDocumento), 0) FROM VW_DOCUMENTOS_OPERACION WHERE RutCliente = @RutCliente AND FechaVencimiento >= GETDATE()) AS MontoPorVencer
            FROM CLI c
            WHERE c.CLI_RUT_CLI = @RutCliente";

        return await _connection.QueryFirstOrDefaultAsync<ResumenDocumentoClienteDto>(sql, new { RutCliente = rutCliente })
            ?? new ResumenDocumentoClienteDto { RutCliente = rutCliente };
    }

    public async Task<IEnumerable<DocumentoClienteDto>> GetDocumentosVencidosAsync(long rutCliente)
    {
        return await GetDocumentosClienteAsync(new FiltroDocumentoClienteDto
        {
            RutCliente = rutCliente,
            SoloVencidos = true
        });
    }

    public async Task<IEnumerable<DocumentoClienteDto>> GetDocumentosPorVencerAsync(long rutCliente, int dias)
    {
        var sql = @"
            SELECT * FROM (
                -- Query similar a GetDocumentosClienteAsync pero con filtro de días
                SELECT 
                    f.FAO_NUM_OPE AS NumeroOperacion,
                    1 AS TipoDocumento,
                    'Factura' AS TipoDocumentoDescripcion,
                    f.FAO_NUM_DOC AS NumeroDocumento,
                    o.OPE_RUT_CLI AS RutCliente,
                    RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                    f.FAO_RUT_DEU AS RutDeudor,
                    RTRIM(d.DEU_RAZ_SOC) AS NombreDeudor,
                    f.FAO_MTO_DOC AS MontoDocumento,
                    f.FAO_SAL_DOC AS SaldoDocumento,
                    o.OPE_FEC_OPE AS FechaOperacion,
                    f.FAO_FEC_VTO AS FechaVencimiento,
                    DATEDIFF(DAY, GETDATE(), f.FAO_FEC_VTO) AS DiasVencido,
                    f.FAO_EST_DOC AS Estado,
                    'Vigente' AS EstadoDescripcion
                FROM FAO f
                INNER JOIN OPE o ON f.FAO_NUM_OPE = o.OPE_NUM_OPE
                INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
                LEFT JOIN DEU d ON f.FAO_RUT_DEU = d.DEU_RUT_DEU
                WHERE o.OPE_RUT_CLI = @RutCliente
                AND f.FAO_FEC_VTO >= GETDATE()
                AND f.FAO_FEC_VTO <= DATEADD(DAY, @Dias, GETDATE())
                AND f.FAO_EST_DOC = 1
                
                UNION ALL
                
                SELECT 
                    l.LEO_NUM_OPE, 3, 'Letra', l.LEO_NUM_DOC,
                    o.OPE_RUT_CLI, RTRIM(c.CLI_RAZ_SOC),
                    l.LEO_RUT_DEU, RTRIM(d.DEU_RAZ_SOC),
                    l.LEO_MTO_DOC, l.LEO_SAL_DOC,
                    o.OPE_FEC_OPE, l.LEO_FEC_VTO,
                    DATEDIFF(DAY, GETDATE(), l.LEO_FEC_VTO),
                    l.LEO_EST_DOC, 'Vigente'
                FROM LEO l
                INNER JOIN OPE o ON l.LEO_NUM_OPE = o.OPE_NUM_OPE
                INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
                LEFT JOIN DEU d ON l.LEO_RUT_DEU = d.DEU_RUT_DEU
                WHERE o.OPE_RUT_CLI = @RutCliente
                AND l.LEO_FEC_VTO >= GETDATE()
                AND l.LEO_FEC_VTO <= DATEADD(DAY, @Dias, GETDATE())
                AND l.LEO_EST_DOC = 1
            ) docs
            ORDER BY FechaVencimiento";

        return await _connection.QueryAsync<DocumentoClienteDto>(sql, new { RutCliente = rutCliente, Dias = dias });
    }
}

/// <summary>
/// Repositorio para cartola de cliente
/// Migrado de: F_GC1900.frm
/// </summary>
public class CartolaClienteRepository : ICartolaClienteRepository
{
    private readonly IDbConnection _connection;

    public CartolaClienteRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<CartolaClienteDto>> GetCartolaAsync(FiltroCartolaClienteDto filtro)
    {
        var sql = @"
            SELECT 
                @RutCliente AS RutCliente,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                m.Fecha,
                m.TipoMovimiento,
                CASE m.TipoMovimiento
                    WHEN 'OPE' THEN 'Operación'
                    WHEN 'PAG' THEN 'Pago'
                    WHEN 'EXC' THEN 'Excedente'
                    WHEN 'PRO' THEN 'Prórroga'
                    WHEN 'GIR' THEN 'Giro'
                    ELSE 'Otro'
                END AS TipoMovimientoDescripcion,
                m.Descripcion,
                m.Referencia,
                m.Cargo,
                m.Abono,
                SUM(m.Abono - m.Cargo) OVER (ORDER BY m.Fecha, m.Referencia) AS Saldo
            FROM CLI c
            CROSS APPLY (
                -- Operaciones (Cargos)
                SELECT 
                    OPE_FEC_OPE AS Fecha,
                    'OPE' AS TipoMovimiento,
                    'Operación N° ' + CAST(OPE_NUM_OPE AS VARCHAR) AS Descripcion,
                    CAST(OPE_NUM_OPE AS VARCHAR) AS Referencia,
                    OPE_MTO_OPE AS Cargo,
                    0 AS Abono
                FROM OPE
                WHERE OPE_RUT_CLI = @RutCliente
                AND (@FechaDesde IS NULL OR OPE_FEC_OPE >= @FechaDesde)
                AND (@FechaHasta IS NULL OR OPE_FEC_OPE <= @FechaHasta)
                
                UNION ALL
                
                -- Pagos (Abonos)
                SELECT 
                    PAG_FEC_PAG AS Fecha,
                    'PAG' AS TipoMovimiento,
                    'Pago documento' AS Descripcion,
                    CAST(PAG_NUM_DOC AS VARCHAR) AS Referencia,
                    0 AS Cargo,
                    PAG_MTO_PAG AS Abono
                FROM PAG
                WHERE PAG_RUT_CLI = @RutCliente
                AND (@FechaDesde IS NULL OR PAG_FEC_PAG >= @FechaDesde)
                AND (@FechaHasta IS NULL OR PAG_FEC_PAG <= @FechaHasta)
                
                UNION ALL
                
                -- Excedentes (Abonos)
                SELECT 
                    EXC_FEC_GEN AS Fecha,
                    'EXC' AS TipoMovimiento,
                    'Excedente N° ' + CAST(EXC_NUM_EXC AS VARCHAR) AS Descripcion,
                    CAST(EXC_NUM_EXC AS VARCHAR) AS Referencia,
                    0 AS Cargo,
                    EXC_MON_EXC AS Abono
                FROM EXC
                WHERE EXC_RUT_CLI = @RutCliente
                AND (@FechaDesde IS NULL OR EXC_FEC_GEN >= @FechaDesde)
                AND (@FechaHasta IS NULL OR EXC_FEC_GEN <= @FechaHasta)
            ) m
            WHERE c.CLI_RUT_CLI = @RutCliente
            AND (@TipoMovimiento IS NULL OR m.TipoMovimiento = @TipoMovimiento)
            ORDER BY m.Fecha, m.Referencia";

        return await _connection.QueryAsync<CartolaClienteDto>(sql, filtro);
    }

    public async Task<ResumenCartolaClienteDto> GetResumenCartolaAsync(long rutCliente)
    {
        var sql = @"
            SELECT 
                @RutCliente AS RutCliente,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                ISNULL((SELECT SUM(OPE_MTO_OPE) FROM OPE WHERE OPE_RUT_CLI = @RutCliente), 0) AS TotalCargos,
                ISNULL((SELECT SUM(PAG_MTO_PAG) FROM PAG WHERE PAG_RUT_CLI = @RutCliente), 0) +
                ISNULL((SELECT SUM(EXC_MON_EXC) FROM EXC WHERE EXC_RUT_CLI = @RutCliente), 0) AS TotalAbonos,
                ISNULL((SELECT SUM(OPE_MTO_OPE) FROM OPE WHERE OPE_RUT_CLI = @RutCliente), 0) -
                ISNULL((SELECT SUM(PAG_MTO_PAG) FROM PAG WHERE PAG_RUT_CLI = @RutCliente), 0) -
                ISNULL((SELECT SUM(EXC_MON_EXC) FROM EXC WHERE EXC_RUT_CLI = @RutCliente), 0) AS SaldoActual,
                (SELECT COUNT(*) FROM OPE WHERE OPE_RUT_CLI = @RutCliente) +
                (SELECT COUNT(*) FROM PAG WHERE PAG_RUT_CLI = @RutCliente) +
                (SELECT COUNT(*) FROM EXC WHERE EXC_RUT_CLI = @RutCliente) AS CantidadMovimientos
            FROM CLI c
            WHERE c.CLI_RUT_CLI = @RutCliente";

        return await _connection.QueryFirstOrDefaultAsync<ResumenCartolaClienteDto>(sql, new { RutCliente = rutCliente })
            ?? new ResumenCartolaClienteDto { RutCliente = rutCliente };
    }
}
