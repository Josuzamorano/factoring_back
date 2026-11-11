using Factoring.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Factoring.Infrastructure.Services.BancoIntegracionService;

namespace Factoring.API.Controllers;

/// <summary>
/// Controlador para integración bancaria
/// Generación de archivos planos para transferencias TEF
/// </summary>
[ApiController]
[Route("api/bancos/integracion")]
[Authorize]
[SwaggerTag("Integración Bancaria - Generación de archivos planos para transferencias")]
public class BancosIntegracionController : ControllerBase
{
    private readonly BancoIntegracionService _bancoService;
    private readonly ILogger<BancosIntegracionController> _logger;

    public BancosIntegracionController(
        BancoIntegracionService bancoService,
        ILogger<BancosIntegracionController> logger)
    {
        _bancoService = bancoService;
        _logger = logger;
    }

    #region DTOs

    /// <summary>
    /// DTO para solicitud de generación de archivo
    /// </summary>
    public class GenerarArchivoRequest
    {
        /// <summary>Código del banco destino (012=Estado, 001=Chile, 037=Santander, 016=BCI)</summary>
        public string CodigoBanco { get; set; } = string.Empty;
        /// <summary>RUT de la empresa pagadora</summary>
        public string RutEmpresa { get; set; } = string.Empty;
        /// <summary>Número de cuenta origen</summary>
        public string CuentaOrigen { get; set; } = string.Empty;
        /// <summary>Lista de transferencias a incluir</summary>
        public List<TransferenciaDto> Transferencias { get; set; } = new();
    }

    /// <summary>
    /// DTO para una transferencia individual
    /// </summary>
    public class TransferenciaDto
    {
        /// <summary>RUT del beneficiario (sin DV)</summary>
        public long RutBeneficiario { get; set; }
        /// <summary>Dígito verificador</summary>
        public string DvBeneficiario { get; set; } = string.Empty;
        /// <summary>Nombre o razón social del beneficiario</summary>
        public string NombreBeneficiario { get; set; } = string.Empty;
        /// <summary>Código del banco del beneficiario</summary>
        public string CodigoBancoDestino { get; set; } = string.Empty;
        /// <summary>Tipo de cuenta (CC=Corriente, AH=Ahorro, VIS=Vista)</summary>
        public string TipoCuenta { get; set; } = "CC";
        /// <summary>Número de cuenta del beneficiario</summary>
        public string NumeroCuenta { get; set; } = string.Empty;
        /// <summary>Monto a transferir</summary>
        public decimal Monto { get; set; }
        /// <summary>Email del beneficiario para notificación</summary>
        public string? Email { get; set; }
        /// <summary>Glosa o descripción de la transferencia</summary>
        public string? Glosa { get; set; }
        /// <summary>Número de operación de referencia</summary>
        public int? NumeroOperacion { get; set; }
    }

    #endregion

    /// <summary>
    /// Lista los bancos soportados para generación de archivos
    /// </summary>
    [HttpGet("bancos-soportados")]
    [SwaggerOperation(
        Summary = "Listar bancos soportados",
        Description = "Retorna la lista de bancos para los cuales se puede generar archivos de transferencia")]
    [SwaggerResponse(200, "Lista de bancos")]
    public ActionResult<object> ObtenerBancosSoportados()
    {
        var bancos = _bancoService.ObtenerBancosSoportados()
            .Select(b => new
            {
                codigo = b.Codigo,
                nombre = b.Nombre,
                soportado = b.Soportado
            });

        return Ok(bancos);
    }

    /// <summary>
    /// Genera un archivo plano de transferencias para un banco específico
    /// </summary>
    [HttpPost("generar-archivo")]
    [SwaggerOperation(
        Summary = "Generar archivo de transferencias",
        Description = "Genera un archivo plano en el formato requerido por el banco para carga masiva de transferencias")]
    [SwaggerResponse(200, "Archivo generado exitosamente")]
    [SwaggerResponse(400, "Error en los datos")]
    public ActionResult GenerarArchivo([FromBody] GenerarArchivoRequest request)
    {
        if (string.IsNullOrEmpty(request.CodigoBanco))
        {
            return BadRequest(new { mensaje = "Código de banco es requerido" });
        }

        if (string.IsNullOrEmpty(request.RutEmpresa))
        {
            return BadRequest(new { mensaje = "RUT de empresa es requerido" });
        }

        if (!request.Transferencias.Any())
        {
            return BadRequest(new { mensaje = "Debe incluir al menos una transferencia" });
        }

        // Validar transferencias
        foreach (var t in request.Transferencias)
        {
            if (t.Monto <= 0)
            {
                return BadRequest(new { mensaje = $"Monto inválido para beneficiario {t.RutBeneficiario}" });
            }
        }

        var transferencias = request.Transferencias.Select((t, i) => new TransferenciaBancaria
        {
            Secuencia = i + 1,
            RutBeneficiario = t.RutBeneficiario,
            DvBeneficiario = t.DvBeneficiario,
            NombreBeneficiario = t.NombreBeneficiario,
            CodigoBanco = t.CodigoBancoDestino,
            TipoCuenta = t.TipoCuenta,
            NumeroCuenta = t.NumeroCuenta,
            Monto = t.Monto,
            Email = t.Email ?? "",
            Glosa = t.Glosa ?? "",
            NumeroOperacion = t.NumeroOperacion
        }).ToList();

        var result = _bancoService.GenerarArchivo(
            request.CodigoBanco,
            transferencias,
            request.RutEmpresa,
            request.CuentaOrigen);

        if (!result.Exito)
        {
            return BadRequest(new { mensaje = result.Mensaje });
        }

        _logger.LogInformation(
            "Archivo generado: {NombreArchivo}, {TotalRegistros} transferencias, ${MontoTotal:N0}",
            result.NombreArchivo, result.TotalRegistros, result.MontoTotal);

        return Ok(new
        {
            exito = true,
            nombreArchivo = result.NombreArchivo,
            totalRegistros = result.TotalRegistros,
            montoTotal = result.MontoTotal,
            contenido = result.Contenido
        });
    }

    /// <summary>
    /// Genera y descarga el archivo plano como un archivo de texto
    /// </summary>
    [HttpPost("descargar-archivo")]
    [SwaggerOperation(
        Summary = "Descargar archivo de transferencias",
        Description = "Genera y descarga el archivo plano en formato de texto")]
    [SwaggerResponse(200, "Archivo descargado")]
    [SwaggerResponse(400, "Error en los datos")]
    public ActionResult DescargarArchivo([FromBody] GenerarArchivoRequest request)
    {
        if (string.IsNullOrEmpty(request.CodigoBanco))
        {
            return BadRequest(new { mensaje = "Código de banco es requerido" });
        }

        if (!request.Transferencias.Any())
        {
            return BadRequest(new { mensaje = "Debe incluir al menos una transferencia" });
        }

        var transferencias = request.Transferencias.Select((t, i) => new TransferenciaBancaria
        {
            Secuencia = i + 1,
            RutBeneficiario = t.RutBeneficiario,
            DvBeneficiario = t.DvBeneficiario,
            NombreBeneficiario = t.NombreBeneficiario,
            CodigoBanco = t.CodigoBancoDestino,
            TipoCuenta = t.TipoCuenta,
            NumeroCuenta = t.NumeroCuenta,
            Monto = t.Monto,
            Email = t.Email ?? "",
            Glosa = t.Glosa ?? "",
            NumeroOperacion = t.NumeroOperacion
        }).ToList();

        var result = _bancoService.GenerarArchivo(
            request.CodigoBanco,
            transferencias,
            request.RutEmpresa,
            request.CuentaOrigen);

        if (!result.Exito || result.ContenidoBytes == null)
        {
            return BadRequest(new { mensaje = result.Mensaje ?? "Error generando archivo" });
        }

        var contentType = result.NombreArchivo?.EndsWith(".csv") == true
            ? "text/csv"
            : "text/plain";

        return File(result.ContenidoBytes, contentType, result.NombreArchivo);
    }

    /// <summary>
    /// Genera archivo de pagos desde operaciones del sistema
    /// </summary>
    [HttpPost("generar-desde-operaciones")]
    [SwaggerOperation(
        Summary = "Generar archivo desde operaciones",
        Description = "Genera archivo de transferencias a partir de operaciones otorgadas pendientes de pago")]
    [SwaggerResponse(200, "Archivo generado")]
    public ActionResult GenerarDesdeOperaciones(
        [FromQuery] string codigoBanco,
        [FromQuery] string rutEmpresa,
        [FromQuery] string cuentaOrigen,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        // Aquí se integraría con OperacionService para obtener las operaciones
        // Por ahora retorna ejemplo
        return Ok(new
        {
            mensaje = "Funcionalidad disponible. Use /generar-archivo con la lista de transferencias.",
            ejemplo = new
            {
                codigoBanco = "012",
                rutEmpresa = "76123456-K",
                cuentaOrigen = "1234567890",
                transferencias = new[]
                {
                    new
                    {
                        rutBeneficiario = 12345678,
                        dvBeneficiario = "9",
                        nombreBeneficiario = "CLIENTE EJEMPLO",
                        codigoBancoDestino = "001",
                        tipoCuenta = "CC",
                        numeroCuenta = "0012345678",
                        monto = 1500000,
                        glosa = "PAGO FACTORING OP-001"
                    }
                }
            }
        });
    }

    /// <summary>
    /// Parsea una cartola bancaria
    /// </summary>
    [HttpPost("parsear-cartola")]
    [SwaggerOperation(
        Summary = "Parsear cartola bancaria",
        Description = "Lee y extrae los movimientos de una cartola bancaria en formato CSV")]
    [SwaggerResponse(200, "Movimientos extraídos")]
    public ActionResult ParsearCartola(
        [FromBody] string contenido,
        [FromQuery] string separador = ";")
    {
        if (string.IsNullOrWhiteSpace(contenido))
        {
            return BadRequest(new { mensaje = "Contenido de cartola es requerido" });
        }

        var movimientos = _bancoService.LeerCartolaEstandar(contenido, separador);

        return Ok(new
        {
            totalMovimientos = movimientos.Count,
            totalCargos = movimientos.Sum(m => m.Cargo),
            totalAbonos = movimientos.Sum(m => m.Abono),
            movimientos
        });
    }
}
