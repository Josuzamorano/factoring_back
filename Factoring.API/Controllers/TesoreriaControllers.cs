using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;
using System.Security.Claims;

namespace Factoring.API.Controllers;

/// <summary>
/// Controller de Giros - Migrado de F_TS1000.frm, F_TS1100.frm, F_TS1200.frm
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GirosController : ControllerBase
{
    private readonly IGiroRepository _giroRepository;

    public GirosController(IGiroRepository giroRepository)
    {
        _giroRepository = giroRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GiroDto>>> GetAll()
    {
        var giros = await _giroRepository.GetAllAsync();
        var result = giros.Select(MapToDto);
        return Ok(result);
    }

    [HttpGet("fecha")]
    public async Task<ActionResult<IEnumerable<GiroDto>>> GetByFecha([FromQuery] DateTime? fechaDesde, [FromQuery] DateTime? fechaHasta)
    {
        var giros = await _giroRepository.GetByFechaAsync(fechaDesde, fechaHasta);
        var result = giros.Select(MapToDto);
        return Ok(result);
    }

    [HttpGet("cliente/{rutCliente}")]
    public async Task<ActionResult<IEnumerable<GiroDto>>> GetByCliente(long rutCliente)
    {
        var giros = await _giroRepository.GetByClienteAsync(rutCliente);
        var result = giros.Select(MapToDto);
        return Ok(result);
    }

    [HttpGet("operacion/{numeroOperacion}")]
    public async Task<ActionResult<IEnumerable<GiroDto>>> GetByOperacion(int numeroOperacion)
    {
        var giros = await _giroRepository.GetByOperacionAsync(numeroOperacion);
        var result = giros.Select(MapToDto);
        return Ok(result);
    }

    [HttpPost("filtro")]
    public async Task<ActionResult<IEnumerable<GiroDto>>> GetByFiltro([FromBody] FiltroTesoreriaDto filtro)
    {
        var giros = await _giroRepository.GetByFiltroAsync(filtro);
        var result = giros.Select(MapToDto);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GiroDto>> GetById(int id)
    {
        var giro = await _giroRepository.GetByIdAsync(id);
        if (giro == null)
            return NotFound(new { mensaje = "Giro no encontrado" });

        return Ok(MapToDto(giro));
    }

    [HttpGet("resumen/{fecha}")]
    public async Task<ActionResult<ResumenGirosDto>> GetResumen(DateTime? fecha)
    {
        var resumen = await _giroRepository.GetResumenByFechaAsync(fecha);
        return Ok(resumen);
    }

    [HttpPost]
    public async Task<ActionResult<GiroDto>> Create([FromBody] GiroCreateDto dto)
    {
        var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "SYSTEM";
        
        var giro = new Giro
        {
            RutCliente = dto.RutCliente,
            NumeroOperacion = dto.NumeroOperacion,
            TipoGiro = dto.TipoGiro,
            FechaGiro = dto.FechaGiro,
            MontoGiro = dto.MontoGiro,
            CuentaCorriente = dto.CuentaCorriente,
            CodigoBanco = dto.CodigoBanco,
            Beneficiario = dto.Beneficiario,
            FormaCancelacion = dto.FormaCancelacion,
            UsuarioCreacion = usuario
        };

        var numeroGiro = await _giroRepository.CreateAsync(giro);
        var giroCreado = await _giroRepository.GetByIdAsync(numeroGiro);
        
        return CreatedAtAction(nameof(GetById), new { id = numeroGiro }, MapToDto(giroCreado!));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] GiroUpdateDto dto)
    {
        var giro = await _giroRepository.GetByIdAsync(id);
        if (giro == null)
            return NotFound(new { mensaje = "Giro no encontrado" });

        var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "SYSTEM";

        giro.TipoGiro = dto.TipoGiro;
        giro.MontoGiro = dto.MontoGiro;
        giro.CuentaCorriente = dto.CuentaCorriente;
        giro.CodigoBanco = dto.CodigoBanco;
        giro.Beneficiario = dto.Beneficiario;
        giro.Estado = dto.Estado;
        giro.FormaCancelacion = dto.FormaCancelacion;
        giro.UsuarioModificacion = usuario;

        var result = await _giroRepository.UpdateAsync(giro);
        if (!result)
            return BadRequest(new { mensaje = "Error al actualizar el giro" });

        return NoContent();
    }

    [HttpPatch("{id}/estado")]
    public async Task<ActionResult> UpdateEstado(int id, [FromBody] int nuevoEstado)
    {
        var giro = await _giroRepository.GetByIdAsync(id);
        if (giro == null)
            return NotFound(new { mensaje = "Giro no encontrado" });

        var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "SYSTEM";
        var result = await _giroRepository.UpdateEstadoAsync(id, nuevoEstado, usuario);
        
        if (!result)
            return BadRequest(new { mensaje = "Error al actualizar el estado del giro" });

        return NoContent();
    }

    [HttpPatch("{id}/emitir")]
    public async Task<ActionResult> Emitir(int id)
    {
        var giro = await _giroRepository.GetByIdAsync(id);
        if (giro == null)
            return NotFound(new { mensaje = "Giro no encontrado" });

        if (giro.Estado != 1)
            return BadRequest(new { mensaje = "Solo se pueden emitir giros en estado Digitado" });

        var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "SYSTEM";
        var result = await _giroRepository.UpdateEstadoAsync(id, 2, usuario); // 2 = Emitido
        
        if (!result)
            return BadRequest(new { mensaje = "Error al emitir el giro" });

        return NoContent();
    }

    [HttpPatch("{id}/anular")]
    public async Task<ActionResult> Anular(int id)
    {
        var giro = await _giroRepository.GetByIdAsync(id);
        if (giro == null)
            return NotFound(new { mensaje = "Giro no encontrado" });

        var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "SYSTEM";
        var result = await _giroRepository.AnularAsync(id, usuario);
        
        if (!result)
            return BadRequest(new { mensaje = "Error al anular el giro" });

        return NoContent();
    }

    private static GiroDto MapToDto(Giro g) => new()
    {
        NumeroGiro = g.NumeroGiro,
        RutCliente = g.RutCliente,
        NombreCliente = g.NombreCliente,
        NumeroOperacion = g.NumeroOperacion,
        TipoGiro = g.TipoGiro,
        DescripcionTipoGiro = g.DescripcionTipoGiro,
        FechaGiro = g.FechaGiro,
        MontoGiro = g.MontoGiro,
        CuentaCorriente = g.CuentaCorriente,
        CodigoBanco = g.CodigoBanco,
        NombreBanco = g.NombreBanco,
        Beneficiario = g.Beneficiario,
        Estado = g.Estado,
        DescripcionEstado = g.DescripcionEstado,
        FormaCancelacion = g.FormaCancelacion,
        UsuarioCreacion = g.UsuarioCreacion,
        FechaCreacion = g.FechaCreacion
    };
}

/// <summary>
/// Controller de Movimientos - Migrado de F_TS1800.frm
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MovimientosController : ControllerBase
{
    private readonly IMovimientoRepository _movimientoRepository;

    public MovimientosController(IMovimientoRepository movimientoRepository)
    {
        _movimientoRepository = movimientoRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovimientoDto>>> GetAll()
    {
        var movimientos = await _movimientoRepository.GetAllAsync();
        var result = movimientos.Select(MapToDto);
        return Ok(result);
    }

    [HttpGet("fecha")]
    public async Task<ActionResult<IEnumerable<MovimientoDto>>> GetByFecha([FromQuery] DateTime? fechaDesde, [FromQuery] DateTime? fechaHasta)
    {
        var movimientos = await _movimientoRepository.GetByFechaAsync(fechaDesde, fechaHasta);
        var result = movimientos.Select(MapToDto);
        return Ok(result);
    }

    [HttpGet("pizarra/{fecha}")]
    public async Task<ActionResult<IEnumerable<MovimientoDto>>> GetPizarra(DateTime? fecha)
    {
        var movimientos = await _movimientoRepository.GetPizarraAsync(fecha);
        var result = movimientos.Select(MapToDto);
        return Ok(result);
    }

    [HttpGet("cliente/{rutCliente}")]
    public async Task<ActionResult<IEnumerable<MovimientoDto>>> GetByCliente(long rutCliente)
    {
        var movimientos = await _movimientoRepository.GetByClienteAsync(rutCliente);
        var result = movimientos.Select(MapToDto);
        return Ok(result);
    }

    [HttpPost("filtro")]
    public async Task<ActionResult<IEnumerable<MovimientoDto>>> GetByFiltro([FromBody] FiltroTesoreriaDto filtro)
    {
        var movimientos = await _movimientoRepository.GetByFiltroAsync(filtro);
        var result = movimientos.Select(MapToDto);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovimientoDto>> GetById(int id)
    {
        var movimiento = await _movimientoRepository.GetByIdAsync(id);
        if (movimiento == null)
            return NotFound(new { mensaje = "Movimiento no encontrado" });

        return Ok(MapToDto(movimiento));
    }

    [HttpGet("resumen/{fecha}")]
    public async Task<ActionResult<ResumenMovimientosDto>> GetResumen(DateTime? fecha)
    {
        var resumen = await _movimientoRepository.GetResumenByFechaAsync(fecha);
        return Ok(resumen);
    }

    [HttpPost]
    public async Task<ActionResult<MovimientoDto>> Create([FromBody] MovimientoCreateDto dto)
    {
        var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "SYSTEM";
        
        var movimiento = new Movimiento
        {
            FechaMovimiento = dto.FechaMovimiento,
            TipoMovimiento = dto.TipoMovimiento,
            RutCliente = dto.RutCliente,
            NumeroOperacion = dto.NumeroOperacion,
            NumeroGiro = dto.NumeroGiro,
            Monto = dto.Monto,
            Glosa = dto.Glosa,
            CodigoBanco = dto.CodigoBanco,
            CuentaBanco = dto.CuentaBanco,
            UsuarioCreacion = usuario
        };

        var numeroMovimiento = await _movimientoRepository.CreateAsync(movimiento);
        var movimientoCreado = await _movimientoRepository.GetByIdAsync(numeroMovimiento);
        
        return CreatedAtAction(nameof(GetById), new { id = numeroMovimiento }, MapToDto(movimientoCreado!));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] MovimientoUpdateDto dto)
    {
        var movimiento = await _movimientoRepository.GetByIdAsync(id);
        if (movimiento == null)
            return NotFound(new { mensaje = "Movimiento no encontrado" });

        var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? "SYSTEM";

        movimiento.TipoMovimiento = dto.TipoMovimiento;
        movimiento.Monto = dto.Monto;
        movimiento.Glosa = dto.Glosa;
        movimiento.CodigoBanco = dto.CodigoBanco;
        movimiento.CuentaBanco = dto.CuentaBanco;
        movimiento.Estado = dto.Estado;
        movimiento.UsuarioModificacion = usuario;

        var result = await _movimientoRepository.UpdateAsync(movimiento);
        if (!result)
            return BadRequest(new { mensaje = "Error al actualizar el movimiento" });

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var movimiento = await _movimientoRepository.GetByIdAsync(id);
        if (movimiento == null)
            return NotFound(new { mensaje = "Movimiento no encontrado" });

        var result = await _movimientoRepository.DeleteAsync(id);
        if (!result)
            return BadRequest(new { mensaje = "Error al eliminar el movimiento" });

        return NoContent();
    }

    private static MovimientoDto MapToDto(Movimiento m) => new()
    {
        NumeroMovimiento = m.NumeroMovimiento,
        FechaMovimiento = m.FechaMovimiento,
        TipoMovimiento = m.TipoMovimiento,
        DescripcionTipoMovimiento = m.DescripcionTipoMovimiento,
        RutCliente = m.RutCliente,
        NombreCliente = m.NombreCliente,
        NumeroOperacion = m.NumeroOperacion,
        NumeroGiro = m.NumeroGiro,
        Monto = m.Monto,
        Glosa = m.Glosa,
        CodigoBanco = m.CodigoBanco,
        NombreBanco = m.NombreBanco,
        CuentaBanco = m.CuentaBanco,
        Estado = m.Estado,
        UsuarioCreacion = m.UsuarioCreacion,
        FechaCreacion = m.FechaCreacion
    };
}

/// <summary>
/// Controller de Arqueo - Migrado de F_TS2100.frm
/// Informe de arqueo de documentos (facturas, cheques, letras)
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ArqueoController : ControllerBase
{
    private readonly IArqueoRepository _arqueoRepository;

    public ArqueoController(IArqueoRepository arqueoRepository)
    {
        _arqueoRepository = arqueoRepository;
    }

    /// <summary>
    /// Consulta documentos por filtro (fechas, cliente, tipo, estado)
    /// </summary>
    [HttpPost("consultar")]
    public async Task<ActionResult<IEnumerable<ArqueoDocumentoDto>>> Consultar([FromBody] FiltroArqueoDto filtro)
    {
        var documentos = await _arqueoRepository.GetByFiltroAsync(filtro);
        var result = documentos.Select(MapToDto);
        return Ok(result);
    }

    /// <summary>
    /// Consulta documentos por cliente y rango de fechas
    /// </summary>
    [HttpGet("cliente/{rutCliente}")]
    public async Task<ActionResult<IEnumerable<ArqueoDocumentoDto>>> GetByCliente(
        long rutCliente, 
        [FromQuery] DateTime? fechaDesde, 
        [FromQuery] DateTime? fechaHasta)
    {
        var documentos = await _arqueoRepository.GetByClienteAsync(rutCliente, fechaDesde, fechaHasta);
        var result = documentos.Select(MapToDto);
        return Ok(result);
    }

    /// <summary>
    /// Consulta documentos por tipo (FAO, CHO, LEO)
    /// </summary>
    [HttpGet("tipo/{tipoDocumento}")]
    public async Task<ActionResult<IEnumerable<ArqueoDocumentoDto>>> GetByTipoDocumento(
        string tipoDocumento, 
        [FromQuery] DateTime? fechaDesde, 
        [FromQuery] DateTime? fechaHasta)
    {
        var documentos = await _arqueoRepository.GetByTipoDocumentoAsync(tipoDocumento, fechaDesde, fechaHasta);
        var result = documentos.Select(MapToDto);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene resumen de arqueo por filtro
    /// </summary>
    [HttpPost("resumen")]
    public async Task<ActionResult<ResumenArqueoDto>> GetResumen([FromBody] FiltroArqueoDto filtro)
    {
        var resumen = await _arqueoRepository.GetResumenAsync(filtro);
        return Ok(resumen);
    }

    /// <summary>
    /// Obtiene documentos vencidos a una fecha de corte
    /// </summary>
    [HttpGet("vencidos")]
    public async Task<ActionResult<IEnumerable<ArqueoDocumentoDto>>> GetDocumentosVencidos([FromQuery] DateTime? fechaCorte)
    {
        var fecha = fechaCorte ?? DateTime.Today;
        var documentos = await _arqueoRepository.GetDocumentosVencidosAsync(fecha);
        var result = documentos.Select(MapToDto);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene documentos próximos a vencer
    /// </summary>
    [HttpGet("por-vencer")]
    public async Task<ActionResult<IEnumerable<ArqueoDocumentoDto>>> GetDocumentosPorVencer(
        [FromQuery] DateTime? fechaDesde, 
        [FromQuery] DateTime? fechaHasta)
    {
        var desde = fechaDesde ?? DateTime.Today;
        var hasta = fechaHasta ?? DateTime.Today.AddDays(30);
        var documentos = await _arqueoRepository.GetDocumentosPorVencerAsync(desde, hasta);
        var result = documentos.Select(MapToDto);
        return Ok(result);
    }

    private static ArqueoDocumentoDto MapToDto(ArqueoDocumento d) => new()
    {
        TipoDocumento = d.TipoDocumento,
        NumeroOperacion = d.NumeroOperacion,
        NumeroDocumento = d.NumeroDocumento,
        RutDeudor = d.RutDeudor,
        FechaVencimiento = d.FechaVencimiento,
        MontoDocumento = d.MontoDocumento,
        EstadoDocumento = d.EstadoDocumento,
        RutCliente = d.RutCliente,
        NombreCliente = d.NombreCliente,
        NombreDeudor = d.NombreDeudor,
        CodigoBanco = d.CodigoBanco,
        CuentaCorriente = d.CuentaCorriente,
        NombreBanco = d.NombreBanco,
        DescripcionTipoDocumento = d.DescripcionTipoDocumento,
        DescripcionEstado = d.DescripcionEstado
    };
}
