namespace Factoring.Core.DTOs;

// =============================================
// DTOs para Banco
// =============================================
public class BancoDto
{
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string PermiteDeposito { get; set; } = "N";
    public string? CuentaCorriente { get; set; }
    public int? CuentaContable { get; set; }
    public string TransferenciaElectronica { get; set; } = "N";
    public bool Activo => Estado == 0;
    public int Estado { get; set; }
}

public class BancoCreateDto
{
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string PermiteDeposito { get; set; } = "N";
    public string? CuentaCorriente { get; set; }
    public int? CuentaContable { get; set; }
    public string TransferenciaElectronica { get; set; } = "N";
}

public class BancoUpdateDto
{
    public string Descripcion { get; set; } = string.Empty;
    public string PermiteDeposito { get; set; } = "N";
    public string? CuentaCorriente { get; set; }
    public int? CuentaContable { get; set; }
    public string TransferenciaElectronica { get; set; } = "N";
    public int Estado { get; set; } = 0;
}

// =============================================
// DTOs para Plaza
// =============================================
public class PlazaDto
{
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int DiasCanje { get; set; }
    public string PlazaFuera { get; set; } = "N";
    public bool Activo => Estado == 0;
    public int Estado { get; set; }
}

public class PlazaCreateDto
{
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int DiasCanje { get; set; } = 0;
    public string PlazaFuera { get; set; } = "N";
}

public class PlazaUpdateDto
{
    public string Descripcion { get; set; } = string.Empty;
    public int DiasCanje { get; set; }
    public string PlazaFuera { get; set; } = "N";
    public int Estado { get; set; } = 0;
}

// =============================================
// DTOs para Sucursal
// =============================================
public class SucursalDto
{
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool Activo => Estado == 0;
    public int Estado { get; set; }
}

public class SucursalCreateDto
{
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
}

public class SucursalUpdateDto
{
    public string Descripcion { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public int Estado { get; set; } = 0;
}

// =============================================
// DTOs para Ejecutivo
// =============================================
public class EjecutivoDto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Rut { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public int? CodigoSucursal { get; set; }
    public string? NombreSucursal { get; set; }
    public bool Activo => Estado == 0;
    public int Estado { get; set; }
}

public class EjecutivoCreateDto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Rut { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public int? CodigoSucursal { get; set; }
}

public class EjecutivoUpdateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Rut { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public int? CodigoSucursal { get; set; }
    public int Estado { get; set; } = 0;
}

// =============================================
// DTOs para Comuna
// =============================================
public class ComunaDto
{
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int? CodigoRegion { get; set; }
    public string? NombreRegion { get; set; }
    public bool Activo => Estado == 0;
    public int Estado { get; set; }
}

// =============================================
// DTOs para Region
// =============================================
public class RegionDto
{
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activo => Estado == 0;
    public int Estado { get; set; }
    public List<ComunaDto>? Comunas { get; set; }
}

// =============================================
// DTOs para Gasto
// =============================================
public class GastoDto
{
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int TipoGasto { get; set; }
    public string TipoGastoDescripcion => TipoGasto switch
    {
        1 => "Comisión",
        2 => "Gasto Fijo",
        3 => "Impuesto",
        _ => "Otro"
    };
    public int TipoCalculo { get; set; }
    public string TipoCalculoDescripcion => TipoCalculo switch
    {
        0 => "Monto Fijo",
        1 => "Porcentaje",
        _ => "Otro"
    };
    public decimal Valor { get; set; }
    public int? CuentaContable { get; set; }
    public bool Activo => Estado == 0;
    public int Estado { get; set; }
}

public class GastoCreateDto
{
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int TipoGasto { get; set; }
    public int TipoCalculo { get; set; }
    public decimal Valor { get; set; }
    public int? CuentaContable { get; set; }
}

public class GastoUpdateDto
{
    public string Descripcion { get; set; } = string.Empty;
    public int TipoGasto { get; set; }
    public int TipoCalculo { get; set; }
    public decimal Valor { get; set; }
    public int? CuentaContable { get; set; }
    public int Estado { get; set; } = 0;
}

// =============================================
// DTOs para Parametro
// =============================================
public class ParametroDto
{
    public int CodigoTabla { get; set; }
    public int CodigoParametro { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? CodigoAuxiliar { get; set; }
    public decimal? ValorNumerico { get; set; }
    public string? ValorTexto { get; set; }
    public bool Activo => Estado == 0;
    public int Estado { get; set; }
}

// =============================================
// DTO genérico para listas desplegables
// =============================================
public class SelectItemDto
{
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
