using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Factoring.Core.DTOs
{
    #region Filtros

    /// <summary>
    /// Filtro para búsqueda de pagarés
    /// </summary>
    public class FiltroPagaresDto
    {
        public int? RutCliente { get; set; }
        public int? NumeroOperacion { get; set; }
        public int? NumeroLinea { get; set; }
        public int? Estado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public DateTime? VencimientoDesde { get; set; }
        public DateTime? VencimientoHasta { get; set; }
        public bool? SoloVigentes { get; set; }
        public bool? SoloVencidos { get; set; }
    }

    /// <summary>
    /// Filtro para búsqueda de avales
    /// </summary>
    public class FiltroAvalesDto
    {
        public int? RutCliente { get; set; }
        public int? RutAval { get; set; }
        public int? TipoAval { get; set; }
        public int? Estado { get; set; }
        public bool? SoloActivos { get; set; }
        public bool? SoloSocios { get; set; }
        public bool ConsultarTodos { get; set; }
    }

    /// <summary>
    /// Filtro para búsqueda de firmas
    /// </summary>
    public class FiltroFirmasDto
    {
        public int? RutCliente { get; set; }
        public int? RutFirmante { get; set; }
        public DateTime? FechaVigenciaDesde { get; set; }
        public DateTime? FechaVigenciaHasta { get; set; }
        public bool? SoloActivas { get; set; }
        public bool ConsultarTodos { get; set; }
    }

    /// <summary>
    /// Filtro para contratos marco
    /// </summary>
    public class FiltroContratosMarcoDto
    {
        public int? RutCliente { get; set; }
        public int? NumeroContrato { get; set; }
        public string? Estado { get; set; }
        public int? EstadoSolicitud { get; set; }
        public int? EstadoLinea { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public DateTime? FechaSolicitudDesde { get; set; }
        public DateTime? FechaSolicitudHasta { get; set; }
        public bool SoloVigentes { get; set; }
    }

    #endregion

    #region DTOs Pagarés

    /// <summary>
    /// DTO de Pagaré para consultas
    /// </summary>
    public class PagareDto
    {
        public int Id { get; set; }
        public int RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RutFormateado { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public int NumeroPagare { get; set; }
        public DateTime? FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal MontoOriginal { get; set; }
        public decimal MontoActual { get; set; }
        public int CodigoMoneda { get; set; }
        public string DescripcionMoneda { get; set; } = string.Empty;
        public int Estado { get; set; }
        public string DescripcionEstado { get; set; } = string.Empty;
        public string TipoEmision { get; set; } = string.Empty;
        public int? NumeroOperacion { get; set; }
        public int? NumeroLinea { get; set; }
        public string NombreDeudor { get; set; } = string.Empty;
        public string DireccionDeudor { get; set; } = string.Empty;
        public int? CodigoNotaria { get; set; }
        public string NombreNotaria { get; set; } = string.Empty;
        public DateTime? FechaProtocolo { get; set; }
        public string Observaciones { get; set; } = string.Empty;
        public decimal? Impuesto { get; set; }
        public bool EstaVencido => FechaVencimiento.HasValue && FechaVencimiento.Value < DateTime.Today && Estado == 1;
        public int? DiasParaVencimiento => FechaVencimiento.HasValue
            ? (int)(FechaVencimiento.Value - DateTime.Today).TotalDays
            : null;
    }

    /// <summary>
    /// DTO para crear pagaré
    /// </summary>
    public class CreatePagareDto
    {
        [Required(ErrorMessage = "El RUT del cliente es requerido")]
        public int RutCliente { get; set; }

        [Required]
        public string DvCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de emisión es requerida")]
        public DateTime? FechaEmision { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoOriginal { get; set; }

        public int CodigoMoneda { get; set; } = 1; // Pesos por defecto

        public string TipoEmision { get; set; } = "V"; // Vista por defecto

        public int? NumeroOperacion { get; set; }
        public int? NumeroLinea { get; set; }
        public int? NumeroSublinea { get; set; }

        [Required(ErrorMessage = "El nombre del deudor es requerido")]
        public string NombreDeudor { get; set; } = string.Empty;

        public string DireccionDeudor { get; set; } = string.Empty;
        public string ComunaDeudor { get; set; } = string.Empty;
        public string CiudadDeudor { get; set; } = string.Empty;
        public int? CodigoNotaria { get; set; }
        public DateTime? FechaProtocolo { get; set; }
        public string NumeroRepertorio { get; set; } = string.Empty;
        public decimal? Impuesto { get; set; }
        public string Observaciones { get; set; } = string.Empty;
        public string CuentaCorriente { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para actualizar pagaré
    /// </summary>
    public class UpdatePagareDto
    {
        public DateTime? FechaVencimiento { get; set; }
        public decimal? MontoActual { get; set; }
        public int? Estado { get; set; }
        public string NombreDeudor { get; set; } = string.Empty;
        public string DireccionDeudor { get; set; } = string.Empty;
        public int? CodigoNotaria { get; set; }
        public DateTime? FechaProtocolo { get; set; }
        public string NumeroRepertorio { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para cambiar estado de pagaré
    /// </summary>
    public class CambiarEstadoPagareDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int NuevoEstado { get; set; }

        public string Motivo { get; set; } = string.Empty;
    }

    #endregion

    #region DTOs Avales

    /// <summary>
    /// DTO de Aval para consultas
    /// </summary>
    public class AvalDto
    {
        public int Id { get; set; }
        public int RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RutClienteFormateado { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public int RutAval { get; set; }
        public string DvAval { get; set; } = string.Empty;
        public string RutAvalFormateado { get; set; } = string.Empty;
        public string NombreAval { get; set; } = string.Empty;
        public int TipoAval { get; set; }
        public string DescripcionTipoAval { get; set; } = string.Empty;
        public int EstadoAval { get; set; }
        public string DescripcionEstado { get; set; } = string.Empty;
        public int? RegimenMatrimonial { get; set; }
        public string DescripcionRegimenMatrimonial { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int? CodigoComuna { get; set; }
        public string DescripcionComuna { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal? PatrimonioDeclarado { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public DateTime? FechaVigencia { get; set; }
        public bool EsSocio { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para crear aval
    /// </summary>
    public class CreateAvalDto
    {
        [Required(ErrorMessage = "El RUT del cliente es requerido")]
        public int RutCliente { get; set; }

        [Required]
        public string DvCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUT del aval es requerido")]
        public int RutAval { get; set; }

        [Required]
        public string DvAval { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del aval es requerido")]
        [MaxLength(100)]
        public string NombreAval { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de aval es requerido")]
        public int TipoAval { get; set; }

        public int? RegimenMatrimonial { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public int? CodigoComuna { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal? PatrimonioDeclarado { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public DateTime? FechaVigencia { get; set; }
        public bool EsSocio { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para actualizar aval
    /// </summary>
    public class UpdateAvalDto
    {
        public string NombreAval { get; set; } = string.Empty;
        public int? TipoAval { get; set; }
        public int? EstadoAval { get; set; }
        public int? RegimenMatrimonial { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public int? CodigoComuna { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal? PatrimonioDeclarado { get; set; }
        public DateTime? FechaVigencia { get; set; }
        public bool? EsSocio { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }

    #endregion

    #region DTOs Firmas

    /// <summary>
    /// DTO de Firma para consultas
    /// </summary>
    public class FirmaDto
    {
        public int Id { get; set; }
        public int RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RutClienteFormateado { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public int RutFirmante { get; set; }
        public string DvFirmante { get; set; } = string.Empty;
        public string RutFirmanteFormateado { get; set; } = string.Empty;
        public string NombreFirmante { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public DateTime? FechaVigenciaDesde { get; set; }
        public DateTime? FechaVigenciaHasta { get; set; }
        public bool Activa { get; set; }
        public string TipoDocumentoFirma { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public bool EstaVigente => Activa &&
            (!FechaVigenciaHasta.HasValue || FechaVigenciaHasta.Value >= DateTime.Today);
    }

    /// <summary>
    /// DTO para crear firma
    /// </summary>
    public class CreateFirmaDto
    {
        [Required(ErrorMessage = "El RUT del cliente es requerido")]
        public int RutCliente { get; set; }

        [Required]
        public string DvCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUT del firmante es requerido")]
        public int RutFirmante { get; set; }

        [Required]
        public string DvFirmante { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del firmante es requerido")]
        [MaxLength(100)]
        public string NombreFirmante { get; set; } = string.Empty;

        public string Cargo { get; set; } = string.Empty;
        public DateTime? FechaVigenciaDesde { get; set; }
        public DateTime? FechaVigenciaHasta { get; set; }
        public string TipoDocumentoFirma { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para actualizar firma
    /// </summary>
    public class UpdateFirmaDto
    {
        public string NombreFirmante { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public DateTime? FechaVigenciaDesde { get; set; }
        public DateTime? FechaVigenciaHasta { get; set; }
        public bool? Activa { get; set; }
        public string TipoDocumentoFirma { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
    }

    #endregion

    #region DTOs Contrato Marco

    /// <summary>
    /// DTO de Contrato Marco para consultas
    /// </summary>
    public class ContratoMarcoDto
    {
        public int Id { get; set; }
        public int RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RutFormateado { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public int NumeroContrato { get; set; }
        public DateTime? FechaSolicitud { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public decimal MontoSolicitado { get; set; }
        public decimal MontoAprobado { get; set; }
        public decimal MontoDisponible { get; set; }
        public int EstadoSolicitud { get; set; }
        public string DescripcionEstadoSolicitud { get; set; } = string.Empty;
        public int EstadoLinea { get; set; }
        public string DescripcionEstadoLinea { get; set; } = string.Empty;
        public decimal? TasaNegocio { get; set; }
        public decimal? Spread { get; set; }
        public int? CodigoNotaria { get; set; }
        public string NombreNotaria { get; set; } = string.Empty;
        public DateTime? FechaEscritura { get; set; }
        public string NumeroEscritura { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public decimal PorcentajeUtilizacion => MontoAprobado > 0
            ? ((MontoAprobado - MontoDisponible) / MontoAprobado) * 100
            : 0;
    }

    /// <summary>
    /// DTO para crear contrato marco
    /// </summary>
    public class CreateContratoMarcoDto
    {
        [Required(ErrorMessage = "El RUT del cliente es requerido")]
        public int RutCliente { get; set; }

        [Required]
        public string DvCliente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de solicitud es requerida")]
        public DateTime? FechaSolicitud { get; set; }

        [Required(ErrorMessage = "El monto solicitado es requerido")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoSolicitado { get; set; }

        public decimal? TasaNegocio { get; set; }
        public decimal? Spread { get; set; }
        public int? CodigoNotaria { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para actualizar contrato marco
    /// </summary>
    public class UpdateContratoMarcoDto
    {
        public DateTime? FechaResolucion { get; set; }
        public decimal? MontoAprobado { get; set; }
        public decimal? MontoSolicitado { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public int? EstadoSolicitud { get; set; }
        public int? EstadoLinea { get; set; }
        public decimal? TasaNegocio { get; set; }
        public decimal? Spread { get; set; }
        public int? CodigoNotaria { get; set; }
        public DateTime? FechaEscritura { get; set; }
        public string NumeroEscritura { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
    }

    #endregion

    #region DTOs Resumen

    /// <summary>
    /// DTO Resumen de garantías por cliente
    /// </summary>
    public class ResumenGarantiasClienteDto
    {
        public int RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public string RutFormateado { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public int TotalPagares { get; set; }
        public int PagaresVigentes { get; set; }
        public decimal MontoPagaresVigentes { get; set; }
        public int TotalAvales { get; set; }
        public int AvalesActivos { get; set; }
        public int TotalFirmas { get; set; }
        public int FirmasVigentes { get; set; }
        public int TotalContratosMarco { get; set; }
        public decimal MontoLineasAprobadas { get; set; }
        public decimal MontoLineasDisponibles { get; set; }
    }

    /// <summary>
    /// DTO Resumen general del módulo Legal
    /// </summary>
    public class ResumenLegalDto
    {
        public int TotalPagares { get; set; }
        public int PagaresVigentes { get; set; }
        public int PagaresVencidos { get; set; }
        public int PagaresEmitidosHoy { get; set; }
        public decimal MontoTotalPagares { get; set; }
        public int TotalAvales { get; set; }
        public int AvalesActivos { get; set; }
        public int AvalesPorVencer { get; set; }
        public int TotalFirmas { get; set; }
        public int FirmasVigentes { get; set; }
        public int FirmasPorVencer { get; set; }
        public int TotalContratosMarco { get; set; }
        public decimal MontoTotalLineas { get; set; }
        public decimal MontoDisponibleLineas { get; set; }
    }

    /// <summary>
    /// DTO para emisión de pagaré
    /// </summary>
    public class EmisionPagareDto
    {
        public int Id { get; set; }
        public int IdPagare { get; set; }
        public int RutDeudor { get; set; }
        public string DvDeudor { get; set; } = string.Empty;
        public string RutDeudorFormateado { get; set; } = string.Empty;
        public string NombreDeudor { get; set; } = string.Empty;
        public string DireccionDeudor { get; set; } = string.Empty;
        public string CiudadDeudor { get; set; } = string.Empty;
        public int CodigoMoneda { get; set; }
        public string DescripcionMoneda { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string TipoEmision { get; set; } = string.Empty;
        public DateTime? FechaVencimiento { get; set; }
        public DateTime? FechaEmision { get; set; }
        public bool Impreso { get; set; }
        public DateTime? FechaImpresion { get; set; }
    }

    #endregion
}
