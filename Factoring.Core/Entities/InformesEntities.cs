using System;
using System.Collections.Generic;

namespace Factoring.Core.Entities
{
    #region Enums

    /// <summary>
    /// Tipos de informes disponibles
    /// Basado en F_INF1000.frm - Listado maestro de informes
    /// </summary>
    public enum TipoInformeEnum
    {
        AnticiposCursados = 1,      // F_INF1100.frm
        ProtestosDiarios = 2,       // F_INF1200.frm
        DocumentosPorDeudor = 3,    // F_INF1300.frm
        CarteraCliente = 4,         // F_INF1400.frm
        DetalleDevengamiento = 5,   // F_INF1500.frm
        Adelantos = 6,              // F_INF1600.frm
        CupoCliente = 7,            // F_INF1700.frm
        VolumenOperaciones = 8,     // F_INF1800.frm
        Excedentes = 9,             // F_INF1900.frm
        ConcentracionTerceros = 10, // F_INF2000.frm
        InformeGestion = 11,
        ProtestosCliente = 12
    }

    public enum FormatoInformeEnum
    {
        Pantalla = 1,
        Excel = 2,
        PDF = 3,
        CSV = 4
    }

    public enum TipoDetalleInformeEnum
    {
        Resumido = 1,
        Detallado = 2
    }

    #endregion

    #region Definiciones de Informes

    /// <summary>
    /// Definición de un informe disponible
    /// </summary>
    public class DefinicionInforme
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public TipoInformeEnum Tipo { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public bool RequiereFechas { get; set; }
        public bool RequiereCliente { get; set; }
        public bool RequiereDeudor { get; set; }
        public bool RequiereEjecutivo { get; set; }
        public bool RequiereTipoDocumento { get; set; }
        public bool PermiteExcel { get; set; }
        public bool PermitePDF { get; set; }
        public bool Activo { get; set; }
        public int Orden { get; set; }
    }

    /// <summary>
    /// Parámetros para generar un informe
    /// </summary>
    public class ParametrosInforme
    {
        public TipoInformeEnum TipoInforme { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public DateTime? FechaCorte { get; set; }
        public long? RutCliente { get; set; }
        public long? RutDeudor { get; set; }
        public int? CodigoEjecutivo { get; set; }
        public int? TipoDocumento { get; set; }
        public int? CentroCosto { get; set; }
        public TipoDetalleInformeEnum TipoDetalle { get; set; } = TipoDetalleInformeEnum.Resumido;
        public FormatoInformeEnum Formato { get; set; } = FormatoInformeEnum.Pantalla;
    }

    /// <summary>
    /// Resultado genérico de un informe
    /// </summary>
    public class ResultadoInforme<T>
    {
        public TipoInformeEnum TipoInforme { get; set; }
        public string NombreInforme { get; set; } = string.Empty;
        public DateTime? FechaGeneracion { get; set; }
        public List<T> Datos { get; set; } = new();
        public int TotalRegistros { get; set; }
        public Dictionary<string, decimal> Totales { get; set; } = new();
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }
    }

    #endregion

    #region Entidades de Informes Específicos

    /// <summary>
    /// Anticipo Cursado - F_INF1100.frm
    /// </summary>
    public class InformeAnticipoCursado
    {
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
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
        public int CodigoBanco { get; set; }
        public string NombreBanco { get; set; } = string.Empty;
    }

    /// <summary>
    /// Protesto Diario - F_INF1200.frm
    /// </summary>
    public class InformeProtestoDiario
    {
        public DateTime? FechaProtesto { get; set; }
        public int CodigoEjecutivo { get; set; }
        public string NombreEjecutivo { get; set; } = string.Empty;
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RazonSocialCliente { get; set; } = string.Empty;
        public long RutDeudor { get; set; }
        public string DvDeudor { get; set; } = string.Empty;
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
    /// Documento por Deudor - F_INF1300.frm
    /// </summary>
    public class InformeDocumentoDeudor
    {
        public long RutDeudor { get; set; }
        public string DvDeudor { get; set; } = string.Empty;
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
    /// Cartera Cliente - F_INF1400.frm
    /// </summary>
    public class InformeCarteraCliente
    {
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RazonSocialCliente { get; set; } = string.Empty;
        public string NombreEjecutivo { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public int TotalDocumentos { get; set; }
        public decimal MontoVigente { get; set; }
        public decimal MontoMora { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal PorcentajeConcentracion { get; set; }
        public List<DetalleCarteraCliente> Detalle { get; set; } = new();
    }

    /// <summary>
    /// Detalle de cartera cliente
    /// </summary>
    public class DetalleCarteraCliente
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
    /// Devengamiento - F_INF1500.frm
    /// </summary>
    public class InformeDevengamiento
    {
        public DateTime? FechaCorte { get; set; }
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
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
        public int CentroCosto { get; set; }
        public string DescripcionCentroCosto { get; set; } = string.Empty;
    }

    /// <summary>
    /// Adelanto - F_INF1600.frm
    /// </summary>
    public class InformeAdelanto
    {
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
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
    /// Cupo Cliente - F_INF1700.frm
    /// </summary>
    public class InformeCupoCliente
    {
        public int CodigoEjecutivo { get; set; }
        public string NombreEjecutivo { get; set; } = string.Empty;
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RazonSocialCliente { get; set; } = string.Empty;
        public decimal LineaAprobada { get; set; }
        public decimal MontoUtilizado { get; set; }
        public decimal MontoDisponible { get; set; }
        public decimal PorcentajeUtilizacion { get; set; }
        public DateTime? FechaVencimientoLinea { get; set; }
        public string EstadoLinea { get; set; } = string.Empty;
    }

    /// <summary>
    /// Volumen Operaciones - F_INF1800.frm
    /// </summary>
    public class InformeVolumenOperaciones
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; } = string.Empty;
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
    /// Excedente - F_INF1900.frm
    /// </summary>
    public class InformeExcedente
    {
        public long RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
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
    /// Concentración Terceros - F_INF2000.frm
    /// </summary>
    public class InformeConcentracionTerceros
    {
        public long RutDeudor { get; set; }
        public string DvDeudor { get; set; } = string.Empty;
        public string NombreDeudor { get; set; } = string.Empty;
        public int CantidadClientes { get; set; }
        public int CantidadDocumentos { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoVigente { get; set; }
        public decimal MontoVencido { get; set; }
        public decimal PorcentajeConcentracion { get; set; }
        public string? CustodiaAsignada { get; set; }
        public List<DetalleConcentracionPorCliente> DetallePorCliente { get; set; } = new();
    }

    /// <summary>
    /// Detalle de concentración por cliente
    /// </summary>
    public class DetalleConcentracionPorCliente
    {
        public long RutCliente { get; set; }
        public string RazonSocialCliente { get; set; } = string.Empty;
        public int CantidadDocumentos { get; set; }
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }
    }

    #endregion

    #region Resumen y Estadísticas

    /// <summary>
    /// Resumen general para dashboard de informes
    /// </summary>
    public class ResumenInformes
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

    #endregion

    #region Historial

    /// <summary>
    /// Historial de informes generados
    /// </summary>
    public class HistorialInforme
    {
        public int Id { get; set; }
        public int TipoInforme { get; set; }
        public string NombreInforme { get; set; } = string.Empty;
        public DateTime? FechaGeneracion { get; set; }
        public string UsuarioGeneracion { get; set; } = string.Empty;
        public string? ParametrosJson { get; set; }
        public int Formato { get; set; }
        public int TotalRegistros { get; set; }
        public int TiempoEjecucionMs { get; set; }
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }
    }

    #endregion
}
