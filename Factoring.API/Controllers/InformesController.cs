using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;

namespace Factoring.API.Controllers
{
    /// <summary>
    /// Controlador para generación de informes
    /// Migrado de F_INF*.frm de VB6
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InformesController : ControllerBase
    {
        private readonly IInformesRepository _informesRepository;

        public InformesController(IInformesRepository informesRepository)
        {
            _informesRepository = informesRepository;
        }

        #region Catálogo de Informes

        /// <summary>
        /// Obtiene el catálogo de informes disponibles
        /// GET: api/informes/catalogo
        /// </summary>
        [HttpGet("catalogo")]
        public async Task<ActionResult<IEnumerable<DefinicionInforme>>> GetCatalogo()
        {
            try
            {
                var informes = await _informesRepository.GetInformesDisponiblesAsync();
                return Ok(informes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener catálogo de informes: {ex.Message}");
            }
        }

        #endregion

        #region Anticipos Cursados (F_INF1100)

        /// <summary>
        /// Genera informe de anticipos cursados
        /// POST: api/informes/anticipos-cursados
        /// </summary>
        [HttpPost("anticipos-cursados")]
        public async Task<ActionResult<ResultadoInforme<InformeAnticipoCursado>>> GetAnticiposCursados(
            [FromBody] FiltroAnticiposRequest filtro)
        {
            try
            {
                var datos = await _informesRepository.GetAnticiposCursadosAsync(
                    filtro.FechaDesde,
                    filtro.FechaHasta,
                    filtro.TipoDocumento,
                    filtro.RutCliente);

                var resultado = new ResultadoInforme<InformeAnticipoCursado>
                {
                    TipoInforme = TipoInformeEnum.AnticiposCursados,
                    NombreInforme = "Anticipos Cursados",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.ToList(),
                    TotalRegistros = datos.Count(),
                    Exitoso = true,
                    Totales = new Dictionary<string, decimal>
                    {
                        ["MontoDocumentos"] = datos.Sum(x => x.MontoDocumentos),
                        ["MontoAnticipo"] = datos.Sum(x => x.MontoAnticipo),
                        ["Comision"] = datos.Sum(x => x.Comision),
                        ["Gastos"] = datos.Sum(x => x.Gastos),
                        ["Diferencia"] = datos.Sum(x => x.Diferencia)
                    }
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInforme<InformeAnticipoCursado>
                {
                    Exitoso = false,
                    MensajeError = $"Error al generar informe: {ex.Message}"
                });
            }
        }

        #endregion

        #region Protestos Diarios (F_INF1200)

        /// <summary>
        /// Genera informe de protestos diarios
        /// POST: api/informes/protestos-diarios
        /// </summary>
        [HttpPost("protestos-diarios")]
        public async Task<ActionResult<ResultadoInforme<InformeProtestoDiario>>> GetProtestosDiarios(
            [FromBody] FiltroProtestosRequest filtro)
        {
            try
            {
                var datos = await _informesRepository.GetProtestosDiariosAsync(
                    filtro.FechaDesde,
                    filtro.FechaHasta,
                    filtro.CodigoEjecutivo);

                var resultado = new ResultadoInforme<InformeProtestoDiario>
                {
                    TipoInforme = TipoInformeEnum.ProtestosDiarios,
                    NombreInforme = "Protestos Diarios",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.ToList(),
                    TotalRegistros = datos.Count(),
                    Exitoso = true,
                    Totales = new Dictionary<string, decimal>
                    {
                        ["MontoTotal"] = datos.Sum(x => x.MontoDocumento),
                        ["CantidadProtestos"] = datos.Count()
                    }
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInforme<InformeProtestoDiario>
                {
                    Exitoso = false,
                    MensajeError = $"Error al generar informe: {ex.Message}"
                });
            }
        }

        #endregion

        #region Documentos por Deudor (F_INF1300)

        /// <summary>
        /// Genera informe de documentos por deudor
        /// POST: api/informes/documentos-deudor
        /// </summary>
        [HttpPost("documentos-deudor")]
        public async Task<ActionResult<ResultadoInforme<InformeDocumentoDeudor>>> GetDocumentosDeudor(
            [FromBody] FiltroDocumentosDeudorRequest filtro)
        {
            try
            {
                var datos = await _informesRepository.GetDocumentosPorDeudorAsync(
                    filtro.RutDeudor,
                    filtro.TipoDocumento);

                var resultado = new ResultadoInforme<InformeDocumentoDeudor>
                {
                    TipoInforme = TipoInformeEnum.DocumentosPorDeudor,
                    NombreInforme = "Documentos por Deudor",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.ToList(),
                    TotalRegistros = datos.Count(),
                    Exitoso = true,
                    Totales = new Dictionary<string, decimal>
                    {
                        ["MontoDocumentos"] = datos.Sum(x => x.MontoDocumento),
                        ["SaldoTotal"] = datos.Sum(x => x.SaldoDocumento)
                    }
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInforme<InformeDocumentoDeudor>
                {
                    Exitoso = false,
                    MensajeError = $"Error al generar informe: {ex.Message}"
                });
            }
        }

        #endregion

        #region Cartera Cliente (F_INF1400)

        /// <summary>
        /// Genera informe de cartera por cliente
        /// POST: api/informes/cartera-cliente
        /// </summary>
        [HttpPost("cartera-cliente")]
        public async Task<ActionResult<ResultadoInforme<InformeCarteraCliente>>> GetCarteraCliente(
            [FromBody] FiltroCarteraClienteRequest filtro)
        {
            try
            {
                var cartera = await _informesRepository.GetCarteraClienteAsync(
                    filtro.RutCliente,
                    filtro.TipoDocumento);

                if (cartera == null)
                {
                    return NotFound($"No se encontró cartera para el cliente {filtro.RutCliente}");
                }

                var resultado = new ResultadoInforme<InformeCarteraCliente>
                {
                    TipoInforme = TipoInformeEnum.CarteraCliente,
                    NombreInforme = "Cartera Cliente",
                    FechaGeneracion = DateTime.Now,
                    Datos = new List<InformeCarteraCliente> { cartera },
                    TotalRegistros = cartera.TotalDocumentos,
                    Exitoso = true,
                    Totales = new Dictionary<string, decimal>
                    {
                        ["MontoVigente"] = cartera.MontoVigente,
                        ["MontoMora"] = cartera.MontoMora,
                        ["MontoTotal"] = cartera.MontoTotal
                    }
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInforme<InformeCarteraCliente>
                {
                    Exitoso = false,
                    MensajeError = $"Error al generar informe: {ex.Message}"
                });
            }
        }

        #endregion

        #region Devengamiento (F_INF1500)

        /// <summary>
        /// Genera informe de devengamiento
        /// POST: api/informes/devengamiento
        /// </summary>
        [HttpPost("devengamiento")]
        public async Task<ActionResult<ResultadoInforme<InformeDevengamiento>>> GetDevengamiento(
            [FromBody] FiltroDevengamientoRequest filtro)
        {
            try
            {
                var datos = await _informesRepository.GetDevengamientoAsync(
                    filtro.FechaCorte,
                    filtro.RutCliente,
                    filtro.CentroCosto);

                var resultado = new ResultadoInforme<InformeDevengamiento>
                {
                    TipoInforme = TipoInformeEnum.DetalleDevengamiento,
                    NombreInforme = "Detalle Devengamiento",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.ToList(),
                    TotalRegistros = datos.Count(),
                    Exitoso = true,
                    Totales = new Dictionary<string, decimal>
                    {
                        ["InteresTotal"] = datos.Sum(x => x.InteresTotal),
                        ["InteresDevengado"] = datos.Sum(x => x.InteresDevengado),
                        ["InteresPorDevengar"] = datos.Sum(x => x.InteresPorDevengar),
                        ["MontoDocumentos"] = datos.Sum(x => x.MontoDocumento)
                    }
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInforme<InformeDevengamiento>
                {
                    Exitoso = false,
                    MensajeError = $"Error al generar informe: {ex.Message}"
                });
            }
        }

        #endregion

        #region Adelantos (F_INF1600)

        /// <summary>
        /// Genera informe de adelantos
        /// POST: api/informes/adelantos
        /// </summary>
        [HttpPost("adelantos")]
        public async Task<ActionResult<ResultadoInforme<InformeAdelanto>>> GetAdelantos(
            [FromBody] FiltroAdelantosRequest filtro)
        {
            try
            {
                var datos = await _informesRepository.GetAdelantosAsync(
                    filtro.FechaDesde,
                    filtro.FechaHasta,
                    filtro.RutCliente);

                var resultado = new ResultadoInforme<InformeAdelanto>
                {
                    TipoInforme = TipoInformeEnum.Adelantos,
                    NombreInforme = "Adelantos",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.ToList(),
                    TotalRegistros = datos.Count(),
                    Exitoso = true,
                    Totales = new Dictionary<string, decimal>
                    {
                        ["MontoAdelanto"] = datos.Sum(x => x.MontoAdelanto),
                        ["MontoDocumentos"] = datos.Sum(x => x.MontoDocumentos)
                    }
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInforme<InformeAdelanto>
                {
                    Exitoso = false,
                    MensajeError = $"Error al generar informe: {ex.Message}"
                });
            }
        }

        #endregion

        #region Cupo Cliente (F_INF1700)

        /// <summary>
        /// Genera informe de cupo de clientes
        /// POST: api/informes/cupo-cliente
        /// </summary>
        [HttpPost("cupo-cliente")]
        public async Task<ActionResult<ResultadoInforme<InformeCupoCliente>>> GetCupoCliente(
            [FromBody] FiltroCupoClienteRequest filtro)
        {
            try
            {
                var datos = await _informesRepository.GetCupoClientesAsync(
                    filtro.CodigoEjecutivo,
                    filtro.RutCliente);

                var resultado = new ResultadoInforme<InformeCupoCliente>
                {
                    TipoInforme = TipoInformeEnum.CupoCliente,
                    NombreInforme = "Cupo Cliente por Ejecutivo",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.ToList(),
                    TotalRegistros = datos.Count(),
                    Exitoso = true,
                    Totales = new Dictionary<string, decimal>
                    {
                        ["LineaAprobada"] = datos.Sum(x => x.LineaAprobada),
                        ["MontoUtilizado"] = datos.Sum(x => x.MontoUtilizado),
                        ["MontoDisponible"] = datos.Sum(x => x.MontoDisponible)
                    }
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInforme<InformeCupoCliente>
                {
                    Exitoso = false,
                    MensajeError = $"Error al generar informe: {ex.Message}"
                });
            }
        }

        #endregion

        #region Volumen Operaciones (F_INF1800)

        /// <summary>
        /// Genera informe de volumen de operaciones
        /// POST: api/informes/volumen-operaciones
        /// </summary>
        [HttpPost("volumen-operaciones")]
        public async Task<ActionResult<ResultadoInforme<InformeVolumenOperaciones>>> GetVolumenOperaciones(
            [FromBody] FiltroVolumenOperacionesRequest filtro)
        {
            try
            {
                var datos = await _informesRepository.GetVolumenOperacionesAsync(
                    filtro.Anio,
                    filtro.Mes,
                    filtro.CodigoEjecutivo);

                var resultado = new ResultadoInforme<InformeVolumenOperaciones>
                {
                    TipoInforme = TipoInformeEnum.VolumenOperaciones,
                    NombreInforme = "Volumen de Operaciones",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.ToList(),
                    TotalRegistros = datos.Count(),
                    Exitoso = true,
                    Totales = new Dictionary<string, decimal>
                    {
                        ["MontoOperado"] = datos.Sum(x => x.MontoOperado),
                        ["MontoAnticipado"] = datos.Sum(x => x.MontoAnticipado),
                        ["Comisiones"] = datos.Sum(x => x.ComisionesGeneradas),
                        ["Intereses"] = datos.Sum(x => x.InteresesGenerados),
                        ["CantidadOperaciones"] = datos.Sum(x => x.CantidadOperaciones)
                    }
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInforme<InformeVolumenOperaciones>
                {
                    Exitoso = false,
                    MensajeError = $"Error al generar informe: {ex.Message}"
                });
            }
        }

        #endregion

        #region Excedentes (F_INF1900)

        /// <summary>
        /// Genera informe de excedentes
        /// POST: api/informes/excedentes
        /// </summary>
        [HttpPost("excedentes")]
        public async Task<ActionResult<ResultadoInforme<InformeExcedente>>> GetExcedentes(
            [FromBody] FiltroExcedentesRequest filtro)
        {
            try
            {
                var datos = await _informesRepository.GetExcedentesAsync(
                    filtro.FechaDesde,
                    filtro.FechaHasta,
                    filtro.RutCliente,
                    filtro.Estado);

                var resultado = new ResultadoInforme<InformeExcedente>
                {
                    TipoInforme = TipoInformeEnum.Excedentes,
                    NombreInforme = "Detalle Excedentes",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.ToList(),
                    TotalRegistros = datos.Count(),
                    Exitoso = true,
                    Totales = new Dictionary<string, decimal>
                    {
                        ["MontoExcedente"] = datos.Sum(x => x.MontoExcedente),
                        ["MontoDevuelto"] = datos.Sum(x => x.MontoDevuelto ?? 0)
                    }
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInforme<InformeExcedente>
                {
                    Exitoso = false,
                    MensajeError = $"Error al generar informe: {ex.Message}"
                });
            }
        }

        #endregion

        #region Concentración Terceros (F_INF2000)

        /// <summary>
        /// Genera informe de concentración por terceros
        /// POST: api/informes/concentracion-terceros
        /// </summary>
        [HttpPost("concentracion-terceros")]
        public async Task<ActionResult<ResultadoInforme<InformeConcentracionTerceros>>> GetConcentracionTerceros(
            [FromBody] FiltroConcentracionTercerosRequest filtro)
        {
            try
            {
                var datos = await _informesRepository.GetConcentracionTercerosAsync(
                    filtro.FechaCorte,
                    filtro.RutDeudor,
                    filtro.PorcentajeMinimo);

                var resultado = new ResultadoInforme<InformeConcentracionTerceros>
                {
                    TipoInforme = TipoInformeEnum.ConcentracionTerceros,
                    NombreInforme = "Concentración por Terceros",
                    FechaGeneracion = DateTime.Now,
                    Datos = datos.ToList(),
                    TotalRegistros = datos.Count(),
                    Exitoso = true,
                    Totales = new Dictionary<string, decimal>
                    {
                        ["MontoTotal"] = datos.Sum(x => x.MontoTotal),
                        ["MontoVigente"] = datos.Sum(x => x.MontoVigente),
                        ["MontoVencido"] = datos.Sum(x => x.MontoVencido)
                    }
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultadoInforme<InformeConcentracionTerceros>
                {
                    Exitoso = false,
                    MensajeError = $"Error al generar informe: {ex.Message}"
                });
            }
        }

        #endregion

        #region Resumen Dashboard

        /// <summary>
        /// Obtiene resumen general para dashboard
        /// GET: api/informes/resumen
        /// </summary>
        [HttpGet("resumen")]
        public async Task<ActionResult<ResumenInformes>> GetResumen([FromQuery] DateTime? fechaCorte = null)
        {
            try
            {
                var resumen = await _informesRepository.GetResumenGeneralAsync(fechaCorte);
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener resumen: {ex.Message}");
            }
        }

        #endregion

        #region Historial

        /// <summary>
        /// Obtiene historial de informes generados
        /// GET: api/informes/historial
        /// </summary>
        [HttpGet("historial")]
        public async Task<ActionResult<IEnumerable<HistorialInforme>>> GetHistorial(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] string? usuario = null,
            [FromQuery] int? tipoInforme = null)
        {
            try
            {
                var historial = await _informesRepository.GetHistorialAsync(
                    fechaDesde,
                    fechaHasta,
                    usuario,
                    tipoInforme);

                return Ok(historial);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener historial: {ex.Message}");
            }
        }

        #endregion
    }

    #region Request DTOs

    public class FiltroAnticiposRequest
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? TipoDocumento { get; set; }
        public long? RutCliente { get; set; }
    }

    public class FiltroProtestosRequest
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? CodigoEjecutivo { get; set; }
    }

    public class FiltroDocumentosDeudorRequest
    {
        public long RutDeudor { get; set; }
        public int? TipoDocumento { get; set; }
    }

    public class FiltroCarteraClienteRequest
    {
        public long RutCliente { get; set; }
        public int? TipoDocumento { get; set; }
    }

    public class FiltroDevengamientoRequest
    {
        public DateTime? FechaCorte { get; set; }
        public long? RutCliente { get; set; }
        public int? CentroCosto { get; set; }
    }

    public class FiltroAdelantosRequest
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public long? RutCliente { get; set; }
    }

    public class FiltroCupoClienteRequest
    {
        public int? CodigoEjecutivo { get; set; }
        public long? RutCliente { get; set; }
    }

    public class FiltroVolumenOperacionesRequest
    {
        public int Anio { get; set; }
        public int? Mes { get; set; }
        public int? CodigoEjecutivo { get; set; }
    }

    public class FiltroExcedentesRequest
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public long? RutCliente { get; set; }
        public string? Estado { get; set; }
    }

    public class FiltroConcentracionTercerosRequest
    {
        public DateTime? FechaCorte { get; set; }
        public long? RutDeudor { get; set; }
        public decimal? PorcentajeMinimo { get; set; }
    }

    #endregion
}
