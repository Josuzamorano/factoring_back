// =============================================
// GruposDeudorController.cs
// Controller for Grupos de Deudores - F_CD1300.frm
// =============================================

using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Factoring.API.Controllers;

[ApiController]
[Route("api/grupos-deudor")]
[Authorize]
public class GruposDeudorController : ControllerBase
{
    private readonly IGrupoDeudorRepository _grupoRepository;
    private readonly IDeudorGrupoRepository _deudorGrupoRepository;
    private readonly ILogger<GruposDeudorController> _logger;

    public GruposDeudorController(
        IGrupoDeudorRepository grupoRepository,
        IDeudorGrupoRepository deudorGrupoRepository,
        ILogger<GruposDeudorController> logger)
    {
        _grupoRepository = grupoRepository;
        _deudorGrupoRepository = deudorGrupoRepository;
        _logger = logger;
    }

    #region Grupos de Deudores

    /// <summary>
    /// Obtener todos los grupos de deudores
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GrupoDeudorDto>>> GetAll()
    {
        try
        {
            var grupos = await _grupoRepository.GetAllAsync();
            var dtos = grupos.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener grupos de deudores");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar grupos de deudores
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<GrupoDeudorDto>>> Search([FromQuery] string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(new { message = "Debe proporcionar un término de búsqueda" });

            var grupos = await _grupoRepository.SearchAsync(term);
            var dtos = grupos.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar grupos de deudores");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un grupo por RUT
    /// </summary>
    [HttpGet("{rut:long}")]
    public async Task<ActionResult<GrupoDeudorDto>> GetByRut(long rut)
    {
        try
        {
            var grupo = await _grupoRepository.GetByRutAsync(rut);
            if (grupo == null)
                return NotFound(new { message = "Grupo no encontrado" });

            return Ok(MapToDto(grupo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener grupo {Rut}", rut);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener cupo de un grupo
    /// </summary>
    [HttpGet("{rut:long}/cupo")]
    public async Task<ActionResult<CupoGrupoDto>> GetCupo(long rut)
    {
        try
        {
            var grupo = await _grupoRepository.GetByRutAsync(rut);
            if (grupo == null)
                return NotFound(new { message = "Grupo no encontrado" });

            var cupo = await _grupoRepository.GetCupoAsync(rut);

            return Ok(new CupoGrupoDto
            {
                RutGrupo = rut,
                RazonSocial = grupo.RazonSocial,
                MontoLinea = cupo.Linea,
                MontoOcupado = cupo.Ocupado,
                MontoDisponible = cupo.Disponible,
                PorcentajeUtilizacion = cupo.Linea > 0 ? (cupo.Ocupado / cupo.Linea) * 100 : 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cupo del grupo {Rut}", rut);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crear un nuevo grupo de deudores
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GrupoDeudorDto>> Create([FromBody] CreateGrupoDeudorDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar si ya existe
            if (await _grupoRepository.ExistsAsync(dto.RutGrupo))
                return Conflict(new { message = "Ya existe un grupo con este RUT" });

            var grupo = new GrupoDeudor
            {
                RutGrupo = dto.RutGrupo,
                RazonSocial = dto.RazonSocial.ToUpper(),
                MontoLineaAprobada = dto.MontoLineaAprobada,
                FechaVencimiento = dto.FechaVencimiento,
                UsuarioCreacion = User.Identity?.Name
            };

            await _grupoRepository.CreateAsync(grupo);

            var created = await _grupoRepository.GetByRutAsync(dto.RutGrupo);
            return CreatedAtAction(nameof(GetByRut), new { rut = dto.RutGrupo }, MapToDto(created!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear grupo de deudores");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualizar un grupo de deudores
    /// </summary>
    [HttpPut("{rut:long}")]
    public async Task<ActionResult<GrupoDeudorDto>> Update(long rut, [FromBody] UpdateGrupoDeudorDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grupo = await _grupoRepository.GetByRutAsync(rut);
            if (grupo == null)
                return NotFound(new { message = "Grupo no encontrado" });

            grupo.RazonSocial = dto.RazonSocial.ToUpper();
            grupo.MontoLineaAprobada = dto.MontoLineaAprobada;
            grupo.FechaVencimiento = dto.FechaVencimiento;
            grupo.UsuarioModificacion = User.Identity?.Name;

            await _grupoRepository.UpdateAsync(grupo);

            var Updated = await _grupoRepository.GetByRutAsync(rut);
            return Ok(MapToDto(Updated!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar grupo {Rut}", rut);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Eliminar un grupo de deudores
    /// </summary>
    [HttpDelete("{rut:long}")]
    public async Task<ActionResult> Delete(long rut)
    {
        try
        {
            var grupo = await _grupoRepository.GetByRutAsync(rut);
            if (grupo == null)
                return NotFound(new { message = "Grupo no encontrado" });

            await _grupoRepository.DeleteAsync(rut);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar grupo {Rut}", rut);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    #endregion

    #region Deudores del Grupo

    /// <summary>
    /// Obtener deudores de un grupo
    /// </summary>
    [HttpGet("{rut:long}/deudores")]
    public async Task<ActionResult<IEnumerable<DeudorGrupoDto>>> GetDeudores(long rut)
    {
        try
        {
            var grupo = await _grupoRepository.GetByRutAsync(rut);
            if (grupo == null)
                return NotFound(new { message = "Grupo no encontrado" });

            var deudores = await _deudorGrupoRepository.GetByGrupoAsync(rut);
            var dtos = deudores.Select(MapDeudorGrupoToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener deudores del grupo {Rut}", rut);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Agregar deudor a un grupo
    /// </summary>
    [HttpPost("{rut:long}/deudores")]
    public async Task<ActionResult<DeudorGrupoDto>> AddDeudor(long rut, [FromBody] CreateDeudorGrupoDto dto)
    {
        try
        {
            var grupo = await _grupoRepository.GetByRutAsync(rut);
            if (grupo == null)
                return NotFound(new { message = "Grupo no encontrado" });

            // Verificar si el deudor ya está en el grupo
            if (await _deudorGrupoRepository.ExistsAsync(rut, dto.RutDeudor))
                return Conflict(new { message = "El deudor ya pertenece a este grupo" });

            var deudorGrupo = new DeudorGrupo
            {
                RutGrupo = rut,
                RutDeudor = dto.RutDeudor,
                UsuarioCreacion = User.Identity?.Name
            };

            var id = await _deudorGrupoRepository.CreateAsync(deudorGrupo);
            var created = await _deudorGrupoRepository.GetByIdAsync(id);

            return CreatedAtAction(nameof(GetDeudores), new { rut }, MapDeudorGrupoToDto(created!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar deudor al grupo {Rut}", rut);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Eliminar deudor de un grupo por ID
    /// </summary>
    [HttpDelete("{rut:long}/deudores/{id:int}")]
    public async Task<ActionResult> RemoveDeudorById(long rut, int id)
    {
        try
        {
            var deudorGrupo = await _deudorGrupoRepository.GetByIdAsync(id);
            if (deudorGrupo == null || deudorGrupo.RutGrupo != rut)
                return NotFound(new { message = "Relación no encontrada" });

            await _deudorGrupoRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar deudor {Id} del grupo {Rut}", id, rut);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Eliminar deudor de un grupo por RUT
    /// </summary>
    [HttpDelete("{rutGrupo:long}/deudores/rut/{rutDeudor:long}")]
    public async Task<ActionResult> RemoveDeudorByRut(long rutGrupo, long rutDeudor)
    {
        try
        {
            if (!await _deudorGrupoRepository.ExistsAsync(rutGrupo, rutDeudor))
                return NotFound(new { message = "El deudor no pertenece a este grupo" });

            await _deudorGrupoRepository.DeleteByRutAsync(rutGrupo, rutDeudor);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar deudor {RutDeudor} del grupo {RutGrupo}", rutDeudor, rutGrupo);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    #endregion

    #region Private Methods

    private static GrupoDeudorDto MapToDto(GrupoDeudor grupo)
    {
        return new GrupoDeudorDto
        {
            RutGrupo = grupo.RutGrupo,
            DigitoVerificador = CalcularDigitoVerificador(grupo.RutGrupo),
            RazonSocial = grupo.RazonSocial,
            MontoLineaAprobada = grupo.MontoLineaAprobada,
            FechaVencimiento = grupo.FechaVencimiento,
            FechaCreacion = grupo.FechaCreacion,
            FechaModificacion = grupo.FechaModificacion,
            CantidadDeudores = grupo.CantidadDeudores,
            MontoOcupado = grupo.MontoOcupado,
            MontoDisponible = grupo.MontoDisponible
        };
    }

    private static DeudorGrupoDto MapDeudorGrupoToDto(DeudorGrupo deudor)
    {
        return new DeudorGrupoDto
        {
            Id = deudor.Id,
            RutGrupo = deudor.RutGrupo,
            NombreGrupo = deudor.NombreGrupo ?? string.Empty,
            RutDeudor = deudor.RutDeudor,
            DigitoVerificador = CalcularDigitoVerificador(deudor.RutDeudor),
            NombreDeudor = deudor.NombreDeudor ?? string.Empty,
            NombreFantasia = deudor.NombreFantasia,
            Giro = deudor.Giro,
            FechaCreacion = deudor.FechaCreacion
        };
    }

    private static string CalcularDigitoVerificador(long rut)
    {
        int suma = 0;
        int multiplicador = 2;
        string rutStr = rut.ToString();

        for (int i = rutStr.Length - 1; i >= 0; i--)
        {
            suma += int.Parse(rutStr[i].ToString()) * multiplicador;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
        }

        int resto = suma % 11;
        int digito = 11 - resto;

        if (digito == 11) return "0";
        if (digito == 10) return "K";
        return digito.ToString();
    }

    #endregion
}
