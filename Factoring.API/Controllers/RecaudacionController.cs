using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Factoring.Core.Interfaces;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;

namespace Factoring.API.Controllers;

/// <summary>
/// Controlador de Recaudación/Gestión de Cobranza - Migrado de F_GC1000.frm y sub-formularios
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecaudacionController : ControllerBase
{
    private readonly IGestionCobranzaRepository _gestionCobranzaRepo;
    private readonly ICobradorRecaudacionRepository _cobradorRepo;
    private readonly IDocumentoGestionRepository _documentoGestionRepo;
    private readonly IEstadoCobranzaRepository _estadoCobranzaRepo;
    private readonly IDeudaTotalClienteRepository _deudaTotalRepo;
    private readonly ILogger<RecaudacionController> _logger;

    public RecaudacionController(
        IGestionCobranzaRepository gestionCobranzaRepo,
        ICobradorRecaudacionRepository cobradorRepo,
        IDocumentoGestionRepository documentoGestionRepo,
        IEstadoCobranzaRepository estadoCobranzaRepo,
        IDeudaTotalClienteRepository deudaTotalRepo,
        ILogger<RecaudacionController> logger)
    {
        _gestionCobranzaRepo = gestionCobranzaRepo;
        _cobradorRepo = cobradorRepo;
        _documentoGestionRepo = documentoGestionRepo;
        _estadoCobranzaRepo = estadoCobranzaRepo;
        _deudaTotalRepo = deudaTotalRepo;
        _logger = logger;
    }

    #region Cobradores

    /// <summary>
    /// Obtiene todos los cobradores
    /// </summary>
    [HttpGet("cobradores")]
    public async Task<ActionResult<IEnumerable<CobradorDto>>> GetCobradores()
    {
        try
        {
            var cobradores = await _cobradorRepo.GetAllAsync();
            return Ok(cobradores.Select(c => new CobradorDto
            {
                CodigoCobrador = c.CodigoCobrador,
                NombreCobrador = c.NombreCobrador,
                Telefono = c.Telefono,
                Email = c.Email,
                Estado = c.Estado,
                Sector = c.Sector,
                TotalClientesAsignados = c.TotalClientesAsignados
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cobradores");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene cobradores activos para dropdown
    /// </summary>
    [HttpGet("cobradores/activos")]
    public async Task<ActionResult<IEnumerable<CobradorSelectDto>>> GetCobradoresActivos()
    {
        try
        {
            var cobradores = await _cobradorRepo.GetActivosAsync();
            return Ok(cobradores.Select(c => new CobradorSelectDto
            {
                CodigoCobrador = c.CodigoCobrador,
                NombreCobrador = c.NombreCobrador
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cobradores activos");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene un cobrador por código
    /// </summary>
    [HttpGet("cobradores/{codigo}")]
    public async Task<ActionResult<CobradorDto>> GetCobrador(string codigo)
    {
        try
        {
            var cobrador = await _cobradorRepo.GetByCodigoAsync(codigo);
            if (cobrador == null) return NotFound("Cobrador no encontrado");
            
            return Ok(new CobradorDto
            {
                CodigoCobrador = cobrador.CodigoCobrador,
                NombreCobrador = cobrador.NombreCobrador,
                Telefono = cobrador.Telefono,
                Email = cobrador.Email,
                Estado = cobrador.Estado,
                Sector = cobrador.Sector,
                TotalClientesAsignados = cobrador.TotalClientesAsignados
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cobrador {Codigo}", codigo);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea un nuevo cobrador
    /// </summary>
    [HttpPost("cobradores")]
    public async Task<ActionResult<int>> CreateCobrador([FromBody] CobradorDto dto)
    {
        try
        {
            var cobrador = new CobradorRecaudacion
            {
                CodigoCobrador = dto.CodigoCobrador,
                NombreCobrador = dto.NombreCobrador,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Estado = dto.Estado,
                Sector = dto.Sector
            };
            var result = await _cobradorRepo.CreateAsync(cobrador);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cobrador");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza un cobrador existente
    /// </summary>
    [HttpPut("cobradores/{codigo}")]
    public async Task<ActionResult<bool>> UpdateCobrador(string codigo, [FromBody] CobradorDto dto)
    {
        try
        {
            if (codigo != dto.CodigoCobrador) return BadRequest("Código no coincide");
            
            var cobrador = new CobradorRecaudacion
            {
                CodigoCobrador = dto.CodigoCobrador,
                NombreCobrador = dto.NombreCobrador,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Estado = dto.Estado,
                Sector = dto.Sector
            };
            var result = await _cobradorRepo.UpdateAsync(cobrador);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cobrador {Codigo}", codigo);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region Gestión de Cobranza

    /// <summary>
    /// Obtiene deudores por cobrador para selección
    /// </summary>
    [HttpGet("gestiones/deudores/{codigoCobrador}")]
    public async Task<ActionResult<IEnumerable<DeudorGestionDto>>> GetDeudoresByCobrador(string codigoCobrador)
    {
        try
        {
            var deudores = await _documentoGestionRepo.GetDeudoresByCobradorAsync(codigoCobrador);
            return Ok(deudores.Select(d => new DeudorGestionDto
            {
                RutDeudor = d.RutDeudor,
                NombreDeudor = d.NombreDeudor
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener deudores por cobrador");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene clientes por cobrador y deudor
    /// </summary>
    [HttpGet("gestiones/clientes/{codigoCobrador}/{rutDeudor}")]
    public async Task<ActionResult<IEnumerable<ClienteCobradorDto>>> GetClientesByCobrador(string codigoCobrador, long rutDeudor)
    {
        try
        {
            var clientes = await _documentoGestionRepo.GetClientesByCobradorDeudorAsync(codigoCobrador, rutDeudor);
            return Ok(clientes.Select(c => new ClienteCobradorDto
            {
                RutCliente = c.RutCliente,
                NombreCliente = c.NombreCliente
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener clientes por cobrador y deudor");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene documentos para gestión por cliente y deudor
    /// </summary>
    [HttpGet("gestiones/documentos/{rutCliente}/{rutDeudor}")]
    public async Task<ActionResult<IEnumerable<DocumentoGestionDto>>> GetDocumentosGestion(long rutCliente, long rutDeudor)
    {
        try
        {
            var documentos = await _documentoGestionRepo.GetByClienteDeudorAsync(rutCliente, rutDeudor);
            return Ok(documentos.Select(d => new DocumentoGestionDto
            {
                RutCliente = d.RutCliente,
                RutDeudor = d.RutDeudor,
                NumeroOperacion = d.NumeroOperacion,
                TipoDocumento = d.TipoDocumento,
                NumeroDocumento = d.NumeroDocumento,
                FechaVencimiento = d.FechaVencimiento,
                MontoDocumento = d.MontoDocumento,
                EstadoDocumento = d.EstadoDocumento,
                EstadoVerificacion = d.EstadoVerificacion,
                EstadoCobranza = d.EstadoCobranza,
                FechaNotificacion = d.FechaNotificacion,
                TieneProblema = d.TieneProblema,
                DescripcionTipoDocumento = d.DescripcionTipoDocumento,
                DescripcionEstadoDocumento = d.DescripcionEstadoDocumento,
                DescripcionEstadoCobranza = d.DescripcionEstadoCobranza
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documentos de gestión");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene historial de gestiones por documento
    /// </summary>
    [HttpGet("gestiones/historial")]
    public async Task<ActionResult<IEnumerable<GestionCobranzaDto>>> GetHistorialGestiones(
        [FromQuery] long rutCliente, 
        [FromQuery] long rutDeudor, 
        [FromQuery] int numeroOperacion, 
        [FromQuery] int tipoDocumento, 
        [FromQuery] long numeroDocumento)
    {
        try
        {
            var gestiones = await _gestionCobranzaRepo.GetByDocumentoAsync(rutCliente, rutDeudor, numeroOperacion, tipoDocumento, numeroDocumento);
            return Ok(gestiones.Select(g => MapToDto(g)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial de gestiones");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea una nueva gestión de cobranza
    /// </summary>
    [HttpPost("gestiones")]
    public async Task<ActionResult<int>> CreateGestion([FromBody] GestionCobranzaCreateDto dto)
    {
        try
        {
            var gestion = new GestionCobranza
            {
                RutCliente = dto.RutCliente,
                RutDeudor = dto.RutDeudor,
                NumeroOperacion = dto.NumeroOperacion,
                TipoDocumento = dto.TipoDocumento,
                NumeroDocumento = dto.NumeroDocumento,
                CodigoCobrador = dto.CodigoCobrador,
                FechaGestion = dto.FechaGestion,
                HoraGestion = dto.HoraGestion,
                EstadoCobranza = dto.EstadoCobranza,
                FechaPago = dto.FechaPago,
                HoraPago = dto.HoraPago,
                DocumentoNecesario = dto.DocumentoNecesario,
                Observacion = dto.Observacion,
                FechaProximaGestion = dto.FechaProximaGestion,
                HoraProximaGestion = dto.HoraProximaGestion,
                CodigoRecaudador = dto.CodigoRecaudador
            };

            var correlativo = await _gestionCobranzaRepo.CreateAsync(gestion);

            // Actualizar estado en documento
            await _gestionCobranzaRepo.UpdateEstadoDocumentoAsync(
                dto.RutCliente, dto.RutDeudor, dto.NumeroOperacion, dto.NumeroDocumento, 
                dto.TipoDocumento, dto.CodigoCobrador, dto.EstadoCobranza, dto.FechaNotificacion);

            return Ok(correlativo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear gestión de cobranza");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza datos de contacto del deudor
    /// </summary>
    [HttpPut("gestiones/deudor-contacto")]
    public async Task<ActionResult<bool>> UpdateDeudorContacto([FromBody] DeudorGestionUpdateDto dto)
    {
        try
        {
            var result = await _documentoGestionRepo.UpdateDeudorContactoAsync(dto.RutCliente, dto.RutDeudor, dto.ContactoCobranza, dto.DireccionPago);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar contacto de deudor");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Busca gestiones por filtros
    /// </summary>
    [HttpPost("gestiones/buscar")]
    public async Task<ActionResult<IEnumerable<GestionCobranzaDto>>> BuscarGestiones([FromBody] FiltroGestionCobranzaDto filtro)
    {
        try
        {
            var gestiones = await _gestionCobranzaRepo.GetByFiltroAsync(filtro);
            return Ok(gestiones.Select(g => MapToDto(g)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar gestiones");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region Estados de Cobranza (Reportes)

    /// <summary>
    /// Obtiene estados de cobranza por filtros
    /// </summary>
    [HttpPost("estados")]
    public async Task<ActionResult<IEnumerable<EstadoCobranzaDocumentoDto>>> GetEstadosCobranza([FromBody] FiltroEstadoCobranzaDto filtro)
    {
        try
        {
            var estados = await _estadoCobranzaRepo.GetByFiltroAsync(filtro);
            return Ok(estados.Select(e => new EstadoCobranzaDocumentoDto
            {
                RutCliente = e.RutCliente,
                NombreCliente = e.NombreCliente,
                RutDeudor = e.RutDeudor,
                NombreDeudor = e.NombreDeudor,
                TipoDocumento = e.TipoDocumento,
                DescripcionTipoDocumento = e.DescripcionTipoDocumento,
                NumeroDocumento = e.NumeroDocumento,
                FechaVencimiento = e.FechaVencimiento,
                EstadoCobranza = e.EstadoCobranza,
                FechaPago = e.FechaPago,
                MontoDocumento = e.MontoDocumento,
                NombreBanco = e.NombreBanco,
                NombreEjecutivo = e.NombreEjecutivo,
                NombreCobrador = e.NombreCobrador,
                DescripcionEstadoDocumento = e.DescripcionEstadoDocumento,
                DescripcionEstadoCobranza = e.DescripcionEstadoCobranza
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estados de cobranza");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene resumen de estados de cobranza
    /// </summary>
    [HttpPost("estados/resumen")]
    public async Task<ActionResult<ResumenEstadoCobranzaDto>> GetResumenEstados([FromBody] FiltroEstadoCobranzaDto filtro)
    {
        try
        {
            var resumen = await _estadoCobranzaRepo.GetResumenAsync(filtro);
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de estados");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region Deuda Total Cliente

    /// <summary>
    /// Calcula la deuda total de un cliente
    /// </summary>
    [HttpGet("deuda-total/{rutCliente}")]
    public async Task<ActionResult<DeudaTotalClienteDto>> GetDeudaTotal(long rutCliente, [FromQuery] DateTime? fecha = null)
    {
        try
        {
            var fechaCalculo = fecha ?? DateTime.Today;
            var deuda = await _deudaTotalRepo.CalcularDeudaTotalAsync(rutCliente, fechaCalculo);
            
            return Ok(new DeudaTotalClienteDto
            {
                RutCliente = deuda.RutCliente,
                NombreCliente = deuda.NombreCliente,
                FechaCalculo = deuda.FechaCalculo,
                TotalFacturas = deuda.TotalFacturas,
                TotalCheques = deuda.TotalCheques,
                TotalLetras = deuda.TotalLetras,
                TotalOtros = deuda.TotalOtros,
                CuentasPorPagar = deuda.CuentasPorPagar,
                CuentasPorCobrar = deuda.CuentasPorCobrar,
                FacturasNoFactoring = deuda.FacturasNoFactoring,
                Excedentes = deuda.Excedentes,
                DeudaNeta = deuda.DeudaNeta
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular deuda total para cliente {RutCliente}", rutCliente);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene detalle de deuda por tipo de documento
    /// </summary>
    [HttpGet("deuda-total/{rutCliente}/detalle/{tipoDocumento}")]
    public async Task<ActionResult<IEnumerable<DetalleDeudaDocumentoDto>>> GetDetalleDeuda(long rutCliente, int tipoDocumento, [FromQuery] DateTime? fecha = null)
    {
        try
        {
            var fechaCalculo = fecha ?? DateTime.Today;
            var detalle = await _deudaTotalRepo.GetDetalleDeudaByTipoAsync(rutCliente, tipoDocumento, fechaCalculo);
            
            return Ok(detalle.Select(d => new DetalleDeudaDocumentoDto
            {
                RutDeudor = d.RutDeudor,
                NombreDeudor = d.NombreDeudor,
                NumeroOperacion = d.NumeroOperacion,
                TipoDocumentoCodigo = d.TipoDocumentoCodigo,
                NumeroDocumento = d.NumeroDocumento,
                FechaVencimiento = d.FechaVencimiento,
                MontoDocumento = d.MontoDocumento,
                MontoAnticipo = d.MontoAnticipo,
                InteresVencido = d.InteresVencido,
                InteresMora = d.InteresMora,
                InteresNoDevengado = d.InteresNoDevengado,
                EstadoDocumento = d.EstadoDocumento,
                Abono = d.Abono,
                DeudaNeta = d.DeudaNeta
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de deuda");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region Métodos Auxiliares

    private static GestionCobranzaDto MapToDto(GestionCobranza g)
    {
        return new GestionCobranzaDto
        {
            Correlativo = g.Correlativo,
            RutCliente = g.RutCliente,
            RutDeudor = g.RutDeudor,
            TipoDocumento = g.TipoDocumento,
            NumeroDocumento = g.NumeroDocumento,
            NumeroOperacion = g.NumeroOperacion,
            CodigoCobrador = g.CodigoCobrador,
            FechaGestion = g.FechaGestion,
            HoraGestion = g.HoraGestion,
            EstadoCobranza = g.EstadoCobranza,
            FechaPago = g.FechaPago,
            HoraPago = g.HoraPago,
            DocumentoNecesario = g.DocumentoNecesario,
            Observacion = g.Observacion,
            FechaProximaGestion = g.FechaProximaGestion,
            HoraProximaGestion = g.HoraProximaGestion,
            CodigoRecaudador = g.CodigoRecaudador,
            NombreCliente = g.NombreCliente,
            NombreDeudor = g.NombreDeudor,
            NombreCobrador = g.NombreCobrador,
            DescripcionEstadoCobranza = g.DescripcionEstadoCobranza,
            DescripcionTipoDocumento = g.DescripcionTipoDocumento
        };
    }

    #endregion
}
