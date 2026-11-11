using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FactoringBackend.Core.DTOs;
using FactoringBackend.Core.Entities;
using FactoringBackend.Core.Interfaces;
using System.Diagnostics;

namespace FactoringBackend.API.Controllers
{
    /// <summary>
    /// Controlador para generación de informes
    /// Basado en F_INF1000.frm y formularios F_INF*.frm
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InformesController : ControllerBase
    {
        private readonly IInformesRepository _informesRepository;
        private readonly ILogger<InformesController> _logger;

        public InformesController(
            IInformesRepository informesRepository,
            ILogger<InformesController> logger)
        {
            _informesRepository = informesRepository;
            _logger = logger;
        }

        #region Catálogo de Informes

        /// <summary>
        /// Obtiene la lista de informes disponibles
        /// </summary>
        [HttpGet("catalogo")]
        public async Task<ActionResult<IEnumerable<DefinicionInformeDto>>> GetCatalogoInformes()
        {
            try
            {
                var informes = await _informesRepository.GetInformesDisponiblesAsync();
                var result = informes.Select(i => new DefinicionInformeDto
                {
                    Id = i.Id,
                    Codigo = i.Codigo,
                    Nombre = i.Nombre,
                    Descripcion = i.Descripcion,
                    TipoInforme = (int)i.Tipo,
                    Categoria = i.Categoria,
                    RequiereFechas = i.RequiereFechas,
                    RequiereCliente = i.RequiereCliente,
                    RequiereDeudor = i.RequiereDeudor,
                    RequiereEjecutivo = i.RequiereEjecutivo,
                    RequiereTipoDocumento = i.RequiereTipoDocumento,
                    PermiteExcel = i.PermiteExcel,
                    PermitePDF = i.PermitePDF,
                    Orden = i.Orden
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener catálogo de informes");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Anticipos Cursados (F_INF1100)

        /// <summary>
        /// Genera informe de anticipos cursados
        /// </summary>
        [HttpPost("anticipos-cursados")]
        public async Task<ActionResult<ResultadoInformeDto<AnticipoCursadoDto>>> GetAnticiposCursados(
            [FromBody] FiltroAnticiposCursadosDto filtro)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var datos = await _informesRepository.GetAnticiposCursadosAsync(
                    filtro.FechaDesde,
                    filtro.FechaHasta,
                    filtro.TipoDocumento,
                    filtro.RutCliente);

                stopwatch.Stop();

                var result = new ResultadoInformeDto<AnticipoCursadoDto>
                {
                    TipoInforme = (int)TipoInformeEnum.AnticiposCursados,
                    NombreInforme = "Anticipos Cursados",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.Select(d => new AnticipoCursadoDto
                    {
                        RutCliente = d.RutCliente,
                        DvCliente = d.DvCliente,
                        RazonSocialCliente = d.RazonSocialCliente,
                        NumeroOperacion = d.NumeroOperacion,
                        FechaOperacion = d.FechaOperacion,
                        FechaCurse = d.FechaCurse,
                        TipoDocumento = d.TipoDocumento,
                        CantidadDocumentos = d.CantidadDocumentos,
                        MontoDocumentos = d.MontoDocumentos,
                        MontoAnticipo = d.MontoAnticipo,
                        TasaOperacion = d.TasaOperacion,
                        Diferencia = d.Diferencia,
                        Comision = d.Comision,
                        Gastos = d.Gastos,
                        NombreEjecutivo = d.NombreEjecutivo,
                        NombreBanco = d.NombreBanco
                    }).ToList(),
                    TotalRegistros = datos.Count(),
                    Totales = new Dictionary<string, decimal>
                    {
                        { "TotalMontoDocumentos", datos.Sum(d => d.MontoDocumentos) },
                        { "TotalMontoAnticipo", datos.Sum(d => d.MontoAnticipo) },
                        { "TotalDiferencia", datos.Sum(d => d.Diferencia) },
                        { "TotalComision", datos.Sum(d => d.Comision) },
                        { "TotalGastos", datos.Sum(d => d.Gastos) }
                    },
                    Exitoso = true
                };

                _logger.LogInformation(
                    "Informe Anticipos Cursados generado: {Registros} registros en {Tiempo}ms",
                    result.TotalRegistros, stopwatch.ElapsedMilliseconds);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar informe de anticipos cursados");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Protestos Diarios (F_INF1200)

        /// <summary>
        /// Genera informe de protestos diarios por ejecutivo
        /// </summary>
        [HttpPost("protestos-diarios")]
        public async Task<ActionResult<ResultadoInformeDto<ProtestoDiarioDto>>> GetProtestosDiarios(
            [FromBody] FiltroProtestosDiariosDto filtro)
        {
            try
            {
                var datos = await _informesRepository.GetProtestosDiariosAsync(
                    filtro.FechaDesde,
                    filtro.FechaHasta,
                    filtro.CodigoEjecutivo);

                var result = new ResultadoInformeDto<ProtestoDiarioDto>
                {
                    TipoInforme = (int)TipoInformeEnum.ProtestosDiarios,
                    NombreInforme = "Protestos Diarios por Ejecutivo",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.Select(d => new ProtestoDiarioDto
                    {
                        FechaProtesto = d.FechaProtesto,
                        CodigoEjecutivo = d.CodigoEjecutivo,
                        NombreEjecutivo = d.NombreEjecutivo,
                        RutCliente = d.RutCliente,
                        DvCliente = d.DvCliente,
                        RazonSocialCliente = d.RazonSocialCliente,
                        RutDeudor = d.RutDeudor,
                        DvDeudor = d.DvDeudor,
                        NombreDeudor = d.NombreDeudor,
                        NumeroOperacion = d.NumeroOperacion,
                        NumeroDocumento = d.NumeroDocumento,
                        TipoDocumento = d.TipoDocumento,
                        MontoDocumento = d.MontoDocumento,
                        FechaVencimiento = d.FechaVencimiento,
                        DiasVencido = d.DiasVencido,
                        CausalProtesto = d.CausalProtesto
                    }).ToList(),
                    TotalRegistros = datos.Count(),
                    Totales = new Dictionary<string, decimal>
                    {
                        { "TotalMontoProtestado", datos.Sum(d => d.MontoDocumento) }
                    },
                    Exitoso = true
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar informe de protestos diarios");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Documentos por Deudor (F_INF1300)

        /// <summary>
        /// Genera informe de documentos por deudor
        /// </summary>
        [HttpPost("documentos-deudor")]
        public async Task<ActionResult<ResultadoInformeDto<DocumentoDeudorDto>>> GetDocumentosPorDeudor(
            [FromBody] FiltroDocumentosDeudorDto filtro)
        {
            try
            {
                var datos = await _informesRepository.GetDocumentosPorDeudorAsync(
                    filtro.RutDeudor,
                    filtro.TipoDocumento);

                var result = new ResultadoInformeDto<DocumentoDeudorDto>
                {
                    TipoInforme = (int)TipoInformeEnum.DocumentosPorDeudor,
                    NombreInforme = "Documentos Comprados por Deudor",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.Select(d => new DocumentoDeudorDto
                    {
                        RutDeudor = d.RutDeudor,
                        DvDeudor = d.DvDeudor,
                        NombreDeudor = d.NombreDeudor,
                        RutCliente = d.RutCliente,
                        DvCliente = d.DvCliente,
                        RazonSocialCliente = d.RazonSocialCliente,
                        NumeroOperacion = d.NumeroOperacion,
                        NumeroDocumento = d.NumeroDocumento,
                        TipoDocumento = d.TipoDocumento,
                        FechaOperacion = d.FechaOperacion,
                        FechaVencimiento = d.FechaVencimiento,
                        MontoDocumento = d.MontoDocumento,
                        SaldoDocumento = d.SaldoDocumento,
                        EstadoDocumento = d.EstadoDocumento,
                        DiasVencido = d.DiasVencido
                    }).ToList(),
                    TotalRegistros = datos.Count(),
                    Totales = new Dictionary<string, decimal>
                    {
                        { "TotalMontoDocumentos", datos.Sum(d => d.MontoDocumento) },
                        { "TotalSaldoDocumentos", datos.Sum(d => d.SaldoDocumento) }
                    },
                    Exitoso = true
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar informe de documentos por deudor");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Cartera Cliente (F_INF1400)

        /// <summary>
        /// Genera informe de cartera por cliente
        /// </summary>
        [HttpPost("cartera-cliente")]
        public async Task<ActionResult<CarteraClienteDto>> GetCarteraCliente(
            [FromBody] FiltroCarteraClienteDto filtro)
        {
            try
            {
                var cartera = await _informesRepository.GetCarteraClienteAsync(
                    filtro.RutCliente,
                    filtro.TipoDocumento);

                if (cartera == null)
                {
                    return NotFound(new { message = "Cliente no encontrado o sin cartera" });
                }

                var result = new CarteraClienteDto
                {
                    RutCliente = cartera.RutCliente,
                    DvCliente = cartera.DvCliente,
                    RazonSocialCliente = cartera.RazonSocialCliente,
                    NombreEjecutivo = cartera.NombreEjecutivo,
                    TipoDocumento = cartera.TipoDocumento,
                    TotalDocumentos = cartera.TotalDocumentos,
                    MontoVigente = cartera.MontoVigente,
                    MontoMora = cartera.MontoMora,
                    MontoTotal = cartera.MontoTotal,
                    PorcentajeConcentracion = cartera.PorcentajeConcentracion,
                    Detalle = cartera.Detalle.Select(d => new DetalleCarteraDto
                    {
                        NumeroOperacion = d.NumeroOperacion,
                        NumeroDocumento = d.NumeroDocumento,
                        TipoDocumento = d.TipoDocumento,
                        RutDeudor = d.RutDeudor,
                        NombreDeudor = d.NombreDeudor,
                        FechaOperacion = d.FechaOperacion,
                        FechaVencimiento = d.FechaVencimiento,
                        MontoOriginal = d.MontoOriginal,
                        Saldo = d.Saldo,
                        DiasVencido = d.DiasVencido,
                        Estado = d.Estado
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar informe de cartera cliente");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Devengamiento (F_INF1500)

        /// <summary>
        /// Genera informe de detalle devengamiento
        /// </summary>
        [HttpPost("devengamiento")]
        public async Task<ActionResult<ResultadoInformeDto<DevengamientoDto>>> GetDevengamiento(
            [FromBody] FiltroDevengamientoDto filtro)
        {
            try
            {
                var datos = await _informesRepository.GetDevengamientoAsync(
                    filtro.FechaCorte,
                    filtro.RutCliente,
                    filtro.CentroCosto);

                var result = new ResultadoInformeDto<DevengamientoDto>
                {
                    TipoInforme = (int)TipoInformeEnum.DetalleDevengamiento,
                    NombreInforme = "Detalle Devengamiento",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.Select(d => new DevengamientoDto
                    {
                        FechaCorte = d.FechaCorte,
                        RutCliente = d.RutCliente,
                        DvCliente = d.DvCliente,
                        RazonSocialCliente = d.RazonSocialCliente,
                        NumeroOperacion = d.NumeroOperacion,
                        NumeroDocumento = d.NumeroDocumento,
                        FechaOperacion = d.FechaOperacion,
                        FechaVencimiento = d.FechaVencimiento,
                        PlazoTotal = d.PlazoTotal,
                        DiasDevengados = d.DiasDevengados,
                        DiasPorDevengar = d.DiasPorDevengar,
                        MontoDocumento = d.MontoDocumento,
                        TasaOperacion = d.TasaOperacion,
                        InteresTotal = d.InteresTotal,
                        InteresDevengado = d.InteresDevengado,
                        InteresPorDevengar = d.InteresPorDevengar,
                        DescripcionCentroCosto = d.DescripcionCentroCosto
                    }).ToList(),
                    TotalRegistros = datos.Count(),
                    Totales = new Dictionary<string, decimal>
                    {
                        { "TotalMontoDocumentos", datos.Sum(d => d.MontoDocumento) },
                        { "TotalInteresTotal", datos.Sum(d => d.InteresTotal) },
                        { "TotalInteresDevengado", datos.Sum(d => d.InteresDevengado) },
                        { "TotalInteresPorDevengar", datos.Sum(d => d.InteresPorDevengar) }
                    },
                    Exitoso = true
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar informe de devengamiento");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Adelantos (F_INF1600)

        /// <summary>
        /// Genera informe de adelantos
        /// </summary>
        [HttpPost("adelantos")]
        public async Task<ActionResult<ResultadoInformeDto<AdelantoDto>>> GetAdelantos(
            [FromBody] FiltroAdelantosDto filtro)
        {
            try
            {
                var datos = await _informesRepository.GetAdelantosAsync(
                    filtro.FechaDesde,
                    filtro.FechaHasta,
                    filtro.RutCliente);

                var result = new ResultadoInformeDto<AdelantoDto>
                {
                    TipoInforme = (int)TipoInformeEnum.Adelantos,
                    NombreInforme = "Informe de Adelantos",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.Select(d => new AdelantoDto
                    {
                        RutCliente = d.RutCliente,
                        DvCliente = d.DvCliente,
                        RazonSocialCliente = d.RazonSocialCliente,
                        NumeroOperacion = d.NumeroOperacion,
                        FechaOperacion = d.FechaOperacion,
                        FechaAdelanto = d.FechaAdelanto,
                        MontoAdelanto = d.MontoAdelanto,
                        MontoDocumentos = d.MontoDocumentos,
                        PorcentajeAdelanto = d.PorcentajeAdelanto,
                        Estado = d.Estado,
                        FechaRecuperacion = d.FechaRecuperacion,
                        MontoRecuperado = d.MontoRecuperado
                    }).ToList(),
                    TotalRegistros = datos.Count(),
                    Totales = new Dictionary<string, decimal>
                    {
                        { "TotalMontoAdelanto", datos.Sum(d => d.MontoAdelanto) },
                        { "TotalMontoDocumentos", datos.Sum(d => d.MontoDocumentos) }
                    },
                    Exitoso = true
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar informe de adelantos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Cupo Cliente (F_INF1700)

        /// <summary>
        /// Genera informe de cupo por cliente
        /// </summary>
        [HttpPost("cupo-cliente")]
        public async Task<ActionResult<ResultadoInformeDto<CupoClienteDto>>> GetCupoCliente(
            [FromBody] FiltroCupoClienteDto filtro)
        {
            try
            {
                var datos = await _informesRepository.GetCupoClientesAsync(
                    filtro.CodigoEjecutivo,
                    filtro.RutCliente);

                var result = new ResultadoInformeDto<CupoClienteDto>
                {
                    TipoInforme = (int)TipoInformeEnum.CupoCliente,
                    NombreInforme = "Cupo Cliente por Ejecutivo",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.Select(d => new CupoClienteDto
                    {
                        CodigoEjecutivo = d.CodigoEjecutivo,
                        NombreEjecutivo = d.NombreEjecutivo,
                        RutCliente = d.RutCliente,
                        DvCliente = d.DvCliente,
                        RazonSocialCliente = d.RazonSocialCliente,
                        LineaAprobada = d.LineaAprobada,
                        MontoUtilizado = d.MontoUtilizado,
                        MontoDisponible = d.MontoDisponible,
                        PorcentajeUtilizacion = d.PorcentajeUtilizacion,
                        FechaVencimientoLinea = d.FechaVencimientoLinea,
                        EstadoLinea = d.EstadoLinea
                    }).ToList(),
                    TotalRegistros = datos.Count(),
                    Totales = new Dictionary<string, decimal>
                    {
                        { "TotalLineaAprobada", datos.Sum(d => d.LineaAprobada) },
                        { "TotalMontoUtilizado", datos.Sum(d => d.MontoUtilizado) },
                        { "TotalMontoDisponible", datos.Sum(d => d.MontoDisponible) }
                    },
                    Exitoso = true
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar informe de cupo cliente");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Volumen Operaciones (F_INF1800)

        /// <summary>
        /// Genera informe de volumen de operaciones
        /// </summary>
        [HttpPost("volumen-operaciones")]
        public async Task<ActionResult<ResultadoInformeDto<VolumenOperacionesDto>>> GetVolumenOperaciones(
            [FromBody] FiltroVolumenOperacionesDto filtro)
        {
            try
            {
                var datos = await _informesRepository.GetVolumenOperacionesAsync(
                    filtro.Anio,
                    filtro.Mes,
                    filtro.CodigoEjecutivo);

                var result = new ResultadoInformeDto<VolumenOperacionesDto>
                {
                    TipoInforme = (int)TipoInformeEnum.VolumenOperaciones,
                    NombreInforme = "Volumen de Operaciones",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.Select(d => new VolumenOperacionesDto
                    {
                        Anio = d.Anio,
                        Mes = d.Mes,
                        NombreMes = d.NombreMes,
                        CodigoEjecutivo = d.CodigoEjecutivo,
                        NombreEjecutivo = d.NombreEjecutivo,
                        CantidadOperaciones = d.CantidadOperaciones,
                        CantidadDocumentos = d.CantidadDocumentos,
                        MontoOperado = d.MontoOperado,
                        MontoAnticipado = d.MontoAnticipado,
                        ComisionesGeneradas = d.ComisionesGeneradas,
                        InteresesGenerados = d.InteresesGenerados,
                        ClientesNuevos = d.ClientesNuevos,
                        ClientesActivos = d.ClientesActivos
                    }).ToList(),
                    TotalRegistros = datos.Count(),
                    Totales = new Dictionary<string, decimal>
                    {
                        { "TotalOperaciones", datos.Sum(d => d.CantidadOperaciones) },
                        { "TotalDocumentos", datos.Sum(d => d.CantidadDocumentos) },
                        { "TotalMontoOperado", datos.Sum(d => d.MontoOperado) },
                        { "TotalMontoAnticipado", datos.Sum(d => d.MontoAnticipado) },
                        { "TotalComisiones", datos.Sum(d => d.ComisionesGeneradas) },
                        { "TotalIntereses", datos.Sum(d => d.InteresesGenerados) }
                    },
                    Exitoso = true
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar informe de volumen operaciones");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Excedentes (F_INF1900)

        /// <summary>
        /// Genera informe de excedentes
        /// </summary>
        [HttpPost("excedentes")]
        public async Task<ActionResult<ResultadoInformeDto<ExcedenteInformeDto>>> GetExcedentes(
            [FromBody] FiltroExcedentesDto filtro)
        {
            try
            {
                var datos = await _informesRepository.GetExcedentesAsync(
                    filtro.FechaDesde,
                    filtro.FechaHasta,
                    filtro.RutCliente,
                    filtro.Estado);

                var result = new ResultadoInformeDto<ExcedenteInformeDto>
                {
                    TipoInforme = (int)TipoInformeEnum.Excedentes,
                    NombreInforme = "Detalle de Excedentes",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.Select(d => new ExcedenteInformeDto
                    {
                        RutCliente = d.RutCliente,
                        DvCliente = d.DvCliente,
                        RazonSocialCliente = d.RazonSocialCliente,
                        NumeroOperacion = d.NumeroOperacion,
                        FechaOperacion = d.FechaOperacion,
                        MontoExcedente = d.MontoExcedente,
                        FechaGeneracion = d.FechaGeneracion,
                        FechaDevolucion = d.FechaDevolucion,
                        MontoDevuelto = d.MontoDevuelto,
                        Estado = d.Estado,
                        FormaPago = d.FormaPago,
                        NombreEjecutivo = d.NombreEjecutivo
                    }).ToList(),
                    TotalRegistros = datos.Count(),
                    Totales = new Dictionary<string, decimal>
                    {
                        { "TotalExcedentes", datos.Sum(d => d.MontoExcedente) },
                        { "TotalDevuelto", datos.Where(d => d.MontoDevuelto.HasValue).Sum(d => d.MontoDevuelto ?? 0) }
                    },
                    Exitoso = true
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar informe de excedentes");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Concentración Terceros (F_INF2000)

        /// <summary>
        /// Genera informe de concentración de terceros
        /// </summary>
        [HttpPost("concentracion-terceros")]
        public async Task<ActionResult<ResultadoInformeDto<ConcentracionTercerosDto>>> GetConcentracionTerceros(
            [FromBody] FiltroConcentracionTercerosDto filtro)
        {
            try
            {
                var datos = await _informesRepository.GetConcentracionTercerosAsync(
                    filtro.FechaCorte,
                    filtro.RutDeudor,
                    filtro.PorcentajeMinimoConcentracion);

                var result = new ResultadoInformeDto<ConcentracionTercerosDto>
                {
                    TipoInforme = (int)TipoInformeEnum.ConcentracionTerceros,
                    NombreInforme = "Concentración de Terceros",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.Select(d => new ConcentracionTercerosDto
                    {
                        RutDeudor = d.RutDeudor,
                        DvDeudor = d.DvDeudor,
                        NombreDeudor = d.NombreDeudor,
                        CantidadClientes = d.CantidadClientes,
                        CantidadDocumentos = d.CantidadDocumentos,
                        MontoTotal = d.MontoTotal,
                        MontoVigente = d.MontoVigente,
                        MontoVencido = d.MontoVencido,
                        PorcentajeConcentracion = d.PorcentajeConcentracion,
                        CustodiaAsignada = d.CustodiaAsignada
                    }).ToList(),
                    TotalRegistros = datos.Count(),
                    Totales = new Dictionary<string, decimal>
                    {
                        { "TotalMonto", datos.Sum(d => d.MontoTotal) },
                        { "TotalMontoVigente", datos.Sum(d => d.MontoVigente) },
                        { "TotalMontoVencido", datos.Sum(d => d.MontoVencido) }
                    },
                    Exitoso = true
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar informe de concentración terceros");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Resumen Dashboard

        /// <summary>
        /// Obtiene resumen general para dashboard de informes
        /// </summary>
        [HttpGet("resumen")]
        public async Task<ActionResult<ResumenInformesDto>> GetResumen([FromQuery] DateTime? fechaCorte = null)
        {
            try
            {
                var resumen = await _informesRepository.GetResumenGeneralAsync(fechaCorte);

                var result = new ResumenInformesDto
                {
                    FechaCorte = resumen.FechaCorte,
                    TotalOperacionesMes = resumen.TotalOperacionesMes,
                    MontoOperadoMes = resumen.MontoOperadoMes,
                    TotalClientesActivos = resumen.TotalClientesActivos,
                    CarteraVigente = resumen.CarteraVigente,
                    CarteraVencida = resumen.CarteraVencida,
                    PorcentajeMorosidad = resumen.PorcentajeMorosidad,
                    DocumentosProtestados = resumen.DocumentosProtestados,
                    MontoProtestado = resumen.MontoProtestado,
                    ExcedentesPendientes = resumen.ExcedentesPendientes
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de informes");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Historial

        /// <summary>
        /// Obtiene historial de informes generados
        /// </summary>
        [HttpGet("historial")]
        public async Task<ActionResult<IEnumerable<HistorialInformeDto>>> GetHistorial(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] string? usuario = null,
            [FromQuery] int? tipoInforme = null)
        {
            try
            {
                var historial = await _informesRepository.GetHistorialAsync(
                    fechaDesde, fechaHasta, usuario, tipoInforme);

                var result = historial.Select(h => new HistorialInformeDto
                {
                    Id = h.Id,
                    TipoInforme = (int)h.TipoInforme,
                    NombreInforme = h.NombreInforme,
                    FechaGeneracion = h.FechaGeneracion,
                    UsuarioGeneracion = h.UsuarioGeneracion,
                    Formato = (int)h.Formato,
                    FormatoDescripcion = h.Formato switch
                    {
                        FormatoInformeEnum.Pantalla => "Pantalla",
                        FormatoInformeEnum.Excel => "Excel",
                        FormatoInformeEnum.PDF => "PDF",
                        _ => "Otro"
                    },
                    TotalRegistros = h.TotalRegistros,
                    TiempoEjecucionMs = h.TiempoEjecucionMs,
                    Exitoso = h.Exitoso
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de informes");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion
    }
}
