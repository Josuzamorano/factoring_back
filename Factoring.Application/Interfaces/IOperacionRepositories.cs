using Factoring.Domain.Entities;

namespace Factoring.Application.Interfaces;

/// <summary>
/// Repositorio para Operaciones - Versión simplificada para inicio
/// </summary>
public interface IOperacionRepository
{
    Task<IEnumerable<Operacion>> GetAllAsync();
    Task<Operacion?> GetByIdAsync(long numeroOperacion);
    Task<Operacion?> GetFullByIdAsync(long numeroOperacion);
    Task<IEnumerable<Operacion>> GetByClienteAsync(long rutCliente);
    Task<IEnumerable<Operacion>> SearchAsync(DateTime? fechaDesde, DateTime? fechaHasta, 
        long? rutCliente, int? estado, int? tipoDocumento);
    Task<long> CreateAsync(Operacion operacion);
    Task<bool> UpdateAsync(Operacion operacion);
    Task<bool> DeleteAsync(long numeroOperacion);
    Task<bool> CambiarEstadoAsync(long numeroOperacion, int nuevoEstado);
    Task<bool> OtorgarAsync(long numeroOperacion, DateTime? fechaOtorgamiento);
    Task<long> GetNextNumeroAsync();
}

/// <summary>
/// Repositorio para Facturas Otorgadas
/// </summary>
public interface IFacturaOtorgadaRepository
{
    Task<IEnumerable<FacturaOtorgada>> GetByOperacionAsync(long numeroOperacion);
    Task<FacturaOtorgada?> GetByIdAsync(long numeroOperacion, int correlativoDocumento);
    Task<int> CreateAsync(FacturaOtorgada factura);
    Task<bool> UpdateAsync(FacturaOtorgada factura);
    Task<bool> DeleteAsync(long numeroOperacion, int correlativoDocumento);
    Task<bool> DeleteByOperacionAsync(long numeroOperacion);
    Task<bool> VerificarAsync(long numeroOperacion, int correlativoDocumento, bool verificado);
    Task<bool> ActualizarSaldosAsync(long numeroOperacion, int correlativoDocumento, decimal montoAbono);
}

/// <summary>
/// Repositorio para Cheques Otorgados
/// </summary>
public interface IChequeOtorgadoRepository
{
    Task<IEnumerable<ChequeOtorgado>> GetByOperacionAsync(long numeroOperacion);
    Task<ChequeOtorgado?> GetByIdAsync(long numeroOperacion, int correlativoDocumento);
    Task<int> CreateAsync(ChequeOtorgado cheque);
    Task<bool> UpdateAsync(ChequeOtorgado cheque);
    Task<bool> DeleteAsync(long numeroOperacion, int correlativoDocumento);
    Task<bool> DeleteByOperacionAsync(long numeroOperacion);
}

/// <summary>
/// Repositorio para Letras Otorgadas
/// </summary>
public interface ILetraOtorgadaRepository
{
    Task<IEnumerable<LetraOtorgada>> GetByOperacionAsync(long numeroOperacion);
    Task<LetraOtorgada?> GetByIdAsync(long numeroOperacion, int correlativoDocumento);
    Task<int> CreateAsync(LetraOtorgada letra);
    Task<bool> UpdateAsync(LetraOtorgada letra);
    Task<bool> DeleteAsync(long numeroOperacion, int correlativoDocumento);
    Task<bool> DeleteByOperacionAsync(long numeroOperacion);
}

/// <summary>
/// Repositorio para Otros Documentos Otorgados
/// </summary>
public interface IOtroDocumentoOtorgadoRepository
{
    Task<IEnumerable<OtroDocumentoOtorgado>> GetByOperacionAsync(long numeroOperacion);
    Task<OtroDocumentoOtorgado?> GetByIdAsync(long numeroOperacion, int correlativoDocumento);
    Task<int> CreateAsync(OtroDocumentoOtorgado otro);
    Task<bool> UpdateAsync(OtroDocumentoOtorgado otro);
    Task<bool> DeleteAsync(long numeroOperacion, int correlativoDocumento);
    Task<bool> DeleteByOperacionAsync(long numeroOperacion);
}

/// <summary>
/// Repositorio unificado para todos los documentos de operación
/// </summary>
public interface IDocumentoOperacionRepository
{
    Task<IEnumerable<DocumentoOperacion>> GetByOperacionAsync(long numeroOperacion);
    Task<decimal> GetTotalByOperacionAsync(long numeroOperacion);
}

/// <summary>
/// Repositorio para Tipos de Gasto
/// </summary>
public interface ITipoGastoRepository
{
    Task<IEnumerable<TipoGasto>> GetAllAsync();
    Task<TipoGasto?> GetByIdAsync(int codigoGasto);
}

/// <summary>
/// Repositorio para Gastos de Operación
/// </summary>
public interface IGastoOperacionRepository
{
    Task<IEnumerable<GastoOperacion>> GetByOperacionAsync(long numeroOperacion);
    Task<int> CreateAsync(GastoOperacion gasto);
    Task<bool> DeleteAsync(long numeroOperacion, int correlativoGasto);
    Task<bool> DeleteByOperacionAsync(long numeroOperacion);
    Task<decimal> GetTotalGastosAsync(long numeroOperacion);
}
