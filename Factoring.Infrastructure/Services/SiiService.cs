using Factoring.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace Factoring.Infrastructure.Services;

/// <summary>
/// Servicio de integracion con el SII (Servicio de Impuestos Internos) de Chile
/// Funcionalidades:
/// - Consulta de contribuyentes (validacion RUT, razon social)
/// - Cesion electronica de facturas (AEC)
/// - Consulta de estado de cesion
/// </summary>
public class SiiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SiiService> _logger;
    private readonly string _ambiente; // "certificacion" o "produccion"
    private readonly string _rutEmpresa;
    private readonly string _certificadoPath;

    // URLs del SII
    private const string URL_CONSULTA_CONTRIBUYENTE_CERT = "https://maullin.sii.cl/cgi_AUT2000/CAutInwor498.cgi";
    private const string URL_CONSULTA_CONTRIBUYENTE_PROD = "https://palena.sii.cl/cgi_AUT2000/CAutInwor498.cgi";
    private const string URL_CESION_CERT = "https://maullin.sii.cl/cgi_rtc/RTC/RTCAnotEnvioDTE.cgi";
    private const string URL_CESION_PROD = "https://palena.sii.cl/cgi_rtc/RTC/RTCAnotEnvioDTE.cgi";
    private const string URL_ESTADO_CESION_CERT = "https://maullin.sii.cl/cgi_rtc/RTC/RTCConsultaCesion.cgi";
    private const string URL_ESTADO_CESION_PROD = "https://palena.sii.cl/cgi_rtc/RTC/RTCConsultaCesion.cgi";

    public SiiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SiiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _ambiente = configuration["Sii:Ambiente"] ?? "certificacion";
        _rutEmpresa = configuration["Sii:RutEmpresa"] ?? "";
        _certificadoPath = configuration["Sii:CertificadoPath"] ?? "";

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/xml"));
    }

    #region Consulta de Contribuyentes

    /// <summary>
    /// Resultado de consulta de contribuyente al SII
    /// </summary>
    public class ContribuyenteResult
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
        public string? Rut { get; set; }
        public string? RazonSocial { get; set; }
        public string? Giro { get; set; }
        public string? ActividadEconomica { get; set; }
        public string? Direccion { get; set; }
        public string? Comuna { get; set; }
        public bool EsContribuyente { get; set; }
        public DateTime? FechaInicioActividades { get; set; }
        public List<string> ActividadesSecundarias { get; set; } = new();
    }

    /// <summary>
    /// Consulta informacion de un contribuyente en el SII
    /// </summary>
    public async Task<ContribuyenteResult> ConsultarContribuyenteAsync(long rut, string digitoVerificador)
    {
        var result = new ContribuyenteResult { Rut = $"{rut}-{digitoVerificador}" };

        try
        {
            _logger.LogInformation("Consultando contribuyente {Rut} en SII", result.Rut);

            // Validar formato RUT
            if (!ValidarRut(rut, digitoVerificador))
            {
                result.Mensaje = "RUT inválido";
                return result;
            }

            // En ambiente de desarrollo, simular respuesta
            if (_ambiente == "desarrollo" || string.IsNullOrEmpty(_certificadoPath))
            {
                return SimularConsultaContribuyente(rut, digitoVerificador);
            }

            var url = _ambiente == "produccion"
                ? URL_CONSULTA_CONTRIBUYENTE_PROD
                : URL_CONSULTA_CONTRIBUYENTE_CERT;

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("RUT", rut.ToString()),
                new KeyValuePair<string, string>("DV", digitoVerificador),
                new KeyValuePair<string, string>("PRG", "STC"),
                new KeyValuePair<string, string>("OPC", "NOR")
            });

            var response = await _httpClient.PostAsync(url, formData);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                result = ParsearRespuestaContribuyente(content, rut, digitoVerificador);
            }
            else
            {
                result.Mensaje = $"Error en consulta SII: {response.StatusCode}";
                _logger.LogWarning("Error consultando SII: {StatusCode} - {Content}",
                    response.StatusCode, content);
            }
        }
        catch (Exception ex)
        {
            result.Mensaje = $"Error de conexión con SII: {ex.Message}";
            _logger.LogError(ex, "Error consultando contribuyente {Rut} en SII", result.Rut);
        }

        return result;
    }

    /// <summary>
    /// Valida el dígito verificador de un RUT chileno
    /// </summary>
    public bool ValidarRut(long rut, string digitoVerificador)
    {
        if (rut < 1000000 || rut > 99999999) return false;

        int suma = 0;
        int multiplicador = 2;
        string rutString = rut.ToString();

        for (int i = rutString.Length - 1; i >= 0; i--)
        {
            suma += int.Parse(rutString[i].ToString()) * multiplicador;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
        }

        int resto = suma % 11;
        string dvCalculado = resto == 0 ? "0" : resto == 1 ? "K" : (11 - resto).ToString();

        return dvCalculado.Equals(digitoVerificador.ToUpper());
    }

    /// <summary>
    /// Calcula el dígito verificador de un RUT
    /// </summary>
    public string CalcularDigitoVerificador(long rut)
    {
        int suma = 0;
        int multiplicador = 2;
        string rutString = rut.ToString();

        for (int i = rutString.Length - 1; i >= 0; i--)
        {
            suma += int.Parse(rutString[i].ToString()) * multiplicador;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
        }

        int resto = suma % 11;
        return resto == 0 ? "0" : resto == 1 ? "K" : (11 - resto).ToString();
    }

    private ContribuyenteResult SimularConsultaContribuyente(long rut, string dv)
    {
        // Simulación para desarrollo/testing
        var contribuyentes = new Dictionary<long, (string razon, string giro)>
        {
            { 76123456, ("EMPRESA DEMO S.A.", "Comercio al por mayor") },
            { 76987654, ("IMPORTADORA CHILE LTDA", "Importación de productos") },
            { 96123456, ("SUPERMERCADOS GRANDE S.A.", "Comercio minorista") },
            { 96789012, ("TIENDAS RETAIL CHILE S.A.", "Retail") }
        };

        if (contribuyentes.TryGetValue(rut, out var data))
        {
            return new ContribuyenteResult
            {
                Exito = true,
                Rut = $"{rut}-{dv}",
                RazonSocial = data.razon,
                Giro = data.giro,
                EsContribuyente = true,
                Direccion = "Dirección de prueba 123",
                Comuna = "Santiago",
                FechaInicioActividades = DateTime.Today.AddYears(-5)
            };
        }

        return new ContribuyenteResult
        {
            Exito = true,
            Rut = $"{rut}-{dv}",
            RazonSocial = $"CONTRIBUYENTE {rut}",
            Giro = "Actividad general",
            EsContribuyente = true
        };
    }

    private ContribuyenteResult ParsearRespuestaContribuyente(string xmlContent, long rut, string dv)
    {
        var result = new ContribuyenteResult { Rut = $"{rut}-{dv}" };

        try
        {
            var doc = XDocument.Parse(xmlContent);
            var body = doc.Root?.Element("BODY");

            if (body != null)
            {
                result.RazonSocial = body.Element("NOMBRE")?.Value;
                result.Giro = body.Element("GIRO")?.Value;
                result.Direccion = body.Element("DIRECCION")?.Value;
                result.Comuna = body.Element("COMUNA")?.Value;
                result.EsContribuyente = body.Element("ESTADO")?.Value == "ACTIVO";
                result.Exito = true;

                if (DateTime.TryParse(body.Element("FEC_INICIO")?.Value, out var fechaInicio))
                {
                    result.FechaInicioActividades = fechaInicio;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parseando respuesta SII");
            result.Mensaje = "Error al procesar respuesta del SII";
        }

        return result;
    }

    #endregion

    #region Cesión Electrónica de Facturas

    /// <summary>
    /// Resultado de cesión electrónica
    /// </summary>
    public class CesionResult
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
        public string? TrackId { get; set; }
        public string? CodigoCesion { get; set; }
        public DateTime? FechaCesion { get; set; }
        public string? EstadoCesion { get; set; }
    }

    /// <summary>
    /// Datos para realizar cesión electrónica
    /// </summary>
    public class CesionRequest
    {
        public long RutCedente { get; set; }
        public string DvCedente { get; set; } = string.Empty;
        public long RutCesionario { get; set; }
        public string DvCesionario { get; set; } = string.Empty;
        public long RutDeudor { get; set; }
        public string DvDeudor { get; set; } = string.Empty;
        public int TipoDocumento { get; set; } // 33=Factura, 34=Factura Exenta
        public long Folio { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal MontoTotal { get; set; }
        public string? XmlFactura { get; set; }
        public string? EmailCedente { get; set; }
        public string? EmailCesionario { get; set; }
        public string? EmailDeudor { get; set; }
    }

    /// <summary>
    /// Registra cesión electrónica de factura en el SII (AEC)
    /// </summary>
    public async Task<CesionResult> RegistrarCesionAsync(CesionRequest request)
    {
        var result = new CesionResult();

        try
        {
            _logger.LogInformation(
                "Registrando cesión: Cedente {RutCedente}, Cesionario {RutCesionario}, Folio {Folio}",
                request.RutCedente, request.RutCesionario, request.Folio);

            // En desarrollo, simular
            if (_ambiente == "desarrollo" || string.IsNullOrEmpty(_certificadoPath))
            {
                return SimularCesion(request);
            }

            // Construir XML AEC
            var xmlAec = ConstruirXmlAec(request);

            var url = _ambiente == "produccion" ? URL_CESION_PROD : URL_CESION_CERT;

            var content = new StringContent(xmlAec, Encoding.UTF8, "application/xml");
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                result = ParsearRespuestaCesion(responseContent);
            }
            else
            {
                result.Mensaje = $"Error en cesión SII: {response.StatusCode}";
                _logger.LogWarning("Error en cesión SII: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);
            }
        }
        catch (Exception ex)
        {
            result.Mensaje = $"Error al registrar cesión: {ex.Message}";
            _logger.LogError(ex, "Error registrando cesión en SII");
        }

        return result;
    }

    /// <summary>
    /// Consulta estado de una cesión
    /// </summary>
    public async Task<CesionResult> ConsultarEstadoCesionAsync(
        long rutCedente, string dvCedente,
        int tipoDocumento, long folio)
    {
        var result = new CesionResult();

        try
        {
            _logger.LogInformation(
                "Consultando estado cesión: Cedente {RutCedente}, TipoDoc {TipoDoc}, Folio {Folio}",
                rutCedente, tipoDocumento, folio);

            if (_ambiente == "desarrollo" || string.IsNullOrEmpty(_certificadoPath))
            {
                return new CesionResult
                {
                    Exito = true,
                    EstadoCesion = "CEDIDO",
                    CodigoCesion = $"CES-{folio}",
                    FechaCesion = DateTime.Today
                };
            }

            var url = _ambiente == "produccion" ? URL_ESTADO_CESION_PROD : URL_ESTADO_CESION_CERT;

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("RUT_CEDENTE", rutCedente.ToString()),
                new KeyValuePair<string, string>("DV_CEDENTE", dvCedente),
                new KeyValuePair<string, string>("TIPO_DOC", tipoDocumento.ToString()),
                new KeyValuePair<string, string>("FOLIO", folio.ToString())
            });

            var response = await _httpClient.PostAsync(url, formData);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                result = ParsearEstadoCesion(content);
            }
            else
            {
                result.Mensaje = $"Error consultando cesión: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            result.Mensaje = $"Error al consultar cesión: {ex.Message}";
            _logger.LogError(ex, "Error consultando estado cesión");
        }

        return result;
    }

    /// <summary>
    /// Verifica si una factura ya está cedida a otro
    /// </summary>
    public async Task<(bool YaCedida, string? CesionarioActual)> VerificarFacturaCedidaAsync(
        long rutEmisor, string dvEmisor,
        int tipoDocumento, long folio)
    {
        var estado = await ConsultarEstadoCesionAsync(rutEmisor, dvEmisor, tipoDocumento, folio);

        if (estado.Exito && estado.EstadoCesion == "CEDIDO")
        {
            return (true, estado.CodigoCesion);
        }

        return (false, null);
    }

    private CesionResult SimularCesion(CesionRequest request)
    {
        return new CesionResult
        {
            Exito = true,
            TrackId = Guid.NewGuid().ToString("N")[..10].ToUpper(),
            CodigoCesion = $"CES-{DateTime.Now:yyyyMMdd}-{request.Folio}",
            FechaCesion = DateTime.Now,
            EstadoCesion = "ACEPTADO",
            Mensaje = "Cesión registrada exitosamente (simulación)"
        };
    }

    private string ConstruirXmlAec(CesionRequest request)
    {
        // Construir XML de Archivo Electrónico de Cesión (AEC)
        var xml = new XDocument(
            new XDeclaration("1.0", "ISO-8859-1", null),
            new XElement("AEC",
                new XAttribute("version", "1.0"),
                new XElement("DocumentoAEC",
                    new XElement("Caratula",
                        new XElement("RutCedente", $"{request.RutCedente}-{request.DvCedente}"),
                        new XElement("RutCesionario", $"{request.RutCesionario}-{request.DvCesionario}"),
                        new XElement("NmbContacto", "Sistema Factoring"),
                        new XElement("MailContacto", request.EmailCedente ?? "factoring@empresa.cl"),
                        new XElement("TmstFirmaEnvio", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"))
                    ),
                    new XElement("Cesiones",
                        new XElement("DTECedido",
                            new XElement("DocumentoDTECedido",
                                new XElement("TipoDTE", request.TipoDocumento),
                                new XElement("Folio", request.Folio),
                                new XElement("FchEmis", request.FechaEmision.ToString("yyyy-MM-dd")),
                                new XElement("RUTEmisor", $"{request.RutCedente}-{request.DvCedente}"),
                                new XElement("RUTReceptor", $"{request.RutDeudor}-{request.DvDeudor}"),
                                new XElement("MntTotal", request.MontoTotal)
                            )
                        ),
                        new XElement("Cesion",
                            new XElement("DocumentoCesion",
                                new XElement("SeqCesion", 1),
                                new XElement("IdDTE",
                                    new XElement("TipoDTE", request.TipoDocumento),
                                    new XElement("Folio", request.Folio),
                                    new XElement("RUTEmisor", $"{request.RutCedente}-{request.DvCedente}")
                                ),
                                new XElement("Cedente",
                                    new XElement("RUT", $"{request.RutCedente}-{request.DvCedente}"),
                                    new XElement("eMail", request.EmailCedente ?? "cedente@empresa.cl")
                                ),
                                new XElement("Cesionario",
                                    new XElement("RUT", $"{request.RutCesionario}-{request.DvCesionario}"),
                                    new XElement("eMail", request.EmailCesionario ?? "cesionario@empresa.cl")
                                ),
                                new XElement("MontoCesion", request.MontoTotal),
                                new XElement("TmstCesion", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"))
                            )
                        )
                    )
                )
            )
        );

        return xml.ToString();
    }

    private CesionResult ParsearRespuestaCesion(string xmlContent)
    {
        var result = new CesionResult();

        try
        {
            var doc = XDocument.Parse(xmlContent);
            var respuesta = doc.Root;

            result.TrackId = respuesta?.Element("TRACKID")?.Value;
            result.EstadoCesion = respuesta?.Element("ESTADO")?.Value;

            if (result.EstadoCesion == "ACEPTADO" || result.EstadoCesion == "OK")
            {
                result.Exito = true;
                result.FechaCesion = DateTime.Now;
                result.CodigoCesion = result.TrackId;
            }
            else
            {
                result.Mensaje = respuesta?.Element("GLOSA")?.Value ?? "Error en cesión";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parseando respuesta cesión");
            result.Mensaje = "Error procesando respuesta del SII";
        }

        return result;
    }

    private CesionResult ParsearEstadoCesion(string xmlContent)
    {
        var result = new CesionResult();

        try
        {
            var doc = XDocument.Parse(xmlContent);
            var respuesta = doc.Root;

            result.EstadoCesion = respuesta?.Element("ESTADO")?.Value;
            result.CodigoCesion = respuesta?.Element("CODIGO")?.Value;
            result.Exito = true;

            if (DateTime.TryParse(respuesta?.Element("FECHA_CESION")?.Value, out var fecha))
            {
                result.FechaCesion = fecha;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parseando estado cesión");
            result.Mensaje = "Error procesando respuesta del SII";
        }

        return result;
    }

    #endregion

    #region Lectura de XML de Facturas

    /// <summary>
    /// Datos extraídos de un DTE (Documento Tributario Electrónico)
    /// </summary>
    public class DteData
    {
        public int TipoDte { get; set; }
        public long Folio { get; set; }
        public DateTime FechaEmision { get; set; }
        public long RutEmisor { get; set; }
        public string DvEmisor { get; set; } = string.Empty;
        public string RazonSocialEmisor { get; set; } = string.Empty;
        public long RutReceptor { get; set; }
        public string DvReceptor { get; set; } = string.Empty;
        public string RazonSocialReceptor { get; set; } = string.Empty;
        public decimal MontoNeto { get; set; }
        public decimal MontoIva { get; set; }
        public decimal MontoTotal { get; set; }
        public List<DteDetalle> Detalles { get; set; } = new();
    }

    public class DteDetalle
    {
        public int NumeroLinea { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal MontoItem { get; set; }
    }

    /// <summary>
    /// Lee y parsea un XML de factura electrónica
    /// </summary>
    public DteData? LeerXmlFactura(string xmlContent)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);
            var dte = doc.Root?.Element("SetDTE")?.Element("DTE")
                      ?? doc.Root?.Element("DTE")
                      ?? doc.Root;

            if (dte == null) return null;

            var documento = dte.Element("Documento");
            var encabezado = documento?.Element("Encabezado");
            var idDoc = encabezado?.Element("IdDoc");
            var emisor = encabezado?.Element("Emisor");
            var receptor = encabezado?.Element("Receptor");
            var totales = encabezado?.Element("Totales");

            var data = new DteData
            {
                TipoDte = int.Parse(idDoc?.Element("TipoDTE")?.Value ?? "0"),
                Folio = long.Parse(idDoc?.Element("Folio")?.Value ?? "0"),
                RazonSocialEmisor = emisor?.Element("RznSoc")?.Value ?? "",
                RazonSocialReceptor = receptor?.Element("RznSocRecep")?.Value ?? "",
                MontoNeto = decimal.Parse(totales?.Element("MntNeto")?.Value ?? "0"),
                MontoIva = decimal.Parse(totales?.Element("IVA")?.Value ?? "0"),
                MontoTotal = decimal.Parse(totales?.Element("MntTotal")?.Value ?? "0")
            };

            // Parsear RUT emisor
            var rutEmisor = emisor?.Element("RUTEmisor")?.Value ?? "";
            if (rutEmisor.Contains("-"))
            {
                var partes = rutEmisor.Split('-');
                data.RutEmisor = long.Parse(partes[0].Replace(".", ""));
                data.DvEmisor = partes[1];
            }

            // Parsear RUT receptor
            var rutReceptor = receptor?.Element("RUTRecep")?.Value ?? "";
            if (rutReceptor.Contains("-"))
            {
                var partes = rutReceptor.Split('-');
                data.RutReceptor = long.Parse(partes[0].Replace(".", ""));
                data.DvReceptor = partes[1];
            }

            // Parsear fecha
            if (DateTime.TryParse(idDoc?.Element("FchEmis")?.Value, out var fecha))
            {
                data.FechaEmision = fecha;
            }

            // Parsear detalles
            var detalles = documento?.Elements("Detalle");
            if (detalles != null)
            {
                int linea = 1;
                foreach (var det in detalles)
                {
                    data.Detalles.Add(new DteDetalle
                    {
                        NumeroLinea = linea++,
                        Descripcion = det.Element("NmbItem")?.Value ?? "",
                        Cantidad = decimal.Parse(det.Element("QtyItem")?.Value ?? "1"),
                        PrecioUnitario = decimal.Parse(det.Element("PrcItem")?.Value ?? "0"),
                        MontoItem = decimal.Parse(det.Element("MontoItem")?.Value ?? "0")
                    });
                }
            }

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parseando XML de factura");
            return null;
        }
    }

    #endregion
}
