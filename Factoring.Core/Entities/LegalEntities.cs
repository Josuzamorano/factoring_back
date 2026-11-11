using System;
using System.Collections.Generic;

namespace Factoring.Core.Entities
{
    #region Enums

    /// <summary>
    /// Tipos de garantía legal
    /// </summary>
    public enum TipoGarantiaEnum
    {
        Pagare = 1,
        Cheque = 2,
        Aval = 3,
        Firma = 4,
        ContratoMarco = 5,
        Otros = 9
    }

    /// <summary>
    /// Estados de pagaré
    /// </summary>
    public enum EstadoPagareEnum
    {
        Vigente = 1,
        Emitido = 2,
        Vencido = 3,
        Cobrado = 4,
        Protestado = 5,
        Anulado = 9
    }

    /// <summary>
    /// Estados de aval
    /// </summary>
    public enum EstadoAvalEnum
    {
        Activo = 1,
        Inactivo = 2,
        Suspendido = 3,
        Liberado = 4
    }

    /// <summary>
    /// Tipos de aval
    /// </summary>
    public enum TipoAvalEnum
    {
        Solidario = 1,
        Personal = 2,
        Hipotecario = 3,
        Prendario = 4
    }

    /// <summary>
    /// Estados de régimen matrimonial
    /// </summary>
    public enum RegimenMatrimonialEnum
    {
        SociedadConyugal = 1,
        SeparacionBienes = 2,
        ParticipacionGananciales = 3,
        Soltero = 4,
        NoDeclarado = 9
    }

    #endregion

    #region Entidades Principales

    /// <summary>
    /// Pagaré - Tabla PAG
    /// Migrado de F_LE1100.frm (Consulta Garantía) y F_LE1400.frm (Ingreso Garantía)
    /// </summary>
    public class Pagare
    {
        public int Id { get; set; }
        public int RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public int NumeroPagare { get; set; }
        public DateTime? FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal MontoOriginal { get; set; }
        public decimal MontoActual { get; set; }
        public int CodigoMoneda { get; set; }
        public int Estado { get; set; }
        public string TipoEmision { get; set; } = string.Empty; // "V" = Vista, "P" = Plazo
        public int? NumeroOperacion { get; set; }
        public int? NumeroLinea { get; set; }
        public int? NumeroSublinea { get; set; }
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
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public int? UsuarioCreacion { get; set; }
        public int? UsuarioModificacion { get; set; }

        // Propiedades de navegación
        public string RutFormateado => $"{RutCliente:N0}-{DvCliente}";
        public string DescripcionEstado { get; set; } = string.Empty;
        public string DescripcionMoneda { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
    }

    /// <summary>
    /// Aval - Tabla AVA
    /// Migrado de F_LE1200.frm (Consulta Avales) y F_LE1500.frm (Ingreso Avales)
    /// </summary>
    public class Aval
    {
        public int Id { get; set; }
        public int RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public int RutAval { get; set; }
        public string DvAval { get; set; } = string.Empty;
        public string NombreAval { get; set; } = string.Empty;
        public int TipoAval { get; set; }
        public int EstadoAval { get; set; }
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
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public int? UsuarioCreacion { get; set; }
        public int? UsuarioModificacion { get; set; }

        // Propiedades de navegación
        public string RutClienteFormateado => $"{RutCliente:N0}-{DvCliente}";
        public string RutAvalFormateado => $"{RutAval:N0}-{DvAval}";
        public string DescripcionTipoAval { get; set; } = string.Empty;
        public string DescripcionEstado { get; set; } = string.Empty;
        public string DescripcionRegimenMatrimonial { get; set; } = string.Empty;
        public string DescripcionComuna { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
    }

    /// <summary>
    /// Firma autorizada - Tabla FIR
    /// Migrado de F_LE1300.frm (Consulta Firmas) y F_LE1600.frm (Ingreso Firmas)
    /// </summary>
    public class Firma
    {
        public int Id { get; set; }
        public int RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public int RutFirmante { get; set; }
        public string DvFirmante { get; set; } = string.Empty;
        public string NombreFirmante { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public DateTime? FechaVigenciaDesde { get; set; }
        public DateTime? FechaVigenciaHasta { get; set; }
        public bool Activa { get; set; }
        public string TipoDocumentoFirma { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public int? UsuarioCreacion { get; set; }
        public int? UsuarioModificacion { get; set; }

        // Propiedades de navegación
        public string RutClienteFormateado => $"{RutCliente:N0}-{DvCliente}";
        public string RutFirmanteFormateado => $"{RutFirmante:N0}-{DvFirmante}";
        public string NombreCliente { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contrato Marco Global - Relacionado con F_LE1700.frm
    /// </summary>
    public class ContratoMarco
    {
        public int Id { get; set; }
        public int RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
        public int NumeroContrato { get; set; }
        public DateTime? FechaSolicitud { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public decimal MontoSolicitado { get; set; }
        public decimal MontoAprobado { get; set; }
        public decimal MontoDisponible { get; set; }
        public int EstadoSolicitud { get; set; }
        public int EstadoLinea { get; set; }
        public decimal? TasaNegocio { get; set; }
        public decimal? Spread { get; set; }
        public int? CodigoNotaria { get; set; }
        public DateTime? FechaEscritura { get; set; }
        public string NumeroEscritura { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // Propiedades de navegación
        public string RutFormateado => $"{RutCliente:N0}-{DvCliente}";
        public string DescripcionEstadoSolicitud { get; set; } = string.Empty;
        public string DescripcionEstadoLinea { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string NombreNotaria { get; set; } = string.Empty;
    }

    /// <summary>
    /// Emisión de Pagaré - Migrado de F_LE1800.frm
    /// </summary>
    public class EmisionPagare
    {
        public int Id { get; set; }
        public int IdPagare { get; set; }
        public int RutDeudor { get; set; }
        public string DvDeudor { get; set; } = string.Empty;
        public string NombreDeudor { get; set; } = string.Empty;
        public string DireccionDeudor { get; set; } = string.Empty;
        public string CiudadDeudor { get; set; } = string.Empty;
        public int CodigoMoneda { get; set; }
        public decimal Monto { get; set; }
        public string TipoEmision { get; set; } = string.Empty; // "V" = Vista, "P" = Plazo
        public DateTime? FechaVencimiento { get; set; }
        public DateTime? FechaEmision { get; set; }
        public DateTime? FechaCorreccion { get; set; }
        public bool Impreso { get; set; }
        public DateTime? FechaImpresion { get; set; }
        public int? UsuarioEmision { get; set; }

        // Propiedades de navegación
        public string RutDeudorFormateado => $"{RutDeudor:N0}-{DvDeudor}";
        public string DescripcionMoneda { get; set; } = string.Empty;
    }

    #endregion

    #region Resúmenes y Consultas

    /// <summary>
    /// Resumen de garantías por cliente
    /// </summary>
    public class ResumenGarantiasCliente
    {
        public int RutCliente { get; set; }
        public string DvCliente { get; set; } = string.Empty;
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

        public string RutFormateado => $"{RutCliente:N0}-{DvCliente}";
    }

    /// <summary>
    /// Resumen general del módulo Legal
    /// </summary>
    public class ResumenLegal
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
    /// Historial de acciones legales
    /// </summary>
    public class HistorialLegal
    {
        public int Id { get; set; }
        public string TipoEntidad { get; set; } = string.Empty; // PAG, AVA, FIR, CTM
        public int IdEntidad { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime? FechaAccion { get; set; }
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
    }

    #endregion
}
