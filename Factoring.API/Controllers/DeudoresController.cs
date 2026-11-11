// =============================================
// Controller para Módulo 3 - Carpeta Deudor
// API REST para gestión de deudores
// =============================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;

namespace Factoring.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DeudoresController : ControllerBase
{
    private readonly IDeudorRepository _deudorRepository;
    private readonly IDeudorTelefonoRepository _telefonoRepository;
    private readonly IDeudorDocumentoRepository _documentoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger<DeudoresController> _logger;

    public DeudoresController(
        IDeudorRepository deudorRepository,
        IDeudorTelefonoRepository telefonoRepository,
        IDeudorDocumentoRepository documentoRepository,
        IClienteRepository clienteRepository,
        ILogger<DeudoresController> logger)
    {
        _deudorRepository = deudorRepository;
        _telefonoRepository = telefonoRepository;
        _documentoRepository = documentoRepository;
        _clienteRepository = clienteRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los deudores
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeudorListDto>>> GetAll()
    {
        try
        {
            var deudores = await _deudorRepository.GetAllAsync();
            var result = deudores.Select(MapToListDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener deudores");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene deudores por cliente
    /// </summary>
    [HttpGet("cliente/{rutCliente}")]
    public async Task<ActionResult<IEnumerable<DeudorListDto>>> GetByCliente(int rutCliente)
    {
        try
        {
            var deudores = await _deudorRepository.GetByClienteAsync(rutCliente);
            var result = deudores.Select(MapToListDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener deudores del cliente {RutCliente}", rutCliente);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Busca deudores con filtros y paginación
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<object>> Search(
        [FromQuery] int? rutCliente,
        [FromQuery] string? rutDeudor,
        [FromQuery] string? razonSocial,
        [FromQuery] int? estado,
        [FromQuery] int? categoriaRiesgo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var (items, totalCount) = await _deudorRepository.SearchAsync(
                rutCliente, rutDeudor, razonSocial, estado, categoriaRiesgo, page, pageSize);

            return Ok(new
            {
                items = items.Select(MapToListDto),
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en búsqueda de deudores");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un deudor específico con sus teléfonos y documentos
    /// </summary>
    [HttpGet("{rutCliente}/{rutDeudor}")]
    public async Task<ActionResult<DeudorDto>> GetById(int rutCliente, int rutDeudor)
    {
        try
        {
            var deudor = await _deudorRepository.GetByIdAsync(rutCliente, rutDeudor);
            if (deudor == null)
            {
                return NotFound(new { message = "Deudor no encontrado" });
            }

            // Cargar teléfonos y documentos
            var telefonos = await _telefonoRepository.GetByDeudorAsync(rutCliente, rutDeudor);
            var documentos = await _documentoRepository.GetByDeudorAsync(rutCliente, rutDeudor);

            var dto = MapToDto(deudor);
            
            // Retornar con detalle adicional
            return Ok(new 
            {
                dto.RutCliente,
                dto.RutDeudor,
                dto.DigitoVerificador,
                dto.RutFormateado,
                dto.RazonSocial,
                dto.Direccion,
                dto.CodigoComuna,
                dto.NombreComuna,
                dto.EstadoDeudor,
                dto.EstadoDescripcion,
                dto.ContactoCobranza,
                dto.PorcentajeVentas,
                dto.Notificacion,
                dto.MontoOtorgado,
                dto.MontoOcupado,
                dto.MontoDisponible,
                dto.CategoriaRiesgo,
                dto.DescripcionCategoriaRiesgo,
                dto.NombreCliente,
                dto.Activo,
                dto.FechaCreacion,
                GiroDeudor = deudor.GiroDeudor,
                Telefonos = telefonos.Select(t => new DeudorTelefonoDto
                {
                    RutCliente = t.RutCliente,
                    RutDeudor = t.RutDeudor,
                    TipoComunicacion = t.TipoComunicacion,
                    Descripcion = t.Descripcion,
                    NumeroComunicacion = t.NumeroComunicacion ?? ""
                }).ToList(),
                Documentos = documentos.Select(d => new DeudorDocumentoDto
                {
                    RutCliente = d.RutCliente,
                    RutDeudor = d.RutDeudor,
                    NumeroDocumento = d.NumeroDocumento,
                    TipoDocumento = d.TipoDocumento,
                    NombreDocumento = d.NombreDocumento ?? "",
                    RutaDocumento = d.RutaDocumento,
                    FechaDocumento = d.FechaDocumento,
                    Observaciones = d.Observaciones
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener deudor {RutCliente}/{RutDeudor}", rutCliente, rutDeudor);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea un nuevo deudor
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DeudorDto>> Create([FromBody] DeudorCreateDto dto)
    {
        try
        {
            // Validar que exista el cliente
            var cliente = await _clienteRepository.GetByRutAsync(dto.RutCliente);
            if (cliente == null)
            {
                return BadRequest(new { message = "El cliente no existe" });
            }

            // Verificar si ya existe
            if (await _deudorRepository.ExistsAsync(dto.RutCliente, dto.RutDeudor))
            {
                return BadRequest(new { message = "El deudor ya existe para este cliente" });
            }

            var deudor = new Deudor
            {
                RutCliente = dto.RutCliente,
                RutDeudor = dto.RutDeudor,
                DigitoVerificador = dto.DigitoVerificador,
                RazonSocial = dto.RazonSocial?.ToUpper(),
                Direccion = dto.Direccion?.ToUpper(),
                CodigoComuna = dto.CodigoComuna,
                ContactoCobranza = dto.ContactoCobranza,
                PorcentajeVentas = dto.PorcentajeVentas,
                Notificacion = dto.Notificacion,
                MontoOtorgado = dto.MontoOtorgado,
                CategoriaRiesgo = dto.CategoriaRiesgo,
                GiroDeudor = dto.GiroDeudor?.ToUpper(),
                EstadoDeudor = 1
            };

            await _deudorRepository.CreateAsync(deudor);

            _logger.LogInformation("Deudor creado: {RutCliente}/{RutDeudor}", dto.RutCliente, dto.RutDeudor);

            var created = await _deudorRepository.GetByIdAsync(dto.RutCliente, dto.RutDeudor);
            return CreatedAtAction(nameof(GetById), 
                new { rutCliente = dto.RutCliente, rutDeudor = dto.RutDeudor }, 
                MapToDto(created!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear deudor");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza un deudor existente
    /// </summary>
    [HttpPut("{rutCliente}/{rutDeudor}")]
    public async Task<ActionResult> Update(int rutCliente, int rutDeudor, [FromBody] DeudorUpdateDto dto)
    {
        try
        {
            var deudor = await _deudorRepository.GetByIdAsync(rutCliente, rutDeudor);
            if (deudor == null)
            {
                return NotFound(new { message = "Deudor no encontrado" });
            }

            deudor.RazonSocial = dto.RazonSocial?.ToUpper() ?? deudor.RazonSocial;
            deudor.Direccion = dto.Direccion?.ToUpper();
            deudor.CodigoComuna = dto.CodigoComuna;
            deudor.EstadoDeudor = dto.EstadoDeudor;
            deudor.ContactoCobranza = dto.ContactoCobranza;
            deudor.PorcentajeVentas = dto.PorcentajeVentas;
            deudor.Notificacion = dto.Notificacion;
            deudor.MontoOtorgado = dto.MontoOtorgado;
            deudor.CategoriaRiesgo = dto.CategoriaRiesgo;
            deudor.GiroDeudor = dto.GiroDeudor?.ToUpper();

            await _deudorRepository.UpdateAsync(deudor);

            _logger.LogInformation("Deudor actualizado: {RutCliente}/{RutDeudor}", rutCliente, rutDeudor);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar deudor {RutCliente}/{RutDeudor}", rutCliente, rutDeudor);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina (desactiva) un deudor
    /// </summary>
    [HttpDelete("{rutCliente}/{rutDeudor}")]
    public async Task<ActionResult> Delete(int rutCliente, int rutDeudor)
    {
        try
        {
            var deudor = await _deudorRepository.GetByIdAsync(rutCliente, rutDeudor);
            if (deudor == null)
            {
                return NotFound(new { message = "Deudor no encontrado" });
            }

            // Verificar si tiene operaciones pendientes
            if (deudor.MontoOcupado > 0)
            {
                return BadRequest(new { message = "No se puede eliminar un deudor con monto ocupado" });
            }

            await _deudorRepository.DeleteAsync(rutCliente, rutDeudor);

            _logger.LogInformation("Deudor eliminado: {RutCliente}/{RutDeudor}", rutCliente, rutDeudor);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar deudor {RutCliente}/{RutDeudor}", rutCliente, rutDeudor);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // ========================================
    // Endpoints para Teléfonos
    // ========================================

    /// <summary>
    /// Obtiene teléfonos de un deudor
    /// </summary>
    [HttpGet("{rutCliente}/{rutDeudor}/telefonos")]
    public async Task<ActionResult<IEnumerable<DeudorTelefonoDto>>> GetTelefonos(int rutCliente, int rutDeudor)
    {
        try
        {
            var telefonos = await _telefonoRepository.GetByDeudorAsync(rutCliente, rutDeudor);
            var result = telefonos.Select(t => new DeudorTelefonoDto
            {
                RutCliente = t.RutCliente,
                RutDeudor = t.RutDeudor,
                TipoComunicacion = t.TipoComunicacion,
                Descripcion = t.Descripcion,
                NumeroComunicacion = t.NumeroComunicacion ?? ""
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener teléfonos del deudor");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Agrega un teléfono al deudor
    /// </summary>
    [HttpPost("{rutCliente}/{rutDeudor}/telefonos")]
    public async Task<ActionResult> AddTelefono(int rutCliente, int rutDeudor, [FromBody] DeudorTelefonoCreateDto dto)
    {
        try
        {
            var deudor = await _deudorRepository.GetByIdAsync(rutCliente, rutDeudor);
            if (deudor == null)
            {
                return NotFound(new { message = "Deudor no encontrado" });
            }

            var telefono = new DeudorTelefono
            {
                RutCliente = rutCliente,
                RutDeudor = rutDeudor,
                TipoComunicacion = dto.TipoComunicacion,
                Descripcion = dto.Descripcion,
                NumeroComunicacion = dto.NumeroComunicacion
            };

            await _telefonoRepository.CreateAsync(telefono);
            return Ok(new { message = "Teléfono agregado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar teléfono al deudor");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina un teléfono del deudor
    /// </summary>
    [HttpDelete("{rutCliente}/{rutDeudor}/telefonos/{tipo}/{numero}")]
    public async Task<ActionResult> DeleteTelefono(int rutCliente, int rutDeudor, int tipo, string numero)
    {
        try
        {
            await _telefonoRepository.DeleteAsync(rutCliente, rutDeudor, tipo, numero);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar teléfono del deudor");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // ========================================
    // Endpoints para Documentos
    // ========================================

    /// <summary>
    /// Obtiene documentos de un deudor
    /// </summary>
    [HttpGet("{rutCliente}/{rutDeudor}/documentos")]
    public async Task<ActionResult<IEnumerable<DeudorDocumentoDto>>> GetDocumentos(int rutCliente, int rutDeudor)
    {
        try
        {
            var documentos = await _documentoRepository.GetByDeudorAsync(rutCliente, rutDeudor);
            var result = documentos.Select(d => new DeudorDocumentoDto
            {
                RutCliente = d.RutCliente,
                RutDeudor = d.RutDeudor,
                NumeroDocumento = d.NumeroDocumento,
                TipoDocumento = d.TipoDocumento,
                NombreDocumento = d.NombreDocumento ?? "",
                RutaDocumento = d.RutaDocumento,
                FechaDocumento = d.FechaDocumento,
                Observaciones = d.Observaciones
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documentos del deudor");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Agrega un documento al deudor
    /// </summary>
    [HttpPost("{rutCliente}/{rutDeudor}/documentos")]
    public async Task<ActionResult> AddDocumento(int rutCliente, int rutDeudor, [FromBody] DeudorDocumentoCreateDto dto)
    {
        try
        {
            var deudor = await _deudorRepository.GetByIdAsync(rutCliente, rutDeudor);
            if (deudor == null)
            {
                return NotFound(new { message = "Deudor no encontrado" });
            }

            var documento = new DeudorDocumento
            {
                RutCliente = rutCliente,
                RutDeudor = rutDeudor,
                TipoDocumento = dto.TipoDocumento,
                NombreDocumento = dto.NombreDocumento,
                RutaDocumento = dto.RutaDocumento,
                FechaDocumento = dto.FechaDocumento ?? DateTime.Today,
                Observaciones = dto.Observaciones
            };

            await _documentoRepository.CreateAsync(documento);
            return Ok(new { message = "Documento agregado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar documento al deudor");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Elimina un documento del deudor
    /// </summary>
    [HttpDelete("{rutCliente}/{rutDeudor}/documentos/{numeroDoc}")]
    public async Task<ActionResult> DeleteDocumento(int rutCliente, int rutDeudor, int numeroDoc)
    {
        try
        {
            await _documentoRepository.DeleteAsync(rutCliente, rutDeudor, numeroDoc);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar documento del deudor");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // ========================================
    // Métodos de ayuda
    // ========================================

    private static DeudorListDto MapToListDto(Deudor d) => new()
    {
        RutCliente = d.RutCliente,
        RutDeudor = d.RutDeudor,
        RutFormateado = FormatRut(d.RutDeudor, d.DigitoVerificador),
        RazonSocial = d.RazonSocial ?? "",
        NombreCliente = d.NombreCliente,
        MontoOtorgado = d.MontoOtorgado,
        MontoOcupado = d.MontoOcupado,
        MontoDisponible = Math.Max(0, d.MontoOtorgado - d.MontoOcupado),
        EstadoDeudor = d.EstadoDeudor,
        Activo = d.EstadoDeudor == 1
    };

    private static DeudorDto MapToDto(Deudor d) => new()
    {
        RutCliente = d.RutCliente,
        RutDeudor = d.RutDeudor,
        DigitoVerificador = d.DigitoVerificador,
        RutFormateado = FormatRut(d.RutDeudor, d.DigitoVerificador),
        RazonSocial = d.RazonSocial ?? "",
        Direccion = d.Direccion,
        CodigoComuna = d.CodigoComuna,
        NombreComuna = d.NombreComuna,
        EstadoDeudor = d.EstadoDeudor,
        EstadoDescripcion = d.EstadoDeudor == 1 ? "Activo" : "Inactivo",
        ContactoCobranza = d.ContactoCobranza,
        PorcentajeVentas = d.PorcentajeVentas,
        Notificacion = d.Notificacion,
        MontoOtorgado = d.MontoOtorgado,
        MontoOcupado = d.MontoOcupado,
        MontoDisponible = Math.Max(0, d.MontoOtorgado - d.MontoOcupado),
        CategoriaRiesgo = d.CategoriaRiesgo,
        DescripcionCategoriaRiesgo = d.DescripcionCategoriaRiesgo,
        NombreCliente = d.NombreCliente,
        Activo = d.EstadoDeudor == 1,
        FechaCreacion = d.FechaCreacion
    };

    private static string FormatRut(int rut, string? dv)
    {
        var rutStr = rut.ToString("N0", new System.Globalization.CultureInfo("es-CL"));
        return $"{rutStr}-{dv ?? "0"}";
    }
}
