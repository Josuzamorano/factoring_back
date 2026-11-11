using Factoring.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Factoring.Infrastructure.Services.SiiService;

namespace Factoring.API.Controllers;

/// <summary>
/// Controlador para integración con el SII (Servicio de Impuestos Internos)
/// Proporciona endpoints para consultar contribuyentes, registrar cesiones y validar RUTs
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Integración con SII - Consulta de contribuyentes y cesión electrónica de facturas")]
public class SiiController : ControllerBase
{
    private readonly SiiService _siiService;
    private readonly ILogger<SiiController> _logger;

    public SiiController(SiiService siiService, ILogger<SiiController> logger)
    {
        _siiService = siiService;
        _logger = logger;
    }

    #region Consulta de Contribuyentes

    /// <summary>
    /// Consulta información de un contribuyente en el SII
    /// </summary>
    /// <param name="rut">RUT del contribuyente (sin dígito verificador)</param>
    /// <param name="dv">Dígito verificador</param>
    /// <returns>Información del contribuyente</returns>
    [HttpGet("contribuyente/{rut}/{dv}")]
    [SwaggerOperation(
        Summary = "Consultar contribuyente",
        Description = "Consulta información de un contribuyente en el SII: razón social, giro, dirección, estado")]
    [SwaggerResponse(200, "Información del contribuyente", typeof(ContribuyenteResult))]
    [SwaggerResponse(400, "RUT inválido")]
    [SwaggerResponse(404, "Contribuyente no encontrado")]
    public async Task<ActionResult<ContribuyenteResult>> ConsultarContribuyente(
        [SwaggerParameter("RUT sin dígito verificador")] long rut,
        [SwaggerParameter("Dígito verificador (0-9 o K)")] string dv)
    {
        if (string.IsNullOrEmpty(dv))
        {
            return BadRequest(new { mensaje = "Dígito verificador es requerido" });
        }

        var result = await _siiService.ConsultarContribuyenteAsync(rut, dv.ToUpper());

        if (!result.Exito)
        {
            return BadRequest(new { mensaje = result.Mensaje });
        }

        if (!result.EsContribuyente)
        {
            return NotFound(new { mensaje = "Contribuyente no encontrado o inactivo" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Valida un RUT chileno
    /// </summary>
    [HttpGet("validar-rut/{rut}/{dv}")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Validar RUT",
        Description = "Valida el dígito verificador de un RUT chileno usando el algoritmo módulo 11")]
    [SwaggerResponse(200, "Resultado de validación")]
    public ActionResult<object> ValidarRut(
        [SwaggerParameter("RUT sin dígito verificador")] long rut,
        [SwaggerParameter("Dígito verificador")] string dv)
    {
        var esValido = _siiService.ValidarRut(rut, dv);
        var dvCalculado = _siiService.CalcularDigitoVerificador(rut);

        return Ok(new
        {
            rut,
            digitoVerificador = dv.ToUpper(),
            esValido,
            digitoVerificadorCalculado = dvCalculado,
            rutFormateado = $"{rut:N0}-{dv}".Replace(",", ".")
        });
    }

    /// <summary>
    /// Calcula el dígito verificador de un RUT
    /// </summary>
    [HttpGet("calcular-dv/{rut}")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Calcular dígito verificador",
        Description = "Calcula el dígito verificador correcto para un RUT")]
    [SwaggerResponse(200, "Dígito verificador calculado")]
    public ActionResult<object> CalcularDigitoVerificador(
        [SwaggerParameter("RUT sin dígito verificador")] long rut)
    {
        var dv = _siiService.CalcularDigitoVerificador(rut);

        return Ok(new
        {
            rut,
            digitoVerificador = dv,
            rutCompleto = $"{rut}-{dv}",
            rutFormateado = $"{rut:N0}-{dv}".Replace(",", ".")
        });
    }

    #endregion

    #region Cesión Electrónica

    /// <summary>
    /// DTO para solicitud de cesión
    /// </summary>
    public class CesionRequestDto
    {
        /// <summary>RUT del cedente (cliente factoring)</summary>
        public long RutCedente { get; set; }
        /// <summary>DV del cedente</summary>
        public string DvCedente { get; set; } = string.Empty;
        /// <summary>RUT del cesionario (empresa factoring)</summary>
        public long RutCesionario { get; set; }
        /// <summary>DV del cesionario</summary>
        public string DvCesionario { get; set; } = string.Empty;
        /// <summary>RUT del deudor (receptor de la factura)</summary>
        public long RutDeudor { get; set; }
        /// <summary>DV del deudor</summary>
        public string DvDeudor { get; set; } = string.Empty;
        /// <summary>Tipo de documento (33=Factura, 34=Factura Exenta)</summary>
        public int TipoDocumento { get; set; } = 33;
        /// <summary>Número de folio de la factura</summary>
        public long Folio { get; set; }
        /// <summary>Fecha de emisión de la factura</summary>
        public DateTime FechaEmision { get; set; }
        /// <summary>Monto total de la factura</summary>
        public decimal MontoTotal { get; set; }
        /// <summary>XML de la factura (opcional)</summary>
        public string? XmlFactura { get; set; }
        /// <summary>Email del cedente para notificación</summary>
        public string? EmailCedente { get; set; }
        /// <summary>Email del cesionario para notificación</summary>
        public string? EmailCesionario { get; set; }
        /// <summary>Email del deudor para notificación</summary>
        public string? EmailDeudor { get; set; }
    }

    /// <summary>
    /// Registra una cesión electrónica de factura en el SII
    /// </summary>
    [HttpPost("cesion")]
    [SwaggerOperation(
        Summary = "Registrar cesión electrónica",
        Description = "Registra la cesión de una factura electrónica al cesionario (factoring) en el registro del SII")]
    [SwaggerResponse(200, "Cesión registrada exitosamente", typeof(CesionResult))]
    [SwaggerResponse(400, "Error en datos de cesión")]
    public async Task<ActionResult<CesionResult>> RegistrarCesion([FromBody] CesionRequestDto dto)
    {
        // Validaciones básicas
        if (!_siiService.ValidarRut(dto.RutCedente, dto.DvCedente))
        {
            return BadRequest(new { mensaje = "RUT cedente inválido" });
        }

        if (!_siiService.ValidarRut(dto.RutCesionario, dto.DvCesionario))
        {
            return BadRequest(new { mensaje = "RUT cesionario inválido" });
        }

        if (!_siiService.ValidarRut(dto.RutDeudor, dto.DvDeudor))
        {
            return BadRequest(new { mensaje = "RUT deudor inválido" });
        }

        if (dto.Folio <= 0)
        {
            return BadRequest(new { mensaje = "Folio debe ser mayor a 0" });
        }

        if (dto.MontoTotal <= 0)
        {
            return BadRequest(new { mensaje = "Monto total debe ser mayor a 0" });
        }

        // Verificar si ya está cedida
        var (yaCedida, cesionarioActual) = await _siiService.VerificarFacturaCedidaAsync(
            dto.RutCedente, dto.DvCedente, dto.TipoDocumento, dto.Folio);

        if (yaCedida)
        {
            return BadRequest(new
            {
                mensaje = "La factura ya está cedida",
                cesionActual = cesionarioActual
            });
        }

        var request = new CesionRequest
        {
            RutCedente = dto.RutCedente,
            DvCedente = dto.DvCedente,
            RutCesionario = dto.RutCesionario,
            DvCesionario = dto.DvCesionario,
            RutDeudor = dto.RutDeudor,
            DvDeudor = dto.DvDeudor,
            TipoDocumento = dto.TipoDocumento,
            Folio = dto.Folio,
            FechaEmision = dto.FechaEmision,
            MontoTotal = dto.MontoTotal,
            XmlFactura = dto.XmlFactura,
            EmailCedente = dto.EmailCedente,
            EmailCesionario = dto.EmailCesionario,
            EmailDeudor = dto.EmailDeudor
        };

        var result = await _siiService.RegistrarCesionAsync(request);

        if (!result.Exito)
        {
            return BadRequest(new { mensaje = result.Mensaje });
        }

        _logger.LogInformation(
            "Cesión registrada: Cedente {RutCedente}, Folio {Folio}, TrackId {TrackId}",
            dto.RutCedente, dto.Folio, result.TrackId);

        return Ok(result);
    }

    /// <summary>
    /// Consulta el estado de una cesión en el SII
    /// </summary>
    [HttpGet("cesion/estado/{rutCedente}/{dv}/{tipoDocumento}/{folio}")]
    [SwaggerOperation(
        Summary = "Consultar estado de cesión",
        Description = "Consulta el estado actual de una cesión registrada en el SII")]
    [SwaggerResponse(200, "Estado de la cesión", typeof(CesionResult))]
    [SwaggerResponse(400, "Datos inválidos")]
    public async Task<ActionResult<CesionResult>> ConsultarEstadoCesion(
        [SwaggerParameter("RUT del cedente")] long rutCedente,
        [SwaggerParameter("DV del cedente")] string dv,
        [SwaggerParameter("Tipo de documento (33 o 34)")] int tipoDocumento,
        [SwaggerParameter("Número de folio")] long folio)
    {
        if (!_siiService.ValidarRut(rutCedente, dv))
        {
            return BadRequest(new { mensaje = "RUT inválido" });
        }

        var result = await _siiService.ConsultarEstadoCesionAsync(rutCedente, dv, tipoDocumento, folio);

        return Ok(result);
    }

    /// <summary>
    /// Verifica si una factura ya está cedida
    /// </summary>
    [HttpGet("cesion/verificar/{rutEmisor}/{dv}/{tipoDocumento}/{folio}")]
    [SwaggerOperation(
        Summary = "Verificar factura cedida",
        Description = "Verifica si una factura ya ha sido cedida a otro cesionario")]
    [SwaggerResponse(200, "Estado de cesión de la factura")]
    public async Task<ActionResult<object>> VerificarFacturaCedida(
        [SwaggerParameter("RUT del emisor")] long rutEmisor,
        [SwaggerParameter("DV del emisor")] string dv,
        [SwaggerParameter("Tipo de documento")] int tipoDocumento,
        [SwaggerParameter("Número de folio")] long folio)
    {
        var (yaCedida, cesionarioActual) = await _siiService.VerificarFacturaCedidaAsync(
            rutEmisor, dv, tipoDocumento, folio);

        return Ok(new
        {
            rutEmisor,
            dv,
            tipoDocumento,
            folio,
            yaCedida,
            cesionarioActual
        });
    }

    #endregion

    #region Lectura de XML

    /// <summary>
    /// DTO para enviar XML de factura
    /// </summary>
    public class XmlFacturaDto
    {
        /// <summary>Contenido XML de la factura electrónica</summary>
        public string XmlContent { get; set; } = string.Empty;
    }

    /// <summary>
    /// Parsea un XML de factura electrónica y extrae sus datos
    /// </summary>
    [HttpPost("parsear-xml")]
    [SwaggerOperation(
        Summary = "Parsear XML de factura",
        Description = "Lee y extrae los datos de un XML de factura electrónica (DTE)")]
    [SwaggerResponse(200, "Datos extraídos de la factura", typeof(DteData))]
    [SwaggerResponse(400, "XML inválido o no parseable")]
    public ActionResult<DteData> ParsearXmlFactura([FromBody] XmlFacturaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.XmlContent))
        {
            return BadRequest(new { mensaje = "El contenido XML es requerido" });
        }

        var data = _siiService.LeerXmlFactura(dto.XmlContent);

        if (data == null)
        {
            return BadRequest(new { mensaje = "No se pudo parsear el XML. Verifique el formato." });
        }

        return Ok(data);
    }

    #endregion
}
