using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Factoring.Core.DTOs;
using Factoring.Core.Interfaces;

namespace Factoring.API.Controllers;

/// <summary>
/// Controller para Líneas de Crédito
/// Migrado de F_LC1100.frm, F_LC1300.frm, F_LC1400.frm
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LineasCreditoController : ControllerBase
{
    private readonly ILineaCreditoRepository _lineaCreditoRepository;
    private readonly ISublineaProductoRepository _sublineaProductoRepository;
    private readonly ISublineaDeudorRepository _sublineaDeudorRepository;
    private readonly INotariaRepository _notariaRepository;

    public LineasCreditoController(
        ILineaCreditoRepository lineaCreditoRepository,
        ISublineaProductoRepository sublineaProductoRepository,
        ISublineaDeudorRepository sublineaDeudorRepository,
        INotariaRepository notariaRepository)
    {
        _lineaCreditoRepository = lineaCreditoRepository;
        _sublineaProductoRepository = sublineaProductoRepository;
        _sublineaDeudorRepository = sublineaDeudorRepository;
        _notariaRepository = notariaRepository;
    }

    #region Líneas de Crédito

    /// <summary>
    /// Obtiene todas las líneas de crédito con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LineaCreditoDto>>> GetLineas([FromQuery] FiltroLineaCreditoDto? filtro)
    {
        var lineas = await _lineaCreditoRepository.GetAllAsync(filtro);
        return Ok(lineas);
    }

    /// <summary>
    /// Obtiene una línea de crédito específica
    /// </summary>
    [HttpGet("{rutCliente}/{numeroLinea}")]
    public async Task<ActionResult<LineaCreditoDto>> GetLinea(int rutCliente, int numeroLinea)
    {
        var linea = await _lineaCreditoRepository.GetByIdAsync(rutCliente, numeroLinea);
        if (linea == null)
            return NotFound(new { message = "Línea de crédito no encontrada" });
        
        return Ok(linea);
    }

    /// <summary>
    /// Obtiene las líneas de crédito de un cliente
    /// </summary>
    [HttpGet("cliente/{rutCliente}")]
    public async Task<ActionResult<IEnumerable<LineaCreditoDto>>> GetLineasByCliente(int rutCliente)
    {
        var lineas = await _lineaCreditoRepository.GetByClienteAsync(rutCliente);
        return Ok(lineas);
    }

    /// <summary>
    /// Crea una nueva línea de crédito
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> CreateLinea([FromBody] CreateLineaCreditoDto dto)
    {
        var usuario = User.Identity?.Name ?? "SISTEMA";
        
        // Validar que no tenga líneas en estado Creada
        var lineasCreadas = await _lineaCreditoRepository.CountLineasCreadasAsync(dto.RutCliente, dto.TipoMoneda);
        if (lineasCreadas > 0)
            return BadRequest(new { message = "El cliente ya tiene una línea en estado Creada para este tipo de moneda" });

        var numeroLinea = await _lineaCreditoRepository.CreateAsync(dto, usuario);
        return CreatedAtAction(nameof(GetLinea), new { rutCliente = dto.RutCliente, numeroLinea }, new { numeroLinea });
    }

    /// <summary>
    /// Actualiza una línea de crédito existente
    /// </summary>
    [HttpPut("{rutCliente}/{numeroLinea}")]
    public async Task<IActionResult> UpdateLinea(int rutCliente, int numeroLinea, [FromBody] UpdateLineaCreditoDto dto)
    {
        var usuario = User.Identity?.Name ?? "SISTEMA";
        var result = await _lineaCreditoRepository.UpdateAsync(rutCliente, numeroLinea, dto, usuario);
        
        if (!result)
            return NotFound(new { message = "Línea de crédito no encontrada" });
        
        return NoContent();
    }

    /// <summary>
    /// Procesa una acción sobre la línea (aprobar/rechazar/suspender/renovar)
    /// </summary>
    [HttpPost("procesar")]
    public async Task<IActionResult> ProcesarAccion([FromBody] AccionLineaCreditoDto dto)
    {
        var usuario = User.Identity?.Name ?? "SISTEMA";
        var result = await _lineaCreditoRepository.ProcesarAccionAsync(dto, usuario);
        
        if (!result)
            return BadRequest(new { message = "No se pudo procesar la acción" });
        
        return Ok(new { message = $"Acción '{dto.Accion}' procesada exitosamente" });
    }

    /// <summary>
    /// Obtiene el resumen de endeudamiento de un cliente
    /// </summary>
    [HttpGet("endeudamiento/{rutCliente}")]
    public async Task<ActionResult<ResumenEndeudamientoDto>> GetEndeudamiento(int rutCliente)
    {
        var resumen = await _lineaCreditoRepository.GetResumenEndeudamientoAsync(rutCliente);
        if (resumen == null)
            return NotFound(new { message = "No se encontró información de endeudamiento" });
        
        return Ok(resumen);
    }

    /// <summary>
    /// Obtiene líneas próximas a vencer
    /// </summary>
    [HttpGet("por-vencer")]
    public async Task<ActionResult<IEnumerable<ConsultaLineaCreditoDto>>> GetLineasPorVencer([FromQuery] int dias = 30)
    {
        var lineas = await _lineaCreditoRepository.GetLineasPorVencerAsync(dias);
        return Ok(lineas);
    }

    /// <summary>
    /// Obtiene líneas vencidas
    /// </summary>
    [HttpGet("vencidas")]
    public async Task<ActionResult<IEnumerable<ConsultaLineaCreditoDto>>> GetLineasVencidas()
    {
        var lineas = await _lineaCreditoRepository.GetLineasVencidasAsync();
        return Ok(lineas);
    }

    /// <summary>
    /// Obtiene el siguiente número de línea para un cliente
    /// </summary>
    [HttpGet("siguiente-numero/{rutCliente}")]
    public async Task<ActionResult<int>> GetSiguienteNumero(int rutCliente)
    {
        var numero = await _lineaCreditoRepository.GetNextNumeroLineaAsync(rutCliente);
        return Ok(new { numeroLinea = numero });
    }

    #endregion

    #region Sublíneas por Producto

    /// <summary>
    /// Obtiene las sublíneas por producto de una línea
    /// </summary>
    [HttpGet("{rutCliente}/{numeroLinea}/sublineas-producto")]
    public async Task<ActionResult<IEnumerable<SublineaProductoDto>>> GetSublineasProducto(int rutCliente, int numeroLinea)
    {
        var sublineas = await _sublineaProductoRepository.GetByLineaAsync(rutCliente, numeroLinea);
        return Ok(sublineas);
    }

    /// <summary>
    /// Obtiene una sublínea por producto específica
    /// </summary>
    [HttpGet("{rutCliente}/{numeroLinea}/sublineas-producto/{tipoDocumento}")]
    public async Task<ActionResult<SublineaProductoDto>> GetSublineaProducto(int rutCliente, int numeroLinea, int tipoDocumento)
    {
        var sublinea = await _sublineaProductoRepository.GetByIdAsync(rutCliente, numeroLinea, tipoDocumento);
        if (sublinea == null)
            return NotFound(new { message = "Sublínea por producto no encontrada" });
        
        return Ok(sublinea);
    }

    /// <summary>
    /// Crea una sublínea por producto
    /// </summary>
    [HttpPost("{rutCliente}/{numeroLinea}/sublineas-producto")]
    public async Task<IActionResult> CreateSublineaProducto(int rutCliente, int numeroLinea, [FromBody] CreateSublineaProductoDto dto)
    {
        dto.RutCliente = rutCliente;
        dto.NumeroLinea = numeroLinea;
        
        var result = await _sublineaProductoRepository.CreateAsync(dto);
        if (!result)
            return BadRequest(new { message = "No se pudo crear la sublínea" });
        
        return CreatedAtAction(nameof(GetSublineaProducto), 
            new { rutCliente, numeroLinea, tipoDocumento = dto.TipoDocumento }, 
            new { message = "Sublínea creada exitosamente" });
    }

    /// <summary>
    /// Actualiza una sublínea por producto
    /// </summary>
    [HttpPut("{rutCliente}/{numeroLinea}/sublineas-producto/{tipoDocumento}")]
    public async Task<IActionResult> UpdateSublineaProducto(int rutCliente, int numeroLinea, int tipoDocumento, [FromBody] CreateSublineaProductoDto dto)
    {
        var result = await _sublineaProductoRepository.UpdateAsync(rutCliente, numeroLinea, tipoDocumento, dto);
        if (!result)
            return NotFound(new { message = "Sublínea no encontrada" });
        
        return NoContent();
    }

    /// <summary>
    /// Elimina una sublínea por producto
    /// </summary>
    [HttpDelete("{rutCliente}/{numeroLinea}/sublineas-producto/{tipoDocumento}")]
    public async Task<IActionResult> DeleteSublineaProducto(int rutCliente, int numeroLinea, int tipoDocumento)
    {
        var result = await _sublineaProductoRepository.DeleteAsync(rutCliente, numeroLinea, tipoDocumento);
        if (!result)
            return NotFound(new { message = "Sublínea no encontrada" });
        
        return NoContent();
    }

    /// <summary>
    /// Obtiene el resumen de sublíneas de una línea
    /// </summary>
    [HttpGet("{rutCliente}/{numeroLinea}/sublineas-resumen")]
    public async Task<ActionResult<ResumenSublineasDto>> GetResumenSublineas(int rutCliente, int numeroLinea)
    {
        var resumen = await _sublineaProductoRepository.GetResumenAsync(rutCliente, numeroLinea);
        return Ok(resumen);
    }

    #endregion

    #region Sublíneas por Deudor

    /// <summary>
    /// Obtiene las sublíneas por deudor de una línea
    /// </summary>
    [HttpGet("{rutCliente}/{numeroLinea}/sublineas-deudor")]
    public async Task<ActionResult<IEnumerable<SublineaDeudorDto>>> GetSublineasDeudor(int rutCliente, int numeroLinea)
    {
        var sublineas = await _sublineaDeudorRepository.GetByLineaAsync(rutCliente, numeroLinea);
        return Ok(sublineas);
    }

    /// <summary>
    /// Obtiene una sublínea por deudor específica
    /// </summary>
    [HttpGet("{rutCliente}/{numeroLinea}/sublineas-deudor/{rutDeudor}/{tipoDocumento}")]
    public async Task<ActionResult<SublineaDeudorDto>> GetSublineaDeudor(int rutCliente, int numeroLinea, int rutDeudor, int tipoDocumento)
    {
        var sublinea = await _sublineaDeudorRepository.GetByIdAsync(rutCliente, numeroLinea, rutDeudor, tipoDocumento);
        if (sublinea == null)
            return NotFound(new { message = "Sublínea por deudor no encontrada" });
        
        return Ok(sublinea);
    }

    /// <summary>
    /// Crea una sublínea por deudor
    /// </summary>
    [HttpPost("{rutCliente}/{numeroLinea}/sublineas-deudor")]
    public async Task<IActionResult> CreateSublineaDeudor(int rutCliente, int numeroLinea, [FromBody] CreateSublineaDeudorDto dto)
    {
        dto.RutCliente = rutCliente;
        dto.NumeroLinea = numeroLinea;
        
        var result = await _sublineaDeudorRepository.CreateAsync(dto);
        if (!result)
            return BadRequest(new { message = "No se pudo crear la sublínea" });
        
        return CreatedAtAction(nameof(GetSublineaDeudor), 
            new { rutCliente, numeroLinea, rutDeudor = dto.RutDeudor, tipoDocumento = dto.TipoDocumento }, 
            new { message = "Sublínea creada exitosamente" });
    }

    /// <summary>
    /// Actualiza una sublínea por deudor
    /// </summary>
    [HttpPut("{rutCliente}/{numeroLinea}/sublineas-deudor/{rutDeudor}/{tipoDocumento}")]
    public async Task<IActionResult> UpdateSublineaDeudor(int rutCliente, int numeroLinea, int rutDeudor, int tipoDocumento, [FromBody] CreateSublineaDeudorDto dto)
    {
        dto.RutCliente = rutCliente;
        dto.NumeroLinea = numeroLinea;
        dto.RutDeudor = rutDeudor;
        dto.TipoDocumento = tipoDocumento;
        
        var result = await _sublineaDeudorRepository.UpdateAsync(dto);
        if (!result)
            return NotFound(new { message = "Sublínea no encontrada" });
        
        return NoContent();
    }

    /// <summary>
    /// Elimina una sublínea por deudor
    /// </summary>
    [HttpDelete("{rutCliente}/{numeroLinea}/sublineas-deudor/{rutDeudor}/{tipoDocumento}")]
    public async Task<IActionResult> DeleteSublineaDeudor(int rutCliente, int numeroLinea, int rutDeudor, int tipoDocumento)
    {
        var result = await _sublineaDeudorRepository.DeleteAsync(rutCliente, numeroLinea, rutDeudor, tipoDocumento);
        if (!result)
            return NotFound(new { message = "Sublínea no encontrada" });
        
        return NoContent();
    }

    /// <summary>
    /// Obtiene las sublíneas de un deudor específico
    /// </summary>
    [HttpGet("deudor/{rutDeudor}/sublineas")]
    public async Task<ActionResult<IEnumerable<SublineaDeudorDto>>> GetSublineasByDeudor(int rutDeudor)
    {
        var sublineas = await _sublineaDeudorRepository.GetByDeudorAsync(rutDeudor);
        return Ok(sublineas);
    }

    #endregion

    #region Notarías

    /// <summary>
    /// Obtiene todas las notarías
    /// </summary>
    [HttpGet("notarias")]
    public async Task<ActionResult<IEnumerable<NotariaDto>>> GetNotarias()
    {
        var notarias = await _notariaRepository.GetAllAsync();
        return Ok(notarias);
    }

    /// <summary>
    /// Obtiene una notaría por código
    /// </summary>
    [HttpGet("notarias/{codigo}")]
    public async Task<ActionResult<NotariaDto>> GetNotaria(int codigo)
    {
        var notaria = await _notariaRepository.GetByIdAsync(codigo);
        if (notaria == null)
            return NotFound(new { message = "Notaría no encontrada" });
        
        return Ok(notaria);
    }

    #endregion
}
