using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;

namespace Factoring.API.Controllers;

/// <summary>
/// Controller de Bancos - Migrado de F_MA1300.frm
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BancosController : ControllerBase
{
    private readonly IBancoRepository _bancoRepository;

    public BancosController(IBancoRepository bancoRepository)
    {
        _bancoRepository = bancoRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BancoDto>>> GetAll()
    {
        var bancos = await _bancoRepository.GetAllAsync();
        var result = bancos.Select(b => new BancoDto
        {
            Codigo = b.Codigo,
            Descripcion = b.Descripcion,
            PermiteDeposito = b.PermiteDeposito,
            CuentaCorriente = b.CuentaCorriente,
            CuentaContable = b.CuentaContable,
            TransferenciaElectronica = b.TransferenciaElectronica,
            Estado = b.Estado
        });
        return Ok(result);
    }

    [HttpGet("activos")]
    public async Task<ActionResult<IEnumerable<SelectItemDto>>> GetActivos()
    {
        var bancos = await _bancoRepository.GetActiveAsync();
        var result = bancos.Select(b => new SelectItemDto
        {
            Codigo = b.Codigo,
            Descripcion = b.Descripcion
        });
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BancoDto>> GetById(int id)
    {
        var banco = await _bancoRepository.GetByIdAsync(id);
        if (banco == null)
            return NotFound(new { mensaje = "Banco no encontrado" });

        return Ok(new BancoDto
        {
            Codigo = banco.Codigo,
            Descripcion = banco.Descripcion,
            PermiteDeposito = banco.PermiteDeposito,
            CuentaCorriente = banco.CuentaCorriente,
            CuentaContable = banco.CuentaContable,
            TransferenciaElectronica = banco.TransferenciaElectronica,
            Estado = banco.Estado
        });
    }

    [HttpPost]
    public async Task<ActionResult<BancoDto>> Create([FromBody] BancoCreateDto dto)
    {
        if (await _bancoRepository.ExistsAsync(dto.Codigo))
            return BadRequest(new { mensaje = "El código de banco ya existe" });

        var banco = new Banco
        {
            Codigo = dto.Codigo,
            Descripcion = dto.Descripcion,
            PermiteDeposito = dto.PermiteDeposito,
            CuentaCorriente = dto.CuentaCorriente,
            CuentaContable = dto.CuentaContable,
            TransferenciaElectronica = dto.TransferenciaElectronica
        };

        var result = await _bancoRepository.CreateAsync(banco);
        if (!result)
            return BadRequest(new { mensaje = "Error al crear el banco" });

        return CreatedAtAction(nameof(GetById), new { id = banco.Codigo }, dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] BancoUpdateDto dto)
    {
        var banco = await _bancoRepository.GetByIdAsync(id);
        if (banco == null)
            return NotFound(new { mensaje = "Banco no encontrado" });

        banco.Descripcion = dto.Descripcion;
        banco.PermiteDeposito = dto.PermiteDeposito;
        banco.CuentaCorriente = dto.CuentaCorriente;
        banco.CuentaContable = dto.CuentaContable;
        banco.TransferenciaElectronica = dto.TransferenciaElectronica;
        banco.Estado = dto.Estado;

        var result = await _bancoRepository.UpdateAsync(banco);
        if (!result)
            return BadRequest(new { mensaje = "Error al actualizar el banco" });

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        if (!await _bancoRepository.ExistsAsync(id))
            return NotFound(new { mensaje = "Banco no encontrado" });

        var result = await _bancoRepository.DeleteAsync(id);
        if (!result)
            return BadRequest(new { mensaje = "Error al eliminar el banco" });

        return NoContent();
    }
}

/// <summary>
/// Controller de Plazas - Migrado de F_MA1400.frm
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PlazasController : ControllerBase
{
    private readonly IPlazaRepository _plazaRepository;

    public PlazasController(IPlazaRepository plazaRepository)
    {
        _plazaRepository = plazaRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlazaDto>>> GetAll()
    {
        var plazas = await _plazaRepository.GetAllAsync();
        var result = plazas.Select(p => new PlazaDto
        {
            Codigo = p.Codigo,
            Descripcion = p.Descripcion,
            DiasCanje = p.DiasCanje,
            PlazaFuera = p.PlazaFuera,
            Estado = p.Estado
        });
        return Ok(result);
    }

    [HttpGet("activos")]
    public async Task<ActionResult<IEnumerable<SelectItemDto>>> GetActivos()
    {
        var plazas = await _plazaRepository.GetActiveAsync();
        var result = plazas.Select(p => new SelectItemDto
        {
            Codigo = p.Codigo,
            Descripcion = p.Descripcion
        });
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlazaDto>> GetById(int id)
    {
        var plaza = await _plazaRepository.GetByIdAsync(id);
        if (plaza == null)
            return NotFound(new { mensaje = "Plaza no encontrada" });

        return Ok(new PlazaDto
        {
            Codigo = plaza.Codigo,
            Descripcion = plaza.Descripcion,
            DiasCanje = plaza.DiasCanje,
            PlazaFuera = plaza.PlazaFuera,
            Estado = plaza.Estado
        });
    }

    [HttpPost]
    public async Task<ActionResult<PlazaDto>> Create([FromBody] PlazaCreateDto dto)
    {
        if (await _plazaRepository.ExistsAsync(dto.Codigo))
            return BadRequest(new { mensaje = "El código de plaza ya existe" });

        var plaza = new Plaza
        {
            Codigo = dto.Codigo,
            Descripcion = dto.Descripcion,
            DiasCanje = dto.DiasCanje,
            PlazaFuera = dto.PlazaFuera
        };

        var result = await _plazaRepository.CreateAsync(plaza);
        if (!result)
            return BadRequest(new { mensaje = "Error al crear la plaza" });

        return CreatedAtAction(nameof(GetById), new { id = plaza.Codigo }, dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] PlazaUpdateDto dto)
    {
        var plaza = await _plazaRepository.GetByIdAsync(id);
        if (plaza == null)
            return NotFound(new { mensaje = "Plaza no encontrada" });

        plaza.Descripcion = dto.Descripcion;
        plaza.DiasCanje = dto.DiasCanje;
        plaza.PlazaFuera = dto.PlazaFuera;
        plaza.Estado = dto.Estado;

        var result = await _plazaRepository.UpdateAsync(plaza);
        if (!result)
            return BadRequest(new { mensaje = "Error al actualizar la plaza" });

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        if (!await _plazaRepository.ExistsAsync(id))
            return NotFound(new { mensaje = "Plaza no encontrada" });

        var result = await _plazaRepository.DeleteAsync(id);
        if (!result)
            return BadRequest(new { mensaje = "Error al eliminar la plaza" });

        return NoContent();
    }
}

/// <summary>
/// Controller de Sucursales
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SucursalesController : ControllerBase
{
    private readonly ISucursalRepository _sucursalRepository;

    public SucursalesController(ISucursalRepository sucursalRepository)
    {
        _sucursalRepository = sucursalRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SucursalDto>>> GetAll()
    {
        var sucursales = await _sucursalRepository.GetAllAsync();
        var result = sucursales.Select(s => new SucursalDto
        {
            Codigo = s.Codigo,
            Descripcion = s.Descripcion,
            Direccion = s.Direccion,
            Telefono = s.Telefono,
            Email = s.Email,
            Estado = s.Estado
        });
        return Ok(result);
    }

    [HttpGet("activos")]
    public async Task<ActionResult<IEnumerable<SelectItemDto>>> GetActivos()
    {
        var sucursales = await _sucursalRepository.GetActiveAsync();
        var result = sucursales.Select(s => new SelectItemDto
        {
            Codigo = s.Codigo,
            Descripcion = s.Descripcion
        });
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SucursalDto>> GetById(int id)
    {
        var sucursal = await _sucursalRepository.GetByIdAsync(id);
        if (sucursal == null)
            return NotFound(new { mensaje = "Sucursal no encontrada" });

        return Ok(new SucursalDto
        {
            Codigo = sucursal.Codigo,
            Descripcion = sucursal.Descripcion,
            Direccion = sucursal.Direccion,
            Telefono = sucursal.Telefono,
            Email = sucursal.Email,
            Estado = sucursal.Estado
        });
    }

    [HttpPost]
    public async Task<ActionResult<SucursalDto>> Create([FromBody] SucursalCreateDto dto)
    {
        if (await _sucursalRepository.ExistsAsync(dto.Codigo))
            return BadRequest(new { mensaje = "El código de sucursal ya existe" });

        var sucursal = new Sucursal
        {
            Codigo = dto.Codigo,
            Descripcion = dto.Descripcion,
            Direccion = dto.Direccion,
            Telefono = dto.Telefono,
            Email = dto.Email
        };

        var result = await _sucursalRepository.CreateAsync(sucursal);
        if (!result)
            return BadRequest(new { mensaje = "Error al crear la sucursal" });

        return CreatedAtAction(nameof(GetById), new { id = sucursal.Codigo }, dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] SucursalUpdateDto dto)
    {
        var sucursal = await _sucursalRepository.GetByIdAsync(id);
        if (sucursal == null)
            return NotFound(new { mensaje = "Sucursal no encontrada" });

        sucursal.Descripcion = dto.Descripcion;
        sucursal.Direccion = dto.Direccion;
        sucursal.Telefono = dto.Telefono;
        sucursal.Email = dto.Email;
        sucursal.Estado = dto.Estado;

        var result = await _sucursalRepository.UpdateAsync(sucursal);
        if (!result)
            return BadRequest(new { mensaje = "Error al actualizar la sucursal" });

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        if (!await _sucursalRepository.ExistsAsync(id))
            return NotFound(new { mensaje = "Sucursal no encontrada" });

        var result = await _sucursalRepository.DeleteAsync(id);
        if (!result)
            return BadRequest(new { mensaje = "Error al eliminar la sucursal" });

        return NoContent();
    }
}

/// <summary>
/// Controller de Parámetros - Migrado de F_MA1100.frm
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ParametrosController : ControllerBase
{
    private readonly IParametroRepository _parametroRepository;

    public ParametrosController(IParametroRepository parametroRepository)
    {
        _parametroRepository = parametroRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParametroDto>>> GetAll()
    {
        var parametros = await _parametroRepository.GetAllAsync();
        var result = parametros.Select(p => new ParametroDto
        {
            CodigoTabla = p.CodigoTabla,
            CodigoParametro = p.CodigoParametro,
            Descripcion = p.Descripcion,
            CodigoAuxiliar = p.CodigoAuxiliar,
            ValorNumerico = p.ValorNumerico,
            ValorTexto = p.ValorTexto,
            Estado = p.Estado
        });
        return Ok(result);
    }

    [HttpGet("tabla/{codigoTabla}")]
    public async Task<ActionResult<IEnumerable<ParametroDto>>> GetByTabla(int codigoTabla)
    {
        var parametros = await _parametroRepository.GetByTablaAsync(codigoTabla);
        var result = parametros.Select(p => new ParametroDto
        {
            CodigoTabla = p.CodigoTabla,
            CodigoParametro = p.CodigoParametro,
            Descripcion = p.Descripcion,
            CodigoAuxiliar = p.CodigoAuxiliar,
            ValorNumerico = p.ValorNumerico,
            ValorTexto = p.ValorTexto,
            Estado = p.Estado
        });
        return Ok(result);
    }

    [HttpGet("tabla/{codigoTabla}/select")]
    public async Task<ActionResult<IEnumerable<SelectItemDto>>> GetSelectByTabla(int codigoTabla)
    {
        var parametros = await _parametroRepository.GetByTablaAsync(codigoTabla);
        var result = parametros.Select(p => new SelectItemDto
        {
            Codigo = p.CodigoParametro,
            Descripcion = p.Descripcion
        });
        return Ok(result);
    }

    [HttpGet("{codigoTabla}/{codigoParametro}")]
    public async Task<ActionResult<ParametroDto>> GetById(int codigoTabla, int codigoParametro)
    {
        var parametro = await _parametroRepository.GetByIdAsync(codigoTabla, codigoParametro);
        if (parametro == null)
            return NotFound(new { mensaje = "Parámetro no encontrado" });

        return Ok(new ParametroDto
        {
            CodigoTabla = parametro.CodigoTabla,
            CodigoParametro = parametro.CodigoParametro,
            Descripcion = parametro.Descripcion,
            CodigoAuxiliar = parametro.CodigoAuxiliar,
            ValorNumerico = parametro.ValorNumerico,
            ValorTexto = parametro.ValorTexto,
            Estado = parametro.Estado
        });
    }
}
