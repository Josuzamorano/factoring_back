using Factoring.Core.DTOs;
using Factoring.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Factoring.API.Controllers
{
    /// <summary>
    /// Controller para gestión del módulo Legal
    /// Migrado de F_LE1000.frm (menú), F_LE1100-F_LE1800.frm
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LegalController : ControllerBase
    {
        private readonly IPagareRepository _pagareRepository;
        private readonly IAvalRepository _avalRepository;
        private readonly IFirmaRepository _firmaRepository;
        private readonly IContratoMarcoRepository _contratoMarcoRepository;
        private readonly ILegalRepository _legalRepository;

        public LegalController(
            IPagareRepository pagareRepository,
            IAvalRepository avalRepository,
            IFirmaRepository firmaRepository,
            IContratoMarcoRepository contratoMarcoRepository,
            ILegalRepository legalRepository)
        {
            _pagareRepository = pagareRepository;
            _avalRepository = avalRepository;
            _firmaRepository = firmaRepository;
            _contratoMarcoRepository = contratoMarcoRepository;
            _legalRepository = legalRepository;
        }

        private int GetUsuarioId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        #region Resumen General

        /// <summary>
        /// Obtiene el resumen general del módulo Legal
        /// </summary>
        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumenGeneral()
        {
            var resumen = await _legalRepository.GetResumenGeneralAsync();
            return Ok(resumen);
        }

        /// <summary>
        /// Obtiene el resumen de garantías por cliente
        /// </summary>
        [HttpGet("resumen/cliente/{rutCliente}")]
        public async Task<IActionResult> GetResumenByCliente(int rutCliente)
        {
            var resumen = await _legalRepository.GetResumenByClienteAsync(rutCliente);
            if (resumen == null)
                return NotFound("Cliente no encontrado");
            return Ok(resumen);
        }

        /// <summary>
        /// Obtiene el resumen de todos los clientes
        /// </summary>
        [HttpGet("resumen/todos")]
        public async Task<IActionResult> GetResumenTodosClientes()
        {
            var resumen = await _legalRepository.GetResumenTodosClientesAsync();
            return Ok(resumen);
        }

        #endregion

        #region Pagarés

        /// <summary>
        /// Lista pagarés con filtros
        /// </summary>
        [HttpPost("pagares/consultar")]
        public async Task<IActionResult> GetPagares([FromBody] FiltroPagaresDto filtro)
        {
            var pagares = await _pagareRepository.GetAllAsync(filtro);
            return Ok(pagares);
        }

        /// <summary>
        /// Obtiene un pagaré por ID
        /// </summary>
        [HttpGet("pagares/{id}")]
        public async Task<IActionResult> GetPagareById(int id)
        {
            var pagare = await _pagareRepository.GetByIdAsync(id);
            if (pagare == null)
                return NotFound("Pagaré no encontrado");
            return Ok(pagare);
        }

        /// <summary>
        /// Obtiene pagarés por cliente
        /// </summary>
        [HttpGet("pagares/cliente/{rutCliente}")]
        public async Task<IActionResult> GetPagaresByCliente(int rutCliente)
        {
            var pagares = await _pagareRepository.GetByClienteAsync(rutCliente);
            return Ok(pagares);
        }

        /// <summary>
        /// Obtiene pagarés vigentes
        /// </summary>
        [HttpGet("pagares/vigentes")]
        public async Task<IActionResult> GetPagaresVigentes()
        {
            var pagares = await _pagareRepository.GetVigentesAsync();
            return Ok(pagares);
        }

        /// <summary>
        /// Obtiene pagarés vencidos
        /// </summary>
        [HttpGet("pagares/vencidos")]
        public async Task<IActionResult> GetPagaresVencidos()
        {
            var pagares = await _pagareRepository.GetVencidosAsync();
            return Ok(pagares);
        }

        /// <summary>
        /// Obtiene pagarés por vencer
        /// </summary>
        [HttpGet("pagares/por-vencer")]
        public async Task<IActionResult> GetPagaresPorVencer([FromQuery] int dias = 30)
        {
            var pagares = await _pagareRepository.GetPorVencerAsync(dias);
            return Ok(pagares);
        }

        /// <summary>
        /// Crea un nuevo pagaré
        /// </summary>
        [HttpPost("pagares")]
        public async Task<IActionResult> CreatePagare([FromBody] CreatePagareDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _pagareRepository.CreateAsync(dto, GetUsuarioId());
            var pagare = await _pagareRepository.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetPagareById), new { id }, pagare);
        }

        /// <summary>
        /// Actualiza un pagaré
        /// </summary>
        [HttpPut("pagares/{id}")]
        public async Task<IActionResult> UpdatePagare(int id, [FromBody] UpdatePagareDto dto)
        {
            var result = await _pagareRepository.UpdateAsync(id, dto, GetUsuarioId());
            if (!result)
                return NotFound("Pagaré no encontrado");
            return Ok(await _pagareRepository.GetByIdAsync(id));
        }

        /// <summary>
        /// Cambia el estado de un pagaré
        /// </summary>
        [HttpPost("pagares/{id}/cambiar-estado")]
        public async Task<IActionResult> CambiarEstadoPagare(int id, [FromBody] CambiarEstadoPagareDto dto)
        {
            var result = await _pagareRepository.CambiarEstadoAsync(id, dto.NuevoEstado, dto.Motivo, GetUsuarioId());
            if (!result)
                return NotFound("Pagaré no encontrado");
            return Ok(new { mensaje = "Estado actualizado correctamente" });
        }

        /// <summary>
        /// Elimina (anula) un pagaré
        /// </summary>
        [HttpDelete("pagares/{id}")]
        public async Task<IActionResult> DeletePagare(int id)
        {
            var result = await _pagareRepository.DeleteAsync(id, GetUsuarioId());
            if (!result)
                return NotFound("Pagaré no encontrado");
            return Ok(new { mensaje = "Pagaré anulado correctamente" });
        }

        /// <summary>
        /// Obtiene el siguiente número de pagaré disponible
        /// </summary>
        [HttpGet("pagares/siguiente-numero")]
        public async Task<IActionResult> GetSiguienteNumeroPagare()
        {
            var numero = await _pagareRepository.GetSiguienteNumeroAsync();
            return Ok(new { siguienteNumero = numero });
        }

        #endregion

        #region Avales

        /// <summary>
        /// Lista avales con filtros
        /// </summary>
        [HttpPost("avales/consultar")]
        public async Task<IActionResult> GetAvales([FromBody] FiltroAvalesDto filtro)
        {
            var avales = await _avalRepository.GetAllAsync(filtro);
            return Ok(avales);
        }

        /// <summary>
        /// Obtiene un aval por ID
        /// </summary>
        [HttpGet("avales/{id}")]
        public async Task<IActionResult> GetAvalById(int id)
        {
            var aval = await _avalRepository.GetByIdAsync(id);
            if (aval == null)
                return NotFound("Aval no encontrado");
            return Ok(aval);
        }

        /// <summary>
        /// Obtiene avales por cliente
        /// </summary>
        [HttpGet("avales/cliente/{rutCliente}")]
        public async Task<IActionResult> GetAvalesByCliente(int rutCliente)
        {
            var avales = await _avalRepository.GetByClienteAsync(rutCliente);
            return Ok(avales);
        }

        /// <summary>
        /// Obtiene avales activos
        /// </summary>
        [HttpGet("avales/activos")]
        public async Task<IActionResult> GetAvalesActivos()
        {
            var avales = await _avalRepository.GetActivosAsync();
            return Ok(avales);
        }

        /// <summary>
        /// Obtiene avales por vencer
        /// </summary>
        [HttpGet("avales/por-vencer")]
        public async Task<IActionResult> GetAvalesPorVencer([FromQuery] int dias = 30)
        {
            var avales = await _avalRepository.GetPorVencerAsync(dias);
            return Ok(avales);
        }

        /// <summary>
        /// Crea un nuevo aval
        /// </summary>
        [HttpPost("avales")]
        public async Task<IActionResult> CreateAval([FromBody] CreateAvalDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar que no exista ya el aval para el cliente
            var existe = await _avalRepository.ExisteAvalParaClienteAsync(dto.RutCliente, dto.RutAval);
            if (existe)
                return BadRequest("Ya existe un aval activo con ese RUT para este cliente");

            var id = await _avalRepository.CreateAsync(dto, GetUsuarioId());
            var aval = await _avalRepository.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetAvalById), new { id }, aval);
        }

        /// <summary>
        /// Actualiza un aval
        /// </summary>
        [HttpPut("avales/{id}")]
        public async Task<IActionResult> UpdateAval(int id, [FromBody] UpdateAvalDto dto)
        {
            var result = await _avalRepository.UpdateAsync(id, dto, GetUsuarioId());
            if (!result)
                return NotFound("Aval no encontrado");
            return Ok(await _avalRepository.GetByIdAsync(id));
        }

        /// <summary>
        /// Cambia el estado de un aval
        /// </summary>
        [HttpPost("avales/{id}/cambiar-estado")]
        public async Task<IActionResult> CambiarEstadoAval(int id, [FromQuery] int nuevoEstado)
        {
            var result = await _avalRepository.CambiarEstadoAsync(id, nuevoEstado, GetUsuarioId());
            if (!result)
                return NotFound("Aval no encontrado");
            return Ok(new { mensaje = "Estado actualizado correctamente" });
        }

        /// <summary>
        /// Elimina (libera) un aval
        /// </summary>
        [HttpDelete("avales/{id}")]
        public async Task<IActionResult> DeleteAval(int id)
        {
            var result = await _avalRepository.DeleteAsync(id, GetUsuarioId());
            if (!result)
                return NotFound("Aval no encontrado");
            return Ok(new { mensaje = "Aval liberado correctamente" });
        }

        #endregion

        #region Firmas

        /// <summary>
        /// Lista firmas con filtros
        /// </summary>
        [HttpPost("firmas/consultar")]
        public async Task<IActionResult> GetFirmas([FromBody] FiltroFirmasDto filtro)
        {
            var firmas = await _firmaRepository.GetAllAsync(filtro);
            return Ok(firmas);
        }

        /// <summary>
        /// Obtiene una firma por ID
        /// </summary>
        [HttpGet("firmas/{id}")]
        public async Task<IActionResult> GetFirmaById(int id)
        {
            var firma = await _firmaRepository.GetByIdAsync(id);
            if (firma == null)
                return NotFound("Firma no encontrada");
            return Ok(firma);
        }

        /// <summary>
        /// Obtiene firmas por cliente
        /// </summary>
        [HttpGet("firmas/cliente/{rutCliente}")]
        public async Task<IActionResult> GetFirmasByCliente(int rutCliente)
        {
            var firmas = await _firmaRepository.GetByClienteAsync(rutCliente);
            return Ok(firmas);
        }

        /// <summary>
        /// Obtiene firmas vigentes
        /// </summary>
        [HttpGet("firmas/vigentes")]
        public async Task<IActionResult> GetFirmasVigentes()
        {
            var firmas = await _firmaRepository.GetVigentesAsync();
            return Ok(firmas);
        }

        /// <summary>
        /// Obtiene firmas por vencer
        /// </summary>
        [HttpGet("firmas/por-vencer")]
        public async Task<IActionResult> GetFirmasPorVencer([FromQuery] int dias = 30)
        {
            var firmas = await _firmaRepository.GetPorVencerAsync(dias);
            return Ok(firmas);
        }

        /// <summary>
        /// Crea una nueva firma
        /// </summary>
        [HttpPost("firmas")]
        public async Task<IActionResult> CreateFirma([FromBody] CreateFirmaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar que no exista ya la firma para el cliente
            var existe = await _firmaRepository.ExisteFirmaParaClienteAsync(dto.RutCliente, dto.RutFirmante);
            if (existe)
                return BadRequest("Ya existe una firma activa con ese RUT para este cliente");

            var id = await _firmaRepository.CreateAsync(dto, GetUsuarioId());
            var firma = await _firmaRepository.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetFirmaById), new { id }, firma);
        }

        /// <summary>
        /// Actualiza una firma
        /// </summary>
        [HttpPut("firmas/{id}")]
        public async Task<IActionResult> UpdateFirma(int id, [FromBody] UpdateFirmaDto dto)
        {
            var result = await _firmaRepository.UpdateAsync(id, dto, GetUsuarioId());
            if (!result)
                return NotFound("Firma no encontrada");
            return Ok(await _firmaRepository.GetByIdAsync(id));
        }

        /// <summary>
        /// Activa o desactiva una firma
        /// </summary>
        [HttpPost("firmas/{id}/activar")]
        public async Task<IActionResult> ActivarDesactivarFirma(int id, [FromQuery] bool activa)
        {
            var result = await _firmaRepository.ActivarDesactivarAsync(id, activa, GetUsuarioId());
            if (!result)
                return NotFound("Firma no encontrada");
            return Ok(new { mensaje = activa ? "Firma activada" : "Firma desactivada" });
        }

        /// <summary>
        /// Elimina (desactiva) una firma
        /// </summary>
        [HttpDelete("firmas/{id}")]
        public async Task<IActionResult> DeleteFirma(int id)
        {
            var result = await _firmaRepository.DeleteAsync(id, GetUsuarioId());
            if (!result)
                return NotFound("Firma no encontrada");
            return Ok(new { mensaje = "Firma desactivada correctamente" });
        }

        #endregion

        #region Contratos Marco

        /// <summary>
        /// Lista contratos marco con filtros
        /// Migrado de F_LE1700.frm - Contrato Global en Pesos
        /// </summary>
        [HttpPost("contratos/consultar")]
        public async Task<IActionResult> GetContratos([FromBody] FiltroContratosMarcoDto filtro)
        {
            var contratos = await _contratoMarcoRepository.GetAllAsync(filtro);
            return Ok(contratos);
        }

        /// <summary>
        /// Obtiene un contrato marco por ID
        /// </summary>
        [HttpGet("contratos/{id}")]
        public async Task<IActionResult> GetContratoById(int id)
        {
            var contrato = await _contratoMarcoRepository.GetByIdAsync(id);
            if (contrato == null)
                return NotFound("Contrato no encontrado");
            return Ok(contrato);
        }

        /// <summary>
        /// Obtiene un contrato por número
        /// </summary>
        [HttpGet("contratos/numero/{numeroContrato}")]
        public async Task<IActionResult> GetContratoByNumero(int numeroContrato)
        {
            var contrato = await _contratoMarcoRepository.GetByNumeroAsync(numeroContrato);
            if (contrato == null)
                return NotFound("Contrato no encontrado");
            return Ok(contrato);
        }

        /// <summary>
        /// Obtiene contratos por cliente
        /// </summary>
        [HttpGet("contratos/cliente/{rutCliente}")]
        public async Task<IActionResult> GetContratosByCliente(int rutCliente)
        {
            var contratos = await _contratoMarcoRepository.GetByClienteAsync(rutCliente);
            return Ok(contratos);
        }

        /// <summary>
        /// Crea un nuevo contrato marco
        /// </summary>
        [HttpPost("contratos")]
        public async Task<IActionResult> CreateContrato([FromBody] CreateContratoMarcoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _contratoMarcoRepository.CreateAsync(dto, GetUsuarioId());
            var contrato = await _contratoMarcoRepository.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetContratoById), new { id }, contrato);
        }

        /// <summary>
        /// Actualiza un contrato marco
        /// </summary>
        [HttpPut("contratos/{id}")]
        public async Task<IActionResult> UpdateContrato(int id, [FromBody] UpdateContratoMarcoDto dto)
        {
            var result = await _contratoMarcoRepository.UpdateAsync(id, dto, GetUsuarioId());
            if (!result)
                return NotFound("Contrato no encontrado");
            return Ok(await _contratoMarcoRepository.GetByIdAsync(id));
        }

        /// <summary>
        /// Aprueba un contrato marco
        /// </summary>
        [HttpPost("contratos/{id}/aprobar")]
        public async Task<IActionResult> AprobarContrato(int id, [FromQuery] decimal montoAprobado)
        {
            var result = await _contratoMarcoRepository.AprobarAsync(id, montoAprobado, GetUsuarioId());
            if (!result)
                return BadRequest("No se pudo aprobar el contrato. Verifique que esté en estado pendiente.");
            return Ok(new { mensaje = "Contrato aprobado correctamente" });
        }

        /// <summary>
        /// Rechaza un contrato marco
        /// </summary>
        [HttpPost("contratos/{id}/rechazar")]
        public async Task<IActionResult> RechazarContrato(int id, [FromQuery] string motivo)
        {
            var result = await _contratoMarcoRepository.RechazarAsync(id, motivo ?? "Sin motivo especificado", GetUsuarioId());
            if (!result)
                return BadRequest("No se pudo rechazar el contrato. Verifique que esté en estado pendiente.");
            return Ok(new { mensaje = "Contrato rechazado" });
        }

        /// <summary>
        /// Obtiene el siguiente número de contrato disponible
        /// </summary>
        [HttpGet("contratos/siguiente-numero")]
        public async Task<IActionResult> GetSiguienteNumeroContrato()
        {
            var numero = await _contratoMarcoRepository.GetSiguienteNumeroAsync();
            return Ok(new { siguienteNumero = numero });
        }

        /// <summary>
        /// Obtiene el monto disponible por cliente
        /// </summary>
        [HttpGet("contratos/disponible/{rutCliente}")]
        public async Task<IActionResult> GetMontoDisponibleByCliente(int rutCliente)
        {
            var monto = await _contratoMarcoRepository.GetMontoDisponibleByClienteAsync(rutCliente);
            return Ok(new { rutCliente, montoDisponible = monto });
        }

        /// <summary>
        /// Elimina (anula) un contrato marco
        /// </summary>
        [HttpDelete("contratos/{id}")]
        public async Task<IActionResult> DeleteContrato(int id)
        {
            var result = await _contratoMarcoRepository.DeleteAsync(id, GetUsuarioId());
            if (!result)
                return NotFound("Contrato no encontrado");
            return Ok(new { mensaje = "Contrato anulado correctamente" });
        }

        #endregion
    }
}
