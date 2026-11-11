using Factoring.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Factoring.Infrastructure.Services.NotificacionService;

namespace Factoring.API.Controllers;

/// <summary>
/// Controlador para envío de notificaciones por email
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Notificaciones - Envío de emails y alertas")]
public class NotificacionesController : ControllerBase
{
    private readonly NotificacionService _notificacionService;
    private readonly ILogger<NotificacionesController> _logger;

    public NotificacionesController(
        NotificacionService notificacionService,
        ILogger<NotificacionesController> logger)
    {
        _notificacionService = notificacionService;
        _logger = logger;
    }

    #region DTOs

    /// <summary>
    /// DTO para envío de email genérico
    /// </summary>
    public class EnviarEmailDto
    {
        /// <summary>Lista de destinatarios</summary>
        public List<string> Destinatarios { get; set; } = new();
        /// <summary>Destinatarios en copia</summary>
        public List<string>? ConCopia { get; set; }
        /// <summary>Asunto del email</summary>
        public string Asunto { get; set; } = string.Empty;
        /// <summary>Cuerpo del mensaje</summary>
        public string Cuerpo { get; set; } = string.Empty;
        /// <summary>Indica si el cuerpo es HTML</summary>
        public bool EsHtml { get; set; } = false;
    }

    /// <summary>
    /// DTO para notificación de operación
    /// </summary>
    public class NotificarOperacionDto
    {
        public string EmailCliente { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public int NumeroOperacion { get; set; }
        public decimal MontoOperacion { get; set; }
        public decimal MontoGiro { get; set; }
        public DateTime FechaOperacion { get; set; }
        public List<DocumentoNotificacionDto> Documentos { get; set; } = new();
    }

    public class DocumentoNotificacionDto
    {
        public string NumeroDocumento { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Vencimiento { get; set; }
    }

    /// <summary>
    /// DTO para notificación de vencimientos
    /// </summary>
    public class NotificarVencimientosDto
    {
        public string EmailCliente { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public List<DocumentoVencimientoDto> Documentos { get; set; } = new();
    }

    public class DocumentoVencimientoDto
    {
        public string NumeroDocumento { get; set; } = string.Empty;
        public string NombreDeudor { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Vencimiento { get; set; }
        public int DiasRestantes { get; set; }
    }

    /// <summary>
    /// DTO para recordatorio de cobranza
    /// </summary>
    public class RecordatorioCobranzaDto
    {
        public string EmailDeudor { get; set; } = string.Empty;
        public string NombreDeudor { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public List<DocumentoCobranzaDto> Documentos { get; set; } = new();
    }

    public class DocumentoCobranzaDto
    {
        public string NumeroDocumento { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Vencimiento { get; set; }
        public int DiasVencido { get; set; }
    }

    #endregion

    /// <summary>
    /// Envía un email genérico
    /// </summary>
    [HttpPost("enviar")]
    [SwaggerOperation(
        Summary = "Enviar email",
        Description = "Envía un email genérico a uno o más destinatarios")]
    [SwaggerResponse(200, "Email enviado exitosamente")]
    [SwaggerResponse(400, "Error en los datos")]
    public async Task<ActionResult<EmailResult>> EnviarEmail([FromBody] EnviarEmailDto dto)
    {
        if (!dto.Destinatarios.Any())
        {
            return BadRequest(new { mensaje = "Debe especificar al menos un destinatario" });
        }

        if (string.IsNullOrEmpty(dto.Asunto))
        {
            return BadRequest(new { mensaje = "El asunto es requerido" });
        }

        var request = new EmailRequest
        {
            Destinatarios = dto.Destinatarios,
            ConCopia = dto.ConCopia ?? new List<string>(),
            Asunto = dto.Asunto,
            Cuerpo = dto.Cuerpo,
            EsHtml = dto.EsHtml
        };

        var result = await _notificacionService.EnviarEmailAsync(request);

        if (!result.Exito)
        {
            return BadRequest(new { mensaje = result.Mensaje });
        }

        return Ok(result);
    }

    /// <summary>
    /// Notifica otorgamiento de operación al cliente
    /// </summary>
    [HttpPost("operacion-otorgada")]
    [SwaggerOperation(
        Summary = "Notificar operación otorgada",
        Description = "Envía email de confirmación de operación otorgada al cliente")]
    [SwaggerResponse(200, "Notificación enviada")]
    public async Task<ActionResult<EmailResult>> NotificarOperacionOtorgada(
        [FromBody] NotificarOperacionDto dto)
    {
        if (string.IsNullOrEmpty(dto.EmailCliente))
        {
            return BadRequest(new { mensaje = "Email del cliente es requerido" });
        }

        var documentos = dto.Documentos
            .Select(d => (d.NumeroDocumento, d.Monto, d.Vencimiento))
            .ToList();

        var result = await _notificacionService.NotificarOperacionOtorgadaAsync(
            dto.EmailCliente,
            dto.NombreCliente,
            dto.NumeroOperacion,
            dto.MontoOperacion,
            dto.MontoGiro,
            dto.FechaOperacion,
            documentos);

        return Ok(result);
    }

    /// <summary>
    /// Notifica documentos próximos a vencer
    /// </summary>
    [HttpPost("documentos-por-vencer")]
    [SwaggerOperation(
        Summary = "Notificar documentos por vencer",
        Description = "Envía email de alerta de documentos próximos a vencer")]
    [SwaggerResponse(200, "Notificación enviada")]
    public async Task<ActionResult<EmailResult>> NotificarDocumentosPorVencer(
        [FromBody] NotificarVencimientosDto dto)
    {
        if (string.IsNullOrEmpty(dto.EmailCliente))
        {
            return BadRequest(new { mensaje = "Email del cliente es requerido" });
        }

        if (!dto.Documentos.Any())
        {
            return BadRequest(new { mensaje = "Debe incluir al menos un documento" });
        }

        var documentos = dto.Documentos
            .Select(d => (d.NumeroDocumento, d.NombreDeudor, d.Monto, d.Vencimiento, d.DiasRestantes))
            .ToList();

        var result = await _notificacionService.NotificarDocumentosPorVencerAsync(
            dto.EmailCliente,
            dto.NombreCliente,
            documentos);

        return Ok(result);
    }

    /// <summary>
    /// Envía recordatorio de cobranza a deudor
    /// </summary>
    [HttpPost("recordatorio-cobranza")]
    [SwaggerOperation(
        Summary = "Enviar recordatorio de cobranza",
        Description = "Envía email de recordatorio de pago a un deudor")]
    [SwaggerResponse(200, "Recordatorio enviado")]
    public async Task<ActionResult<EmailResult>> EnviarRecordatorioCobranza(
        [FromBody] RecordatorioCobranzaDto dto)
    {
        if (string.IsNullOrEmpty(dto.EmailDeudor))
        {
            return BadRequest(new { mensaje = "Email del deudor es requerido" });
        }

        if (!dto.Documentos.Any())
        {
            return BadRequest(new { mensaje = "Debe incluir al menos un documento" });
        }

        var documentos = dto.Documentos
            .Select(d => (d.NumeroDocumento, d.Monto, d.Vencimiento, d.DiasVencido))
            .ToList();

        var result = await _notificacionService.EnviarRecordatorioCobranzaAsync(
            dto.EmailDeudor,
            dto.NombreDeudor,
            dto.NombreCliente,
            documentos);

        return Ok(result);
    }

    /// <summary>
    /// Notifica pago registrado al cliente
    /// </summary>
    [HttpPost("pago-registrado")]
    [SwaggerOperation(
        Summary = "Notificar pago registrado",
        Description = "Envía email de confirmación de pago registrado")]
    [SwaggerResponse(200, "Notificación enviada")]
    public async Task<ActionResult<EmailResult>> NotificarPagoRegistrado(
        [FromQuery] string emailCliente,
        [FromQuery] string nombreCliente,
        [FromQuery] string numeroDocumento,
        [FromQuery] string nombreDeudor,
        [FromQuery] decimal montoPagado,
        [FromQuery] DateTime fechaPago)
    {
        if (string.IsNullOrEmpty(emailCliente))
        {
            return BadRequest(new { mensaje = "Email del cliente es requerido" });
        }

        var result = await _notificacionService.NotificarPagoRegistradoAsync(
            emailCliente,
            nombreCliente,
            numeroDocumento,
            nombreDeudor,
            montoPagado,
            fechaPago);

        return Ok(result);
    }

    /// <summary>
    /// Notifica cesión electrónica registrada
    /// </summary>
    [HttpPost("cesion-registrada")]
    [SwaggerOperation(
        Summary = "Notificar cesión registrada",
        Description = "Envía email de confirmación de cesión electrónica en el SII")]
    [SwaggerResponse(200, "Notificación enviada")]
    public async Task<ActionResult<EmailResult>> NotificarCesionRegistrada(
        [FromQuery] string emailCliente,
        [FromQuery] string nombreCliente,
        [FromQuery] string numeroFactura,
        [FromQuery] decimal monto,
        [FromQuery] string trackIdSii)
    {
        if (string.IsNullOrEmpty(emailCliente))
        {
            return BadRequest(new { mensaje = "Email del cliente es requerido" });
        }

        var result = await _notificacionService.NotificarCesionRegistradaAsync(
            emailCliente,
            nombreCliente,
            numeroFactura,
            monto,
            trackIdSii);

        return Ok(result);
    }

    /// <summary>
    /// Prueba la configuración de email
    /// </summary>
    [HttpPost("test")]
    [SwaggerOperation(
        Summary = "Probar configuración de email",
        Description = "Envía un email de prueba para verificar la configuración SMTP")]
    [SwaggerResponse(200, "Email de prueba enviado")]
    public async Task<ActionResult<EmailResult>> ProbarEmail([FromQuery] string emailDestino)
    {
        if (string.IsNullOrEmpty(emailDestino))
        {
            return BadRequest(new { mensaje = "Email destino es requerido" });
        }

        var result = await _notificacionService.EnviarEmailAsync(new EmailRequest
        {
            Destinatarios = new List<string> { emailDestino },
            Asunto = "Prueba de Configuración - Sistema Factoring",
            Cuerpo = $@"
                <h2>Email de Prueba</h2>
                <p>Este es un email de prueba del Sistema de Factoring.</p>
                <p>Si recibió este mensaje, la configuración SMTP está funcionando correctamente.</p>
                <p>Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
            ",
            EsHtml = true
        });

        return Ok(result);
    }
}
