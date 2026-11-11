using Factoring.Core.Entities;
using Factoring.Core.DTOs;

namespace Factoring.Core.Interfaces;

/// <summary>
/// Repositorio de Giros - Migrado de F_TS1000.frm
/// </summary>
public interface IGiroRepository
{
    Task<IEnumerable<Giro>> GetAllAsync();
    Task<IEnumerable<Giro>> GetByFechaAsync(DateTime? fechaDesde, DateTime? fechaHasta);
    Task<IEnumerable<Giro>> GetByClienteAsync(long rutCliente);
    Task<IEnumerable<Giro>> GetByOperacionAsync(int numeroOperacion);
    Task<IEnumerable<Giro>> GetByFiltroAsync(FiltroTesoreriaDto filtro);
    Task<Giro?> GetByIdAsync(int numeroGiro);
    Task<int> CreateAsync(Giro giro);
    Task<bool> UpdateAsync(Giro giro);
    Task<bool> UpdateEstadoAsync(int numeroGiro, int nuevoEstado, string usuario);
    Task<bool> AnularAsync(int numeroGiro, string usuario);
    Task<ResumenGirosDto> GetResumenByFechaAsync(DateTime? fecha);
    Task<int> GetNextNumeroGiroAsync();
}

/// <summary>
/// Repositorio de Movimientos - Migrado de F_TS1800.frm
/// </summary>
public interface IMovimientoRepository
{
    Task<IEnumerable<Movimiento>> GetAllAsync();
    Task<IEnumerable<Movimiento>> GetByFechaAsync(DateTime? fechaDesde, DateTime? fechaHasta);
    Task<IEnumerable<Movimiento>> GetByClienteAsync(long rutCliente);
    Task<IEnumerable<Movimiento>> GetByFiltroAsync(FiltroTesoreriaDto filtro);
    Task<Movimiento?> GetByIdAsync(int numeroMovimiento);
    Task<int> CreateAsync(Movimiento movimiento);
    Task<bool> UpdateAsync(Movimiento movimiento);
    Task<bool> DeleteAsync(int numeroMovimiento);
    Task<ResumenMovimientosDto> GetResumenByFechaAsync(DateTime? fecha);
    Task<IEnumerable<Movimiento>> GetPizarraAsync(DateTime? fecha);
}

/// <summary>
/// Repositorio de Arqueo - Migrado de F_TS2100.frm
/// Consulta documentos consolidados de FAO, CHO, LEO para informes de arqueo
/// </summary>
public interface IArqueoRepository
{
    Task<IEnumerable<ArqueoDocumento>> GetByFiltroAsync(FiltroArqueoDto filtro);
    Task<IEnumerable<ArqueoDocumento>> GetByClienteAsync(long rutCliente, DateTime? fechaDesde, DateTime? fechaHasta);
    Task<IEnumerable<ArqueoDocumento>> GetByTipoDocumentoAsync(string tipoDocumento, DateTime? fechaDesde, DateTime? fechaHasta);
    Task<ResumenArqueoDto> GetResumenAsync(FiltroArqueoDto filtro);
    Task<IEnumerable<ArqueoDocumento>> GetDocumentosVencidosAsync(DateTime? fechaCorte);
    Task<IEnumerable<ArqueoDocumento>> GetDocumentosPorVencerAsync(DateTime? fechaDesde, DateTime? fechaHasta);
}
