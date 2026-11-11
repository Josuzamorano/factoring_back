namespace Factoring.Core.Entities;

/// <summary>
/// Entidad Cliente - Migrada de tabla CLI (VB6)
/// Representa la información principal de un cliente de factoring
/// </summary>
public class Cliente
{
    /// <summary>RUT del cliente (sin dígito verificador)</summary>
    public long RutCliente { get; set; }
    
    /// <summary>Dígito verificador del RUT</summary>
    public string? DigitoVerificador { get; set; }
    
    /// <summary>Tipo cliente (1=Natural, 2=Jurídico)</summary>
    public int TipoCliente { get; set; } = 2;
    
    /// <summary>Código sucursal</summary>
    public string? CodigoSucursal { get; set; }
    
    /// <summary>Código ejecutivo cobranza</summary>
    public string? CobradorTelefono { get; set; }
    
    /// <summary>Estado cobranza (días gestión)</summary>
    public int EstadoCobranza { get; set; }
    
    /// <summary>Razón social o nombre completo</summary>
    public string RazonSocial { get; set; } = string.Empty;
    
    /// <summary>Apellido paterno (persona natural)</summary>
    public string? ApellidoPaterno { get; set; }
    
    /// <summary>Apellido materno (persona natural)</summary>
    public string? ApellidoMaterno { get; set; }
    
    /// <summary>Sexo (1=Masculino, 2=Femenino)</summary>
    public int Sexo { get; set; } = 1;
    
    /// <summary>Dirección comercial</summary>
    public string? Direccion { get; set; }
    
    /// <summary>Código comuna</summary>
    public int? CodigoComuna { get; set; }
    
    /// <summary>Código giro comercial</summary>
    public int? CodigoGiro { get; set; }
    
    /// <summary>Código postal</summary>
    public string? CodigoPostal { get; set; }
    
    /// <summary>Correo electrónico</summary>
    public string? CorreoElectronico { get; set; }
    
    /// <summary>Fecha inicio de relación comercial</summary>
    public DateTime FechaInicio { get; set; } = DateTime.Now;
    
    /// <summary>Estado cliente (1=Activo, 0=Inactivo)</summary>
    public int EstadoCliente { get; set; } = 1;
    
    /// <summary>Código ejecutivo actual</summary>
    public string? CodigoEjecutivo { get; set; }
    
    /// <summary>Observaciones</summary>
    public string? Observaciones { get; set; }
    
    /// <summary>Código ejecutivo inicial</summary>
    public string? CodigoEjecutivoInicial { get; set; }
    
    /// <summary>Comisión prórroga</summary>
    public int ComisionProrroga { get; set; }
    
    /// <summary>Comisión protesto</summary>
    public int ComisionProtesto { get; set; }
    
    /// <summary>Código actividad económica</summary>
    public int? CodigoActividad { get; set; }
    
    /// <summary>Línea banco aprobada</summary>
    public decimal LineaBanco { get; set; }
    
    /// <summary>Línea banco utilizada</summary>
    public decimal UtilizadoBanco { get; set; }
    
    /// <summary>Sucursal bancaria</summary>
    public string? SucursalBanco { get; set; }
    
    /// <summary>Ejecutivo bancario</summary>
    public string? EjecutivoBanco { get; set; }
    
    /// <summary>Fecha de creación</summary>
    public DateTime FechaCreacion { get; set; }
    
    /// <summary>Usuario creación</summary>
    public string? UsuarioCreacion { get; set; }
    
    /// <summary>Fecha modificación</summary>
    public DateTime? FechaModificacion { get; set; }
    
    /// <summary>Usuario modificación</summary>
    public string? UsuarioModificacion { get; set; }
    
    // Propiedades calculadas
    public bool Activo => EstadoCliente == 1;
    public string RutFormateado => $"{RutCliente:N0}-{DigitoVerificador}".Replace(",", ".");
    public string NombreCompleto => TipoCliente == 1 
        ? $"{RazonSocial} {ApellidoPaterno} {ApellidoMaterno}".Trim() 
        : RazonSocial;
}

/// <summary>
/// Teléfono del Cliente - Migrada de tabla CLI_TLF
/// </summary>
public class ClienteTelefono
{
    public long RutCliente { get; set; }
    public int TipoTelefono { get; set; }
    public string NumeroTelefono { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public int Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// Contacto del Cliente - Migrada de tabla CLI_CON
/// </summary>
public class ClienteContacto
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
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// Cuenta Bancaria del Cliente - Migrada de tabla CLI_BCO
/// </summary>
public class ClienteBanco
{
    public long RutCliente { get; set; }
    public int CodigoBanco { get; set; }
    public int TipoCuenta { get; set; }
    public string NumeroCuenta { get; set; } = string.Empty;
    public int Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    
    // Propiedad de navegación
    public string? NombreBanco { get; set; }
}

/// <summary>
/// Cobrador/Ejecutivo de Cobranza - Migrada de tabla COB
/// </summary>
public class Cobrador
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public int Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
}
