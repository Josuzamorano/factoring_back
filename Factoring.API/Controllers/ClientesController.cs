using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;

namespace Factoring.API.Controllers;

/// <summary>
/// Controller de Clientes - Migrado de F_CC1700.frm
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private static readonly ConcurrentDictionary<long, ConcurrentDictionary<int, ClienteOrganigramaItem>> Organigramas = new();
    private static readonly ConcurrentDictionary<long, ConcurrentDictionary<long, ClienteEmpresaRelacionadaItem>> EmpresasRelacionadas = new();

    private readonly IClienteRepository _clienteRepository;
    private readonly IClienteTelefonoRepository _telefonoRepository;
    private readonly IClienteContactoRepository _contactoRepository;
    private readonly IClienteBancoRepository _bancoRepository;

    public ClientesController(
        IClienteRepository clienteRepository,
        IClienteTelefonoRepository telefonoRepository,
        IClienteContactoRepository contactoRepository,
        IClienteBancoRepository bancoRepository)
    {
        _clienteRepository = clienteRepository;
        _telefonoRepository = telefonoRepository;
        _contactoRepository = contactoRepository;
        _bancoRepository = bancoRepository;
    }

    private sealed class ClienteOrganigramaItem
    {
        public long RutCliente { get; set; }
        public int NumeroOrganigrama { get; set; }
        public string NombrePersona { get; set; } = string.Empty;
        public long? RutPersona { get; set; }
        public string? DigitoVerificador { get; set; }
        public int? CodigoCargo { get; set; }
        public string? DescripcionCargo { get; set; }
        public bool AtributoFirma { get; set; }
        public int Estado { get; set; } = 1;
        public bool Activo => Estado == 0;
    }

    private sealed class ClienteEmpresaRelacionadaItem
    {
        public long RutCliente { get; set; }
        public long RutEmpresa { get; set; }
        public string? DigitoVerificador { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public DateTime? FechaSituacion { get; set; }
        public decimal MontoDeudaDirecta { get; set; }
        public decimal MontoDeudaIndirecta { get; set; }
        public bool SituacionDicom { get; set; }
        public string EstadoDicom { get; set; } = "ACTIVO";
        public int Estado { get; set; } = 1;
        public bool Activo => Estado == 0;
    }

    /// <summary>
    /// Obtener todos los clientes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteListDto>>> GetAll()
    {
        var clientes = await _clienteRepository.GetAllAsync();
        var result = clientes.Select(MapToListDto);
        return Ok(result);
    }

    /// <summary>
    /// Obtener clientes activos (para dropdowns)
    /// </summary>
    [HttpGet("activos")]
    public async Task<ActionResult<IEnumerable<ClienteListDto>>> GetActivos()
    {
        var clientes = await _clienteRepository.GetActivosAsync();
        var result = clientes.Select(MapToListDto);
        return Ok(result);
    }

    /// <summary>
    /// Buscar clientes con filtros y paginación
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<PagedResult<ClienteListDto>>> Search([FromBody] ClienteSearchDto search)
    {
        if (!string.IsNullOrWhiteSpace(search.Rut) && !EsRutValido(search.Rut))
            return BadRequest(new { mensaje = "El RUT ingresado no es válido" });

        search.PageNumber = search.PageNumber <= 0 ? 1 : search.PageNumber;
        search.PageSize = search.PageSize <= 0 ? 20 : Math.Min(search.PageSize, 200);

        var result = await _clienteRepository.SearchAsync(search);
        return Ok(new PagedResult<ClienteListDto>
        {
            Items = result.Items.Select(MapToListDto),
            TotalItems = result.TotalItems,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        });
    }

    /// <summary>
    /// Obtener cliente por RUT
    /// </summary>
    [HttpGet("{rut}")]
    public async Task<ActionResult<ClienteDto>> GetByRut(long rut)
    {
        var cliente = await _clienteRepository.GetByRutAsync(rut);
        if (cliente == null)
            return NotFound(new { mensaje = "Cliente no encontrado" });

        return Ok(MapToDto(cliente));
    }

    /// <summary>
    /// Crear nuevo cliente
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create([FromBody] ClienteCreateDto dto)
    {
        if (!EsRutValido(dto.RutCliente, dto.DigitoVerificador))
            return BadRequest(new { mensaje = "El RUT del cliente no es válido" });

        if (await _clienteRepository.ExistsAsync(dto.RutCliente))
            return BadRequest(new { mensaje = "El RUT del cliente ya existe" });

        var cliente = new Cliente
        {
            RutCliente = dto.RutCliente,
            DigitoVerificador = dto.DigitoVerificador ?? CalcularDigitoVerificador(dto.RutCliente),
            TipoCliente = dto.TipoCliente,
            RazonSocial = dto.RazonSocial,
            ApellidoPaterno = dto.ApellidoPaterno,
            ApellidoMaterno = dto.ApellidoMaterno,
            Sexo = dto.Sexo,
            Direccion = dto.Direccion,
            CodigoComuna = dto.CodigoComuna,
            CodigoGiro = dto.CodigoGiro,
            CodigoPostal = dto.CodigoPostal,
            CorreoElectronico = dto.CorreoElectronico,
            CodigoEjecutivo = dto.CodigoEjecutivo,
            CodigoEjecutivoInicial = dto.CodigoEjecutivo,
            CodigoSucursal = dto.CodigoSucursal,
            Observaciones = dto.Observaciones,
            CodigoActividad = dto.CodigoActividad,
            FechaInicio = DateTime.Now,
            EstadoCliente = 1,
            UsuarioCreacion = User.Identity?.Name
        };

        var success = await _clienteRepository.CreateAsync(cliente);
        if (!success)
            return StatusCode(500, new { mensaje = "Error al crear el cliente" });

        var created = await _clienteRepository.GetByRutAsync(dto.RutCliente);
        return CreatedAtAction(nameof(GetByRut), new { rut = dto.RutCliente }, MapToDto(created!));
    }

    /// <summary>
    /// Actualizar cliente
    /// </summary>
    [HttpPut("{rut}")]
    public async Task<IActionResult> Update(long rut, [FromBody] ClienteUpdateDto dto)
    {
        var cliente = await _clienteRepository.GetByRutAsync(rut);
        if (cliente == null)
            return NotFound(new { mensaje = "Cliente no encontrado" });

        cliente.TipoCliente = dto.TipoCliente;
        cliente.RazonSocial = dto.RazonSocial;
        cliente.ApellidoPaterno = dto.ApellidoPaterno;
        cliente.ApellidoMaterno = dto.ApellidoMaterno;
        cliente.Sexo = dto.Sexo;
        cliente.Direccion = dto.Direccion;
        cliente.CodigoComuna = dto.CodigoComuna;
        cliente.CodigoGiro = dto.CodigoGiro;
        cliente.CodigoPostal = dto.CodigoPostal;
        cliente.CorreoElectronico = dto.CorreoElectronico;
        cliente.CodigoEjecutivo = dto.CodigoEjecutivo;
        cliente.CodigoSucursal = dto.CodigoSucursal;
        cliente.Observaciones = dto.Observaciones;
        cliente.CodigoActividad = dto.CodigoActividad;
        cliente.EstadoCliente = dto.EstadoCliente;
        cliente.UsuarioModificacion = User.Identity?.Name;

        await _clienteRepository.UpdateAsync(cliente);
        return NoContent();
    }

    /// <summary>
    /// Eliminar cliente (soft delete)
    /// </summary>
    [HttpDelete("{rut}")]
    public async Task<IActionResult> Delete(long rut)
    {
        if (!await _clienteRepository.ExistsAsync(rut))
            return NotFound(new { mensaje = "Cliente no encontrado" });

        await _clienteRepository.DeleteAsync(rut);
        return NoContent();
    }

    // =============================================
    // Teléfonos del Cliente
    // =============================================

    /// <summary>
    /// Obtener teléfonos del cliente
    /// </summary>
    [HttpGet("{rut}/telefonos")]
    public async Task<ActionResult<IEnumerable<ClienteTelefonoDto>>> GetTelefonos(long rut)
    {
        var telefonos = await _telefonoRepository.GetByClienteAsync(rut);
        var result = telefonos.Select(t => new ClienteTelefonoDto
        {
            RutCliente = t.RutCliente,
            TipoTelefono = t.TipoTelefono,
            NumeroTelefono = t.NumeroTelefono,
            Observacion = t.Observacion,
            Estado = t.Estado
        });
        return Ok(result);
    }

    /// <summary>
    /// Agregar teléfono al cliente
    /// </summary>
    [HttpPost("{rut}/telefonos")]
    public async Task<IActionResult> AddTelefono(long rut, [FromBody] ClienteTelefonoCreateDto dto)
    {
        if (!await _clienteRepository.ExistsAsync(rut))
            return NotFound(new { mensaje = "Cliente no encontrado" });

        var telefono = new ClienteTelefono
        {
            TipoTelefono = dto.TipoTelefono,
            NumeroTelefono = dto.NumeroTelefono,
            Observacion = dto.Observacion
        };

        await _telefonoRepository.CreateAsync(rut, telefono);
        return CreatedAtAction(nameof(GetTelefonos), new { rut }, null);
    }

    /// <summary>
    /// Eliminar teléfono del cliente
    /// </summary>
    [HttpDelete("{rut}/telefonos/{tipo}/{numero}")]
    public async Task<IActionResult> DeleteTelefono(long rut, int tipo, string numero)
    {
        await _telefonoRepository.DeleteAsync(rut, tipo, numero);
        return NoContent();
    }

    // =============================================
    // Contactos del Cliente
    // =============================================

    /// <summary>
    /// Obtener contactos del cliente
    /// </summary>
    [HttpGet("{rut}/contactos")]
    public async Task<ActionResult<IEnumerable<ClienteContactoDto>>> GetContactos(long rut)
    {
        var contactos = await _contactoRepository.GetByClienteAsync(rut);
        var result = contactos.Select(c => new ClienteContactoDto
        {
            RutCliente = c.RutCliente,
            NumeroContacto = c.NumeroContacto,
            NombreContacto = c.NombreContacto,
            Cargo = c.Cargo,
            Telefono = c.Telefono,
            Celular = c.Celular,
            Email = c.Email,
            Observaciones = c.Observaciones,
            Estado = c.Estado
        });
        return Ok(result);
    }

    /// <summary>
    /// Agregar contacto al cliente
    /// </summary>
    [HttpPost("{rut}/contactos")]
    public async Task<IActionResult> AddContacto(long rut, [FromBody] ClienteContactoCreateDto dto)
    {
        if (!await _clienteRepository.ExistsAsync(rut))
            return NotFound(new { mensaje = "Cliente no encontrado" });

        var contacto = new ClienteContacto
        {
            NombreContacto = dto.NombreContacto,
            Cargo = dto.Cargo,
            Telefono = dto.Telefono,
            Celular = dto.Celular,
            Email = dto.Email,
            Observaciones = dto.Observaciones
        };

        await _contactoRepository.CreateAsync(rut, contacto);
        return CreatedAtAction(nameof(GetContactos), new { rut }, null);
    }

    /// <summary>
    /// Actualizar contacto del cliente
    /// </summary>
    [HttpPut("{rut}/contactos/{numeroContacto}")]
    public async Task<IActionResult> UpdateContacto(long rut, int numeroContacto, [FromBody] ClienteContactoUpdateDto dto)
    {
        var contacto = await _contactoRepository.GetByIdAsync(rut, numeroContacto);
        if (contacto == null)
            return NotFound(new { mensaje = "Contacto no encontrado" });

        contacto.NombreContacto = dto.NombreContacto;
        contacto.Cargo = dto.Cargo;
        contacto.Telefono = dto.Telefono;
        contacto.Celular = dto.Celular;
        contacto.Email = dto.Email;
        contacto.Observaciones = dto.Observaciones;
        contacto.Estado = dto.Estado;

        await _contactoRepository.UpdateAsync(contacto);
        return NoContent();
    }

    /// <summary>
    /// Eliminar contacto del cliente
    /// </summary>
    [HttpDelete("{rut}/contactos/{numeroContacto}")]
    public async Task<IActionResult> DeleteContacto(long rut, int numeroContacto)
    {
        await _contactoRepository.DeleteAsync(rut, numeroContacto);
        return NoContent();
    }

    // =============================================
    // Cuentas Bancarias del Cliente
    // =============================================

    /// <summary>
    /// Obtener cuentas bancarias del cliente
    /// </summary>
    [HttpGet("{rut}/bancos")]
    public async Task<ActionResult<IEnumerable<ClienteBancoDto>>> GetBancos(long rut)
    {
        var cuentas = await _bancoRepository.GetByClienteAsync(rut);
        var result = cuentas.Select(c => new ClienteBancoDto
        {
            RutCliente = c.RutCliente,
            CodigoBanco = c.CodigoBanco,
            NombreBanco = c.NombreBanco,
            TipoCuenta = c.TipoCuenta,
            NumeroCuenta = c.NumeroCuenta,
            Estado = c.Estado
        });
        return Ok(result);
    }

    /// <summary>
    /// Agregar cuenta bancaria al cliente
    /// </summary>
    [HttpPost("{rut}/bancos")]
    public async Task<IActionResult> AddBanco(long rut, [FromBody] ClienteBancoCreateDto dto)
    {
        if (!await _clienteRepository.ExistsAsync(rut))
            return NotFound(new { mensaje = "Cliente no encontrado" });

        var cuenta = new ClienteBanco
        {
            CodigoBanco = dto.CodigoBanco,
            TipoCuenta = dto.TipoCuenta,
            NumeroCuenta = dto.NumeroCuenta
        };

        await _bancoRepository.CreateAsync(rut, cuenta);
        return CreatedAtAction(nameof(GetBancos), new { rut }, null);
    }

    /// <summary>
    /// Eliminar cuenta bancaria del cliente
    /// </summary>
    [HttpDelete("{rut}/bancos/{codigoBanco}/{numeroCuenta}")]
    public async Task<IActionResult> DeleteBanco(long rut, int codigoBanco, string numeroCuenta)
    {
        await _bancoRepository.DeleteAsync(rut, codigoBanco, numeroCuenta);
        return NoContent();
    }

    // =============================================
    // Organigrama del Cliente
    // =============================================

    [HttpGet("{rut}/organigrama")]
    public async Task<ActionResult<IEnumerable<ClienteOrganigramaDto>>> GetOrganigrama(long rut)
    {
        if (!await _clienteRepository.ExistsAsync(rut))
            return NotFound(new { mensaje = "Cliente no encontrado" });

        var items = Organigramas.GetOrAdd(rut, _ => new ConcurrentDictionary<int, ClienteOrganigramaItem>());
        var result = items.Values
            .OrderBy(x => x.NumeroOrganigrama)
            .Select(x => new ClienteOrganigramaDto
            {
                RutCliente = x.RutCliente,
                NumeroOrganigrama = x.NumeroOrganigrama,
                NombrePersona = x.NombrePersona,
                RutPersona = x.RutPersona,
                DigitoVerificador = x.DigitoVerificador,
                RutFormateado = x.RutPersona.HasValue ? FormatRut(x.RutPersona.Value, x.DigitoVerificador) : string.Empty,
                CodigoCargo = x.CodigoCargo,
                DescripcionCargo = x.DescripcionCargo,
                AtributoFirma = x.AtributoFirma,
                Estado = x.Estado,
                Activo = x.Activo
            });

        return Ok(result);
    }

    [HttpPost("{rut}/organigrama")]
    public async Task<IActionResult> AddOrganigrama(long rut, [FromBody] ClienteOrganigramaCreateDto dto)
    {
        if (!await _clienteRepository.ExistsAsync(rut))
            return NotFound(new { mensaje = "Cliente no encontrado" });

        var items = Organigramas.GetOrAdd(rut, _ => new ConcurrentDictionary<int, ClienteOrganigramaItem>());
        var numero = items.Count == 0 ? 1 : items.Keys.Max() + 1;

        var item = new ClienteOrganigramaItem
        {
            RutCliente = rut,
            NumeroOrganigrama = numero,
            NombrePersona = dto.NombrePersona,
            RutPersona = dto.RutPersona,
            DigitoVerificador = dto.DigitoVerificador,
            CodigoCargo = dto.CodigoCargo,
            DescripcionCargo = dto.CodigoCargo.HasValue ? MapCargo(dto.CodigoCargo.Value) : null,
            AtributoFirma = dto.AtributoFirma,
            Estado = 0,
        };

        items[numero] = item;
        return CreatedAtAction(nameof(GetOrganigrama), new { rut }, item);
    }

    [HttpPut("{rut}/organigrama/{numero}")]
    public async Task<IActionResult> UpdateOrganigrama(long rut, int numero, [FromBody] ClienteOrganigramaupdateDto dto)
    {
        if (!await _clienteRepository.ExistsAsync(rut))
            return NotFound(new { mensaje = "Cliente no encontrado" });

        var items = Organigramas.GetOrAdd(rut, _ => new ConcurrentDictionary<int, ClienteOrganigramaItem>());
        if (!items.TryGetValue(numero, out var item))
            return NotFound(new { mensaje = "Registro del organigrama no encontrado" });

        item.NombrePersona = dto.NombrePersona;
        item.RutPersona = dto.RutPersona;
        item.DigitoVerificador = dto.DigitoVerificador;
        item.CodigoCargo = dto.CodigoCargo;
        item.DescripcionCargo = dto.CodigoCargo.HasValue ? MapCargo(dto.CodigoCargo.Value) : null;
        item.AtributoFirma = dto.AtributoFirma;
        item.Estado = dto.Estado;

        return NoContent();
    }

    [HttpDelete("{rut}/organigrama/{numero}")]
    public async Task<IActionResult> DeleteOrganigrama(long rut, int numero)
    {
        if (!await _clienteRepository.ExistsAsync(rut))
            return NotFound(new { mensaje = "Cliente no encontrado" });

        var items = Organigramas.GetOrAdd(rut, _ => new ConcurrentDictionary<int, ClienteOrganigramaItem>());
        items.TryRemove(numero, out _);
        return NoContent();
    }

    // =============================================
    // Empresas Relacionadas del Cliente
    // =============================================

    [HttpGet("{rut}/empresas")]
    public async Task<ActionResult<IEnumerable<ClienteEmpresaDto>>> GetEmpresas(long rut)
    {
        if (!await _clienteRepository.ExistsAsync(rut))
            return NotFound(new { mensaje = "Cliente no encontrado" });

        var items = EmpresasRelacionadas.GetOrAdd(rut, _ => new ConcurrentDictionary<long, ClienteEmpresaRelacionadaItem>());
        var result = items.Values
            .OrderBy(x => x.RazonSocial)
            .Select(x => new ClienteEmpresaDto
            {
                RutCliente = x.RutCliente,
                RutEmpresa = x.RutEmpresa,
                DigitoVerificador = x.DigitoVerificador,
                RutFormateado = FormatRut(x.RutEmpresa, x.DigitoVerificador),
                RazonSocial = x.RazonSocial,
                FechaSituacion = x.FechaSituacion?.ToString("yyyy-MM-dd"),
                MontoDeudaDirecta = x.MontoDeudaDirecta,
                MontoDeudaIndirecta = x.MontoDeudaIndirecta,
                SituacionDicom = x.SituacionDicom,
                EstadoDicom = x.EstadoDicom,
                Estado = x.Estado,
                Activo = x.Activo
            });

        return Ok(result);
    }

    [HttpPost("{rut}/empresas")]
    public async Task<IActionResult> AddEmpresa(long rut, [FromBody] ClienteEmpresaCreateDto dto)
    {
        if (!await _clienteRepository.ExistsAsync(rut))
            return NotFound(new { mensaje = "Cliente no encontrado" });

        var items = EmpresasRelacionadas.GetOrAdd(rut, _ => new ConcurrentDictionary<long, ClienteEmpresaRelacionadaItem>());
        var item = new ClienteEmpresaRelacionadaItem
        {
            RutCliente = rut,
            RutEmpresa = dto.RutEmpresa,
            DigitoVerificador = dto.DigitoVerificador ?? CalcularDigitoVerificador(dto.RutEmpresa),
            RazonSocial = dto.RazonSocial,
            FechaSituacion = string.IsNullOrWhiteSpace(dto.FechaSituacion) ? null : DateTime.Parse(dto.FechaSituacion),
            MontoDeudaDirecta = dto.MontoDeudaDirecta,
            MontoDeudaIndirecta = dto.MontoDeudaIndirecta,
            SituacionDicom = dto.SituacionDicom,
            EstadoDicom = dto.SituacionDicom ? "ALERTA" : "ACTIVO",
            Estado = 0,
        };

        items[dto.RutEmpresa] = item;
        return CreatedAtAction(nameof(GetEmpresas), new { rut }, item);
    }

    [HttpPut("{rut}/empresas/{rutEmpresa}")]
    public async Task<IActionResult> UpdateEmpresa(long rut, long rutEmpresa, [FromBody] ClienteEmpresaupdateDto dto)
    {
        if (!await _clienteRepository.ExistsAsync(rut))
            return NotFound(new { mensaje = "Cliente no encontrado" });

        var items = EmpresasRelacionadas.GetOrAdd(rut, _ => new ConcurrentDictionary<long, ClienteEmpresaRelacionadaItem>());
        if (!items.TryGetValue(rutEmpresa, out var item))
            return NotFound(new { mensaje = "Empresa relacionada no encontrada" });

        item.RazonSocial = dto.RazonSocial;
        item.FechaSituacion = string.IsNullOrWhiteSpace(dto.FechaSituacion) ? null : DateTime.Parse(dto.FechaSituacion);
        item.MontoDeudaDirecta = dto.MontoDeudaDirecta;
        item.MontoDeudaIndirecta = dto.MontoDeudaIndirecta;
        item.SituacionDicom = dto.SituacionDicom;
        item.EstadoDicom = dto.SituacionDicom ? "ALERTA" : "ACTIVO";
        item.Estado = dto.Estado;

        return NoContent();
    }

    [HttpDelete("{rut}/empresas/{rutEmpresa}")]
    public async Task<IActionResult> DeleteEmpresa(long rut, long rutEmpresa)
    {
        if (!await _clienteRepository.ExistsAsync(rut))
            return NotFound(new { mensaje = "Cliente no encontrado" });

        var items = EmpresasRelacionadas.GetOrAdd(rut, _ => new ConcurrentDictionary<long, ClienteEmpresaRelacionadaItem>());
        items.TryRemove(rutEmpresa, out _);
        return NoContent();
    }

    // =============================================
    // Helpers
    // =============================================

    private static ClienteListDto MapToListDto(Cliente c) => new()
    {
        RutCliente = c.RutCliente,
        RutFormateado = FormatRut(c.RutCliente, c.DigitoVerificador),
        NombreCompleto = c.TipoCliente == 1 
            ? $"{c.RazonSocial} {c.ApellidoPaterno} {c.ApellidoMaterno}".Trim() 
            : c.RazonSocial,
        NombreEjecutivo = null, // Se podría cargar con JOIN
        EstadoCliente = c.EstadoCliente
    };

    private static ClienteDto MapToDto(Cliente c) => new()
    {
        RutCliente = c.RutCliente,
        DigitoVerificador = c.DigitoVerificador,
        RutFormateado = FormatRut(c.RutCliente, c.DigitoVerificador),
        TipoCliente = c.TipoCliente,
        RazonSocial = c.RazonSocial,
        ApellidoPaterno = c.ApellidoPaterno,
        ApellidoMaterno = c.ApellidoMaterno,
        NombreCompleto = c.TipoCliente == 1 
            ? $"{c.RazonSocial} {c.ApellidoPaterno} {c.ApellidoMaterno}".Trim() 
            : c.RazonSocial,
        Direccion = c.Direccion,
        CodigoComuna = c.CodigoComuna,
        CodigoGiro = c.CodigoGiro,
        CodigoPostal = c.CodigoPostal,
        CorreoElectronico = c.CorreoElectronico,
        FechaInicio = c.FechaInicio,
        EstadoCliente = c.EstadoCliente,
        CodigoEjecutivo = c.CodigoEjecutivo,
        Observaciones = c.Observaciones,
        CodigoSucursal = c.CodigoSucursal,
        CodigoActividad = c.CodigoActividad,
        LineaBanco = c.LineaBanco,
        UtilizadoBanco = c.UtilizadoBanco,
        FechaCreacion = c.FechaCreacion
    };

    private static string MapCargo(int codigoCargo) => codigoCargo switch
    {
        20 => "Gerente General",
        21 => "Director",
        22 => "Jefe de Operaciones",
        23 => "Ejecutivo Comercial",
        24 => "Analista",
        _ => "Cargo no definido"
    };

    private static bool EsRutValido(string rut)
    {
        var limpio = rut?.Replace(".", "").Replace("-", "").Trim();
        if (string.IsNullOrWhiteSpace(limpio) || limpio.Length < 2)
            return false;

        var dv = limpio[^1].ToString();
        if (!long.TryParse(limpio[..^1], out var numero))
            return false;

        return CalcularDigitoVerificador(numero) == dv.ToUpperInvariant();
    }

    private static bool EsRutValido(long rut, string? dv)
    {
        if (rut <= 0)
            return false;

        var digito = dv?.Trim();
        if (string.IsNullOrWhiteSpace(digito))
            digito = CalcularDigitoVerificador(rut);

        return CalcularDigitoVerificador(rut) == digito.ToUpperInvariant();
    }

    private static string FormatRut(long rut, string? dv)
    {
        return $"{rut:N0}-{dv ?? CalcularDigitoVerificador(rut)}".Replace(",", ".");
    }

    private static string CalcularDigitoVerificador(long rut)
    {
        int suma = 0;
        int multiplicador = 2;
        
        while (rut > 0)
        {
            suma += (int)(rut % 10) * multiplicador;
            rut /= 10;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
        }
        
        int resto = 11 - (suma % 11);
        
        return resto switch
        {
            11 => "0",
            10 => "K",
            _ => resto.ToString()
        };
    }
}
