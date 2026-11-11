using Factoring.Core.Entities;

namespace Factoring.Core.Interfaces;

/// <summary>
/// Interfaz genérica para repositorio de maestros
/// </summary>
public interface IMasterRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetActiveAsync();
    Task<T?> GetByIdAsync(int id);
    Task<bool> CreateAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

/// <summary>
/// Repositorio de Bancos
/// </summary>
public interface IBancoRepository : IMasterRepository<Banco>
{
}

/// <summary>
/// Repositorio de Plazas
/// </summary>
public interface IPlazaRepository : IMasterRepository<Plaza>
{
}

/// <summary>
/// Repositorio de Sucursales
/// </summary>
public interface ISucursalRepository : IMasterRepository<Sucursal>
{
}

/// <summary>
/// Repositorio de Ejecutivos
/// </summary>
public interface IEjecutivoRepository : IMasterRepository<Ejecutivo>
{
    Task<IEnumerable<Ejecutivo>> GetBySucursalAsync(int codigoSucursal);
}

/// <summary>
/// Repositorio de Comunas
/// </summary>
public interface IComunaRepository : IMasterRepository<Comuna>
{
    Task<IEnumerable<Comuna>> GetByRegionAsync(int codigoRegion);
}

/// <summary>
/// Repositorio de Regiones
/// </summary>
public interface IRegionRepository : IMasterRepository<Region>
{
}

/// <summary>
/// Repositorio de Gastos
/// </summary>
public interface IGastoRepository : IMasterRepository<Gasto>
{
    Task<IEnumerable<Gasto>> GetByTipoAsync(int tipoGasto);
}

/// <summary>
/// Repositorio de Parámetros
/// </summary>
public interface IParametroRepository
{
    Task<IEnumerable<Parametro>> GetAllAsync();
    Task<IEnumerable<Parametro>> GetByTablaAsync(int codigoTabla);
    Task<Parametro?> GetByIdAsync(int codigoTabla, int codigoParametro);
    Task<bool> CreateAsync(Parametro parametro);
    Task<bool> UpdateAsync(Parametro parametro);
    Task<bool> DeleteAsync(int codigoTabla, int codigoParametro);
}
