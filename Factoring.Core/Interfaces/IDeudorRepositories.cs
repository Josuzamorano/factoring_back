// =============================================
// Interfaces de Repositorio para Módulo 3 - Carpeta Deudor
// =============================================

using Factoring.Core.Entities;

namespace Factoring.Core.Interfaces;

public interface IDeudorRepository
{
    Task<IEnumerable<Deudor>> GetAllAsync();
    Task<IEnumerable<Deudor>> GetByClienteAsync(int rutCliente);
    Task<Deudor?> GetByIdAsync(int rutCliente, int rutDeudor);
    Task<Deudor?> GetDeudorGlobalAsync(int rutDeudor);
    Task<(IEnumerable<Deudor> Items, int TotalCount)> SearchAsync(
        int? rutCliente, string? rutDeudor, string? razonSocial, 
        int? estado, int? categoriaRiesgo, int pageNumber, int pageSize);
    Task<bool> ExistsAsync(int rutCliente, int rutDeudor);
    Task CreateAsync(Deudor deudor);
    Task UpdateAsync(Deudor deudor);
    Task DeleteAsync(int rutCliente, int rutDeudor);
    Task UpdateMontoOcupadoAsync(int rutCliente, int rutDeudor, decimal monto);
}

public interface IDeudorTelefonoRepository
{
    Task<IEnumerable<DeudorTelefono>> GetByDeudorAsync(int rutCliente, int rutDeudor);
    Task CreateAsync(DeudorTelefono telefono);
    Task DeleteAsync(int rutCliente, int rutDeudor, int tipo, string numero);
}

public interface IDeudorDocumentoRepository
{
    Task<IEnumerable<DeudorDocumento>> GetByDeudorAsync(int rutCliente, int rutDeudor);
    Task<DeudorDocumento?> GetByIdAsync(int rutCliente, int rutDeudor, int numeroDoc);
    Task CreateAsync(DeudorDocumento documento);
    Task DeleteAsync(int rutCliente, int rutDeudor, int numeroDoc);
}

public interface IGrupoDeudorRepository
{
    Task<IEnumerable<GrupoDeudor>> GetAllAsync();
    Task<IEnumerable<GrupoDeudor>> SearchAsync(string term);
    Task<GrupoDeudor?> GetByRutAsync(long rut);
    Task<(decimal Linea, decimal Ocupado, decimal Disponible)> GetCupoAsync(long rut);
    Task<bool> ExistsAsync(long rut);
    Task CreateAsync(GrupoDeudor grupo);
    Task UpdateAsync(GrupoDeudor grupo);
    Task DeleteAsync(long rut);
}

public interface IDeudorGrupoRepository
{
    Task<IEnumerable<DeudorGrupo>> GetByGrupoAsync(long rutGrupo);
    Task<DeudorGrupo?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(long rutGrupo, long rutDeudor);
    Task<int> CreateAsync(DeudorGrupo deudorGrupo);
    Task DeleteAsync(int id);
    Task DeleteByRutAsync(long rutGrupo, long rutDeudor);
}
