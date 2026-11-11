using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Factoring.Core.DTOs;
using Factoring.Core.Interfaces;
using Factoring.Infrastructure.Data;

namespace Factoring.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de Líneas de Crédito
/// Migrado de F_LC1100.frm, F_LC1400.frm
/// </summary>
public class LineaCreditoRepository : ILineaCreditoRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public LineaCreditoRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<LineaCreditoDto>> GetAllAsync(FiltroLineaCreditoDto? filtro = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                L.LCR_RUT_CLI AS RutCliente,
                L.LCR_NUM_LIN AS NumeroLinea,
                L.LCR_TIP_MON AS TipoMoneda,
                CASE L.LCR_TIP_MON 
                    WHEN 1 THEN 'Peso' 
                    WHEN 2 THEN 'Dólar' 
                    WHEN 3 THEN 'UF' 
                END AS DescripcionMoneda,
                L.LCR_FEC_SOL AS FechaSolicitud,
                L.LCR_FEC_RES AS FechaResolucion,
                L.LCR_FVI_DES AS VigenciaDesde,
                L.LCR_FVI_HAS AS VigenciaHasta,
                L.LCR_EST_SOL AS EstadoSolicitud,
                PS.PAR_DES_PAR AS DescripcionEstadoSolicitud,
                L.LCR_TAS_NEG AS TasaNegocio,
                L.LCR_SPR_EAD AS Spread,
                L.LCR_MON_SOL AS MontoSolicitado,
                L.LCR_MON_APR AS MontoAprobado,
                L.LCR_MON_DIS AS MontoDisponible,
                L.LCR_EST_LIN AS EstadoLinea,
                PL.PAR_DES_PAR AS DescripcionEstadoLinea,
                ISNULL(C.CLI_RAZ_SOC, C.CLI_APE_PAT + ' ' + ISNULL(C.CLI_APE_MAT, '')) AS NombreCliente,
                PR.PAR_DES_PAR AS CategoriaRiesgo,
                L.LCR_NUM_CON AS NumeroContrato,
                L.LCR_TIP_CON AS TipoContrato,
                L.LCR_FEC_CON AS FechaContrato,
                L.LCR_VTO_CON AS VencimientoContrato,
                L.LCR_EST_CON AS EstadoContrato,
                L.LCR_OBS_CON AS ObservacionesContrato,
                L.LCR_COD_NOT AS CodigoNotaria,
                N.NTA_NOM_NOT AS NombreNotaria,
                L.LCR_TAS_MOR AS TasaMora,
                L.LCR_DES_MOR AS DiasMora,
                L.LCR_CAL_MOR AS CalculoMora
            FROM LCR L
            INNER JOIN CLI C ON C.CLI_RUT_CLI = L.LCR_RUT_CLI
            LEFT JOIN PAR PS ON PS.PAR_COD_TAB = 15 AND PS.PAR_COD_PAR = L.LCR_EST_SOL
            LEFT JOIN PAR PL ON PL.PAR_COD_TAB = 16 AND PL.PAR_COD_PAR = L.LCR_EST_LIN
            LEFT JOIN PAR PR ON PR.PAR_COD_TAB = 60 AND PR.PAR_COD_PAR = C.CLI_CAT_RIE
            LEFT JOIN NTA N ON N.NTA_COD_NOT = L.LCR_COD_NOT
            WHERE 1=1";

        var parameters = new DynamicParameters();
        
        if (filtro != null)
        {
            if (filtro.RutCliente.HasValue)
            {
                sql += " AND L.LCR_RUT_CLI = @RutCliente";
                parameters.Add("RutCliente", filtro.RutCliente.Value);
            }
            if (filtro.EstadoLinea.HasValue)
            {
                sql += " AND L.LCR_EST_LIN = @EstadoLinea";
                parameters.Add("EstadoLinea", filtro.EstadoLinea.Value);
            }
            if (filtro.EstadoSolicitud.HasValue)
            {
                sql += " AND L.LCR_EST_SOL = @EstadoSolicitud";
                parameters.Add("EstadoSolicitud", filtro.EstadoSolicitud.Value);
            }
            if (filtro.TipoMoneda.HasValue)
            {
                sql += " AND L.LCR_TIP_MON = @TipoMoneda";
                parameters.Add("TipoMoneda", filtro.TipoMoneda.Value);
            }
            if (filtro.FechaSolicitudDesde.HasValue)
            {
                sql += " AND L.LCR_FEC_SOL >= @FechaSolicitudDesde";
                parameters.Add("FechaSolicitudDesde", filtro.FechaSolicitudDesde.Value);
            }
            if (filtro.FechaSolicitudHasta.HasValue)
            {
                sql += " AND L.LCR_FEC_SOL <= @FechaSolicitudHasta";
                parameters.Add("FechaSolicitudHasta", filtro.FechaSolicitudHasta.Value);
            }
            if (filtro.FechaVencimientoDesde.HasValue)
            {
                sql += " AND L.LCR_FVI_HAS >= @FechaVencimientoDesde";
                parameters.Add("FechaVencimientoDesde", filtro.FechaVencimientoDesde.Value);
            }
            if (filtro.FechaVencimientoHasta.HasValue)
            {
                sql += " AND L.LCR_FVI_HAS <= @FechaVencimientoHasta";
                parameters.Add("FechaVencimientoHasta", filtro.FechaVencimientoHasta.Value);
            }
        }

        sql += " ORDER BY L.LCR_RUT_CLI, L.LCR_NUM_LIN";

        return await connection.QueryAsync<LineaCreditoDto>(sql, parameters);
    }

    public async Task<LineaCreditoDto?> GetByIdAsync(int rutCliente, int numeroLinea)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                L.LCR_RUT_CLI AS RutCliente,
                L.LCR_NUM_LIN AS NumeroLinea,
                L.LCR_TIP_MON AS TipoMoneda,
                CASE L.LCR_TIP_MON WHEN 1 THEN 'Peso' WHEN 2 THEN 'Dólar' WHEN 3 THEN 'UF' END AS DescripcionMoneda,
                L.LCR_FEC_SOL AS FechaSolicitud,
                L.LCR_FEC_RES AS FechaResolucion,
                L.LCR_FVI_DES AS VigenciaDesde,
                L.LCR_FVI_HAS AS VigenciaHasta,
                L.LCR_EST_SOL AS EstadoSolicitud,
                PS.PAR_DES_PAR AS DescripcionEstadoSolicitud,
                L.LCR_TAS_NEG AS TasaNegocio,
                L.LCR_SPR_EAD AS Spread,
                L.LCR_MON_SOL AS MontoSolicitado,
                L.LCR_MON_APR AS MontoAprobado,
                L.LCR_MON_DIS AS MontoDisponible,
                L.LCR_EST_LIN AS EstadoLinea,
                PL.PAR_DES_PAR AS DescripcionEstadoLinea,
                ISNULL(C.CLI_RAZ_SOC, C.CLI_APE_PAT + ' ' + ISNULL(C.CLI_APE_MAT, '')) AS NombreCliente,
                PR.PAR_DES_PAR AS CategoriaRiesgo,
                L.LCR_NUM_CON AS NumeroContrato,
                L.LCR_TIP_CON AS TipoContrato,
                L.LCR_FEC_CON AS FechaContrato,
                L.LCR_VTO_CON AS VencimientoContrato,
                L.LCR_EST_CON AS EstadoContrato,
                L.LCR_OBS_CON AS ObservacionesContrato,
                L.LCR_COD_NOT AS CodigoNotaria,
                N.NTA_NOM_NOT AS NombreNotaria,
                L.LCR_TAS_MOR AS TasaMora,
                L.LCR_DES_MOR AS DiasMora,
                L.LCR_CAL_MOR AS CalculoMora
            FROM LCR L
            INNER JOIN CLI C ON C.CLI_RUT_CLI = L.LCR_RUT_CLI
            LEFT JOIN PAR PS ON PS.PAR_COD_TAB = 15 AND PS.PAR_COD_PAR = L.LCR_EST_SOL
            LEFT JOIN PAR PL ON PL.PAR_COD_TAB = 16 AND PL.PAR_COD_PAR = L.LCR_EST_LIN
            LEFT JOIN PAR PR ON PR.PAR_COD_TAB = 60 AND PR.PAR_COD_PAR = C.CLI_CAT_RIE
            LEFT JOIN NTA N ON N.NTA_COD_NOT = L.LCR_COD_NOT
            WHERE L.LCR_RUT_CLI = @RutCliente AND L.LCR_NUM_LIN = @NumeroLinea";

        return await connection.QueryFirstOrDefaultAsync<LineaCreditoDto>(sql, new { RutCliente = rutCliente, NumeroLinea = numeroLinea });
    }

    public async Task<IEnumerable<LineaCreditoDto>> GetByClienteAsync(int rutCliente)
    {
        var filtro = new FiltroLineaCreditoDto { RutCliente = rutCliente };
        return await GetAllAsync(filtro);
    }

    public async Task<int> CreateAsync(CreateLineaCreditoDto dto, string usuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var numeroLinea = await GetNextNumeroLineaAsync(dto.RutCliente);

            var sql = @"
                INSERT INTO LCR (
                    LCR_RUT_CLI, LCR_NUM_LIN, LCR_TIP_MON, LCR_FEC_SOL, LCR_FEC_RES,
                    LCR_FVI_DES, LCR_FVI_HAS, LCR_EST_SOL, LCR_TAS_NEG, LCR_SPR_EAD,
                    LCR_MON_SOL, LCR_MON_APR, LCR_MON_DIS, LCR_EST_LIN,
                    LCR_NUM_CON, LCR_TIP_CON, LCR_FEC_CON, LCR_VTO_CON, LCR_EST_CON,
                    LCR_FES_CON, LCR_OBS_CON, LCR_COD_NOT,
                    LCR_USR_MOD, LCR_FEC_MOD, LCR_HOR_MOD,
                    LCR_TAS_MOR, LCR_DES_MOR, LCR_CAL_MOR
                ) VALUES (
                    @RutCliente, @NumeroLinea, @TipoMoneda, @FechaSolicitud, @FechaResolucion,
                    @VigenciaDesde, @VigenciaHasta, @EstadoSolicitud, @TasaNegocio, @Spread,
                    @MontoSolicitado, @MontoAprobado, @MontoAprobado, @EstadoLinea,
                    0, @TipoContrato, @FechaContrato, @VencimientoContrato, @EstadoContrato,
                    NULL, @ObservacionesContrato, @CodigoNotaria,
                    @Usuario, @FechaModificacion, @HoraModificacion,
                    @TasaMora, @DiasMora, @CalculoMora
                )";

            await connection.ExecuteAsync(sql, new
            {
                dto.RutCliente,
                NumeroLinea = numeroLinea,
                dto.TipoMoneda,
                dto.FechaSolicitud,
                dto.FechaResolucion,
                dto.VigenciaDesde,
                dto.VigenciaHasta,
                dto.EstadoSolicitud,
                dto.TasaNegocio,
                dto.Spread,
                dto.MontoSolicitado,
                dto.MontoAprobado,
                dto.EstadoLinea,
                dto.TipoContrato,
                dto.FechaContrato,
                dto.VencimientoContrato,
                dto.EstadoContrato,
                dto.ObservacionesContrato,
                dto.CodigoNotaria,
                Usuario = usuario,
                FechaModificacion = DateTime.Today,
                HoraModificacion = DateTime.Now.ToString("HH:mm"),
                dto.TasaMora,
                dto.DiasMora,
                dto.CalculoMora
            }, transaction);

            // Guardar comisiones si se proporcionan
            if (dto.ComisionApertura.HasValue || dto.ComisionRenovacion.HasValue || dto.ComisionExceso.HasValue)
            {
                var sqlParam = "INSERT INTO LCR_PAR (LCP_NRO_LCR, LCP_RUT_CLI, LCP_TIP_PAR, LCP_VAL_PAR) VALUES (@NumeroLinea, @RutCliente, @TipoParametro, @Valor)";
                
                if (dto.ComisionApertura.HasValue)
                    await connection.ExecuteAsync(sqlParam, new { NumeroLinea = numeroLinea, dto.RutCliente, TipoParametro = 1, Valor = dto.ComisionApertura.Value }, transaction);
                if (dto.ComisionRenovacion.HasValue)
                    await connection.ExecuteAsync(sqlParam, new { NumeroLinea = numeroLinea, dto.RutCliente, TipoParametro = 2, Valor = dto.ComisionRenovacion.Value }, transaction);
                if (dto.ComisionExceso.HasValue)
                    await connection.ExecuteAsync(sqlParam, new { NumeroLinea = numeroLinea, dto.RutCliente, TipoParametro = 3, Valor = dto.ComisionExceso.Value }, transaction);
            }

            transaction.Commit();
            return numeroLinea;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int rutCliente, int numeroLinea, UpdateLineaCreditoDto dto, string usuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            Update LCR SET
                LCR_FEC_RES = ISNULL(@FechaResolucion, LCR_FEC_RES),
                LCR_FVI_DES = ISNULL(@VigenciaDesde, LCR_FVI_DES),
                LCR_FVI_HAS = ISNULL(@VigenciaHasta, LCR_FVI_HAS),
                LCR_EST_SOL = ISNULL(@EstadoSolicitud, LCR_EST_SOL),
                LCR_TAS_NEG = ISNULL(@TasaNegocio, LCR_TAS_NEG),
                LCR_SPR_EAD = ISNULL(@Spread, LCR_SPR_EAD),
                LCR_MON_SOL = ISNULL(@MontoSolicitado, LCR_MON_SOL),
                LCR_MON_APR = ISNULL(@MontoAprobado, LCR_MON_APR),
                LCR_MON_DIS = ISNULL(@MontoDisponible, LCR_MON_DIS),
                LCR_EST_LIN = ISNULL(@EstadoLinea, LCR_EST_LIN),
                LCR_NUM_CON = ISNULL(@NumeroContrato, LCR_NUM_CON),
                LCR_TIP_CON = ISNULL(@TipoContrato, LCR_TIP_CON),
                LCR_FEC_CON = ISNULL(@FechaContrato, LCR_FEC_CON),
                LCR_VTO_CON = ISNULL(@VencimientoContrato, LCR_VTO_CON),
                LCR_EST_CON = ISNULL(@EstadoContrato, LCR_EST_CON),
                LCR_OBS_CON = ISNULL(@ObservacionesContrato, LCR_OBS_CON),
                LCR_COD_NOT = ISNULL(@CodigoNotaria, LCR_COD_NOT),
                LCR_USR_MOD = @Usuario,
                LCR_FEC_MOD = @FechaModificacion,
                LCR_HOR_MOD = @HoraModificacion,
                LCR_TAS_MOR = ISNULL(@TasaMora, LCR_TAS_MOR),
                LCR_DES_MOR = ISNULL(@DiasMora, LCR_DES_MOR),
                LCR_CAL_MOR = ISNULL(@CalculoMora, LCR_CAL_MOR)
            WHERE LCR_RUT_CLI = @RutCliente AND LCR_NUM_LIN = @NumeroLinea";

        var rows = await connection.ExecuteAsync(sql, new
        {
            RutCliente = rutCliente,
            NumeroLinea = numeroLinea,
            dto.FechaResolucion,
            dto.VigenciaDesde,
            dto.VigenciaHasta,
            dto.EstadoSolicitud,
            dto.TasaNegocio,
            dto.Spread,
            dto.MontoSolicitado,
            dto.MontoAprobado,
            dto.MontoDisponible,
            dto.EstadoLinea,
            dto.NumeroContrato,
            dto.TipoContrato,
            dto.FechaContrato,
            dto.VencimientoContrato,
            dto.EstadoContrato,
            dto.ObservacionesContrato,
            dto.CodigoNotaria,
            Usuario = usuario,
            FechaModificacion = DateTime.Today,
            HoraModificacion = DateTime.Now.ToString("HH:mm"),
            dto.TasaMora,
            dto.DiasMora,
            dto.CalculoMora
        });

        return rows > 0;
    }

    public async Task<int> GetNextNumeroLineaAsync(int rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = "SELECT ISNULL(MAX(LCR_NUM_LIN), 0) + 1 FROM LCR WHERE LCR_RUT_CLI = @RutCliente";
        return await connection.ExecuteScalarAsync<int>(sql, new { RutCliente = rutCliente });
    }

    public async Task<int> CountLineasCreadasAsync(int rutCliente, int tipoMoneda)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = "SELECT COUNT(*) FROM LCR WHERE LCR_RUT_CLI = @RutCliente AND LCR_TIP_MON = @TipoMoneda AND LCR_EST_LIN = 1";
        return await connection.ExecuteScalarAsync<int>(sql, new { RutCliente = rutCliente, TipoMoneda = tipoMoneda });
    }

    public async Task<bool> ProcesarAccionAsync(AccionLineaCreditoDto dto, string usuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            int nuevoEstadoSolicitud = 0;
            int nuevoEstadoLinea = 0;

            switch (dto.Accion.ToLower())
            {
                case "aprobar":
                    nuevoEstadoSolicitud = 2; // Aprobada
                    nuevoEstadoLinea = 3;     // Aprobada
                    break;
                case "rechazar":
                    nuevoEstadoSolicitud = 3; // Rechazada
                    nuevoEstadoLinea = 1;     // Creada
                    break;
                case "suspender":
                    nuevoEstadoLinea = 5;     // Suspendida
                    break;
                case "renovar":
                    nuevoEstadoLinea = 2;     // Vigente
                    break;
                default:
                    return false;
            }

            var sql = @"
                Update LCR SET
                    LCR_EST_SOL = CASE WHEN @NuevoEstadoSolicitud > 0 THEN @NuevoEstadoSolicitud ELSE LCR_EST_SOL END,
                    LCR_EST_LIN = @NuevoEstadoLinea,
                    LCR_MON_APR = ISNULL(@MontoAprobado, LCR_MON_APR),
                    LCR_MON_DIS = ISNULL(@MontoAprobado, LCR_MON_DIS),
                    LCR_FVI_DES = ISNULL(@VigenciaDesde, LCR_FVI_DES),
                    LCR_FVI_HAS = ISNULL(@VigenciaHasta, LCR_FVI_HAS),
                    LCR_OBS_CON = ISNULL(@Observaciones, LCR_OBS_CON),
                    LCR_USR_MOD = @Usuario,
                    LCR_FEC_MOD = @FechaModificacion,
                    LCR_HOR_MOD = @HoraModificacion
                WHERE LCR_RUT_CLI = @RutCliente AND LCR_NUM_LIN = @NumeroLinea";

            await connection.ExecuteAsync(sql, new
            {
                dto.RutCliente,
                dto.NumeroLinea,
                NuevoEstadoSolicitud = nuevoEstadoSolicitud,
                NuevoEstadoLinea = nuevoEstadoLinea,
                dto.MontoAprobado,
                dto.VigenciaDesde,
                dto.VigenciaHasta,
                dto.Observaciones,
                Usuario = usuario,
                FechaModificacion = DateTime.Today,
                HoraModificacion = DateTime.Now.ToString("HH:mm")
            }, transaction);

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<ResumenEndeudamientoDto?> GetResumenEndeudamientoAsync(int rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                L.LCR_RUT_CLI AS RutCliente,
                ISNULL(C.CLI_RAZ_SOC, C.CLI_APE_PAT + ' ' + ISNULL(C.CLI_APE_MAT, '')) AS NombreCliente,
                SUM(L.LCR_MON_SOL) AS TotalSolicitado,
                SUM(L.LCR_MON_APR) AS TotalAprobado,
                SUM(L.LCR_MON_DIS) AS TotalDisponible,
                SUM(L.LCR_MON_APR - L.LCR_MON_DIS) AS TotalUtilizado,
                CASE 
                    WHEN SUM(L.LCR_MON_APR) > 0 
                    THEN ROUND((SUM(L.LCR_MON_APR - L.LCR_MON_DIS) / SUM(L.LCR_MON_APR)) * 100, 2) 
                    ELSE 0 
                END AS PorcentajeUtilizado,
                COUNT(*) AS CantidadLineas,
                SUM(CASE WHEN L.LCR_EST_LIN = 2 THEN 1 ELSE 0 END) AS LineasVigentes,
                SUM(CASE WHEN L.LCR_EST_LIN = 4 THEN 1 ELSE 0 END) AS LineasVencidas,
                PR.PAR_DES_PAR AS CategoriaRiesgo
            FROM LCR L
            INNER JOIN CLI C ON C.CLI_RUT_CLI = L.LCR_RUT_CLI
            LEFT JOIN PAR PR ON PR.PAR_COD_TAB = 60 AND PR.PAR_COD_PAR = C.CLI_CAT_RIE
            WHERE L.LCR_RUT_CLI = @RutCliente
            GROUP BY L.LCR_RUT_CLI, C.CLI_RAZ_SOC, C.CLI_APE_PAT, C.CLI_APE_MAT, PR.PAR_DES_PAR";

        return await connection.QueryFirstOrDefaultAsync<ResumenEndeudamientoDto>(sql, new { RutCliente = rutCliente });
    }

    public async Task<IEnumerable<ConsultaLineaCreditoDto>> GetLineasPorVencerAsync(int dias = 30)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                L.LCR_NUM_LIN AS NumeroLinea,
                L.LCR_RUT_CLI AS RutCliente,
                ISNULL(C.CLI_RAZ_SOC, C.CLI_APE_PAT + ' ' + ISNULL(C.CLI_APE_MAT, '')) AS NombreCliente,
                L.LCR_FEC_SOL AS FechaSolicitud,
                L.LCR_FVI_DES AS VigenciaDesde,
                L.LCR_FVI_HAS AS VigenciaHasta,
                PS.PAR_DES_PAR AS EstadoSolicitud,
                L.LCR_SPR_EAD AS Spread,
                L.LCR_MON_SOL AS MontoSolicitado,
                L.LCR_MON_APR AS MontoAprobado,
                PL.PAR_DES_PAR AS EstadoLinea,
                CASE L.LCR_TIP_MON WHEN 1 THEN 'Peso' WHEN 2 THEN 'Dólar' WHEN 3 THEN 'UF' END AS TipoMoneda,
                PR.PAR_DES_PAR AS CategoriaRiesgo,
                0 AS EstaVencida,
                DATEDIFF(DAY, GETDATE(), L.LCR_FVI_HAS) AS DiasParaVencer
            FROM LCR L
            INNER JOIN CLI C ON C.CLI_RUT_CLI = L.LCR_RUT_CLI
            LEFT JOIN PAR PS ON PS.PAR_COD_TAB = 15 AND PS.PAR_COD_PAR = L.LCR_EST_SOL
            LEFT JOIN PAR PL ON PL.PAR_COD_TAB = 16 AND PL.PAR_COD_PAR = L.LCR_EST_LIN
            LEFT JOIN PAR PR ON PR.PAR_COD_TAB = 60 AND PR.PAR_COD_PAR = C.CLI_CAT_RIE
            WHERE L.LCR_EST_LIN IN (2, 3)
              AND L.LCR_FVI_HAS IS NOT NULL
              AND L.LCR_FVI_HAS BETWEEN GETDATE() AND DATEADD(DAY, @Dias, GETDATE())
            ORDER BY L.LCR_FVI_HAS";

        return await connection.QueryAsync<ConsultaLineaCreditoDto>(sql, new { Dias = dias });
    }

    public async Task<IEnumerable<ConsultaLineaCreditoDto>> GetLineasVencidasAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                L.LCR_NUM_LIN AS NumeroLinea,
                L.LCR_RUT_CLI AS RutCliente,
                ISNULL(C.CLI_RAZ_SOC, C.CLI_APE_PAT + ' ' + ISNULL(C.CLI_APE_MAT, '')) AS NombreCliente,
                L.LCR_FEC_SOL AS FechaSolicitud,
                L.LCR_FVI_DES AS VigenciaDesde,
                L.LCR_FVI_HAS AS VigenciaHasta,
                PS.PAR_DES_PAR AS EstadoSolicitud,
                L.LCR_SPR_EAD AS Spread,
                L.LCR_MON_SOL AS MontoSolicitado,
                L.LCR_MON_APR AS MontoAprobado,
                PL.PAR_DES_PAR AS EstadoLinea,
                CASE L.LCR_TIP_MON WHEN 1 THEN 'Peso' WHEN 2 THEN 'Dólar' WHEN 3 THEN 'UF' END AS TipoMoneda,
                PR.PAR_DES_PAR AS CategoriaRiesgo,
                1 AS EstaVencida,
                DATEDIFF(DAY, L.LCR_FVI_HAS, GETDATE()) AS DiasParaVencer
            FROM LCR L
            INNER JOIN CLI C ON C.CLI_RUT_CLI = L.LCR_RUT_CLI
            LEFT JOIN PAR PS ON PS.PAR_COD_TAB = 15 AND PS.PAR_COD_PAR = L.LCR_EST_SOL
            LEFT JOIN PAR PL ON PL.PAR_COD_TAB = 16 AND PL.PAR_COD_PAR = L.LCR_EST_LIN
            LEFT JOIN PAR PR ON PR.PAR_COD_TAB = 60 AND PR.PAR_COD_PAR = C.CLI_CAT_RIE
            WHERE L.LCR_FVI_HAS IS NOT NULL
              AND L.LCR_FVI_HAS < GETDATE()
              AND L.LCR_EST_LIN NOT IN (4, 5)
            ORDER BY L.LCR_FVI_HAS";

        return await connection.QueryAsync<ConsultaLineaCreditoDto>(sql);
    }
}

/// <summary>
/// Implementación del repositorio de Sublíneas por Producto
/// </summary>
public class SublineaProductoRepository : ISublineaProductoRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public SublineaProductoRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<SublineaProductoDto>> GetByLineaAsync(int rutCliente, int numeroLinea)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                A.ANT_RUT_CLI AS RutCliente,
                A.ANT_NUM_LIN AS NumeroLinea,
                A.ANT_TIP_DOC AS TipoDocumento,
                PD.PAR_DES_PAR AS DescripcionTipoDocumento,
                A.ANT_TIP_MON AS TipoMoneda,
                PM.PAR_DES_PAR AS DescripcionMoneda,
                A.ANT_POR_ANT AS PorcentajeAnticipo,
                A.ANT_MON_SOL AS MontoSolicitado,
                A.ANT_MON_APR AS MontoAprobado,
                A.ANT_MON_DIS AS MontoDisponible,
                A.ANT_NUM_CON AS NumeroContrato,
                A.ANT_FEC_CON AS FechaContrato,
                A.ANT_VTO_CON AS VencimientoContrato,
                A.ANT_EST_CON AS EstadoContrato,
                PC.PAR_DES_PAR AS DescripcionEstadoContrato,
                A.ANT_PLA_VTO AS PlazoVencimiento,
                A.ANT_PLA_CES AS PlazoCesion,
                A.ANT_COM_MIN AS ComisionMinima,
                A.ANT_COM_MAX AS ComisionMaxima,
                A.ANT_COM_POR AS ComisionPorcentual
            FROM ANT A
            LEFT JOIN PAR PD ON PD.PAR_COD_TAB = 1 AND PD.PAR_COD_PAR = A.ANT_TIP_DOC
            LEFT JOIN PAR PM ON PM.PAR_COD_TAB = 2 AND PM.PAR_COD_PAR = A.ANT_TIP_MON
            LEFT JOIN PAR PC ON PC.PAR_COD_TAB = 17 AND PC.PAR_COD_PAR = A.ANT_EST_CON
            WHERE A.ANT_RUT_CLI = @RutCliente AND A.ANT_NUM_LIN = @NumeroLinea
            ORDER BY A.ANT_TIP_DOC";

        return await connection.QueryAsync<SublineaProductoDto>(sql, new { RutCliente = rutCliente, NumeroLinea = numeroLinea });
    }

    public async Task<SublineaProductoDto?> GetByIdAsync(int rutCliente, int numeroLinea, int tipoDocumento)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                A.ANT_RUT_CLI AS RutCliente,
                A.ANT_NUM_LIN AS NumeroLinea,
                A.ANT_TIP_DOC AS TipoDocumento,
                PD.PAR_DES_PAR AS DescripcionTipoDocumento,
                A.ANT_TIP_MON AS TipoMoneda,
                PM.PAR_DES_PAR AS DescripcionMoneda,
                A.ANT_POR_ANT AS PorcentajeAnticipo,
                A.ANT_MON_SOL AS MontoSolicitado,
                A.ANT_MON_APR AS MontoAprobado,
                A.ANT_MON_DIS AS MontoDisponible,
                A.ANT_NUM_CON AS NumeroContrato,
                A.ANT_FEC_CON AS FechaContrato,
                A.ANT_VTO_CON AS VencimientoContrato,
                A.ANT_EST_CON AS EstadoContrato,
                PC.PAR_DES_PAR AS DescripcionEstadoContrato,
                A.ANT_PLA_VTO AS PlazoVencimiento,
                A.ANT_PLA_CES AS PlazoCesion,
                A.ANT_COM_MIN AS ComisionMinima,
                A.ANT_COM_MAX AS ComisionMaxima,
                A.ANT_COM_POR AS ComisionPorcentual
            FROM ANT A
            LEFT JOIN PAR PD ON PD.PAR_COD_TAB = 1 AND PD.PAR_COD_PAR = A.ANT_TIP_DOC
            LEFT JOIN PAR PM ON PM.PAR_COD_TAB = 2 AND PM.PAR_COD_PAR = A.ANT_TIP_MON
            LEFT JOIN PAR PC ON PC.PAR_COD_TAB = 17 AND PC.PAR_COD_PAR = A.ANT_EST_CON
            WHERE A.ANT_RUT_CLI = @RutCliente 
              AND A.ANT_NUM_LIN = @NumeroLinea 
              AND A.ANT_TIP_DOC = @TipoDocumento";

        return await connection.QueryFirstOrDefaultAsync<SublineaProductoDto>(sql, 
            new { RutCliente = rutCliente, NumeroLinea = numeroLinea, TipoDocumento = tipoDocumento });
    }

    public async Task<bool> CreateAsync(CreateSublineaProductoDto dto)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            INSERT INTO ANT (
                ANT_RUT_CLI, ANT_NUM_LIN, ANT_TIP_DOC, ANT_TIP_MON, ANT_POR_ANT,
                ANT_MON_SOL, ANT_MON_APR, ANT_MON_DIS,
                ANT_NUM_CON, ANT_FEC_CON, ANT_VTO_CON, ANT_EST_CON,
                ANT_PLA_VTO, ANT_PLA_CES, ANT_COM_MIN, ANT_COM_MAX, ANT_COM_POR
            ) VALUES (
                @RutCliente, @NumeroLinea, @TipoDocumento, @TipoMoneda, @PorcentajeAnticipo,
                @MontoSolicitado, @MontoAprobado, @MontoAprobado,
                @NumeroContrato, @FechaContrato, @VencimientoContrato, @EstadoContrato,
                @PlazoVencimiento, @PlazoCesion, @ComisionMinima, @ComisionMaxima, @ComisionPorcentual
            )";

        var rows = await connection.ExecuteAsync(sql, dto);
        return rows > 0;
    }

    public async Task<bool> UpdateAsync(int rutCliente, int numeroLinea, int tipoDocumento, CreateSublineaProductoDto dto)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            Update ANT SET
                ANT_TIP_MON = @TipoMoneda,
                ANT_POR_ANT = @PorcentajeAnticipo,
                ANT_MON_SOL = @MontoSolicitado,
                ANT_MON_APR = @MontoAprobado,
                ANT_NUM_CON = @NumeroContrato,
                ANT_FEC_CON = @FechaContrato,
                ANT_VTO_CON = @VencimientoContrato,
                ANT_EST_CON = @EstadoContrato,
                ANT_PLA_VTO = @PlazoVencimiento,
                ANT_PLA_CES = @PlazoCesion,
                ANT_COM_MIN = @ComisionMinima,
                ANT_COM_MAX = @ComisionMaxima,
                ANT_COM_POR = @ComisionPorcentual
            WHERE ANT_RUT_CLI = @RutCliente 
              AND ANT_NUM_LIN = @NumeroLinea 
              AND ANT_TIP_DOC = @TipoDocumento";

        var rows = await connection.ExecuteAsync(sql, new
        {
            RutCliente = rutCliente,
            NumeroLinea = numeroLinea,
            TipoDocumento = tipoDocumento,
            dto.TipoMoneda,
            dto.PorcentajeAnticipo,
            dto.MontoSolicitado,
            dto.MontoAprobado,
            dto.NumeroContrato,
            dto.FechaContrato,
            dto.VencimientoContrato,
            dto.EstadoContrato,
            dto.PlazoVencimiento,
            dto.PlazoCesion,
            dto.ComisionMinima,
            dto.ComisionMaxima,
            dto.ComisionPorcentual
        });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int rutCliente, int numeroLinea, int tipoDocumento)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = "DELETE FROM ANT WHERE ANT_RUT_CLI = @RutCliente AND ANT_NUM_LIN = @NumeroLinea AND ANT_TIP_DOC = @TipoDocumento";
        var rows = await connection.ExecuteAsync(sql, new { RutCliente = rutCliente, NumeroLinea = numeroLinea, TipoDocumento = tipoDocumento });
        return rows > 0;
    }

    public async Task<ResumenSublineasDto> GetResumenAsync(int rutCliente, int numeroLinea)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                @RutCliente AS RutCliente,
                @NumeroLinea AS NumeroLinea,
                COUNT(*) AS CantidadSublineasProducto,
                SUM(ANT_MON_APR) AS TotalMontoProducto,
                SUM(ANT_MON_DIS) AS TotalDisponibleProducto,
                0 AS CantidadSublineasDeudor,
                0 AS TotalMontoDeudor,
                0 AS TotalDisponibleDeudor,
                0 AS MaximoCreditoDeudor
            FROM ANT
            WHERE ANT_RUT_CLI = @RutCliente AND ANT_NUM_LIN = @NumeroLinea";

        var resultado = await connection.QueryFirstOrDefaultAsync<ResumenSublineasDto>(sql, 
            new { RutCliente = rutCliente, NumeroLinea = numeroLinea });
        
        return resultado ?? new ResumenSublineasDto { RutCliente = rutCliente, NumeroLinea = numeroLinea };
    }
}

/// <summary>
/// Implementación del repositorio de Sublíneas por Deudor
/// </summary>
public class SublineaDeudorRepository : ISublineaDeudorRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public SublineaDeudorRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<SublineaDeudorDto>> GetByLineaAsync(int rutCliente, int numeroLinea)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                S.SUB_RUT_CLI AS RutCliente,
                S.SUB_NUM_LIN AS NumeroLinea,
                S.SUB_RUT_DEU AS RutDeudor,
                ISNULL(D.DEU_RAZ_SOC, D.DEU_APE_PAT + ' ' + ISNULL(D.DEU_APE_MAT, '')) AS NombreDeudor,
                S.SUB_TIP_DOC AS TipoDocumento,
                PD.PAR_DES_PAR AS DescripcionTipoDocumento,
                S.SUB_TIP_MON AS TipoMoneda,
                PM.PAR_DES_PAR AS DescripcionMoneda,
                S.SUB_POR_ANT AS PorcentajeAnticipo,
                S.SUB_MON_SOL AS MontoSolicitado,
                S.SUB_MON_APR AS MontoAprobado,
                S.SUB_MON_DIS AS MontoDisponible,
                S.SUB_FEC_SOL AS FechaSolicitud,
                S.SUB_FEC_RES AS FechaResolucion,
                S.SUB_FEC_VTO AS FechaVencimiento,
                S.SUB_EST_LIN AS EstadoLinea,
                PE.PAR_DES_PAR AS DescripcionEstado,
                S.SUB_VERIF AS Verificado
            FROM SUB S
            INNER JOIN DEU D ON D.DEU_RUT_DEU = S.SUB_RUT_DEU AND D.DEU_RUT_CLI = S.SUB_RUT_CLI
            LEFT JOIN PAR PD ON PD.PAR_COD_TAB = 1 AND PD.PAR_COD_PAR = S.SUB_TIP_DOC
            LEFT JOIN PAR PM ON PM.PAR_COD_TAB = 2 AND PM.PAR_COD_PAR = S.SUB_TIP_MON
            LEFT JOIN PAR PE ON PE.PAR_COD_TAB = 16 AND PE.PAR_COD_PAR = S.SUB_EST_LIN
            WHERE S.SUB_RUT_CLI = @RutCliente AND S.SUB_NUM_LIN = @NumeroLinea
            ORDER BY S.SUB_RUT_DEU";

        return await connection.QueryAsync<SublineaDeudorDto>(sql, new { RutCliente = rutCliente, NumeroLinea = numeroLinea });
    }

    public async Task<SublineaDeudorDto?> GetByIdAsync(int rutCliente, int numeroLinea, int rutDeudor, int tipoDocumento)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                S.SUB_RUT_CLI AS RutCliente,
                S.SUB_NUM_LIN AS NumeroLinea,
                S.SUB_RUT_DEU AS RutDeudor,
                ISNULL(D.DEU_RAZ_SOC, D.DEU_APE_PAT + ' ' + ISNULL(D.DEU_APE_MAT, '')) AS NombreDeudor,
                S.SUB_TIP_DOC AS TipoDocumento,
                PD.PAR_DES_PAR AS DescripcionTipoDocumento,
                S.SUB_TIP_MON AS TipoMoneda,
                PM.PAR_DES_PAR AS DescripcionMoneda,
                S.SUB_POR_ANT AS PorcentajeAnticipo,
                S.SUB_MON_SOL AS MontoSolicitado,
                S.SUB_MON_APR AS MontoAprobado,
                S.SUB_MON_DIS AS MontoDisponible,
                S.SUB_FEC_SOL AS FechaSolicitud,
                S.SUB_FEC_RES AS FechaResolucion,
                S.SUB_FEC_VTO AS FechaVencimiento,
                S.SUB_EST_LIN AS EstadoLinea,
                PE.PAR_DES_PAR AS DescripcionEstado,
                S.SUB_VERIF AS Verificado
            FROM SUB S
            INNER JOIN DEU D ON D.DEU_RUT_DEU = S.SUB_RUT_DEU AND D.DEU_RUT_CLI = S.SUB_RUT_CLI
            LEFT JOIN PAR PD ON PD.PAR_COD_TAB = 1 AND PD.PAR_COD_PAR = S.SUB_TIP_DOC
            LEFT JOIN PAR PM ON PM.PAR_COD_TAB = 2 AND PM.PAR_COD_PAR = S.SUB_TIP_MON
            LEFT JOIN PAR PE ON PE.PAR_COD_TAB = 16 AND PE.PAR_COD_PAR = S.SUB_EST_LIN
            WHERE S.SUB_RUT_CLI = @RutCliente 
              AND S.SUB_NUM_LIN = @NumeroLinea 
              AND S.SUB_RUT_DEU = @RutDeudor
              AND S.SUB_TIP_DOC = @TipoDocumento";

        return await connection.QueryFirstOrDefaultAsync<SublineaDeudorDto>(sql, 
            new { RutCliente = rutCliente, NumeroLinea = numeroLinea, RutDeudor = rutDeudor, TipoDocumento = tipoDocumento });
    }

    public async Task<bool> CreateAsync(CreateSublineaDeudorDto dto)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            INSERT INTO SUB (
                SUB_RUT_CLI, SUB_NUM_LIN, SUB_RUT_DEU, SUB_TIP_DOC, SUB_TIP_MON,
                SUB_POR_ANT, SUB_MON_SOL, SUB_MON_APR, SUB_MON_DIS,
                SUB_FEC_SOL, SUB_FEC_RES, SUB_FEC_VTO, SUB_EST_LIN, SUB_VERIF
            ) VALUES (
                @RutCliente, @NumeroLinea, @RutDeudor, @TipoDocumento, @TipoMoneda,
                @PorcentajeAnticipo, @MontoSolicitado, 0, 0,
                @FechaSolicitud, NULL, NULL, @EstadoLinea, @Verificado
            )";

        var rows = await connection.ExecuteAsync(sql, dto);
        return rows > 0;
    }

    public async Task<bool> UpdateAsync(CreateSublineaDeudorDto dto)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            Update SUB SET
                SUB_TIP_MON = @TipoMoneda,
                SUB_POR_ANT = @PorcentajeAnticipo,
                SUB_MON_SOL = @MontoSolicitado,
                SUB_EST_LIN = @EstadoLinea,
                SUB_VERIF = @Verificado
            WHERE SUB_RUT_CLI = @RutCliente 
              AND SUB_NUM_LIN = @NumeroLinea 
              AND SUB_RUT_DEU = @RutDeudor
              AND SUB_TIP_DOC = @TipoDocumento";

        var rows = await connection.ExecuteAsync(sql, dto);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int rutCliente, int numeroLinea, int rutDeudor, int tipoDocumento)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"DELETE FROM SUB 
                    WHERE SUB_RUT_CLI = @RutCliente 
                      AND SUB_NUM_LIN = @NumeroLinea 
                      AND SUB_RUT_DEU = @RutDeudor
                      AND SUB_TIP_DOC = @TipoDocumento";
        
        var rows = await connection.ExecuteAsync(sql, 
            new { RutCliente = rutCliente, NumeroLinea = numeroLinea, RutDeudor = rutDeudor, TipoDocumento = tipoDocumento });
        return rows > 0;
    }

    public async Task<IEnumerable<SublineaDeudorDto>> GetByDeudorAsync(int rutDeudor)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                S.SUB_RUT_CLI AS RutCliente,
                S.SUB_NUM_LIN AS NumeroLinea,
                S.SUB_RUT_DEU AS RutDeudor,
                ISNULL(D.DEU_RAZ_SOC, D.DEU_APE_PAT + ' ' + ISNULL(D.DEU_APE_MAT, '')) AS NombreDeudor,
                S.SUB_TIP_DOC AS TipoDocumento,
                PD.PAR_DES_PAR AS DescripcionTipoDocumento,
                S.SUB_TIP_MON AS TipoMoneda,
                S.SUB_POR_ANT AS PorcentajeAnticipo,
                S.SUB_MON_SOL AS MontoSolicitado,
                S.SUB_MON_APR AS MontoAprobado,
                S.SUB_MON_DIS AS MontoDisponible,
                S.SUB_FEC_SOL AS FechaSolicitud,
                S.SUB_EST_LIN AS EstadoLinea,
                PE.PAR_DES_PAR AS DescripcionEstado
            FROM SUB S
            INNER JOIN DEU D ON D.DEU_RUT_DEU = S.SUB_RUT_DEU AND D.DEU_RUT_CLI = S.SUB_RUT_CLI
            LEFT JOIN PAR PD ON PD.PAR_COD_TAB = 1 AND PD.PAR_COD_PAR = S.SUB_TIP_DOC
            LEFT JOIN PAR PE ON PE.PAR_COD_TAB = 16 AND PE.PAR_COD_PAR = S.SUB_EST_LIN
            WHERE S.SUB_RUT_DEU = @RutDeudor
            ORDER BY S.SUB_RUT_CLI";

        return await connection.QueryAsync<SublineaDeudorDto>(sql, new { RutDeudor = rutDeudor });
    }
}

/// <summary>
/// Implementación del repositorio de Notarías
/// </summary>
public class NotariaRepository : INotariaRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public NotariaRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<NotariaDto>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                NTA_COD_NOT AS Codigo,
                NTA_NOM_NOT AS Nombre,
                NTA_DES_NOT AS Descripcion
            FROM NTA
            ORDER BY NTA_NOM_NOT";

        return await connection.QueryAsync<NotariaDto>(sql);
    }

    public async Task<NotariaDto?> GetByIdAsync(int codigo)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                NTA_COD_NOT AS Codigo,
                NTA_NOM_NOT AS Nombre,
                NTA_DES_NOT AS Descripcion
            FROM NTA
            WHERE NTA_COD_NOT = @Codigo";

        return await connection.QueryFirstOrDefaultAsync<NotariaDto>(sql, new { Codigo = codigo });
    }
}
