using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Factoring.Core.Entities;

namespace Factoring.Core.Interfaces
{
    /// <summary>
    /// Repositorio para consultas de informes
    /// Basado en F_INF*.frm de VB6
    /// </summary>
    public interface IInformesRepository
    {
        #region Definiciones de Informes

        /// <summary>
        /// Obtiene lista de informes disponibles - F_INF1000.frm
        /// </summary>
        Task<IEnumerable<DefinicionInforme>> GetInformesDisponiblesAsync();

        /// <summary>
        /// Obtiene un informe por tipo
        /// </summary>
        Task<DefinicionInforme?> GetInformePorTipoAsync(TipoInformeEnum tipo);

        #endregion

        #region Anticipos Cursados (F_INF1100)

        /// <summary>
        /// Obtiene anticipos cursados por período
        /// </summary>
        Task<IEnumerable<InformeAnticipoCursado>> GetAnticiposCursadosAsync(
            DateTime? fechaDesde, 
            DateTime? fechaHasta, 
            int? tipoDocumento = null, 
            long? rutCliente = null);

        #endregion

        #region Protestos Diarios (F_INF1200)

        /// <summary>
        /// Obtiene protestos diarios por período y ejecutivo
        /// </summary>
        Task<IEnumerable<InformeProtestoDiario>> GetProtestosDiariosAsync(
            DateTime? fechaDesde, 
            DateTime? fechaHasta, 
            int? codigoEjecutivo = null);

        #endregion

        #region Documentos por Deudor (F_INF1300)

        /// <summary>
        /// Obtiene documentos de un deudor específico
        /// </summary>
        Task<IEnumerable<InformeDocumentoDeudor>> GetDocumentosPorDeudorAsync(
            long rutDeudor, 
            int? tipoDocumento = null);

        #endregion

        #region Cartera Cliente (F_INF1400)

        /// <summary>
        /// Obtiene cartera de un cliente
        /// </summary>
        Task<InformeCarteraCliente?> GetCarteraClienteAsync(
            long rutCliente, 
            int? tipoDocumento = null);

        /// <summary>
        /// Obtiene detalle de cartera de un cliente
        /// </summary>
        Task<IEnumerable<DetalleCarteraCliente>> GetDetalleCarteraAsync(
            long rutCliente, 
            int? tipoDocumento = null);

        #endregion

        #region Devengamiento (F_INF1500)

        /// <summary>
        /// Obtiene informe de devengamiento a una fecha de corte
        /// </summary>
        Task<IEnumerable<InformeDevengamiento>> GetDevengamientoAsync(
            DateTime? fechaCorte, 
            long? rutCliente = null, 
            int? centroCosto = null);

        #endregion

        #region Adelantos (F_INF1600)

        /// <summary>
        /// Obtiene informe de adelantos
        /// </summary>
        Task<IEnumerable<InformeAdelanto>> GetAdelantosAsync(
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null, 
            long? rutCliente = null);

        #endregion

        #region Cupo Cliente (F_INF1700)

        /// <summary>
        /// Obtiene cupos de clientes por ejecutivo
        /// </summary>
        Task<IEnumerable<InformeCupoCliente>> GetCupoClientesAsync(
            int? codigoEjecutivo = null, 
            long? rutCliente = null);

        #endregion

        #region Volumen Operaciones (F_INF1800)

        /// <summary>
        /// Obtiene volumen de operaciones por período
        /// </summary>
        Task<IEnumerable<InformeVolumenOperaciones>> GetVolumenOperacionesAsync(
            int anio, 
            int? mes = null, 
            int? codigoEjecutivo = null);

        #endregion

        #region Excedentes (F_INF1900)

        /// <summary>
        /// Obtiene informe de excedentes
        /// </summary>
        Task<IEnumerable<InformeExcedente>> GetExcedentesAsync(
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null, 
            long? rutCliente = null, 
            string? estado = null);

        #endregion

        #region Concentración Terceros (F_INF2000)

        /// <summary>
        /// Obtiene informe de concentración por terceros/deudores
        /// </summary>
        Task<IEnumerable<InformeConcentracionTerceros>> GetConcentracionTercerosAsync(
            DateTime? fechaCorte = null, 
            long? rutDeudor = null, 
            decimal? porcentajeMinimo = null);

        #endregion

        #region Resumen y Estadísticas

        /// <summary>
        /// Obtiene resumen general para dashboard
        /// </summary>
        Task<ResumenInformes> GetResumenGeneralAsync(DateTime? fechaCorte = null);

        #endregion

        #region Historial

        /// <summary>
        /// Registra generación de un informe en historial
        /// </summary>
        Task<int> RegistrarHistorialAsync(HistorialInforme historial);

        /// <summary>
        /// Obtiene historial de informes generados
        /// </summary>
        Task<IEnumerable<HistorialInforme>> GetHistorialAsync(
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null, 
            string? usuario = null, 
            int? tipoInforme = null);

        #endregion
    }

    /// <summary>
    /// Servicio de exportación de informes
    /// </summary>
    public interface IExportacionInformesService
    {
        /// <summary>
        /// Exporta datos a Excel
        /// </summary>
        Task<byte[]> ExportarExcelAsync<T>(IEnumerable<T> datos, string nombreHoja);

        /// <summary>
        /// Exporta datos a PDF
        /// </summary>
        Task<byte[]> ExportarPdfAsync<T>(IEnumerable<T> datos, string titulo);

        /// <summary>
        /// Exporta datos a CSV
        /// </summary>
        Task<byte[]> ExportarCsvAsync<T>(IEnumerable<T> datos);
    }
}
