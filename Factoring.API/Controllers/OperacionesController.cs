using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Factoring.Application.DTOs;
using Factoring.Application.Interfaces;
using Factoring.Application.Services;
using Factoring.Domain.Entities;
using System.Security.Claims;

namespace Factoring.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OperacionesController : ControllerBase
{
    private readonly IOperacionRepository _operacionRepository;
    private readonly IFacturaOtorgadaRepository _facturaRepository;
    private readonly IChequeOtorgadoRepository _chequeRepository;
    private readonly ILetraOtorgadaRepository _letraRepository;
    private readonly IOtroDocumentoOtorgadoRepository _otroRepository;
    private readonly IDocumentoOperacionRepository _documentoRepository;
    private readonly ITipoGastoRepository _tipoGastoRepository;
    private readonly IGastoOperacionRepository _gastoRepository;
    private readonly OperacionService _operacionService;

    public OperacionesController(
        IOperacionRepository operacionRepository,
        IFacturaOtorgadaRepository facturaRepository,
        IChequeOtorgadoRepository chequeRepository,
        ILetraOtorgadaRepository letraRepository,
        IOtroDocumentoOtorgadoRepository otroRepository,
        IDocumentoOperacionRepository documentoRepository,
        ITipoGastoRepository tipoGastoRepository,
        IGastoOperacionRepository gastoRepository,
        OperacionService operacionService)
    {
        _operacionRepository = operacionRepository;
        _facturaRepository = facturaRepository;
        _chequeRepository = chequeRepository;
        _letraRepository = letraRepository;
        _otroRepository = otroRepository;
        _documentoRepository = documentoRepository;
        _tipoGastoRepository = tipoGastoRepository;
        _gastoRepository = gastoRepository;
        _operacionService = operacionService;
    }

    private string GetCurrentUser() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

    // =============================================
    // Operaciones CRUD
    // =============================================

    /// <summary>
    /// Obtener todas las operaciones
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Operacion>>> GetAll()
    {
        var operaciones = await _operacionRepository.GetAllAsync();
        return Ok(operaciones);
    }

    /// <summary>
    /// Obtener operación por número
    /// </summary>
    [HttpGet("{numeroOperacion:long}")]
    public async Task<ActionResult<OperacionDto>> GetById(long numeroOperacion)
    {
        var operacion = await _operacionRepository.GetFullByIdAsync(numeroOperacion);
        if (operacion == null)
            return NotFound(new { message = $"Operación {numeroOperacion} no encontrada" });

        // Cargar documentos
        var facturas = await _facturaRepository.GetByOperacionAsync(numeroOperacion);
        var cheques = await _chequeRepository.GetByOperacionAsync(numeroOperacion);
        var letras = await _letraRepository.GetByOperacionAsync(numeroOperacion);
        var otros = await _otroRepository.GetByOperacionAsync(numeroOperacion);
        var gastos = await _gastoRepository.GetByOperacionAsync(numeroOperacion);

        var dto = new OperacionDto
        {
            NumeroOperacion = operacion.NumeroOperacion,
            RutCliente = operacion.RutCliente,
            NombreCliente = operacion.NombreCliente ?? "",
            TipoDocumento = operacion.TipoDocumento,
            NombreTipoDocumento = operacion.NombreTipoDocumento ?? "",
            FechaOperacion = operacion.FechaOperacion,
            FechaOtorgamiento = operacion.FechaOtorgamiento,
            EstadoOperacion = operacion.EstadoOperacion,
            NombreEstado = operacion.NombreEstado ?? "",
            TipoMoneda = operacion.TipoMoneda,
            NombreMoneda = operacion.NombreMoneda ?? "",
            MontoDocumentos = operacion.MontoDocumentos,
            PorcentajeAnticipo = operacion.PorcentajeAnticipo,
            MontoAnticipo = operacion.MontoAnticipo,
            MontoAdelanto = operacion.MontoAdelanto,
            MontoGastos = operacion.MontoGastos,
            MontoIvaComision = operacion.MontoIvaComision,
            TotalGiro = operacion.TotalGiro,
            CodigoBanco = operacion.CodigoBanco,
            NombreBanco = operacion.NombreBanco,
            CuentaCorriente = operacion.CuentaCorriente,
            Observaciones = operacion.Observaciones,
            FechaCreacion = operacion.FechaCreacion,
            Facturas = facturas.Select(f => MapFacturaToDocumentoDto(f)).ToList(),
            Cheques = cheques.Select(c => MapChequeToDocumentoDto(c)).ToList(),
            Letras = letras.Select(l => MapLetraToDocumentoDto(l)).ToList(),
            Otros = otros.Select(o => MapOtroToDocumentoDto(o)).ToList(),
            Gastos = gastos.Select(g => new GastoDto
            {
                CorrelativoOperacion = g.CorrelativoOperacion,
                CorrelativoGasto = g.CorrelativoGasto,
                CodigoGasto = g.CodigoGasto,
                DescripcionGasto = g.DescripcionGasto ?? "",
                MontoGasto = g.MontoGasto,
                AfectoIva = g.AfectoIva
            }).ToList()
        };

        return Ok(dto);
    }

    /// <summary>
    /// Obtener operaciones por cliente
    /// </summary>
    [HttpGet("cliente/{rutCliente:long}")]
    public async Task<ActionResult<IEnumerable<Operacion>>> GetByCliente(long rutCliente)
    {
        var operaciones = await _operacionRepository.GetByClienteAsync(rutCliente);
        return Ok(operaciones);
    }

    /// <summary>
    /// Buscar operaciones con filtros
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<Operacion>>> Search([FromBody] OperacionSearchDto search)
    {
        var operaciones = await _operacionRepository.SearchAsync(
            search.FechaDesde,
            search.FechaHasta,
            search.RutCliente,
            search.Estado,
            search.TipoDocumento);

        return Ok(operaciones);
    }

    /// <summary>
    /// Crear nueva operación
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Operacion>> Create([FromBody] CreateOperacionDto dto)
    {
        // Calcular montos
        decimal montoDocumentos = 0;
        montoDocumentos += dto.Facturas?.Sum(f => f.MontoDocumento) ?? 0;
        montoDocumentos += dto.Cheques?.Sum(c => c.MontoDocumento) ?? 0;
        montoDocumentos += dto.Letras?.Sum(l => l.MontoDocumento) ?? 0;
        montoDocumentos += dto.Otros?.Sum(o => o.MontoDocumento) ?? 0;

        decimal montoAnticipo = montoDocumentos * dto.PorcentajeAnticipo / 100;
        decimal montoGastos = dto.Gastos?.Sum(g => g.MontoGasto) ?? 0;
        decimal montoIvaComision = dto.Gastos?.Where(g => g.AfectoIva).Sum(g => g.MontoGasto * 0.19m) ?? 0;
        decimal totalGiro = montoAnticipo - montoGastos - montoIvaComision;

        var operacion = new Operacion
        {
            RutCliente = dto.RutCliente,
            TipoDocumento = dto.TipoDocumento,
            FechaOperacion = dto.FechaOperacion,
            EstadoOperacion = 1, // Ingresada
            TipoMoneda = dto.TipoMoneda,
            MontoDocumentos = montoDocumentos,
            PorcentajeAnticipo = dto.PorcentajeAnticipo,
            MontoAnticipo = montoAnticipo,
            MontoAdelanto = montoAnticipo, // Inicialmente igual al anticipo
            MontoGastos = montoGastos,
            MontoIvaComision = montoIvaComision,
            TotalGiro = totalGiro,
            CodigoBanco = dto.CodigoBanco,
            CuentaCorriente = dto.CuentaCorriente,
            Observaciones = dto.Observaciones,
            UsuarioCreacion = GetCurrentUser()
        };

        var numeroOperacion = await _operacionRepository.CreateAsync(operacion);
        operacion.NumeroOperacion = numeroOperacion;

        // Crear facturas
        foreach (var f in dto.Facturas ?? new List<CreateDocumentoDto>())
        {
            var factura = new FacturaOtorgada
            {
                CorrelativoOperacion = numeroOperacion,
                RutDeudor = f.RutDeudor,
                NumeroFactura = f.NumeroDocumento,
                FechaEmision = f.FechaEmision,
                FechaVencimiento = f.FechaVencimiento,
                MontoDocumento = f.MontoDocumento,
                MontoAbonado = 0,
                MontoSaldo = f.MontoDocumento,
                EstadoDocumento = 1,
                TipoMoneda = f.TipoMoneda,
                Observaciones = f.Observaciones
            };
            await _facturaRepository.CreateAsync(factura);
        }

        // Crear cheques
        foreach (var c in dto.Cheques ?? new List<CreateDocumentoDto>())
        {
            var cheque = new ChequeOtorgado
            {
                CorrelativoOperacion = numeroOperacion,
                RutDeudor = c.RutDeudor,
                NumeroCheque = c.NumeroDocumento,
                CodigoBanco = c.CodigoBanco,
                FechaEmision = c.FechaEmision,
                FechaVencimiento = c.FechaVencimiento,
                MontoDocumento = c.MontoDocumento,
                MontoAbonado = 0,
                MontoSaldo = c.MontoDocumento,
                EstadoDocumento = 1,
                TipoMoneda = c.TipoMoneda,
                Observaciones = c.Observaciones
            };
            await _chequeRepository.CreateAsync(cheque);
        }

        // Crear letras
        foreach (var l in dto.Letras ?? new List<CreateDocumentoDto>())
        {
            var letra = new LetraOtorgada
            {
                CorrelativoOperacion = numeroOperacion,
                RutDeudor = l.RutDeudor,
                NumeroLetra = l.NumeroDocumento,
                FechaEmision = l.FechaEmision,
                FechaVencimiento = l.FechaVencimiento,
                MontoDocumento = l.MontoDocumento,
                MontoAbonado = 0,
                MontoSaldo = l.MontoDocumento,
                EstadoDocumento = 1,
                TipoMoneda = l.TipoMoneda,
                Observaciones = l.Observaciones
            };
            await _letraRepository.CreateAsync(letra);
        }

        // Crear otros documentos
        foreach (var o in dto.Otros ?? new List<CreateDocumentoDto>())
        {
            var otro = new OtroDocumentoOtorgado
            {
                CorrelativoOperacion = numeroOperacion,
                RutDeudor = o.RutDeudor,
                TipoDocumento = o.TipoDocumento ?? 9,
                NumeroDocumento = o.NumeroDocumento,
                FechaEmision = o.FechaEmision,
                FechaVencimiento = o.FechaVencimiento,
                MontoDocumento = o.MontoDocumento,
                MontoAbonado = 0,
                MontoSaldo = o.MontoDocumento,
                EstadoDocumento = 1,
                TipoMoneda = o.TipoMoneda,
                Observaciones = o.Observaciones
            };
            await _otroRepository.CreateAsync(otro);
        }

        // Crear gastos
        foreach (var g in dto.Gastos ?? new List<CreateGastoDto>())
        {
            var gasto = new GastoOperacion
            {
                CorrelativoOperacion = numeroOperacion,
                CodigoGasto = g.CodigoGasto,
                MontoGasto = g.MontoGasto,
                AfectoIva = g.AfectoIva
            };
            await _gastoRepository.CreateAsync(gasto);
        }

        return CreatedAtAction(nameof(GetById), new { numeroOperacion = numeroOperacion }, operacion);
    }

    /// <summary>
    /// Actualizar operación
    /// </summary>
    [HttpPut("{numeroOperacion:long}")]
    public async Task<IActionResult> Update(long numeroOperacion, [FromBody] UpdateOperacionDto dto)
    {
        var operacion = await _operacionRepository.GetByIdAsync(numeroOperacion);
        if (operacion == null)
            return NotFound(new { message = $"Operación {numeroOperacion} no encontrada" });

        if (operacion.EstadoOperacion != 1)
            return BadRequest(new { message = "Solo se pueden modificar operaciones en estado 'Ingresada'" });

        operacion.TipoDocumento = dto.TipoDocumento;
        operacion.FechaOperacion = dto.FechaOperacion;
        operacion.TipoMoneda = dto.TipoMoneda;
        operacion.PorcentajeAnticipo = dto.PorcentajeAnticipo;
        operacion.CodigoBanco = dto.CodigoBanco;
        operacion.CuentaCorriente = dto.CuentaCorriente;
        operacion.Observaciones = dto.Observaciones;
        operacion.UsuarioModificacion = GetCurrentUser();

        var updated = await _operacionRepository.UpdateAsync(operacion);
        if (!updated)
            return StatusCode(500, new { message = "Error al actualizar la operación" });

        return NoContent();
    }

    /// <summary>
    /// Eliminar operación
    /// </summary>
    [HttpDelete("{numeroOperacion:long}")]
    public async Task<IActionResult> Delete(long numeroOperacion)
    {
        var operacion = await _operacionRepository.GetByIdAsync(numeroOperacion);
        if (operacion == null)
            return NotFound(new { message = $"Operación {numeroOperacion} no encontrada" });

        if (operacion.EstadoOperacion != 1)
            return BadRequest(new { message = "Solo se pueden eliminar operaciones en estado 'Ingresada'" });

        // Eliminar documentos relacionados
        await _facturaRepository.DeleteByOperacionAsync(numeroOperacion);
        await _chequeRepository.DeleteByOperacionAsync(numeroOperacion);
        await _letraRepository.DeleteByOperacionAsync(numeroOperacion);
        await _otroRepository.DeleteByOperacionAsync(numeroOperacion);
        await _gastoRepository.DeleteByOperacionAsync(numeroOperacion);

        var deleted = await _operacionRepository.DeleteAsync(numeroOperacion);
        if (!deleted)
            return StatusCode(500, new { message = "Error al eliminar la operación" });

        return NoContent();
    }

    /// <summary>
    /// Cambiar estado de operación
    /// </summary>
    [HttpPut("{numeroOperacion:long}/estado")]
    public async Task<IActionResult> CambiarEstado(long numeroOperacion, [FromBody] CambiarEstadoDto dto)
    {
        var operacion = await _operacionRepository.GetByIdAsync(numeroOperacion);
        if (operacion == null)
            return NotFound(new { message = $"Operación {numeroOperacion} no encontrada" });

        var updated = await _operacionRepository.CambiarEstadoAsync(numeroOperacion, dto.NuevoEstado);
        if (!updated)
            return StatusCode(500, new { message = "Error al cambiar el estado de la operación" });

        return NoContent();
    }

    /// <summary>
    /// Otorgar operación
    /// </summary>
    [HttpPut("{numeroOperacion:long}/otorgar")]
    public async Task<IActionResult> Otorgar(long numeroOperacion, [FromBody] OtorgarOperacionDto dto)
    {
        var operacion = await _operacionRepository.GetByIdAsync(numeroOperacion);
        if (operacion == null)
            return NotFound(new { message = $"Operación {numeroOperacion} no encontrada" });

        if (operacion.EstadoOperacion != 1)
            return BadRequest(new { message = "Solo se pueden otorgar operaciones en estado 'Ingresada'" });

        var updated = await _operacionRepository.OtorgarAsync(numeroOperacion, dto.FechaOtorgamiento);
        if (!updated)
            return StatusCode(500, new { message = "Error al otorgar la operación" });

        return NoContent();
    }

    /// <summary>
    /// Obtener siguiente número de operación
    /// </summary>
    [HttpGet("siguiente-numero")]
    public async Task<ActionResult<long>> GetSiguienteNumero()
    {
        var numero = await _operacionRepository.GetNextNumeroAsync();
        return Ok(numero);
    }

    // =============================================
    // Tipos de Gasto
    // =============================================

    /// <summary>
    /// Obtener tipos de gasto
    /// </summary>
    [HttpGet("tipos-gasto")]
    public async Task<ActionResult<IEnumerable<TipoGasto>>> GetTiposGasto()
    {
        var tipos = await _tipoGastoRepository.GetAllAsync();
        return Ok(tipos);
    }

    // =============================================
    // Helper Methods
    // =============================================

    private DocumentoDto MapFacturaToDocumentoDto(FacturaOtorgada f) => new()
    {
        CorrelativoOperacion = f.CorrelativoOperacion,
        CorrelativoDocumento = f.CorrelativoDocumento,
        TipoDocumento = "FAC",
        RutDeudor = f.RutDeudor,
        NombreDeudor = f.NombreDeudor ?? "",
        NumeroDocumento = f.NumeroFactura,
        FechaEmision = f.FechaEmision,
        FechaVencimiento = f.FechaVencimiento,
        MontoDocumento = f.MontoDocumento,
        MontoAbonado = f.MontoAbonado,
        MontoSaldo = f.MontoSaldo,
        EstadoDocumento = f.EstadoDocumento,
        NombreEstado = f.NombreEstado ?? "",
        TipoMoneda = f.TipoMoneda,
        Observaciones = f.Observaciones
    };

    private DocumentoDto MapChequeToDocumentoDto(ChequeOtorgado c) => new()
    {
        CorrelativoOperacion = c.CorrelativoOperacion,
        CorrelativoDocumento = c.CorrelativoDocumento,
        TipoDocumento = "CHE",
        RutDeudor = c.RutDeudor,
        NombreDeudor = c.NombreDeudor ?? "",
        NumeroDocumento = c.NumeroCheque,
        FechaEmision = c.FechaEmision,
        FechaVencimiento = c.FechaVencimiento,
        MontoDocumento = c.MontoDocumento,
        MontoAbonado = c.MontoAbonado,
        MontoSaldo = c.MontoSaldo,
        EstadoDocumento = c.EstadoDocumento,
        NombreEstado = c.NombreEstado ?? "",
        TipoMoneda = c.TipoMoneda,
        CodigoBanco = c.CodigoBanco,
        NombreBanco = c.NombreBanco,
        Observaciones = c.Observaciones
    };

    private DocumentoDto MapLetraToDocumentoDto(LetraOtorgada l) => new()
    {
        CorrelativoOperacion = l.CorrelativoOperacion,
        CorrelativoDocumento = l.CorrelativoDocumento,
        TipoDocumento = "LET",
        RutDeudor = l.RutDeudor,
        NombreDeudor = l.NombreDeudor ?? "",
        NumeroDocumento = l.NumeroLetra,
        FechaEmision = l.FechaEmision,
        FechaVencimiento = l.FechaVencimiento,
        MontoDocumento = l.MontoDocumento,
        MontoAbonado = l.MontoAbonado,
        MontoSaldo = l.MontoSaldo,
        EstadoDocumento = l.EstadoDocumento,
        NombreEstado = l.NombreEstado ?? "",
        TipoMoneda = l.TipoMoneda,
        Observaciones = l.Observaciones
    };

    private DocumentoDto MapOtroToDocumentoDto(OtroDocumentoOtorgado o) => new()
    {
        CorrelativoOperacion = o.CorrelativoOperacion,
        CorrelativoDocumento = o.CorrelativoDocumento,
        TipoDocumento = "OTR",
        RutDeudor = o.RutDeudor,
        NombreDeudor = o.NombreDeudor ?? "",
        NumeroDocumento = o.NumeroDocumento,
        FechaEmision = o.FechaEmision,
        FechaVencimiento = o.FechaVencimiento,
        MontoDocumento = o.MontoDocumento,
        MontoAbonado = o.MontoAbonado,
        MontoSaldo = o.MontoSaldo,
        EstadoDocumento = o.EstadoDocumento,
        NombreEstado = o.NombreEstado ?? "",
        TipoMoneda = o.TipoMoneda,
        Observaciones = o.Observaciones
    };

    // =============================================
    // Endpoints con validación de línea de crédito
    // =============================================

    /// <summary>
    /// Validar si una operación puede ser cursada
    /// </summary>
    [HttpPost("validar")]
    public async Task<ActionResult<ValidacionOperacionResult>> ValidarOperacion([FromBody] ValidarOperacionRequest request)
    {
        var resultado = await _operacionService.ValidarOperacionAsync(
            request.RutCliente,
            request.NumeroLinea,
            request.TipoDocumento,
            request.MontoOperacion,
            request.RutDeudor);

        return Ok(resultado);
    }

    /// <summary>
    /// Calcular valores de una operación (intereses, comisiones, giro)
    /// </summary>
    [HttpPost("calcular")]
    public async Task<ActionResult<CalculoOperacionResult>> CalcularOperacion([FromBody] CalcularOperacionRequest request)
    {
        var resultado = await _operacionService.CalcularOperacionAsync(
            request.RutCliente,
            request.NumeroLinea,
            request.TipoDocumento,
            request.MontoDocumentos,
            request.PlazoDias,
            request.PorcentajeAnticipo);

        return Ok(resultado);
    }

    /// <summary>
    /// Crear operación completa con validación de línea y cálculo automático
    /// </summary>
    [HttpPost("crear-completa")]
    public async Task<IActionResult> CrearOperacionCompleta([FromBody] CrearOperacionRequest request)
    {
        request.Usuario = GetCurrentUser();
        var (exito, numeroOperacion, mensaje) = await _operacionService.CrearOperacionCompletaAsync(request);

        if (!exito)
            return BadRequest(new { success = false, message = mensaje });

        return Ok(new { success = true, numeroOperacion, message = mensaje });
    }

    /// <summary>
    /// Otorgar operación con generación de giro
    /// </summary>
    [HttpPost("{numeroOperacion:long}/otorgar-completo")]
    public async Task<IActionResult> OtorgarOperacionCompleta(long numeroOperacion)
    {
        var usuario = GetCurrentUser();
        var (exito, mensaje) = await _operacionService.OtorgarOperacionAsync(numeroOperacion, usuario);

        if (!exito)
            return BadRequest(new { success = false, message = mensaje });

        return Ok(new { success = true, message = mensaje });
    }

    /// <summary>
    /// Anular operación con reversión de movimientos
    /// </summary>
    [HttpPost("{numeroOperacion:long}/anular")]
    public async Task<IActionResult> AnularOperacion(long numeroOperacion, [FromBody] AnularOperacionRequest request)
    {
        var usuario = GetCurrentUser();
        var (exito, mensaje) = await _operacionService.AnularOperacionAsync(numeroOperacion, request.Motivo, usuario);

        if (!exito)
            return BadRequest(new { success = false, message = mensaje });

        return Ok(new { success = true, message = mensaje });
    }
}

// DTOs adicionales para los nuevos endpoints
public class ValidarOperacionRequest
{
    public long RutCliente { get; set; }
    public int NumeroLinea { get; set; }
    public int TipoDocumento { get; set; }
    public decimal MontoOperacion { get; set; }
    public long? RutDeudor { get; set; }
}

public class CalcularOperacionRequest
{
    public long RutCliente { get; set; }
    public int NumeroLinea { get; set; }
    public int TipoDocumento { get; set; }
    public decimal MontoDocumentos { get; set; }
    public int PlazoDias { get; set; }
    public decimal? PorcentajeAnticipo { get; set; }
}

public class AnularOperacionRequest
{
    public string Motivo { get; set; } = string.Empty;
}
