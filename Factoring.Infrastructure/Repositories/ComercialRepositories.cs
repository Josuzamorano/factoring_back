// ====================================================
// Implementación de Repositorios del Módulo Comercial
// Migrado de: F_CM1000.frm, F_CM1200.frm, F_CM1400.frm, 
//             F_CM1500.frm, F_CM1700.frm, F_CM1800.frm
// ====================================================

using Microsoft.Data.SqlClient;
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
    private readonly IDbConnectionFactory _connectionFactory;

    public ExcedenteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ExcedenteDto>> GetExcedentesAsync(FiltroExcedenteDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryAsync<ExcedenteDto>(sql, new
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
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryFirstOrDefaultAsync<ExcedenteDto>(sql, new { NumeroExcedente = numeroExcedente });
    }

    public async Task<IEnumerable<ResumenExcedenteDto>> GetResumenExcedentesAsync(FiltroExcedenteDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryAsync<ResumenExcedenteDto>(sql, new
        {
            filtro.RutCliente,
            filtro.FechaDesde,
            filtro.FechaHasta
        });
    }

    public async Task<IEnumerable<ExcedenteDetalleDto>> GetExcedentesDisponiblesAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryAsync<ExcedenteDetalleDto>(sql, new { RutCliente = rutCliente });
    }

    public async Task<bool> AplicarExcedenteAsync(AplicarExcedenteDto dto, string usuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            INSERT INTO DET_EXC (DET_NUM_EXC, DET_NUM_DOC, DET_MTO_APL, DET_EST_EXC, DET_FEC_APL, DET_USR_APL)
            VALUES (@NumeroExcedente, @NumeroDocumento, @MontoAplicar, 1, GETDATE(), @Usuario);
            
            Update EXC 
            SET EXC_SAL_EXC = EXC_SAL_EXC - @MontoAplicar
            WHERE EXC_NUM_EXC = @NumeroExcedente;";

        var result = await connection.ExecuteAsync(sql, new
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
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT EXC_SAL_EXC - ISNULL((
                SELECT SUM(VVE_MTO_ABO) FROM VVE 
                WHERE VVE_NUM_EXC = @NumeroExcedente 
                AND VVE_EST_ADO IN (1, 2)
            ), 0)
            FROM EXC WHERE EXC_NUM_EXC = @NumeroExcedente";

        return await connection.QueryFirstOrDefaultAsync<decimal>(sql, new { NumeroExcedente = numeroExcedente });
    }
}

/// <summary>
/// Repositorio para gestión de prórrogas
/// Migrado de: F_CM1400.frm, F_CM1800.frm
/// </summary>
public class ProrrogaRepository : IProrrogaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProrrogaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ProrrogaDto>> GetProrrogasAsync(FiltroProrrogaDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryAsync<ProrrogaDto>(sql, filtro);
    }

    public async Task<ProrrogaDto?> GetProrrogaByIdAsync(int numeroProrrroga)
    {
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryFirstOrDefaultAsync<ProrrogaDto>(sql, new { NumeroProrrroga = numeroProrrroga });
    }

    public async Task<IEnumerable<ProrrogaDto>> GetProrrogasPendientesAsync()
    {
        return await GetProrrogasAsync(new FiltroProrrogaDto { Estado = 1 });
    }

    public async Task<int> CreateProrrogaAsync(CreateProrrogaDto dto, string usuario)
    {
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryFirstAsync<int>(sql, new
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
        using var connection = _connectionFactory.CreateConnection();
        var nuevoEstado = dto.Aprobar ? 2 : 3;
        var sql = @"
            Update PRO 
            SET PRO_EST = @Estado,
                PRO_FEC_APR = GETDATE(),
                PRO_USR_APR = @Usuario,
                PRO_OBS = ISNULL(PRO_OBS, '') + ' | ' + ISNULL(@Observaciones, '')
            WHERE PRO_NUM_PRO = @NumeroProrrroga AND PRO_EST = 1";

        var result = await connection.ExecuteAsync(sql, new
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
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            Update PRO 
            SET PRO_EST = 4,
                PRO_FEC_CTB = GETDATE(),
                PRO_USR_CTB = @Usuario
            WHERE PRO_NUM_PRO = @NumeroProrrroga AND PRO_EST = 2";

        var result = await connection.ExecuteAsync(sql, new
        {
            NumeroProrrroga = numeroProrrroga,
            Usuario = usuario
        });

        return result > 0;
    }

    public async Task<DocumentoClienteDto?> GetDocumentoParaProrrogaAsync(int tipoDocumento, string numeroDocumento)
    {
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryFirstOrDefaultAsync<DocumentoClienteDto>(sql, new { NumeroDocumento = numeroDocumento });
    }
}

/// <summary>
/// Repositorio para informe de documentos por cliente
/// Migrado de: F_CM1700.frm
/// </summary>
public class DocumentoClienteRepository : IDocumentoClienteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DocumentoClienteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<DocumentoClienteDto>> GetDocumentosClienteAsync(FiltroDocumentoClienteDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryAsync<DocumentoClienteDto>(sql, filtro);
    }

    public async Task<ResumenDocumentoClienteDto> GetResumenDocumentosAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryFirstOrDefaultAsync<ResumenDocumentoClienteDto>(sql, new { RutCliente = rutCliente })
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
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryAsync<DocumentoClienteDto>(sql, new { RutCliente = rutCliente, Dias = dias });
    }
}

/// <summary>
/// Repositorio para cartola de cliente
/// Migrado de: F_GC1900.frm
/// </summary>
public class CartolaClienteRepository : ICartolaClienteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CartolaClienteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<CartolaClienteDto>> GetCartolaAsync(FiltroCartolaClienteDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryAsync<CartolaClienteDto>(sql, filtro);
    }

    public async Task<ResumenCartolaClienteDto> GetResumenCartolaAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
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

        return await connection.QueryFirstOrDefaultAsync<ResumenCartolaClienteDto>(sql, new { RutCliente = rutCliente })
            ?? new ResumenCartolaClienteDto { RutCliente = rutCliente };
    }
}

/// <summary>
/// Repositorio para gestión de renovaciones
/// Migrado de: F_CM1500.frm
/// </summary>
public class RenovacionRepository : IRenovacionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RenovacionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<RenovacionDto>> GetRenovacionesAsync(FiltroRenovacionDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT 
                r.REN_NUM_REN AS NumeroRenovacion,
                r.REN_NUM_OPE_ORI AS NumeroOperacionOriginal,
                r.REN_NUM_OPE_NUE AS NumeroOperacionNueva,
                r.REN_RUT_CLI AS RutCliente,
                CONVERT(VARCHAR, r.REN_RUT_CLI) + '-' + dbo.Fnc_DigitoRut(r.REN_RUT_CLI) AS RutClienteFormateado,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                r.REN_MTO_ORI AS MontoOriginal,
                r.REN_MTO_REN AS MontoRenovacion,
                r.REN_FEC_SOL AS FechaSolicitud,
                r.REN_FEC_APR AS FechaAprobacion,
                r.REN_EST AS Estado,
                CASE r.REN_EST 
                    WHEN 1 THEN 'Pendiente'
                    WHEN 2 THEN 'Aprobada'
                    WHEN 3 THEN 'Rechazada'
                    ELSE 'Desconocido'
                END AS EstadoDescripcion,
                r.REN_USR_SOL AS UsuarioSolicita,
                r.REN_USR_APR AS UsuarioAprueba,
                r.REN_OBS AS Observaciones
            FROM REN r
            INNER JOIN CLI c ON r.REN_RUT_CLI = c.CLI_RUT_CLI
            WHERE (@RutCliente IS NULL OR r.REN_RUT_CLI = @RutCliente)
            AND (@NumeroOperacion IS NULL OR r.REN_NUM_OPE_ORI = @NumeroOperacion)
            AND (@Estado IS NULL OR r.REN_EST = @Estado)
            AND (@FechaDesde IS NULL OR r.REN_FEC_SOL >= @FechaDesde)
            AND (@FechaHasta IS NULL OR r.REN_FEC_SOL <= @FechaHasta)
            ORDER BY r.REN_FEC_SOL DESC";

        return await connection.QueryAsync<RenovacionDto>(sql, filtro);
    }

    public async Task<RenovacionDto?> GetRenovacionByIdAsync(int numeroRenovacion)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT 
                r.REN_NUM_REN AS NumeroRenovacion,
                r.REN_NUM_OPE_ORI AS NumeroOperacionOriginal,
                r.REN_NUM_OPE_NUE AS NumeroOperacionNueva,
                r.REN_RUT_CLI AS RutCliente,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                r.REN_MTO_ORI AS MontoOriginal,
                r.REN_MTO_REN AS MontoRenovacion,
                r.REN_FEC_SOL AS FechaSolicitud,
                r.REN_EST AS Estado,
                r.REN_OBS AS Observaciones
            FROM REN r
            INNER JOIN CLI c ON r.REN_RUT_CLI = c.CLI_RUT_CLI
            WHERE r.REN_NUM_REN = @NumeroRenovacion";

        return await connection.QueryFirstOrDefaultAsync<RenovacionDto>(sql, new { NumeroRenovacion = numeroRenovacion });
    }

    public async Task<int> CreateRenovacionAsync(CreateRenovacionDto dto, string usuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            DECLARE @NumRen INT;
            SELECT @NumRen = ISNULL(MAX(REN_NUM_REN), 0) + 1 FROM REN;
            
            INSERT INTO REN (
                REN_NUM_REN, REN_NUM_OPE_ORI, REN_TIP_DOC_ORI, REN_NUM_DOC_ORI,
                REN_TIP_DOC_NUE, REN_NUM_DOC_NUE, REN_MTO_NUE, REN_FEC_VTO_NUE,
                REN_DIA_PLA, REN_FEC_SOL, REN_EST, REN_USR_SOL, REN_OBS
            )
            VALUES (
                @NumRen, @NumeroOperacionOriginal, @TipoDocumentoOriginal, @NumeroDocumentoOriginal,
                @TipoDocumentoNuevo, @NumeroDocumentoNuevo, @MontoNuevo, @FechaVencimientoNueva,
                @DiasPlazo, GETDATE(), 1, @Usuario, @Observaciones
            );
            
            SELECT @NumRen;";

        return await connection.QueryFirstAsync<int>(sql, new
        {
            dto.NumeroOperacionOriginal,
            dto.TipoDocumentoOriginal,
            dto.NumeroDocumentoOriginal,
            dto.TipoDocumentoNuevo,
            dto.NumeroDocumentoNuevo,
            dto.MontoNuevo,
            dto.FechaVencimientoNueva,
            dto.DiasPlazo,
            dto.Observaciones,
            Usuario = usuario
        });
    }

    public async Task<bool> AprobarRenovacionAsync(int numeroRenovacion, string usuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            Update REN 
            SET REN_EST = 2,
                REN_FEC_APR = GETDATE(),
                REN_USR_APR = @Usuario
            WHERE REN_NUM_REN = @NumeroRenovacion AND REN_EST = 1";

        var result = await connection.ExecuteAsync(sql, new
        {
            NumeroRenovacion = numeroRenovacion,
            Usuario = usuario
        });

        return result > 0;
    }

    public async Task<bool> RechazarRenovacionAsync(int numeroRenovacion, string usuario, string? motivo)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            Update REN 
            SET REN_EST = 3,
                REN_FEC_APR = GETDATE(),
                REN_USR_APR = @Usuario,
                REN_OBS = ISNULL(REN_OBS, '') + ' | Rechazada: ' + ISNULL(@Motivo, '')
            WHERE REN_NUM_REN = @NumeroRenovacion AND REN_EST = 1";

        var result = await connection.ExecuteAsync(sql, new
        {
            NumeroRenovacion = numeroRenovacion,
            Usuario = usuario,
            Motivo = motivo
        });

        return result > 0;
    }
}

/// <summary>
/// Repositorio para consulta de otorgamientos
/// Migrado de: F_PUBLI05.frm
/// </summary>
public class ConsultaOtorgamientoRepository : IConsultaOtorgamientoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ConsultaOtorgamientoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ConsultaOtorgamientoDto>> GetOtorgamientosAsync(FiltroOtorgamientoDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT 
                o.OPE_NUM_OPE AS NumeroOperacion,
                o.OPE_RUT_CLI AS RutCliente,
                CONVERT(VARCHAR, o.OPE_RUT_CLI) + '-' + dbo.Fnc_DigitoRut(o.OPE_RUT_CLI) AS RutClienteFormateado,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                o.OPE_FEC_OPE AS FechaOperacion,
                o.OPE_MTO_OPE AS MontoOperacion,
                o.OPE_MTO_GIR AS MontoGirado,
                o.OPE_EST_OPE AS Estado,
                CASE o.OPE_EST_OPE 
                    WHEN 1 THEN 'Ingresada'
                    WHEN 2 THEN 'Aprobada'
                    WHEN 3 THEN 'Girada'
                    WHEN 4 THEN 'Cancelada'
                    ELSE 'Desconocido'
                END AS EstadoDescripcion,
                o.OPE_TIP_OPE AS TipoOperacion,
                o.OPE_FEC_GIR AS FechaGiro
            FROM OPE o
            INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
            WHERE (@RutCliente IS NULL OR o.OPE_RUT_CLI = @RutCliente)
            AND (@NumeroOperacion IS NULL OR o.OPE_NUM_OPE = @NumeroOperacion)
            AND (@FechaDesde IS NULL OR o.OPE_FEC_OPE >= @FechaDesde)
            AND (@FechaHasta IS NULL OR o.OPE_FEC_OPE <= @FechaHasta)
            AND (@Estado IS NULL OR o.OPE_EST_OPE = @Estado)
            ORDER BY o.OPE_FEC_OPE DESC";

        return await connection.QueryAsync<ConsultaOtorgamientoDto>(sql, filtro);
    }

    public async Task<ConsultaOtorgamientoDto?> GetOtorgamientoByIdAsync(int numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT 
                o.OPE_NUM_OPE AS NumeroOperacion,
                o.OPE_RUT_CLI AS RutCliente,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                o.OPE_FEC_OPE AS FechaOperacion,
                o.OPE_MTO_OPE AS MontoOperacion,
                o.OPE_MTO_GIR AS MontoGirado,
                o.OPE_EST_OPE AS Estado,
                o.OPE_TIP_OPE AS TipoOperacion
            FROM OPE o
            INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
            WHERE o.OPE_NUM_OPE = @NumeroOperacion";

        return await connection.QueryFirstOrDefaultAsync<ConsultaOtorgamientoDto>(sql, new { NumeroOperacion = numeroOperacion });
    }
}

/// <summary>
/// Repositorio para pizarra comercial
/// Migrado de: F_PUBLI04.frm
/// </summary>
public class PizarraComercialRepository : IPizarraComercialRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PizarraComercialRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<PizarraComercialDto>> GetPizarraAsync(FiltroPizarraDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT 
                o.OPE_NUM_OPE AS NumeroOperacion,
                o.OPE_RUT_CLI AS RutCliente,
                CONVERT(VARCHAR, o.OPE_RUT_CLI) + '-' + dbo.Fnc_DigitoRut(o.OPE_RUT_CLI) AS RutClienteFormateado,
                RTRIM(c.CLI_RAZ_SOC) AS NombreCliente,
                o.OPE_FEC_OPE AS FechaOperacion,
                o.OPE_MTO_OPE AS MontoOperacion,
                o.OPE_MTO_GIR AS MontoGirado,
                o.OPE_MTO_OPE - ISNULL(o.OPE_MTO_GIR, 0) AS SaldoPendiente,
                o.OPE_EST_OPE AS Estado,
                CASE o.OPE_EST_OPE 
                    WHEN 2 THEN 'Por Girar'
                    WHEN 3 THEN 'Girado Parcial'
                    ELSE 'Otro'
                END AS EstadoDescripcion,
                e.EJE_NOM_EJE AS NombreEjecutivo,
                o.OPE_OBS AS Observaciones
            FROM OPE o
            INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN EJE e ON o.OPE_COD_EJE = e.EJE_COD_EJE
            WHERE o.OPE_EST_OPE IN (2, 3)
            AND o.OPE_MTO_OPE > ISNULL(o.OPE_MTO_GIR, 0)
            AND (@RutCliente IS NULL OR o.OPE_RUT_CLI = @RutCliente)
            AND (@FechaDesde IS NULL OR o.OPE_FEC_OPE >= @FechaDesde)
            AND (@FechaHasta IS NULL OR o.OPE_FEC_OPE <= @FechaHasta)
            AND (@CodigoEjecutivo IS NULL OR o.OPE_COD_EJE = @CodigoEjecutivo)
            ORDER BY o.OPE_FEC_OPE DESC";

        return await connection.QueryAsync<PizarraComercialDto>(sql, filtro);
    }
}

