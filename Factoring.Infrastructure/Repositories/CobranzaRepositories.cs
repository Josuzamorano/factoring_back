using System.Data;
using Dapper;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;

namespace Factoring.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de Pagos de Documentos
/// </summary>
public class PagoDocumentoRepository : IPagoDocumentoRepository
{
    private readonly IDbConnection _connection;

    public PagoDocumentoRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<PagoDocumento>> GetAllAsync()
    {
        const string sql = @"
            SELECT p.*, c.CLI_NOM_CLI as NombreCliente, d.DEU_NOM_DEU as NombreDeudor,
                   CASE p.PAG_TIP_DOC WHEN 1 THEN 'Factura' WHEN 2 THEN 'Cheque' WHEN 3 THEN 'Letra' ELSE 'Otro' END as DescripcionTipoDocumento,
                   CASE p.PAG_EST_PAG WHEN 1 THEN 'Pendiente' WHEN 2 THEN 'Aprobado' WHEN 3 THEN 'Rechazado' WHEN 4 THEN 'Eliminado' END as DescripcionEstadoPago
            FROM PAG p
            LEFT JOIN CLI c ON p.PAG_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON p.PAG_RUT_DEU = d.DEU_RUT_DEU
            ORDER BY p.PAG_FEC_PAG DESC";
        return await _connection.QueryAsync<PagoDocumento>(sql);
    }

    public async Task<PagoDocumento?> GetByIdAsync(int numeroPago)
    {
        const string sql = @"
            SELECT p.*, c.CLI_NOM_CLI as NombreCliente, d.DEU_NOM_DEU as NombreDeudor
            FROM PAG p
            LEFT JOIN CLI c ON p.PAG_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON p.PAG_RUT_DEU = d.DEU_RUT_DEU
            WHERE p.PAG_NUM_PAG = @NumeroPago";
        return await _connection.QueryFirstOrDefaultAsync<PagoDocumento>(sql, new { NumeroPago = numeroPago });
    }

    public async Task<int> CreateAsync(PagoDocumentoCreateDto dto)
    {
        const string sql = @"
            INSERT INTO PAG (PAG_FEC_PAG, PAG_TIP_DOC, PAG_NUM_DOC, PAG_NUM_OPE, PAG_RUT_CLI, PAG_RUT_DEU,
                PAG_MTO_DOC, PAG_SAL_DOC, PAG_MTO_PAG, PAG_MTO_MOR, PAG_MTO_DES, PAG_MTO_ABO, PAG_DEU_NET,
                PAG_TAS_MOR, PAG_TAS_MOR_NEW, PAG_TIP_PAG, PAG_EST_PAG, PAG_COD_GES, PAG_CON_GES, PAG_USR_CRE, PAG_FEC_CRE)
            VALUES (@FechaPago, @TipoDocumento, @NumeroDocumento, @NumeroOperacion, @RutCliente, @RutDeudor,
                @MontoDocumento, @SaldoDocumento, @MontoPago, @MontoMora, @MontoDescuento, @MontoAbono, @DeudaNeta,
                @TasaMora, @TasaNuevaMora, @TipoPago, 1, @CodigoGestion, @ConGestion, @Usuario, GETDATE());
            SELECT SCOPE_IDENTITY();";
        return await _connection.ExecuteScalarAsync<int>(sql, dto);
    }

    public async Task<bool> UpdateEstadoAsync(PagoDocumentoUpdateDto dto)
    {
        const string sql = @"
            Update PAG SET PAG_EST_PAG = @EstadoPago, PAG_USR_MOD = @Usuario, PAG_FEC_MOD = GETDATE()
            WHERE PAG_NUM_PAG = @NumeroPago";
        return await _connection.ExecuteAsync(sql, dto) > 0;
    }

    public async Task<bool> DeleteAsync(int numeroPago)
    {
        const string sql = "Update PAG SET PAG_EST_PAG = 4 WHERE PAG_NUM_PAG = @NumeroPago";
        return await _connection.ExecuteAsync(sql, new { NumeroPago = numeroPago }) > 0;
    }

    public async Task<IEnumerable<PagoDocumento>> GetByFechaAsync(DateTime? fecha)
    {
        const string sql = @"
            SELECT p.*, c.CLI_NOM_CLI as NombreCliente, d.DEU_NOM_DEU as NombreDeudor
            FROM PAG p
            LEFT JOIN CLI c ON p.PAG_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON p.PAG_RUT_DEU = d.DEU_RUT_DEU
            WHERE CAST(p.PAG_FEC_PAG AS Dayjs) = @Fecha
            ORDER BY p.PAG_FEC_PAG DESC";
        return await _connection.QueryAsync<PagoDocumento>(sql, new { Fecha = fecha });
    }

    public async Task<IEnumerable<PagoDocumento>> GetByClienteAsync(long rutCliente)
    {
        const string sql = @"SELECT * FROM PAG WHERE PAG_RUT_CLI = @RutCliente ORDER BY PAG_FEC_PAG DESC";
        return await _connection.QueryAsync<PagoDocumento>(sql, new { RutCliente = rutCliente });
    }

    public async Task<IEnumerable<PagoDocumento>> GetByDeudorAsync(long rutDeudor)
    {
        const string sql = @"SELECT * FROM PAG WHERE PAG_RUT_DEU = @RutDeudor ORDER BY PAG_FEC_PAG DESC";
        return await _connection.QueryAsync<PagoDocumento>(sql, new { RutDeudor = rutDeudor });
    }

    public async Task<IEnumerable<PagoDocumento>> GetByTipoPagoAsync(int tipoPago)
    {
        const string sql = @"SELECT * FROM PAG WHERE PAG_TIP_PAG = @TipoPago ORDER BY PAG_FEC_PAG DESC";
        return await _connection.QueryAsync<PagoDocumento>(sql, new { TipoPago = tipoPago });
    }

    public async Task<IEnumerable<PagoDocumento>> GetByEstadoAsync(int estadoPago)
    {
        const string sql = @"SELECT * FROM PAG WHERE PAG_EST_PAG = @EstadoPago ORDER BY PAG_FEC_PAG DESC";
        return await _connection.QueryAsync<PagoDocumento>(sql, new { EstadoPago = estadoPago });
    }

    public async Task<IEnumerable<PagoDocumento>> GetPendientesAprobacionAsync()
    {
        const string sql = @"
            SELECT p.*, c.CLI_NOM_CLI as NombreCliente, d.DEU_NOM_DEU as NombreDeudor
            FROM PAG p
            LEFT JOIN CLI c ON p.PAG_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON p.PAG_RUT_DEU = d.DEU_RUT_DEU
            WHERE p.PAG_EST_PAG = 1
            ORDER BY p.PAG_FEC_PAG";
        return await _connection.QueryAsync<PagoDocumento>(sql);
    }

    public async Task<IEnumerable<DocumentoPagarDto>> GetDocumentosPagarDeudorAsync(long rutDeudor, int? tipoDocumento = null)
    {
        var sql = @"
            SELECT f.FAO_NUM_OPE as NumeroOperacion, 1 as TipoDocumento, 'Factura' as DescripcionTipoDocumento,
                   f.FAO_NUM_FAC as NumeroDocumento, f.FAO_RUT_CLI as RutCliente, c.CLI_NOM_CLI as NombreCliente,
                   f.FAO_RUT_DEU as RutDeudor, d.DEU_NOM_DEU as NombreDeudor, f.FAO_FEC_VTO as FechaVencimiento,
                   f.FAO_MTO_FAC as MontoDocumento, f.FAO_SAL_FAC as SaldoPendiente, f.FAO_EST_FAC as EstadoDocumento
            FROM FAO f
            LEFT JOIN CLI c ON f.FAO_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON f.FAO_RUT_DEU = d.DEU_RUT_DEU
            WHERE f.FAO_RUT_DEU = @RutDeudor AND f.FAO_EST_FAC NOT IN (4,6,7)
            UNION ALL
            SELECT l.LEO_NUM_OPE, 3, 'Letra', l.LEO_NUM_LET, l.LEO_RUT_CLI, c.CLI_NOM_CLI,
                   l.LEO_RUT_DEU, d.DEU_NOM_DEU, l.LEO_FEC_VTO, l.LEO_MTO_LET, l.LEO_SAL_LET, l.LEO_EST_LET
            FROM LEO l
            LEFT JOIN CLI c ON l.LEO_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON l.LEO_RUT_DEU = d.DEU_RUT_DEU
            WHERE l.LEO_RUT_DEU = @RutDeudor AND l.LEO_EST_LET NOT IN (4,6,7)";
        
        if (tipoDocumento.HasValue)
            sql = tipoDocumento.Value == 1 ? sql.Split("UNION")[0] : sql.Split("UNION")[1].Replace("UNION ALL", "");
            
        return await _connection.QueryAsync<DocumentoPagarDto>(sql, new { RutDeudor = rutDeudor });
    }

    public async Task<IEnumerable<DocumentoPagarDto>> GetDocumentosPagarClienteAsync(long rutCliente, int? tipoDocumento = null)
    {
        const string sql = @"
            SELECT f.FAO_NUM_OPE as NumeroOperacion, 1 as TipoDocumento, f.FAO_NUM_FAC as NumeroDocumento,
                   f.FAO_RUT_CLI as RutCliente, c.CLI_NOM_CLI as NombreCliente, f.FAO_RUT_DEU as RutDeudor,
                   d.DEU_NOM_DEU as NombreDeudor, f.FAO_FEC_VTO as FechaVencimiento, f.FAO_MTO_FAC as MontoDocumento,
                   f.FAO_SAL_FAC as SaldoPendiente, f.FAO_EST_FAC as EstadoDocumento
            FROM FAO f
            LEFT JOIN CLI c ON f.FAO_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON f.FAO_RUT_DEU = d.DEU_RUT_DEU
            WHERE f.FAO_RUT_CLI = @RutCliente AND f.FAO_EST_FAC NOT IN (4,6,7)";
        return await _connection.QueryAsync<DocumentoPagarDto>(sql, new { RutCliente = rutCliente });
    }

    public async Task<DocumentoPagarDto?> GetDetalleDocumentoAsync(int tipoDocumento, long numeroDocumento, int numeroOperacion)
    {
        string sql = tipoDocumento switch
        {
            1 => @"SELECT FAO_NUM_OPE as NumeroOperacion, 1 as TipoDocumento, FAO_NUM_FAC as NumeroDocumento,
                   FAO_MTO_FAC as MontoDocumento, FAO_SAL_FAC as SaldoPendiente FROM FAO 
                   WHERE FAO_NUM_FAC = @NumeroDocumento AND FAO_NUM_OPE = @NumeroOperacion",
            3 => @"SELECT LEO_NUM_OPE as NumeroOperacion, 3 as TipoDocumento, LEO_NUM_LET as NumeroDocumento,
                   LEO_MTO_LET as MontoDocumento, LEO_SAL_LET as SaldoPendiente FROM LEO 
                   WHERE LEO_NUM_LET = @NumeroDocumento AND LEO_NUM_OPE = @NumeroOperacion",
            _ => throw new ArgumentException("Tipo de documento no válido")
        };
        return await _connection.QueryFirstOrDefaultAsync<DocumentoPagarDto>(sql, new { NumeroDocumento = numeroDocumento, NumeroOperacion = numeroOperacion });
    }

    public async Task<decimal> CalcularMoraAsync(int tipoDocumento, long numeroDocumento, DateTime? fechaPago)
    {
        // Implementación simplificada - debe ajustarse según reglas de negocio
        return await Task.FromResult(0m);
    }

    public async Task<decimal> GetSaldoPendienteAsync(int tipoDocumento, long numeroDocumento, int numeroOperacion)
    {
        string sql = tipoDocumento switch
        {
            1 => "SELECT ISNULL(FAO_SAL_FAC, 0) FROM FAO WHERE FAO_NUM_FAC = @NumeroDocumento AND FAO_NUM_OPE = @NumeroOperacion",
            3 => "SELECT ISNULL(LEO_SAL_LET, 0) FROM LEO WHERE LEO_NUM_LET = @NumeroDocumento AND LEO_NUM_OPE = @NumeroOperacion",
            _ => throw new ArgumentException("Tipo de documento no válido")
        };
        return await _connection.ExecuteScalarAsync<decimal>(sql, new { NumeroDocumento = numeroDocumento, NumeroOperacion = numeroOperacion });
    }
}

/// <summary>
/// Implementación del repositorio de Verificación
/// </summary>
public class VerificacionRepository : IVerificacionRepository
{
    private readonly IDbConnection _connection;

    public VerificacionRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<VerificacionDocumento>> GetVerificacionAsync(FiltroVerificacionDto filtro)
    {
        // Usa stored procedure SEL_VERIFICACION del VB6
        const string sql = @"
            SELECT d.DEU_RUT_DEU as RutDeudor, d.DEU_NOM_DEU as NombreDeudor,
                   c.CLI_RUT_CLI as RutCliente, c.CLI_NOM_CLI as NombreCliente,
                   f.FAO_NUM_OPE as NumeroOperacion, COUNT(*) as CantidadDocumentos,
                   SUM(f.FAO_MTO_FAC) as MontoTotal, @TipoDocumento as TipoDocumento
            FROM FAO f
            INNER JOIN CLI c ON f.FAO_RUT_CLI = c.CLI_RUT_CLI
            INNER JOIN DEU d ON f.FAO_RUT_DEU = d.DEU_RUT_DEU
            WHERE f.FAO_FEC_VTO BETWEEN @FechaDesde AND @FechaHasta
              AND f.FAO_EST_FAC NOT IN (4,6,7)
            GROUP BY d.DEU_RUT_DEU, d.DEU_NOM_DEU, c.CLI_RUT_CLI, c.CLI_NOM_CLI, f.FAO_NUM_OPE
            ORDER BY c.CLI_NOM_CLI";
        return await _connection.QueryAsync<VerificacionDocumento>(sql, filtro);
    }

    public async Task<IEnumerable<VerificacionDocumento>> GetVerificacionOtorgadosAsync(FiltroVerificacionDto filtro)
    {
        // Usa stored procedure SEL_VERIFICACION_OTORGADAS del VB6
        const string sql = @"
            SELECT d.DEU_RUT_DEU as RutDeudor, d.DEU_NOM_DEU as NombreDeudor,
                   c.CLI_RUT_CLI as RutCliente, c.CLI_NOM_CLI as NombreCliente,
                   o.OPE_NUM_OPE as NumeroOperacion, COUNT(*) as CantidadDocumentos,
                   SUM(f.FAO_MTO_FAC) as MontoTotal, 1 as Otorgado
            FROM OPE o
            INNER JOIN FAO f ON o.OPE_NUM_OPE = f.FAO_NUM_OPE
            INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
            INNER JOIN DEU d ON f.FAO_RUT_DEU = d.DEU_RUT_DEU
            WHERE o.OPE_FEC_OTO BETWEEN @FechaDesde AND @FechaHasta
            GROUP BY d.DEU_RUT_DEU, d.DEU_NOM_DEU, c.CLI_RUT_CLI, c.CLI_NOM_CLI, o.OPE_NUM_OPE";
        return await _connection.QueryAsync<VerificacionDocumento>(sql, filtro);
    }

    public async Task<IEnumerable<VerificacionDocumento>> GetVerificacionCaratulaAsync(FiltroVerificacionDto filtro)
    {
        const string sql = @"
            SELECT c.CLI_RUT_CLI as RutCliente, c.CLI_NOM_CLI as NombreCliente,
                   o.OPE_NUM_OPE as NumeroOperacion, COUNT(*) as CantidadDocumentos,
                   SUM(f.FAO_MTO_FAC) as MontoTotal, 1 as TieneCaratula
            FROM OPE o
            INNER JOIN FAO f ON o.OPE_NUM_OPE = f.FAO_NUM_OPE
            INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
            WHERE o.OPE_FEC_DIG BETWEEN @FechaDesde AND @FechaHasta
            GROUP BY c.CLI_RUT_CLI, c.CLI_NOM_CLI, o.OPE_NUM_OPE";
        return await _connection.QueryAsync<VerificacionDocumento>(sql, filtro);
    }

    public async Task<IEnumerable<DetalleVerificacion>> GetDetalleVerificacionAsync(long rutCliente, long rutDeudor, int numeroOperacion)
    {
        const string sql = @"
            SELECT f.FAO_NUM_OPE as NumeroOperacion, 1 as TipoDocumento, f.FAO_NUM_FAC as NumeroDocumento,
                   f.FAO_RUT_CLI as RutCliente, c.CLI_NOM_CLI as NombreCliente,
                   f.FAO_RUT_DEU as RutDeudor, d.DEU_NOM_DEU as NombreDeudor,
                   f.FAO_FEC_VTO as FechaVencimiento, f.FAO_MTO_FAC as MontoDocumento,
                   f.FAO_EST_FAC as EstadoDocumento
            FROM FAO f
            LEFT JOIN CLI c ON f.FAO_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON f.FAO_RUT_DEU = d.DEU_RUT_DEU
            WHERE f.FAO_RUT_CLI = @RutCliente AND f.FAO_RUT_DEU = @RutDeudor AND f.FAO_NUM_OPE = @NumeroOperacion";
        return await _connection.QueryAsync<DetalleVerificacion>(sql, new { RutCliente = rutCliente, RutDeudor = rutDeudor, NumeroOperacion = numeroOperacion });
    }

    public async Task<bool> MarcarVerificadoAsync(int tipoDocumento, long numeroDocumento, int numeroOperacion, bool verificado)
    {
        const string sql = "Update FAO SET FAO_VER_FAC = @Verificado WHERE FAO_NUM_FAC = @NumeroDocumento AND FAO_NUM_OPE = @NumeroOperacion";
        return await _connection.ExecuteAsync(sql, new { Verificado = verificado ? 1 : 0, NumeroDocumento = numeroDocumento, NumeroOperacion = numeroOperacion }) > 0;
    }
}

/// <summary>
/// Implementación del repositorio de Caja
/// </summary>
public class CajaRepository : ICajaRepository
{
    private readonly IDbConnection _connection;

    public CajaRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<MovimientoCaja>> GetMovimientosCajaAsync(FiltroCajaDto filtro)
    {
        var sql = @"
            SELECT p.PAG_NUM_PAG as NumeroMovimiento, p.PAG_FEC_PAG as FechaMovimiento,
                   p.PAG_TIP_DOC as TipoDocumento, p.PAG_NUM_DOC as NumeroDocumento,
                   p.PAG_NUM_OPE as NumeroOperacion, p.PAG_RUT_CLI as RutCliente,
                   c.CLI_NOM_CLI as NombreCliente, p.PAG_RUT_DEU as RutDeudor,
                   d.DEU_NOM_DEU as NombreDeudor, p.PAG_MTO_PAG as Monto,
                   p.PAG_EST_PAG as EstadoPago, p.PAG_USR_CRE as UsuarioCaja
            FROM PAG p
            LEFT JOIN CLI c ON p.PAG_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON p.PAG_RUT_DEU = d.DEU_RUT_DEU
            WHERE CAST(p.PAG_FEC_PAG AS Dayjs) = @Fecha";

        if (filtro.TipoDocumento.HasValue)
            sql += " AND p.PAG_TIP_DOC = @TipoDocumento";
        if (filtro.EstadoPago.HasValue && filtro.EstadoPago > 0)
            sql += " AND p.PAG_EST_PAG = @EstadoPago";

        sql += " ORDER BY p.PAG_FEC_PAG DESC";
        return await _connection.QueryAsync<MovimientoCaja>(sql, filtro);
    }

    public async Task<MovimientoCaja?> GetByIdAsync(int numeroMovimiento)
    {
        const string sql = "SELECT * FROM PAG WHERE PAG_NUM_PAG = @NumeroMovimiento";
        return await _connection.QueryFirstOrDefaultAsync<MovimientoCaja>(sql, new { NumeroMovimiento = numeroMovimiento });
    }

    public async Task<ResumenCaja> GetResumenCajaAsync(DateTime? fecha, int? tipoDocumento = null)
    {
        var sql = @"
            SELECT @Fecha as Fecha,
                   SUM(CASE WHEN PAG_EST_PAG = 1 THEN 1 ELSE 0 END) as CantidadActuales,
                   SUM(CASE WHEN PAG_EST_PAG = 1 THEN PAG_MTO_PAG ELSE 0 END) as MontoActuales,
                   SUM(CASE WHEN PAG_EST_PAG = 2 THEN 1 ELSE 0 END) as CantidadProcesados,
                   SUM(CASE WHEN PAG_EST_PAG = 2 THEN PAG_MTO_PAG ELSE 0 END) as MontoProcesados,
                   COUNT(*) as CantidadTotal, SUM(PAG_MTO_PAG) as MontoTotal
            FROM PAG WHERE CAST(PAG_FEC_PAG AS Dayjs) = @Fecha";
        if (tipoDocumento.HasValue) sql += " AND PAG_TIP_DOC = @TipoDocumento";
        return await _connection.QueryFirstAsync<ResumenCaja>(sql, new { Fecha = fecha, TipoDocumento = tipoDocumento });
    }

    public async Task<IEnumerable<ResumenCaja>> GetResumenCajaPorTipoAsync(DateTime? fecha)
    {
        const string sql = @"
            SELECT @Fecha as Fecha, PAG_TIP_DOC as TipoDocumento,
                   COUNT(*) as CantidadTotal, SUM(PAG_MTO_PAG) as MontoTotal
            FROM PAG WHERE CAST(PAG_FEC_PAG AS Dayjs) = @Fecha
            GROUP BY PAG_TIP_DOC";
        return await _connection.QueryAsync<ResumenCaja>(sql, new { Fecha = fecha });
    }

    public async Task<bool> AprobarPagoAsync(int numeroMovimiento, string usuario)
    {
        const string sql = "Update PAG SET PAG_EST_PAG = 2, PAG_USR_MOD = @Usuario, PAG_FEC_MOD = GETDATE() WHERE PAG_NUM_PAG = @NumeroMovimiento";
        return await _connection.ExecuteAsync(sql, new { NumeroMovimiento = numeroMovimiento, Usuario = usuario }) > 0;
    }

    public async Task<bool> RechazarPagoAsync(int numeroMovimiento, string usuario, string? motivo)
    {
        const string sql = "Update PAG SET PAG_EST_PAG = 3, PAG_USR_MOD = @Usuario, PAG_FEC_MOD = GETDATE() WHERE PAG_NUM_PAG = @NumeroMovimiento";
        return await _connection.ExecuteAsync(sql, new { NumeroMovimiento = numeroMovimiento, Usuario = usuario }) > 0;
    }

    public async Task<bool> EliminarPagoAsync(int numeroMovimiento, string usuario)
    {
        const string sql = "Update PAG SET PAG_EST_PAG = 4, PAG_USR_MOD = @Usuario, PAG_FEC_MOD = GETDATE() WHERE PAG_NUM_PAG = @NumeroMovimiento";
        return await _connection.ExecuteAsync(sql, new { NumeroMovimiento = numeroMovimiento, Usuario = usuario }) > 0;
    }

    public async Task<IEnumerable<MovimientoCaja>> GetPagosActualesAsync(DateTime? fecha)
    {
        return await GetMovimientosCajaAsync(new FiltroCajaDto { Fecha = fecha, EstadoPago = 1 });
    }

    public async Task<IEnumerable<MovimientoCaja>> GetPagosEnRetencionAsync(DateTime? fecha)
    {
        return await GetMovimientosCajaAsync(new FiltroCajaDto { Fecha = fecha, EstadoCaja = 2 });
    }

    public async Task<IEnumerable<MovimientoCaja>> GetPagosProcesadosAsync(DateTime? fecha)
    {
        return await GetMovimientosCajaAsync(new FiltroCajaDto { Fecha = fecha, EstadoPago = 2 });
    }
}

/// <summary>
/// Implementación del repositorio de Informe de Abonos
/// </summary>
public class InformeAbonoRepository : IInformeAbonoRepository
{
    private readonly IDbConnection _connection;

    public InformeAbonoRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<InformeAbono>> GetAbonosAsync(FiltroInformeAbonoDto filtro)
    {
        var sql = @"
            SELECT p.PAG_FEC_PAG as FechaAbono, p.PAG_RUT_CLI as RutCliente, c.CLI_NOM_CLI as NombreCliente,
                   p.PAG_RUT_DEU as RutDeudor, d.DEU_NOM_DEU as NombreDeudor, p.PAG_NUM_OPE as NumeroOperacion,
                   p.PAG_TIP_DOC as TipoDocumento, p.PAG_NUM_DOC as NumeroDocumento,
                   p.PAG_MTO_DOC as MontoDocumento, p.PAG_MTO_ABO as MontoAbono,
                   (p.PAG_SAL_DOC - p.PAG_MTO_ABO) as SaldoPendiente, p.PAG_TIP_PAG as TipoPago
            FROM PAG p
            LEFT JOIN CLI c ON p.PAG_RUT_CLI = c.CLI_RUT_CLI
            LEFT JOIN DEU d ON p.PAG_RUT_DEU = d.DEU_RUT_DEU
            WHERE p.PAG_FEC_PAG BETWEEN @FechaDesde AND @FechaHasta AND p.PAG_EST_PAG = 2";
        
        if (filtro.RutCliente.HasValue) sql += " AND p.PAG_RUT_CLI = @RutCliente";
        if (filtro.RutDeudor.HasValue) sql += " AND p.PAG_RUT_DEU = @RutDeudor";
        if (filtro.TipoDocumento.HasValue) sql += " AND p.PAG_TIP_DOC = @TipoDocumento";
        sql += " ORDER BY p.PAG_FEC_PAG DESC";
        
        return await _connection.QueryAsync<InformeAbono>(sql, filtro);
    }

    public async Task<ResumenAbonoDto> GetResumenAbonosAsync(FiltroInformeAbonoDto filtro)
    {
        const string sql = @"
            SELECT @FechaDesde as FechaDesde, @FechaHasta as FechaHasta,
                   COUNT(*) as CantidadAbonos, SUM(PAG_MTO_ABO) as MontoTotalAbonos,
                   SUM(PAG_MTO_DOC) as MontoTotalDocumentos,
                   SUM(CASE WHEN PAG_TIP_PAG = 1 THEN 1 ELSE 0 END) as CantidadPagosDeudor,
                   SUM(CASE WHEN PAG_TIP_PAG = 1 THEN PAG_MTO_ABO ELSE 0 END) as MontoPagosDeudor,
                   SUM(CASE WHEN PAG_TIP_PAG = 2 THEN 1 ELSE 0 END) as CantidadPagosCliente,
                   SUM(CASE WHEN PAG_TIP_PAG = 2 THEN PAG_MTO_ABO ELSE 0 END) as MontoPagosCliente
            FROM PAG WHERE PAG_FEC_PAG BETWEEN @FechaDesde AND @FechaHasta AND PAG_EST_PAG = 2";
        return await _connection.QueryFirstAsync<ResumenAbonoDto>(sql, filtro);
    }

    public async Task<IEnumerable<InformeAbono>> GetAbonosByCobradorAsync(string codigoCobrador, DateTime? fechaDesde, DateTime? fechaHasta)
    {
        const string sql = @"
            SELECT p.PAG_FEC_PAG as FechaAbono, p.PAG_RUT_CLI as RutCliente, p.PAG_MTO_ABO as MontoAbono
            FROM PAG p
            INNER JOIN GES g ON p.PAG_COD_GES = g.GES_COR_GES
            WHERE g.GES_COB_TEL = @CodigoCobrador AND p.PAG_FEC_PAG BETWEEN @FechaDesde AND @FechaHasta";
        return await _connection.QueryAsync<InformeAbono>(sql, new { CodigoCobrador = codigoCobrador, FechaDesde = fechaDesde, FechaHasta = fechaHasta });
    }
}

/// <summary>
/// Implementación del repositorio de Verificación Diaria
/// Migrado de F_CO2510.frm (VB6)
/// </summary>
public class VerificacionDiariaRepository : IVerificacionDiariaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public VerificacionDiariaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Obtiene verificaciones diarias por fecha y tipo de documento
    /// </summary>
    public async Task<IEnumerable<VerificacionDiaria>> GetByFechaAsync(DateTime? fecha, int? tipoDocumento = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT
                DVF.DVF_FEC_VER as FechaVerificacion,
                DVF.DVF_TIP_DOC as TipoDocumento,
                DVF.DVF_RUT_CLI as RutCliente,
                RTRIM(ISNULL(CLI.CLI_RAZ_SOC, '') + ' ' + ISNULL(CLI.CLI_APE_PAT, '') + ' ' + ISNULL(CLI.CLI_APE_MAT, '')) as NombreCliente,
                DVF.DVF_RUT_DEU as RutDeudor,
                ISNULL(DEU.DEU_RAZ_SOC, '') as NombreDeudor,
                DVF.DVF_NUM_OPE as NumeroOperacion,
                DVF.DVF_NUM_FAC as NumeroDocumento,
                DOC.DOC_FEC_VEN as FechaVencimiento,
                ISNULL(DOC.DOC_MTO_DOC, 0) as MontoDocumento,
                DVF.DVF_EST_VER as EstadoVerificacion,
                ISNULL(PAR.PAR_DES_PAR, '') as Resultado,
                CAST(DVF.DVF_OBS_VER AS VARCHAR(8000)) as Observacion,
                DVF.DVF_USR_VER as UsuarioVerificacion,
                ISNULL(PARTD.PAR_DES_PAR, '') as DescripcionTipoDocumento
            FROM DVF DVF
            INNER JOIN CLI CLI ON DVF.DVF_RUT_CLI = CLI.CLI_RUT_CLI
            LEFT JOIN DEU DEU ON DVF.DVF_RUT_DEU = DEU.DEU_RUT_DEU AND DVF.DVF_RUT_CLI = DEU.DEU_RUT_CLI
            LEFT JOIN DOC DOC ON DVF.DVF_TIP_DOC = DOC.DOC_TIP_DOC AND DVF.DVF_NUM_FAC = DOC.DOC_NUM_DOC AND DVF.DVF_NUM_OPE = DOC.DOC_NUM_OPE
            LEFT JOIN PAR PAR ON PAR.PAR_COD_TAB = 77 AND PAR.PAR_COD_PAR = DVF.DVF_EST_VER
            LEFT JOIN PAR PARTD ON PARTD.PAR_COD_TAB = 31 AND PARTD.PAR_COD_PAR = DVF.DVF_TIP_DOC
            WHERE DVF.DVF_FEC_VER = @Fecha";

        if (tipoDocumento.HasValue)
        {
            sql += " AND DVF.DVF_TIP_DOC = @TipoDocumento";
        }

        sql += " ORDER BY DVF.DVF_RUT_CLI, DVF.DVF_RUT_DEU, DVF.DVF_NUM_OPE";

        return await connection.QueryAsync<VerificacionDiaria>(sql, new { Fecha = fecha, TipoDocumento = tipoDocumento });
    }

    /// <summary>
    /// Obtiene documentos pendientes de verificación para una fecha
    /// </summary>
    public async Task<IEnumerable<VerificacionDiaria>> GetPendientesAsync(DateTime? fecha)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                @Fecha as FechaVerificacion,
                DOC.DOC_TIP_DOC as TipoDocumento,
                OPE.OPE_RUT_CLI as RutCliente,
                RTRIM(ISNULL(CLI.CLI_RAZ_SOC, '') + ' ' + ISNULL(CLI.CLI_APE_PAT, '') + ' ' + ISNULL(CLI.CLI_APE_MAT, '')) as NombreCliente,
                DOC.DOC_RUT_DEU as RutDeudor,
                ISNULL(DEU.DEU_RAZ_SOC, '') as NombreDeudor,
                DOC.DOC_NUM_OPE as NumeroOperacion,
                DOC.DOC_NUM_DOC as NumeroDocumento,
                DOC.DOC_FEC_VEN as FechaVencimiento,
                ISNULL(DOC.DOC_MTO_DOC, 0) as MontoDocumento,
                0 as EstadoVerificacion,
                '' as Resultado,
                '' as Observacion,
                '' as UsuarioVerificacion,
                ISNULL(PAR.PAR_DES_PAR, '') as DescripcionTipoDocumento
            FROM DOC DOC
            INNER JOIN OPE OPE ON DOC.DOC_NUM_OPE = OPE.OPE_NUM_OPE
            INNER JOIN CLI CLI ON OPE.OPE_RUT_CLI = CLI.CLI_RUT_CLI
            LEFT JOIN DEU DEU ON DOC.DOC_RUT_DEU = DEU.DEU_RUT_DEU AND OPE.OPE_RUT_CLI = DEU.DEU_RUT_CLI
            LEFT JOIN PAR PAR ON PAR.PAR_COD_TAB = 31 AND PAR.PAR_COD_PAR = DOC.DOC_TIP_DOC
            WHERE OPE.OPE_EST_OPE IN (3, 4) -- Otorgada, Cierre Otorgada
            AND DOC.DOC_EST_DOC = 1 -- Vigente
            AND DOC.DOC_FEC_VEN >= @Fecha
            AND NOT EXISTS (
                SELECT 1 FROM DVF
                WHERE DVF.DVF_RUT_CLI = OPE.OPE_RUT_CLI
                AND DVF.DVF_RUT_DEU = DOC.DOC_RUT_DEU
                AND DVF.DVF_NUM_OPE = DOC.DOC_NUM_OPE
                AND DVF.DVF_NUM_FAC = DOC.DOC_NUM_DOC
                AND DVF.DVF_TIP_DOC = DOC.DOC_TIP_DOC
                AND DVF.DVF_FEC_VER = @Fecha
            )
            ORDER BY DOC.DOC_FEC_VEN, OPE.OPE_RUT_CLI";

        return await connection.QueryAsync<VerificacionDiaria>(sql, new { Fecha = fecha });
    }

    /// <summary>
    /// Registra una nueva verificación diaria
    /// </summary>
    public async Task<bool> RegistrarVerificacionAsync(VerificacionDiaria verificacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO DVF (
                DVF_RUT_CLI, DVF_RUT_DEU, DVF_NUM_OPE, DVF_NUM_FAC, DVF_TIP_DOC,
                DVF_FEC_VER, DVF_HOR_VER, DVF_EST_VER, DVF_OBS_VER, DVF_USR_VER,
                DVF_PER_VER, DVF_CAR_PER, DVF_FAC_REC, DVF_FAC_ING, DVF_ACE_NOT,
                DVF_COB_FAC, DVF_DEP_CLI, DVF_ENT_INF, DVF_TIM_BRE
            ) VALUES (
                @RutCliente, @RutDeudor, @NumeroOperacion, @NumeroDocumento, @TipoDocumento,
                @FechaVerificacion, CONVERT(VARCHAR(5), GETDATE(), 108), @EstadoVerificacion, @Observacion, @UsuarioVerificacion,
                @PersonaContacto, @CargoPersona, @FacturaRecibida, @FacturaIngresada, @AceptaNotificacion,
                @DocumentoCedido, @DepositoCliente, @EntregaInformacion, @TimbreDeudor
            )";

        return await connection.ExecuteAsync(sql, verificacion) > 0;
    }

    /// <summary>
    /// Actualiza el resultado de una verificación existente
    /// </summary>
    public async Task<bool> UpdateResultadoAsync(int numeroOperacion, int tipoDocumento, long numeroDocumento, string resultado, string? observacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE DVF SET
                DVF_EST_VER = @Resultado,
                DVF_OBS_VER = @Observacion,
                DVF_HOR_VER = CONVERT(VARCHAR(5), GETDATE(), 108)
            WHERE DVF_NUM_OPE = @NumeroOperacion
            AND DVF_TIP_DOC = @TipoDocumento
            AND DVF_NUM_FAC = @NumeroDocumento
            AND DVF_FEC_VER = CAST(GETDATE() AS DATE)";

        return await connection.ExecuteAsync(sql, new { NumeroOperacion = numeroOperacion, TipoDocumento = tipoDocumento, NumeroDocumento = numeroDocumento, Resultado = resultado, Observacion = observacion }) > 0;
    }
}

/// <summary>
/// Implementación del repositorio de Documentos No Cedidos
/// </summary>
public class DocumentoNoCedidoRepository : IDocumentoNoCedidoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DocumentoNoCedidoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<DocumentoNoCedido>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT DOC.DOC_NUM_OPE as NumeroOperacion, DOC.DOC_TIP_DOC as TipoDocumento, DOC.DOC_NUM_DOC as NumeroDocumento,
                   OPE.OPE_RUT_CLI as RutCliente, CLI.CLI_NOM_CLI as NombreCliente,
                   DOC.DOC_RUT_DEU as RutDeudor, DEU.DEU_NOM_DEU as NombreDeudor,
                   DOC.DOC_FEC_VEN as FechaVencimiento, DOC.DOC_MTO_DOC as MontoDocumento,
                   PAR.PAR_DES_PAR as DescripcionTipoDocumento
            FROM DOC DOC
            INNER JOIN OPE OPE ON DOC.DOC_NUM_OPE = OPE.OPE_NUM_OPE
            INNER JOIN CLI CLI ON OPE.OPE_RUT_CLI = CLI.CLI_RUT_CLI
            LEFT JOIN DEU DEU ON DOC.DOC_RUT_DEU = DEU.DEU_RUT_DEU
            LEFT JOIN PAR PAR ON PAR.PAR_COD_TAB = 31 AND PAR.PAR_COD_PAR = DOC.DOC_TIP_DOC
            WHERE DOC.DOC_EST_CES = 0 -- No cedido
            ORDER BY DOC.DOC_FEC_VEN";
        return await connection.QueryAsync<DocumentoNoCedido>(sql);
    }

    public async Task<IEnumerable<DocumentoNoCedido>> GetByClienteAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT DOC.DOC_NUM_OPE as NumeroOperacion, DOC.DOC_TIP_DOC as TipoDocumento, DOC.DOC_NUM_DOC as NumeroDocumento,
                   @RutCliente as RutCliente, CLI.CLI_NOM_CLI as NombreCliente,
                   DOC.DOC_RUT_DEU as RutDeudor, DEU.DEU_NOM_DEU as NombreDeudor,
                   DOC.DOC_FEC_VEN as FechaVencimiento, DOC.DOC_MTO_DOC as MontoDocumento
            FROM DOC DOC
            INNER JOIN OPE OPE ON DOC.DOC_NUM_OPE = OPE.OPE_NUM_OPE
            INNER JOIN CLI CLI ON OPE.OPE_RUT_CLI = CLI.CLI_RUT_CLI
            LEFT JOIN DEU DEU ON DOC.DOC_RUT_DEU = DEU.DEU_RUT_DEU
            WHERE OPE.OPE_RUT_CLI = @RutCliente AND DOC.DOC_EST_CES = 0";
        return await connection.QueryAsync<DocumentoNoCedido>(sql, new { RutCliente = rutCliente });
    }

    public async Task<IEnumerable<DocumentoNoCedido>> GetByDeudorAsync(long rutDeudor)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT DOC.DOC_NUM_OPE as NumeroOperacion, DOC.DOC_TIP_DOC as TipoDocumento, DOC.DOC_NUM_DOC as NumeroDocumento,
                   OPE.OPE_RUT_CLI as RutCliente, CLI.CLI_NOM_CLI as NombreCliente,
                   @RutDeudor as RutDeudor, DEU.DEU_NOM_DEU as NombreDeudor,
                   DOC.DOC_FEC_VEN as FechaVencimiento, DOC.DOC_MTO_DOC as MontoDocumento
            FROM DOC DOC
            INNER JOIN OPE OPE ON DOC.DOC_NUM_OPE = OPE.OPE_NUM_OPE
            INNER JOIN CLI CLI ON OPE.OPE_RUT_CLI = CLI.CLI_RUT_CLI
            LEFT JOIN DEU DEU ON DOC.DOC_RUT_DEU = DEU.DEU_RUT_DEU
            WHERE DOC.DOC_RUT_DEU = @RutDeudor AND DOC.DOC_EST_CES = 0";
        return await connection.QueryAsync<DocumentoNoCedido>(sql, new { RutDeudor = rutDeudor });
    }

    public async Task<IEnumerable<DocumentoNoCedido>> GetByFechaAsync(DateTime? fechaDesde, DateTime? fechaHasta)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT DOC.DOC_NUM_OPE as NumeroOperacion, DOC.DOC_TIP_DOC as TipoDocumento, DOC.DOC_NUM_DOC as NumeroDocumento,
                   OPE.OPE_RUT_CLI as RutCliente, CLI.CLI_NOM_CLI as NombreCliente,
                   DOC.DOC_RUT_DEU as RutDeudor, DEU.DEU_NOM_DEU as NombreDeudor,
                   DOC.DOC_FEC_VEN as FechaVencimiento, DOC.DOC_MTO_DOC as MontoDocumento
            FROM DOC DOC
            INNER JOIN OPE OPE ON DOC.DOC_NUM_OPE = OPE.OPE_NUM_OPE
            INNER JOIN CLI CLI ON OPE.OPE_RUT_CLI = CLI.CLI_RUT_CLI
            LEFT JOIN DEU DEU ON DOC.DOC_RUT_DEU = DEU.DEU_RUT_DEU
            WHERE DOC.DOC_EST_CES = 0 AND DOC.DOC_FEC_VEN BETWEEN @FechaDesde AND @FechaHasta";
        return await connection.QueryAsync<DocumentoNoCedido>(sql, new { FechaDesde = fechaDesde, FechaHasta = fechaHasta });
    }

    public async Task<bool> RegistrarNoCedidoAsync(int tipoDocumento, long numeroDocumento, int numeroOperacion, string motivo)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE DOC SET DOC_EST_CES = 0, DOC_MOT_NCS = @Motivo
            WHERE DOC_TIP_DOC = @TipoDocumento AND DOC_NUM_DOC = @NumeroDocumento AND DOC_NUM_OPE = @NumeroOperacion";
        return await connection.ExecuteAsync(sql, new { TipoDocumento = tipoDocumento, NumeroDocumento = numeroDocumento, NumeroOperacion = numeroOperacion, Motivo = motivo }) > 0;
    }
}

/// <summary>
/// Implementación del repositorio de Consulta Masiva de Documentos
/// Permite consultar documentos de múltiples tipos con filtros avanzados
/// </summary>
public class ConsultaMasivaRepository : IConsultaMasivaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ConsultaMasivaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Query base para documentos (unión de FAC, CHE, LET, OTR)
    /// </summary>
    private static string GetBaseQuery()
    {
        return @"
            SELECT * FROM (
                -- Facturas
                SELECT
                    FAC.FAC_NUM_OPE as NumeroOperacion,
                    FAC.FAC_TIP_DOC as TipoDocumento,
                    'Factura' as DescripcionTipoDocumento,
                    FAC.FAC_NUM_FAC as NumeroDocumento,
                    OPE.OPE_RUT_CLI as RutCliente,
                    RTRIM(ISNULL(CLI.CLI_RAZ_SOC, '') + ' ' + ISNULL(CLI.CLI_APE_PAT, '') + ' ' + ISNULL(CLI.CLI_APE_MAT, '')) as NombreCliente,
                    FAC.FAC_RUT_DEU as RutDeudor,
                    ISNULL(DEU.DEU_RAZ_SOC, '') as NombreDeudor,
                    FAC.FAC_FEC_EMI as FechaEmision,
                    FAC.FAC_FEC_VTO as FechaVencimiento,
                    FAC.FAC_VTO_REA as FechaVencimientoReal,
                    FAC.FAC_MON_FAC as MontoDocumento,
                    ISNULL(FAC.FAC_SAL_PEN, FAC.FAC_MON_FAC) as SaldoPendiente,
                    ISNULL(FAC.FAC_MTO_ABO, 0) as MontoAbonado,
                    FAC.FAC_EST_FAC as EstadoDocumento,
                    FAC.FAC_EST_CES as EstadoCesion,
                    DATEDIFF(DAY, FAC.FAC_VTO_REA, GETDATE()) as DiasVencidos,
                    ISNULL(FAC.FAC_PRO_DOC, 0) as Protestado,
                    ISNULL(FAC.FAC_COB_JUD, 0) as EnCobranza,
                    EJE.EJE_NOM_EJE as Ejecutivo,
                    OPE.OPE_COD_EJE as CodigoEjecutivo
                FROM FAC FAC
                INNER JOIN OPE OPE ON FAC.FAC_NUM_OPE = OPE.OPE_NUM_OPE
                INNER JOIN CLI CLI ON OPE.OPE_RUT_CLI = CLI.CLI_RUT_CLI
                LEFT JOIN DEU DEU ON FAC.FAC_RUT_DEU = DEU.DEU_RUT_DEU
                LEFT JOIN EJE EJE ON OPE.OPE_COD_EJE = EJE.EJE_COD_EJE
                WHERE FAC.FAC_EST_FAC NOT IN (5, 6) -- Excluir anulados y pagados

                UNION ALL

                -- Cheques
                SELECT
                    CHE.CHE_NUM_OPE as NumeroOperacion,
                    CHE.CHE_TIP_DOC as TipoDocumento,
                    'Cheque' as DescripcionTipoDocumento,
                    CHE.CHE_NUM_FAC as NumeroDocumento,
                    OPE.OPE_RUT_CLI as RutCliente,
                    RTRIM(ISNULL(CLI.CLI_RAZ_SOC, '') + ' ' + ISNULL(CLI.CLI_APE_PAT, '') + ' ' + ISNULL(CLI.CLI_APE_MAT, '')) as NombreCliente,
                    CHE.CHE_RUT_DEU as RutDeudor,
                    ISNULL(DEU.DEU_RAZ_SOC, '') as NombreDeudor,
                    CHE.CHE_FEC_EMI as FechaEmision,
                    CHE.CHE_FEC_VTO as FechaVencimiento,
                    CHE.CHE_VTO_REA as FechaVencimientoReal,
                    CHE.CHE_MON_FAC as MontoDocumento,
                    ISNULL(CHE.CHE_SAL_PEN, CHE.CHE_MON_FAC) as SaldoPendiente,
                    ISNULL(CHE.CHE_MTO_ABO, 0) as MontoAbonado,
                    CHE.CHE_EST_FAC as EstadoDocumento,
                    CHE.CHE_EST_CES as EstadoCesion,
                    DATEDIFF(DAY, CHE.CHE_VTO_REA, GETDATE()) as DiasVencidos,
                    ISNULL(CHE.CHE_PRO_DOC, 0) as Protestado,
                    ISNULL(CHE.CHE_COB_JUD, 0) as EnCobranza,
                    EJE.EJE_NOM_EJE as Ejecutivo,
                    OPE.OPE_COD_EJE as CodigoEjecutivo
                FROM CHE CHE
                INNER JOIN OPE OPE ON CHE.CHE_NUM_OPE = OPE.OPE_NUM_OPE
                INNER JOIN CLI CLI ON OPE.OPE_RUT_CLI = CLI.CLI_RUT_CLI
                LEFT JOIN DEU DEU ON CHE.CHE_RUT_DEU = DEU.DEU_RUT_DEU
                LEFT JOIN EJE EJE ON OPE.OPE_COD_EJE = EJE.EJE_COD_EJE
                WHERE CHE.CHE_EST_FAC NOT IN (5, 6)

                UNION ALL

                -- Letras
                SELECT
                    LET.LET_NUM_OPE as NumeroOperacion,
                    LET.LET_TIP_DOC as TipoDocumento,
                    'Letra' as DescripcionTipoDocumento,
                    LET.LET_NUM_FAC as NumeroDocumento,
                    OPE.OPE_RUT_CLI as RutCliente,
                    RTRIM(ISNULL(CLI.CLI_RAZ_SOC, '') + ' ' + ISNULL(CLI.CLI_APE_PAT, '') + ' ' + ISNULL(CLI.CLI_APE_MAT, '')) as NombreCliente,
                    LET.LET_RUT_DEU as RutDeudor,
                    ISNULL(DEU.DEU_RAZ_SOC, '') as NombreDeudor,
                    LET.LET_FEC_EMI as FechaEmision,
                    LET.LET_FEC_VTO as FechaVencimiento,
                    LET.LET_VTO_REA as FechaVencimientoReal,
                    LET.LET_MON_FAC as MontoDocumento,
                    ISNULL(LET.LET_SAL_PEN, LET.LET_MON_FAC) as SaldoPendiente,
                    ISNULL(LET.LET_MTO_ABO, 0) as MontoAbonado,
                    LET.LET_EST_FAC as EstadoDocumento,
                    LET.LET_EST_CES as EstadoCesion,
                    DATEDIFF(DAY, LET.LET_VTO_REA, GETDATE()) as DiasVencidos,
                    ISNULL(LET.LET_PRO_DOC, 0) as Protestado,
                    ISNULL(LET.LET_COB_JUD, 0) as EnCobranza,
                    EJE.EJE_NOM_EJE as Ejecutivo,
                    OPE.OPE_COD_EJE as CodigoEjecutivo
                FROM LET LET
                INNER JOIN OPE OPE ON LET.LET_NUM_OPE = OPE.OPE_NUM_OPE
                INNER JOIN CLI CLI ON OPE.OPE_RUT_CLI = CLI.CLI_RUT_CLI
                LEFT JOIN DEU DEU ON LET.LET_RUT_DEU = DEU.DEU_RUT_DEU
                LEFT JOIN EJE EJE ON OPE.OPE_COD_EJE = EJE.EJE_COD_EJE
                WHERE LET.LET_EST_FAC NOT IN (5, 6)

                UNION ALL

                -- Otros documentos
                SELECT
                    OTR.OTR_NUM_OPE as NumeroOperacion,
                    OTR.OTR_TIP_DOC as TipoDocumento,
                    ISNULL(PAR.PAR_DES_PAR, 'Otro') as DescripcionTipoDocumento,
                    OTR.OTR_NUM_FAC as NumeroDocumento,
                    OPE.OPE_RUT_CLI as RutCliente,
                    RTRIM(ISNULL(CLI.CLI_RAZ_SOC, '') + ' ' + ISNULL(CLI.CLI_APE_PAT, '') + ' ' + ISNULL(CLI.CLI_APE_MAT, '')) as NombreCliente,
                    OTR.OTR_RUT_DEU as RutDeudor,
                    ISNULL(DEU.DEU_RAZ_SOC, '') as NombreDeudor,
                    OTR.OTR_FEC_EMI as FechaEmision,
                    OTR.OTR_FEC_VTO as FechaVencimiento,
                    OTR.OTR_VTO_REA as FechaVencimientoReal,
                    OTR.OTR_MON_FAC as MontoDocumento,
                    ISNULL(OTR.OTR_SAL_PEN, OTR.OTR_MON_FAC) as SaldoPendiente,
                    ISNULL(OTR.OTR_MTO_ABO, 0) as MontoAbonado,
                    OTR.OTR_EST_FAC as EstadoDocumento,
                    OTR.OTR_EST_CES as EstadoCesion,
                    DATEDIFF(DAY, OTR.OTR_VTO_REA, GETDATE()) as DiasVencidos,
                    ISNULL(OTR.OTR_PRO_DOC, 0) as Protestado,
                    ISNULL(OTR.OTR_COB_JUD, 0) as EnCobranza,
                    EJE.EJE_NOM_EJE as Ejecutivo,
                    OPE.OPE_COD_EJE as CodigoEjecutivo
                FROM OTR OTR
                INNER JOIN OPE OPE ON OTR.OTR_NUM_OPE = OPE.OPE_NUM_OPE
                INNER JOIN CLI CLI ON OPE.OPE_RUT_CLI = CLI.CLI_RUT_CLI
                LEFT JOIN DEU DEU ON OTR.OTR_RUT_DEU = DEU.DEU_RUT_DEU
                LEFT JOIN PAR PAR ON PAR.PAR_COD_TAB = 31 AND PAR.PAR_COD_PAR = OTR.OTR_TIP_DOC
                LEFT JOIN EJE EJE ON OPE.OPE_COD_EJE = EJE.EJE_COD_EJE
                WHERE OTR.OTR_EST_FAC NOT IN (5, 6)
            ) AS Documentos";
    }

    public async Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosAsync(FiltroConsultaMasivaDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = GetBaseQuery() + " WHERE 1=1";
        var parameters = new DynamicParameters();

        if (filtro.RutCliente.HasValue)
        {
            sql += " AND RutCliente = @RutCliente";
            parameters.Add("RutCliente", filtro.RutCliente.Value);
        }

        if (filtro.RutDeudor.HasValue)
        {
            sql += " AND RutDeudor = @RutDeudor";
            parameters.Add("RutDeudor", filtro.RutDeudor.Value);
        }

        if (filtro.TipoDocumento.HasValue)
        {
            sql += " AND TipoDocumento = @TipoDocumento";
            parameters.Add("TipoDocumento", filtro.TipoDocumento.Value);
        }

        if (filtro.NumeroDocumento.HasValue)
        {
            sql += " AND NumeroDocumento = @NumeroDocumento";
            parameters.Add("NumeroDocumento", filtro.NumeroDocumento.Value);
        }

        if (filtro.NumeroOperacion.HasValue)
        {
            sql += " AND NumeroOperacion = @NumeroOperacion";
            parameters.Add("NumeroOperacion", filtro.NumeroOperacion.Value);
        }

        if (filtro.FechaEmisionDesde.HasValue)
        {
            sql += " AND FechaEmision >= @FechaEmisionDesde";
            parameters.Add("FechaEmisionDesde", filtro.FechaEmisionDesde.Value);
        }

        if (filtro.FechaEmisionHasta.HasValue)
        {
            sql += " AND FechaEmision <= @FechaEmisionHasta";
            parameters.Add("FechaEmisionHasta", filtro.FechaEmisionHasta.Value);
        }

        if (filtro.FechaVencimientoDesde.HasValue)
        {
            sql += " AND FechaVencimientoReal >= @FechaVencimientoDesde";
            parameters.Add("FechaVencimientoDesde", filtro.FechaVencimientoDesde.Value);
        }

        if (filtro.FechaVencimientoHasta.HasValue)
        {
            sql += " AND FechaVencimientoReal <= @FechaVencimientoHasta";
            parameters.Add("FechaVencimientoHasta", filtro.FechaVencimientoHasta.Value);
        }

        if (filtro.MontoMinimo.HasValue)
        {
            sql += " AND MontoDocumento >= @MontoMinimo";
            parameters.Add("MontoMinimo", filtro.MontoMinimo.Value);
        }

        if (filtro.MontoMaximo.HasValue)
        {
            sql += " AND MontoDocumento <= @MontoMaximo";
            parameters.Add("MontoMaximo", filtro.MontoMaximo.Value);
        }

        if (filtro.EstadoDocumento.HasValue)
        {
            sql += " AND EstadoDocumento = @EstadoDocumento";
            parameters.Add("EstadoDocumento", filtro.EstadoDocumento.Value);
        }

        if (filtro.EstadoCesion.HasValue)
        {
            sql += " AND EstadoCesion = @EstadoCesion";
            parameters.Add("EstadoCesion", filtro.EstadoCesion.Value);
        }

        if (filtro.SoloVencidos == true)
        {
            sql += " AND DiasVencidos > 0";
        }

        if (filtro.SoloProtestados == true)
        {
            sql += " AND Protestado = 1";
        }

        if (filtro.SoloEnCobranza == true)
        {
            sql += " AND EnCobranza = 1";
        }

        if (filtro.CodigoEjecutivo.HasValue)
        {
            sql += " AND CodigoEjecutivo = @CodigoEjecutivo";
            parameters.Add("CodigoEjecutivo", filtro.CodigoEjecutivo.Value);
        }

        sql += " ORDER BY FechaVencimientoReal DESC";

        return await connection.QueryAsync<DocumentoConsultaMasiva>(sql, parameters);
    }

    public async Task<ResumenConsultaMasivaDto> GetResumenAsync(FiltroConsultaMasivaDto filtro)
    {
        var documentos = await GetDocumentosAsync(filtro);
        var lista = documentos.ToList();

        var resumen = new ResumenConsultaMasivaDto
        {
            TotalDocumentos = lista.Count,
            MontoTotal = lista.Sum(d => d.MontoDocumento),
            SaldoTotal = lista.Sum(d => d.SaldoPendiente),
            DocumentosVigentes = lista.Count(d => d.DiasVencidos <= 0),
            MontoVigente = lista.Where(d => d.DiasVencidos <= 0).Sum(d => d.MontoDocumento),
            DocumentosVencidos = lista.Count(d => d.DiasVencidos > 0),
            MontoVencido = lista.Where(d => d.DiasVencidos > 0).Sum(d => d.MontoDocumento),
            DocumentosProtestados = lista.Count(d => d.Protestado),
            MontoProtestado = lista.Where(d => d.Protestado).Sum(d => d.MontoDocumento),
            DocumentosEnCobranza = lista.Count(d => d.EnCobranza),
            MontoEnCobranza = lista.Where(d => d.EnCobranza).Sum(d => d.MontoDocumento),
            PorTipoDocumento = lista
                .GroupBy(d => new { d.TipoDocumento, d.DescripcionTipoDocumento })
                .Select(g => new ResumenPorTipoDocumentoDto
                {
                    TipoDocumento = g.Key.TipoDocumento,
                    DescripcionTipoDocumento = g.Key.DescripcionTipoDocumento,
                    Cantidad = g.Count(),
                    Monto = g.Sum(d => d.MontoDocumento),
                    Saldo = g.Sum(d => d.SaldoPendiente)
                }).ToList(),
            PorEstado = lista
                .GroupBy(d => d.EstadoDocumento)
                .Select(g => new ResumenEstadoConsultaMasivaDto
                {
                    Estado = g.Key,
                    DescripcionEstado = GetDescripcionEstado(g.Key),
                    Cantidad = g.Count(),
                    Monto = g.Sum(d => d.MontoDocumento)
                }).ToList()
        };

        return resumen;
    }

    private static string GetDescripcionEstado(int estado)
    {
        return estado switch
        {
            1 => "Vigente",
            2 => "Vencido",
            3 => "En Cobranza",
            4 => "Protestado",
            5 => "Pagado",
            6 => "Anulado",
            _ => "Otro"
        };
    }

    public async Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosVencidosAsync(int? diasVencidos = null)
    {
        var filtro = new FiltroConsultaMasivaDto { SoloVencidos = true };
        var documentos = await GetDocumentosAsync(filtro);

        if (diasVencidos.HasValue)
        {
            documentos = documentos.Where(d => d.DiasVencidos >= diasVencidos.Value);
        }

        return documentos;
    }

    public async Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosByClienteAsync(long rutCliente)
    {
        var filtro = new FiltroConsultaMasivaDto { RutCliente = rutCliente };
        return await GetDocumentosAsync(filtro);
    }

    public async Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosByDeudorAsync(long rutDeudor)
    {
        var filtro = new FiltroConsultaMasivaDto { RutDeudor = rutDeudor };
        return await GetDocumentosAsync(filtro);
    }

    public async Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosByOperacionAsync(int numeroOperacion)
    {
        var filtro = new FiltroConsultaMasivaDto { NumeroOperacion = numeroOperacion };
        return await GetDocumentosAsync(filtro);
    }

    public async Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosProtestadosAsync()
    {
        var filtro = new FiltroConsultaMasivaDto { SoloProtestados = true };
        return await GetDocumentosAsync(filtro);
    }

    public async Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosEnCobranzaAsync()
    {
        var filtro = new FiltroConsultaMasivaDto { SoloEnCobranza = true };
        return await GetDocumentosAsync(filtro);
    }
}
