// ====================================================
// Entidades del Módulo Comercial
// Migrado de: F_CM1000.frm, F_CM1200.frm, F_CM1400.frm, 
//             F_CM1500.frm, F_CM1700.frm, F_CM1800.frm
// ====================================================

namespace Factoring.Core.Entities;

/// <summary>
/// Excedente de operación - Tabla EXC
/// Migrado de: F_CM1200.frm (Gestión de Excedentes)
/// </summary>
public class Excedente
{
    public int NumeroExcedente { get; set; }          // EXC_NUM_EXC
    public long RutCliente { get; set; }               // EXC_RUT_CLI
    public int TipoDocumento { get; set; }             // EXC_TIP_DOC
    public string NumeroDocumento { get; set; } = string.Empty; // EXC_NUM_DOC
    public DateTime? FechaGeneracion { get; set; }      // EXC_FEC_GEN
    public decimal MontoExcedente { get; set; }        // EXC_MON_EXC
    public decimal SaldoExcedente { get; set; }        // EXC_SAL_EXC
    public decimal SaldoInicial { get; set; }          // EXC_SAL_INI
    public decimal SaldoFinal { get; set; }            // EXC_SAL_FIN
    public int EstadoExcedente { get; set; }           // EXC_EST_EXC (5=Retenido, 6=Rechazado, 1=Generado)
    public string? Observaciones { get; set; }         // EXC_OBS_EXC
    
    // Navegación
    public string? NombreCliente { get; set; }
    public string? DescripcionEstado { get; set; }
    public string? TipoDocumentoDescripcion { get; set; }
}

/// <summary>
/// Detalle de excedente generado - Tabla EXCGEN
/// </summary>
public class ExcedenteGenerado
{
    public int NumeroExcedente { get; set; }           // EXC_NUM_EXC
    public string NumeroDocumento { get; set; } = string.Empty; // EXC_NUM_DOC
    public DateTime? FechaGeneracion { get; set; }      // EXC_FEC_GEN
    public decimal MontoExcedente { get; set; }        // EXC_SAL_INI
    public decimal SaldoExcedente { get; set; }        // EXC_SAL_FIN
}

/// <summary>
/// Detalle de aplicación de excedente - Tabla DET_EXC
/// </summary>
public class DetalleExcedente
{
    public int NumeroExcedente { get; set; }           // DET_NUM_EXC
    public string NumeroDocumento { get; set; } = string.Empty; // DET_NUM_DOC
    public decimal MontoAplicado { get; set; }         // DET_MTO_APL
    public int EstadoExcedente { get; set; }           // DET_EST_EXC (1=Activo)
}

/// <summary>
/// Vale de venta de excedente - Tabla VVE
/// </summary>
public class ValeVenta
{
    public int NumeroUnico { get; set; }               // VVE_NRO_UNI
    public int NumeroExcedente { get; set; }           // VVE_NUM_EXC
    public decimal MontoAbono { get; set; }            // VVE_MTO_ABO
    public int EstadoAdoption { get; set; }            // VVE_EST_ADO (1=Generado, 2=Preparado)
    public string? Observaciones { get; set; }         // VVE_OBS_VVE
    public DateTime? FechaCreacion { get; set; }
}

/// <summary>
/// Prórroga de documento - Migrado de F_CM1400.frm
/// </summary>
public class Prorroga
{
    public int NumeroProrrroga { get; set; }           // PRO_NUM_PRO
    public int TipoDocumento { get; set; }             // Tipo de documento (1=Factura, 2=Cheque, 3=Letra)
    public string NumeroDocumento { get; set; } = string.Empty; // Número del documento
    public int NumeroOperacion { get; set; }           // Operación relacionada
    public long RutCliente { get; set; }               // Cliente
    public long RutDeudor { get; set; }                // Deudor
    public DateTime? FechaVencimientoOriginal { get; set; } // Vencimiento original
    public DateTime? FechaVencimientoNueva { get; set; }    // Nueva fecha de vencimiento
    public int DiasProrrroga { get; set; }             // Días de prórroga
    public decimal PorcentajeProrrroga { get; set; }   // % prórroga
    public decimal MontoAbono { get; set; }            // Monto abonado
    public int TipoAbono { get; set; }                 // Tipo de abono
    public string? Observaciones { get; set; }         // Observaciones
    public int Estado { get; set; }                    // 1=Pendiente, 2=Aprobada, 3=Rechazada, 4=Contabilizada
    public DateTime? FechaSolicitud { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? UsuarioSolicita { get; set; }
    public string? UsuarioAprueba { get; set; }
    
    // Navegación
    public string? NombreCliente { get; set; }
    public string? NombreDeudor { get; set; }
    public string? DescripcionEstado { get; set; }
}

/// <summary>
/// Renovación de documento - Migrado de F_CM1500.frm
/// </summary>
public class Renovacion
{
    public int NumeroRenovacion { get; set; }
    public int TipoDocumentoOriginal { get; set; }
    public string NumeroDocumentoOriginal { get; set; } = string.Empty;
    public int NumeroOperacionOriginal { get; set; }
    public int TipoDocumentoNuevo { get; set; }
    public string NumeroDocumentoNuevo { get; set; } = string.Empty;
    public int NumeroOperacionNueva { get; set; }
    public long RutCliente { get; set; }
    public long RutDeudor { get; set; }
    public decimal MontoOriginal { get; set; }
    public decimal MontoNuevo { get; set; }
    public DateTime? FechaVencimientoOriginal { get; set; }
    public DateTime? FechaVencimientoNueva { get; set; }
    public int DiasPlazo { get; set; }
    public string? Observaciones { get; set; }
    public int Estado { get; set; }                    // 1=Pendiente, 2=Aprobada, 3=Rechazada
    public DateTime? FechaRenovacion { get; set; }
    public string? UsuarioRegistra { get; set; }
    
    // Navegación
    public string? NombreCliente { get; set; }
    public string? NombreDeudor { get; set; }
}

/// <summary>
/// Informe de documentos por cliente - Migrado de F_CM1700.frm
/// </summary>
public class DocumentoCliente
{
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public long RutCliente { get; set; }
    public long RutDeudor { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal SaldoDocumento { get; set; }
    public DateTime? FechaOperacion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int DiasVencido { get; set; }
    public int Estado { get; set; }
    
    // Navegación
    public string? NombreCliente { get; set; }
    public string? NombreDeudor { get; set; }
    public string? TipoDocumentoDescripcion { get; set; }
    public string? DescripcionEstado { get; set; }
}

/// <summary>
/// Resumen de excedentes por cliente
/// </summary>
public class ResumenExcedente
{
    public long RutCliente { get; set; }
    public string RutClienteFormateado { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public decimal MontoTotalExcedentes { get; set; }
    public decimal SaldoTotalExcedentes { get; set; }
    public int CantidadExcedentes { get; set; }
}

/// <summary>
/// Consulta de otorgamiento - Migrado de F_PUBLI05.frm
/// </summary>
public class ConsultaOtorgamiento
{
    public int NumeroOperacion { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime? FechaOperacion { get; set; }
    public DateTime? FechaOtorgamiento { get; set; }
    public decimal MontoOperacion { get; set; }
    public decimal MontoOtorgado { get; set; }
    public int EstadoOperacion { get; set; }
    public string? EstadoDescripcion { get; set; }
    public int TipoMoneda { get; set; }
    public string? MonedaDescripcion { get; set; }
    public int CantidadDocumentos { get; set; }
}

/// <summary>
/// Cartola de cliente - Migrado de F_GC1900.frm
/// </summary>
public class CartolaCliente
{
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime? Fecha { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty; // OPE, PAG, EXC, PRO
    public string Descripcion { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public decimal Cargo { get; set; }
    public decimal Abono { get; set; }
    public decimal Saldo { get; set; }
}

/// <summary>
/// Enumeración de estados de excedente
/// </summary>
public enum EstadoExcedenteEnum
{
    Generado = 1,
    Preparado = 2,
    Aprobado = 3,
    Rechazado = 6,
    Retenido = 5
}

/// <summary>
/// Enumeración de estados de prórroga
/// </summary>
public enum EstadoProrrogaEnum
{
    Pendiente = 1,
    Aprobada = 2,
    Rechazada = 3,
    Contabilizada = 4
}

/// <summary>
/// Enumeración de estados de renovación
/// </summary>
public enum EstadoRenovacionEnum
{
    Pendiente = 1,
    Aprobada = 2,
    Rechazada = 3
}
