using System.Collections.Generic;
using System.Threading.Tasks;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;

namespace Factoring.Core.Interfaces;

/// <summary>
/// Repositorio para Líneas de Crédito
/// Migrado de F_LC1100.frm, F_LC1400.frm
/// </summary>
public interface ILineaCreditoRepository
{
    /// <summary>
    /// Obtiene todas las líneas de crédito con filtros opcionales
    /// </summary>
    Task<IEnumerable<LineaCreditoDto>> GetAllAsync(FiltroLineaCreditoDto? filtro = null);
    
    /// <summary>
    /// Obtiene una línea de crédito específica
    /// </summary>
    Task<LineaCreditoDto?> GetByIdAsync(int rutCliente, int numeroLinea);
    
    /// <summary>
    /// Obtiene las líneas de crédito de un cliente
    /// </summary>
    Task<IEnumerable<LineaCreditoDto>> GetByClienteAsync(int rutCliente);
    
    /// <summary>
    /// Crea una nueva línea de crédito
    /// </summary>
    Task<int> CreateAsync(CreateLineaCreditoDto dto, string usuario);
    
    /// <summary>
    /// Actualiza una línea de crédito existente
    /// </summary>
    Task<bool> UpdateAsync(int rutCliente, int numeroLinea, UpdateLineaCreditoDto dto, string usuario);
    
    /// <summary>
    /// Obtiene el siguiente número de línea para un cliente
    /// </summary>
    Task<int> GetNextNumeroLineaAsync(int rutCliente);
    
    /// <summary>
    /// Cuenta las líneas en estado "Creada" de un cliente
    /// </summary>
    Task<int> CountLineasCreadasAsync(int rutCliente, int tipoMoneda);
    
    /// <summary>
    /// Procesa una acción sobre la línea (aprobar/rechazar/suspender/renovar)
    /// </summary>
    Task<bool> ProcesarAccionAsync(AccionLineaCreditoDto dto, string usuario);
    
    /// <summary>
    /// Obtiene el resumen de endeudamiento de un cliente
    /// </summary>
    Task<ResumenEndeudamientoDto?> GetResumenEndeudamientoAsync(int rutCliente);
    
    /// <summary>
    /// Obtiene líneas próximas a vencer
    /// </summary>
    Task<IEnumerable<ConsultaLineaCreditoDto>> GetLineasPorVencerAsync(int dias = 30);
    
    /// <summary>
    /// Obtiene líneas vencidas
    /// </summary>
    Task<IEnumerable<ConsultaLineaCreditoDto>> GetLineasVencidasAsync();
}

/// <summary>
/// Repositorio para Sublíneas por Producto
/// Migrado de F_LC1100.frm (Tab "Sublínea por Producto")
/// </summary>
public interface ISublineaProductoRepository
{
    /// <summary>
    /// Obtiene las sublíneas por producto de una línea
    /// </summary>
    Task<IEnumerable<SublineaProductoDto>> GetByLineaAsync(int rutCliente, int numeroLinea);
    
    /// <summary>
    /// Obtiene una sublínea por producto específica
    /// </summary>
    Task<SublineaProductoDto?> GetByIdAsync(int rutCliente, int numeroLinea, int tipoDocumento);
    
    /// <summary>
    /// Crea una sublínea por producto
    /// </summary>
    Task<bool> CreateAsync(CreateSublineaProductoDto dto);
    
    /// <summary>
    /// Actualiza una sublínea por producto
    /// </summary>
    Task<bool> UpdateAsync(int rutCliente, int numeroLinea, int tipoDocumento, CreateSublineaProductoDto dto);
    
    /// <summary>
    /// Elimina una sublínea por producto
    /// </summary>
    Task<bool> DeleteAsync(int rutCliente, int numeroLinea, int tipoDocumento);
    
    /// <summary>
    /// Obtiene el resumen de sublíneas por producto de una línea
    /// </summary>
    Task<ResumenSublineasDto> GetResumenAsync(int rutCliente, int numeroLinea);
}

/// <summary>
/// Repositorio para Sublíneas por Deudor
/// Migrado de F_LC1100.frm (Tab "Sublínea Deudores")
/// </summary>
public interface ISublineaDeudorRepository
{
    /// <summary>
    /// Obtiene las sublíneas por deudor de una línea
    /// </summary>
    Task<IEnumerable<SublineaDeudorDto>> GetByLineaAsync(int rutCliente, int numeroLinea);
    
    /// <summary>
    /// Obtiene una sublínea por deudor específica
    /// </summary>
    Task<SublineaDeudorDto?> GetByIdAsync(int rutCliente, int numeroLinea, int rutDeudor, int tipoDocumento);
    
    /// <summary>
    /// Crea una sublínea por deudor
    /// </summary>
    Task<bool> CreateAsync(CreateSublineaDeudorDto dto);
    
    /// <summary>
    /// Actualiza una sublínea por deudor
    /// </summary>
    Task<bool> UpdateAsync(CreateSublineaDeudorDto dto);
    
    /// <summary>
    /// Elimina una sublínea por deudor
    /// </summary>
    Task<bool> DeleteAsync(int rutCliente, int numeroLinea, int rutDeudor, int tipoDocumento);
    
    /// <summary>
    /// Obtiene las sublíneas de un deudor específico
    /// </summary>
    Task<IEnumerable<SublineaDeudorDto>> GetByDeudorAsync(int rutDeudor);
}

/// <summary>
/// Repositorio para Parámetros de Línea (Comisiones)
/// Migrado de F_LC1400.frm
/// </summary>
public interface ILineaCreditoParametroRepository
{
    /// <summary>
    /// Obtiene los parámetros de una línea
    /// </summary>
    Task<IEnumerable<LineaCreditoParametroDto>> GetByLineaAsync(int rutCliente, int numeroLinea);
    
    /// <summary>
    /// Guarda o actualiza parámetros de comisión
    /// </summary>
    Task<bool> SaveParametrosAsync(int rutCliente, int numeroLinea, decimal apertura, decimal renovacion, decimal exceso);
    
    /// <summary>
    /// Obtiene un parámetro específico
    /// </summary>
    Task<decimal?> GetParametroAsync(int rutCliente, int numeroLinea, int tipoParametro);
}

/// <summary>
/// Repositorio para Eventos de Línea
/// Migrado de F_LC1400.frm
/// </summary>
public interface ILineaCreditoEventoRepository
{
    /// <summary>
    /// Obtiene los eventos de una línea
    /// </summary>
    Task<IEnumerable<LineaCreditoEventoDto>> GetByLineaAsync(int rutCliente, int numeroLinea);
    
    /// <summary>
    /// Registra un evento de línea
    /// </summary>
    Task<bool> CreateAsync(CreateLineaCreditoEventoDto dto);
    
    /// <summary>
    /// Verifica si existe un evento de un tipo para una línea
    /// </summary>
    Task<bool> ExisteEventoAsync(int rutCliente, int numeroLinea, int tipoEvento);
}

/// <summary>
/// Repositorio para Notarías
/// </summary>
public interface INotariaRepository
{
    /// <summary>
    /// Obtiene todas las notarías
    /// </summary>
    Task<IEnumerable<NotariaDto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene una notaría por código
    /// </summary>
    Task<NotariaDto?> GetByIdAsync(int codigo);
}
