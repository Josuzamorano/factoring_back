using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Factoring.Core.Entities;
using Factoring.Core.DTOs;
using Factoring.Core.Interfaces;

namespace Factoring.API.Controllers;

/// <summary>
/// Controller para gestión de Finanzas - Tasas y Plazos
/// Migrado de F_FI1000.frm, F_FI1100.frm, F_FI1200.frm
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinanzasController : ControllerBase
{
    private readonly ITasaPlazoRepository _tasaPlazoRepository;
    private readonly IIndicadorEconomicoRepository _indicadorRepository;
    private readonly ILogger<FinanzasController> _logger;

    public FinanzasController(
        ITasaPlazoRepository tasaPlazoRepository,
        IIndicadorEconomicoRepository indicadorRepository,
        ILogger<FinanzasController> logger)
    {
        _tasaPlazoRepository = tasaPlazoRepository;
        _indicadorRepository = indicadorRepository;
        _logger = logger;
    }

    #region Tasas y Plazos

    /// <summary>
    /// Obtiene tasas por fecha y moneda
    /// </summary>
    [HttpGet("tasas")]
    public async Task<ActionResult<IEnumerable<TasaPlazoDto>>> GetTasas(
        [FromQuery] DateTime? fecha,
        [FromQuery] int tipoMoneda)
    {
        try
        {
            var tasas = await _tasaPlazoRepository.GetByFechaYMonedaAsync(fecha, tipoMoneda);
            
            var result = tasas.Select(t => new TasaPlazoDto
            {
                FechaVigencia = t.FechaVigencia,
                TipoTasa = t.TipoTasa,
                DescripcionTipoTasa = t.DescripcionTipoTasa,
                TipoMoneda = t.TipoMoneda,
                DescripcionMoneda = t.DescripcionMoneda,
                DiasTramo = t.DiasTramo,
                TasaBase = t.TasaBase,
                TasaMora = t.TasaMora,
                TasaMaximaConvencional = t.TasaMaximaConvencional,
                TasaAuxiliar = t.TasaAuxiliar
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tasas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene tasas con filtros avanzados
    /// </summary>
    [HttpGet("tasas/filtrar")]
    public async Task<ActionResult<IEnumerable<TasaPlazoDto>>> GetTasasFiltradas(
        [FromQuery] FiltroTasaPlazoDto filtro)
    {
        try
        {
            var tasas = await _tasaPlazoRepository.GetByFiltroAsync(filtro);
            
            var result = tasas.Select(t => new TasaPlazoDto
            {
                FechaVigencia = t.FechaVigencia,
                TipoTasa = t.TipoTasa,
                DescripcionTipoTasa = t.DescripcionTipoTasa,
                TipoMoneda = t.TipoMoneda,
                DescripcionMoneda = t.DescripcionMoneda,
                DiasTramo = t.DiasTramo,
                TasaBase = t.TasaBase,
                TasaMora = t.TasaMora,
                TasaMaximaConvencional = t.TasaMaximaConvencional,
                TasaAuxiliar = t.TasaAuxiliar
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al filtrar tasas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene una tasa específica por clave compuesta
    /// </summary>
    [HttpGet("tasas/{fecha}/{tipoTasa}/{tipoMoneda}/{dias}")]
    public async Task<ActionResult<TasaPlazoDto>> GetTasa(
        DateTime? fecha, int tipoTasa, int tipoMoneda, int dias)
    {
        try
        {
            var tasa = await _tasaPlazoRepository.GetByClaveAsync(fecha, tipoTasa, tipoMoneda, dias);
            
            if (tasa == null)
                return NotFound("Tasa no encontrada");

            return Ok(new TasaPlazoDto
            {
                FechaVigencia = tasa.FechaVigencia,
                TipoTasa = tasa.TipoTasa,
                DescripcionTipoTasa = tasa.DescripcionTipoTasa,
                TipoMoneda = tasa.TipoMoneda,
                DescripcionMoneda = tasa.DescripcionMoneda,
                DiasTramo = tasa.DiasTramo,
                TasaBase = tasa.TasaBase,
                TasaMora = tasa.TasaMora,
                TasaMaximaConvencional = tasa.TasaMaximaConvencional,
                TasaAuxiliar = tasa.TasaAuxiliar
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tasa");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene la tasa aplicable para una operación específica
    /// </summary>
    [HttpGet("tasas/aplicable")]
    public async Task<ActionResult<TasaPlazoDto>> GetTasaAplicable(
        [FromQuery] DateTime? fecha,
        [FromQuery] int tipoMoneda,
        [FromQuery] int dias,
        [FromQuery] decimal monto)
    {
        try
        {
            var tasa = await _tasaPlazoRepository.GetTasaAplicableAsync(fecha, tipoMoneda, dias, monto);
            
            if (tasa == null)
                return NotFound("No se encontró tasa aplicable para los parámetros indicados");

            return Ok(new TasaPlazoDto
            {
                FechaVigencia = tasa.FechaVigencia,
                TipoTasa = tasa.TipoTasa,
                DescripcionTipoTasa = tasa.DescripcionTipoTasa,
                TipoMoneda = tasa.TipoMoneda,
                DescripcionMoneda = tasa.DescripcionMoneda,
                DiasTramo = tasa.DiasTramo,
                TasaBase = tasa.TasaBase,
                TasaMora = tasa.TasaMora,
                TasaMaximaConvencional = tasa.TasaMaximaConvencional,
                TasaAuxiliar = tasa.TasaAuxiliar
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tasa aplicable");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea una nueva tasa
    /// </summary>
    [HttpPost("tasas")]
    public async Task<ActionResult> CreateTasa([FromBody] CreateTasaPlazoDto dto)
    {
        try
        {
            // Validar que no exista
            var existente = await _tasaPlazoRepository.GetByClaveAsync(
                dto.FechaVigencia, dto.TipoTasa, dto.TipoMoneda, dto.DiasTramo);
            
            if (existente != null)
                return Conflict("Ya existe una tasa para la combinación de fecha, tipo, moneda y días especificada");

            var tasa = new TasaPlazo
            {
                FechaVigencia = dto.FechaVigencia,
                TipoTasa = dto.TipoTasa,
                TipoMoneda = dto.TipoMoneda,
                DiasTramo = dto.DiasTramo,
                TasaBase = dto.TasaBase,
                TasaMora = dto.TasaMora,
                TasaMaximaConvencional = dto.TasaMaximaConvencional,
                TasaAuxiliar = dto.TasaAuxiliar
            };

            var result = await _tasaPlazoRepository.CreateAsync(tasa);
            
            if (!result)
                return BadRequest("No se pudo crear la tasa");

            return CreatedAtAction(nameof(GetTasa), 
                new { fecha = tasa.FechaVigencia?.ToString("yyyy-MM-dd"), tipoTasa = tasa.TipoTasa, tipoMoneda = tasa.TipoMoneda, dias = tasa.DiasTramo }, 
                dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tasa");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza una tasa existente
    /// </summary>
    [HttpPut("tasas/{fecha}/{tipoTasa}/{tipoMoneda}/{dias}")]
    public async Task<ActionResult> UpdateTasa(
        DateTime? fecha, int tipoTasa, int tipoMoneda, int dias,
        [FromBody] UpdateTasaPlazoDto dto)
    {
        try
        {
            var tasa = await _tasaPlazoRepository.GetByClaveAsync(fecha, tipoTasa, tipoMoneda, dias);
            
            if (tasa == null)
                return NotFound("Tasa no encontrada");

            tasa.TasaBase = dto.TasaBase;
            tasa.TasaMora = dto.TasaMora;
            tasa.TasaMaximaConvencional = dto.TasaMaximaConvencional;
            tasa.TasaAuxiliar = dto.TasaAuxiliar;

            var result = await _tasaPlazoRepository.UpdateAsync(tasa);
            
            if (!result)
                return BadRequest("No se pudo actualizar la tasa");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tasa");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Elimina una tasa
    /// </summary>
    [HttpDelete("tasas/{fecha}/{tipoTasa}/{tipoMoneda}/{dias}")]
    public async Task<ActionResult> DeleteTasa(
        DateTime? fecha, int tipoTasa, int tipoMoneda, int dias)
    {
        try
        {
            var result = await _tasaPlazoRepository.DeleteAsync(fecha, tipoTasa, tipoMoneda, dias);
            
            if (!result)
                return NotFound("Tasa no encontrada");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar tasa");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Copia tasas de una fecha a otra
    /// </summary>
    [HttpPost("tasas/copiar")]
    public async Task<ActionResult<ResultadoCopiaDto>> CopiarTasas([FromBody] CopiarTasasDto dto)
    {
        try
        {
            var copiadas = await _tasaPlazoRepository.CopiarTasasAsync(
                dto.FechaOrigen, dto.FechaDestino, dto.TipoMoneda, dto.AjustePorcentual);

            return Ok(new ResultadoCopiaDto
            {
                Exitoso = copiadas > 0,
                TasasCopiadas = copiadas,
                Mensaje = copiadas > 0 
                    ? $"Se copiaron {copiadas} tasas exitosamente"
                    : "No se encontraron tasas para copiar o ya existen en la fecha destino"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al copiar tasas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene fechas disponibles con tasas registradas
    /// </summary>
    [HttpGet("tasas/fechas-disponibles")]
    public async Task<ActionResult<IEnumerable<DateTime?>>> GetFechasDisponibles(
        [FromQuery] int tipoMoneda,
        [FromQuery] int? ultimosDias = 30)
    {
        try
        {
            var fechas = await _tasaPlazoRepository.GetFechasDisponiblesAsync(tipoMoneda, ultimosDias);
            return Ok(fechas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener fechas disponibles");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region Resumen y Histórico

    /// <summary>
    /// Obtiene resumen de tasas por fecha y moneda
    /// </summary>
    [HttpGet("tasas/resumen")]
    public async Task<ActionResult<ResumenTasasDto>> GetResumen(
        [FromQuery] DateTime? fecha,
        [FromQuery] int tipoMoneda)
    {
        try
        {
            var resumen = await _tasaPlazoRepository.GetResumenAsync(fecha, tipoMoneda);
            
            if (resumen == null)
                return NotFound("No hay tasas para la fecha y moneda indicadas");

            return Ok(new ResumenTasasDto
            {
                Fecha = resumen.Fecha,
                TipoMoneda = resumen.TipoMoneda,
                DescripcionMoneda = resumen.DescripcionMoneda,
                CantidadTramos = resumen.CantidadTramos,
                TasaBasePromedio = resumen.TasaBasePromedio,
                TasaBaseMinima = resumen.TasaBaseMinima,
                TasaBaseMaxima = resumen.TasaBaseMaxima,
                TasaMoraPromedio = resumen.TasaMoraPromedio
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de tasas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene histórico de tasas para análisis
    /// </summary>
    [HttpGet("tasas/historico")]
    public async Task<ActionResult<IEnumerable<HistoricoTasaDto>>> GetHistorico(
        [FromQuery] FiltroHistoricoTasaDto filtro)
    {
        try
        {
            var historico = await _tasaPlazoRepository.GetHistoricoAsync(filtro);
            
            var result = historico.Select(h => new HistoricoTasaDto
            {
                Fecha = h.Fecha,
                TipoMoneda = h.TipoMoneda,
                DescripcionMoneda = h.DescripcionMoneda,
                DiasTramo = h.DiasTramo,
                TasaBase = h.TasaBase,
                TasaMora = h.TasaMora,
                TasaMaximaConvencional = h.TasaMaximaConvencional
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener histórico de tasas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region Simulación

    /// <summary>
    /// Simula el cálculo de tasa para una operación
    /// </summary>
    [HttpPost("simular")]
    public async Task<ActionResult<SimulacionTasaResponseDto>> Simular([FromBody] SimulacionTasaRequestDto request)
    {
        try
        {
            // Obtener la tasa aplicable
            var tasa = await _tasaPlazoRepository.GetTasaAplicableAsync(
                request.Fecha, request.TipoMoneda, request.DiasOperacion, request.MontoOperacion);

            if (tasa == null)
                return NotFound("No se encontró tasa aplicable para los parámetros indicados");

            // Calcular valores
            var spread = request.Spread ?? 0;
            var tasaFinal = tasa.TasaBase + spread;
            var interesNeto = request.MontoOperacion * (tasaFinal / 100) * (request.DiasOperacion / 30.0m);
            var interesMora = request.MontoOperacion * (tasa.TasaMora / 100) * (request.DiasOperacion / 30.0m);
            var totalDescuento = interesNeto;
            var montoLiquido = request.MontoOperacion - totalDescuento;

            return Ok(new SimulacionTasaResponseDto
            {
                Fecha = request.Fecha,
                TipoMoneda = request.TipoMoneda,
                DescripcionMoneda = tasa.DescripcionMoneda,
                DiasOperacion = request.DiasOperacion,
                MontoOperacion = request.MontoOperacion,
                TasaBase = tasa.TasaBase,
                TasaMora = tasa.TasaMora,
                TasaMaximaConvencional = tasa.TasaMaximaConvencional,
                Spread = spread,
                TasaFinal = tasaFinal,
                InteresNeto = Math.Round(interesNeto, 2),
                InteresMora = Math.Round(interesMora, 2),
                TotalDescuento = Math.Round(totalDescuento, 2),
                MontoLiquido = Math.Round(montoLiquido, 2),
                TramoAplicado = tasa.DescripcionTipoTasa,
                Observaciones = $"Tasa vigente desde {tasa.FechaVigencia:dd/MM/yyyy}, tramo {tasa.DiasTramo} días"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al simular tasa");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region Indicadores Económicos

    /// <summary>
    /// Obtiene todos los indicadores económicos actuales
    /// </summary>
    [HttpGet("indicadores")]
    public async Task<ActionResult<IEnumerable<IndicadorEconomicoDto>>> GetIndicadores()
    {
        try
        {
            var indicadores = await _indicadorRepository.GetAllUltimosAsync();
            
            var result = indicadores.Select(i => new IndicadorEconomicoDto
            {
                Id = i.Id,
                Codigo = i.Codigo,
                Nombre = i.Nombre,
                Fecha = i.Fecha,
                Valor = i.Valor,
                VariacionDiaria = i.VariacionDiaria,
                VariacionMensual = i.VariacionMensual,
                VariacionAnual = i.VariacionAnual
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener indicadores");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene el último valor de un indicador específico
    /// </summary>
    [HttpGet("indicadores/{codigo}")]
    public async Task<ActionResult<IndicadorEconomicoDto>> GetIndicador(string codigo)
    {
        try
        {
            var indicador = await _indicadorRepository.GetUltimoAsync(codigo);
            
            if (indicador == null)
                return NotFound($"Indicador '{codigo}' no encontrado");

            return Ok(new IndicadorEconomicoDto
            {
                Id = indicador.Id,
                Codigo = indicador.Codigo,
                Nombre = indicador.Nombre,
                Fecha = indicador.Fecha,
                Valor = indicador.Valor,
                VariacionDiaria = indicador.VariacionDiaria,
                VariacionMensual = indicador.VariacionMensual,
                VariacionAnual = indicador.VariacionAnual
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener indicador");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene histórico de un indicador
    /// </summary>
    [HttpGet("indicadores/{codigo}/historico")]
    public async Task<ActionResult<IEnumerable<IndicadorEconomicoDto>>> GetIndicadorHistorico(
        string codigo,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        try
        {
            var historico = await _indicadorRepository.GetHistoricoAsync(codigo, fechaDesde, fechaHasta);
            
            var result = historico.Select(i => new IndicadorEconomicoDto
            {
                Id = i.Id,
                Codigo = i.Codigo,
                Nombre = i.Nombre,
                Fecha = i.Fecha,
                Valor = i.Valor,
                VariacionDiaria = i.VariacionDiaria,
                VariacionMensual = i.VariacionMensual,
                VariacionAnual = i.VariacionAnual
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener histórico de indicador");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion
}
