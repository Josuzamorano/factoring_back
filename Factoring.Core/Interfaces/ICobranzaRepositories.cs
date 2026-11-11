using Factoring.Core.DTOs;
using Factoring.Core.Entities;

namespace Factoring.Core.Interfaces;

/// <summary>
/// Repositorio para pagos de documentos (deudor y cliente)
/// </summary>
public interface IPagoDocumentoRepository
{
    // CRUD Pagos
    Task<IEnumerable<PagoDocumento>> GetAllAsync();
    Task<PagoDocumento?> GetByIdAsync(int numeroPago);
    Task<int> CreateAsync(PagoDocumentoCreateDto dto);
    Task<bool> UpdateEstadoAsync(PagoDocumentoUpdateDto dto);
    Task<bool> DeleteAsync(int numeroPago);
    
    // Consultas específicas
    Task<IEnumerable<PagoDocumento>> GetByFechaAsync(DateTime? fecha);
    Task<IEnumerable<PagoDocumento>> GetByClienteAsync(long rutCliente);
    Task<IEnumerable<PagoDocumento>> GetByDeudorAsync(long rutDeudor);
    Task<IEnumerable<PagoDocumento>> GetByTipoPagoAsync(int tipoPago); // 1=Deudor, 2=Cliente
    Task<IEnumerable<PagoDocumento>> GetByEstadoAsync(int estadoPago);
    Task<IEnumerable<PagoDocumento>> GetPendientesAprobacionAsync();
    
    // Documentos disponibles para pago
    Task<IEnumerable<DocumentoPagarDto>> GetDocumentosPagarDeudorAsync(long rutDeudor, int? tipoDocumento = null);
    Task<IEnumerable<DocumentoPagarDto>> GetDocumentosPagarClienteAsync(long rutCliente, int? tipoDocumento = null);
    Task<DocumentoPagarDto?> GetDetalleDocumentoAsync(int tipoDocumento, long numeroDocumento, int numeroOperacion);
    
    // Cálculos
    Task<decimal> CalcularMoraAsync(int tipoDocumento, long numeroDocumento, DateTime? fechaPago);
    Task<decimal> GetSaldoPendienteAsync(int tipoDocumento, long numeroDocumento, int numeroOperacion);
}

/// <summary>
/// Repositorio para recaudación (cheques de pago)
/// </summary>
public interface IRecaudacionDocumentoRepository
{
    Task<IEnumerable<RecaudacionDocumento>> GetAllAsync();
    Task<RecaudacionDocumento?> GetByIdAsync(int numeroRecaudacion);
    Task<IEnumerable<RecaudacionDocumento>> GetByPagoAsync(int numeroPago);
    Task<int> CreateAsync(RecaudacionDocumentoCreateDto dto);
    Task<bool> UpdateAsync(RecaudacionDocumento entity);
    Task<bool> DeleteAsync(int numeroRecaudacion);
    
    // Consultas por fecha
    Task<IEnumerable<RecaudacionDocumento>> GetByFechaDepositoAsync(DateTime? fechaDesde, DateTime? fechaHasta);
    Task<IEnumerable<RecaudacionDocumento>> GetEnRetencionAsync();
}

/// <summary>
/// Repositorio para verificación de documentos
/// </summary>
public interface IVerificacionRepository
{
    // Verificación general (no otorgados)
    Task<IEnumerable<VerificacionDocumento>> GetVerificacionAsync(FiltroVerificacionDto filtro);
    
    // Verificación otorgados
    Task<IEnumerable<VerificacionDocumento>> GetVerificacionOtorgadosAsync(FiltroVerificacionDto filtro);
    
    // Verificación por carátula
    Task<IEnumerable<VerificacionDocumento>> GetVerificacionCaratulaAsync(FiltroVerificacionDto filtro);
    
    // Detalle de verificación
    Task<IEnumerable<DetalleVerificacion>> GetDetalleVerificacionAsync(long rutCliente, long rutDeudor, int numeroOperacion);
    
    // Actualizar estado verificación
    Task<bool> MarcarVerificadoAsync(int tipoDocumento, long numeroDocumento, int numeroOperacion, bool verificado);
}

/// <summary>
/// Repositorio para caja (gestión de pagos)
/// </summary>
public interface ICajaRepository
{
    // Consultas
    Task<IEnumerable<MovimientoCaja>> GetMovimientosCajaAsync(FiltroCajaDto filtro);
    Task<MovimientoCaja?> GetByIdAsync(int numeroMovimiento);
    
    // Resúmenes
    Task<ResumenCaja> GetResumenCajaAsync(DateTime? fecha, int? tipoDocumento = null);
    Task<IEnumerable<ResumenCaja>> GetResumenCajaPorTipoAsync(DateTime? fecha);
    
    // Operaciones
    Task<bool> AprobarPagoAsync(int numeroMovimiento, string usuario);
    Task<bool> RechazarPagoAsync(int numeroMovimiento, string usuario, string? motivo);
    Task<bool> EliminarPagoAsync(int numeroMovimiento, string usuario);
    
    // Consultas por estado
    Task<IEnumerable<MovimientoCaja>> GetPagosActualesAsync(DateTime? fecha);
    Task<IEnumerable<MovimientoCaja>> GetPagosEnRetencionAsync(DateTime? fecha);
    Task<IEnumerable<MovimientoCaja>> GetPagosProcesadosAsync(DateTime? fecha);
}

/// <summary>
/// Repositorio para verificación diaria
/// </summary>
public interface IVerificacionDiariaRepository
{
    Task<IEnumerable<VerificacionDiaria>> GetByFechaAsync(DateTime? fecha, int? tipoDocumento = null);
    Task<IEnumerable<VerificacionDiaria>> GetPendientesAsync(DateTime? fecha);
    Task<bool> RegistrarVerificacionAsync(VerificacionDiaria verificacion);
    Task<bool> UpdateResultadoAsync(int numeroOperacion, int tipoDocumento, long numeroDocumento, string resultado, string? observacion);
}

/// <summary>
/// Repositorio para documentos no cedidos
/// </summary>
public interface IDocumentoNoCedidoRepository
{
    Task<IEnumerable<DocumentoNoCedido>> GetAllAsync();
    Task<IEnumerable<DocumentoNoCedido>> GetByClienteAsync(long rutCliente);
    Task<IEnumerable<DocumentoNoCedido>> GetByDeudorAsync(long rutDeudor);
    Task<IEnumerable<DocumentoNoCedido>> GetByFechaAsync(DateTime? fechaDesde, DateTime? fechaHasta);
    Task<bool> RegistrarNoCedidoAsync(int tipoDocumento, long numeroDocumento, int numeroOperacion, string motivo);
}

/// <summary>
/// Repositorio para informe de abonos
/// </summary>
public interface IInformeAbonoRepository
{
    Task<IEnumerable<InformeAbono>> GetAbonosAsync(FiltroInformeAbonoDto filtro);
    Task<ResumenAbonoDto> GetResumenAbonosAsync(FiltroInformeAbonoDto filtro);
    Task<IEnumerable<InformeAbono>> GetAbonosByCobradorAsync(string codigoCobrador, DateTime? fechaDesde, DateTime? fechaHasta);
}

/// <summary>
/// Repositorio para gastos de cobranza
/// </summary>
public interface IGastoCobranzaRepository
{
    Task<IEnumerable<GastoCobranza>> GetByPagoAsync(int numeroPago);
    Task<int> CreateAsync(GastoCobranzaCreateDto dto);
    Task<bool> DeleteAsync(int numeroGasto);
    Task<IEnumerable<GastoCobranza>> GetTiposGastoAsync();
}

/// <summary>
/// Repositorio para consulta masiva de documentos
/// </summary>
public interface IConsultaMasivaRepository
{
    /// <summary>
    /// Obtiene documentos según filtros
    /// </summary>
    Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosAsync(FiltroConsultaMasivaDto filtro);

    /// <summary>
    /// Obtiene resumen de documentos según filtros
    /// </summary>
    Task<ResumenConsultaMasivaDto> GetResumenAsync(FiltroConsultaMasivaDto filtro);

    /// <summary>
    /// Obtiene documentos vencidos
    /// </summary>
    Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosVencidosAsync(int? diasVencidos = null);

    /// <summary>
    /// Obtiene documentos por cliente
    /// </summary>
    Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosByClienteAsync(long rutCliente);

    /// <summary>
    /// Obtiene documentos por deudor
    /// </summary>
    Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosByDeudorAsync(long rutDeudor);

    /// <summary>
    /// Obtiene documentos por operación
    /// </summary>
    Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosByOperacionAsync(int numeroOperacion);

    /// <summary>
    /// Obtiene documentos protestados
    /// </summary>
    Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosProtestadosAsync();

    /// <summary>
    /// Obtiene documentos en cobranza judicial
    /// </summary>
    Task<IEnumerable<DocumentoConsultaMasiva>> GetDocumentosEnCobranzaAsync();
}
