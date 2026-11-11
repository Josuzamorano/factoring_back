using Microsoft.AspNetCore.Mvc;
using Factoring.Application.Services;
using Factoring.API.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Factoring.API.Controllers;

/// <summary>
/// Controlador para generacion de documentos PDF
/// Genera: Liquidacion de Operacion, Pagare, Estado de Cuenta
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Generacion de documentos PDF para operaciones y clientes")]
public class DocumentosController : ControllerBase
{
    private readonly PdfService _pdfService;
    private readonly PdfGeneratorService _pdfGenerator;

    public DocumentosController(PdfService pdfService, PdfGeneratorService pdfGenerator)
    {
        _pdfService = pdfService;
        _pdfGenerator = pdfGenerator;
    }

    // ==========================================
    // LIQUIDACION DE OPERACION
    // ==========================================

    /// <summary>
    /// Obtiene datos de liquidacion de operacion (JSON)
    /// </summary>
    /// <remarks>
    /// Retorna los datos estructurados de la liquidacion para una operacion.
    /// Incluye: datos cliente, documentos, calculo de intereses, gastos y giro.
    /// Usar este endpoint para previsualizar antes de generar PDF.
    /// </remarks>
    /// <param name="numeroOperacion">Numero de operacion</param>
    /// <returns>Datos de liquidacion en formato JSON</returns>
    /// <response code="200">Datos de liquidacion</response>
    /// <response code="404">Operacion no encontrada</response>
    [HttpGet("liquidacion/{numeroOperacion}")]
    [SwaggerOperation(Summary = "Obtener datos de liquidacion", Description = "Retorna datos JSON de liquidacion para previsualizacion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLiquidacion(int numeroOperacion)
    {
        var liquidacion = await _pdfService.GetLiquidacionOperacionAsync(numeroOperacion);

        if (liquidacion == null)
            return NotFound(new { Mensaje = "Operacion no encontrada" });

        return Ok(liquidacion);
    }

    /// <summary>
    /// Genera PDF de liquidacion de operacion
    /// </summary>
    /// <remarks>
    /// Genera un documento PDF profesional con la liquidacion de la operacion.
    /// Incluye: encabezado empresa, datos cliente/deudor, tabla de documentos,
    /// detalle de intereses/gastos, resumen financiero y espacio para firmas.
    /// El archivo se descarga automaticamente.
    /// </remarks>
    /// <param name="numeroOperacion">Numero de operacion</param>
    /// <returns>Archivo PDF de liquidacion</returns>
    /// <response code="200">Archivo PDF generado</response>
    /// <response code="404">Operacion no encontrada</response>
    [HttpGet("liquidacion/{numeroOperacion}/pdf")]
    [SwaggerOperation(Summary = "Generar PDF de liquidacion", Description = "Genera documento PDF de liquidacion para impresion o envio")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLiquidacionPdf(int numeroOperacion)
    {
        var liquidacion = await _pdfService.GetLiquidacionOperacionAsync(numeroOperacion);

        if (liquidacion == null)
            return NotFound(new { Mensaje = "Operacion no encontrada" });

        var pdfBytes = _pdfGenerator.GenerarLiquidacion(liquidacion);

        var fileName = $"Liquidacion_{numeroOperacion}_{DateTime.Now:yyyyMMdd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // ==========================================
    // PAGARE
    // ==========================================

    /// <summary>
    /// Obtiene datos de pagare (JSON)
    /// </summary>
    /// <remarks>
    /// Retorna datos del pagare incluyendo monto en letras.
    /// Formato legal chileno con datos del deudor principal y aval.
    /// </remarks>
    /// <param name="numeroOperacion">Numero de operacion</param>
    /// <returns>Datos del pagare</returns>
    [HttpGet("pagare/{numeroOperacion}")]
    [SwaggerOperation(Summary = "Obtener datos de pagare", Description = "Retorna datos JSON del pagare")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPagare(int numeroOperacion)
    {
        var pagare = await _pdfService.GetPagareAsync(numeroOperacion);

        if (pagare == null)
            return NotFound(new { Mensaje = "Operacion no encontrada" });

        return Ok(pagare);
    }

    /// <summary>
    /// Genera PDF de pagare en formato legal chileno
    /// </summary>
    /// <remarks>
    /// Genera documento PDF de pagare con formato legal.
    /// Incluye: monto en letras, clausulas de mora, datos deudor/aval.
    /// Listo para firma y presentacion ante notario.
    /// </remarks>
    /// <param name="numeroOperacion">Numero de operacion</param>
    /// <returns>Archivo PDF del pagare</returns>
    [HttpGet("pagare/{numeroOperacion}/pdf")]
    [SwaggerOperation(Summary = "Generar PDF de pagare", Description = "Genera documento legal de pagare")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPagarePdf(int numeroOperacion)
    {
        var pagare = await _pdfService.GetPagareAsync(numeroOperacion);

        if (pagare == null)
            return NotFound(new { Mensaje = "Operacion no encontrada" });

        var pdfBytes = _pdfGenerator.GenerarPagare(pagare);

        var fileName = $"Pagare_{numeroOperacion}_{DateTime.Now:yyyyMMdd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // ==========================================
    // ESTADO DE CUENTA
    // ==========================================

    /// <summary>
    /// Obtiene datos de estado de cuenta del cliente (JSON)
    /// </summary>
    /// <remarks>
    /// Retorna estado de cuenta con movimientos y documentos pendientes.
    /// Filtrable por rango de fechas.
    /// </remarks>
    /// <param name="rutCliente">RUT del cliente (sin digito verificador)</param>
    /// <param name="fechaDesde">Fecha inicial del periodo (opcional)</param>
    /// <param name="fechaHasta">Fecha final del periodo (opcional)</param>
    /// <returns>Estado de cuenta del cliente</returns>
    [HttpGet("estado-cuenta/{rutCliente}")]
    [SwaggerOperation(Summary = "Obtener estado de cuenta", Description = "Retorna datos JSON del estado de cuenta")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEstadoCuenta(
        long rutCliente,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        var estado = await _pdfService.GetEstadoCuentaAsync(rutCliente, fechaDesde, fechaHasta);

        if (estado == null)
            return NotFound(new { Mensaje = "Cliente no encontrado" });

        return Ok(estado);
    }

    /// <summary>
    /// Genera PDF de estado de cuenta del cliente
    /// </summary>
    /// <remarks>
    /// Genera documento PDF con estado de cuenta detallado.
    /// Incluye: movimientos del periodo, documentos pendientes,
    /// excedentes disponibles y resumen de saldos.
    /// Ideal para envio al cliente.
    /// </remarks>
    /// <param name="rutCliente">RUT del cliente (sin digito verificador)</param>
    /// <param name="fechaDesde">Fecha inicial del periodo (opcional)</param>
    /// <param name="fechaHasta">Fecha final del periodo (opcional)</param>
    /// <returns>Archivo PDF de estado de cuenta</returns>
    [HttpGet("estado-cuenta/{rutCliente}/pdf")]
    [SwaggerOperation(Summary = "Generar PDF de estado de cuenta", Description = "Genera documento PDF de estado de cuenta para el cliente")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEstadoCuentaPdf(
        long rutCliente,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        var estado = await _pdfService.GetEstadoCuentaAsync(rutCliente, fechaDesde, fechaHasta);

        if (estado == null)
            return NotFound(new { Mensaje = "Cliente no encontrado" });

        var pdfBytes = _pdfGenerator.GenerarEstadoCuenta(estado);

        var fileName = $"EstadoCuenta_{rutCliente}_{DateTime.Now:yyyyMMdd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
