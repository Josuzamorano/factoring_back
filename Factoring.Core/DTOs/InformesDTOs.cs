using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Factoring.Core.DTOs
{
    #region DTOs de Parámetros

    /// <summary>
    /// DTO para solicitar generación de un informe
    /// </summary>
    public class GenerarInformeRequestDto
    {
        [Required(ErrorMessage = "El tipo de informe es requerido")]
        public int TipoInforme { get; set; }

        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public long? RutCliente { get; set; }
        public long? RutDeudor { get; set; }
        public int? CodigoEjecutivo { get; set; }
        public int? TipoDocumento { get; set; }
        public int? TipoMoneda { get; set; }
        
        /// <summary>
        /// 1 = Resumido, 2 = Detallado
        /// </summary>
        public int TipoDetalle { get; set; } = 1;
        
        /// <summary>
        /// 1 = Pantalla, 2 = Excel, 3 = PDF
        /// </summary>
        public int Formato { get; set; } = 1;
        
        public string? OrdenarPor { get; set; }
        public bool OrdenDescendente { get; set; }
        public int? CentroCosto { get; set; }
        public bool TodosLosClientes { get; set; }
    }

    /// <summary>
    /// DTO para filtros de anticipos cursados (F_INF1100)
    /// </summary>
    public class FiltroAnticiposCursadosDto
    {
        [Required(ErrorMessage = "La fecha desde es requerida")]
        public DateTime? FechaDesde { get; set; }
        
        [Required(ErrorMessage = "La fecha hasta es requerida")]
        public DateTime? FechaHasta { get; set; }
        
        public int? TipoDocumento { get; set; }
        public long? RutCliente { get; set; }
    }

    /// <summary>
    /// DTO para filtros de protestos diarios (F_INF1200)
    /// </summary>
    public class FiltroProtestosDiariosDto
    {
        [Required(ErrorMessage = "La fecha desde es requerida")]
        public DateTime? FechaDesde { get; set; }
        
        [Required(ErrorMessage = "La fecha hasta es requerida")]
        public DateTime? FechaHasta { get; set; }
        
        public int? CodigoEjecutivo { get; set; }
    }

    /// <summary>
    /// DTO para filtros de documentos por deudor (F_INF1300)
    /// </summary>
    public class FiltroDocumentosDeudorDto
    {
        [Required(ErrorMessage = "El RUT del deudor es requerido")]
        public long RutDeudor { get; set; }
        
        public int? TipoDocumento { get; set; }
        
        /// <summary>
        /// 1 = Resumido, 2 = Detallado
        /// </summary>
        public int TipoDetalle { get; set; } = 1;
    }

    /// <summary>
    /// DTO para filtros de cartera cliente (F_INF1400)
    /// </summary>
    public class FiltroCarteraClienteDto
    {
        [Required(ErrorMessage = "El RUT del cliente es requerido")]
        public long RutCliente { get; set; }
        
        public int? TipoDocumento { get; set; }
    }

    /// <summary>
    /// DTO para filtros de devengamiento (F_INF1500)
    /// </summary>
    public class FiltroDevengamientoDto
    {
        [Required(ErrorMessage = "La fecha de corte es requerida")]
        public DateTime? FechaCorte { get; set; }
        
        public long? RutCliente { get; set; }
        public bool TodosLosClientes { get; set; }
        public int? CentroCosto { get; set; }
    }

    /// <summary>
    /// DTO para filtros de adelantos (F_INF1600)
    /// </summary>
    public class FiltroAdelantosDto
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public long? RutCliente { get; set; }
        public bool TodosLosClientes { get; set; }
    }

    /// <summary>
    /// DTO para filtros de cupo cliente (F_INF1700)
    /// </summary>
    public class FiltroCupoClienteDto
    {
        public int? CodigoEjecutivo { get; set; }
        public long? RutCliente { get; set; }
    }

    /// <summary>
    /// DTO para filtros de volumen operaciones (F_INF1800)
    /// </summary>
    public class FiltroVolumenOperacionesDto
    {
        [Required(ErrorMessage = "El año es requerido")]
        public int Anio { get; set; }
        
        public int? Mes { get; set; }
        public int? CodigoEjecutivo { get; set; }
    }

    /// <summary>
    /// DTO para filtros de excedentes (F_INF1900)
    /// </summary>
    public class FiltroExcedentesDto
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public long? RutCliente { get; set; }
        public string? Estado { get; set; }
    }

    /// <summary>
    /// DTO para filtros de concentración terceros (F_INF2000)
    /// </summary>
    public class FiltroConcentracionTercerosDto
    {
        public DateTime? FechaCorte { get; set; }
        public long? RutDeudor { get; set; }
        public decimal? PorcentajeMinimoConcentracion { get; set; }
        public bool IncluirCustodia { get; set; }
    }

    #endregion

    #region DTOs de Respuesta

    /// <summary>
    /// DTO de definición de informe para listado
    /// </summary>
    public class DefinicionInformeDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int TipoInforme { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public bool RequiereFechas { get; set; }
        public bool RequiereCliente { get; set; }
        public bool RequiereDeudor { get; set; }
        public bool RequiereEjecutivo { get; set; }
        public bool RequiereTipoDocumento { get; set; }
        public bool PermiteExcel { get; set; }
        public bool PermitePDF { get; set; }
        public int Orden { get; set; }
    }

    /// <summary>
    /// DTO de resultado de informe genérico
    /// </summary>
    public class ResultadoInformeDto<T>
    {
        public int TipoInforme { get; set; }
        public string NombreInforme { get; set; } = string.Empty;
        public DateTime? FechaGeneracion { get; set; }
        public List<T> Datos { get; set; } = new();
        public int TotalRegistros { get; set; }
        public Dictionary<string, decimal> Totales { get; set; } = new();
        public string? MensajeError { get; set; }
        public bool Exitoso { get; set; }
    }

    /// <summary>
    /// DTO de anticipos cursados
    /// </summary>
    public class AnticipoCursadoDto
    {
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RutFormateado => $"{RutCliente:N0}-{DvCliente}";
        public string RazonSocialCliente { get; set; } = string.Empty;
        public int NumeroOperacion { get; set; }
        public DateTime? FechaOperacion { get; set; }
        public DateTime? FechaCurse { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public int CantidadDocumentos { get; set; }
        public decimal MontoDocumentos { get; set; }
        public decimal MontoAnticipo { get; set; }
        public decimal TasaOperacion { get; set; }
        public decimal Diferencia { get; set; }
        public decimal Comision { get; set; }
        public decimal Gastos { get; set; }
        public string NombreEjecutivo { get; set; } = string.Empty;
        public string NombreBanco { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO de protestos diarios
    /// </summary>
    public class ProtestoDiarioDto
    {
        public DateTime? FechaProtesto { get; set; }
        public int CodigoEjecutivo { get; set; }
        public string NombreEjecutivo { get; set; } = string.Empty;
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RutClienteFormateado => $"{RutCliente:N0}-{DvCliente}";
        public string RazonSocialCliente { get; set; } = string.Empty;
        public long RutDeudor { get; set; }
        public string DvDeudor { get; set; } = string.Empty;
        public string RutDeudorFormateado => $"{RutDeudor:N0}-{DvDeudor}";
        public string NombreDeudor { get; set; } = string.Empty;
        public int NumeroOperacion { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public decimal MontoDocumento { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public int DiasVencido { get; set; }
        public string CausalProtesto { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO de documentos por deudor
    /// </summary>
    public class DocumentoDeudorDto
    {
        public long RutDeudor { get; set; }
        public string DvDeudor { get; set; } = string.Empty;
        public string RutDeudorFormateado => $"{RutDeudor:N0}-{DvDeudor}";
        public string NombreDeudor { get; set; } = string.Empty;
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RazonSocialCliente { get; set; } = string.Empty;
        public int NumeroOperacion { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public DateTime? FechaOperacion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal MontoDocumento { get; set; }
        public decimal SaldoDocumento { get; set; }
        public string EstadoDocumento { get; set; } = string.Empty;
        public int DiasVencido { get; set; }
    }

    /// <summary>
    /// DTO de cartera cliente
    /// </summary>
    public class CarteraClienteDto
    {
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RutFormateado => $"{RutCliente:N0}-{DvCliente}";
        public string RazonSocialCliente { get; set; } = string.Empty;
        public string NombreEjecutivo { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public int TotalDocumentos { get; set; }
        public decimal MontoVigente { get; set; }
        public decimal MontoMora { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal PorcentajeConcentracion { get; set; }
        public List<DetalleCarteraDto> Detalle { get; set; } = new();
    }

    public class DetalleCarteraDto
    {
        public int NumeroOperacion { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public long RutDeudor { get; set; }
        public string NombreDeudor { get; set; } = string.Empty;
        public DateTime? FechaOperacion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal MontoOriginal { get; set; }
        public decimal Saldo { get; set; }
        public int DiasVencido { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO de devengamiento
    /// </summary>
    public class DevengamientoDto
    {
        public DateTime? FechaCorte { get; set; }
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RutFormateado => $"{RutCliente:N0}-{DvCliente}";
        public string RazonSocialCliente { get; set; } = string.Empty;
        public int NumeroOperacion { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public DateTime? FechaOperacion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public int PlazoTotal { get; set; }
        public int DiasDevengados { get; set; }
        public int DiasPorDevengar { get; set; }
        public decimal MontoDocumento { get; set; }
        public decimal TasaOperacion { get; set; }
        public decimal InteresTotal { get; set; }
        public decimal InteresDevengado { get; set; }
        public decimal InteresPorDevengar { get; set; }
        public string DescripcionCentroCosto { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO de adelantos
    /// </summary>
    public class AdelantoDto
    {
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RutFormateado => $"{RutCliente:N0}-{DvCliente}";
        public string RazonSocialCliente { get; set; } = string.Empty;
        public int NumeroOperacion { get; set; }
        public DateTime? FechaOperacion { get; set; }
        public DateTime? FechaAdelanto { get; set; }
        public decimal MontoAdelanto { get; set; }
        public decimal MontoDocumentos { get; set; }
        public decimal PorcentajeAdelanto { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime? FechaRecuperacion { get; set; }
        public decimal? MontoRecuperado { get; set; }
    }

    /// <summary>
    /// DTO de cupo cliente
    /// </summary>
    public class CupoClienteDto
    {
        public int CodigoEjecutivo { get; set; }
        public string NombreEjecutivo { get; set; } = string.Empty;
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RutFormateado => $"{RutCliente:N0}-{DvCliente}";
        public string RazonSocialCliente { get; set; } = string.Empty;
        public decimal LineaAprobada { get; set; }
        public decimal MontoUtilizado { get; set; }
        public decimal MontoDisponible { get; set; }
        public decimal PorcentajeUtilizacion { get; set; }
        public DateTime? FechaVencimientoLinea { get; set; }
        public string EstadoLinea { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO de volumen operaciones
    /// </summary>
    public class VolumenOperacionesDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public string Periodo => $"{Anio}-{Mes:D2}";
        public int CodigoEjecutivo { get; set; }
        public string NombreEjecutivo { get; set; } = string.Empty;
        public int CantidadOperaciones { get; set; }
        public int CantidadDocumentos { get; set; }
        public decimal MontoOperado { get; set; }
        public decimal MontoAnticipado { get; set; }
        public decimal ComisionesGeneradas { get; set; }
        public decimal InteresesGenerados { get; set; }
        public int ClientesNuevos { get; set; }
        public int ClientesActivos { get; set; }
    }

    /// <summary>
    /// DTO de excedentes
    /// </summary>
    public class ExcedenteInformeDto
    {
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RutFormateado => $"{RutCliente:N0}-{DvCliente}";
        public string RazonSocialCliente { get; set; } = string.Empty;
        public int NumeroOperacion { get; set; }
        public DateTime? FechaOperacion { get; set; }
        public decimal MontoExcedente { get; set; }
        public DateTime? FechaGeneracion { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public decimal? MontoDevuelto { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string FormaPago { get; set; } = string.Empty;
        public string NombreEjecutivo { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO de concentración terceros
    /// </summary>
    public class ConcentracionTercerosDto
    {
        public long RutDeudor { get; set; }
        public string DvDeudor { get; set; } = string.Empty;
        public string RutFormateado => $"{RutDeudor:N0}-{DvDeudor}";
        public string NombreDeudor { get; set; } = string.Empty;
        public int CantidadClientes { get; set; }
        public int CantidadDocumentos { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoVigente { get; set; }
        public decimal MontoVencido { get; set; }
        public decimal PorcentajeConcentracion { get; set; }
        public string? CustodiaAsignada { get; set; }
        public List<DetalleConcentracionDto> DetallePorCliente { get; set; } = new();
    }

    public class DetalleConcentracionDto
    {
        public long RutCliente { get; set; }
        public string RazonSocialCliente { get; set; } = string.Empty;
        public int CantidadDocumentos { get; set; }
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }
    }

    /// <summary>
    /// DTO de resumen para dashboard
    /// </summary>
    public class ResumenInformesDto
    {
        public DateTime? FechaCorte { get; set; }
        public int TotalOperacionesMes { get; set; }
        public decimal MontoOperadoMes { get; set; }
        public int TotalClientesActivos { get; set; }
        public decimal CarteraVigente { get; set; }
        public decimal CarteraVencida { get; set; }
        public decimal PorcentajeMorosidad { get; set; }
        public int DocumentosProtestados { get; set; }
        public decimal MontoProtestado { get; set; }
        public decimal ExcedentesPendientes { get; set; }
    }

    /// <summary>
    /// DTO de historial de informes
    /// </summary>
    public class HistorialInformeDto
    {
        public int Id { get; set; }
        public int TipoInforme { get; set; }
        public string NombreInforme { get; set; } = string.Empty;
        public DateTime? FechaGeneracion { get; set; }
        public string UsuarioGeneracion { get; set; } = string.Empty;
        public int Formato { get; set; }
        public string FormatoDescripcion { get; set; } = string.Empty;
        public int TotalRegistros { get; set; }
        public int TiempoEjecucionMs { get; set; }
        public bool Exitoso { get; set; }
    }

    #endregion
}
