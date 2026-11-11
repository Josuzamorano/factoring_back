// ====================================================
// Controller del Módulo Comercial
// Migrado de: F_CM1000.frm, F_CM1200.frm, F_CM1400.frm, 
//             F_CM1500.frm, F_CM1700.frm, F_CM1800.frm
// ====================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Factoring.Core.DTOs;
using Factoring.Core.Interfaces;
using System.Security.Claims;

namespace Factoring.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ComercialController : ControllerBase
{
    private readonly IExcedenteRepository _excedenteRepository;
    private readonly IProrrogaRepository _prorrogaRepository;
    private readonly IDocumentoClienteRepository _documentoClienteRepository;
    private readonly ICartolaClienteRepository _cartolaRepository;
    private readonly ILogger<ComercialController> _logger;

    public ComercialController(
        IExcedenteRepository excedenteRepository,
        IProrrogaRepository prorrogaRepository,
        IDocumentoClienteRepository documentoClienteRepository,
        ICartolaClienteRepository cartolaRepository,
        ILogger<ComercialController> logger)
    {
        _excedenteRepository = excedenteRepository;
        _prorrogaRepository = prorrogaRepository;
        _documentoClienteRepository = documentoClienteRepository;
        _cartolaRepository = cartolaRepository;
        _logger = logger;
    }

    private string GetCurrentUser() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

    #region Excedentes - Migrado de F_CM1200.frm

    /// <summary>
    /// Obtiene lista de excedentes con filtros
    /// </summary>
    [HttpGet("excedentes")]
    public async Task<ActionResult<IEnumerable<ExcedenteDto>>> GetExcedentes([FromQuery] FiltroExcedenteDto filtro)
    {
        try
        {
            var excedentes = await _excedenteRepository.GetExcedentesAsync(filtro);
            return Ok(excedentes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener excedentes");
            return StatusCode(500, "Error al obtener excedentes");
        }
    }

    /// <summary>
    /// Obtiene excedente por número
    /// </summary>
    [HttpGet("excedentes/{numeroExcedente}")]
    public async Task<ActionResult<ExcedenteDto>> GetExcedente(int numeroExcedente)
    {
        try
        {
            var excedente = await _excedenteRepository.GetExcedenteByIdAsync(numeroExcedente);
            if (excedente == null)
                return NotFound("Excedente no encontrado");
            return Ok(excedente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener excedente {NumeroExcedente}", numeroExcedente);
            return StatusCode(500, "Error al obtener excedente");
        }
    }

    /// <summary>
    /// Obtiene resumen de excedentes por cliente
    /// </summary>
    [HttpGet("excedentes/resumen")]
    public async Task<ActionResult<IEnumerable<ResumenExcedenteDto>>> GetResumenExcedentes([FromQuery] FiltroExcedenteDto filtro)
    {
        try
        {
            var resumen = await _excedenteRepository.GetResumenExcedentesAsync(filtro);
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de excedentes");
            return StatusCode(500, "Error al obtener resumen de excedentes");
        }
    }

    /// <summary>
    /// Obtiene excedentes disponibles para un cliente
    /// </summary>
    [HttpGet("excedentes/disponibles/{rutCliente}")]
    public async Task<ActionResult<IEnumerable<ExcedenteDetalleDto>>> GetExcedentesDisponibles(long rutCliente)
    {
        try
        {
            var excedentes = await _excedenteRepository.GetExcedentesDisponiblesAsync(rutCliente);
            return Ok(excedentes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener excedentes disponibles para cliente {RutCliente}", rutCliente);
            return StatusCode(500, "Error al obtener excedentes disponibles");
        }
    }

    /// <summary>
    /// Aplica excedente a un documento
    /// </summary>
    [HttpPost("excedentes/aplicar")]
    public async Task<ActionResult> AplicarExcedente([FromBody] AplicarExcedenteDto dto)
    {
        try
        {
            // Validar saldo disponible
            var saldo = await _excedenteRepository.GetSaldoExcedenteAsync(dto.NumeroExcedente);
            if (saldo < dto.MontoAplicar)
                return BadRequest("El monto a aplicar excede el saldo disponible del excedente");

            var result = await _excedenteRepository.AplicarExcedenteAsync(dto, GetCurrentUser());
            if (!result)
                return BadRequest("No se pudo aplicar el excedente");

            return Ok(new { message = "Excedente aplicado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aplicar excedente");
            return StatusCode(500, "Error al aplicar excedente");
        }
    }

    /// <summary>
    /// Obtiene saldo de un excedente
    /// </summary>
    [HttpGet("excedentes/{numeroExcedente}/saldo")]
    public async Task<ActionResult<decimal>> GetSaldoExcedente(int numeroExcedente)
    {
        try
        {
            var saldo = await _excedenteRepository.GetSaldoExcedenteAsync(numeroExcedente);
            return Ok(saldo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener saldo de excedente");
            return StatusCode(500, "Error al obtener saldo");
        }
    }

    #endregion

    #region Prórrogas - Migrado de F_CM1400.frm, F_CM1800.frm

    /// <summary>
    /// Obtiene lista de prórrogas con filtros
    /// </summary>
    [HttpGet("prorrogas")]
    public async Task<ActionResult<IEnumerable<ProrrogaDto>>> GetProrrogas([FromQuery] FiltroProrrogaDto filtro)
    {
        try
        {
            var prorrogas = await _prorrogaRepository.GetProrrogasAsync(filtro);
            return Ok(prorrogas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prórrogas");
            return StatusCode(500, "Error al obtener prórrogas");
        }
    }

    /// <summary>
    /// Obtiene prórroga por número
    /// </summary>
    [HttpGet("prorrogas/{numeroProrrroga}")]
    public async Task<ActionResult<ProrrogaDto>> GetProrroga(int numeroProrrroga)
    {
        try
        {
            var prorroga = await _prorrogaRepository.GetProrrogaByIdAsync(numeroProrrroga);
            if (prorroga == null)
                return NotFound("Prórroga no encontrada");
            return Ok(prorroga);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prórroga {NumeroProrrroga}", numeroProrrroga);
            return StatusCode(500, "Error al obtener prórroga");
        }
    }

    /// <summary>
    /// Obtiene prórrogas pendientes de aprobación
    /// </summary>
    [HttpGet("prorrogas/pendientes")]
    public async Task<ActionResult<IEnumerable<ProrrogaDto>>> GetProrrogasPendientes()
    {
        try
        {
            var prorrogas = await _prorrogaRepository.GetProrrogasPendientesAsync();
            return Ok(prorrogas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prórrogas pendientes");
            return StatusCode(500, "Error al obtener prórrogas pendientes");
        }
    }

    /// <summary>
    /// Crea nueva prórroga
    /// </summary>
    [HttpPost("prorrogas")]
    public async Task<ActionResult<int>> CreateProrroga([FromBody] CreateProrrogaDto dto)
    {
        try
        {
            var numeroProrrroga = await _prorrogaRepository.CreateProrrogaAsync(dto, GetCurrentUser());
            return CreatedAtAction(nameof(GetProrroga), new { numeroProrrroga }, new { numeroProrrroga });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear prórroga");
            return StatusCode(500, "Error al crear prórroga");
        }
    }

    /// <summary>
    /// Aprueba o rechaza prórroga
    /// </summary>
    [HttpPost("prorrogas/procesar")]
    public async Task<ActionResult> ProcesarProrroga([FromBody] AccionProrrogaDto dto)
    {
        try
        {
            var result = await _prorrogaRepository.ProcesarProrrogaAsync(dto, GetCurrentUser());
            if (!result)
                return BadRequest("No se pudo procesar la prórroga. Verifique que esté en estado pendiente.");

            var mensaje = dto.Aprobar ? "Prórroga aprobada correctamente" : "Prórroga rechazada";
            return Ok(new { message = mensaje });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar prórroga");
            return StatusCode(500, "Error al procesar prórroga");
        }
    }

    /// <summary>
    /// Contabiliza prórroga aprobada
    /// </summary>
    [HttpPost("prorrogas/{numeroProrrroga}/contabilizar")]
    public async Task<ActionResult> ContabilizarProrroga(int numeroProrrroga)
    {
        try
        {
            var result = await _prorrogaRepository.ContabilizarProrrogaAsync(numeroProrrroga, GetCurrentUser());
            if (!result)
                return BadRequest("No se pudo contabilizar la prórroga. Verifique que esté aprobada.");

            return Ok(new { message = "Prórroga contabilizada correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contabilizar prórroga");
            return StatusCode(500, "Error al contabilizar prórroga");
        }
    }

    /// <summary>
    /// Busca documento para prórroga
    /// </summary>
    [HttpGet("prorrogas/buscar-documento")]
    public async Task<ActionResult<DocumentoClienteDto>> BuscarDocumentoParaProrroga(
        [FromQuery] int tipoDocumento, 
        [FromQuery] string numeroDocumento)
    {
        try
        {
            var documento = await _prorrogaRepository.GetDocumentoParaProrrogaAsync(tipoDocumento, numeroDocumento);
            if (documento == null)
                return NotFound("Documento no encontrado");
            return Ok(documento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar documento para prórroga");
            return StatusCode(500, "Error al buscar documento");
        }
    }

    #endregion

    #region Documentos Cliente - Migrado de F_CM1700.frm

    /// <summary>
    /// Obtiene documentos de un cliente
    /// </summary>
    [HttpGet("documentos")]
    public async Task<ActionResult<IEnumerable<DocumentoClienteDto>>> GetDocumentosCliente([FromQuery] FiltroDocumentoClienteDto filtro)
    {
        try
        {
            var documentos = await _documentoClienteRepository.GetDocumentosClienteAsync(filtro);
            return Ok(documentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documentos del cliente");
            return StatusCode(500, "Error al obtener documentos");
        }
    }

    /// <summary>
    /// Obtiene resumen de documentos por cliente
    /// </summary>
    [HttpGet("documentos/resumen/{rutCliente}")]
    public async Task<ActionResult<ResumenDocumentoClienteDto>> GetResumenDocumentos(long rutCliente)
    {
        try
        {
            var resumen = await _documentoClienteRepository.GetResumenDocumentosAsync(rutCliente);
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de documentos para cliente {RutCliente}", rutCliente);
            return StatusCode(500, "Error al obtener resumen");
        }
    }

    /// <summary>
    /// Obtiene documentos vencidos de un cliente
    /// </summary>
    [HttpGet("documentos/vencidos/{rutCliente}")]
    public async Task<ActionResult<IEnumerable<DocumentoClienteDto>>> GetDocumentosVencidos(long rutCliente)
    {
        try
        {
            var documentos = await _documentoClienteRepository.GetDocumentosVencidosAsync(rutCliente);
            return Ok(documentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documentos vencidos");
            return StatusCode(500, "Error al obtener documentos vencidos");
        }
    }

    /// <summary>
    /// Obtiene documentos por vencer en los próximos N días
    /// </summary>
    [HttpGet("documentos/por-vencer/{rutCliente}")]
    public async Task<ActionResult<IEnumerable<DocumentoClienteDto>>> GetDocumentosPorVencer(
        long rutCliente, 
        [FromQuery] int dias = 30)
    {
        try
        {
            var documentos = await _documentoClienteRepository.GetDocumentosPorVencerAsync(rutCliente, dias);
            return Ok(documentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documentos por vencer");
            return StatusCode(500, "Error al obtener documentos por vencer");
        }
    }

    #endregion

    #region Cartola Cliente - Migrado de F_GC1900.frm

    /// <summary>
    /// Obtiene cartola de movimientos del cliente
    /// </summary>
    [HttpGet("cartola")]
    public async Task<ActionResult<IEnumerable<CartolaClienteDto>>> GetCartola([FromQuery] FiltroCartolaClienteDto filtro)
    {
        try
        {
            var cartola = await _cartolaRepository.GetCartolaAsync(filtro);
            return Ok(cartola);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cartola del cliente");
            return StatusCode(500, "Error al obtener cartola");
        }
    }

    /// <summary>
    /// Obtiene resumen de cartola del cliente
    /// </summary>
    [HttpGet("cartola/resumen/{rutCliente}")]
    public async Task<ActionResult<ResumenCartolaClienteDto>> GetResumenCartola(long rutCliente)
    {
        try
        {
            var resumen = await _cartolaRepository.GetResumenCartolaAsync(rutCliente);
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de cartola para cliente {RutCliente}", rutCliente);
            return StatusCode(500, "Error al obtener resumen de cartola");
        }
    }

    #endregion
}
