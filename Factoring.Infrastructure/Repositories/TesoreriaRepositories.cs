using Dapper;
using Factoring.Core.Entities;
using Factoring.Core.DTOs;
using Factoring.Core.Interfaces;
using Factoring.Infrastructure.Data;

namespace Factoring.Infrastructure.Repositories;

/// <summary>
/// Repositorio de Giros - Migrado de F_TS1000.frm
/// </summary>
public class GiroRepository : IGiroRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public GiroRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Giro>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT g.GIR_NUM_GIR AS NumeroGiro, g.GIR_RUT_CLI AS RutCliente,
                   g.GIR_NUM_OPE AS NumeroOperacion, g.GIR_TIP_GIR AS TipoGiro,
                   g.GIR_FEC_GIR AS FechaGiro, g.GIR_MON_GIR AS MontoGiro,
                   g.GIR_CTA_CTE AS CuentaCorriente, g.GIR_COD_BCO AS CodigoBanco,
                   g.GIR_BEN_GIR AS Beneficiario, g.GIR_EST_GIR AS Estado,
                   g.GIR_FOR_CAN AS FormaCancelacion, g.GIR_USR_CRE AS UsuarioCreacion,
                   g.GIR_FEC_CRE AS FechaCreacion,
                   c.CLI_RAZ_SOC AS NombreCliente,
                   b.BCO_DES_BCO AS NombreBanco,
                   CASE g.GIR_TIP_GIR 
                       WHEN 1 THEN 'Adelanto'
                       WHEN 2 THEN 'Otorgamiento'
                       WHEN 3 THEN 'Otorgamiento 2'
                       ELSE 'Otro'
                   END AS DescripcionTipoGiro,
                   CASE g.GIR_EST_GIR 
                       WHEN 1 THEN 'Digitado'
                       WHEN 2 THEN 'Emitido'
                       WHEN 3 THEN 'Anulado'
                       ELSE 'Desconocido'
                   END AS DescripcionEstado
            FROM GIR g
            LEFT JOIN CLI c ON g.GIR_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN BCO b ON g.GIR_COD_BCO = b.BCO_COD_BCO
            ORDER BY g.GIR_FEC_GIR DESC, g.GIR_NUM_GIR DESC";
        return await connection.QueryAsync<Giro>(sql);
    }

    public async Task<IEnumerable<Giro>> GetByFechaAsync(DateTime? fechaDesde, DateTime? fechaHasta)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT g.GIR_NUM_GIR AS NumeroGiro, g.GIR_RUT_CLI AS RutCliente,
                   g.GIR_NUM_OPE AS NumeroOperacion, g.GIR_TIP_GIR AS TipoGiro,
                   g.GIR_FEC_GIR AS FechaGiro, g.GIR_MON_GIR AS MontoGiro,
                   g.GIR_CTA_CTE AS CuentaCorriente, g.GIR_COD_BCO AS CodigoBanco,
                   g.GIR_BEN_GIR AS Beneficiario, g.GIR_EST_GIR AS Estado,
                   g.GIR_FOR_CAN AS FormaCancelacion, g.GIR_USR_CRE AS UsuarioCreacion,
                   g.GIR_FEC_CRE AS FechaCreacion,
                   c.CLI_RAZ_SOC AS NombreCliente,
                   b.BCO_DES_BCO AS NombreBanco,
                   CASE g.GIR_TIP_GIR 
                       WHEN 1 THEN 'Adelanto'
                       WHEN 2 THEN 'Otorgamiento'
                       WHEN 3 THEN 'Otorgamiento 2'
                       ELSE 'Otro'
                   END AS DescripcionTipoGiro,
                   CASE g.GIR_EST_GIR 
                       WHEN 1 THEN 'Digitado'
                       WHEN 2 THEN 'Emitido'
                       WHEN 3 THEN 'Anulado'
                       ELSE 'Desconocido'
                   END AS DescripcionEstado
            FROM GIR g
            LEFT JOIN CLI c ON g.GIR_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN BCO b ON g.GIR_COD_BCO = b.BCO_COD_BCO
            WHERE g.GIR_FEC_GIR BETWEEN @FechaDesde AND @FechaHasta
            ORDER BY g.GIR_FEC_GIR DESC, g.GIR_NUM_GIR DESC";
        return await connection.QueryAsync<Giro>(sql, new { FechaDesde = fechaDesde, FechaHasta = fechaHasta });
    }

    public async Task<IEnumerable<Giro>> GetByClienteAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT g.GIR_NUM_GIR AS NumeroGiro, g.GIR_RUT_CLI AS RutCliente,
                   g.GIR_NUM_OPE AS NumeroOperacion, g.GIR_TIP_GIR AS TipoGiro,
                   g.GIR_FEC_GIR AS FechaGiro, g.GIR_MON_GIR AS MontoGiro,
                   g.GIR_CTA_CTE AS CuentaCorriente, g.GIR_COD_BCO AS CodigoBanco,
                   g.GIR_BEN_GIR AS Beneficiario, g.GIR_EST_GIR AS Estado,
                   g.GIR_FOR_CAN AS FormaCancelacion, g.GIR_USR_CRE AS UsuarioCreacion,
                   g.GIR_FEC_CRE AS FechaCreacion,
                   c.CLI_RAZ_SOC AS NombreCliente,
                   b.BCO_DES_BCO AS NombreBanco
            FROM GIR g
            LEFT JOIN CLI c ON g.GIR_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN BCO b ON g.GIR_COD_BCO = b.BCO_COD_BCO
            WHERE g.GIR_RUT_CLI = @RutCliente
            ORDER BY g.GIR_FEC_GIR DESC";
        return await connection.QueryAsync<Giro>(sql, new { RutCliente = rutCliente });
    }

    public async Task<IEnumerable<Giro>> GetByOperacionAsync(int numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT g.GIR_NUM_GIR AS NumeroGiro, g.GIR_RUT_CLI AS RutCliente,
                   g.GIR_NUM_OPE AS NumeroOperacion, g.GIR_TIP_GIR AS TipoGiro,
                   g.GIR_FEC_GIR AS FechaGiro, g.GIR_MON_GIR AS MontoGiro,
                   g.GIR_CTA_CTE AS CuentaCorriente, g.GIR_COD_BCO AS CodigoBanco,
                   g.GIR_BEN_GIR AS Beneficiario, g.GIR_EST_GIR AS Estado,
                   g.GIR_FOR_CAN AS FormaCancelacion,
                   c.CLI_RAZ_SOC AS NombreCliente,
                   b.BCO_DES_BCO AS NombreBanco
            FROM GIR g
            LEFT JOIN CLI c ON g.GIR_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN BCO b ON g.GIR_COD_BCO = b.BCO_COD_BCO
            WHERE g.GIR_NUM_OPE = @NumeroOperacion
            ORDER BY g.GIR_NUM_GIR";
        return await connection.QueryAsync<Giro>(sql, new { NumeroOperacion = numeroOperacion });
    }

    public async Task<IEnumerable<Giro>> GetByFiltroAsync(FiltroTesoreriaDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT g.GIR_NUM_GIR AS NumeroGiro, g.GIR_RUT_CLI AS RutCliente,
                   g.GIR_NUM_OPE AS NumeroOperacion, g.GIR_TIP_GIR AS TipoGiro,
                   g.GIR_FEC_GIR AS FechaGiro, g.GIR_MON_GIR AS MontoGiro,
                   g.GIR_CTA_CTE AS CuentaCorriente, g.GIR_COD_BCO AS CodigoBanco,
                   g.GIR_BEN_GIR AS Beneficiario, g.GIR_EST_GIR AS Estado,
                   g.GIR_FOR_CAN AS FormaCancelacion, g.GIR_USR_CRE AS UsuarioCreacion,
                   g.GIR_FEC_CRE AS FechaCreacion,
                   c.CLI_RAZ_SOC AS NombreCliente,
                   b.BCO_DES_BCO AS NombreBanco,
                   CASE g.GIR_TIP_GIR 
                       WHEN 1 THEN 'Adelanto'
                       WHEN 2 THEN 'Otorgamiento'
                       WHEN 3 THEN 'Otorgamiento 2'
                       ELSE 'Otro'
                   END AS DescripcionTipoGiro,
                   CASE g.GIR_EST_GIR 
                       WHEN 1 THEN 'Digitado'
                       WHEN 2 THEN 'Emitido'
                       WHEN 3 THEN 'Anulado'
                       ELSE 'Desconocido'
                   END AS DescripcionEstado
            FROM GIR g
            LEFT JOIN CLI c ON g.GIR_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN BCO b ON g.GIR_COD_BCO = b.BCO_COD_BCO
            WHERE 1=1";

        if (filtro.FechaDesde.HasValue)
            sql += " AND g.GIR_FEC_GIR >= @FechaDesde";
        if (filtro.FechaHasta.HasValue)
            sql += " AND g.GIR_FEC_GIR <= @FechaHasta";
        if (filtro.RutCliente.HasValue)
            sql += " AND g.GIR_RUT_CLI = @RutCliente";
        if (filtro.TipoGiro.HasValue)
            sql += " AND g.GIR_TIP_GIR = @TipoGiro";
        if (filtro.Estado.HasValue)
            sql += " AND g.GIR_EST_GIR = @Estado";
        if (filtro.CodigoBanco.HasValue)
            sql += " AND g.GIR_COD_BCO = @CodigoBanco";

        sql += " ORDER BY g.GIR_FEC_GIR DESC, g.GIR_NUM_GIR DESC";

        return await connection.QueryAsync<Giro>(sql, filtro);
    }

    public async Task<Giro?> GetByIdAsync(int numeroGiro)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT g.GIR_NUM_GIR AS NumeroGiro, g.GIR_RUT_CLI AS RutCliente,
                   g.GIR_NUM_OPE AS NumeroOperacion, g.GIR_TIP_GIR AS TipoGiro,
                   g.GIR_FEC_GIR AS FechaGiro, g.GIR_MON_GIR AS MontoGiro,
                   g.GIR_CTA_CTE AS CuentaCorriente, g.GIR_COD_BCO AS CodigoBanco,
                   g.GIR_BEN_GIR AS Beneficiario, g.GIR_EST_GIR AS Estado,
                   g.GIR_FOR_CAN AS FormaCancelacion, g.GIR_USR_CRE AS UsuarioCreacion,
                   g.GIR_FEC_CRE AS FechaCreacion, g.GIR_USR_MOD AS UsuarioModificacion,
                   g.GIR_FEC_MOD AS FechaModificacion,
                   c.CLI_RAZ_SOC AS NombreCliente,
                   b.BCO_DES_BCO AS NombreBanco
            FROM GIR g
            LEFT JOIN CLI c ON g.GIR_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN BCO b ON g.GIR_COD_BCO = b.BCO_COD_BCO
            WHERE g.GIR_NUM_GIR = @NumeroGiro";
        return await connection.QueryFirstOrDefaultAsync<Giro>(sql, new { NumeroGiro = numeroGiro });
    }

    public async Task<int> CreateAsync(Giro giro)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Obtener siguiente número de giro
        var nextNumero = await GetNextNumeroGiroAsync();
        giro.NumeroGiro = nextNumero;
        
        const string sql = @"
            INSERT INTO GIR (GIR_NUM_GIR, GIR_RUT_CLI, GIR_NUM_OPE, GIR_TIP_GIR, GIR_FEC_GIR,
                            GIR_MON_GIR, GIR_CTA_CTE, GIR_COD_BCO, GIR_BEN_GIR, GIR_EST_GIR,
                            GIR_FOR_CAN, GIR_USR_CRE, GIR_FEC_CRE)
            VALUES (@NumeroGiro, @RutCliente, @NumeroOperacion, @TipoGiro, @FechaGiro,
                   @MontoGiro, @CuentaCorriente, @CodigoBanco, @Beneficiario, 1,
                   @FormaCancelacion, @UsuarioCreacion, GETDATE())";
        
        await connection.ExecuteAsync(sql, giro);
        return giro.NumeroGiro;
    }

    public async Task<bool> UpdateAsync(Giro giro)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            Update GIR SET
                GIR_TIP_GIR = @TipoGiro,
                GIR_MON_GIR = @MontoGiro,
                GIR_CTA_CTE = @CuentaCorriente,
                GIR_COD_BCO = @CodigoBanco,
                GIR_BEN_GIR = @Beneficiario,
                GIR_EST_GIR = @Estado,
                GIR_FOR_CAN = @FormaCancelacion,
                GIR_USR_MOD = @UsuarioModificacion,
                GIR_FEC_MOD = GETDATE()
            WHERE GIR_NUM_GIR = @NumeroGiro";
        var result = await connection.ExecuteAsync(sql, giro);
        return result > 0;
    }

    public async Task<bool> UpdateEstadoAsync(int numeroGiro, int nuevoEstado, string usuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            Update GIR SET
                GIR_EST_GIR = @NuevoEstado,
                GIR_USR_MOD = @Usuario,
                GIR_FEC_MOD = GETDATE()
            WHERE GIR_NUM_GIR = @NumeroGiro";
        var result = await connection.ExecuteAsync(sql, new { NumeroGiro = numeroGiro, NuevoEstado = nuevoEstado, Usuario = usuario });
        return result > 0;
    }

    public async Task<bool> AnularAsync(int numeroGiro, string usuario)
    {
        return await UpdateEstadoAsync(numeroGiro, 3, usuario); // 3 = Anulado
    }

    public async Task<ResumenGirosDto> GetResumenByFechaAsync(DateTime? fecha)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT @Fecha AS Fecha,
                   COUNT(*) AS TotalGiros,
                   ISNULL(SUM(GIR_MON_GIR), 0) AS MontoTotal,
                   SUM(CASE WHEN GIR_EST_GIR = 2 THEN 1 ELSE 0 END) AS GirosEmitidos,
                   SUM(CASE WHEN GIR_EST_GIR = 1 THEN 1 ELSE 0 END) AS GirosPendientes,
                   SUM(CASE WHEN GIR_EST_GIR = 3 THEN 1 ELSE 0 END) AS GirosAnulados
            FROM GIR
            WHERE GIR_FEC_GIR = @Fecha";
        return await connection.QueryFirstOrDefaultAsync<ResumenGirosDto>(sql, new { Fecha = fecha }) 
            ?? new ResumenGirosDto { Fecha = fecha };
    }

    public async Task<int> GetNextNumeroGiroAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT ISNULL(MAX(GIR_NUM_GIR), 0) + 1 FROM GIR";
        return await connection.ExecuteScalarAsync<int>(sql);
    }
}

/// <summary>
/// Repositorio de Movimientos - Migrado de F_TS1800.frm
/// </summary>
public class MovimientoRepository : IMovimientoRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public MovimientoRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Movimiento>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT m.MOV_NUM_MOV AS NumeroMovimiento, m.MOV_FEC_MOV AS FechaMovimiento,
                   m.MOV_TIP_MOV AS TipoMovimiento, m.MOV_RUT_CLI AS RutCliente,
                   m.MOV_NUM_OPE AS NumeroOperacion, m.MOV_NUM_GIR AS NumeroGiro,
                   m.MOV_MON_MOV AS Monto, m.MOV_GLS_MOV AS Glosa,
                   m.MOV_COD_BCO AS CodigoBanco, m.MOV_CTA_BCO AS CuentaBanco,
                   m.MOV_EST_MOV AS Estado, m.MOV_USR_CRE AS UsuarioCreacion,
                   m.MOV_FEC_CRE AS FechaCreacion,
                   c.CLI_RAZ_SOC AS NombreCliente,
                   b.BCO_DES_BCO AS NombreBanco,
                   p.PAR_DES_PAR AS DescripcionTipoMovimiento
            FROM MOV m
            LEFT JOIN CLI c ON m.MOV_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN BCO b ON m.MOV_COD_BCO = b.BCO_COD_BCO
            LEFT JOIN PAR p ON p.PAR_COD_TAB = 40 AND p.PAR_COD_PAR = m.MOV_TIP_MOV
            ORDER BY m.MOV_FEC_MOV DESC, m.MOV_NUM_MOV DESC";
        return await connection.QueryAsync<Movimiento>(sql);
    }

    public async Task<IEnumerable<Movimiento>> GetByFechaAsync(DateTime? fechaDesde, DateTime? fechaHasta)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT m.MOV_NUM_MOV AS NumeroMovimiento, m.MOV_FEC_MOV AS FechaMovimiento,
                   m.MOV_TIP_MOV AS TipoMovimiento, m.MOV_RUT_CLI AS RutCliente,
                   m.MOV_NUM_OPE AS NumeroOperacion, m.MOV_NUM_GIR AS NumeroGiro,
                   m.MOV_MON_MOV AS Monto, m.MOV_GLS_MOV AS Glosa,
                   m.MOV_COD_BCO AS CodigoBanco, m.MOV_CTA_BCO AS CuentaBanco,
                   m.MOV_EST_MOV AS Estado, m.MOV_USR_CRE AS UsuarioCreacion,
                   m.MOV_FEC_CRE AS FechaCreacion,
                   c.CLI_RAZ_SOC AS NombreCliente,
                   b.BCO_DES_BCO AS NombreBanco
            FROM MOV m
            LEFT JOIN CLI c ON m.MOV_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN BCO b ON m.MOV_COD_BCO = b.BCO_COD_BCO
            WHERE m.MOV_FEC_MOV BETWEEN @FechaDesde AND @FechaHasta
            ORDER BY m.MOV_FEC_MOV DESC, m.MOV_NUM_MOV DESC";
        return await connection.QueryAsync<Movimiento>(sql, new { FechaDesde = fechaDesde, FechaHasta = fechaHasta });
    }

    public async Task<IEnumerable<Movimiento>> GetByClienteAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT m.MOV_NUM_MOV AS NumeroMovimiento, m.MOV_FEC_MOV AS FechaMovimiento,
                   m.MOV_TIP_MOV AS TipoMovimiento, m.MOV_RUT_CLI AS RutCliente,
                   m.MOV_NUM_OPE AS NumeroOperacion, m.MOV_NUM_GIR AS NumeroGiro,
                   m.MOV_MON_MOV AS Monto, m.MOV_GLS_MOV AS Glosa,
                   m.MOV_COD_BCO AS CodigoBanco, m.MOV_CTA_BCO AS CuentaBanco,
                   m.MOV_EST_MOV AS Estado,
                   c.CLI_RAZ_SOC AS NombreCliente,
                   b.BCO_DES_BCO AS NombreBanco
            FROM MOV m
            LEFT JOIN CLI c ON m.MOV_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN BCO b ON m.MOV_COD_BCO = b.BCO_COD_BCO
            WHERE m.MOV_RUT_CLI = @RutCliente
            ORDER BY m.MOV_FEC_MOV DESC";
        return await connection.QueryAsync<Movimiento>(sql, new { RutCliente = rutCliente });
    }

    public async Task<IEnumerable<Movimiento>> GetByFiltroAsync(FiltroTesoreriaDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT m.MOV_NUM_MOV AS NumeroMovimiento, m.MOV_FEC_MOV AS FechaMovimiento,
                   m.MOV_TIP_MOV AS TipoMovimiento, m.MOV_RUT_CLI AS RutCliente,
                   m.MOV_NUM_OPE AS NumeroOperacion, m.MOV_NUM_GIR AS NumeroGiro,
                   m.MOV_MON_MOV AS Monto, m.MOV_GLS_MOV AS Glosa,
                   m.MOV_COD_BCO AS CodigoBanco, m.MOV_CTA_BCO AS CuentaBanco,
                   m.MOV_EST_MOV AS Estado, m.MOV_USR_CRE AS UsuarioCreacion,
                   m.MOV_FEC_CRE AS FechaCreacion,
                   c.CLI_RAZ_SOC AS NombreCliente,
                   b.BCO_DES_BCO AS NombreBanco
            FROM MOV m
            LEFT JOIN CLI c ON m.MOV_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN BCO b ON m.MOV_COD_BCO = b.BCO_COD_BCO
            WHERE 1=1";

        if (filtro.FechaDesde.HasValue)
            sql += " AND m.MOV_FEC_MOV >= @FechaDesde";
        if (filtro.FechaHasta.HasValue)
            sql += " AND m.MOV_FEC_MOV <= @FechaHasta";
        if (filtro.RutCliente.HasValue)
            sql += " AND m.MOV_RUT_CLI = @RutCliente";
        if (filtro.TipoMovimiento.HasValue)
            sql += " AND m.MOV_TIP_MOV = @TipoMovimiento";
        if (filtro.Estado.HasValue)
            sql += " AND m.MOV_EST_MOV = @Estado";
        if (filtro.CodigoBanco.HasValue)
            sql += " AND m.MOV_COD_BCO = @CodigoBanco";

        sql += " ORDER BY m.MOV_FEC_MOV DESC, m.MOV_NUM_MOV DESC";

        return await connection.QueryAsync<Movimiento>(sql, filtro);
    }

    public async Task<Movimiento?> GetByIdAsync(int numeroMovimiento)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT m.MOV_NUM_MOV AS NumeroMovimiento, m.MOV_FEC_MOV AS FechaMovimiento,
                   m.MOV_TIP_MOV AS TipoMovimiento, m.MOV_RUT_CLI AS RutCliente,
                   m.MOV_NUM_OPE AS NumeroOperacion, m.MOV_NUM_GIR AS NumeroGiro,
                   m.MOV_MON_MOV AS Monto, m.MOV_GLS_MOV AS Glosa,
                   m.MOV_COD_BCO AS CodigoBanco, m.MOV_CTA_BCO AS CuentaBanco,
                   m.MOV_EST_MOV AS Estado, m.MOV_USR_CRE AS UsuarioCreacion,
                   m.MOV_FEC_CRE AS FechaCreacion, m.MOV_USR_MOD AS UsuarioModificacion,
                   m.MOV_FEC_MOD AS FechaModificacion,
                   c.CLI_RAZ_SOC AS NombreCliente,
                   b.BCO_DES_BCO AS NombreBanco
            FROM MOV m
            LEFT JOIN CLI c ON m.MOV_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN BCO b ON m.MOV_COD_BCO = b.BCO_COD_BCO
            WHERE m.MOV_NUM_MOV = @NumeroMovimiento";
        return await connection.QueryFirstOrDefaultAsync<Movimiento>(sql, new { NumeroMovimiento = numeroMovimiento });
    }

    public async Task<int> CreateAsync(Movimiento movimiento)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO MOV (MOV_FEC_MOV, MOV_TIP_MOV, MOV_RUT_CLI, MOV_NUM_OPE, MOV_NUM_GIR,
                            MOV_MON_MOV, MOV_GLS_MOV, MOV_COD_BCO, MOV_CTA_BCO, MOV_EST_MOV,
                            MOV_USR_CRE, MOV_FEC_CRE)
            OUTPUT INSERTED.MOV_NUM_MOV
            VALUES (@FechaMovimiento, @TipoMovimiento, @RutCliente, @NumeroOperacion, @NumeroGiro,
                   @Monto, @Glosa, @CodigoBanco, @CuentaBanco, 1,
                   @UsuarioCreacion, GETDATE())";
        
        return await connection.ExecuteScalarAsync<int>(sql, movimiento);
    }

    public async Task<bool> UpdateAsync(Movimiento movimiento)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            Update MOV SET
                MOV_TIP_MOV = @TipoMovimiento,
                MOV_MON_MOV = @Monto,
                MOV_GLS_MOV = @Glosa,
                MOV_COD_BCO = @CodigoBanco,
                MOV_CTA_BCO = @CuentaBanco,
                MOV_EST_MOV = @Estado,
                MOV_USR_MOD = @UsuarioModificacion,
                MOV_FEC_MOD = GETDATE()
            WHERE MOV_NUM_MOV = @NumeroMovimiento";
        var result = await connection.ExecuteAsync(sql, movimiento);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(int numeroMovimiento)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "Update MOV SET MOV_EST_MOV = 0, MOV_FEC_MOD = GETDATE() WHERE MOV_NUM_MOV = @NumeroMovimiento";
        var result = await connection.ExecuteAsync(sql, new { NumeroMovimiento = numeroMovimiento });
        return result > 0;
    }

    public async Task<ResumenMovimientosDto> GetResumenByFechaAsync(DateTime? fecha)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT @Fecha AS Fecha,
                   COUNT(*) AS TotalMovimientos,
                   ISNULL(SUM(CASE WHEN MOV_TIP_MOV = 1 THEN MOV_MON_MOV ELSE 0 END), 0) AS TotalIngresos,
                   ISNULL(SUM(CASE WHEN MOV_TIP_MOV = 2 THEN MOV_MON_MOV ELSE 0 END), 0) AS TotalEgresos,
                   ISNULL(SUM(CASE WHEN MOV_TIP_MOV = 1 THEN MOV_MON_MOV ELSE -MOV_MON_MOV END), 0) AS Saldo
            FROM MOV
            WHERE MOV_FEC_MOV = @Fecha AND MOV_EST_MOV = 1";
        return await connection.QueryFirstOrDefaultAsync<ResumenMovimientosDto>(sql, new { Fecha = fecha }) 
            ?? new ResumenMovimientosDto { Fecha = fecha };
    }

    public async Task<IEnumerable<Movimiento>> GetPizarraAsync(DateTime? fecha)
    {
        return await GetByFechaAsync(fecha, fecha);
    }
}

/// <summary>
/// Repositorio de Arqueo - Migrado de F_TS2100.frm
/// Consulta documentos consolidados de FAO, CHO, LEO para informes de arqueo
/// </summary>
public class ArqueoRepository : IArqueoRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public ArqueoRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ArqueoDocumento>> GetByFiltroAsync(FiltroArqueoDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT TIP_DOC AS TipoDocumento, NUM_OPE AS NumeroOperacion,
                   NUM_DOC AS NumeroDocumento, RUT_DEU AS RutDeudor,
                   FEC_VTO AS FechaVencimiento, MTO_DOC AS MontoDocumento,
                   EST_DOC AS EstadoDocumento, RUT_CLI AS RutCliente,
                   NOM_CLI AS NombreCliente, NOM_DEU AS NombreDeudor,
                   COD_BCO AS CodigoBanco, CTA_COR AS CuentaCorriente,
                   NOM_BCO AS NombreBanco,
                   CASE TIP_DOC 
                       WHEN 'FAO' THEN 'Factura'
                       WHEN 'CHO' THEN 'Cheque'
                       WHEN 'LEO' THEN 'Letra'
                       ELSE TIP_DOC
                   END AS DescripcionTipoDocumento,
                   CASE EST_DOC 
                       WHEN 1 THEN 'Vigente'
                       WHEN 2 THEN 'Prorrogado'
                       WHEN 3 THEN 'Renovado'
                       WHEN 4 THEN 'Cancelado'
                       WHEN 5 THEN 'Moroso'
                       ELSE 'Desconocido'
                   END AS DescripcionEstado
            FROM VW_ARQUEO
            WHERE FEC_VTO BETWEEN @FechaDesde AND @FechaHasta";

        if (filtro.RutCliente.HasValue)
            sql += " AND RUT_CLI = @RutCliente";
        if (!string.IsNullOrEmpty(filtro.TipoDocumento))
            sql += " AND TIP_DOC = @TipoDocumento";
        if (filtro.Estado.HasValue)
            sql += " AND EST_DOC = @Estado";

        sql += " ORDER BY FEC_VTO, TIP_DOC, NUM_DOC";

        return await connection.QueryAsync<ArqueoDocumento>(sql, filtro);
    }

    public async Task<IEnumerable<ArqueoDocumento>> GetByClienteAsync(long rutCliente, DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var filtro = new FiltroArqueoDto
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            RutCliente = rutCliente
        };
        return await GetByFiltroAsync(filtro);
    }

    public async Task<IEnumerable<ArqueoDocumento>> GetByTipoDocumentoAsync(string tipoDocumento, DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var filtro = new FiltroArqueoDto
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            TipoDocumento = tipoDocumento
        };
        return await GetByFiltroAsync(filtro);
    }

    public async Task<ResumenArqueoDto> GetResumenAsync(FiltroArqueoDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT @FechaDesde AS FechaDesde, @FechaHasta AS FechaHasta,
                   COUNT(*) AS TotalDocumentos,
                   ISNULL(SUM(MTO_DOC), 0) AS MontoTotal,
                   SUM(CASE WHEN TIP_DOC = 'FAO' THEN 1 ELSE 0 END) AS TotalFacturas,
                   ISNULL(SUM(CASE WHEN TIP_DOC = 'FAO' THEN MTO_DOC ELSE 0 END), 0) AS MontoFacturas,
                   SUM(CASE WHEN TIP_DOC = 'CHO' THEN 1 ELSE 0 END) AS TotalCheques,
                   ISNULL(SUM(CASE WHEN TIP_DOC = 'CHO' THEN MTO_DOC ELSE 0 END), 0) AS MontoCheques,
                   SUM(CASE WHEN TIP_DOC = 'LEO' THEN 1 ELSE 0 END) AS TotalLetras,
                   ISNULL(SUM(CASE WHEN TIP_DOC = 'LEO' THEN MTO_DOC ELSE 0 END), 0) AS MontoLetras,
                   SUM(CASE WHEN EST_DOC = 1 THEN 1 ELSE 0 END) AS DocumentosVigentes,
                   SUM(CASE WHEN EST_DOC = 5 THEN 1 ELSE 0 END) AS DocumentosMorosos,
                   SUM(CASE WHEN EST_DOC = 4 THEN 1 ELSE 0 END) AS DocumentosCancelados
            FROM VW_ARQUEO
            WHERE FEC_VTO BETWEEN @FechaDesde AND @FechaHasta";

        if (filtro.RutCliente.HasValue)
            sql += " AND RUT_CLI = @RutCliente";
        if (!string.IsNullOrEmpty(filtro.TipoDocumento))
            sql += " AND TIP_DOC = @TipoDocumento";
        if (filtro.Estado.HasValue)
            sql += " AND EST_DOC = @Estado";

        return await connection.QueryFirstOrDefaultAsync<ResumenArqueoDto>(sql, filtro)
            ?? new ResumenArqueoDto { FechaDesde = filtro.FechaDesde, FechaHasta = filtro.FechaHasta };
    }

    public async Task<IEnumerable<ArqueoDocumento>> GetDocumentosVencidosAsync(DateTime? fechaCorte)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT TIP_DOC AS TipoDocumento, NUM_OPE AS NumeroOperacion,
                   NUM_DOC AS NumeroDocumento, RUT_DEU AS RutDeudor,
                   FEC_VTO AS FechaVencimiento, MTO_DOC AS MontoDocumento,
                   EST_DOC AS EstadoDocumento, RUT_CLI AS RutCliente,
                   NOM_CLI AS NombreCliente, NOM_DEU AS NombreDeudor,
                   COD_BCO AS CodigoBanco, CTA_COR AS CuentaCorriente,
                   NOM_BCO AS NombreBanco,
                   CASE TIP_DOC 
                       WHEN 'FAO' THEN 'Factura'
                       WHEN 'CHO' THEN 'Cheque'
                       WHEN 'LEO' THEN 'Letra'
                       ELSE TIP_DOC
                   END AS DescripcionTipoDocumento,
                   'Vencido' AS DescripcionEstado
            FROM VW_ARQUEO
            WHERE FEC_VTO < @FechaCorte AND EST_DOC = 1
            ORDER BY FEC_VTO, TIP_DOC";
        return await connection.QueryAsync<ArqueoDocumento>(sql, new { FechaCorte = fechaCorte });
    }

    public async Task<IEnumerable<ArqueoDocumento>> GetDocumentosPorVencerAsync(DateTime? fechaDesde, DateTime? fechaHasta)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT TIP_DOC AS TipoDocumento, NUM_OPE AS NumeroOperacion,
                   NUM_DOC AS NumeroDocumento, RUT_DEU AS RutDeudor,
                   FEC_VTO AS FechaVencimiento, MTO_DOC AS MontoDocumento,
                   EST_DOC AS EstadoDocumento, RUT_CLI AS RutCliente,
                   NOM_CLI AS NombreCliente, NOM_DEU AS NombreDeudor,
                   COD_BCO AS CodigoBanco, CTA_COR AS CuentaCorriente,
                   NOM_BCO AS NombreBanco,
                   CASE TIP_DOC 
                       WHEN 'FAO' THEN 'Factura'
                       WHEN 'CHO' THEN 'Cheque'
                       WHEN 'LEO' THEN 'Letra'
                       ELSE TIP_DOC
                   END AS DescripcionTipoDocumento,
                   'Por Vencer' AS DescripcionEstado
            FROM VW_ARQUEO
            WHERE FEC_VTO BETWEEN @FechaDesde AND @FechaHasta AND EST_DOC = 1
            ORDER BY FEC_VTO, TIP_DOC";
        return await connection.QueryAsync<ArqueoDocumento>(sql, new { FechaDesde = fechaDesde, FechaHasta = fechaHasta });
    }
}
