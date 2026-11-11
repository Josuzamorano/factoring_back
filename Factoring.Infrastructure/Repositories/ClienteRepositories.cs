using Dapper;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;
using Factoring.Infrastructure.Data;

namespace Factoring.Infrastructure.Repositories;

/// <summary>
/// Repositorio de Clientes - Migrado de F_CC1700.frm
/// </summary>
public class ClienteRepository : IClienteRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public ClienteRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Cliente>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT CLI_RUT_CLI AS RutCliente, CLI_DIG_VER AS DigitoVerificador,
                   CLI_TIP_CLI AS TipoCliente, CLI_COD_SUC AS CodigoSucursal,
                   CLI_RAZ_SOC AS RazonSocial, CLI_APE_PAT AS ApellidoPaterno,
                   CLI_APE_MAT AS ApellidoMaterno, CLI_SEX_CLI AS Sexo,
                   CLI_DOM_CLI AS Direccion, CLI_COM_DIR AS CodigoComuna,
                   CLI_GIR_CLI AS CodigoGiro, CLI_COD_POS AS CodigoPostal,
                   CLI_COR_ELE AS CorreoElectronico, CLI_FEC_INI AS FechaInicio,
                   CLI_EST_CLI AS EstadoCliente, CLI_COD_EJE AS CodigoEjecutivo,
                   CLI_OBS_CLI AS Observaciones, CLI_CEJ_INI AS CodigoEjecutivoInicial,
                   CLI_COD_ACT AS CodigoActividad, CLI_LIN_BCO AS LineaBanco,
                   CLI_UTI_BCO AS UtilizadoBanco, CLI_FEC_CRE AS FechaCreacion
            FROM CLI
            ORDER BY CLI_RAZ_SOC";
        return await connection.QueryAsync<Cliente>(sql);
    }

    public async Task<IEnumerable<Cliente>> GetActivosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT CLI_RUT_CLI AS RutCliente, CLI_DIG_VER AS DigitoVerificador,
                   CLI_TIP_CLI AS TipoCliente, CLI_RAZ_SOC AS RazonSocial,
                   CLI_APE_PAT AS ApellidoPaterno, CLI_APE_MAT AS ApellidoMaterno,
                   CLI_COD_EJE AS CodigoEjecutivo, CLI_EST_CLI AS EstadoCliente
            FROM CLI
            WHERE CLI_EST_CLI = 1
            ORDER BY CLI_RAZ_SOC";
        return await connection.QueryAsync<Cliente>(sql);
    }

    public async Task<Cliente?> GetByRutAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT CLI_RUT_CLI AS RutCliente, CLI_DIG_VER AS DigitoVerificador,
                   CLI_TIP_CLI AS TipoCliente, CLI_COD_SUC AS CodigoSucursal,
                   CLI_COB_TEL AS CobradorTelefono, CLI_EST_COB AS EstadoCobranza,
                   CLI_RAZ_SOC AS RazonSocial, CLI_APE_PAT AS ApellidoPaterno,
                   CLI_APE_MAT AS ApellidoMaterno, CLI_SEX_CLI AS Sexo,
                   CLI_DOM_CLI AS Direccion, CLI_COM_DIR AS CodigoComuna,
                   CLI_GIR_CLI AS CodigoGiro, CLI_COD_POS AS CodigoPostal,
                   CLI_COR_ELE AS CorreoElectronico, CLI_FEC_INI AS FechaInicio,
                   CLI_EST_CLI AS EstadoCliente, CLI_COD_EJE AS CodigoEjecutivo,
                   CLI_OBS_CLI AS Observaciones, CLI_CEJ_INI AS CodigoEjecutivoInicial,
                   CLI_COM_PRO AS ComisionProrroga, CLI_COM_PRT AS ComisionProtesto,
                   CLI_COD_ACT AS CodigoActividad, CLI_LIN_BCO AS LineaBanco,
                   CLI_UTI_BCO AS UtilizadoBanco, CLI_SUC_BCO AS SucursalBanco,
                   CLI_EJE_BCO AS EjecutivoBanco, CLI_FEC_CRE AS FechaCreacion,
                   CLI_FEC_MOD AS FechaModificacion
            FROM CLI
            WHERE CLI_RUT_CLI = @RutCliente";
        return await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { RutCliente = rutCliente });
    }

    public async Task<bool> CreateAsync(Cliente cliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO CLI (CLI_RUT_CLI, CLI_DIG_VER, CLI_TIP_CLI, CLI_COD_SUC, CLI_COB_TEL,
                           CLI_EST_COB, CLI_RAZ_SOC, CLI_APE_PAT, CLI_APE_MAT, CLI_SEX_CLI,
                           CLI_DOM_CLI, CLI_COM_DIR, CLI_GIR_CLI, CLI_COD_POS, CLI_COR_ELE,
                           CLI_FEC_INI, CLI_EST_CLI, CLI_COD_EJE, CLI_OBS_CLI, CLI_CEJ_INI,
                           CLI_COD_ACT, CLI_FEC_CRE, CLI_USR_CRE)
            VALUES (@RutCliente, @DigitoVerificador, @TipoCliente, @CodigoSucursal, @CobradorTelefono,
                   @EstadoCobranza, @RazonSocial, @ApellidoPaterno, @ApellidoMaterno, @Sexo,
                   @Direccion, @CodigoComuna, @CodigoGiro, @CodigoPostal, @CorreoElectronico,
                   @FechaInicio, @EstadoCliente, @CodigoEjecutivo, @Observaciones, @CodigoEjecutivoInicial,
                   @CodigoActividad, GETDATE(), @UsuarioCreacion)";
        var result = await connection.ExecuteAsync(sql, cliente);
        return result > 0;
    }

    public async Task<bool> UpdateAsync(Cliente cliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE CLI SET
                CLI_TIP_CLI = @TipoCliente,
                CLI_COD_SUC = @CodigoSucursal,
                CLI_COB_TEL = @CobradorTelefono,
                CLI_EST_COB = @EstadoCobranza,
                CLI_RAZ_SOC = @RazonSocial,
                CLI_APE_PAT = @ApellidoPaterno,
                CLI_APE_MAT = @ApellidoMaterno,
                CLI_SEX_CLI = @Sexo,
                CLI_DOM_CLI = @Direccion,
                CLI_COM_DIR = @CodigoComuna,
                CLI_GIR_CLI = @CodigoGiro,
                CLI_COD_POS = @CodigoPostal,
                CLI_COR_ELE = @CorreoElectronico,
                CLI_EST_CLI = @EstadoCliente,
                CLI_COD_EJE = @CodigoEjecutivo,
                CLI_OBS_CLI = @Observaciones,
                CLI_COD_ACT = @CodigoActividad,
                CLI_FEC_MOD = GETDATE(),
                CLI_USR_MOD = @UsuarioModificacion
            WHERE CLI_RUT_CLI = @RutCliente";
        var result = await connection.ExecuteAsync(sql, cliente);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Soft delete - cambiar estado a inactivo
        const string sql = @"
            UPDATE CLI SET CLI_EST_CLI = 0, CLI_FEC_MOD = GETDATE()
            WHERE CLI_RUT_CLI = @RutCliente";
        var result = await connection.ExecuteAsync(sql, new { RutCliente = rutCliente });
        return result > 0;
    }

    public async Task<bool> ExistsAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM CLI WHERE CLI_RUT_CLI = @RutCliente";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { RutCliente = rutCliente });
        return count > 0;
    }

    public async Task<PagedResult<Cliente>> SearchAsync(ClienteSearchDto search)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();
        
        if (!string.IsNullOrEmpty(search.Rut))
        {
            // Limpiar RUT y buscar
            var rutLimpio = search.Rut.Replace(".", "").Replace("-", "").Trim();
            if (long.TryParse(rutLimpio.Length > 1 ? rutLimpio[..^1] : rutLimpio, out var rut))
            {
                whereClause += " AND CLI_RUT_CLI = @Rut";
                parameters.Add("Rut", rut);
            }
        }
        
        if (!string.IsNullOrEmpty(search.RazonSocial))
        {
            whereClause += " AND (CLI_RAZ_SOC LIKE @RazonSocial OR CLI_APE_PAT LIKE @RazonSocial)";
            parameters.Add("RazonSocial", $"%{search.RazonSocial}%");
        }
        
        if (!string.IsNullOrEmpty(search.CodigoEjecutivo))
        {
            whereClause += " AND CLI_COD_EJE = @CodigoEjecutivo";
            parameters.Add("CodigoEjecutivo", search.CodigoEjecutivo);
        }
        
        if (search.TipoCliente.HasValue)
        {
            whereClause += " AND CLI_TIP_CLI = @TipoCliente";
            parameters.Add("TipoCliente", search.TipoCliente.Value);
        }
        
        if (search.EstadoCliente.HasValue)
        {
            whereClause += " AND CLI_EST_CLI = @EstadoCliente";
            parameters.Add("EstadoCliente", search.EstadoCliente.Value);
        }
        
        if (search.CodigoComuna.HasValue)
        {
            whereClause += " AND CLI_COM_DIR = @CodigoComuna";
            parameters.Add("CodigoComuna", search.CodigoComuna.Value);
        }

        // Contar total
        var countSql = $"SELECT COUNT(*) FROM CLI {whereClause}";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // Obtener página
        var offset = (search.PageNumber - 1) * search.PageSize;
        var sql = $@"
            SELECT CLI_RUT_CLI AS RutCliente, CLI_DIG_VER AS DigitoVerificador,
                   CLI_TIP_CLI AS TipoCliente, CLI_RAZ_SOC AS RazonSocial,
                   CLI_APE_PAT AS ApellidoPaterno, CLI_APE_MAT AS ApellidoMaterno,
                   CLI_DOM_CLI AS Direccion, CLI_COR_ELE AS CorreoElectronico,
                   CLI_EST_CLI AS EstadoCliente, CLI_COD_EJE AS CodigoEjecutivo,
                   CLI_FEC_CRE AS FechaCreacion
            FROM CLI
            {whereClause}
            ORDER BY CLI_RAZ_SOC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", search.PageSize);
        
        var items = await connection.QueryAsync<Cliente>(sql, parameters);

        return new PagedResult<Cliente>
        {
            Items = items,
            TotalItems = totalItems,
            PageNumber = search.PageNumber,
            PageSize = search.PageSize
        };
    }

    public async Task<IEnumerable<Cliente>> GetByEjecutivoAsync(string codigoEjecutivo)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT CLI_RUT_CLI AS RutCliente, CLI_DIG_VER AS DigitoVerificador,
                   CLI_TIP_CLI AS TipoCliente, CLI_RAZ_SOC AS RazonSocial,
                   CLI_APE_PAT AS ApellidoPaterno, CLI_APE_MAT AS ApellidoMaterno,
                   CLI_EST_CLI AS EstadoCliente, CLI_COD_EJE AS CodigoEjecutivo
            FROM CLI
            WHERE CLI_COD_EJE = @CodigoEjecutivo AND CLI_EST_CLI = 1
            ORDER BY CLI_RAZ_SOC";
        return await connection.QueryAsync<Cliente>(sql, new { CodigoEjecutivo = codigoEjecutivo });
    }

    public async Task<bool> UpdateLineaBancoAsync(long rutCliente, decimal linea, decimal utilizado, string? sucursal, string? ejecutivo)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE CLI SET
                CLI_LIN_BCO = @Linea,
                CLI_UTI_BCO = @Utilizado,
                CLI_SUC_BCO = @Sucursal,
                CLI_EJE_BCO = @Ejecutivo,
                CLI_FEC_MOD = GETDATE()
            WHERE CLI_RUT_CLI = @RutCliente";
        var result = await connection.ExecuteAsync(sql, new { RutCliente = rutCliente, Linea = linea, Utilizado = utilizado, Sucursal = sucursal, Ejecutivo = ejecutivo });
        return result > 0;
    }
}

/// <summary>
/// Repositorio de Teléfonos del Cliente
/// </summary>
public class ClienteTelefonoRepository : IClienteTelefonoRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public ClienteTelefonoRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ClienteTelefono>> GetByClienteAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT CLT_RUT_CLI AS RutCliente, CLT_TIP_TLF AS TipoTelefono,
                   CLT_NUM_TLF AS NumeroTelefono, CLT_OBS_TLF AS Observacion,
                   CLT_EST_TLF AS Estado
            FROM CLI_TLF
            WHERE CLT_RUT_CLI = @RutCliente AND CLT_EST_TLF = 0
            ORDER BY CLT_TIP_TLF";
        return await connection.QueryAsync<ClienteTelefono>(sql, new { RutCliente = rutCliente });
    }

    public async Task<bool> CreateAsync(long rutCliente, ClienteTelefono telefono)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO CLI_TLF (CLT_RUT_CLI, CLT_TIP_TLF, CLT_NUM_TLF, CLT_OBS_TLF, CLT_EST_TLF, CLT_FEC_CRE)
            VALUES (@RutCliente, @TipoTelefono, @NumeroTelefono, @Observacion, 0, GETDATE())";
        var result = await connection.ExecuteAsync(sql, new
        {
            RutCliente = rutCliente,
            telefono.TipoTelefono,
            telefono.NumeroTelefono,
            telefono.Observacion
        });
        return result > 0;
    }

    public async Task<bool> DeleteAsync(long rutCliente, int tipoTelefono, string numeroTelefono)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE CLI_TLF SET CLT_EST_TLF = 1
            WHERE CLT_RUT_CLI = @RutCliente AND CLT_TIP_TLF = @TipoTelefono AND CLT_NUM_TLF = @NumeroTelefono";
        var result = await connection.ExecuteAsync(sql, new { RutCliente = rutCliente, TipoTelefono = tipoTelefono, NumeroTelefono = numeroTelefono });
        return result > 0;
    }

    public async Task<bool> DeleteAllByClienteAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE CLI_TLF SET CLT_EST_TLF = 1 WHERE CLT_RUT_CLI = @RutCliente";
        await connection.ExecuteAsync(sql, new { RutCliente = rutCliente });
        return true;
    }
}

/// <summary>
/// Repositorio de Contactos del Cliente
/// </summary>
public class ClienteContactoRepository : IClienteContactoRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public ClienteContactoRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ClienteContacto>> GetByClienteAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT CCN_RUT_CLI AS RutCliente, CCN_NRO_CON AS NumeroContacto,
                   CCN_NOM_CON AS NombreContacto, CCN_CAR_CON AS Cargo,
                   CCN_TLF_CON AS Telefono, CCN_CEL_CON AS Celular,
                   CCN_EML_CON AS Email, CCN_OBS_CON AS Observaciones,
                   CCN_EST_CON AS Estado
            FROM CLI_CON
            WHERE CCN_RUT_CLI = @RutCliente AND CCN_EST_CON = 0
            ORDER BY CCN_NOM_CON";
        return await connection.QueryAsync<ClienteContacto>(sql, new { RutCliente = rutCliente });
    }

    public async Task<ClienteContacto?> GetByIdAsync(long rutCliente, int numeroContacto)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT CCN_RUT_CLI AS RutCliente, CCN_NRO_CON AS NumeroContacto,
                   CCN_NOM_CON AS NombreContacto, CCN_CAR_CON AS Cargo,
                   CCN_TLF_CON AS Telefono, CCN_CEL_CON AS Celular,
                   CCN_EML_CON AS Email, CCN_OBS_CON AS Observaciones,
                   CCN_EST_CON AS Estado
            FROM CLI_CON
            WHERE CCN_RUT_CLI = @RutCliente AND CCN_NRO_CON = @NumeroContacto";
        return await connection.QueryFirstOrDefaultAsync<ClienteContacto>(sql, new { RutCliente = rutCliente, NumeroContacto = numeroContacto });
    }

    public async Task<bool> CreateAsync(long rutCliente, ClienteContacto contacto)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        var numeroContacto = await connection.ExecuteScalarAsync<int>(
            "SELECT ISNULL(MAX(CCN_NRO_CON), 0) + 1 FROM CLI_CON WITH (UPDLOCK, HOLDLOCK) WHERE CCN_RUT_CLI = @RutCliente",
            new { RutCliente = rutCliente },
            transaction: transaction);

        const string sql = @"
            INSERT INTO CLI_CON (CCN_RUT_CLI, CCN_NRO_CON, CCN_NOM_CON, CCN_CAR_CON,
                               CCN_TLF_CON, CCN_CEL_CON, CCN_EML_CON, CCN_OBS_CON,
                               CCN_EST_CON, CCN_FEC_CRE)
            VALUES (@RutCliente, @NumeroContacto, @NombreContacto, @Cargo,
                   @Telefono, @Celular, @Email, @Observaciones, 0, GETDATE())";
        var result = await connection.ExecuteAsync(sql, new
        {
            RutCliente = rutCliente,
            NumeroContacto = numeroContacto,
            contacto.NombreContacto,
            contacto.Cargo,
            contacto.Telefono,
            contacto.Celular,
            contacto.Email,
            contacto.Observaciones
        }, transaction: transaction);

        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> UpdateAsync(ClienteContacto contacto)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE CLI_CON SET
                CCN_NOM_CON = @NombreContacto,
                CCN_CAR_CON = @Cargo,
                CCN_TLF_CON = @Telefono,
                CCN_CEL_CON = @Celular,
                CCN_EML_CON = @Email,
                CCN_OBS_CON = @Observaciones,
                CCN_EST_CON = @Estado,
                CCN_FEC_MOD = GETDATE()
            WHERE CCN_RUT_CLI = @RutCliente AND CCN_NRO_CON = @NumeroContacto";
        var result = await connection.ExecuteAsync(sql, contacto);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(long rutCliente, int numeroContacto)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE CLI_CON SET CCN_EST_CON = 1, CCN_FEC_MOD = GETDATE()
            WHERE CCN_RUT_CLI = @RutCliente AND CCN_NRO_CON = @NumeroContacto";
        var result = await connection.ExecuteAsync(sql, new { RutCliente = rutCliente, NumeroContacto = numeroContacto });
        return result > 0;
    }

    public async Task<int> GetNextNumeroContactoAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT ISNULL(MAX(CCN_NRO_CON), 0) + 1 FROM CLI_CON WITH (UPDLOCK, HOLDLOCK) WHERE CCN_RUT_CLI = @RutCliente";
        return await connection.ExecuteScalarAsync<int>(sql, new { RutCliente = rutCliente });
    }
}

/// <summary>
/// Repositorio de Cuentas Bancarias del Cliente
/// </summary>
public class ClienteBancoRepository : IClienteBancoRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public ClienteBancoRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ClienteBanco>> GetByClienteAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT cb.CBB_RUT_CLI AS RutCliente, cb.CBB_COD_BCO AS CodigoBanco,
                   b.BCO_DES_BCO AS NombreBanco, cb.CBB_TIP_CTA AS TipoCuenta,
                   cb.CBB_NUM_CTA AS NumeroCuenta, cb.CBB_EST_CTA AS Estado
            FROM CLI_BCO cb
            INNER JOIN BCO b ON b.BCO_COD_BCO = cb.CBB_COD_BCO
            WHERE cb.CBB_RUT_CLI = @RutCliente AND cb.CBB_EST_CTA = 0
            ORDER BY b.BCO_DES_BCO";
        return await connection.QueryAsync<ClienteBanco>(sql, new { RutCliente = rutCliente });
    }

    public async Task<bool> CreateAsync(long rutCliente, ClienteBanco cuenta)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO CLI_BCO (CBB_RUT_CLI, CBB_COD_BCO, CBB_TIP_CTA, CBB_NUM_CTA, CBB_EST_CTA, CBB_FEC_CRE)
            VALUES (@RutCliente, @CodigoBanco, @TipoCuenta, @NumeroCuenta, 0, GETDATE())";
        var result = await connection.ExecuteAsync(sql, new
        {
            RutCliente = rutCliente,
            cuenta.CodigoBanco,
            cuenta.TipoCuenta,
            cuenta.NumeroCuenta
        });
        return result > 0;
    }

    public async Task<bool> DeleteAsync(long rutCliente, int codigoBanco, string numeroCuenta)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE CLI_BCO SET CBB_EST_CTA = 1
            WHERE CBB_RUT_CLI = @RutCliente AND CBB_COD_BCO = @CodigoBanco AND CBB_NUM_CTA = @NumeroCuenta";
        var result = await connection.ExecuteAsync(sql, new { RutCliente = rutCliente, CodigoBanco = codigoBanco, NumeroCuenta = numeroCuenta });
        return result > 0;
    }

    public async Task<bool> DeleteAllByClienteAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE CLI_BCO SET CBB_EST_CTA = 1 WHERE CBB_RUT_CLI = @RutCliente";
        await connection.ExecuteAsync(sql, new { RutCliente = rutCliente });
        return true;
    }
}
