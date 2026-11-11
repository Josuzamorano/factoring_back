namespace Factoring.Core.DTOs;

// =============================================
// DTOs para Cliente - Migrado de F_CC1700.frm
// =============================================

/// <summary>
/// DTO para lectura de Cliente
/// </summary>
public class ClienteDto
{
    public long RutCliente { get; set; }
    public string? DigitoVerificador { get; set; }
    public string RutFormateado { get; set; } = string.Empty;
    public int TipoCliente { get; set; }
    public string TipoClienteDescripcion => TipoCliente == 1 ? "Natural" : "Jurídico";
    public string RazonSocial { get; set; } = string.Empty;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public int? CodigoComuna { get; set; }
    public string? NombreComuna { get; set; }
    public int? CodigoGiro { get; set; }
    public string? DescripcionGiro { get; set; }
    public string? CodigoPostal { get; set; }
    public string? CorreoElectronico { get; set; }
    public DateTime FechaInicio { get; set; }
    public int EstadoCliente { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
    public string? CodigoEjecutivo { get; set; }
    public string? NombreEjecutivo { get; set; }
    public string? Observaciones { get; set; }
    public string? CodigoSucursal { get; set; }
    public string? NombreSucursal { get; set; }
    public int? CodigoActividad { get; set; }
    
    // Línea bancaria
    public decimal LineaBanco { get; set; }
    public decimal UtilizadoBanco { get; set; }
    public decimal DisponibleBanco => LineaBanco - UtilizadoBanco;
    
    public bool Activo => EstadoCliente == 1;
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// DTO para crear un nuevo Cliente
/// </summary>
public class ClienteCreateDto
{
    public long RutCliente { get; set; }
    public string? DigitoVerificador { get; set; }
    public int TipoCliente { get; set; } = 2;
    public string RazonSocial { get; set; } = string.Empty;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public int Sexo { get; set; } = 1;
    public string? Direccion { get; set; }
    public int? CodigoComuna { get; set; }
    public int? CodigoGiro { get; set; }
    public string? CodigoPostal { get; set; }
    public string? CorreoElectronico { get; set; }
    public string? CodigoEjecutivo { get; set; }
    public string? CodigoSucursal { get; set; }
    public string? Observaciones { get; set; }
    public int? CodigoActividad { get; set; }
}

/// <summary>
/// DTO para actualizar un Cliente existente
/// </summary>
public class ClienteUpdateDto
{
    public int TipoCliente { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public int Sexo { get; set; }
    public string? Direccion { get; set; }
    public int? CodigoComuna { get; set; }
    public int? CodigoGiro { get; set; }
    public string? CodigoPostal { get; set; }
    public string? CorreoElectronico { get; set; }
    public string? CodigoEjecutivo { get; set; }
    public string? CodigoSucursal { get; set; }
    public string? Observaciones { get; set; }
    public int? CodigoActividad { get; set; }
    public int EstadoCliente { get; set; } = 1;
}

/// <summary>
/// DTO para búsqueda de Clientes
/// </summary>
public class ClienteSearchDto
{
    public string? Rut { get; set; }
    public string? RazonSocial { get; set; }
    public string? CodigoEjecutivo { get; set; }
    public int? TipoCliente { get; set; }
    public int? EstadoCliente { get; set; }
    public int? CodigoComuna { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO para listado resumido de Clientes (dropdowns, grids)
/// </summary>
public class ClienteListDto
{
    public long RutCliente { get; set; }
    public string RutFormateado { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? NombreEjecutivo { get; set; }
    public int EstadoCliente { get; set; }
    public bool Activo => EstadoCliente == 1;
}

// =============================================
// DTOs para Teléfonos del Cliente
// =============================================

public class ClienteTelefonoDto
{
    public long RutCliente { get; set; }
    public int TipoTelefono { get; set; }
    public string TipoTelefonoDescripcion => TipoTelefono switch
    {
        1 => "Fijo",
        2 => "Celular",
        3 => "Fax",
        _ => "Otro"
    };
    public string NumeroTelefono { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public int Estado { get; set; }
}

public class ClienteTelefonoCreateDto
{
    public int TipoTelefono { get; set; }
    public string NumeroTelefono { get; set; } = string.Empty;
    public string? Observacion { get; set; }
}

// =============================================
// DTOs para Contactos del Cliente
// =============================================

public class ClienteContactoDto
{
    public long RutCliente { get; set; }
    public int NumeroContacto { get; set; }
    public string NombreContacto { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }
    public string? Observaciones { get; set; }
    public int Estado { get; set; }
}

public class ClienteContactoCreateDto
{
    public string NombreContacto { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }
    public string? Observaciones { get; set; }
}

public class ClienteContactoUpdateDto
{
    public string NombreContacto { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }
    public string? Observaciones { get; set; }
    public int Estado { get; set; }
}

// =============================================
// DTOs para Cuentas Bancarias del Cliente
// =============================================

public class ClienteBancoDto
{
    public long RutCliente { get; set; }
    public int CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
    public int TipoCuenta { get; set; }
    public string TipoCuentaDescripcion => TipoCuenta switch
    {
        1 => "Cuenta Corriente",
        2 => "Cuenta Ahorro",
        3 => "Cuenta Vista",
        _ => "Otra"
    };
    public string NumeroCuenta { get; set; } = string.Empty;
    public int Estado { get; set; }
}

public class ClienteBancoCreateDto
{
    public int CodigoBanco { get; set; }
    public int TipoCuenta { get; set; }
    public string NumeroCuenta { get; set; } = string.Empty;
}

// =============================================
// DTOs para Organigrama del Cliente
// =============================================

public class ClienteOrganigramaDto
{
    public long RutCliente { get; set; }
    public int NumeroOrganigrama { get; set; }
    public string NombrePersona { get; set; } = string.Empty;
    public long? RutPersona { get; set; }
    public string? DigitoVerificador { get; set; }
    public string RutFormateado { get; set; } = string.Empty;
    public int? CodigoCargo { get; set; }
    public string? DescripcionCargo { get; set; }
    public bool AtributoFirma { get; set; }
    public int Estado { get; set; }
    public bool Activo { get; set; }
}

public class ClienteOrganigramaCreateDto
{
    public string NombrePersona { get; set; } = string.Empty;
    public long? RutPersona { get; set; }
    public string? DigitoVerificador { get; set; }
    public int? CodigoCargo { get; set; }
    public bool AtributoFirma { get; set; }
}

public class ClienteOrganigramaupdateDto
{
    public string NombrePersona { get; set; } = string.Empty;
    public long? RutPersona { get; set; }
    public string? DigitoVerificador { get; set; }
    public int? CodigoCargo { get; set; }
    public bool AtributoFirma { get; set; }
    public int Estado { get; set; }
}

// =============================================
// DTOs para Empresas Relacionadas del Cliente
// =============================================

public class ClienteEmpresaDto
{
    public long RutCliente { get; set; }
    public long RutEmpresa { get; set; }
    public string? DigitoVerificador { get; set; }
    public string RutFormateado { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? FechaSituacion { get; set; }
    public decimal MontoDeudaDirecta { get; set; }
    public decimal MontoDeudaIndirecta { get; set; }
    public bool SituacionDicom { get; set; }
    public string EstadoDicom { get; set; } = string.Empty;
    public int Estado { get; set; }
    public bool Activo { get; set; }
}

public class ClienteEmpresaCreateDto
{
    public long RutEmpresa { get; set; }
    public string? DigitoVerificador { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string? FechaSituacion { get; set; }
    public decimal MontoDeudaDirecta { get; set; }
    public decimal MontoDeudaIndirecta { get; set; }
    public bool SituacionDicom { get; set; }
}

public class ClienteEmpresaupdateDto
{
    public string RazonSocial { get; set; } = string.Empty;
    public string? FechaSituacion { get; set; }
    public decimal MontoDeudaDirecta { get; set; }
    public decimal MontoDeudaIndirecta { get; set; }
    public bool SituacionDicom { get; set; }
    public int Estado { get; set; }
}

// =============================================
// DTO Paginación
// =============================================

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalItems { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
