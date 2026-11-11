// =============================================
// Repositorios para Módulo 4 - Operaciones
// Implementación con Dapper y SqlConnectionFactory
// CORREGIDO para mapear a estructura real de BD
// =============================================

using Dapper;
using Factoring.Core.Interfaces;
using Factoring.Domain.Entities;
using Factoring.Application.Interfaces;

namespace Factoring.Infrastructure.Repositories;

// =============================================
// 1. Operación Repository
// =============================================
public class OperacionRepository : IOperacionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OperacionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Operacion>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                OPE_NUM_OPE as NumeroOperacion,
                OPE_RUT_CLI as RutCliente,
                OPE_TIP_DOC as TipoDocumento,
                OPE_FEC_OPE as FechaOperacion,
                OPE_EST_OPE as EstadoOperacion,
                OPE_MON_OPE as TipoMoneda,
                OPE_MON_DOC as MontoDocumentos,
                OPE_FEC_OTO as FechaOtorgamiento,
                OPE_POR_ANT as PorcentajeAnticipo,
                OPE_MON_ANT as MontoAnticipo,
                OPE_MON_ADE as MontoAdelanto,
                OPE_MON_GAS as MontoGastos,
                OPE_IVA_COM as MontoIvaComision,
                OPE_TOT_GIR as TotalGiro,
                OPE_COD_BAN as CodigoBanco,
                OPE_CTA_CTE as CuentaCorriente,
                OPE_OBS_OPE as Observaciones,
                OPE_FEC_CRE as FechaCreacion
            FROM OPE
            ORDER BY OPE_FEC_OPE DESC";

        return await connection.QueryAsync<Operacion>(sql);
    }

    public async Task<Operacion?> GetByIdAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                OPE_NUM_OPE as NumeroOperacion,
                OPE_RUT_CLI as RutCliente,
                OPE_TIP_DOC as TipoDocumento,
                OPE_FEC_OPE as FechaOperacion,
                OPE_EST_OPE as EstadoOperacion,
                OPE_MON_OPE as TipoMoneda,
                OPE_MON_DOC as MontoDocumentos,
                OPE_FEC_OTO as FechaOtorgamiento,
                OPE_POR_ANT as PorcentajeAnticipo,
                OPE_MON_ANT as MontoAnticipo,
                OPE_MON_ADE as MontoAdelanto,
                OPE_MON_GAS as MontoGastos,
                OPE_IVA_COM as MontoIvaComision,
                OPE_TOT_GIR as TotalGiro,
                OPE_COD_BAN as CodigoBanco,
                OPE_CTA_CTE as CuentaCorriente,
                OPE_OBS_OPE as Observaciones,
                OPE_FEC_CRE as FechaCreacion
            FROM OPE
            WHERE OPE_NUM_OPE = @NumeroOperacion";

        return await connection.QueryFirstOrDefaultAsync<Operacion>(sql, new { NumeroOperacion = numeroOperacion });
    }

    public async Task<Operacion?> GetFullByIdAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                O.OPE_NUM_OPE as NumeroOperacion,
                O.OPE_RUT_CLI as RutCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                O.OPE_TIP_DOC as TipoDocumento,
                TD.PAR_DES_PAR as NombreTipoDocumento,
                O.OPE_FEC_OPE as FechaOperacion,
                O.OPE_EST_OPE as EstadoOperacion,
                E.PAR_DES_PAR as NombreEstado,
                O.OPE_MON_OPE as TipoMoneda,
                M.PAR_DES_PAR as NombreMoneda,
                O.OPE_MON_DOC as MontoDocumentos,
                O.OPE_FEC_OTO as FechaOtorgamiento,
                O.OPE_POR_ANT as PorcentajeAnticipo,
                O.OPE_MON_ANT as MontoAnticipo,
                O.OPE_MON_ADE as MontoAdelanto,
                O.OPE_MON_GAS as MontoGastos,
                O.OPE_IVA_COM as MontoIvaComision,
                O.OPE_TOT_GIR as TotalGiro,
                O.OPE_COD_BAN as CodigoBanco,
                B.BCO_DES_BCO as NombreBanco,
                O.OPE_CTA_CTE as CuentaCorriente,
                O.OPE_OBS_OPE as Observaciones,
                O.OPE_FEC_CRE as FechaCreacion
            FROM OPE O
            INNER JOIN CLI C ON O.OPE_RUT_CLI = C.CLI_RUT_CLI
            LEFT JOIN PAR TD ON O.OPE_TIP_DOC = TD.PAR_COD_PAR AND TD.PAR_COD_TAB = 1
            LEFT JOIN PAR E ON O.OPE_EST_OPE = E.PAR_COD_PAR AND E.PAR_COD_TAB = 3
            LEFT JOIN PAR M ON O.OPE_MON_OPE = M.PAR_COD_PAR AND M.PAR_COD_TAB = 2
            LEFT JOIN BCO B ON O.OPE_COD_BAN = B.BCO_COD_BCO
            WHERE O.OPE_NUM_OPE = @NumeroOperacion";

        return await connection.QueryFirstOrDefaultAsync<Operacion>(sql, new { NumeroOperacion = numeroOperacion });
    }

    public async Task<IEnumerable<Operacion>> GetByClienteAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                O.OPE_NUM_OPE as NumeroOperacion,
                O.OPE_RUT_CLI as RutCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                O.OPE_TIP_DOC as TipoDocumento,
                O.OPE_FEC_OPE as FechaOperacion,
                O.OPE_EST_OPE as EstadoOperacion,
                O.OPE_MON_OPE as TipoMoneda,
                O.OPE_MON_DOC as MontoDocumentos,
                O.OPE_POR_ANT as PorcentajeAnticipo,
                O.OPE_MON_ANT as MontoAnticipo,
                O.OPE_MON_ADE as MontoAdelanto,
                O.OPE_MON_GAS as MontoGastos,
                O.OPE_IVA_COM as MontoIvaComision,
                O.OPE_TOT_GIR as TotalGiro,
                O.OPE_COD_BAN as CodigoBanco,
                O.OPE_CTA_CTE as CuentaCorriente,
                O.OPE_OBS_OPE as Observaciones
            FROM OPE O
            INNER JOIN CLI C ON O.OPE_RUT_CLI = C.CLI_RUT_CLI
            WHERE O.OPE_RUT_CLI = @RutCliente
            ORDER BY O.OPE_FEC_OPE DESC";

        return await connection.QueryAsync<Operacion>(sql, new { RutCliente = rutCliente });
    }

    public async Task<IEnumerable<Operacion>> SearchAsync(DateTime? fechaDesde, DateTime? fechaHasta,
        long? rutCliente, int? estado, int? tipoDocumento)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT
                O.OPE_NUM_OPE as NumeroOperacion,
                O.OPE_RUT_CLI as RutCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                O.OPE_TIP_DOC as TipoDocumento,
                TD.PAR_DES_PAR as NombreTipoDocumento,
                O.OPE_FEC_OPE as FechaOperacion,
                O.OPE_EST_OPE as EstadoOperacion,
                E.PAR_DES_PAR as NombreEstado,
                O.OPE_MON_OPE as TipoMoneda,
                O.OPE_MON_DOC as MontoDocumentos,
                O.OPE_TOT_GIR as TotalGiro
            FROM OPE O
            INNER JOIN CLI C ON O.OPE_RUT_CLI = C.CLI_RUT_CLI
            LEFT JOIN PAR TD ON O.OPE_TIP_DOC = TD.PAR_COD_PAR AND TD.PAR_COD_TAB = 1
            LEFT JOIN PAR E ON O.OPE_EST_OPE = E.PAR_COD_PAR AND E.PAR_COD_TAB = 3
            WHERE 1=1";

        var parameters = new DynamicParameters();

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

        if (rutCliente.HasValue)
        {
            sql += " AND O.OPE_RUT_CLI = @RutCliente";
            parameters.Add("RutCliente", rutCliente.Value);
        }

        if (estado.HasValue)
        {
            sql += " AND O.OPE_EST_OPE = @Estado";
            parameters.Add("Estado", estado.Value);
        }

        if (tipoDocumento.HasValue)
        {
            sql += " AND O.OPE_TIP_DOC = @TipoDocumento";
            parameters.Add("TipoDocumento", tipoDocumento.Value);
        }

        sql += " ORDER BY O.OPE_FEC_OPE DESC";

        return await connection.QueryAsync<Operacion>(sql, parameters);
    }

    public async Task<long> CreateAsync(Operacion operacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO OPE (
                OPE_RUT_CLI, OPE_TIP_DOC, OPE_FEC_OPE, OPE_EST_OPE, OPE_MON_OPE,
                OPE_MON_DOC, OPE_POR_ANT, OPE_MON_ANT, OPE_MON_ADE, OPE_MON_GAS,
                OPE_IVA_COM, OPE_TOT_GIR, OPE_COD_BAN, OPE_CTA_CTE, OPE_OBS_OPE,
                OPE_USR_CRE, OPE_FEC_CRE
            ) VALUES (
                @RutCliente, @TipoDocumento, @FechaOperacion, @EstadoOperacion, @TipoMoneda,
                @MontoDocumentos, @PorcentajeAnticipo, @MontoAnticipo, @MontoAdelanto, @MontoGastos,
                @MontoIvaComision, @TotalGiro, @CodigoBanco, @CuentaCorriente, @Observaciones,
                @UsuarioCreacion, GETDATE()
            );
            SELECT SCOPE_IDENTITY();";

        return await connection.ExecuteScalarAsync<long>(sql, operacion);
    }

    public async Task<bool> UpdateAsync(Operacion operacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE OPE SET
                OPE_RUT_CLI = @RutCliente,
                OPE_TIP_DOC = @TipoDocumento,
                OPE_FEC_OPE = @FechaOperacion,
                OPE_EST_OPE = @EstadoOperacion,
                OPE_MON_OPE = @TipoMoneda,
                OPE_MON_DOC = @MontoDocumentos,
                OPE_POR_ANT = @PorcentajeAnticipo,
                OPE_MON_ANT = @MontoAnticipo,
                OPE_MON_ADE = @MontoAdelanto,
                OPE_MON_GAS = @MontoGastos,
                OPE_IVA_COM = @MontoIvaComision,
                OPE_TOT_GIR = @TotalGiro,
                OPE_COD_BAN = @CodigoBanco,
                OPE_CTA_CTE = @CuentaCorriente,
                OPE_OBS_OPE = @Observaciones,
                OPE_USR_MOD = @UsuarioModificacion,
                OPE_FEC_MOD = GETDATE()
            WHERE OPE_NUM_OPE = @NumeroOperacion";

        var rows = await connection.ExecuteAsync(sql, operacion);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "DELETE FROM OPE WHERE OPE_NUM_OPE = @NumeroOperacion";
        var rows = await connection.ExecuteAsync(sql, new { NumeroOperacion = numeroOperacion });
        return rows > 0;
    }

    public async Task<bool> CambiarEstadoAsync(long numeroOperacion, int nuevoEstado)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE OPE SET OPE_EST_OPE = @NuevoEstado
            WHERE OPE_NUM_OPE = @NumeroOperacion";

        var rows = await connection.ExecuteAsync(sql, new { NumeroOperacion = numeroOperacion, NuevoEstado = nuevoEstado });
        return rows > 0;
    }

    public async Task<bool> OtorgarAsync(long numeroOperacion, DateTime? fechaOtorgamiento)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE OPE SET
                OPE_EST_OPE = 2, -- Estado Otorgada
                OPE_FEC_OTO = @FechaOtorgamiento
            WHERE OPE_NUM_OPE = @NumeroOperacion AND OPE_EST_OPE = 1"; // Solo si está en Ingresada

        var rows = await connection.ExecuteAsync(sql, new { NumeroOperacion = numeroOperacion, FechaOtorgamiento = fechaOtorgamiento });
        return rows > 0;
    }

    public async Task<long> GetNextNumeroAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "SELECT ISNULL(MAX(OPE_NUM_OPE), 0) + 1 FROM OPE";
        return await connection.ExecuteScalarAsync<long>(sql);
    }
}

// =============================================
// 2. Factura Otorgada Repository
// La tabla FAO usa FAO_NUM_OPE + FAO_NUM_FAC como clave
// =============================================
public class FacturaOtorgadaRepository : IFacturaOtorgadaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FacturaOtorgadaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<FacturaOtorgada>> GetByOperacionAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                F.FAO_NUM_OPE as CorrelativoOperacion,
                ROW_NUMBER() OVER (ORDER BY F.FAO_NUM_FAC) as CorrelativoDocumento,
                F.FAO_RUT_DEU as RutDeudor,
                D.DEU_RAZ_SOC as NombreDeudor,
                F.FAO_NUM_FAC as NumeroFactura,
                F.FAO_FEC_EMI as FechaEmision,
                F.FAO_FEC_VTO as FechaVencimiento,
                F.FAO_MON_FAC as MontoDocumento,
                ISNULL(F.FAO_PAG_PEN, 0) as MontoAbonado,
                ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) as MontoSaldo,
                F.FAO_EST_FAC as EstadoDocumento,
                E.PAR_DES_PAR as NombreEstado,
                1 as TipoMoneda,
                F.FAO_OBS_VER as Observaciones,
                F.FAO_FEC_CRE as FechaCreacion
            FROM FAO F
            LEFT JOIN DEU D ON F.FAO_RUT_DEU = D.DEU_RUT_DEU AND F.FAO_RUT_CLI = D.DEU_RUT_CLI
            LEFT JOIN PAR E ON F.FAO_EST_FAC = E.PAR_COD_PAR AND E.PAR_COD_TAB = 4
            WHERE F.FAO_NUM_OPE = @NumeroOperacion
            ORDER BY F.FAO_NUM_FAC";

        return await connection.QueryAsync<FacturaOtorgada>(sql, new { NumeroOperacion = numeroOperacion });
    }

    public async Task<FacturaOtorgada?> GetByIdAsync(long numeroOperacion, int correlativoDocumento)
    {
        // Como no hay correlativo real, usamos el número de factura directamente
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                F.FAO_NUM_OPE as CorrelativoOperacion,
                @CorrelativoDocumento as CorrelativoDocumento,
                F.FAO_RUT_DEU as RutDeudor,
                D.DEU_RAZ_SOC as NombreDeudor,
                F.FAO_NUM_FAC as NumeroFactura,
                F.FAO_FEC_EMI as FechaEmision,
                F.FAO_FEC_VTO as FechaVencimiento,
                F.FAO_MON_FAC as MontoDocumento,
                ISNULL(F.FAO_PAG_PEN, 0) as MontoAbonado,
                ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) as MontoSaldo,
                F.FAO_EST_FAC as EstadoDocumento,
                1 as TipoMoneda,
                F.FAO_OBS_VER as Observaciones
            FROM FAO F
            LEFT JOIN DEU D ON F.FAO_RUT_DEU = D.DEU_RUT_DEU AND F.FAO_RUT_CLI = D.DEU_RUT_CLI
            WHERE F.FAO_NUM_OPE = @NumeroOperacion";

        var facturas = await connection.QueryAsync<FacturaOtorgada>(sql,
            new { NumeroOperacion = numeroOperacion, CorrelativoDocumento = correlativoDocumento });

        return facturas.Skip(correlativoDocumento - 1).FirstOrDefault();
    }

    public async Task<int> CreateAsync(FacturaOtorgada factura)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Obtener RUT cliente de la operación
        var sqlGetCliente = "SELECT OPE_RUT_CLI FROM OPE WHERE OPE_NUM_OPE = @NumeroOperacion";
        var rutCliente = await connection.ExecuteScalarAsync<long>(sqlGetCliente, new { NumeroOperacion = factura.CorrelativoOperacion });

        const string sql = @"
            INSERT INTO FAO (
                FAO_NUM_OPE, FAO_RUT_CLI, FAO_RUT_DEU, FAO_NUM_FAC, FAO_TIP_DOC,
                FAO_FEC_EMI, FAO_FEC_VTO, FAO_MON_FAC, FAO_SAL_PEN, FAO_EST_FAC,
                FAO_USR_CRE, FAO_FEC_CRE
            ) VALUES (
                @NumeroOperacion, @RutCliente, @RutDeudor, @NumeroFactura, 1,
                @FechaEmision, @FechaVencimiento, @MontoDocumento, @MontoDocumento, 1,
                'SISTEMA', GETDATE()
            )";

        await connection.ExecuteAsync(sql, new
        {
            NumeroOperacion = factura.CorrelativoOperacion,
            RutCliente = rutCliente,
            factura.RutDeudor,
            factura.NumeroFactura,
            factura.FechaEmision,
            factura.FechaVencimiento,
            factura.MontoDocumento
        });

        // Retornar un correlativo basado en el conteo
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM FAO WHERE FAO_NUM_OPE = @NumeroOperacion",
            new { NumeroOperacion = factura.CorrelativoOperacion });

        return count;
    }

    public async Task<bool> UpdateAsync(FacturaOtorgada factura)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE FAO SET
                FAO_FEC_EMI = @FechaEmision,
                FAO_FEC_VTO = @FechaVencimiento,
                FAO_MON_FAC = @MontoDocumento,
                FAO_SAL_PEN = @MontoSaldo,
                FAO_EST_FAC = @EstadoDocumento,
                FAO_USR_MOD = 'SISTEMA',
                FAO_FEC_MOD = GETDATE()
            WHERE FAO_NUM_OPE = @NumeroOperacion AND FAO_NUM_FAC = @NumeroFactura";

        var rows = await connection.ExecuteAsync(sql, new
        {
            NumeroOperacion = factura.CorrelativoOperacion,
            factura.NumeroFactura,
            factura.FechaEmision,
            factura.FechaVencimiento,
            factura.MontoDocumento,
            factura.MontoSaldo,
            factura.EstadoDocumento
        });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(long numeroOperacion, int correlativoDocumento)
    {
        // No implementado directamente por correlativo
        // Se debe usar DeleteByOperacionAsync o buscar por número de factura
        return false;
    }

    public async Task<bool> DeleteByOperacionAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "DELETE FROM FAO WHERE FAO_NUM_OPE = @NumeroOperacion";
        await connection.ExecuteAsync(sql, new { NumeroOperacion = numeroOperacion });
        return true;
    }

    public async Task<bool> VerificarAsync(long numeroOperacion, int correlativoDocumento, bool verificado)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE FAO SET
                FAO_EST_VER = @Verificado,
                FAO_FEC_VER = GETDATE()
            WHERE FAO_NUM_OPE = @NumeroOperacion";

        var rows = await connection.ExecuteAsync(sql,
            new { NumeroOperacion = numeroOperacion, Verificado = verificado ? 1 : 0 });
        return rows > 0;
    }

    public async Task<bool> ActualizarSaldosAsync(long numeroOperacion, int correlativoDocumento, decimal montoAbono)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Actualizar todos los documentos de la operación proporcionalmente
        const string sql = @"
            UPDATE FAO SET
                FAO_PAG_PEN = ISNULL(FAO_PAG_PEN, 0) + @MontoAbono,
                FAO_SAL_PEN = ISNULL(FAO_SAL_PEN, FAO_MON_FAC) - @MontoAbono,
                FAO_EST_FAC = CASE WHEN ISNULL(FAO_SAL_PEN, FAO_MON_FAC) - @MontoAbono <= 0 THEN 4 ELSE FAO_EST_FAC END
            WHERE FAO_NUM_OPE = @NumeroOperacion";

        var rows = await connection.ExecuteAsync(sql,
            new { NumeroOperacion = numeroOperacion, MontoAbono = montoAbono });
        return rows > 0;
    }
}

// =============================================
// 3. Cheque Otorgado Repository
// =============================================
public class ChequeOtorgadoRepository : IChequeOtorgadoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ChequeOtorgadoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ChequeOtorgado>> GetByOperacionAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                C.CHO_NUM_OPE as CorrelativoOperacion,
                ROW_NUMBER() OVER (ORDER BY C.CHO_NUM_FAC) as CorrelativoDocumento,
                C.CHO_RUT_DEU as RutDeudor,
                D.DEU_RAZ_SOC as NombreDeudor,
                C.CHO_NUM_FAC as NumeroCheque,
                C.CHO_COD_BAN as CodigoBanco,
                B.BCO_DES_BCO as NombreBanco,
                C.CHO_FEC_EMI as FechaEmision,
                C.CHO_FEC_VTO as FechaVencimiento,
                C.CHO_MON_FAC as MontoDocumento,
                ISNULL(C.CHO_PAG_PEN, 0) as MontoAbonado,
                ISNULL(C.CHO_SAL_PEN, C.CHO_MON_FAC) as MontoSaldo,
                C.CHO_EST_FAC as EstadoDocumento,
                E.PAR_DES_PAR as NombreEstado,
                1 as TipoMoneda,
                C.CHO_OBS_VER as Observaciones
            FROM CHO C
            LEFT JOIN DEU D ON C.CHO_RUT_DEU = D.DEU_RUT_DEU AND C.CHO_RUT_CLI = D.DEU_RUT_CLI
            LEFT JOIN BCO B ON C.CHO_COD_BAN = B.BCO_COD_BCO
            LEFT JOIN PAR E ON C.CHO_EST_FAC = E.PAR_COD_PAR AND E.PAR_COD_TAB = 4
            WHERE C.CHO_NUM_OPE = @NumeroOperacion
            ORDER BY C.CHO_NUM_FAC";

        return await connection.QueryAsync<ChequeOtorgado>(sql, new { NumeroOperacion = numeroOperacion });
    }

    public async Task<ChequeOtorgado?> GetByIdAsync(long numeroOperacion, int correlativoDocumento)
    {
        var cheques = await GetByOperacionAsync(numeroOperacion);
        return cheques.Skip(correlativoDocumento - 1).FirstOrDefault();
    }

    public async Task<int> CreateAsync(ChequeOtorgado cheque)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sqlGetCliente = "SELECT OPE_RUT_CLI FROM OPE WHERE OPE_NUM_OPE = @NumeroOperacion";
        var rutCliente = await connection.ExecuteScalarAsync<long>(sqlGetCliente, new { NumeroOperacion = cheque.CorrelativoOperacion });

        const string sql = @"
            INSERT INTO CHO (
                CHO_NUM_OPE, CHO_RUT_CLI, CHO_RUT_DEU, CHO_NUM_FAC, CHO_TIP_DOC,
                CHO_COD_BAN, CHO_FEC_EMI, CHO_FEC_VTO, CHO_MON_FAC, CHO_SAL_PEN, CHO_EST_FAC,
                CHO_USR_CRE, CHO_FEC_CRE
            ) VALUES (
                @NumeroOperacion, @RutCliente, @RutDeudor, @NumeroCheque, 2,
                @CodigoBanco, @FechaEmision, @FechaVencimiento, @MontoDocumento, @MontoDocumento, 1,
                'SISTEMA', GETDATE()
            )";

        await connection.ExecuteAsync(sql, new
        {
            NumeroOperacion = cheque.CorrelativoOperacion,
            RutCliente = rutCliente,
            cheque.RutDeudor,
            cheque.NumeroCheque,
            cheque.CodigoBanco,
            cheque.FechaEmision,
            cheque.FechaVencimiento,
            cheque.MontoDocumento
        });

        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CHO WHERE CHO_NUM_OPE = @NumeroOperacion",
            new { NumeroOperacion = cheque.CorrelativoOperacion });

        return count;
    }

    public async Task<bool> UpdateAsync(ChequeOtorgado cheque)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE CHO SET
                CHO_COD_BAN = @CodigoBanco,
                CHO_FEC_EMI = @FechaEmision,
                CHO_FEC_VTO = @FechaVencimiento,
                CHO_MON_FAC = @MontoDocumento,
                CHO_SAL_PEN = @MontoSaldo,
                CHO_EST_FAC = @EstadoDocumento,
                CHO_USR_MOD = 'SISTEMA',
                CHO_FEC_MOD = GETDATE()
            WHERE CHO_NUM_OPE = @NumeroOperacion AND CHO_NUM_FAC = @NumeroCheque";

        var rows = await connection.ExecuteAsync(sql, new
        {
            NumeroOperacion = cheque.CorrelativoOperacion,
            cheque.NumeroCheque,
            cheque.CodigoBanco,
            cheque.FechaEmision,
            cheque.FechaVencimiento,
            cheque.MontoDocumento,
            cheque.MontoSaldo,
            cheque.EstadoDocumento
        });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(long numeroOperacion, int correlativoDocumento)
    {
        return false;
    }

    public async Task<bool> DeleteByOperacionAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "DELETE FROM CHO WHERE CHO_NUM_OPE = @NumeroOperacion";
        await connection.ExecuteAsync(sql, new { NumeroOperacion = numeroOperacion });
        return true;
    }
}

// =============================================
// 4. Letra Otorgada Repository
// =============================================
public class LetraOtorgadaRepository : ILetraOtorgadaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public LetraOtorgadaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<LetraOtorgada>> GetByOperacionAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                L.LEO_NUM_OPE as CorrelativoOperacion,
                ROW_NUMBER() OVER (ORDER BY L.LEO_NUM_FAC) as CorrelativoDocumento,
                L.LEO_RUT_DEU as RutDeudor,
                D.DEU_RAZ_SOC as NombreDeudor,
                L.LEO_NUM_FAC as NumeroLetra,
                L.LEO_FEC_EMI as FechaEmision,
                L.LEO_FEC_VTO as FechaVencimiento,
                L.LEO_MON_FAC as MontoDocumento,
                ISNULL(L.LEO_PAG_PEN, 0) as MontoAbonado,
                ISNULL(L.LEO_SAL_PEN, L.LEO_MON_FAC) as MontoSaldo,
                L.LEO_EST_FAC as EstadoDocumento,
                E.PAR_DES_PAR as NombreEstado,
                1 as TipoMoneda,
                L.LEO_OBS_VER as Observaciones
            FROM LEO L
            LEFT JOIN DEU D ON L.LEO_RUT_DEU = D.DEU_RUT_DEU AND L.LEO_RUT_CLI = D.DEU_RUT_CLI
            LEFT JOIN PAR E ON L.LEO_EST_FAC = E.PAR_COD_PAR AND E.PAR_COD_TAB = 4
            WHERE L.LEO_NUM_OPE = @NumeroOperacion
            ORDER BY L.LEO_NUM_FAC";

        return await connection.QueryAsync<LetraOtorgada>(sql, new { NumeroOperacion = numeroOperacion });
    }

    public async Task<LetraOtorgada?> GetByIdAsync(long numeroOperacion, int correlativoDocumento)
    {
        var letras = await GetByOperacionAsync(numeroOperacion);
        return letras.Skip(correlativoDocumento - 1).FirstOrDefault();
    }

    public async Task<int> CreateAsync(LetraOtorgada letra)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sqlGetCliente = "SELECT OPE_RUT_CLI FROM OPE WHERE OPE_NUM_OPE = @NumeroOperacion";
        var rutCliente = await connection.ExecuteScalarAsync<long>(sqlGetCliente, new { NumeroOperacion = letra.CorrelativoOperacion });

        const string sql = @"
            INSERT INTO LEO (
                LEO_NUM_OPE, LEO_RUT_CLI, LEO_RUT_DEU, LEO_NUM_FAC, LEO_TIP_DOC,
                LEO_FEC_EMI, LEO_FEC_VTO, LEO_MON_FAC, LEO_SAL_PEN, LEO_EST_FAC,
                LEO_USR_CRE, LEO_FEC_CRE
            ) VALUES (
                @NumeroOperacion, @RutCliente, @RutDeudor, @NumeroLetra, 4,
                @FechaEmision, @FechaVencimiento, @MontoDocumento, @MontoDocumento, 1,
                'SISTEMA', GETDATE()
            )";

        await connection.ExecuteAsync(sql, new
        {
            NumeroOperacion = letra.CorrelativoOperacion,
            RutCliente = rutCliente,
            letra.RutDeudor,
            letra.NumeroLetra,
            letra.FechaEmision,
            letra.FechaVencimiento,
            letra.MontoDocumento
        });

        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM LEO WHERE LEO_NUM_OPE = @NumeroOperacion",
            new { NumeroOperacion = letra.CorrelativoOperacion });

        return count;
    }

    public async Task<bool> UpdateAsync(LetraOtorgada letra)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE LEO SET
                LEO_FEC_EMI = @FechaEmision,
                LEO_FEC_VTO = @FechaVencimiento,
                LEO_MON_FAC = @MontoDocumento,
                LEO_SAL_PEN = @MontoSaldo,
                LEO_EST_FAC = @EstadoDocumento,
                LEO_USR_MOD = 'SISTEMA',
                LEO_FEC_MOD = GETDATE()
            WHERE LEO_NUM_OPE = @NumeroOperacion AND LEO_NUM_FAC = @NumeroLetra";

        var rows = await connection.ExecuteAsync(sql, new
        {
            NumeroOperacion = letra.CorrelativoOperacion,
            letra.NumeroLetra,
            letra.FechaEmision,
            letra.FechaVencimiento,
            letra.MontoDocumento,
            letra.MontoSaldo,
            letra.EstadoDocumento
        });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(long numeroOperacion, int correlativoDocumento)
    {
        return false;
    }

    public async Task<bool> DeleteByOperacionAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "DELETE FROM LEO WHERE LEO_NUM_OPE = @NumeroOperacion";
        await connection.ExecuteAsync(sql, new { NumeroOperacion = numeroOperacion });
        return true;
    }
}

// =============================================
// 5. Otro Documento Otorgado Repository
// =============================================
public class OtroDocumentoOtorgadoRepository : IOtroDocumentoOtorgadoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OtroDocumentoOtorgadoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<OtroDocumentoOtorgado>> GetByOperacionAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                O.OTO_NUM_OPE as CorrelativoOperacion,
                ROW_NUMBER() OVER (ORDER BY O.OTO_NUM_FAC) as CorrelativoDocumento,
                O.OTO_RUT_DEU as RutDeudor,
                D.DEU_RAZ_SOC as NombreDeudor,
                O.OTO_TIP_DOC as TipoDocumento,
                O.OTO_NUM_FAC as NumeroDocumento,
                O.OTO_FEC_EMI as FechaEmision,
                O.OTO_FEC_VTO as FechaVencimiento,
                O.OTO_MON_FAC as MontoDocumento,
                ISNULL(O.OTO_PAG_PEN, 0) as MontoAbonado,
                ISNULL(O.OTO_SAL_PEN, O.OTO_MON_FAC) as MontoSaldo,
                O.OTO_EST_FAC as EstadoDocumento,
                E.PAR_DES_PAR as NombreEstado,
                1 as TipoMoneda,
                O.OTO_OBS_VER as Observaciones
            FROM OTO O
            LEFT JOIN DEU D ON O.OTO_RUT_DEU = D.DEU_RUT_DEU AND O.OTO_RUT_CLI = D.DEU_RUT_CLI
            LEFT JOIN PAR E ON O.OTO_EST_FAC = E.PAR_COD_PAR AND E.PAR_COD_TAB = 4
            WHERE O.OTO_NUM_OPE = @NumeroOperacion
            ORDER BY O.OTO_NUM_FAC";

        return await connection.QueryAsync<OtroDocumentoOtorgado>(sql, new { NumeroOperacion = numeroOperacion });
    }

    public async Task<OtroDocumentoOtorgado?> GetByIdAsync(long numeroOperacion, int correlativoDocumento)
    {
        var otros = await GetByOperacionAsync(numeroOperacion);
        return otros.Skip(correlativoDocumento - 1).FirstOrDefault();
    }

    public async Task<int> CreateAsync(OtroDocumentoOtorgado otro)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sqlGetCliente = "SELECT OPE_RUT_CLI FROM OPE WHERE OPE_NUM_OPE = @NumeroOperacion";
        var rutCliente = await connection.ExecuteScalarAsync<long>(sqlGetCliente, new { NumeroOperacion = otro.CorrelativoOperacion });

        const string sql = @"
            INSERT INTO OTO (
                OTO_NUM_OPE, OTO_RUT_CLI, OTO_RUT_DEU, OTO_TIP_DOC, OTO_NUM_FAC,
                OTO_FEC_EMI, OTO_FEC_VTO, OTO_MON_FAC, OTO_SAL_PEN, OTO_EST_FAC,
                OTO_USR_CRE, OTO_FEC_CRE
            ) VALUES (
                @NumeroOperacion, @RutCliente, @RutDeudor, @TipoDocumento, @NumeroDocumento,
                @FechaEmision, @FechaVencimiento, @MontoDocumento, @MontoDocumento, 1,
                'SISTEMA', GETDATE()
            )";

        await connection.ExecuteAsync(sql, new
        {
            NumeroOperacion = otro.CorrelativoOperacion,
            RutCliente = rutCliente,
            otro.RutDeudor,
            otro.TipoDocumento,
            otro.NumeroDocumento,
            otro.FechaEmision,
            otro.FechaVencimiento,
            otro.MontoDocumento
        });

        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM OTO WHERE OTO_NUM_OPE = @NumeroOperacion",
            new { NumeroOperacion = otro.CorrelativoOperacion });

        return count;
    }

    public async Task<bool> UpdateAsync(OtroDocumentoOtorgado otro)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE OTO SET
                OTO_FEC_EMI = @FechaEmision,
                OTO_FEC_VTO = @FechaVencimiento,
                OTO_MON_FAC = @MontoDocumento,
                OTO_SAL_PEN = @MontoSaldo,
                OTO_EST_FAC = @EstadoDocumento,
                OTO_USR_MOD = 'SISTEMA',
                OTO_FEC_MOD = GETDATE()
            WHERE OTO_NUM_OPE = @NumeroOperacion AND OTO_NUM_FAC = @NumeroDocumento";

        var rows = await connection.ExecuteAsync(sql, new
        {
            NumeroOperacion = otro.CorrelativoOperacion,
            otro.NumeroDocumento,
            otro.FechaEmision,
            otro.FechaVencimiento,
            otro.MontoDocumento,
            otro.MontoSaldo,
            otro.EstadoDocumento
        });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(long numeroOperacion, int correlativoDocumento)
    {
        return false;
    }

    public async Task<bool> DeleteByOperacionAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "DELETE FROM OTO WHERE OTO_NUM_OPE = @NumeroOperacion";
        await connection.ExecuteAsync(sql, new { NumeroOperacion = numeroOperacion });
        return true;
    }
}

// =============================================
// 6. Documento Operación Repository (Vista Unificada)
// =============================================
public class DocumentoOperacionRepository : IDocumentoOperacionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DocumentoOperacionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<DocumentoOperacion>> GetByOperacionAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                FAO_NUM_OPE as CorrelativoOperacion,
                FAO_NUM_FAC as NumeroDocumento,
                'FAC' as TipoDocumento,
                FAO_RUT_DEU as RutDeudor,
                FAO_FEC_EMI as FechaEmision,
                FAO_FEC_VTO as FechaVencimiento,
                FAO_MON_FAC as MontoDocumento,
                ISNULL(FAO_PAG_PEN, 0) as MontoAbonado,
                ISNULL(FAO_SAL_PEN, FAO_MON_FAC) as MontoSaldo,
                FAO_EST_FAC as EstadoDocumento,
                1 as TipoMoneda
            FROM FAO WHERE FAO_NUM_OPE = @NumeroOperacion
            UNION ALL
            SELECT
                CHO_NUM_OPE, CHO_NUM_FAC, 'CHE',
                CHO_RUT_DEU, CHO_FEC_EMI, CHO_FEC_VTO,
                CHO_MON_FAC, ISNULL(CHO_PAG_PEN, 0),
                ISNULL(CHO_SAL_PEN, CHO_MON_FAC), CHO_EST_FAC, 1
            FROM CHO WHERE CHO_NUM_OPE = @NumeroOperacion
            UNION ALL
            SELECT
                LEO_NUM_OPE, LEO_NUM_FAC, 'LET',
                LEO_RUT_DEU, LEO_FEC_EMI, LEO_FEC_VTO,
                LEO_MON_FAC, ISNULL(LEO_PAG_PEN, 0),
                ISNULL(LEO_SAL_PEN, LEO_MON_FAC), LEO_EST_FAC, 1
            FROM LEO WHERE LEO_NUM_OPE = @NumeroOperacion
            UNION ALL
            SELECT
                OTO_NUM_OPE, OTO_NUM_FAC, 'OTR',
                OTO_RUT_DEU, OTO_FEC_EMI, OTO_FEC_VTO,
                OTO_MON_FAC, ISNULL(OTO_PAG_PEN, 0),
                ISNULL(OTO_SAL_PEN, OTO_MON_FAC), OTO_EST_FAC, 1
            FROM OTO WHERE OTO_NUM_OPE = @NumeroOperacion
            ORDER BY TipoDocumento, NumeroDocumento";

        return await connection.QueryAsync<DocumentoOperacion>(sql, new { NumeroOperacion = numeroOperacion });
    }

    public async Task<decimal> GetTotalByOperacionAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                ISNULL(SUM(FAO_MON_FAC), 0) +
                ISNULL((SELECT SUM(CHO_MON_FAC) FROM CHO WHERE CHO_NUM_OPE = @NumeroOperacion), 0) +
                ISNULL((SELECT SUM(LEO_MON_FAC) FROM LEO WHERE LEO_NUM_OPE = @NumeroOperacion), 0) +
                ISNULL((SELECT SUM(OTO_MON_FAC) FROM OTO WHERE OTO_NUM_OPE = @NumeroOperacion), 0)
            FROM FAO WHERE FAO_NUM_OPE = @NumeroOperacion";

        return await connection.ExecuteScalarAsync<decimal>(sql, new { NumeroOperacion = numeroOperacion });
    }
}

// =============================================
// 7. Tipo Gasto Repository
// =============================================
public class TipoGastoRepository : ITipoGastoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TipoGastoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<TipoGasto>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                GAS_COD_GAS as CodigoGasto,
                GAS_DES_GAS as DescripcionGasto,
                GAS_POR_GAS as PorcentajeGasto,
                GAS_MON_GAS as MontoGasto,
                GAS_AFE_IVA as AfectoIva,
                GAS_EST_GAS as EstadoGasto,
                GAS_FEC_CRE as FechaCreacion
            FROM GAS
            WHERE GAS_EST_GAS = 1
            ORDER BY GAS_DES_GAS";

        return await connection.QueryAsync<TipoGasto>(sql);
    }

    public async Task<TipoGasto?> GetByIdAsync(int codigoGasto)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                GAS_COD_GAS as CodigoGasto,
                GAS_DES_GAS as DescripcionGasto,
                GAS_POR_GAS as PorcentajeGasto,
                GAS_MON_GAS as MontoGasto,
                GAS_AFE_IVA as AfectoIva,
                GAS_EST_GAS as EstadoGasto,
                GAS_FEC_CRE as FechaCreacion
            FROM GAS
            WHERE GAS_COD_GAS = @CodigoGasto";

        return await connection.QueryFirstOrDefaultAsync<TipoGasto>(sql, new { CodigoGasto = codigoGasto });
    }
}

// =============================================
// 8. Gasto Operación Repository
// =============================================
public class GastoOperacionRepository : IGastoOperacionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GastoOperacionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<GastoOperacion>> GetByOperacionAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                G.GDS_NUM_OPE as CorrelativoOperacion,
                G.GDS_COD_GAS as CodigoGasto,
                T.GAS_DES_GAS as DescripcionGasto,
                G.GDS_MON_GAS as MontoGasto,
                G.GDS_AFE_IVA as AfectoIva,
                G.GDS_FEC_CRE as FechaCreacion
            FROM GDS G
            INNER JOIN GAS T ON G.GDS_COD_GAS = T.GAS_COD_GAS
            WHERE G.GDS_NUM_OPE = @NumeroOperacion
            ORDER BY G.GDS_COD_GAS";

        return await connection.QueryAsync<GastoOperacion>(sql, new { NumeroOperacion = numeroOperacion });
    }

    public async Task<decimal> GetTotalGastosAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT ISNULL(SUM(GDS_MON_GAS), 0)
            FROM GDS
            WHERE GDS_NUM_OPE = @NumeroOperacion";

        return await connection.ExecuteScalarAsync<decimal>(sql, new { NumeroOperacion = numeroOperacion });
    }

    public async Task<int> CreateAsync(GastoOperacion gasto)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO GDS (
                GDS_NUM_OPE, GDS_COD_GAS, GDS_MON_GAS, GDS_AFE_IVA, GDS_FEC_CRE
            ) VALUES (
                @NumeroOperacion, @CodigoGasto, @MontoGasto, @AfectoIva, GETDATE()
            )";

        await connection.ExecuteAsync(sql, new
        {
            NumeroOperacion = gasto.CorrelativoOperacion,
            gasto.CodigoGasto,
            gasto.MontoGasto,
            gasto.AfectoIva
        });

        return gasto.CodigoGasto;
    }

    public async Task<bool> DeleteAsync(long numeroOperacion, int codigoGasto)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "DELETE FROM GDS WHERE GDS_NUM_OPE = @NumeroOperacion AND GDS_COD_GAS = @CodigoGasto";
        var rows = await connection.ExecuteAsync(sql, new { NumeroOperacion = numeroOperacion, CodigoGasto = codigoGasto });
        return rows > 0;
    }

    public async Task<bool> DeleteByOperacionAsync(long numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "DELETE FROM GDS WHERE GDS_NUM_OPE = @NumeroOperacion";
        await connection.ExecuteAsync(sql, new { NumeroOperacion = numeroOperacion });
        return true;
    }
}
