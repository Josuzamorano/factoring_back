using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Factoring.Infrastructure.Services;

/// <summary>
/// Servicio de notificaciones por email
/// Soporta SMTP directo y plantillas HTML
/// </summary>
public class NotificacionService
{
    private readonly ILogger<NotificacionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly bool _useSsl;
    private readonly bool _modoSimulacion;

    public NotificacionService(
        IConfiguration configuration,
        ILogger<NotificacionService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Configuración SMTP
        _smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        _smtpUser = configuration["Email:SmtpUser"] ?? "";
        _smtpPassword = configuration["Email:SmtpPassword"] ?? "";
        _fromEmail = configuration["Email:FromEmail"] ?? "noreply@factoring.cl";
        _fromName = configuration["Email:FromName"] ?? "Sistema Factoring";
        _useSsl = bool.Parse(configuration["Email:UseSsl"] ?? "true");
        _modoSimulacion = string.IsNullOrEmpty(_smtpUser) || configuration["Email:Simulacion"] == "true";

        if (_modoSimulacion)
        {
            _logger.LogWarning("NotificacionService en modo simulación - los emails no se enviarán realmente");
        }
    }

    #region Tipos de Datos

    /// <summary>
    /// Resultado del envío de email
    /// </summary>
    public class EmailResult
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
        public string? MessageId { get; set; }
        public DateTime FechaEnvio { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Datos para envío de email
    /// </summary>
    public class EmailRequest
    {
        public List<string> Destinatarios { get; set; } = new();
        public List<string> ConCopia { get; set; } = new();
        public List<string> ConCopiaOculta { get; set; } = new();
        public string Asunto { get; set; } = string.Empty;
        public string Cuerpo { get; set; } = string.Empty;
        public bool EsHtml { get; set; } = true;
        public List<EmailAdjunto> Adjuntos { get; set; } = new();
    }

    /// <summary>
    /// Adjunto de email
    /// </summary>
    public class EmailAdjunto
    {
        public string NombreArchivo { get; set; } = string.Empty;
        public byte[] Contenido { get; set; } = Array.Empty<byte>();
        public string TipoMime { get; set; } = "application/octet-stream";
    }

    /// <summary>
    /// Tipos de notificación predefinidos
    /// </summary>
    public enum TipoNotificacion
    {
        OperacionOtorgada,
        OperacionAnulada,
        DocumentoVencido,
        DocumentoPorVencer,
        PagoRegistrado,
        CesionRegistrada,
        AlertaCobranza,
        RecordatorioPago,
        BienvenidaCliente
    }

    #endregion

    #region Envío de Emails

    /// <summary>
    /// Envía un email
    /// </summary>
    public async Task<EmailResult> EnviarEmailAsync(EmailRequest request)
    {
        var result = new EmailResult();

        try
        {
            _logger.LogInformation(
                "Enviando email a {Destinatarios}: {Asunto}",
                string.Join(", ", request.Destinatarios),
                request.Asunto);

            if (_modoSimulacion)
            {
                return SimularEnvio(request);
            }

            using var mensaje = new MailMessage();
            mensaje.From = new MailAddress(_fromEmail, _fromName);
            mensaje.Subject = request.Asunto;
            mensaje.Body = request.Cuerpo;
            mensaje.IsBodyHtml = request.EsHtml;

            foreach (var dest in request.Destinatarios)
            {
                mensaje.To.Add(dest);
            }

            foreach (var cc in request.ConCopia)
            {
                mensaje.CC.Add(cc);
            }

            foreach (var bcc in request.ConCopiaOculta)
            {
                mensaje.Bcc.Add(bcc);
            }

            foreach (var adjunto in request.Adjuntos)
            {
                var stream = new MemoryStream(adjunto.Contenido);
                mensaje.Attachments.Add(new Attachment(stream, adjunto.NombreArchivo, adjunto.TipoMime));
            }

            using var cliente = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = _useSsl,
                Credentials = new NetworkCredential(_smtpUser, _smtpPassword)
            };

            await cliente.SendMailAsync(mensaje);

            result.Exito = true;
            result.MessageId = Guid.NewGuid().ToString();
            result.Mensaje = "Email enviado exitosamente";

            _logger.LogInformation(
                "Email enviado exitosamente a {Destinatarios}",
                string.Join(", ", request.Destinatarios));
        }
        catch (Exception ex)
        {
            result.Mensaje = $"Error enviando email: {ex.Message}";
            _logger.LogError(ex, "Error enviando email a {Destinatarios}",
                string.Join(", ", request.Destinatarios));
        }

        return result;
    }

    private EmailResult SimularEnvio(EmailRequest request)
    {
        _logger.LogInformation(
            "[SIMULACIÓN] Email enviado a {Destinatarios}: {Asunto}",
            string.Join(", ", request.Destinatarios),
            request.Asunto);

        return new EmailResult
        {
            Exito = true,
            MessageId = $"SIM-{Guid.NewGuid():N}",
            Mensaje = "Email simulado (modo desarrollo)"
        };
    }

    #endregion

    #region Notificaciones Predefinidas

    /// <summary>
    /// Notifica otorgamiento de operación al cliente
    /// </summary>
    public async Task<EmailResult> NotificarOperacionOtorgadaAsync(
        string emailCliente,
        string nombreCliente,
        int numeroOperacion,
        decimal montoOperacion,
        decimal montoGiro,
        DateTime fechaOperacion,
        List<(string NumDoc, decimal Monto, DateTime Vencimiento)> documentos)
    {
        var html = GenerarPlantillaOperacionOtorgada(
            nombreCliente, numeroOperacion, montoOperacion, montoGiro, fechaOperacion, documentos);

        return await EnviarEmailAsync(new EmailRequest
        {
            Destinatarios = new List<string> { emailCliente },
            Asunto = $"Operación #{numeroOperacion} - Confirmación de Otorgamiento",
            Cuerpo = html,
            EsHtml = true
        });
    }

    /// <summary>
    /// Notifica vencimiento próximo de documentos
    /// </summary>
    public async Task<EmailResult> NotificarDocumentosPorVencerAsync(
        string emailCliente,
        string nombreCliente,
        List<(string NumDoc, string Deudor, decimal Monto, DateTime Vencimiento, int DiasRestantes)> documentos)
    {
        var html = GenerarPlantillaDocumentosPorVencer(nombreCliente, documentos);

        return await EnviarEmailAsync(new EmailRequest
        {
            Destinatarios = new List<string> { emailCliente },
            Asunto = $"Aviso: {documentos.Count} documento(s) próximos a vencer",
            Cuerpo = html,
            EsHtml = true
        });
    }

    /// <summary>
    /// Notifica registro de pago
    /// </summary>
    public async Task<EmailResult> NotificarPagoRegistradoAsync(
        string emailCliente,
        string nombreCliente,
        string numeroDocumento,
        string nombreDeudor,
        decimal montoPagado,
        DateTime fechaPago)
    {
        var html = GenerarPlantillaPagoRegistrado(
            nombreCliente, numeroDocumento, nombreDeudor, montoPagado, fechaPago);

        return await EnviarEmailAsync(new EmailRequest
        {
            Destinatarios = new List<string> { emailCliente },
            Asunto = $"Pago Registrado - Documento {numeroDocumento}",
            Cuerpo = html,
            EsHtml = true
        });
    }

    /// <summary>
    /// Notifica cesión electrónica registrada
    /// </summary>
    public async Task<EmailResult> NotificarCesionRegistradaAsync(
        string emailCliente,
        string nombreCliente,
        string numeroFactura,
        decimal monto,
        string trackIdSii)
    {
        var html = GenerarPlantillaCesionRegistrada(
            nombreCliente, numeroFactura, monto, trackIdSii);

        return await EnviarEmailAsync(new EmailRequest
        {
            Destinatarios = new List<string> { emailCliente },
            Asunto = $"Cesión Electrónica Registrada - Factura {numeroFactura}",
            Cuerpo = html,
            EsHtml = true
        });
    }

    /// <summary>
    /// Envía recordatorio de cobranza al deudor
    /// </summary>
    public async Task<EmailResult> EnviarRecordatorioCobranzaAsync(
        string emailDeudor,
        string nombreDeudor,
        string nombreCliente,
        List<(string NumDoc, decimal Monto, DateTime Vencimiento, int DiasVencido)> documentos)
    {
        var html = GenerarPlantillaRecordatorioCobranza(
            nombreDeudor, nombreCliente, documentos);

        return await EnviarEmailAsync(new EmailRequest
        {
            Destinatarios = new List<string> { emailDeudor },
            Asunto = $"Recordatorio de Pago - {documentos.Count} documento(s) pendiente(s)",
            Cuerpo = html,
            EsHtml = true
        });
    }

    #endregion

    #region Plantillas HTML

    private string GenerarPlantillaOperacionOtorgada(
        string nombreCliente,
        int numeroOperacion,
        decimal montoOperacion,
        decimal montoGiro,
        DateTime fechaOperacion,
        List<(string NumDoc, decimal Monto, DateTime Vencimiento)> documentos)
    {
        var sb = new StringBuilder();
        sb.AppendLine(GetHeaderHtml());
        sb.AppendLine($@"
        <div class='content'>
            <h2>Confirmación de Operación</h2>
            <p>Estimado(a) <strong>{nombreCliente}</strong>,</p>
            <p>Le informamos que su operación de factoring ha sido otorgada exitosamente:</p>

            <div class='highlight'>
                <table>
                    <tr><td><strong>Número de Operación:</strong></td><td>#{numeroOperacion}</td></tr>
                    <tr><td><strong>Fecha:</strong></td><td>{fechaOperacion:dd/MM/yyyy}</td></tr>
                    <tr><td><strong>Monto Total Documentos:</strong></td><td>${montoOperacion:N0}</td></tr>
                    <tr><td><strong>Monto a Girar:</strong></td><td>${montoGiro:N0}</td></tr>
                </table>
            </div>

            <h3>Documentos Incluidos</h3>
            <table class='data-table'>
                <thead>
                    <tr>
                        <th>N° Documento</th>
                        <th>Monto</th>
                        <th>Vencimiento</th>
                    </tr>
                </thead>
                <tbody>");

        foreach (var doc in documentos)
        {
            sb.AppendLine($@"
                    <tr>
                        <td>{doc.NumDoc}</td>
                        <td>${doc.Monto:N0}</td>
                        <td>{doc.Vencimiento:dd/MM/yyyy}</td>
                    </tr>");
        }

        sb.AppendLine($@"
                </tbody>
            </table>

            <p>El monto será depositado en su cuenta bancaria registrada en las próximas horas.</p>
            <p>Ante cualquier consulta, no dude en contactarnos.</p>
        </div>");
        sb.AppendLine(GetFooterHtml());

        return sb.ToString();
    }

    private string GenerarPlantillaDocumentosPorVencer(
        string nombreCliente,
        List<(string NumDoc, string Deudor, decimal Monto, DateTime Vencimiento, int DiasRestantes)> documentos)
    {
        var sb = new StringBuilder();
        sb.AppendLine(GetHeaderHtml());
        sb.AppendLine($@"
        <div class='content'>
            <h2>Aviso de Vencimiento Próximo</h2>
            <p>Estimado(a) <strong>{nombreCliente}</strong>,</p>
            <p>Le informamos que los siguientes documentos están próximos a vencer:</p>

            <table class='data-table'>
                <thead>
                    <tr>
                        <th>N° Documento</th>
                        <th>Deudor</th>
                        <th>Monto</th>
                        <th>Vencimiento</th>
                        <th>Días Restantes</th>
                    </tr>
                </thead>
                <tbody>");

        foreach (var doc in documentos)
        {
            var clase = doc.DiasRestantes <= 3 ? "urgente" : "";
            sb.AppendLine($@"
                    <tr class='{clase}'>
                        <td>{doc.NumDoc}</td>
                        <td>{doc.Deudor}</td>
                        <td>${doc.Monto:N0}</td>
                        <td>{doc.Vencimiento:dd/MM/yyyy}</td>
                        <td>{doc.DiasRestantes} días</td>
                    </tr>");
        }

        sb.AppendLine($@"
                </tbody>
            </table>

            <p>Le sugerimos coordinar con sus deudores para asegurar el pago oportuno.</p>
        </div>");
        sb.AppendLine(GetFooterHtml());

        return sb.ToString();
    }

    private string GenerarPlantillaPagoRegistrado(
        string nombreCliente,
        string numeroDocumento,
        string nombreDeudor,
        decimal montoPagado,
        DateTime fechaPago)
    {
        var sb = new StringBuilder();
        sb.AppendLine(GetHeaderHtml());
        sb.AppendLine($@"
        <div class='content'>
            <h2>Pago Registrado</h2>
            <p>Estimado(a) <strong>{nombreCliente}</strong>,</p>
            <p>Le informamos que se ha registrado el siguiente pago:</p>

            <div class='highlight success'>
                <table>
                    <tr><td><strong>Documento:</strong></td><td>{numeroDocumento}</td></tr>
                    <tr><td><strong>Deudor:</strong></td><td>{nombreDeudor}</td></tr>
                    <tr><td><strong>Monto Pagado:</strong></td><td>${montoPagado:N0}</td></tr>
                    <tr><td><strong>Fecha de Pago:</strong></td><td>{fechaPago:dd/MM/yyyy}</td></tr>
                </table>
            </div>

            <p>El excedente (si corresponde) será depositado en su cuenta.</p>
        </div>");
        sb.AppendLine(GetFooterHtml());

        return sb.ToString();
    }

    private string GenerarPlantillaCesionRegistrada(
        string nombreCliente,
        string numeroFactura,
        decimal monto,
        string trackIdSii)
    {
        var sb = new StringBuilder();
        sb.AppendLine(GetHeaderHtml());
        sb.AppendLine($@"
        <div class='content'>
            <h2>Cesión Electrónica Registrada</h2>
            <p>Estimado(a) <strong>{nombreCliente}</strong>,</p>
            <p>Le informamos que la cesión electrónica de su factura ha sido registrada exitosamente en el SII:</p>

            <div class='highlight'>
                <table>
                    <tr><td><strong>Factura:</strong></td><td>{numeroFactura}</td></tr>
                    <tr><td><strong>Monto:</strong></td><td>${monto:N0}</td></tr>
                    <tr><td><strong>Track ID SII:</strong></td><td>{trackIdSii}</td></tr>
                    <tr><td><strong>Fecha Registro:</strong></td><td>{DateTime.Now:dd/MM/yyyy HH:mm}</td></tr>
                </table>
            </div>

            <p>La cesión quedó inscrita en el Registro de Transferencias y Cesiones del SII.</p>
        </div>");
        sb.AppendLine(GetFooterHtml());

        return sb.ToString();
    }

    private string GenerarPlantillaRecordatorioCobranza(
        string nombreDeudor,
        string nombreCliente,
        List<(string NumDoc, decimal Monto, DateTime Vencimiento, int DiasVencido)> documentos)
    {
        var montoTotal = documentos.Sum(d => d.Monto);

        var sb = new StringBuilder();
        sb.AppendLine(GetHeaderHtml());
        sb.AppendLine($@"
        <div class='content'>
            <h2>Recordatorio de Pago</h2>
            <p>Estimado(a) <strong>{nombreDeudor}</strong>,</p>
            <p>Le recordamos que mantiene documentos pendientes de pago con <strong>{nombreCliente}</strong>:</p>

            <table class='data-table'>
                <thead>
                    <tr>
                        <th>N° Documento</th>
                        <th>Monto</th>
                        <th>Vencimiento</th>
                        <th>Estado</th>
                    </tr>
                </thead>
                <tbody>");

        foreach (var doc in documentos)
        {
            var estado = doc.DiasVencido > 0 ? $"Vencido ({doc.DiasVencido} días)" : "Por vencer";
            var clase = doc.DiasVencido > 0 ? "urgente" : "";
            sb.AppendLine($@"
                    <tr class='{clase}'>
                        <td>{doc.NumDoc}</td>
                        <td>${doc.Monto:N0}</td>
                        <td>{doc.Vencimiento:dd/MM/yyyy}</td>
                        <td>{estado}</td>
                    </tr>");
        }

        sb.AppendLine($@"
                </tbody>
                <tfoot>
                    <tr>
                        <td colspan='1'><strong>Total:</strong></td>
                        <td colspan='3'><strong>${montoTotal:N0}</strong></td>
                    </tr>
                </tfoot>
            </table>

            <p>Por favor, regularice su situación a la brevedad.</p>
            <p>Para coordinar el pago, comuníquese con nuestro departamento de cobranza.</p>
        </div>");
        sb.AppendLine(GetFooterHtml());

        return sb.ToString();
    }

    private string GetHeaderHtml()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #1a365d; color: white; padding: 20px; text-align: center; }
        .header h1 { margin: 0; font-size: 24px; }
        .content { padding: 20px; background-color: #f8f9fa; }
        .highlight { background-color: #e8f4f8; padding: 15px; border-radius: 5px; margin: 15px 0; }
        .highlight.success { background-color: #d4edda; }
        .highlight table { width: 100%; }
        .highlight td { padding: 5px 10px; }
        .data-table { width: 100%; border-collapse: collapse; margin: 15px 0; }
        .data-table th, .data-table td { border: 1px solid #ddd; padding: 10px; text-align: left; }
        .data-table th { background-color: #1a365d; color: white; }
        .data-table tr:nth-child(even) { background-color: #f2f2f2; }
        .data-table tr.urgente { background-color: #f8d7da; }
        .footer { background-color: #1a365d; color: white; padding: 15px; text-align: center; font-size: 12px; }
        h2 { color: #1a365d; }
        h3 { color: #2d3748; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Sistema Factoring</h1>
        </div>";
    }

    private string GetFooterHtml()
    {
        return $@"
        <div class='footer'>
            <p>Este es un mensaje automático del Sistema de Factoring.</p>
            <p>Por favor no responda a este correo.</p>
            <p>&copy; {DateTime.Now.Year} Factoring S.A. - Todos los derechos reservados</p>
        </div>
    </div>
</body>
</html>";
    }

    #endregion

    #region Envío Masivo

    /// <summary>
    /// Envía múltiples emails en lote
    /// </summary>
    public async Task<List<EmailResult>> EnviarEmailsMasivosAsync(List<EmailRequest> requests)
    {
        var resultados = new List<EmailResult>();

        _logger.LogInformation("Iniciando envío masivo de {Count} emails", requests.Count);

        foreach (var request in requests)
        {
            var result = await EnviarEmailAsync(request);
            resultados.Add(result);

            // Pequeña pausa para no sobrecargar el servidor SMTP
            await Task.Delay(100);
        }

        var exitosos = resultados.Count(r => r.Exito);
        _logger.LogInformation(
            "Envío masivo completado: {Exitosos}/{Total} exitosos",
            exitosos, resultados.Count);

        return resultados;
    }

    #endregion
}
