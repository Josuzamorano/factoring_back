using Dapper;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;
using Factoring.Infrastructure.Data;

namespace Factoring.Infrastructure.Repositories;

/// <summary>
/// Repositorio de Bancos - Migrado de F_MA1300.frm
/// </summary>
public class BancoRepository : IBancoRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public BancoRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Banco>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT BCO_COD_BCO AS Codigo, BCO_DES_BCO AS Descripcion,
                   BCO_DEP_BCO AS PermiteDeposito, BCO_CTA_BCO AS CuentaCorriente,
                   BCO_CTA_CON AS CuentaContable, BCO_TRA_ELE AS TransferenciaElectronica,
                   BCO_EST_BCO AS Estado, BCO_FEC_CRE AS FechaCreacion, BCO_FEC_MOD AS FechaModificacion
            FROM BCO
            ORDER BY BCO_DES_BCO";
        return await connection.QueryAsync<Banco>(sql);
    }

    public async Task<IEnumerable<Banco>> GetActiveAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT BCO_COD_BCO AS Codigo, BCO_DES_BCO AS Descripcion,
                   BCO_DEP_BCO AS PermiteDeposito, BCO_CTA_BCO AS CuentaCorriente,
                   BCO_CTA_CON AS CuentaContable, BCO_TRA_ELE AS TransferenciaElectronica,
                   BCO_EST_BCO AS Estado, BCO_FEC_CRE AS FechaCreacion, BCO_FEC_MOD AS FechaModificacion
            FROM BCO
            WHERE BCO_EST_BCO = 0
            ORDER BY BCO_DES_BCO";
        return await connection.QueryAsync<Banco>(sql);
    }

    public async Task<Banco?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT BCO_COD_BCO AS Codigo, BCO_DES_BCO AS Descripcion,
                   BCO_DEP_BCO AS PermiteDeposito, BCO_CTA_BCO AS CuentaCorriente,
                   BCO_CTA_CON AS CuentaContable, BCO_TRA_ELE AS TransferenciaElectronica,
                   BCO_EST_BCO AS Estado, BCO_FEC_CRE AS FechaCreacion, BCO_FEC_MOD AS FechaModificacion
            FROM BCO
            WHERE BCO_COD_BCO = @Id";
        return await connection.QueryFirstOrDefaultAsync<Banco>(sql, new { Id = id });
    }

    public async Task<bool> CreateAsync(Banco entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO BCO (BCO_COD_BCO, BCO_DES_BCO, BCO_DEP_BCO, BCO_CTA_BCO, BCO_CTA_CON, BCO_TRA_ELE, BCO_EST_BCO, BCO_FEC_CRE)
            VALUES (@Codigo, @Descripcion, @PermiteDeposito, @CuentaCorriente, @CuentaContable, @TransferenciaElectronica, 0, GETDATE())";
        var result = await connection.ExecuteAsync(sql, entity);
        return result > 0;
    }

    public async Task<bool> UpdateAsync(Banco entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE BCO SET
                BCO_DES_BCO = @Descripcion,
                BCO_DEP_BCO = @PermiteDeposito,
                BCO_CTA_BCO = @CuentaCorriente,
                BCO_CTA_CON = @CuentaContable,
                BCO_TRA_ELE = @TransferenciaElectronica,
                BCO_EST_BCO = @Estado,
                BCO_FEC_MOD = GETDATE()
            WHERE BCO_COD_BCO = @Codigo";
        var result = await connection.ExecuteAsync(sql, entity);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Soft delete - cambiar estado a inactivo
        const string sql = "UPDATE BCO SET BCO_EST_BCO = 1, BCO_FEC_MOD = GETDATE() WHERE BCO_COD_BCO = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM BCO WHERE BCO_COD_BCO = @Id";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}

/// <summary>
/// Repositorio de Plazas - Migrado de F_MA1400.frm
/// </summary>
public class PlazaRepository : IPlazaRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public PlazaRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Plaza>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT PZA_COD_PZA AS Codigo, PZA_DES_PZA AS Descripcion,
                   PZA_DIA_PZA AS DiasCanje, PZA_FUE_PZA AS PlazaFuera,
                   PZA_EST_PZA AS Estado, PZA_FEC_CRE AS FechaCreacion, PZA_FEC_MOD AS FechaModificacion
            FROM PZA
            ORDER BY PZA_DES_PZA";
        return await connection.QueryAsync<Plaza>(sql);
    }

    public async Task<IEnumerable<Plaza>> GetActiveAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT PZA_COD_PZA AS Codigo, PZA_DES_PZA AS Descripcion,
                   PZA_DIA_PZA AS DiasCanje, PZA_FUE_PZA AS PlazaFuera,
                   PZA_EST_PZA AS Estado, PZA_FEC_CRE AS FechaCreacion, PZA_FEC_MOD AS FechaModificacion
            FROM PZA
            WHERE PZA_EST_PZA = 0
            ORDER BY PZA_DES_PZA";
        return await connection.QueryAsync<Plaza>(sql);
    }

    public async Task<Plaza?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT PZA_COD_PZA AS Codigo, PZA_DES_PZA AS Descripcion,
                   PZA_DIA_PZA AS DiasCanje, PZA_FUE_PZA AS PlazaFuera,
                   PZA_EST_PZA AS Estado, PZA_FEC_CRE AS FechaCreacion, PZA_FEC_MOD AS FechaModificacion
            FROM PZA
            WHERE PZA_COD_PZA = @Id";
        return await connection.QueryFirstOrDefaultAsync<Plaza>(sql, new { Id = id });
    }

    public async Task<bool> CreateAsync(Plaza entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO PZA (PZA_COD_PZA, PZA_DES_PZA, PZA_DIA_PZA, PZA_FUE_PZA, PZA_EST_PZA, PZA_FEC_CRE)
            VALUES (@Codigo, @Descripcion, @DiasCanje, @PlazaFuera, 0, GETDATE())";
        var result = await connection.ExecuteAsync(sql, entity);
        return result > 0;
    }

    public async Task<bool> UpdateAsync(Plaza entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE PZA SET
                PZA_DES_PZA = @Descripcion,
                PZA_DIA_PZA = @DiasCanje,
                PZA_FUE_PZA = @PlazaFuera,
                PZA_EST_PZA = @Estado,
                PZA_FEC_MOD = GETDATE()
            WHERE PZA_COD_PZA = @Codigo";
        var result = await connection.ExecuteAsync(sql, entity);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE PZA SET PZA_EST_PZA = 1, PZA_FEC_MOD = GETDATE() WHERE PZA_COD_PZA = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM PZA WHERE PZA_COD_PZA = @Id";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}

/// <summary>
/// Repositorio de Sucursales
/// </summary>
public class SucursalRepository : ISucursalRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public SucursalRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Sucursal>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT SUC_COD_SUC AS Codigo, SUC_DES_SUC AS Descripcion,
                   SUC_DIR_SUC AS Direccion, SUC_TEL_SUC AS Telefono, SUC_EMA_SUC AS Email,
                   SUC_EST_SUC AS Estado, SUC_FEC_CRE AS FechaCreacion, SUC_FEC_MOD AS FechaModificacion
            FROM SUC
            ORDER BY SUC_DES_SUC";
        return await connection.QueryAsync<Sucursal>(sql);
    }

    public async Task<IEnumerable<Sucursal>> GetActiveAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT SUC_COD_SUC AS Codigo, SUC_DES_SUC AS Descripcion,
                   SUC_DIR_SUC AS Direccion, SUC_TEL_SUC AS Telefono, SUC_EMA_SUC AS Email,
                   SUC_EST_SUC AS Estado, SUC_FEC_CRE AS FechaCreacion, SUC_FEC_MOD AS FechaModificacion
            FROM SUC
            WHERE SUC_EST_SUC = 0
            ORDER BY SUC_DES_SUC";
        return await connection.QueryAsync<Sucursal>(sql);
    }

    public async Task<Sucursal?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT SUC_COD_SUC AS Codigo, SUC_DES_SUC AS Descripcion,
                   SUC_DIR_SUC AS Direccion, SUC_TEL_SUC AS Telefono, SUC_EMA_SUC AS Email,
                   SUC_EST_SUC AS Estado, SUC_FEC_CRE AS FechaCreacion, SUC_FEC_MOD AS FechaModificacion
            FROM SUC
            WHERE SUC_COD_SUC = @Id";
        return await connection.QueryFirstOrDefaultAsync<Sucursal>(sql, new { Id = id });
    }

    public async Task<bool> CreateAsync(Sucursal entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO SUC (SUC_COD_SUC, SUC_DES_SUC, SUC_DIR_SUC, SUC_TEL_SUC, SUC_EMA_SUC, SUC_EST_SUC, SUC_FEC_CRE)
            VALUES (@Codigo, @Descripcion, @Direccion, @Telefono, @Email, 0, GETDATE())";
        var result = await connection.ExecuteAsync(sql, entity);
        return result > 0;
    }

    public async Task<bool> UpdateAsync(Sucursal entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE SUC SET
                SUC_DES_SUC = @Descripcion,
                SUC_DIR_SUC = @Direccion,
                SUC_TEL_SUC = @Telefono,
                SUC_EMA_SUC = @Email,
                SUC_EST_SUC = @Estado,
                SUC_FEC_MOD = GETDATE()
            WHERE SUC_COD_SUC = @Codigo";
        var result = await connection.ExecuteAsync(sql, entity);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE SUC SET SUC_EST_SUC = 1, SUC_FEC_MOD = GETDATE() WHERE SUC_COD_SUC = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM SUC WHERE SUC_COD_SUC = @Id";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}

/// <summary>
/// Repositorio de Parámetros - Migrado de F_MA1100.frm
/// </summary>
public class ParametroRepository : IParametroRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public ParametroRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Parametro>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT PAR_COD_TAB AS CodigoTabla, PAR_COD_PAR AS CodigoParametro,
                   PAR_DES_PAR AS Descripcion, PAR_COD_AUX AS CodigoAuxiliar,
                   PAR_VAL_NUM AS ValorNumerico, PAR_VAL_TXT AS ValorTexto,
                   PAR_EST_PAR AS Estado, PAR_FEC_CRE AS FechaCreacion, PAR_FEC_MOD AS FechaModificacion
            FROM PAR
            ORDER BY PAR_COD_TAB, PAR_COD_PAR";
        return await connection.QueryAsync<Parametro>(sql);
    }

    public async Task<IEnumerable<Parametro>> GetByTablaAsync(int codigoTabla)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT PAR_COD_TAB AS CodigoTabla, PAR_COD_PAR AS CodigoParametro,
                   PAR_DES_PAR AS Descripcion, PAR_COD_AUX AS CodigoAuxiliar,
                   PAR_VAL_NUM AS ValorNumerico, PAR_VAL_TXT AS ValorTexto,
                   PAR_EST_PAR AS Estado, PAR_FEC_CRE AS FechaCreacion, PAR_FEC_MOD AS FechaModificacion
            FROM PAR
            WHERE PAR_COD_TAB = @CodigoTabla AND PAR_EST_PAR = 0
            ORDER BY PAR_COD_PAR";
        return await connection.QueryAsync<Parametro>(sql, new { CodigoTabla = codigoTabla });
    }

    public async Task<Parametro?> GetByIdAsync(int codigoTabla, int codigoParametro)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT PAR_COD_TAB AS CodigoTabla, PAR_COD_PAR AS CodigoParametro,
                   PAR_DES_PAR AS Descripcion, PAR_COD_AUX AS CodigoAuxiliar,
                   PAR_VAL_NUM AS ValorNumerico, PAR_VAL_TXT AS ValorTexto,
                   PAR_EST_PAR AS Estado, PAR_FEC_CRE AS FechaCreacion, PAR_FEC_MOD AS FechaModificacion
            FROM PAR
            WHERE PAR_COD_TAB = @CodigoTabla AND PAR_COD_PAR = @CodigoParametro";
        return await connection.QueryFirstOrDefaultAsync<Parametro>(sql, 
            new { CodigoTabla = codigoTabla, CodigoParametro = codigoParametro });
    }

    public async Task<bool> CreateAsync(Parametro parametro)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO PAR (PAR_COD_TAB, PAR_COD_PAR, PAR_DES_PAR, PAR_COD_AUX, PAR_VAL_NUM, PAR_VAL_TXT, PAR_EST_PAR, PAR_FEC_CRE)
            VALUES (@CodigoTabla, @CodigoParametro, @Descripcion, @CodigoAuxiliar, @ValorNumerico, @ValorTexto, 0, GETDATE())";
        var result = await connection.ExecuteAsync(sql, parametro);
        return result > 0;
    }

    public async Task<bool> UpdateAsync(Parametro parametro)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE PAR SET
                PAR_DES_PAR = @Descripcion,
                PAR_COD_AUX = @CodigoAuxiliar,
                PAR_VAL_NUM = @ValorNumerico,
                PAR_VAL_TXT = @ValorTexto,
                PAR_EST_PAR = @Estado,
                PAR_FEC_MOD = GETDATE()
            WHERE PAR_COD_TAB = @CodigoTabla AND PAR_COD_PAR = @CodigoParametro";
        var result = await connection.ExecuteAsync(sql, parametro);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(int codigoTabla, int codigoParametro)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE PAR SET PAR_EST_PAR = 1, PAR_FEC_MOD = GETDATE() 
            WHERE PAR_COD_TAB = @CodigoTabla AND PAR_COD_PAR = @CodigoParametro";
        var result = await connection.ExecuteAsync(sql, 
            new { CodigoTabla = codigoTabla, CodigoParametro = codigoParametro });
        return result > 0;
    }
}
