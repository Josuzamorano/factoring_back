// =============================================
// Repositorios para Módulo 3 - Carpeta Deudor
// Implementación con Dapper
// =============================================

using Dapper;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;

namespace Factoring.Infrastructure.Repositories;

public class DeudorRepository : IDeudorRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DeudorRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Deudor>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                DEU_RUT_CLI as RutCliente,
                DEU_RUT_DEU as RutDeudor,
                DEU_DIG_VER as DigitoVerificador,
                DEU_RAZ_SOC as RazonSocial,
                DEU_DIR_DEU as Direccion,
                DEU_COM_DEU as CodigoComuna,
                DEU_EST_DEU as EstadoDeudor,
                DEU_MON_OTO as MontoOtorgado,
                DEU_MON_OCU as MontoOcupado,
                DEU_CAT_RIE as CategoriaRiesgo,
                DEU_FEC_CRE as FechaCreacion
            FROM DEU
            ORDER BY DEU_RAZ_SOC";

        return await connection.QueryAsync<Deudor>(sql);
    }

    public async Task<IEnumerable<Deudor>> GetByClienteAsync(int rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                D.DEU_RUT_CLI as RutCliente,
                D.DEU_RUT_DEU as RutDeudor,
                D.DEU_DIG_VER as DigitoVerificador,
                D.DEU_RAZ_SOC as RazonSocial,
                D.DEU_DIR_DEU as Direccion,
                D.DEU_COM_DEU as CodigoComuna,
                D.DEU_EST_DEU as EstadoDeudor,
                D.DEU_MON_OTO as MontoOtorgado,
                D.DEU_MON_OCU as MontoOcupado,
                D.DEU_CAT_RIE as CategoriaRiesgo,
                D.DEU_FEC_CRE as FechaCreacion,
                COM.PAR_DES_PAR as NombreComuna,
                CAT.PAR_DES_PAR as DescripcionCategoriaRiesgo
            FROM DEU D
            LEFT JOIN PAR COM ON D.DEU_COM_DEU = COM.PAR_COD_ELE AND COM.PAR_COD_TAB = 1
            LEFT JOIN PAR CAT ON D.DEU_CAT_RIE = CAT.PAR_COD_ELE AND CAT.PAR_COD_TAB = 8
            WHERE D.DEU_RUT_CLI = @RutCliente
            ORDER BY D.DEU_RAZ_SOC";

        return await connection.QueryAsync<Deudor>(sql, new { RutCliente = rutCliente });
    }

    public async Task<Deudor?> GetByIdAsync(int rutCliente, int rutDeudor)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                D.DEU_RUT_CLI as RutCliente,
                D.DEU_RUT_DEU as RutDeudor,
                D.DEU_DIG_VER as DigitoVerificador,
                D.DEU_SUC_FAC as CodigoSucursal,
                D.DEU_RAZ_SOC as RazonSocial,
                D.DEU_DIR_DEU as Direccion,
                D.DEU_COM_DEU as CodigoComuna,
                D.DEU_EST_DEU as EstadoDeudor,
                D.DEU_CTO_COB as ContactoCobranza,
                D.DEU_POR_VTA as PorcentajeVentas,
                D.DEU_NOT_DEU as Notificacion,
                D.DEU_MON_OTO as MontoOtorgado,
                D.DEU_MON_OCU as MontoOcupado,
                D.DEU_CHE_GIR as ChequesGirados,
                D.DEU_LUG_PGO as LugarPago,
                D.DEU_DIR_PGO as DireccionPago,
                D.DEU_NUM_PRO as NumeroProtestos,
                D.DEU_CTO_CO1 as ContactoCobranza1,
                D.DEU_CAR_NOT as CargoNotificacion,
                D.DEU_ACT_ECO as ActividadEconomica,
                D.DEU_CAT_RIE as CategoriaRiesgo,
                D.DEU_ATT_NOT as AtencionNotificacion,
                D.DEU_GIR_DEU as GiroDeudor,
                D.DEU_FEC_CRE as FechaCreacion,
                D.DEU_FEC_MOD as FechaModificacion,
                COM.PAR_DES_PAR as NombreComuna,
                CAT.PAR_DES_PAR as DescripcionCategoriaRiesgo,
                C.CLI_RAZ_SOC as NombreCliente
            FROM DEU D
            LEFT JOIN PAR COM ON D.DEU_COM_DEU = COM.PAR_COD_ELE AND COM.PAR_COD_TAB = 1
            LEFT JOIN PAR CAT ON D.DEU_CAT_RIE = CAT.PAR_COD_ELE AND CAT.PAR_COD_TAB = 8
            LEFT JOIN CLI C ON D.DEU_RUT_CLI = C.CLI_RUT_CLI
            WHERE D.DEU_RUT_CLI = @RutCliente AND D.DEU_RUT_DEU = @RutDeudor";

        return await connection.QueryFirstOrDefaultAsync<Deudor>(sql, 
            new { RutCliente = rutCliente, RutDeudor = rutDeudor });
    }

    public async Task<Deudor?> GetDeudorGlobalAsync(int rutDeudor)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Buscar el deudor en cualquier cliente (para búsqueda global)
        const string sql = @"
            SELECT TOP 1
                D.DEU_RUT_CLI as RutCliente,
                D.DEU_RUT_DEU as RutDeudor,
                D.DEU_DIG_VER as DigitoVerificador,
                D.DEU_RAZ_SOC as RazonSocial,
                D.DEU_DIR_DEU as Direccion,
                D.DEU_EST_DEU as EstadoDeudor,
                D.DEU_MON_OTO as MontoOtorgado,
                D.DEU_MON_OCU as MontoOcupado
            FROM DEU D
            WHERE D.DEU_RUT_DEU = @RutDeudor
            ORDER BY D.DEU_FEC_CRE DESC";

        return await connection.QueryFirstOrDefaultAsync<Deudor>(sql, new { RutDeudor = rutDeudor });
    }

    public async Task<(IEnumerable<Deudor> Items, int TotalCount)> SearchAsync(
        int? rutCliente, string? rutDeudor, string? razonSocial,
        int? estado, int? categoriaRiesgo, int pageNumber, int pageSize)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();

        if (rutCliente.HasValue)
        {
            whereClause += " AND D.DEU_RUT_CLI = @RutCliente";
            parameters.Add("RutCliente", rutCliente.Value);
        }

        if (!string.IsNullOrWhiteSpace(rutDeudor))
        {
            var rutNum = rutDeudor.Replace(".", "").Replace("-", "").Trim();
            if (rutNum.Length > 1)
                rutNum = rutNum.Substring(0, rutNum.Length - 1);
            
            if (int.TryParse(rutNum, out int rutNumerico))
            {
                whereClause += " AND D.DEU_RUT_DEU = @RutDeudor";
                parameters.Add("RutDeudor", rutNumerico);
            }
        }

        if (!string.IsNullOrWhiteSpace(razonSocial))
        {
            whereClause += " AND D.DEU_RAZ_SOC LIKE @RazonSocial";
            parameters.Add("RazonSocial", $"%{razonSocial}%");
        }

        if (estado.HasValue)
        {
            whereClause += " AND D.DEU_EST_DEU = @Estado";
            parameters.Add("Estado", estado.Value);
        }

        if (categoriaRiesgo.HasValue)
        {
            whereClause += " AND D.DEU_CAT_RIE = @CategoriaRiesgo";
            parameters.Add("CategoriaRiesgo", categoriaRiesgo.Value);
        }

        // Contar total
        var countSql = $@"SELECT COUNT(*) FROM DEU D {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // Obtener página
        var offset = (pageNumber - 1) * pageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var sql = $@"
            SELECT 
                D.DEU_RUT_CLI as RutCliente,
                D.DEU_RUT_DEU as RutDeudor,
                D.DEU_DIG_VER as DigitoVerificador,
                D.DEU_RAZ_SOC as RazonSocial,
                D.DEU_EST_DEU as EstadoDeudor,
                D.DEU_MON_OTO as MontoOtorgado,
                D.DEU_MON_OCU as MontoOcupado,
                D.DEU_CAT_RIE as CategoriaRiesgo,
                C.CLI_RAZ_SOC as NombreCliente
            FROM DEU D
            LEFT JOIN CLI C ON D.DEU_RUT_CLI = C.CLI_RUT_CLI
            {whereClause}
            ORDER BY D.DEU_RAZ_SOC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = await connection.QueryAsync<Deudor>(sql, parameters);

        return (items, totalCount);
    }

    public async Task<bool> ExistsAsync(int rutCliente, int rutDeudor)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT COUNT(1) FROM DEU 
            WHERE DEU_RUT_CLI = @RutCliente AND DEU_RUT_DEU = @RutDeudor";

        var count = await connection.ExecuteScalarAsync<int>(sql, 
            new { RutCliente = rutCliente, RutDeudor = rutDeudor });
        return count > 0;
    }

    public async Task CreateAsync(Deudor deudor)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO DEU (
                DEU_RUT_CLI, DEU_RUT_DEU, DEU_DIG_VER, DEU_SUC_FAC,
                DEU_RAZ_SOC, DEU_DIR_DEU, DEU_COM_DEU, DEU_EST_DEU,
                DEU_CTO_COB, DEU_POR_VTA, DEU_NOT_DEU, DEU_MON_OTO,
                DEU_CAT_RIE, DEU_GIR_DEU, DEU_FEC_CRE
            ) VALUES (
                @RutCliente, @RutDeudor, @DigitoVerificador, @CodigoSucursal,
                @RazonSocial, @Direccion, @CodigoComuna, 1,
                @ContactoCobranza, @PorcentajeVentas, @Notificacion, @MontoOtorgado,
                @CategoriaRiesgo, @GiroDeudor, GETDATE()
            )";

        await connection.ExecuteAsync(sql, deudor);
    }

    public async Task UpdateAsync(Deudor deudor)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE DEU SET
                DEU_RAZ_SOC = @RazonSocial,
                DEU_DIR_DEU = @Direccion,
                DEU_COM_DEU = @CodigoComuna,
                DEU_EST_DEU = @EstadoDeudor,
                DEU_CTO_COB = @ContactoCobranza,
                DEU_POR_VTA = @PorcentajeVentas,
                DEU_NOT_DEU = @Notificacion,
                DEU_MON_OTO = @MontoOtorgado,
                DEU_CAT_RIE = @CategoriaRiesgo,
                DEU_GIR_DEU = @GiroDeudor,
                DEU_FEC_MOD = GETDATE()
            WHERE DEU_RUT_CLI = @RutCliente AND DEU_RUT_DEU = @RutDeudor";

        await connection.ExecuteAsync(sql, deudor);
    }

    public async Task DeleteAsync(int rutCliente, int rutDeudor)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Soft delete - cambiar estado a inactivo
        const string sql = @"
            UPDATE DEU SET DEU_EST_DEU = 0, DEU_FEC_MOD = GETDATE()
            WHERE DEU_RUT_CLI = @RutCliente AND DEU_RUT_DEU = @RutDeudor";

        await connection.ExecuteAsync(sql, new { RutCliente = rutCliente, RutDeudor = rutDeudor });
    }

    public async Task UpdateMontoOcupadoAsync(int rutCliente, int rutDeudor, decimal monto)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE DEU SET DEU_MON_OCU = @Monto, DEU_FEC_MOD = GETDATE()
            WHERE DEU_RUT_CLI = @RutCliente AND DEU_RUT_DEU = @RutDeudor";

        await connection.ExecuteAsync(sql, 
            new { RutCliente = rutCliente, RutDeudor = rutDeudor, Monto = monto });
    }
}

public class DeudorTelefonoRepository : IDeudorTelefonoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DeudorTelefonoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<DeudorTelefono>> GetByDeudorAsync(int rutCliente, int rutDeudor)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                TED_RUT_CLI as RutCliente,
                TED_RUT_DEU as RutDeudor,
                TED_TIP_COM as TipoComunicacion,
                TED_DES_COM as Descripcion,
                TED_NUM_COM as NumeroComunicacion,
                TED_EST_TED as Estado,
                TED_FEC_CRE as FechaCreacion
            FROM TED
            WHERE TED_RUT_CLI = @RutCliente AND TED_RUT_DEU = @RutDeudor AND TED_EST_TED = 1
            ORDER BY TED_TIP_COM";

        return await connection.QueryAsync<DeudorTelefono>(sql, 
            new { RutCliente = rutCliente, RutDeudor = rutDeudor });
    }

    public async Task CreateAsync(DeudorTelefono telefono)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO TED (TED_RUT_CLI, TED_RUT_DEU, TED_TIP_COM, TED_DES_COM, TED_NUM_COM, TED_EST_TED, TED_FEC_CRE)
            VALUES (@RutCliente, @RutDeudor, @TipoComunicacion, @Descripcion, @NumeroComunicacion, 1, GETDATE())";

        await connection.ExecuteAsync(sql, telefono);
    }

    public async Task DeleteAsync(int rutCliente, int rutDeudor, int tipo, string numero)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            DELETE FROM TED 
            WHERE TED_RUT_CLI = @RutCliente AND TED_RUT_DEU = @RutDeudor 
              AND TED_TIP_COM = @Tipo AND TED_NUM_COM = @Numero";

        await connection.ExecuteAsync(sql, 
            new { RutCliente = rutCliente, RutDeudor = rutDeudor, Tipo = tipo, Numero = numero });
    }
}

public class DeudorDocumentoRepository : IDeudorDocumentoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DeudorDocumentoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<DeudorDocumento>> GetByDeudorAsync(int rutCliente, int rutDeudor)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                DED_RUT_CLI as RutCliente,
                DED_RUT_DEU as RutDeudor,
                DED_NUM_DOC as NumeroDocumento,
                DED_TIP_DOC as TipoDocumento,
                DED_NOM_DOC as NombreDocumento,
                DED_RUT_DOC as RutaDocumento,
                DED_FEC_DOC as FechaDocumento,
                DED_OBS_DOC as Observaciones,
                DED_EST_DOC as Estado,
                DED_FEC_CRE as FechaCreacion
            FROM DEU_DOC
            WHERE DED_RUT_CLI = @RutCliente AND DED_RUT_DEU = @RutDeudor AND DED_EST_DOC = 1
            ORDER BY DED_FEC_CRE DESC";

        return await connection.QueryAsync<DeudorDocumento>(sql, 
            new { RutCliente = rutCliente, RutDeudor = rutDeudor });
    }

    public async Task<DeudorDocumento?> GetByIdAsync(int rutCliente, int rutDeudor, int numeroDoc)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                DED_RUT_CLI as RutCliente,
                DED_RUT_DEU as RutDeudor,
                DED_NUM_DOC as NumeroDocumento,
                DED_TIP_DOC as TipoDocumento,
                DED_NOM_DOC as NombreDocumento,
                DED_RUT_DOC as RutaDocumento,
                DED_FEC_DOC as FechaDocumento,
                DED_OBS_DOC as Observaciones,
                DED_EST_DOC as Estado
            FROM DEU_DOC
            WHERE DED_RUT_CLI = @RutCliente AND DED_RUT_DEU = @RutDeudor AND DED_NUM_DOC = @NumeroDoc";

        return await connection.QueryFirstOrDefaultAsync<DeudorDocumento>(sql, 
            new { RutCliente = rutCliente, RutDeudor = rutDeudor, NumeroDoc = numeroDoc });
    }

    public async Task CreateAsync(DeudorDocumento documento)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO DEU_DOC (DED_RUT_CLI, DED_RUT_DEU, DED_TIP_DOC, DED_NOM_DOC, DED_RUT_DOC, DED_FEC_DOC, DED_OBS_DOC, DED_EST_DOC, DED_FEC_CRE)
            VALUES (@RutCliente, @RutDeudor, @TipoDocumento, @NombreDocumento, @RutaDocumento, @FechaDocumento, @Observaciones, 1, GETDATE())";

        await connection.ExecuteAsync(sql, documento);
    }

    public async Task DeleteAsync(int rutCliente, int rutDeudor, int numeroDoc)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE DEU_DOC SET DED_EST_DOC = 0
            WHERE DED_RUT_CLI = @RutCliente AND DED_RUT_DEU = @RutDeudor AND DED_NUM_DOC = @NumeroDoc";

        await connection.ExecuteAsync(sql, 
            new { RutCliente = rutCliente, RutDeudor = rutDeudor, NumeroDoc = numeroDoc });
    }
}
