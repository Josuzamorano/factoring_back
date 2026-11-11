namespace Factoring.Core.DTOs;

// =============================================
// DTOs para Giro
// =============================================
public class GiroDto
{
    public int NumeroGiro { get; set; }
    public long RutCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int NumeroOperacion { get; set; }
    public int TipoGiro { get; set; }
    public string? DescripcionTipoGiro { get; set; }
    public DateTime? FechaGiro { get; set; }
    public decimal MontoGiro { get; set; }
    public string? CuentaCorriente { get; set; }
    public int? CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
    public string? Beneficiario { get; set; }
    public int Estado { get; set; }
    public string? DescripcionEstado { get; set; }
    public int? FormaCancelacion { get; set; }
    public string? UsuarioCreacion { get; set; }
    public DateTime? FechaCreacion { get; set; }
}

public class GiroCreateDto
{
    public long RutCliente { get; set; }
    public int NumeroOperacion { get; set; }
    public int TipoGiro { get; set; } = 1;
    public DateTime? FechaGiro { get; set; } = DateTime.Today;
    public decimal MontoGiro { get; set; }
    public string? CuentaCorriente { get; set; }
    public int? CodigoBanco { get; set; }
    public string? Beneficiario { get; set; }
    public int? FormaCancelacion { get; set; }
}

public class GiroUpdateDto
{
    public int TipoGiro { get; set; }
    public decimal MontoGiro { get; set; }
    public string? CuentaCorriente { get; set; }
    public int? CodigoBanco { get; set; }
    public string? Beneficiario { get; set; }
    public int Estado { get; set; }
    public int? FormaCancelacion { get; set; }
}

// =============================================
// DTOs para Movimiento
// =============================================
public class MovimientoDto
{
    public int NumeroMovimiento { get; set; }
    public DateTime? FechaMovimiento { get; set; }
    public int TipoMovimiento { get; set; }
    public string? DescripcionTipoMovimiento { get; set; }
    public long? RutCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int? NumeroOperacion { get; set; }
    public int? NumeroGiro { get; set; }
    public decimal Monto { get; set; }
    public string? Glosa { get; set; }
    public int? CodigoBanco { get; set; }
    public string? NombreBanco { get; set; }
    public string? CuentaBanco { get; set; }
    public int Estado { get; set; }
    public string? UsuarioCreacion { get; set; }
    public DateTime? FechaCreacion { get; set; }
}

public class MovimientoCreateDto
{
    public DateTime? FechaMovimiento { get; set; } = DateTime.Today;
    public int TipoMovimiento { get; set; }
    public long? RutCliente { get; set; }
    public int? NumeroOperacion { get; set; }
    public int? NumeroGiro { get; set; }
    public decimal Monto { get; set; }
    public string? Glosa { get; set; }
    public int? CodigoBanco { get; set; }
    public string? CuentaBanco { get; set; }
}

public class MovimientoUpdateDto
{
    public int TipoMovimiento { get; set; }
    public decimal Monto { get; set; }
    public string? Glosa { get; set; }
    public int? CodigoBanco { get; set; }
    public string? CuentaBanco { get; set; }
    public int Estado { get; set; }
}

// =============================================
// DTOs para Resúmenes/Consultas
// =============================================
public class ResumenGirosDto
{
    public DateTime? Fecha { get; set; }
    public int TotalGiros { get; set; }
    public decimal MontoTotal { get; set; }
    public int GirosEmitidos { get; set; }
    public int GirosPendientes { get; set; }
    public int GirosAnulados { get; set; }
}

public class ResumenMovimientosDto
{
    public DateTime? Fecha { get; set; }
    public int TotalMovimientos { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal Saldo { get; set; }
}

public class FiltroTesoreriaDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public long? RutCliente { get; set; }
    public int? TipoGiro { get; set; }
    public int? TipoMovimiento { get; set; }
    public int? Estado { get; set; }
    public int? CodigoBanco { get; set; }
}

// =============================================
// DTOs para Arqueo (F_TS2100)
// =============================================
public class ArqueoDocumentoDto
{
    public string TipoDocumento { get; set; } = string.Empty;
    public int NumeroOperacion { get; set; }
    public long NumeroDocumento { get; set; }
    public long RutDeudor { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public int EstadoDocumento { get; set; }
    public long RutCliente { get; set; }
    public string? NombreCliente { get; set; }
    public string? NombreDeudor { get; set; }
    public string? CodigoBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? NombreBanco { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
    public string? DescripcionEstado { get; set; }
}

public class FiltroArqueoDto
{
    public DateTime? FechaDesde { get; set; } = DateTime.Today.AddMonths(-1);
    public DateTime? FechaHasta { get; set; } = DateTime.Today;
    public long? RutCliente { get; set; }
    public string? TipoDocumento { get; set; }  // FAO, CHO, LEO o null para todos
    public int? Estado { get; set; }            // 1=Vigente, 2=Prorrogado, 3=Renovado, 4=Cancelado, 5=Moroso
    public bool SoloRetencion { get; set; }     // true = Retención, false = Vigentes
}

public class ResumenArqueoDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int TotalDocumentos { get; set; }
    public decimal MontoTotal { get; set; }
    public int TotalFacturas { get; set; }
    public decimal MontoFacturas { get; set; }
    public int TotalCheques { get; set; }
    public decimal MontoCheques { get; set; }
    public int TotalLetras { get; set; }
    public decimal MontoLetras { get; set; }
    public int DocumentosVigentes { get; set; }
    public int DocumentosMorosos { get; set; }
    public int DocumentosCancelados { get; set; }
}
