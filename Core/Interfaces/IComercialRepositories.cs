// ====================================================
// Interfaces de Repositorios del Módulo Comercial
// Migrado de: F_CM1000.frm, F_CM1200.frm, F_CM1400.frm, 
//             F_CM1500.frm, F_CM1700.frm, F_CM1800.frm
// ====================================================

using Factoring.Core.DTOs;
using Factoring.Core.Entities;

namespace Factoring.Core.Interfaces;

/// <summary>
/// Repositorio para gestión de excedentes
/// Migrado de: F_CM1200.frm
/// </summary>
public interface IExcedenteRepository
{
    /// <summary>
    /// Obtiene lista de excedentes con filtros
    /// </summary>
    Task<IEnumerable<ExcedenteDto>> GetExcedentesAsync(FiltroExcedenteDto filtro);
    
    /// <summary>
    /// Obtiene excedente por número
    /// </summary>
    Task<ExcedenteDto?> GetExcedenteByIdAsync(int numeroExcedente);
    
    /// <summary>
    /// Obtiene resumen de excedentes por cliente
    /// </summary>
    Task<IEnumerable<ResumenExcedenteDto>> GetResumenExcedentesAsync(FiltroExcedenteDto filtro);
    
    /// <summary>
    /// Obtiene detalle de excedentes disponibles para un cliente
    /// </summary>
    Task<IEnumerable<ExcedenteDetalleDto>> GetExcedentesDisponiblesAsync(long rutCliente);
    
    /// <summary>
    /// Aplica excedente a un documento
    /// </summary>
    Task<bool> AplicarExcedenteAsync(AplicarExcedenteDto dto, string usuario);
    
    /// <summary>
    /// Obtiene el saldo disponible de un excedente
    /// </summary>
    Task<decimal> GetSaldoExcedenteAsync(int numeroExcedente);
}

/// <summary>
/// Repositorio para gestión de prórrogas
/// Migrado de: F_CM1400.frm, F_CM1800.frm
/// </summary>
public interface IProrrogaRepository
{
    /// <summary>
    /// Obtiene lista de prórrogas con filtros
    /// </summary>
    Task<IEnumerable<ProrrogaDto>> GetProrrogasAsync(FiltroProrrogaDto filtro);
    
    /// <summary>
    /// Obtiene prórroga por número
    /// </summary>
    Task<ProrrogaDto?> GetProrrogaByIdAsync(int numeroProrrroga);
    
    /// <summary>
    /// Obtiene prórrogas pendientes de aprobación
    /// </summary>
    Task<IEnumerable<ProrrogaDto>> GetProrrogasPendientesAsync();
    
    /// <summary>
    /// Crea nueva prórroga
    /// </summary>
    Task<int> CreateProrrogaAsync(CreateProrrogaDto dto, string usuario);
    
    /// <summary>
    /// Aprueba o rechaza prórroga
    /// </summary>
    Task<bool> ProcesarProrrogaAsync(AccionProrrogaDto dto, string usuario);
    
    /// <summary>
    /// Contabiliza prórroga aprobada
    /// </summary>
    Task<bool> ContabilizarProrrogaAsync(int numeroProrrroga, string usuario);
    
    /// <summary>
    /// Obtiene documento para prórroga
    /// </summary>
    Task<DocumentoClienteDto?> GetDocumentoParaProrrogaAsync(int tipoDocumento, string numeroDocumento);
}

/// <summary>
/// Repositorio para gestión de renovaciones
/// Migrado de: F_CM1500.frm
/// </summary>
public interface IRenovacionRepository
{
    /// <summary>
    /// Obtiene lista de renovaciones con filtros
    /// </summary>
    Task<IEnumerable<RenovacionDto>> GetRenovacionesAsync(FiltroRenovacionDto filtro);
    
    /// <summary>
    /// Obtiene renovación por número
    /// </summary>
    Task<RenovacionDto?> GetRenovacionByIdAsync(int numeroRenovacion);
    
    /// <summary>
    /// Crea nueva renovación
    /// </summary>
    Task<int> CreateRenovacionAsync(CreateRenovacionDto dto, string usuario);
    
    /// <summary>
    /// Aprueba renovación
    /// </summary>
    Task<bool> AprobarRenovacionAsync(int numeroRenovacion, string usuario);
    
    /// <summary>
    /// Rechaza renovación
    /// </summary>
    Task<bool> RechazarRenovacionAsync(int numeroRenovacion, string usuario, string? motivo);
}

/// <summary>
/// Repositorio para informe de documentos por cliente
/// Migrado de: F_CM1700.frm
/// </summary>
public interface IDocumentoClienteRepository
{
    /// <summary>
    /// Obtiene documentos de un cliente
    /// </summary>
    Task<IEnumerable<DocumentoClienteDto>> GetDocumentosClienteAsync(FiltroDocumentoClienteDto filtro);
    
    /// <summary>
    /// Obtiene resumen de documentos por cliente
    /// </summary>
    Task<ResumenDocumentoClienteDto> GetResumenDocumentosAsync(long rutCliente);
    
    /// <summary>
    /// Obtiene documentos vencidos de un cliente
    /// </summary>
    Task<IEnumerable<DocumentoClienteDto>> GetDocumentosVencidosAsync(long rutCliente);
    
    /// <summary>
    /// Obtiene documentos por vencer en los próximos N días
    /// </summary>
    Task<IEnumerable<DocumentoClienteDto>> GetDocumentosPorVencerAsync(long rutCliente, int dias);
}

/// <summary>
/// Repositorio para consulta de otorgamientos
/// Migrado de: F_PUBLI05.frm
/// </summary>
public interface IConsultaOtorgamientoRepository
{
    /// <summary>
    /// Obtiene lista de otorgamientos
    /// </summary>
    Task<IEnumerable<ConsultaOtorgamientoDto>> GetOtorgamientosAsync(FiltroOtorgamientoDto filtro);
    
    /// <summary>
    /// Obtiene detalle de un otorgamiento
    /// </summary>
    Task<ConsultaOtorgamientoDto?> GetOtorgamientoByIdAsync(int numeroOperacion);
}

/// <summary>
/// Repositorio para cartola de cliente
/// Migrado de: F_GC1900.frm
/// </summary>
public interface ICartolaClienteRepository
{
    /// <summary>
    /// Obtiene cartola de movimientos del cliente
    /// </summary>
    Task<IEnumerable<CartolaClienteDto>> GetCartolaAsync(FiltroCartolaClienteDto filtro);
    
    /// <summary>
    /// Obtiene resumen de cartola
    /// </summary>
    Task<ResumenCartolaClienteDto> GetResumenCartolaAsync(long rutCliente);
}

/// <summary>
/// Repositorio para pizarra comercial
/// Migrado de: F_PUBLI04.frm
/// </summary>
public interface IPizarraComercialRepository
{
    /// <summary>
    /// Obtiene pizarra de giros
    /// </summary>
    Task<IEnumerable<PizarraComercialDto>> GetPizarraAsync(FiltroPizarraDto filtro);
}
