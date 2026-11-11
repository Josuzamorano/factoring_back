using Microsoft.AspNetCore.Mvc;
using Factoring.Application.Services;

namespace Factoring.API.Controllers;

/// <summary>
/// Controlador para alertas y notificaciones del sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AlertasController : ControllerBase
{
    private readonly AlertaService _alertaService;

    public AlertasController(AlertaService alertaService)
    {
        _alertaService = alertaService;
    }

    // ==========================================
    // ALERTAS DE LÍNEAS DE CRÉDITO
    // ==========================================

    /// <summary>
    /// Obtiene alertas de líneas próximas a vencer
    /// </summary>
    [HttpGet("lineas/vencimiento")]
    public async Task<IActionResult> GetAlertasLineasVencimiento([FromQuery] int diasAnticipacion = 30)
    {
        var alertas = await _alertaService.GetAlertasLineasVencimientoAsync(diasAnticipacion);
        return Ok(alertas);
    }

    /// <summary>
    /// Bloquea automáticamente líneas vencidas
    /// </summary>
    [HttpPost("lineas/bloquear-vencidas")]
    public async Task<IActionResult> BloquearLineasVencidas()
    {
        var lineasBloqueadas = await _alertaService.BloquearLineasVencidasAsync();
        return Ok(new {
            Mensaje = $"Se bloquearon {lineasBloqueadas} líneas vencidas",
            LineasBloqueadas = lineasBloqueadas
        });
    }

    /// <summary>
    /// Obtiene clientes con morosidad
    /// </summary>
    [HttpGet("clientes/morosos")]
    public async Task<IActionResult> GetClientesConMorosidad([FromQuery] int diasMoroso = 30)
    {
        var clientes = await _alertaService.GetClientesConMorosidadAsync(diasMoroso);
        return Ok(clientes);
    }

    /// <summary>
    /// Bloquea línea de un cliente por morosidad
    /// </summary>
    [HttpPost("lineas/bloquear-morosidad")]
    public async Task<IActionResult> BloquearLineaPorMorosidad(
        [FromQuery] long rutCliente,
        [FromQuery] string usuario,
        [FromQuery] string motivo = "Morosidad detectada")
    {
        var resultado = await _alertaService.BloquearLineaPorMorosidadAsync(rutCliente, usuario, motivo);

        if (!resultado)
            return NotFound(new { Mensaje = "No se encontró línea activa para bloquear" });

        return Ok(new { Mensaje = $"Línea del cliente {rutCliente} bloqueada por morosidad" });
    }

    // ==========================================
    // ALERTAS DE DOCUMENTOS
    // ==========================================

    /// <summary>
    /// Obtiene alertas de documentos próximos a vencer
    /// </summary>
    [HttpGet("documentos/vencimiento")]
    public async Task<IActionResult> GetAlertasDocumentosVencer([FromQuery] int diasAnticipacion = 30)
    {
        var alertas = await _alertaService.GetAlertasDocumentosVencerAsync(diasAnticipacion);
        return Ok(alertas);
    }

    /// <summary>
    /// Marca documentos como morosos después de período de gracia
    /// </summary>
    [HttpPost("documentos/marcar-morosos")]
    public async Task<IActionResult> MarcarDocumentosMorosos([FromQuery] int diasGracia = 30)
    {
        var documentosMarcados = await _alertaService.MarcarDocumentosMorososAsync(diasGracia);
        return Ok(new {
            Mensaje = $"Se marcaron {documentosMarcados} documentos como morosos",
            DocumentosMarcados = documentosMarcados
        });
    }

    // ==========================================
    // RESUMEN GENERAL
    // ==========================================

    /// <summary>
    /// Obtiene resumen completo de alertas del sistema
    /// </summary>
    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumenAlertas()
    {
        var resumen = await _alertaService.GetResumenAlertasAsync();
        return Ok(resumen);
    }
}
