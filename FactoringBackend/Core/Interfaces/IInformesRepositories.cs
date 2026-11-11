using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FactoringBackend.Core.Entities;
using FactoringBackend.Core.DTOs;

namespace FactoringBackend.Core.Interfaces
{
    /// <summary>
    /// Repositorio principal para generación de informes
    /// Basado en F_INF1000.frm y formularios F_INF*.frm
    /// </summary>
    public interface IInformesRepository
    {
        #region Definiciones de Informes
        
        /// <summary>
        /// Obtiene la lista de informes disponibles
        /// </summary>
        Task<IEnumerable<DefinicionInforme>> GetInformesDisponiblesAsync();
        
        /// <summary>
        /// Obtiene un informe por su tipo
        /// </summary>
        Task<DefinicionInforme?> GetInformePorTipoAsync(TipoInformeEnum tipo);
        
        #endregion

        #region Anticipos Cursados (F_INF1100)
        
        /// <summary>
        /// Genera informe de anticipos cursados
        /// </summary>
        Task<IEnumerable<InformeAnticipoCursado>> GetAnticiposCursadosAsync(
            DateTime? fechaDesde, 
            DateTime? fechaHasta, 
            int? tipoDocumento = null,
            long? rutCliente = null);
        
        #endregion

        #region Protestos Diarios (F_INF1200)
        
        /// <summary>
        /// Genera informe de protestos diarios por ejecutivo
        /// </summary>
        Task<IEnumerable<InformeProtestoDiario>> GetProtestosDiariosAsync(
            DateTime? fechaDesde, 
            DateTime? fechaHasta, 
            int? codigoEjecutivo = null);
        
        #endregion

        #region Documentos por Deudor (F_INF1300)
        
        /// <summary>
        /// Genera informe de documentos comprados por deudor
        /// </summary>
        Task<IEnumerable<InformeDocumentoDeudor>> GetDocumentosPorDeudorAsync(
            long rutDeudor, 
            int? tipoDocumento = null);
        
        #endregion

        #region Cartera Cliente (F_INF1400)
        
        /// <summary>
        /// Genera informe de cartera por cliente
        /// </summary>
        Task<InformeCarteraCliente?> GetCarteraClienteAsync(
            long rutCliente, 
            int? tipoDocumento = null);
        
        /// <summary>
        /// Genera informe de cartera detallada
        /// </summary>
        Task<IEnumerable<DetalleCarteraCliente>> GetDetalleCarteraAsync(
            long rutCliente, 
            int? tipoDocumento = null);
        
        #endregion

        #region Devengamiento (F_INF1500)
        
        /// <summary>
        /// Genera informe de detalle devengamiento
        /// </summary>
        Task<IEnumerable<InformeDevengamiento>> GetDevengamientoAsync(
            DateTime? fechaCorte, 
            long? rutCliente = null,
            int? centroCosto = null);
        
        #endregion

        #region Adelantos (F_INF1600)
        
        /// <summary>
        /// Genera informe de adelantos
        /// </summary>
        Task<IEnumerable<InformeAdelanto>> GetAdelantosAsync(
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null,
            long? rutCliente = null);
        
        #endregion

        #region Cupo Cliente (F_INF1700)
        
        /// <summary>
        /// Genera informe de cupo por cliente
        /// </summary>
        Task<IEnumerable<InformeCupoCliente>> GetCupoClientesAsync(
            int? codigoEjecutivo = null,
            long? rutCliente = null);
        
        #endregion

        #region Volumen Operaciones (F_INF1800)
        
        /// <summary>
        /// Genera informe de volumen de operaciones
        /// </summary>
        Task<IEnumerable<InformeVolumenOperaciones>> GetVolumenOperacionesAsync(
            int anio, 
            int? mes = null,
            int? codigoEjecutivo = null);
        
        #endregion

        #region Excedentes (F_INF1900)
        
        /// <summary>
        /// Genera informe de excedentes
        /// </summary>
        Task<IEnumerable<InformeExcedente>> GetExcedentesAsync(
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null,
            long? rutCliente = null,
            string? estado = null);
        
        #endregion

        #region Concentración Terceros (F_INF2000)
        
        /// <summary>
        /// Genera informe de concentración de terceros
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
        /// Registra la generación de un informe
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
    /// Servicio para exportación de informes
    /// </summary>
    public interface IExportacionInformesService
    {
        /// <summary>
        /// Exporta datos a formato Excel
        /// </summary>
        Task<byte[]> ExportarExcelAsync<T>(IEnumerable<T> datos, string nombreHoja);
        
        /// <summary>
        /// Exporta datos a formato PDF
        /// </summary>
        Task<byte[]> ExportarPdfAsync<T>(IEnumerable<T> datos, string titulo, Dictionary<string, string>? parametros = null);
        
        /// <summary>
        /// Exporta datos a formato CSV
        /// </summary>
        Task<byte[]> ExportarCsvAsync<T>(IEnumerable<T> datos);
    }
}
