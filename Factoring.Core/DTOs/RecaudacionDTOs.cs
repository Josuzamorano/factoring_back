namespace Factoring.Core.DTOs;

// =============================================
// DTOs para Gestión de Cobranza (F_GC1100)
// =============================================
public class GestionCobranzaDto
{
    public int Correlativo { get; set; }
    public long RutCliente { get; set; }
    public string? NombreCliente { get; set; }
    public long RutDeudor { get; set; }
    public string? NombreDeudor { get; set; }
    public int TipoDocumento { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public int NumeroOperacion { get; set; }
    public string CodigoCobrador { get; set; } = string.Empty;
    public string? NombreCobrador { get; set; }
    public DateTime? FechaGestion { get; set; }
    public string HoraGestion { get; set; } = string.Empty;
    public int EstadoCobranza { get; set; }
    public string? DescripcionEstadoCobranza { get; set; }
    public DateTime? FechaPago { get; set; }
    public string? HoraPago { get; set; }
    public string? DocumentoNecesario { get; set; }
    public string? Observacion { get; set; }
    public DateTime? FechaProximaGestion { get; set; }
    public string? HoraProximaGestion { get; set; }
    public string? CodigoRecaudador { get; set; }
}

public class GestionCobranzaCreateDto
{
    public long RutCliente { get; set; }
    public long RutDeudor { get; set; }
    public int TipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public int NumeroOperacion { get; set; }
    public string CodigoCobrador { get; set; } = string.Empty;
    public DateTime? FechaGestion { get; set; }
    public string HoraGestion { get; set; } = string.Empty;
    public int EstadoCobranza { get; set; }
    public DateTime? FechaPago { get; set; }
    public string? HoraPago { get; set; }
    public string? DocumentoNecesario { get; set; }
    public string? Observacion { get; set; }
    public DateTime? FechaProximaGestion { get; set; }
    public string? HoraProximaGestion { get; set; }
    public string? CodigoRecaudador { get; set; }
    public DateTime? FechaNotificacion { get; set; }
}

public class GestionCobranzaUpdateDto
{
    public int EstadoCobranza { get; set; }
    public DateTime? FechaPago { get; set; }
    public string? HoraPago { get; set; }
    public string? DocumentoNecesario { get; set; }
    public string? Observacion { get; set; }
    public DateTime? FechaProximaGestion { get; set; }
    public string? HoraProximaGestion { get; set; }
    public string? CodigoRecaudador { get; set; }
}

// =============================================
// DTOs para Cobrador
// =============================================
public class CobradorDto
{
    public string CodigoCobrador { get; set; } = string.Empty;
    public string NombreCobrador { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public int Estado { get; set; }
    public string? Sector { get; set; }
    public int TotalClientesAsignados { get; set; }
    public int TotalGestionesPendientes { get; set; }
}

public class CobradorSelectDto
{
    public string CodigoCobrador { get; set; } = string.Empty;
    public string NombreCobrador { get; set; } = string.Empty;
}

// =============================================
// DTOs para Documentos de Gestión
// =============================================
public class DocumentoGestionDto
{
    public long RutCliente { get; set; }
    public long RutDeudor { get; set; }
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public int EstadoDocumento { get; set; }
    public string? DescripcionEstadoDocumento { get; set; }
    public int? EstadoVerificacion { get; set; }
    public DateTime? FechaNotificacion { get; set; }
    public int? EstadoCobranza { get; set; }
    public string? DescripcionEstadoCobranza { get; set; }
    public bool TieneProblema { get; set; }
}

// =============================================
// DTOs para Deudor Gestión
// =============================================
public class DeudorGestionDto
{
    public long RutDeudor { get; set; }
    public string DigitoVerificador { get; set; } = string.Empty;
    public string NombreDeudor { get; set; } = string.Empty;
    public string? ContactoCobranza { get; set; }
    public string? DireccionPago { get; set; }
    public string? Telefono { get; set; }
    public string? Fax { get; set; }
    public int TotalDocumentosPendientes { get; set; }
}

public class DeudorGestionUpdateDto
{
    public long RutCliente { get; set; }
    public long RutDeudor { get; set; }
    public string? ContactoCobranza { get; set; }
    public string? DireccionPago { get; set; }
}

// =============================================
// DTOs para Cliente Cobrador
// =============================================
public class ClienteCobradorDto
{
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string CodigoCobrador { get; set; } = string.Empty;
    public string? NombreCobrador { get; set; }
    public string? NombreEjecutivo { get; set; }
    public int TotalDeudores { get; set; }
    public int TotalDocumentosPendientes { get; set; }
    public decimal MontoTotalPendiente { get; set; }
}

// =============================================
// DTOs para Estados de Cobranza (F_GC2600)
// =============================================
public class EstadoCobranzaDocumentoDto
{
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public int TipoDocumento { get; set; }
    public string DescripcionTipoDocumento { get; set; } = string.Empty;
    public long NumeroDocumento { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int EstadoCobranza { get; set; }
    public string DescripcionEstadoCobranza { get; set; } = string.Empty;
    public DateTime? FechaPago { get; set; }
    public decimal MontoDocumento { get; set; }
    public string? NombreBanco { get; set; }
    public string? NombreEjecutivo { get; set; }
    public string? NombreCobrador { get; set; }
    public string? DescripcionEstadoDocumento { get; set; }
    public DateTime? FechaObservacion { get; set; }
    public string? Observacion { get; set; }
}

// =============================================
// DTOs para Deuda Total Cliente (F_GC2200)
// =============================================
public class DeudaTotalClienteDto
{
    public long RutCliente { get; set; }
    public string DigitoVerificador { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime? FechaCalculo { get; set; }
    public decimal TotalFacturas { get; set; }
    public decimal TotalCheques { get; set; }
    public decimal TotalLetras { get; set; }
    public decimal TotalOtros { get; set; }
    public decimal CuentasPorPagar { get; set; }
    public decimal CuentasPorCobrar { get; set; }
    public decimal FacturasNoFactoring { get; set; }
    public decimal Excedentes { get; set; }
    public decimal DeudaNeta { get; set; }
    public List<DetalleDeudaResumenDto> DetalleDeuda { get; set; } = new();
    public decimal TotalDeudaNeta { get; set; }
}

public class DetalleDeudaResumenDto
{
    public string Codigo { get; set; } = string.Empty;  // Código tipo documento o especial (_CXP_, _FNC_, etc.)
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}

public class DetalleDeudaDocumentoDto
{
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public int NumeroOperacion { get; set; }
    public string TipoDocumentoCodigo { get; set; } = string.Empty;
    public long NumeroDocumento { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal MontoAnticipo { get; set; }
    public decimal InteresVencido { get; set; }
    public decimal InteresMora { get; set; }
    public decimal InteresNoDevengado { get; set; }
    public string EstadoDocumento { get; set; } = string.Empty;
    public decimal Abono { get; set; }
    public decimal DeudaNeta { get; set; }
}

// =============================================
// DTOs para Filtros y Consultas
// =============================================
public class FiltroGestionCobranzaDto
{
    public string? CodigoCobrador { get; set; }
    public long? RutCliente { get; set; }
    public long? RutDeudor { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? EstadoCobranza { get; set; }
    public int? TipoDocumento { get; set; }
}

public class FiltroEstadoCobranzaDto
{
    public DateTime? FechaDesde { get; set; } = DateTime.Today.AddYears(-1);
    public DateTime? FechaHasta { get; set; } = DateTime.Today;
    public long? RutCliente { get; set; }
    public int? TipoDocumento { get; set; }  // 0 = todos
    public bool ConObservaciones { get; set; } = true;
}

// =============================================
// DTOs para Resúmenes
// =============================================
public class ResumenGestionCobranzaDto
{
    public string CodigoCobrador { get; set; } = string.Empty;
    public string NombreCobrador { get; set; } = string.Empty;
    public DateTime? FechaConsulta { get; set; }
    public int TotalDeudores { get; set; }
    public int TotalDocumentosPendientes { get; set; }
    public decimal MontoTotalPendiente { get; set; }
    public int GestionesRealizadasHoy { get; set; }
    public int GestionesProgramadasHoy { get; set; }
}

public class ResumenEstadoCobranzaDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int TotalDocumentos { get; set; }
    public decimal MontoTotal { get; set; }
    public int TotalClientes { get; set; }
    public Dictionary<string, ResumenPorEstadoDto> PorEstado { get; set; } = new();
}

public class ResumenPorEstadoDto
{
    public int EstadoCobranza { get; set; }
    public string DescripcionEstado { get; set; } = string.Empty;
    public int CantidadDocumentos { get; set; }
    public decimal Monto { get; set; }
}

