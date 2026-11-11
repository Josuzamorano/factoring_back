using Dapper;
using Factoring.Core.Entities;
using Factoring.Core.DTOs;
using Factoring.Core.Interfaces;
using Factoring.Infrastructure.Data;

namespace Factoring.Infrastructure.Repositories;

/// <summary>
/// Repositorio de Gestión de Cobranza - Migrado de F_GC1100.frm
/// </summary>
public class GestionCobranzaRepository : IGestionCobranzaRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public GestionCobranzaRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<GestionCobranza>> GetByDocumentoAsync(long rutCliente, long rutDeudor, int numeroOperacion, int tipoDocumento, long numeroDocumento)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT g.GES_COR_GES AS Correlativo, g.GES_RUT_CLI AS RutCliente, g.GES_RUT_DEU AS RutDeudor,
                   g.GES_TIP_DOC AS TipoDocumento, g.GES_NUM_FAC AS NumeroDocumento, g.GES_NUM_OPE AS NumeroOperacion,
                   g.GES_COB_TEL AS CodigoCobrador, g.GES_FEC_GES AS FechaGestion, g.GES_HOR_GES AS HoraGestion,
                   g.GES_EST_COB AS EstadoCobranza, g.GES_FEC_PGO AS FechaPago, g.GES_HOR_PGO AS HoraPago,
                   g.GES_DOC_NEC AS DocumentoNecesario, g.GES_OBS_GES AS Observacion,
                   g.GES_FEC_PRX AS FechaProximaGestion, g.GES_HOR_PRX AS HoraProximaGestion,
                   g.GES_REC_GES AS CodigoRecaudador,
                   c.CLI_RAZ_SOC AS NombreCliente,
                   d.DEU_RAZ_SOC AS NombreDeudor,
                   cob.COB_NOM_COB AS NombreCobrador,
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 32 AND PAR_COD_PAR = g.GES_EST_COB) AS DescripcionEstadoCobranza,
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 5 AND PAR_COD_PAR = g.GES_TIP_DOC) AS DescripcionTipoDocumento
            FROM GES g
            LEFT JOIN CLI c ON g.GES_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON g.GES_RUT_CLI = d.DEU_RUT_CLI AND g.GES_RUT_DEU = d.DEU_RUT_DEU
            LEFT JOIN COB cob ON g.GES_COB_TEL = cob.COB_COD_COB
            WHERE g.GES_RUT_CLI = @RutCliente
              AND g.GES_RUT_DEU = @RutDeudor
              AND g.GES_NUM_OPE = @NumeroOperacion
              AND g.GES_TIP_DOC = @TipoDocumento
              AND g.GES_NUM_FAC = @NumeroDocumento
            ORDER BY g.GES_FEC_GES DESC, g.GES_COR_GES DESC";
        return await connection.QueryAsync<GestionCobranza>(sql, new { RutCliente = rutCliente, RutDeudor = rutDeudor, NumeroOperacion = numeroOperacion, TipoDocumento = tipoDocumento, NumeroDocumento = numeroDocumento });
    }

    public async Task<IEnumerable<GestionCobranza>> GetByFiltroAsync(FiltroGestionCobranzaDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT TOP 500 g.GES_COR_GES AS Correlativo, g.GES_RUT_CLI AS RutCliente, g.GES_RUT_DEU AS RutDeudor,
                   g.GES_TIP_DOC AS TipoDocumento, g.GES_NUM_FAC AS NumeroDocumento, g.GES_NUM_OPE AS NumeroOperacion,
                   g.GES_COB_TEL AS CodigoCobrador, g.GES_FEC_GES AS FechaGestion, g.GES_HOR_GES AS HoraGestion,
                   g.GES_EST_COB AS EstadoCobranza, g.GES_FEC_PGO AS FechaPago, g.GES_HOR_PGO AS HoraPago,
                   g.GES_DOC_NEC AS DocumentoNecesario, g.GES_OBS_GES AS Observacion,
                   g.GES_FEC_PRX AS FechaProximaGestion, g.GES_HOR_PRX AS HoraProximaGestion,
                   g.GES_REC_GES AS CodigoRecaudador,
                   c.CLI_RAZ_SOC AS NombreCliente,
                   d.DEU_RAZ_SOC AS NombreDeudor,
                   cob.COB_NOM_COB AS NombreCobrador,
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 32 AND PAR_COD_PAR = g.GES_EST_COB) AS DescripcionEstadoCobranza,
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 5 AND PAR_COD_PAR = g.GES_TIP_DOC) AS DescripcionTipoDocumento
            FROM GES g
            LEFT JOIN CLI c ON g.GES_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON g.GES_RUT_CLI = d.DEU_RUT_CLI AND g.GES_RUT_DEU = d.DEU_RUT_DEU
            LEFT JOIN COB cob ON g.GES_COB_TEL = cob.COB_COD_COB
            WHERE 1=1";

        if (!string.IsNullOrEmpty(filtro.CodigoCobrador))
            sql += " AND g.GES_COB_TEL = @CodigoCobrador";
        if (filtro.RutCliente.HasValue)
            sql += " AND g.GES_RUT_CLI = @RutCliente";
        if (filtro.RutDeudor.HasValue)
            sql += " AND g.GES_RUT_DEU = @RutDeudor";
        if (filtro.FechaDesde.HasValue)
            sql += " AND g.GES_FEC_GES >= @FechaDesde";
        if (filtro.FechaHasta.HasValue)
            sql += " AND g.GES_FEC_GES <= @FechaHasta";
        if (filtro.EstadoCobranza.HasValue)
            sql += " AND g.GES_EST_COB = @EstadoCobranza";
        if (filtro.TipoDocumento.HasValue)
            sql += " AND g.GES_TIP_DOC = @TipoDocumento";

        sql += " ORDER BY g.GES_FEC_GES DESC, g.GES_COR_GES DESC";

        return await connection.QueryAsync<GestionCobranza>(sql, filtro);
    }

    public async Task<GestionCobranza?> GetUltimaGestionAsync(long rutCliente, long rutDeudor, int numeroOperacion, int tipoDocumento, long numeroDocumento)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT TOP 1 g.GES_COR_GES AS Correlativo, g.GES_RUT_CLI AS RutCliente, g.GES_RUT_DEU AS RutDeudor,
                   g.GES_TIP_DOC AS TipoDocumento, g.GES_NUM_FAC AS NumeroDocumento, g.GES_NUM_OPE AS NumeroOperacion,
                   g.GES_COB_TEL AS CodigoCobrador, g.GES_FEC_GES AS FechaGestion, g.GES_HOR_GES AS HoraGestion,
                   g.GES_EST_COB AS EstadoCobranza, g.GES_FEC_PGO AS FechaPago, g.GES_HOR_PGO AS HoraPago,
                   g.GES_DOC_NEC AS DocumentoNecesario, g.GES_OBS_GES AS Observacion,
                   g.GES_FEC_PRX AS FechaProximaGestion, g.GES_HOR_PRX AS HoraProximaGestion,
                   g.GES_REC_GES AS CodigoRecaudador
            FROM GES g
            WHERE g.GES_RUT_CLI = @RutCliente
              AND g.GES_RUT_DEU = @RutDeudor
              AND g.GES_NUM_OPE = @NumeroOperacion
              AND g.GES_TIP_DOC = @TipoDocumento
              AND g.GES_NUM_FAC = @NumeroDocumento
            ORDER BY g.GES_FEC_GES DESC, g.GES_COR_GES DESC";
        return await connection.QueryFirstOrDefaultAsync<GestionCobranza>(sql, new { RutCliente = rutCliente, RutDeudor = rutDeudor, NumeroOperacion = numeroOperacion, TipoDocumento = tipoDocumento, NumeroDocumento = numeroDocumento });
    }

    public async Task<int> CreateAsync(GestionCobranza gestion)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO GES (GES_RUT_CLI, GES_RUT_DEU, GES_TIP_DOC, GES_NUM_FAC, GES_NUM_OPE,
                            GES_COB_TEL, GES_FEC_GES, GES_HOR_GES, GES_EST_COB, GES_FEC_PGO,
                            GES_HOR_PGO, GES_DOC_NEC, GES_OBS_GES, GES_FEC_PRX, GES_HOR_PRX, GES_REC_GES)
            VALUES (@RutCliente, @RutDeudor, @TipoDocumento, @NumeroDocumento, @NumeroOperacion,
                    @CodigoCobrador, @FechaGestion, @HoraGestion, @EstadoCobranza, @FechaPago,
                    @HoraPago, @DocumentoNecesario, @Observacion, @FechaProximaGestion, @HoraProximaGestion, @CodigoRecaudador);
            SELECT SCOPE_IDENTITY();";
        return await connection.ExecuteScalarAsync<int>(sql, gestion);
    }

    public async Task<bool> UpdateEstadoDocumentoAsync(long rutCliente, long rutDeudor, int numeroOperacion, long numeroDocumento, int tipoDocumento, string codigoCobrador, int estadoCobranza, DateTime? fechaNotificacion)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        string sql;
        // Constantes de tipos de documento
        const int TD_FACTURA = 1;
        const int TD_CHEQUE = 2;
        const int TD_LETRA = 3;
        
        switch (tipoDocumento)
        {
            case TD_FACTURA:
                sql = @"Update FAO SET FAO_COD_COB = @CodigoCobrador, FAO_EST_COB = @EstadoCobranza, FAO_FEC_NOT = @FechaNotificacion
                        WHERE FAO_RUT_CLI = @RutCliente AND FAO_RUT_DEU = @RutDeudor AND FAO_NUM_OPE = @NumeroOperacion AND FAO_NUM_FAC = @NumeroDocumento";
                break;
            case TD_CHEQUE:
                sql = @"Update CHO SET CHO_COD_COB = @CodigoCobrador, CHO_EST_COB = @EstadoCobranza, CHO_FEC_NOT = @FechaNotificacion
                        WHERE CHO_RUT_CLI = @RutCliente AND CHO_RUT_DEU = @RutDeudor AND CHO_NUM_OPE = @NumeroOperacion AND CHO_NUM_FAC = @NumeroDocumento";
                break;
            case TD_LETRA:
                sql = @"Update LEO SET LEO_COD_COB = @CodigoCobrador, LEO_EST_COB = @EstadoCobranza, LEO_FEC_NOT = @FechaNotificacion
                        WHERE LEO_RUT_CLI = @RutCliente AND LEO_RUT_DEU = @RutDeudor AND LEO_NUM_OPE = @NumeroOperacion AND LEO_NUM_FAC = @NumeroDocumento";
                break;
            default:
                sql = @"Update OTO SET OTO_COD_COB = @CodigoCobrador, OTO_EST_COB = @EstadoCobranza, OTO_FEC_NOT = @FechaNotificacion
                        WHERE OTO_RUT_CLI = @RutCliente AND OTO_RUT_DEU = @RutDeudor AND OTO_NUM_OPE = @NumeroOperacion AND OTO_NUM_FAC = @NumeroDocumento";
                break;
        }
        
        var result = await connection.ExecuteAsync(sql, new { RutCliente = rutCliente, RutDeudor = rutDeudor, NumeroOperacion = numeroOperacion, NumeroDocumento = numeroDocumento, CodigoCobrador = codigoCobrador, EstadoCobranza = estadoCobranza, FechaNotificacion = fechaNotificacion ?? new DateTime(1900, 1, 1) });
        return result > 0;
    }
}

/// <summary>
/// Repositorio de Cobradores - Migrado de F_GC1000.frm
/// </summary>
public class CobradorRecaudacionRepository : ICobradorRecaudacionRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public CobradorRecaudacionRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<CobradorRecaudacion>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT c.COB_COD_COB AS CodigoCobrador, c.COB_NOM_COB AS NombreCobrador,
                   c.COB_TEL_COB AS Telefono, c.COB_EMA_COB AS Email,
                   ISNULL(c.COB_EST_COB, 1) AS Estado, c.COB_SEC_COB AS Sector,
                   (SELECT COUNT(DISTINCT CLI_RUT_CLI) FROM CLI WHERE CLI_COB_TEL = c.COB_COD_COB) AS TotalClientesAsignados
            FROM COB c
            ORDER BY c.COB_NOM_COB";
        return await connection.QueryAsync<CobradorRecaudacion>(sql);
    }

    public async Task<CobradorRecaudacion?> GetByCodigoAsync(string codigoCobrador)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT c.COB_COD_COB AS CodigoCobrador, c.COB_NOM_COB AS NombreCobrador,
                   c.COB_TEL_COB AS Telefono, c.COB_EMA_COB AS Email,
                   ISNULL(c.COB_EST_COB, 1) AS Estado, c.COB_SEC_COB AS Sector,
                   (SELECT COUNT(DISTINCT CLI_RUT_CLI) FROM CLI WHERE CLI_COB_TEL = c.COB_COD_COB) AS TotalClientesAsignados
            FROM COB c
            WHERE c.COB_COD_COB = @CodigoCobrador";
        return await connection.QueryFirstOrDefaultAsync<CobradorRecaudacion>(sql, new { CodigoCobrador = codigoCobrador });
    }

    public async Task<IEnumerable<CobradorRecaudacion>> GetActivosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT c.COB_COD_COB AS CodigoCobrador, c.COB_NOM_COB AS NombreCobrador
            FROM COB c
            WHERE ISNULL(c.COB_EST_COB, 1) = 1
            ORDER BY c.COB_NOM_COB";
        return await connection.QueryAsync<CobradorRecaudacion>(sql);
    }

    public async Task<int> CreateAsync(CobradorRecaudacion cobrador)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO COB (COB_COD_COB, COB_NOM_COB, COB_TEL_COB, COB_EMA_COB, COB_EST_COB, COB_SEC_COB)
            VALUES (@CodigoCobrador, @NombreCobrador, @Telefono, @Email, @Estado, @Sector)";
        return await connection.ExecuteAsync(sql, cobrador);
    }

    public async Task<bool> UpdateAsync(CobradorRecaudacion cobrador)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            Update COB SET COB_NOM_COB = @NombreCobrador, COB_TEL_COB = @Telefono,
                           COB_EMA_COB = @Email, COB_EST_COB = @Estado, COB_SEC_COB = @Sector
            WHERE COB_COD_COB = @CodigoCobrador";
        var result = await connection.ExecuteAsync(sql, cobrador);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(string codigoCobrador)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "Update COB SET COB_EST_COB = 0 WHERE COB_COD_COB = @CodigoCobrador";
        var result = await connection.ExecuteAsync(sql, new { CodigoCobrador = codigoCobrador });
        return result > 0;
    }
}

/// <summary>
/// Repositorio de Documentos para Gestión
/// </summary>
public class DocumentoGestionRepository : IDocumentoGestionRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    
    // Constantes de estados
    private const int ED_CANCELADO = 4;
    private const int ED_PAGADO = 6;
    private const int ED_EXEDENTE = 7;
    private const int COD_GESPROBLEMA = 46;

    public DocumentoGestionRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<DocumentoGestion>> GetByClienteDeudorAsync(long rutCliente, long rutDeudor)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            -- Facturas
            SELECT FAO_RUT_CLI AS RutCliente, FAO_RUT_DEU AS RutDeudor, FAO_NUM_OPE AS NumeroOperacion,
                   FAO_TIP_DOC AS TipoDocumento, FAO_NUM_FAC AS NumeroDocumento,
                   CASE WHEN FAO_RPN_DOC > 0 THEN FAO_FEC_RPN ELSE FAO_FEC_VTO END AS FechaVencimiento,
                   FAO_MON_FAC AS MontoDocumento, FAO_EST_FAC AS EstadoDocumento,
                   FAO_EST_VER AS EstadoVerificacion, FAO_FEC_NOT AS FechaNotificacion, FAO_EST_COB AS EstadoCobranza,
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 5 AND PAR_COD_PAR = FAO_TIP_DOC) AS DescripcionTipoDocumento,
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 11 AND PAR_COD_PAR = FAO_EST_FAC) AS DescripcionEstadoDocumento,
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 32 AND PAR_COD_PAR = FAO_EST_COB) AS DescripcionEstadoCobranza,
                   CASE WHEN FAO_EST_COB = @CodGesProblema THEN 1 ELSE 0 END AS TieneProblema
            FROM FAO
            WHERE FAO_RUT_CLI = @RutCliente AND FAO_RUT_DEU = @RutDeudor
              AND FAO_EST_FAC NOT IN (@EdCancelado, @EdPagado, @EdExedente)
            UNION ALL
            -- Cheques
            SELECT CHO_RUT_CLI, CHO_RUT_DEU, CHO_NUM_OPE, CHO_TIP_DOC, CHO_NUM_FAC,
                   CASE WHEN CHO_RPN_DOC > 0 THEN CHO_FEC_RPN ELSE CHO_FEC_VTO END,
                   CHO_MON_FAC, CHO_EST_FAC, CHO_EST_VER, CHO_FEC_NOT, CHO_EST_COB,
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 5 AND PAR_COD_PAR = CHO_TIP_DOC),
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 11 AND PAR_COD_PAR = CHO_EST_FAC),
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 32 AND PAR_COD_PAR = CHO_EST_COB),
                   CASE WHEN CHO_EST_COB = @CodGesProblema THEN 1 ELSE 0 END
            FROM CHO
            WHERE CHO_RUT_CLI = @RutCliente AND CHO_RUT_DEU = @RutDeudor
              AND CHO_EST_FAC NOT IN (@EdCancelado, @EdPagado, @EdExedente)
            UNION ALL
            -- Letras
            SELECT LEO_RUT_CLI, LEO_RUT_DEU, LEO_NUM_OPE, LEO_TIP_DOC, LEO_NUM_FAC,
                   CASE WHEN LEO_RPN_DOC > 0 THEN LEO_FEC_RPN ELSE LEO_FEC_VTO END,
                   LEO_MON_FAC, LEO_EST_FAC, LEO_EST_VER, LEO_FEC_NOT, LEO_EST_COB,
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 5 AND PAR_COD_PAR = LEO_TIP_DOC),
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 11 AND PAR_COD_PAR = LEO_EST_FAC),
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 32 AND PAR_COD_PAR = LEO_EST_COB),
                   CASE WHEN LEO_EST_COB = @CodGesProblema THEN 1 ELSE 0 END
            FROM LEO
            WHERE LEO_RUT_CLI = @RutCliente AND LEO_RUT_DEU = @RutDeudor
              AND LEO_EST_FAC NOT IN (@EdCancelado, @EdPagado, @EdExedente)
            UNION ALL
            -- Otros documentos
            SELECT OTO_RUT_CLI, OTO_RUT_DEU, OTO_NUM_OPE, OTO_TIP_DOC, OTO_NUM_FAC,
                   CASE WHEN OTO_RPN_DOC > 0 THEN OTO_FEC_RPN ELSE OTO_FEC_VTO END,
                   OTO_MON_FAC, OTO_EST_FAC, OTO_EST_VER, OTO_FEC_NOT, OTO_EST_COB,
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 5 AND PAR_COD_PAR = OTO_TIP_DOC),
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 11 AND PAR_COD_PAR = OTO_EST_FAC),
                   (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 32 AND PAR_COD_PAR = OTO_EST_COB),
                   CASE WHEN OTO_EST_COB = @CodGesProblema THEN 1 ELSE 0 END
            FROM OTO
            WHERE OTO_RUT_CLI = @RutCliente AND OTO_RUT_DEU = @RutDeudor
              AND OTO_EST_FAC NOT IN (@EdCancelado, @EdPagado, @EdExedente)
            ORDER BY TipoDocumento, RutCliente, RutDeudor";
        
        return await connection.QueryAsync<DocumentoGestion>(sql, new { RutCliente = rutCliente, RutDeudor = rutDeudor, EdCancelado = ED_CANCELADO, EdPagado = ED_PAGADO, EdExedente = ED_EXEDENTE, CodGesProblema = COD_GESPROBLEMA });
    }

    public async Task<IEnumerable<DocumentoGestion>> GetByCobradorAsync(string codigoCobrador)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Similar al anterior pero filtrado por cobrador a través de CLI_COB_TEL
        const string sql = @"
            SELECT FAO_RUT_CLI AS RutCliente, FAO_RUT_DEU AS RutDeudor, FAO_NUM_OPE AS NumeroOperacion,
                   FAO_TIP_DOC AS TipoDocumento, FAO_NUM_FAC AS NumeroDocumento,
                   CASE WHEN FAO_RPN_DOC > 0 THEN FAO_FEC_RPN ELSE FAO_FEC_VTO END AS FechaVencimiento,
                   FAO_MON_FAC AS MontoDocumento, FAO_EST_FAC AS EstadoDocumento
            FROM FAO
            INNER JOIN CLI ON FAO_RUT_CLI = CLI_RUT_CLI
            WHERE CLI_COB_TEL = @CodigoCobrador
              AND FAO_EST_FAC NOT IN (4, 6, 7)";
        return await connection.QueryAsync<DocumentoGestion>(sql, new { CodigoCobrador = codigoCobrador });
    }

    public async Task<IEnumerable<DeudorGestion>> GetDeudoresByCobradorAsync(string codigoCobrador)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT DEU_RUT_DEU AS RutDeudor,
                   (SELECT TOP 1 DEU_RAZ_SOC FROM DEU d2 WHERE d2.DEU_RUT_DEU = DEU.DEU_RUT_DEU) AS NombreDeudor
            FROM CLI, DEU
            WHERE CLI_RUT_CLI = DEU_RUT_CLI
              AND CLI_COB_TEL = @CodigoCobrador
              AND (EXISTS(SELECT TOP 1 FAO_RUT_CLI FROM FAO WHERE FAO_RUT_CLI = DEU_RUT_CLI AND FAO_RUT_DEU = DEU_RUT_DEU AND FAO_EST_FAC NOT IN (4, 6, 7))
                   OR EXISTS(SELECT TOP 1 CHO_RUT_CLI FROM CHO WHERE CHO_RUT_CLI = DEU_RUT_CLI AND CHO_RUT_DEU = DEU_RUT_DEU AND CHO_EST_FAC NOT IN (4, 6, 7))
                   OR EXISTS(SELECT TOP 1 LEO_RUT_CLI FROM LEO WHERE LEO_RUT_CLI = DEU_RUT_CLI AND LEO_RUT_DEU = DEU_RUT_DEU AND LEO_EST_FAC NOT IN (4, 6, 7))
                   OR EXISTS(SELECT TOP 1 OTO_RUT_CLI FROM OTO WHERE OTO_RUT_CLI = DEU_RUT_CLI AND OTO_RUT_DEU = DEU_RUT_DEU AND OTO_EST_FAC NOT IN (4, 6, 7)))
            GROUP BY DEU_RUT_DEU
            ORDER BY NombreDeudor";
        return await connection.QueryAsync<DeudorGestion>(sql, new { CodigoCobrador = codigoCobrador });
    }

    public async Task<IEnumerable<ClienteCobrador>> GetClientesByCobradorDeudorAsync(string codigoCobrador, long rutDeudor)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT CLI.CLI_RUT_CLI AS RutCliente, 
                   RTRIM(CLI.CLI_RAZ_SOC) + ' ' + RTRIM(ISNULL(CLI.CLI_APE_PAT,'')) + ' ' + RTRIM(ISNULL(CLI.CLI_APE_MAT,'')) AS NombreCliente
            FROM CLI, DEU
            WHERE CLI.CLI_RUT_CLI = DEU.DEU_RUT_CLI
              AND DEU.DEU_RUT_DEU = @RutDeudor
              AND CLI.CLI_COB_TEL = @CodigoCobrador
              AND (EXISTS(SELECT * FROM FAO WHERE FAO_RUT_CLI = DEU_RUT_CLI AND FAO_RUT_DEU = DEU_RUT_DEU AND FAO_EST_FAC NOT IN (4, 6, 7))
                   OR EXISTS(SELECT * FROM CHO WHERE CHO_RUT_CLI = DEU_RUT_CLI AND CHO_RUT_DEU = DEU_RUT_DEU AND CHO_EST_FAC NOT IN (4, 6, 7))
                   OR EXISTS(SELECT * FROM LEO WHERE LEO_RUT_CLI = DEU_RUT_CLI AND LEO_RUT_DEU = DEU_RUT_DEU AND LEO_EST_FAC NOT IN (4, 6, 7))
                   OR EXISTS(SELECT * FROM OTO WHERE OTO_RUT_CLI = DEU_RUT_CLI AND OTO_RUT_DEU = DEU_RUT_DEU AND OTO_EST_FAC NOT IN (4, 6, 7)))";
        return await connection.QueryAsync<ClienteCobrador>(sql, new { CodigoCobrador = codigoCobrador, RutDeudor = rutDeudor });
    }

    public async Task<bool> UpdateDeudorContactoAsync(long rutCliente, long rutDeudor, string? contactoCobranza, string? direccionPago)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            Update DEU SET DEU_DIR_PGO = @DireccionPago, DEU_CTO_COB = @ContactoCobranza
            WHERE DEU_RUT_CLI = @RutCliente AND DEU_RUT_DEU = @RutDeudor";
        var result = await connection.ExecuteAsync(sql, new { RutCliente = rutCliente, RutDeudor = rutDeudor, ContactoCobranza = contactoCobranza ?? "SIN CONTACTO", DireccionPago = direccionPago ?? "SIN DIRECCION" });
        return result > 0;
    }
}

/// <summary>
/// Repositorio de Estados de Cobranza - Migrado de F_GC2600.frm
/// </summary>
public class EstadoCobranzaRepository : IEstadoCobranzaRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    
    private const int ED_CANCELADO = 4;
    private const int ED_PAGADO = 6;
    private const int ED_EXEDENTE = 7;

    public EstadoCobranzaRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<EstadoCobranzaDocumento>> GetByFiltroAsync(FiltroEstadoCobranzaDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT CLI.CLI_RUT_CLI AS RutCliente, 
                   RTRIM(CLI.CLI_RAZ_SOC) + ' ' + LTRIM(CLI.CLI_APE_PAT) AS NombreCliente,
                   DEU.DEU_RUT_DEU AS RutDeudor, DEU.DEU_RAZ_SOC AS NombreDeudor,
                   FAO.FAO_TIP_DOC AS TipoDocumento, 
                   DCI_COD_PLA AS DescripcionTipoDocumento,
                   FAO.FAO_NUM_FAC AS NumeroDocumento,
                   FAO.FAO_FEC_VTO AS FechaVencimiento,
                   ISNULL(FAO.FAO_EST_COB, 18) AS EstadoCobranza,
                   CASE WHEN ISDATE(FAO.FAO_FEC_PAG) = 1 THEN FAO.FAO_FEC_PAG ELSE NULL END AS FechaPago,
                   FAO.FAO_MON_FAC AS MontoDocumento,
                   ISNULL((SELECT BCO_DES_BCO FROM BCO WHERE BCO_COD_BCO = FAO.FAO_COD_BCO),' ') AS NombreBanco,
                   (SELECT EJE_NOM_EJE FROM EJE WHERE EJE_COD_EJE = CLI.CLI_COD_EJE) AS NombreEjecutivo,
                   (SELECT COB_NOM_COB FROM COB WHERE COB_COD_COB = CLI.CLI_COB_TEL) AS NombreCobrador,
                   ISNULL((SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 11 AND PAR_COD_PAR = ISNULL(FAO.FAO_EST_FAC, 0)), 'SIN OBSERVACION') AS DescripcionEstadoDocumento,
                   ISNULL((SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 32 AND PAR_COD_PAR = ISNULL(FAO.FAO_EST_COB, 18)), 'SIN OBSERVACION') AS DescripcionEstadoCobranza
            FROM DCI, FAO, CLI, DEU, EJE
            WHERE FAO.FAO_RUT_CLI = CLI.CLI_RUT_CLI
              AND DCI_TIP_DOC = FAO.FAO_TIP_DOC
              AND FAO.FAO_RUT_DEU = DEU.DEU_RUT_DEU
              AND CLI.CLI_RUT_CLI = DEU.DEU_RUT_CLI
              AND FAO.FAO_EST_FAC NOT IN (@EdCancelado, @EdPagado, @EdExedente)
              AND FAO.FAO_FEC_VTO >= @FechaDesde
              AND FAO.FAO_FEC_VTO <= @FechaHasta";

        if (filtro.RutCliente.HasValue)
            sql += " AND FAO.FAO_RUT_CLI = @RutCliente";

        sql += @" UNION ALL
            SELECT CLI.CLI_RUT_CLI, RTRIM(CLI.CLI_RAZ_SOC) + ' ' + LTRIM(CLI.CLI_APE_PAT),
                   DEU.DEU_RUT_DEU, DEU.DEU_RAZ_SOC, CHO.CHO_TIP_DOC, DCI_COD_PLA,
                   CHO.CHO_NUM_FAC, CHO.CHO_FEC_VTO, ISNULL(CHO.CHO_EST_COB, 18),
                   CASE WHEN ISDATE(CHO.CHO_FEC_PAG) = 1 THEN CHO.CHO_FEC_PAG ELSE NULL END,
                   CHO.CHO_MON_FAC,
                   ISNULL((SELECT BCO_DES_BCO FROM BCO WHERE BCO_COD_BCO = CHO.CHO_COD_BAN),' '),
                   (SELECT EJE_NOM_EJE FROM EJE WHERE EJE_COD_EJE = CLI.CLI_COD_EJE),
                   (SELECT COB_NOM_COB FROM COB WHERE COB_COD_COB = CLI.CLI_COB_TEL),
                   ISNULL((SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 11 AND PAR_COD_PAR = ISNULL(CHO.CHO_EST_FAC, 0)), 'SIN OBSERVACION'),
                   ISNULL((SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 32 AND PAR_COD_PAR = ISNULL(CHO.CHO_EST_COB, 18)), 'SIN OBSERVACION')
            FROM DCI, CHO, CLI, DEU, EJE
            WHERE CHO.CHO_RUT_CLI = CLI.CLI_RUT_CLI
              AND DCI_TIP_DOC = CHO.CHO_TIP_DOC
              AND CHO.CHO_RUT_DEU = DEU.DEU_RUT_DEU
              AND CLI.CLI_RUT_CLI = DEU.DEU_RUT_CLI
              AND CHO.CHO_EST_FAC NOT IN (@EdCancelado, @EdPagado, @EdExedente)
              AND CHO.CHO_FEC_VTO >= @FechaDesde
              AND CHO.CHO_FEC_VTO <= @FechaHasta";

        if (filtro.RutCliente.HasValue)
            sql += " AND CHO.CHO_RUT_CLI = @RutCliente";

        sql += @" UNION ALL
            SELECT CLI.CLI_RUT_CLI, RTRIM(CLI.CLI_RAZ_SOC) + ' ' + LTRIM(CLI.CLI_APE_PAT),
                   DEU.DEU_RUT_DEU, DEU.DEU_RAZ_SOC, LEO.LEO_TIP_DOC, DCI_COD_PLA,
                   LEO.LEO_NUM_FAC, LEO.LEO_FEC_VTO, ISNULL(LEO.LEO_EST_COB, 18),
                   CASE WHEN ISDATE(LEO.LEO_FEC_PAG) = 1 THEN LEO.LEO_FEC_PAG ELSE NULL END,
                   LEO.LEO_MON_FAC,
                   ' ',
                   (SELECT EJE_NOM_EJE FROM EJE WHERE EJE_COD_EJE = CLI.CLI_COD_EJE),
                   (SELECT COB_NOM_COB FROM COB WHERE COB_COD_COB = CLI.CLI_COB_TEL),
                   ISNULL((SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 11 AND PAR_COD_PAR = ISNULL(LEO.LEO_EST_FAC, 0)), 'SIN OBSERVACION'),
                   ISNULL((SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 32 AND PAR_COD_PAR = ISNULL(LEO.LEO_EST_COB, 18)), 'SIN OBSERVACION')
            FROM DCI, LEO, CLI, DEU, EJE
            WHERE LEO.LEO_RUT_CLI = CLI.CLI_RUT_CLI
              AND DCI_TIP_DOC = LEO.LEO_TIP_DOC
              AND LEO.LEO_RUT_DEU = DEU.DEU_RUT_DEU
              AND CLI.CLI_RUT_CLI = DEU.DEU_RUT_CLI
              AND LEO.LEO_EST_FAC NOT IN (@EdCancelado, @EdPagado, @EdExedente)
              AND LEO.LEO_FEC_VTO >= @FechaDesde
              AND LEO.LEO_FEC_VTO <= @FechaHasta";

        if (filtro.RutCliente.HasValue)
            sql += " AND LEO.LEO_RUT_CLI = @RutCliente";

        sql += " ORDER BY 2, 9, 8";  // Order by NombreCliente, EstadoCobranza, FechaVencimiento

        return await connection.QueryAsync<EstadoCobranzaDocumento>(sql, new { filtro.FechaDesde, filtro.FechaHasta, filtro.RutCliente, EdCancelado = ED_CANCELADO, EdPagado = ED_PAGADO, EdExedente = ED_EXEDENTE });
    }

    public async Task<ResumenEstadoCobranzaDto> GetResumenAsync(FiltroEstadoCobranzaDto filtro)
    {
        var documentos = await GetByFiltroAsync(filtro);
        var resumen = new ResumenEstadoCobranzaDto
        {
            FechaDesde = filtro.FechaDesde,
            FechaHasta = filtro.FechaHasta,
            TotalDocumentos = documentos.Count(),
            MontoTotal = documentos.Sum(d => d.MontoDocumento),
            TotalClientes = documentos.Select(d => d.RutCliente).Distinct().Count()
        };

        var grupos = documentos.GroupBy(d => d.EstadoCobranza);
        foreach (var grupo in grupos)
        {
            resumen.PorEstado[grupo.Key.ToString()] = new ResumenPorEstadoDto
            {
                EstadoCobranza = grupo.Key,
                DescripcionEstado = grupo.First().DescripcionEstadoCobranza,
                CantidadDocumentos = grupo.Count(),
                Monto = grupo.Sum(d => d.MontoDocumento)
            };
        }

        return resumen;
    }
}

/// <summary>
/// Repositorio de Deuda Total Cliente - Migrado de F_GC2200.frm
/// </summary>
public class DeudaTotalClienteRepository : IDeudaTotalClienteRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    
    private const int ED_CANCELADO = 4;
    private const int ED_PAGADO = 6;
    private const int ED_EXEDENTE = 7;

    public DeudaTotalClienteRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DeudaTotalCliente> CalcularDeudaTotalAsync(long rutCliente, DateTime? fechaCalculo)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Obtener nombre del cliente
        var clienteSql = "SELECT CLI_RAZ_SOC FROM CLI WHERE CLI_RUT_CLI = @RutCliente";
        var nombreCliente = await connection.QueryFirstOrDefaultAsync<string>(clienteSql, new { RutCliente = rutCliente }) ?? "";

        // Calcular totales por tipo de documento
        var sql = @"
            SELECT 
                (SELECT ISNULL(SUM(FAO_MON_FAC), 0) FROM FAO WHERE FAO_RUT_CLI = @RutCliente AND FAO_EST_FAC NOT IN (@EdCancelado, @EdPagado, @EdExedente)) AS TotalFacturas,
                (SELECT ISNULL(SUM(CHO_MON_FAC), 0) FROM CHO WHERE CHO_RUT_CLI = @RutCliente AND CHO_EST_FAC NOT IN (@EdCancelado, @EdPagado, @EdExedente)) AS TotalCheques,
                (SELECT ISNULL(SUM(LEO_MON_FAC), 0) FROM LEO WHERE LEO_RUT_CLI = @RutCliente AND LEO_EST_FAC NOT IN (@EdCancelado, @EdPagado, @EdExedente)) AS TotalLetras,
                (SELECT ISNULL(SUM(OTO_MON_FAC), 0) FROM OTO WHERE OTO_RUT_CLI = @RutCliente AND OTO_EST_FAC NOT IN (@EdCancelado, @EdPagado, @EdExedente)) AS TotalOtros,
                (SELECT ISNULL(SUM(CXP_SAL_CXP), 0) FROM CXP WHERE CXP_RUT_CLI = @RutCliente AND CXP_SAL_CXP > 0 AND (CXP_EST_CXP IS NULL OR CXP_EST_CXP = 0)) AS CuentasPorPagar,
                (SELECT ISNULL(SUM(CXC_SAL_CXC), 0) FROM CXC WHERE CXC_RUT_CLI = @RutCliente AND CXC_SAL_CXC > 0) AS CuentasPorCobrar";
        
        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { RutCliente = rutCliente, EdCancelado = ED_CANCELADO, EdPagado = ED_PAGADO, EdExedente = ED_EXEDENTE });

        var deudaTotal = new DeudaTotalCliente
        {
            RutCliente = rutCliente,
            NombreCliente = nombreCliente,
            FechaCalculo = fechaCalculo,
            TotalFacturas = result?.TotalFacturas ?? 0,
            TotalCheques = result?.TotalCheques ?? 0,
            TotalLetras = result?.TotalLetras ?? 0,
            TotalOtros = result?.TotalOtros ?? 0,
            CuentasPorPagar = result?.CuentasPorPagar ?? 0,
            CuentasPorCobrar = result?.CuentasPorCobrar ?? 0
        };

        // Calcular deuda neta
        deudaTotal.DeudaNeta = deudaTotal.TotalFacturas + deudaTotal.TotalCheques + deudaTotal.TotalLetras + deudaTotal.TotalOtros 
                             - deudaTotal.CuentasPorPagar - deudaTotal.FacturasNoFactoring + deudaTotal.CuentasPorCobrar - deudaTotal.Excedentes;

        return deudaTotal;
    }

    public async Task<IEnumerable<DetalleDeudaDocumento>> GetDetalleDeudaByTipoAsync(long rutCliente, int tipoDocumento, DateTime? fechaCalculo)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const int TD_FACTURA = 1;
        const int TD_CHEQUE = 2;
        const int TD_LETRA = 3;
        
        string sql;
        switch (tipoDocumento)
        {
            case TD_FACTURA:
                sql = @"
                    SELECT FAO_RUT_DEU AS RutDeudor, 
                           (SELECT DEU_RAZ_SOC FROM DEU WHERE DEU_RUT_CLI = FAO_RUT_CLI AND DEU_RUT_DEU = FAO_RUT_DEU) AS NombreDeudor,
                           FAO_NUM_OPE AS NumeroOperacion, 'FAC' AS TipoDocumentoCodigo,
                           FAO_NUM_FAC AS NumeroDocumento, FAO_FEC_VTO AS FechaVencimiento,
                           FAO_MON_FAC AS MontoDocumento, FAO_MON_ANT AS MontoAnticipo,
                           ISNULL(FAO_INT_VDO, 0) AS InteresVencido, ISNULL(FAO_INT_DDS, 0) AS InteresMora,
                           FAO_NOT_DEV AS InteresNoDevengado,
                           (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 11 AND PAR_COD_PAR = FAO_EST_FAC) AS EstadoDocumento,
                           FAO_MON_FAC - FAO_SAL_PEN AS Abono,
                           FAO_SAL_CLI + ISNULL(FAO_INT_VDO, 0) + ISNULL(FAO_INT_DDS, 0) AS DeudaNeta
                    FROM FAO, OPE
                    WHERE FAO_RUT_CLI = @RutCliente
                      AND FAO_RUT_CLI = OPE_RUT_CLI
                      AND FAO_NUM_OPE = OPE_NUM_OPE
                      AND FAO_EST_FAC NOT IN (4, 6, 7)
                    ORDER BY 2, 6, 4";
                break;
            case TD_CHEQUE:
                sql = @"
                    SELECT CHO_RUT_DEU AS RutDeudor,
                           (SELECT DEU_RAZ_SOC FROM DEU WHERE DEU_RUT_CLI = CHO_RUT_CLI AND DEU_RUT_DEU = CHO_RUT_DEU) AS NombreDeudor,
                           CHO_NUM_OPE AS NumeroOperacion, 'CHE' AS TipoDocumentoCodigo,
                           CHO_NUM_FAC AS NumeroDocumento, CHO_FEC_VTO AS FechaVencimiento,
                           CHO_MON_FAC AS MontoDocumento, CHO_MON_ANT AS MontoAnticipo,
                           ISNULL(CHO_INT_VDO, 0) AS InteresVencido, ISNULL(CHO_INT_DDS, 0) AS InteresMora,
                           CHO_NOT_DEV + CHO_DEV_DIA AS InteresNoDevengado,
                           (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 11 AND PAR_COD_PAR = CHO_EST_FAC) AS EstadoDocumento,
                           CHO_MON_FAC - CHO_SAL_PEN AS Abono,
                           CHO_SAL_CLI + ISNULL(CHO_INT_VDO, 0) + ISNULL(CHO_INT_DDS, 0) AS DeudaNeta
                    FROM CHO, OPE
                    WHERE CHO_RUT_CLI = @RutCliente
                      AND CHO_RUT_CLI = OPE_RUT_CLI
                      AND CHO_NUM_OPE = OPE_NUM_OPE
                      AND CHO_EST_FAC NOT IN (4, 6, 7)
                    ORDER BY 2, 6, 4";
                break;
            case TD_LETRA:
                sql = @"
                    SELECT LEO_RUT_DEU AS RutDeudor,
                           (SELECT DEU_RAZ_SOC FROM DEU WHERE DEU_RUT_CLI = LEO_RUT_CLI AND DEU_RUT_DEU = LEO_RUT_DEU) AS NombreDeudor,
                           LEO_NUM_OPE AS NumeroOperacion, 'LET' AS TipoDocumentoCodigo,
                           LEO_NUM_FAC AS NumeroDocumento, LEO_FEC_VTO AS FechaVencimiento,
                           LEO_MON_FAC AS MontoDocumento, LEO_MON_ANT AS MontoAnticipo,
                           ISNULL(LEO_INT_VDO, 0) AS InteresVencido, ISNULL(LEO_INT_DDS, 0) AS InteresMora,
                           LEO_NOT_DEV + LEO_DEV_DIA AS InteresNoDevengado,
                           (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 11 AND PAR_COD_PAR = LEO_EST_FAC) AS EstadoDocumento,
                           LEO_MON_FAC - LEO_SAL_PEN AS Abono,
                           LEO_SAL_CLI + ISNULL(LEO_INT_VDO, 0) + ISNULL(LEO_INT_DDS, 0) AS DeudaNeta
                    FROM LEO, OPE
                    WHERE LEO_RUT_CLI = @RutCliente
                      AND LEO_RUT_CLI = OPE_RUT_CLI
                      AND LEO_NUM_OPE = OPE_NUM_OPE
                      AND LEO_EST_FAC NOT IN (4, 6, 7)
                    ORDER BY 2, 6, 4";
                break;
            default:
                sql = @"
                    SELECT OTO_RUT_DEU AS RutDeudor,
                           (SELECT DEU_RAZ_SOC FROM DEU WHERE DEU_RUT_CLI = OTO_RUT_CLI AND DEU_RUT_DEU = OTO_RUT_DEU) AS NombreDeudor,
                           OTO_NUM_OPE AS NumeroOperacion, 'OTR' AS TipoDocumentoCodigo,
                           OTO_NUM_FAC AS NumeroDocumento, OTO_FEC_VTO AS FechaVencimiento,
                           OTO_MON_FAC AS MontoDocumento, OTO_MON_ANT AS MontoAnticipo,
                           ISNULL(OTO_INT_VDO, 0) AS InteresVencido, ISNULL(OTO_INT_DDS, 0) AS InteresMora,
                           OTO_NOT_DEV + OTO_DEV_DIA AS InteresNoDevengado,
                           (SELECT PAR_DES_PAR FROM PAR WHERE PAR_COD_TAB = 11 AND PAR_COD_PAR = OTO_EST_FAC) AS EstadoDocumento,
                           OTO_MON_FAC - OTO_SAL_PEN AS Abono,
                           OTO_SAL_CLI + ISNULL(OTO_INT_VDO, 0) + ISNULL(OTO_INT_DDS, 0) AS DeudaNeta
                    FROM OTO, OPE
                    WHERE OTO_RUT_CLI = @RutCliente
                      AND OTO_RUT_CLI = OPE_RUT_CLI
                      AND OTO_NUM_OPE = OPE_NUM_OPE
                      AND OTO_EST_FAC NOT IN (4, 6, 7)
                      AND OTO_TIP_DOC = @TipoDocumento
                    ORDER BY 2, 6, 4";
                break;
        }

        return await connection.QueryAsync<DetalleDeudaDocumento>(sql, new { RutCliente = rutCliente, TipoDocumento = tipoDocumento });
    }
}

