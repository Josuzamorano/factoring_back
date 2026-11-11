// =============================================
// DTOs para Módulo 3 - Carpeta Deudor
// Migrado de: F_CD1100.frm, F_CD1200.frm
// =============================================

namespace Factoring.Core.DTOs;

// =============================================
// DTOs de Deudor
// =============================================

public class DeudorDto
{
    public int RutCliente { get; set; }
    public int RutDeudor { get; set; }
    public string? DigitoVerificador { get; set; }
    public string RutFormateado { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? Direccion { get; set; }
    public int? CodigoComuna { get; set; }
    public string? NombreComuna { get; set; }
    public int EstadoDeudor { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
    public string? ContactoCobranza { get; set; }
    public decimal PorcentajeVentas { get; set; }
    public int Notificacion { get; set; }
    public decimal MontoOtorgado { get; set; }
    public decimal MontoOcupado { get; set; }
    public decimal MontoDisponible { get; set; }
    public int? CategoriaRiesgo { get; set; }
    public string? DescripcionCategoriaRiesgo { get; set; }
    public string? NombreCliente { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class DeudorListDto
{
    public int RutCliente { get; set; }
    public int RutDeudor { get; set; }
    public string RutFormateado { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? NombreCliente { get; set; }
    public decimal MontoOtorgado { get; set; }
    public decimal MontoOcupado { get; set; }
    public decimal MontoDisponible { get; set; }
    public int EstadoDeudor { get; set; }
    public bool Activo { get; set; }
}

public class DeudorCreateDto
{
    public int RutCliente { get; set; }
    public int RutDeudor { get; set; }
    public string? DigitoVerificador { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public int? CodigoComuna { get; set; }
    public string? ContactoCobranza { get; set; }
    public decimal PorcentajeVentas { get; set; }
    public int Notificacion { get; set; }
    public decimal MontoOtorgado { get; set; }
    public int? CategoriaRiesgo { get; set; }
    public string? GiroDeudor { get; set; }
}

public class DeudorUpdateDto
{
    public string? RazonSocial { get; set; }
    public string? Direccion { get; set; }
    public int? CodigoComuna { get; set; }
    public int EstadoDeudor { get; set; }
    public string? ContactoCobranza { get; set; }
    public decimal PorcentajeVentas { get; set; }
    public int Notificacion { get; set; }
    public decimal MontoOtorgado { get; set; }
    public int? CategoriaRiesgo { get; set; }
    public string? GiroDeudor { get; set; }
}

public class DeudorSearchDto
{
    public int? RutCliente { get; set; }
    public string? RutDeudor { get; set; }
    public string? RazonSocial { get; set; }
    public int? EstadoDeudor { get; set; }
    public int? CategoriaRiesgo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

// =============================================
// DTOs de Teléfono/Contacto Deudor
// =============================================

public class DeudorTelefonoDto
{
    public int RutCliente { get; set; }
    public int RutDeudor { get; set; }
    public int TipoComunicacion { get; set; }
    public string TipoComunicacionDescripcion { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string NumeroComunicacion { get; set; } = string.Empty;
    public int Estado { get; set; }
}

public class DeudorTelefonoCreateDto
{
    public int TipoComunicacion { get; set; }
    public string? Descripcion { get; set; }
    public string NumeroComunicacion { get; set; } = string.Empty;
}

// =============================================
// DTOs de Documento Deudor
// =============================================

public class DeudorDocumentoDto
{
    public int RutCliente { get; set; }
    public int RutDeudor { get; set; }
    public int NumeroDocumento { get; set; }
    public int TipoDocumento { get; set; }
    public string TipoDocumentoDescripcion { get; set; } = string.Empty;
    public string NombreDocumento { get; set; } = string.Empty;
    public string? RutaDocumento { get; set; }
    public DateTime? FechaDocumento { get; set; }
    public string? Observaciones { get; set; }
    public int Estado { get; set; }
}

public class DeudorDocumentoCreateDto
{
    public int TipoDocumento { get; set; }
    public string NombreDocumento { get; set; } = string.Empty;
    public string? RutaDocumento { get; set; }
    public DateTime? FechaDocumento { get; set; }
    public string? Observaciones { get; set; }
}

// =============================================
// DTOs de Grupos de Deudores
// =============================================

public class GrupoDeudorDto
{
    public long RutGrupo { get; set; }
    public string DigitoVerificador { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public decimal MontoLineaAprobada { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public int CantidadDeudores { get; set; }
    public decimal MontoOcupado { get; set; }
    public decimal MontoDisponible { get; set; }
}

public class CupoGrupoDto
{
    public long RutGrupo { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public decimal MontoLinea { get; set; }
    public decimal MontoOcupado { get; set; }
    public decimal MontoDisponible { get; set; }
    public decimal PorcentajeUtilizacion { get; set; }
}

public class CreateGrupoDeudorDto
{
    public long RutGrupo { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public decimal MontoLineaAprobada { get; set; }
    public DateTime? FechaVencimiento { get; set; }
}

public class UpdateGrupoDeudorDto
{
    public string RazonSocial { get; set; } = string.Empty;
    public decimal MontoLineaAprobada { get; set; }
    public DateTime? FechaVencimiento { get; set; }
}

public class DeudorGrupoDto
{
    public int Id { get; set; }
    public long RutGrupo { get; set; }
    public string NombreGrupo { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string DigitoVerificador { get; set; } = string.Empty;
    public string NombreDeudor { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }
    public string? Giro { get; set; }
    public DateTime? FechaCreacion { get; set; }
}

public class CreateDeudorGrupoDto
{
    public long RutDeudor { get; set; }
}
