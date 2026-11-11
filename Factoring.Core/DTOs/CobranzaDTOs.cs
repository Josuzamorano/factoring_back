namespace Factoring.Core.DTOs;

// ==========================================
// DTOs para Pago de Documentos
// ==========================================

/// <summary>
/// DTO para Pago de Documento
/// </summary>
public class PagoDocumentoDto
{
    public int NumeroPago { get; set; }
    public DateTime? FechaPago { get; set; }
    public int TipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public int NumeroOperacion { get; set; }
    public long RutCliente { get; set; }
    public string? NombreCliente { get; set; }
    public long RutDeudor { get; set; }
    public string? NombreDeudor { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal SaldoDocumento { get; set; }
    public decimal MontoPago { get; set; }
    public decimal MontoMora { get; set; }
    public decimal MontoDescuento { get; set; }
    public decimal MontoAbono { get; set; }
    public decimal DeudaNeta { get; set; }
    public decimal TasaMora { get; set; }
    public decimal TasaNuevaMora { get; set; }
    public int TipoPago { get; set; }
    public int EstadoPago { get; set; }
    public int? CodigoGestion { get; set; }
    public bool ConGestion { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public string? DescripcionEstadoPago { get; set; }
    public string? DescripcionGestion { get; set; }
    public DateTime? FechaCreacion { get; set; }
}

/// <summary>
/// DTO para crear un nuevo pago
/// </summary>
public class PagoDocumentoCreateDto
{
    public DateTime? FechaPago { get; set; }
    public int TipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public int NumeroOperacion { get; set; }
    public long RutCliente { get; set; }
    public long RutDeudor { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal SaldoDocumento { get; set; }
    public decimal MontoPago { get; set; }
    public decimal MontoMora { get; set; }
    public decimal MontoDescuento { get; set; }
    public decimal MontoAbono { get; set; }
    public decimal DeudaNeta { get; set; }
    public decimal TasaMora { get; set; }
    public decimal TasaNuevaMora { get; set; }
    public int TipoPago { get; set; } // 1=Pago Deudor, 2=Pago Cliente
    public int? CodigoGestion { get; set; }
    public bool ConGestion { get; set; }
    public string? Usuario { get; set; }
}

/// <summary>
/// DTO para actualizar un pago (aprobar/rechazar/eliminar)
/// </summary>
public class PagoDocumentoUpdateDto
{
    public int NumeroPago { get; set; }
    public int EstadoPago { get; set; } // 1=Pendiente, 2=Aprobado, 3=Rechazado, 4=Eliminado
    public string? Usuario { get; set; }
    public string? MotivoRechazo { get; set; }
}

// ==========================================
// DTOs para Recaudación
// ==========================================

/// <summary>
/// DTO para documento de recaudación (cheque de pago)
/// </summary>
public class RecaudacionDocumentoDto
{
    public int NumeroRecaudacion { get; set; }
    public int NumeroPago { get; set; }
    public string NumeroDocumentoRecaudacion { get; set; } = string.Empty;
    public int CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
    public int CodigoPlaza { get; set; }
    public string? NombrePlaza { get; set; }
    public decimal MontoRecaudacion { get; set; }
    public DateTime? FechaDeposito { get; set; }
    public DateTime? FechaVencimientoReal { get; set; }
    public int DiasRetencion { get; set; }
    public DateTime? FechaLiberacion { get; set; }
    public bool ChequeANombreEmpresa { get; set; }
    public int Estado { get; set; }
}

/// <summary>
/// DTO para crear recaudación
/// </summary>
public class RecaudacionDocumentoCreateDto
{
    public int NumeroPago { get; set; }
    public string NumeroDocumentoRecaudacion { get; set; } = string.Empty;
    public int CodigoBanco { get; set; }
    public int CodigoPlaza { get; set; }
    public decimal MontoRecaudacion { get; set; }
    public DateTime? FechaDeposito { get; set; }
    public DateTime? FechaVencimientoReal { get; set; }
    public int DiasRetencion { get; set; }
    public DateTime? FechaLiberacion { get; set; }
    public bool ChequeANombreEmpresa { get; set; }
}

// ==========================================
// DTOs para Verificación
// ==========================================

/// <summary>
/// DTO para verificación de documentos
/// </summary>
public class VerificacionDocumentoDto
{
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public int CantidadDocumentos { get; set; }
    public decimal MontoTotal { get; set; }
    public bool Verificado { get; set; }
    public bool Otorgado { get; set; }
    public bool TieneCaratula { get; set; }
}

/// <summary>
/// Filtro para consulta de verificación
/// </summary>
public class FiltroVerificacionDto
{
    public int TipoDocumento { get; set; } // 0=Factura, 1=Letra, 2=Cheque, 3=Otros
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public bool? Verificado { get; set; }
    public bool? Otorgado { get; set; }
    public bool? SoloCaratula { get; set; }
    public decimal? MontoMinimo1 { get; set; } // 0 - 9,999,999
    public decimal? MontoMaximo1 { get; set; }
    public decimal? MontoMinimo2 { get; set; } // 10,000,000 - 19,999,999
    public decimal? MontoMaximo2 { get; set; }
    public decimal? MontoMinimo3 { get; set; } // >= 20,000,000
    public decimal? MontoMaximo3 { get; set; }
}

/// <summary>
/// DTO para detalle de verificación
/// </summary>
public class DetalleVerificacionDto
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

// ==========================================
// DTOs para Caja
// ==========================================

/// <summary>
/// DTO para movimiento de caja
/// </summary>
public class MovimientoCajaDto
{
    public int NumeroMovimiento { get; set; }
    public DateTime? FechaMovimiento { get; set; }
    public int TipoDocumento { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public int NumeroOperacion { get; set; }
    public long RutCliente { get; set; }
    public string? NombreCliente { get; set; }
    public long RutDeudor { get; set; }
    public string? NombreDeudor { get; set; }
    public decimal Monto { get; set; }
    public int EstadoCaja { get; set; }
    public string? DescripcionEstadoCaja { get; set; }
    public int EstadoPago { get; set; }
    public string? UsuarioCaja { get; set; }
    public DateTime? FechaCreacion { get; set; }
}

/// <summary>
/// Filtro para consulta de caja
/// </summary>
public class FiltroCajaDto
{
    public DateTime? Fecha { get; set; }
    public int? TipoDocumento { get; set; }
    public int? EstadoCaja { get; set; } // 1=Actuales, 2=Retención, 3=Procesados
    public int? EstadoPago { get; set; } // 0=Todos, 1=Pendientes, 2=Rechazados, 3=Aprobados, 4=Eliminados
}

/// <summary>
/// Resumen de caja
/// </summary>
public class ResumenCajaDto
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

// ==========================================
// DTOs para Verificación Diaria
// ==========================================

/// <summary>
/// DTO para verificación diaria
/// </summary>
public class VerificacionDiariaDto
{
    public DateTime? FechaVerificacion { get; set; }
    public string? HoraVerificacion { get; set; }
    public int TipoDocumento { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public long RutCliente { get; set; }
    public string? RutClienteFormateado { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string? RutDeudorFormateado { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public int NumeroOperacion { get; set; }
    public long NumeroDocumento { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public int EstadoVerificacion { get; set; }
    public string? DescripcionEstadoVerificacion { get; set; }
    public string? Resultado { get; set; }
    public string? Observacion { get; set; }
    public string? UsuarioVerificacion { get; set; }

    // Campos adicionales del VB6
    public string? PersonaContacto { get; set; }      // Se conversó con
    public string? CargoPersona { get; set; }         // Cargo de la persona
    public string? FacturaRecibida { get; set; }      // S/N
    public string? FacturaIngresada { get; set; }     // S/N
    public string? AceptaNotificacion { get; set; }   // S/N
    public string? DocumentoCedido { get; set; }      // S/N
    public string? DepositoCliente { get; set; }      // S/N
    public string? EntregaInformacion { get; set; }   // S/N
    public string? TimbreDeudor { get; set; }         // S/N
}

/// <summary>
/// Filtro para verificación diaria
/// </summary>
public class FiltroVerificacionDiariaDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? TipoDocumento { get; set; }
    public int? EstadoVerificacion { get; set; }
    public int? NumeroOperacion { get; set; }
    public int? CodigoEjecutivo { get; set; }
    public int? CentroCosto { get; set; }
    public bool? SoloOtorgadas { get; set; }
}

/// <summary>
/// DTO para registrar una verificación
/// </summary>
public class RegistrarVerificacionDto
{
    public long RutCliente { get; set; }
    public long RutDeudor { get; set; }
    public int NumeroOperacion { get; set; }
    public long NumeroDocumento { get; set; }
    public int TipoDocumento { get; set; }
    public int EstadoVerificacion { get; set; }
    public string? Observacion { get; set; }
    public string? PersonaContacto { get; set; }
    public string? CargoPersona { get; set; }
    public string? FacturaRecibida { get; set; }
    public string? FacturaIngresada { get; set; }
    public string? AceptaNotificacion { get; set; }
    public string? DocumentoCedido { get; set; }
    public string? DepositoCliente { get; set; }
    public string? EntregaInformacion { get; set; }
    public string? TimbreDeudor { get; set; }
}

/// <summary>
/// Resumen de verificación diaria
/// </summary>
public class ResumenVerificacionDiariaDto
{
    public DateTime? Fecha { get; set; }
    public int TotalDocumentos { get; set; }
    public int DocumentosVerificados { get; set; }
    public int DocumentosPendientes { get; set; }
    public decimal MontoTotalVerificado { get; set; }
    public decimal MontoTotalPendiente { get; set; }
    public List<ResumenVerificacionPorEstadoDto> PorEstado { get; set; } = new();
}

/// <summary>
/// Resumen por estado de verificación diaria
/// </summary>
public class ResumenVerificacionPorEstadoDto
{
    public int EstadoVerificacion { get; set; }
    public string? DescripcionEstado { get; set; }
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}

// ==========================================
// DTOs para Documentos No Cedidos
// ==========================================

/// <summary>
/// DTO para documento no cedido
/// </summary>
public class DocumentoNoCedidoDto
{
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public long RutCliente { get; set; }
    public string? RutClienteFormateado { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string? RutDeudorFormateado { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public string? MotivoNoCesion { get; set; }
    public DateTime? FechaRegistro { get; set; }
}

/// <summary>
/// DTO para registrar un documento como no cedido
/// </summary>
public class RegistrarDocumentoNoCedidoDto
{
    public int TipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public int NumeroOperacion { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

/// <summary>
/// Filtro para documentos no cedidos
/// </summary>
public class FiltroDocumentoNoCedidoDto
{
    public long? RutCliente { get; set; }
    public long? RutDeudor { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? TipoDocumento { get; set; }
}

// ==========================================
// DTOs para Informe de Abonos
// ==========================================

/// <summary>
/// DTO para informe de abonos
/// </summary>
public class InformeAbonoDto
{
    public DateTime? FechaAbono { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal MontoAbono { get; set; }
    public decimal SaldoPendiente { get; set; }
    public int TipoPago { get; set; }
    public string? DescripcionTipoPago { get; set; }
    public string? NombreCobrador { get; set; }
}

/// <summary>
/// Filtro para informe de abonos
/// </summary>
public class FiltroInformeAbonoDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public long? RutCliente { get; set; }
    public long? RutDeudor { get; set; }
    public int? TipoDocumento { get; set; }
    public int? TipoPago { get; set; }
}

/// <summary>
/// Resumen de abonos
/// </summary>
public class ResumenAbonoDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int CantidadAbonos { get; set; }
    public decimal MontoTotalAbonos { get; set; }
    public decimal MontoTotalDocumentos { get; set; }
    public decimal PorcentajeRecuperacion { get; set; }
    public int CantidadPagosDeudor { get; set; }
    public decimal MontoPagosDeudor { get; set; }
    public int CantidadPagosCliente { get; set; }
    public decimal MontoPagosCliente { get; set; }
}

// ==========================================
// DTOs para Gastos de Cobranza
// ==========================================

/// <summary>
/// DTO para gasto de cobranza
/// </summary>
public class GastoCobranzaDto
{
    public int NumeroGasto { get; set; }
    public int NumeroPago { get; set; }
    public int TipoGasto { get; set; }
    public string? DescripcionGasto { get; set; }
    public decimal MontoGasto { get; set; }
    public DateTime? FechaGasto { get; set; }
}

/// <summary>
/// DTO para crear gasto de cobranza
/// </summary>
public class GastoCobranzaCreateDto
{
    public int NumeroPago { get; set; }
    public int TipoGasto { get; set; }
    public decimal MontoGasto { get; set; }
}

// ==========================================
// DTOs para Documento a Pagar (información completa)
// ==========================================

/// <summary>
/// DTO con información completa de documento para pago
/// </summary>
public class DocumentoPagarDto
{
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public long RutCliente { get; set; }
    public string? NombreCliente { get; set; }
    public long RutDeudor { get; set; }
    public string? NombreDeudor { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal SaldoPendiente { get; set; }
    public decimal SaldoFinanciado { get; set; }
    public decimal MontoAnticipo { get; set; }
    public decimal TasaNegocio { get; set; }
    public decimal TasaMora { get; set; }
    public decimal MontoMora { get; set; }
    public decimal DeudaNeta { get; set; }
    public decimal Abono { get; set; }
    public DateTime? FechaLiberacionDocumento { get; set; }
    public DateTime? FechaAbono { get; set; }
    public int EstadoDocumento { get; set; }
    public string? DescripcionEstado { get; set; }
    public bool DevolucionDiferenciaPrecio { get; set; }
}

/// <summary>
/// DTO para buscar documento a pagar
/// </summary>
public class BusquedaDocumentoPagarDto
{
    public int TipoPago { get; set; } // 1=Deudor, 2=Cliente
    public long? RutCliente { get; set; }
    public long? RutDeudor { get; set; }
    public int? TipoDocumento { get; set; }
    public long? NumeroDocumento { get; set; }
}

// ==========================================
// DTOs para Consulta Masiva
// ==========================================

/// <summary>
/// DTO para documento en consulta masiva
/// </summary>
public class DocumentoConsultaMasivaDto
{
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public long RutCliente { get; set; }
    public string? RutClienteFormateado { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string? RutDeudorFormateado { get; set; }
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

/// <summary>
/// Filtro para consulta masiva de documentos
/// </summary>
public class FiltroConsultaMasivaDto
{
    public long? RutCliente { get; set; }
    public long? RutDeudor { get; set; }
    public int? TipoDocumento { get; set; }
    public long? NumeroDocumento { get; set; }
    public int? NumeroOperacion { get; set; }
    public DateTime? FechaEmisionDesde { get; set; }
    public DateTime? FechaEmisionHasta { get; set; }
    public DateTime? FechaVencimientoDesde { get; set; }
    public DateTime? FechaVencimientoHasta { get; set; }
    public decimal? MontoMinimo { get; set; }
    public decimal? MontoMaximo { get; set; }
    public int? EstadoDocumento { get; set; }
    public int? EstadoCesion { get; set; } // 0=No Cedido, 1=Cedido
    public bool? SoloVencidos { get; set; }
    public bool? SoloProtestados { get; set; }
    public bool? SoloEnCobranza { get; set; }
    public int? CodigoEjecutivo { get; set; }
    public int? CentroCosto { get; set; }
}

/// <summary>
/// Resumen de consulta masiva
/// </summary>
public class ResumenConsultaMasivaDto
{
    public int TotalDocumentos { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal SaldoTotal { get; set; }
    public int DocumentosVigentes { get; set; }
    public decimal MontoVigente { get; set; }
    public int DocumentosVencidos { get; set; }
    public decimal MontoVencido { get; set; }
    public int DocumentosProtestados { get; set; }
    public decimal MontoProtestado { get; set; }
    public int DocumentosEnCobranza { get; set; }
    public decimal MontoEnCobranza { get; set; }
    public List<ResumenPorTipoDocumentoDto> PorTipoDocumento { get; set; } = new();
    public List<ResumenEstadoConsultaMasivaDto> PorEstado { get; set; } = new();
}

/// <summary>
/// Resumen por tipo de documento
/// </summary>
public class ResumenPorTipoDocumentoDto
{
    public int TipoDocumento { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
    public decimal Saldo { get; set; }
}

/// <summary>
/// Resumen por estado para consulta masiva
/// </summary>
public class ResumenEstadoConsultaMasivaDto
{
    public int Estado { get; set; }
    public string? DescripcionEstado { get; set; }
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}
