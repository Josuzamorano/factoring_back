using Factoring.Core.Entities;
using Factoring.Core.DTOs;

namespace Factoring.Core.Interfaces;

/// <summary>
/// Repositorio de Gestión de Cobranza - Migrado de F_GC1100.frm
/// Gestiona las gestiones telefónicas de cobranza
/// </summary>
public interface IGestionCobranzaRepository
{
    Task<IEnumerable<GestionCobranza>> GetByDocumentoAsync(long rutCliente, long rutDeudor, int numeroOperacion, int tipoDocumento, long numeroDocumento);
    Task<IEnumerable<GestionCobranza>> GetByFiltroAsync(FiltroGestionCobranzaDto filtro);
    Task<GestionCobranza?> GetUltimaGestionAsync(long rutCliente, long rutDeudor, int numeroOperacion, int tipoDocumento, long numeroDocumento);
    Task<int> CreateAsync(GestionCobranza gestion);
    Task<bool> UpdateEstadoDocumentoAsync(long rutCliente, long rutDeudor, int numeroOperacion, long numeroDocumento, int tipoDocumento, string codigoCobrador, int estadoCobranza, DateTime? fechaNotificacion);
}

/// <summary>
/// Repositorio de Cobradores para Recaudación - Migrado de F_GC1000.frm
/// Extiende funcionalidades del ICobradorRepository de Clientes
/// </summary>
public interface ICobradorRecaudacionRepository
{
    Task<IEnumerable<CobradorRecaudacion>> GetAllAsync();
    Task<CobradorRecaudacion?> GetByCodigoAsync(string codigoCobrador);
    Task<IEnumerable<CobradorRecaudacion>> GetActivosAsync();
    Task<int> CreateAsync(CobradorRecaudacion cobrador);
    Task<bool> UpdateAsync(CobradorRecaudacion cobrador);
    Task<bool> DeleteAsync(string codigoCobrador);
}

/// <summary>
/// Repositorio de Documentos para Gestión - Vista combinada FAO, CHO, LEO, OTO
/// </summary>
public interface IDocumentoGestionRepository
{
    Task<IEnumerable<DocumentoGestion>> GetByClienteDeudorAsync(long rutCliente, long rutDeudor);
    Task<IEnumerable<DocumentoGestion>> GetByCobradorAsync(string codigoCobrador);
    Task<IEnumerable<DeudorGestion>> GetDeudoresByCobradorAsync(string codigoCobrador);
    Task<IEnumerable<ClienteCobrador>> GetClientesByCobradorDeudorAsync(string codigoCobrador, long rutDeudor);
    Task<bool> UpdateDeudorContactoAsync(long rutCliente, long rutDeudor, string? contactoCobranza, string? direccionPago);
}

/// <summary>
/// Repositorio de Estados de Cobranza - Migrado de F_GC2600.frm
/// Informe de estados de cobranza
/// </summary>
public interface IEstadoCobranzaRepository
{
    Task<IEnumerable<EstadoCobranzaDocumento>> GetByFiltroAsync(FiltroEstadoCobranzaDto filtro);
    Task<ResumenEstadoCobranzaDto> GetResumenAsync(FiltroEstadoCobranzaDto filtro);
}

/// <summary>
/// Repositorio de Deuda Total Cliente - Migrado de F_GC2200.frm
/// Cálculo de deuda total del cliente
/// </summary>
public interface IDeudaTotalClienteRepository
{
    Task<DeudaTotalCliente> CalcularDeudaTotalAsync(long rutCliente, DateTime? fechaCalculo);
    Task<IEnumerable<DetalleDeudaDocumento>> GetDetalleDeudaByTipoAsync(long rutCliente, int tipoDocumento, DateTime? fechaCalculo);
}

