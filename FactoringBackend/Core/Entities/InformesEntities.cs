using System;
using System.Collections.Generic;

namespace FactoringBackend.Core.Entities
{
    #region Enums
    
    /// <summary>
    /// Tipos de informes disponibles en el sistema
    /// Basado en F_INF1000.frm - Menú de Informes
    /// </summary>
    public enum TipoInformeEnum
    {
        AnticiposCursados = 1,      // F_INF1100 - Informe de Anticipos Cursados
        ProtestosDiarios = 2,        // F_INF1200 - Informe Diario de Protestos por Ejecutivo
        DocumentosPorDeudor = 3,     // F_INF1300 - Documentos Comprados por Deudor
        CarteraCliente = 4,          // F_INF1400 - Cartera de Cliente
        DetalleDevengamiento = 5,    // F_INF1500 - Detalle Devengamiento
        Adelantos = 6,               // F_INF1600 - Informe de Adelantos
        CupoCliente = 7,             // F_INF1700 - Cupo Cliente por Ejecutivo
        VolumenOperaciones = 8,      // F_INF1800 - Volumen de Operaciones
        Excedentes = 9,              // F_INF1900 - Detalle de Excedentes
        ConcentracionTerceros = 10,  // F_INF2000 - Concentración Terceros
        InformeGestion = 11,         // Informe de Gestión
        ProtestosCliente = 12,       // Protestos por Cliente
        AvisoProtestos = 13,         // Avisos de Protestos
        CustodiaCheques = 14         // Custodia de Cheques
    }

    /// <summary>
    /// Formato de salida del informe
    /// </summary>
    public enum FormatoInformeEnum
    {
        Pantalla = 1,
        Excel = 2,
        PDF = 3,
        Impresora = 4
    }

    /// <summary>
    /// Tipo de detalle para informes
    /// </summary>
    public enum TipoDetalleInformeEnum
    {
        Resumido = 1,
        Detallado = 2
    }

    #endregion

    #region Entidades Base

    /// <summary>
    /// Definición de un informe en el sistema
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
    /// Parámetros comunes para generación de informes
    /// </summary>
    public class ParametrosInforme
    {
        public TipoInformeEnum TipoInforme { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public long? RutCliente { get; set; }
        public long? RutDeudor { get; set; }
        public int? CodigoEjecutivo { get; set; }
        public int? TipoDocumento { get; set; }
        public int? TipoMoneda { get; set; }
        public TipoDetalleInformeEnum TipoDetalle { get; set; } = TipoDetalleInformeEnum.Resumido;
        public FormatoInformeEnum Formato { get; set; } = FormatoInformeEnum.Pantalla;
        public string? OrdenarPor { get; set; }
        public bool OrdenDescendente { get; set; }
        public int? CentroCosto { get; set; }
        public bool TodosLosClientes { get; set; }
    }

    /// <summary>
    /// Resultado de ejecución de un informe
    /// </summary>
    public class ResultadoInforme<T>
    {
        public TipoInformeEnum TipoInforme { get; set; }
        public string NombreInforme { get; set; } = string.Empty;
        public DateTime? FechaGeneracion { get; set; } = DateTime.Now;
        public string UsuarioGeneracion { get; set; } = string.Empty;
        public ParametrosInforme Parametros { get; set; } = new();
        public List<T> Datos { get; set; } = new();
        public int TotalRegistros { get; set; }
        public Dictionary<string, object> Totales { get; set; } = new();
        public string? MensajeError { get; set; }
        public bool Exitoso => string.IsNullOrEmpty(MensajeError);
    }

    #endregion

    #region Informes Específicos

    /// <summary>
    /// Informe de Anticipos Cursados (F_INF1100)
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
    /// Informe de Protestos Diarios por Ejecutivo (F_INF1200)
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
    /// Informe de Documentos por Deudor (F_INF1300)
    /// Consulta tablas: FAO, CHO, LEO, OTO, OPE
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
    /// Informe de Cartera por Cliente (F_INF1400)
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
    /// Informe de Detalle Devengamiento (F_INF1500)
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
        public int? CentroCosto { get; set; }
        public string DescripcionCentroCosto { get; set; } = string.Empty;
    }

    /// <summary>
    /// Informe de Adelantos (F_INF1600)
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
    /// Informe de Cupo por Cliente (F_INF1700)
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
    /// Informe de Volumen de Operaciones (F_INF1800)
    /// </summary>
    public class InformeVolumenOperaciones
    {
        public int Periodo { get; set; }  // Año-Mes
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
    /// Informe de Excedentes (F_INF1900)
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
    /// Informe de Concentración de Terceros (F_INF2000)
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

    public class DetalleConcentracionPorCliente
    {
        public long RutCliente { get; set; }
        public string RazonSocialCliente { get; set; } = string.Empty;
        public int CantidadDocumentos { get; set; }
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }
    }

    #endregion

    #region Resúmenes y Estadísticas

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

    /// <summary>
    /// Historial de informes generados
    /// </summary>
    public class HistorialInforme
    {
        public int Id { get; set; }
        public TipoInformeEnum TipoInforme { get; set; }
        public string NombreInforme { get; set; } = string.Empty;
        public DateTime? FechaGeneracion { get; set; }
        public string UsuarioGeneracion { get; set; } = string.Empty;
        public string ParametrosJson { get; set; } = string.Empty;
        public FormatoInformeEnum Formato { get; set; }
        public int TotalRegistros { get; set; }
        public int TiempoEjecucionMs { get; set; }
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }
    }

    #endregion
}
