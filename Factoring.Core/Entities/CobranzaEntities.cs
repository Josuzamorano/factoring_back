namespace Factoring.Core.Entities;

/// <summary>
/// Entidad Pago de Documento - Migrada de F_CO1500.frm/F_CO1600.frm
/// Registra los pagos realizados sobre documentos (facturas, cheques, letras)
/// </summary>
public class PagoDocumento
{
    public int NumeroPago { get; set; }              // PAG_NUM_PAG
    public DateTime? FechaPago { get; set; }          // PAG_FEC_PAG
    public int TipoDocumento { get; set; }           // PAG_TIP_DOC (1=Factura, 2=Cheque, 3=Letra)
    public long NumeroDocumento { get; set; }        // PAG_NUM_DOC
    public int NumeroOperacion { get; set; }         // PAG_NUM_OPE
    public long RutCliente { get; set; }             // PAG_RUT_CLI
    public long RutDeudor { get; set; }              // PAG_RUT_DEU
    public decimal MontoDocumento { get; set; }      // PAG_MTO_DOC
    public decimal SaldoDocumento { get; set; }      // PAG_SAL_DOC
    public decimal MontoPago { get; set; }           // PAG_MTO_PAG
    public decimal MontoMora { get; set; }           // PAG_MTO_MOR
    public decimal MontoDescuento { get; set; }      // PAG_MTO_DES
    public decimal MontoAbono { get; set; }          // PAG_MTO_ABO
    public decimal DeudaNeta { get; set; }           // PAG_DEU_NET
    public decimal TasaMora { get; set; }            // PAG_TAS_MOR
    public decimal TasaNuevaMora { get; set; }       // PAG_TAS_MOR_NEW
    public int TipoPago { get; set; }                // PAG_TIP_PAG (1=Pago Deudor, 2=Pago Cliente)
    public int EstadoPago { get; set; }              // PAG_EST_PAG (1=Pendiente, 2=Aprobado, 3=Rechazado, 4=Eliminado)
    public int? CodigoGestion { get; set; }          // PAG_COD_GES (código de gestión asociada)
    public bool ConGestion { get; set; }             // Si tiene gestión asociada
    public string? UsuarioCreacion { get; set; }     // PAG_USR_CRE
    public DateTime? FechaCreacion { get; set; }      // PAG_FEC_CRE
    public string? UsuarioModificacion { get; set; } // PAG_USR_MOD
    public DateTime? FechaModificacion { get; set; } // PAG_FEC_MOD
    
    // Navegación
    public string? NombreCliente { get; set; }
    public string? NombreDeudor { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public string? DescripcionEstadoPago { get; set; }
    public string? DescripcionGestion { get; set; }
}

/// <summary>
/// Entidad Recaudación Documento - Migrada de F_CO1500.frm (panel recaudación)
/// Datos específicos del cheque/documento de recaudación
/// </summary>
public class RecaudacionDocumento
{
    public int NumeroRecaudacion { get; set; }       // REC_NUM_REC
    public int NumeroPago { get; set; }              // REC_NUM_PAG (FK a PagoDocumento)
    public string NumeroDocumentoRecaudacion { get; set; } = string.Empty; // REC_NUM_DOC (número cheque)
    public int CodigoBanco { get; set; }             // REC_COD_BCO
    public int CodigoPlaza { get; set; }             // REC_COD_PZA
    public decimal MontoRecaudacion { get; set; }    // REC_MTO_REC
    public DateTime? FechaDeposito { get; set; }      // REC_FEC_DEP
    public DateTime? FechaVencimientoReal { get; set; } // REC_FEC_VTO
    public int DiasRetencion { get; set; }           // REC_DIA_RET
    public DateTime? FechaLiberacion { get; set; }   // REC_FEC_LIB
    public bool ChequeANombreEmpresa { get; set; }   // REC_CHK_NOM (si cheque a nombre de la empresa)
    public int Estado { get; set; }                  // REC_EST_REC
    
    // Navegación
    public string? NombreBanco { get; set; }
    public string? NombrePlaza { get; set; }
}

/// <summary>
/// Verificación de Documentos - Migrada de F_CO2100.frm
/// Lista de documentos pendientes de verificación
/// </summary>
public class VerificacionDocumento
{
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public int CantidadDocumentos { get; set; }
    public decimal MontoTotal { get; set; }
    public bool Verificado { get; set; }
    public bool Otorgado { get; set; }
    public bool TieneCaratula { get; set; }
    
    // Filtros aplicados
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}

/// <summary>
/// Detalle de Verificación - Para drill-down desde VerificacionDocumento
/// </summary>
public class DetalleVerificacion
{
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public int EstadoDocumento { get; set; }
    public int? EstadoVerificacion { get; set; }
    public string? Observacion { get; set; }
    
    public string? DescripcionEstado { get; set; }
    public string? DescripcionVerificacion { get; set; }
}

/// <summary>
/// Movimiento de Caja - Migrada de F_CO2300.frm
/// Consulta y gestión de pagos en caja
/// </summary>
public class MovimientoCaja
{
    public int NumeroMovimiento { get; set; }        // CAJ_NUM_MOV
    public DateTime? FechaMovimiento { get; set; }    // CAJ_FEC_MOV
    public int TipoDocumento { get; set; }           // CAJ_TIP_DOC
    public long NumeroDocumento { get; set; }        // CAJ_NUM_DOC
    public int NumeroOperacion { get; set; }         // CAJ_NUM_OPE
    public long RutCliente { get; set; }             // CAJ_RUT_CLI
    public long RutDeudor { get; set; }              // CAJ_RUT_DEU
    public decimal Monto { get; set; }               // CAJ_MTO_MOV
    public int EstadoCaja { get; set; }              // CAJ_EST_MOV (1=Actual, 2=Retención, 3=Procesado, etc.)
    public int EstadoPago { get; set; }              // (Todos, Pendientes, Rechazados, Aprobados, Eliminados)
    public string? UsuarioCaja { get; set; }
    public DateTime? FechaCreacion { get; set; }
    
    // Navegación
    public string? NombreCliente { get; set; }
    public string? NombreDeudor { get; set; }
    public string? DescripcionEstadoCaja { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
}

/// <summary>
/// Resumen de Caja - Totales por estado
/// </summary>
public class ResumenCaja
{
    public DateTime? Fecha { get; set; }
    public int TipoDocumento { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public int CantidadActuales { get; set; }
    public decimal MontoActuales { get; set; }
    public int CantidadRetencion { get; set; }
    public decimal MontoRetencion { get; set; }
    public int CantidadProcesados { get; set; }
    public decimal MontoProcesados { get; set; }
    public int CantidadTotal { get; set; }
    public decimal MontoTotal { get; set; }
}

/// <summary>
/// Verificación Diaria - Migrada de F_CO2510.frm
/// </summary>
public class VerificacionDiaria
{
    public DateTime? FechaVerificacion { get; set; }
    public string? HoraVerificacion { get; set; }
    public int TipoDocumento { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public int NumeroOperacion { get; set; }
    public long NumeroDocumento { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public int EstadoVerificacion { get; set; }
    public string? Resultado { get; set; }
    public string? Observacion { get; set; }
    public string? UsuarioVerificacion { get; set; }

    // Campos adicionales del VB6
    public string? PersonaContacto { get; set; }      // DVF_PER_VER - Se conversó con
    public string? CargoPersona { get; set; }         // DVF_CAR_PER - Cargo de la persona
    public string? FacturaRecibida { get; set; }      // DVF_FAC_REC - Factura recibida (S/N)
    public string? FacturaIngresada { get; set; }     // DVF_FAC_ING - Factura ingresada (S/N)
    public string? AceptaNotificacion { get; set; }   // DVF_ACE_NOT - Acepta notificación (S/N)
    public string? DocumentoCedido { get; set; }      // DVF_COB_FAC - Documento cedido (S/N)
    public string? DepositoCliente { get; set; }      // DVF_DEP_CLI - Depósito al cliente (S/N)
    public string? EntregaInformacion { get; set; }   // DVF_ENT_INF - Entrega información (S/N)
    public string? TimbreDeudor { get; set; }         // DVF_TIM_BRE - Timbre deudor (S/N)

    public string? DescripcionTipoDocumento { get; set; }
}

/// <summary>
/// Documento No Cedido - Migrada de F_CO2600.frm
/// Documentos que no fueron cedidos a la empresa de factoring
/// </summary>
public class DocumentoNoCedido
{
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public string? MotivoNoCesion { get; set; }
    public DateTime? FechaRegistro { get; set; }
    
    public string? DescripcionTipoDocumento { get; set; }
}

/// <summary>
/// Informe de Abonos - Para reportes consolidados
/// </summary>
public class InformeAbono
{
    public DateTime? FechaAbono { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal MontoAbono { get; set; }
    public decimal SaldoPendiente { get; set; }
    public int TipoPago { get; set; }
    public string? NombreCobrador { get; set; }
    
    public string? DescripcionTipoDocumento { get; set; }
    public string? DescripcionTipoPago { get; set; }
}

/// <summary>
/// Gasto de Cobranza - Gastos asociados a pagos
/// </summary>
public class GastoCobranza
{
    public int NumeroGasto { get; set; }
    public int NumeroPago { get; set; }
    public int TipoGasto { get; set; }
    public string? DescripcionGasto { get; set; }
    public decimal MontoGasto { get; set; }
    public DateTime? FechaGasto { get; set; }
}

/// <summary>
/// Estado del pago para filtros en caja
/// </summary>
public enum EstadoPagoEnum
{
    Todos = 0,
    Pendientes = 1,
    Rechazados = 2,
    Aprobados = 3,
    Eliminados = 4
}

/// <summary>
/// Estado de caja para filtros
/// </summary>
public enum EstadoCajaEnum
{
    Todos = 0,
    Actuales = 1,
    EnRetencion = 2,
    Procesados = 3
}

/// <summary>
/// Documento para Consulta Masiva
/// Permite consultar todos los documentos con múltiples filtros
/// </summary>
public class DocumentoConsultaMasiva
{
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public DateTime? FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public DateTime? FechaVencimientoReal { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal SaldoPendiente { get; set; }
    public decimal MontoAbonado { get; set; }
    public int EstadoDocumento { get; set; }
    public string? DescripcionEstado { get; set; }
    public int EstadoCesion { get; set; }
    public string? DescripcionCesion { get; set; }
    public int DiasVencidos { get; set; }
    public bool Protestado { get; set; }
    public bool EnCobranza { get; set; }
    public string? Ejecutivo { get; set; }
    public int? CodigoEjecutivo { get; set; }
}
