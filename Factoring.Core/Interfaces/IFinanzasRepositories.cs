using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Factoring.Core.Entities;
using Factoring.Core.DTOs;

namespace Factoring.Core.Interfaces;

/// <summary>
/// Repositorio para Tasas y Plazos (TYP)
/// Migrado de F_FI1100.frm - EXECUTE SEL_TYP, INS_TYP, UPD_TYP
/// </summary>
public interface ITasaPlazoRepository
{
    /// <summary>
    /// Obtiene todas las tasas para una fecha y moneda
    /// </summary>
    Task<IEnumerable<TasaPlazo>> GetByFechaYMonedaAsync(DateTime? fecha, int tipoMoneda);
    
    /// <summary>
    /// Obtiene una tasa específica por su clave compuesta
    /// </summary>
    Task<TasaPlazo?> GetByClaveAsync(DateTime? fecha, int tipoTasa, int tipoMoneda, int dias);
    
    /// <summary>
    /// Obtiene tasas con filtros
    /// </summary>
    Task<IEnumerable<TasaPlazo>> GetByFiltroAsync(FiltroTasaPlazoDto filtro);
    
    /// <summary>
    /// Obtiene la tasa aplicable para un monto y días específicos
    /// </summary>
    Task<TasaPlazo?> GetTasaAplicableAsync(DateTime? fecha, int tipoMoneda, int dias, decimal monto);
    
    /// <summary>
    /// Crea una nueva tasa
    /// </summary>
    Task<bool> CreateAsync(TasaPlazo tasa);
    
    /// <summary>
    /// Actualiza una tasa existente
    /// </summary>
    Task<bool> UpdateAsync(TasaPlazo tasa);
    
    /// <summary>
    /// Elimina una tasa por su clave compuesta
    /// </summary>
    Task<bool> DeleteAsync(DateTime? fecha, int tipoTasa, int tipoMoneda, int dias);
    
    /// <summary>
    /// Copia tasas de una fecha a otra
    /// </summary>
    Task<int> CopiarTasasAsync(DateTime? fechaOrigen, DateTime? fechaDestino, int tipoMoneda, decimal? ajuste = null);
    
    /// <summary>
    /// Obtiene el resumen de tasas por moneda y fecha
    /// </summary>
    Task<ResumenTasas?> GetResumenAsync(DateTime? fecha, int tipoMoneda);
    
    /// <summary>
    /// Obtiene el histórico de tasas para análisis
    /// </summary>
    Task<IEnumerable<HistoricoTasa>> GetHistoricoAsync(FiltroHistoricoTasaDto filtro);
    
    /// <summary>
    /// Obtiene las fechas disponibles con tasas registradas
    /// </summary>
    Task<IEnumerable<DateTime?>> GetFechasDisponiblesAsync(int tipoMoneda, int? ultimosDias = 30);
}

/// <summary>
/// Repositorio para Tasas Base (TASASBASES - conexión externa)
/// Migrado de F_FI1200.frm
/// </summary>
public interface ITasaBaseRepository
{
    /// <summary>
    /// Obtiene todas las tasas base para una fecha y moneda
    /// </summary>
    Task<IEnumerable<TasaBase>> GetByFechaYMonedaAsync(DateTime? fecha, int codigoMoneda);
    
    /// <summary>
    /// Obtiene una tasa base específica
    /// </summary>
    Task<TasaBase?> GetByClaveAsync(DateTime? fecha, int codigoMoneda, string tipoPeriodo);
    
    /// <summary>
    /// Obtiene tasas base con filtros
    /// </summary>
    Task<IEnumerable<TasaBase>> GetByFiltroAsync(FiltroTasaBaseDto filtro);
    
    /// <summary>
    /// Crea una nueva tasa base
    /// </summary>
    Task<bool> CreateAsync(TasaBase tasa);
    
    /// <summary>
    /// Actualiza una tasa base existente
    /// </summary>
    Task<bool> UpdateAsync(TasaBase tasa);
    
    /// <summary>
    /// Elimina una tasa base
    /// </summary>
    Task<bool> DeleteAsync(DateTime? fecha, int codigoMoneda, string tipoPeriodo);
}

/// <summary>
/// Repositorio para Spread de Clientes
/// </summary>
public interface ISpreadClienteRepository
{
    /// <summary>
    /// Obtiene el spread de un cliente para una moneda
    /// </summary>
    Task<SpreadCliente?> GetByClienteYMonedaAsync(int rutCliente, int tipoMoneda);
    
    /// <summary>
    /// Obtiene todos los spreads de un cliente
    /// </summary>
    Task<IEnumerable<SpreadCliente>> GetByClienteAsync(int rutCliente);
    
    /// <summary>
    /// Obtiene todos los spreads activos
    /// </summary>
    Task<IEnumerable<SpreadCliente>> GetActivosAsync();
    
    /// <summary>
    /// Crea un nuevo spread
    /// </summary>
    Task<bool> CreateAsync(SpreadCliente spread);
    
    /// <summary>
    /// Actualiza un spread existente
    /// </summary>
    Task<bool> UpdateAsync(SpreadCliente spread);
    
    /// <summary>
    /// Desactiva un spread
    /// </summary>
    Task<bool> DesactivarAsync(int rutCliente, int tipoMoneda);
}

/// <summary>
/// Repositorio para Indicadores Económicos (UF, Dólar, etc.)
/// </summary>
public interface IIndicadorEconomicoRepository
{
    /// <summary>
    /// Obtiene un indicador por código y fecha
    /// </summary>
    Task<IndicadorEconomico?> GetByCodigoYFechaAsync(string codigo, DateTime? fecha);
    
    /// <summary>
    /// Obtiene el último valor de un indicador
    /// </summary>
    Task<IndicadorEconomico?> GetUltimoAsync(string codigo);
    
    /// <summary>
    /// Obtiene todos los indicadores actuales
    /// </summary>
    Task<IEnumerable<IndicadorEconomico>> GetAllUltimosAsync();
    
    /// <summary>
    /// Obtiene el histórico de un indicador
    /// </summary>
    Task<IEnumerable<IndicadorEconomico>> GetHistoricoAsync(string codigo, DateTime? fechaDesde, DateTime? fechaHasta);
    
    /// <summary>
    /// Actualiza un indicador
    /// </summary>
    Task<bool> UpdateAsync(IndicadorEconomico indicador);
}

/// <summary>
/// Servicio para cálculo de tasas y simulaciones
/// </summary>
public interface ICalculoTasaService
{
    /// <summary>
    /// Calcula la tasa aplicable para una operación
    /// </summary>
    Task<SimulacionTasa> CalcularTasaAsync(SimulacionTasaRequestDto request);
    
    /// <summary>
    /// Calcula el interés para un monto y plazo
    /// </summary>
    decimal CalcularInteres(decimal monto, decimal tasa, int dias);
    
    /// <summary>
    /// Calcula el interés de mora
    /// </summary>
    decimal CalcularInteresMora(decimal monto, decimal tasaMora, int diasMora);
    
    /// <summary>
    /// Obtiene el tramo de monto aplicable
    /// </summary>
    Task<int> ObtenerTramoMontoAsync(decimal monto);
}
