using Factoring.Core.DTOs;
using Factoring.Core.Entities;

namespace Factoring.Core.Interfaces;

/// <summary>
/// Repositorio de Clientes - Migrado de F_CC1700.frm
/// </summary>
public interface IClienteRepository
{
    // CRUD Básico
    Task<IEnumerable<Cliente>> GetAllAsync();
    Task<Cliente?> GetByRutAsync(long rutCliente);
    Task<bool> CreateAsync(Cliente cliente);
    Task<bool> UpdateAsync(Cliente cliente);
    Task<bool> DeleteAsync(long rutCliente);
    Task<bool> ExistsAsync(long rutCliente);
    
    // Búsquedas
    Task<PagedResult<Cliente>> SearchAsync(ClienteSearchDto search);
    Task<IEnumerable<Cliente>> GetByEjecutivoAsync(string codigoEjecutivo);
    Task<IEnumerable<Cliente>> GetActivosAsync();
    
    // Línea bancaria
    Task<bool> UpdateLineaBancoAsync(long rutCliente, decimal linea, decimal utilizado, string? sucursal, string? ejecutivo);
}

/// <summary>
/// Repositorio de Teléfonos del Cliente
/// </summary>
public interface IClienteTelefonoRepository
{
    Task<IEnumerable<ClienteTelefono>> GetByClienteAsync(long rutCliente);
    Task<bool> CreateAsync(long rutCliente, ClienteTelefono telefono);
    Task<bool> DeleteAsync(long rutCliente, int tipoTelefono, string numeroTelefono);
    Task<bool> DeleteAllByClienteAsync(long rutCliente);
}

/// <summary>
/// Repositorio de Contactos del Cliente
/// </summary>
public interface IClienteContactoRepository
{
    Task<IEnumerable<ClienteContacto>> GetByClienteAsync(long rutCliente);
    Task<ClienteContacto?> GetByIdAsync(long rutCliente, int numeroContacto);
    Task<bool> CreateAsync(long rutCliente, ClienteContacto contacto);
    Task<bool> UpdateAsync(ClienteContacto contacto);
    Task<bool> DeleteAsync(long rutCliente, int numeroContacto);
    Task<int> GetNextNumeroContactoAsync(long rutCliente);
}

/// <summary>
/// Repositorio de Cuentas Bancarias del Cliente
/// </summary>
public interface IClienteBancoRepository
{
    Task<IEnumerable<ClienteBanco>> GetByClienteAsync(long rutCliente);
    Task<bool> CreateAsync(long rutCliente, ClienteBanco cuenta);
    Task<bool> DeleteAsync(long rutCliente, int codigoBanco, string numeroCuenta);
    Task<bool> DeleteAllByClienteAsync(long rutCliente);
}

/// <summary>
/// Repositorio de Cobradores
/// </summary>
public interface ICobradorRepository
{
    Task<IEnumerable<Cobrador>> GetAllAsync();
    Task<IEnumerable<Cobrador>> GetActivosAsync();
    Task<Cobrador?> GetByCodigoAsync(string codigo);
}
