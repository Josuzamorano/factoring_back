using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Factoring.Core.Interfaces
{
    /// <summary>
    /// Repositorio para gestión de Pagarés
    /// Migrado de F_LE1100.frm (Consulta) y F_LE1400.frm (Ingreso)
    /// </summary>
    public interface IPagareRepository
    {
        Task<IEnumerable<PagareDto>> GetAllAsync(FiltroPagaresDto filtro);
        Task<PagareDto?> GetByIdAsync(int id);
        Task<PagareDto?> GetByNumeroAsync(int numeroPagare);
        Task<IEnumerable<PagareDto>> GetByClienteAsync(int rutCliente);
        Task<IEnumerable<PagareDto>> GetByOperacionAsync(int numeroOperacion);
        Task<IEnumerable<PagareDto>> GetByLineaAsync(int numeroLinea);
        Task<IEnumerable<PagareDto>> GetVigentesAsync();
        Task<IEnumerable<PagareDto>> GetVencidosAsync();
        Task<IEnumerable<PagareDto>> GetPorVencerAsync(int diasAnticipacion = 30);
        Task<int> CreateAsync(CreatePagareDto dto, int usuarioId);
        Task<bool> UpdateAsync(int id, UpdatePagareDto dto, int usuarioId);
        Task<bool> CambiarEstadoAsync(int id, int nuevoEstado, string motivo, int usuarioId);
        Task<bool> DeleteAsync(int id, int usuarioId);
        Task<int> GetSiguienteNumeroAsync();
        Task<decimal> GetMontoTotalVigenteByClienteAsync(int rutCliente);
    }

    /// <summary>
    /// Repositorio para gestión de Avales
    /// Migrado de F_LE1200.frm (Consulta) y F_LE1500.frm (Ingreso)
    /// </summary>
    public interface IAvalRepository
    {
        Task<IEnumerable<AvalDto>> GetAllAsync(FiltroAvalesDto filtro);
        Task<AvalDto?> GetByIdAsync(int id);
        Task<IEnumerable<AvalDto>> GetByClienteAsync(int rutCliente);
        Task<IEnumerable<AvalDto>> GetByRutAvalAsync(int rutAval);
        Task<IEnumerable<AvalDto>> GetActivosAsync();
        Task<IEnumerable<AvalDto>> GetPorVencerAsync(int diasAnticipacion = 30);
        Task<int> CreateAsync(CreateAvalDto dto, int usuarioId);
        Task<bool> UpdateAsync(int id, UpdateAvalDto dto, int usuarioId);
        Task<bool> CambiarEstadoAsync(int id, int nuevoEstado, int usuarioId);
        Task<bool> DeleteAsync(int id, int usuarioId);
        Task<bool> ExisteAvalParaClienteAsync(int rutCliente, int rutAval);
        Task<int> GetCountActivosByClienteAsync(int rutCliente);
    }

    /// <summary>
    /// Repositorio para gestión de Firmas autorizadas
    /// Migrado de F_LE1300.frm (Consulta) y F_LE1600.frm (Ingreso)
    /// </summary>
    public interface IFirmaRepository
    {
        Task<IEnumerable<FirmaDto>> GetAllAsync(FiltroFirmasDto filtro);
        Task<FirmaDto?> GetByIdAsync(int id);
        Task<IEnumerable<FirmaDto>> GetByClienteAsync(int rutCliente);
        Task<IEnumerable<FirmaDto>> GetByRutFirmanteAsync(int rutFirmante);
        Task<IEnumerable<FirmaDto>> GetVigentesAsync();
        Task<IEnumerable<FirmaDto>> GetPorVencerAsync(int diasAnticipacion = 30);
        Task<int> CreateAsync(CreateFirmaDto dto, int usuarioId);
        Task<bool> UpdateAsync(int id, UpdateFirmaDto dto, int usuarioId);
        Task<bool> ActivarDesactivarAsync(int id, bool activa, int usuarioId);
        Task<bool> DeleteAsync(int id, int usuarioId);
        Task<bool> ExisteFirmaParaClienteAsync(int rutCliente, int rutFirmante);
        Task<int> GetCountVigentesByClienteAsync(int rutCliente);
    }

    /// <summary>
    /// Repositorio para gestión de Contratos Marco
    /// Migrado de F_LE1700.frm
    /// </summary>
    public interface IContratoMarcoRepository
    {
        Task<IEnumerable<ContratoMarcoDto>> GetAllAsync(FiltroContratosMarcoDto filtro);
        Task<ContratoMarcoDto?> GetByIdAsync(int id);
        Task<ContratoMarcoDto?> GetByNumeroAsync(int numeroContrato);
        Task<IEnumerable<ContratoMarcoDto>> GetByClienteAsync(int rutCliente);
        Task<int> CreateAsync(CreateContratoMarcoDto dto, int usuarioId);
        Task<bool> UpdateAsync(int id, UpdateContratoMarcoDto dto, int usuarioId);
        Task<bool> AprobarAsync(int id, decimal montoAprobado, int usuarioId);
        Task<bool> RechazarAsync(int id, string motivo, int usuarioId);
        Task<bool> ActualizarDisponibleAsync(int id, decimal nuevoDisponible);
        Task<bool> DeleteAsync(int id, int usuarioId);
        Task<int> GetSiguienteNumeroAsync();
        Task<decimal> GetMontoDisponibleByClienteAsync(int rutCliente);
    }

    /// <summary>
    /// Repositorio para operaciones generales del módulo Legal
    /// </summary>
    public interface ILegalRepository
    {
        Task<ResumenLegalDto> GetResumenGeneralAsync();
        Task<ResumenGarantiasClienteDto> GetResumenByClienteAsync(int rutCliente);
        Task<IEnumerable<ResumenGarantiasClienteDto>> GetResumenTodosClientesAsync();
        Task<bool> RegistrarHistorialAsync(string tipoEntidad, int idEntidad, string accion, string descripcion, int usuarioId);
        Task<IEnumerable<HistorialLegal>> GetHistorialByEntidadAsync(string tipoEntidad, int idEntidad);
    }

    /// <summary>
    /// Servicio para emisión de pagarés
    /// Migrado de F_LE1800.frm
    /// </summary>
    public interface IEmisionPagareService
    {
        Task<EmisionPagareDto> PrepararEmisionAsync(int idPagare);
        Task<bool> EmitirAsync(int idPagare, int usuarioId);
        Task<bool> MarcarImpresoAsync(int idPagare, int usuarioId);
        Task<byte[]> GenerarPdfPagareAsync(int idPagare);
    }
}
