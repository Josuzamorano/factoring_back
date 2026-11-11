using Microsoft.AspNetCore.Mvc;
using Factoring.Core.DTOs;
using Factoring.Core.Interfaces;
using Factoring.Application.Services;

namespace Factoring.API.Controllers;

/// <summary>
/// Controlador para gestión de Cobranzas - Módulo CO
/// Migrado desde F_CO1000.frm, F_CO1500.frm, F_CO1600.frm, F_CO2100.frm, F_CO2300.frm, F_CO2510.frm
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CobranzaController : ControllerBase
{
    private readonly IPagoDocumentoRepository _pagoRepository;
    private readonly IVerificacionRepository _verificacionRepository;
    private readonly IVerificacionDiariaRepository _verificacionDiariaRepository;
    private readonly IDocumentoNoCedidoRepository _documentoNoCedidoRepository;
    private readonly IConsultaMasivaRepository _consultaMasivaRepository;
    private readonly ICajaRepository _cajaRepository;
    private readonly IInformeAbonoRepository _abonoRepository;
    private readonly CobranzaService _cobranzaService;

    public CobranzaController(
        IPagoDocumentoRepository pagoRepository,
        IVerificacionRepository verificacionRepository,
        IVerificacionDiariaRepository verificacionDiariaRepository,
        IDocumentoNoCedidoRepository documentoNoCedidoRepository,
        IConsultaMasivaRepository consultaMasivaRepository,
        ICajaRepository cajaRepository,
        IInformeAbonoRepository abonoRepository,
        CobranzaService cobranzaService)
    {
        _pagoRepository = pagoRepository;
        _verificacionRepository = verificacionRepository;
        _verificacionDiariaRepository = verificacionDiariaRepository;
        _documentoNoCedidoRepository = documentoNoCedidoRepository;
        _consultaMasivaRepository = consultaMasivaRepository;
        _cajaRepository = cajaRepository;
        _abonoRepository = abonoRepository;
        _cobranzaService = cobranzaService;
    }

    // ==========================================
    // PAGOS DE DOCUMENTOS
    // ==========================================

    /// <summary>
    /// Obtiene todos los pagos registrados
    /// </summary>
    [HttpGet("pagos")]
    public async Task<IActionResult> GetPagos()
    {
        var pagos = await _pagoRepository.GetAllAsync();
        return Ok(pagos);
    }

    /// <summary>
    /// Obtiene un pago por su número
    /// </summary>
    [HttpGet("pagos/{numeroPago}")]
    public async Task<IActionResult> GetPago(int numeroPago)
    {
        var pago = await _pagoRepository.GetByIdAsync(numeroPago);
        if (pago == null) return NotFound();
        return Ok(pago);
    }

    /// <summary>
    /// Obtiene pagos por fecha
    /// </summary>
    [HttpGet("pagos/fecha/{fecha}")]
    public async Task<IActionResult> GetPagosByFecha(DateTime? fecha)
    {
        var pagos = await _pagoRepository.GetByFechaAsync(fecha);
        return Ok(pagos);
    }

    /// <summary>
    /// Obtiene pagos pendientes de aprobación
    /// </summary>
    [HttpGet("pagos/pendientes")]
    public async Task<IActionResult> GetPagosPendientes()
    {
        var pagos = await _pagoRepository.GetPendientesAprobacionAsync();
        return Ok(pagos);
    }

    /// <summary>
    /// Crea un nuevo pago de documento
    /// </summary>
    [HttpPost("pagos")]
    public async Task<IActionResult> CreatePago([FromBody] PagoDocumentoCreateDto dto)
    {
        var numeroPago = await _pagoRepository.CreateAsync(dto);
        return CreatedAtAction(nameof(GetPago), new { numeroPago }, new { NumeroPago = numeroPago });
    }

    /// <summary>
    /// Actualiza el estado de un pago (aprobar/rechazar/eliminar)
    /// </summary>
    [HttpPut("pagos/estado")]
    public async Task<IActionResult> UpdateEstadoPago([FromBody] PagoDocumentoUpdateDto dto)
    {
        var result = await _pagoRepository.UpdateEstadoAsync(dto);
        if (!result) return NotFound();
        return NoContent();
    }

    // ==========================================
    // DOCUMENTOS PARA PAGO
    // ==========================================

    /// <summary>
    /// Obtiene documentos disponibles para pago de deudor
    /// </summary>
    [HttpGet("documentos/deudor/{rutDeudor}")]
    public async Task<IActionResult> GetDocumentosPagarDeudor(long rutDeudor, [FromQuery] int? tipoDocumento)
    {
        var documentos = await _pagoRepository.GetDocumentosPagarDeudorAsync(rutDeudor, tipoDocumento);
        return Ok(documentos);
    }

    /// <summary>
    /// Obtiene documentos disponibles para pago de cliente
    /// </summary>
    [HttpGet("documentos/cliente/{rutCliente}")]
    public async Task<IActionResult> GetDocumentosPagarCliente(long rutCliente, [FromQuery] int? tipoDocumento)
    {
        var documentos = await _pagoRepository.GetDocumentosPagarClienteAsync(rutCliente, tipoDocumento);
        return Ok(documentos);
    }

    /// <summary>
    /// Obtiene detalle de un documento específico para pago
    /// </summary>
    [HttpGet("documentos/detalle")]
    public async Task<IActionResult> GetDetalleDocumento([FromQuery] int tipoDocumento, [FromQuery] long numeroDocumento, [FromQuery] int numeroOperacion)
    {
        var documento = await _pagoRepository.GetDetalleDocumentoAsync(tipoDocumento, numeroDocumento, numeroOperacion);
        if (documento == null) return NotFound();
        return Ok(documento);
    }

    /// <summary>
    /// Obtiene el saldo pendiente de un documento
    /// </summary>
    [HttpGet("documentos/saldo")]
    public async Task<IActionResult> GetSaldoDocumento([FromQuery] int tipoDocumento, [FromQuery] long numeroDocumento, [FromQuery] int numeroOperacion)
    {
        var saldo = await _pagoRepository.GetSaldoPendienteAsync(tipoDocumento, numeroDocumento, numeroOperacion);
        return Ok(new { Saldo = saldo });
    }

    // ==========================================
    // VERIFICACIÓN DE DOCUMENTOS
    // ==========================================

    /// <summary>
    /// Obtiene documentos para verificación (no otorgados)
    /// </summary>
    [HttpPost("verificacion")]
    public async Task<IActionResult> GetVerificacion([FromBody] FiltroVerificacionDto filtro)
    {
        var documentos = await _verificacionRepository.GetVerificacionAsync(filtro);
        return Ok(documentos);
    }

    /// <summary>
    /// Obtiene documentos para verificación (otorgados)
    /// </summary>
    [HttpPost("verificacion/otorgados")]
    public async Task<IActionResult> GetVerificacionOtorgados([FromBody] FiltroVerificacionDto filtro)
    {
        var documentos = await _verificacionRepository.GetVerificacionOtorgadosAsync(filtro);
        return Ok(documentos);
    }

    /// <summary>
    /// Obtiene documentos para verificación por carátula
    /// </summary>
    [HttpPost("verificacion/caratula")]
    public async Task<IActionResult> GetVerificacionCaratula([FromBody] FiltroVerificacionDto filtro)
    {
        var documentos = await _verificacionRepository.GetVerificacionCaratulaAsync(filtro);
        return Ok(documentos);
    }

    /// <summary>
    /// Obtiene detalle de verificación para una operación
    /// </summary>
    [HttpGet("verificacion/detalle")]
    public async Task<IActionResult> GetDetalleVerificacion([FromQuery] long rutCliente, [FromQuery] long rutDeudor, [FromQuery] int numeroOperacion)
    {
        var detalle = await _verificacionRepository.GetDetalleVerificacionAsync(rutCliente, rutDeudor, numeroOperacion);
        return Ok(detalle);
    }

    /// <summary>
    /// Marca un documento como verificado
    /// </summary>
    [HttpPut("verificacion/marcar")]
    public async Task<IActionResult> MarcarVerificado([FromQuery] int tipoDocumento, [FromQuery] long numeroDocumento, [FromQuery] int numeroOperacion, [FromQuery] bool verificado)
    {
        var result = await _verificacionRepository.MarcarVerificadoAsync(tipoDocumento, numeroDocumento, numeroOperacion, verificado);
        if (!result) return NotFound();
        return NoContent();
    }

    // ==========================================
    // CAJA
    // ==========================================

    /// <summary>
    /// Obtiene movimientos de caja según filtros
    /// </summary>
    [HttpPost("caja")]
    public async Task<IActionResult> GetMovimientosCaja([FromBody] FiltroCajaDto filtro)
    {
        var movimientos = await _cajaRepository.GetMovimientosCajaAsync(filtro);
        return Ok(movimientos);
    }

    /// <summary>
    /// Obtiene resumen de caja por fecha
    /// </summary>
    [HttpGet("caja/resumen/{fecha}")]
    public async Task<IActionResult> GetResumenCaja(DateTime? fecha, [FromQuery] int? tipoDocumento)
    {
        var resumen = await _cajaRepository.GetResumenCajaAsync(fecha, tipoDocumento);
        return Ok(resumen);
    }

    /// <summary>
    /// Obtiene resumen de caja agrupado por tipo de documento
    /// </summary>
    [HttpGet("caja/resumen-por-tipo/{fecha}")]
    public async Task<IActionResult> GetResumenCajaPorTipo(DateTime? fecha)
    {
        var resumen = await _cajaRepository.GetResumenCajaPorTipoAsync(fecha);
        return Ok(resumen);
    }

    /// <summary>
    /// Aprueba un pago en caja
    /// </summary>
    [HttpPut("caja/aprobar/{numeroMovimiento}")]
    public async Task<IActionResult> AprobarPago(int numeroMovimiento, [FromQuery] string usuario)
    {
        var result = await _cajaRepository.AprobarPagoAsync(numeroMovimiento, usuario);
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Rechaza un pago en caja
    /// </summary>
    [HttpPut("caja/rechazar/{numeroMovimiento}")]
    public async Task<IActionResult> RechazarPago(int numeroMovimiento, [FromQuery] string usuario, [FromQuery] string? motivo)
    {
        var result = await _cajaRepository.RechazarPagoAsync(numeroMovimiento, usuario, motivo);
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Elimina un pago en caja
    /// </summary>
    [HttpPut("caja/eliminar/{numeroMovimiento}")]
    public async Task<IActionResult> EliminarPago(int numeroMovimiento, [FromQuery] string usuario)
    {
        var result = await _cajaRepository.EliminarPagoAsync(numeroMovimiento, usuario);
        if (!result) return NotFound();
        return NoContent();
    }

    // ==========================================
    // INFORME DE ABONOS
    // ==========================================

    /// <summary>
    /// Obtiene informe de abonos según filtros
    /// </summary>
    [HttpPost("abonos")]
    public async Task<IActionResult> GetAbonos([FromBody] FiltroInformeAbonoDto filtro)
    {
        var abonos = await _abonoRepository.GetAbonosAsync(filtro);
        return Ok(abonos);
    }

    /// <summary>
    /// Obtiene resumen de abonos
    /// </summary>
    [HttpPost("abonos/resumen")]
    public async Task<IActionResult> GetResumenAbonos([FromBody] FiltroInformeAbonoDto filtro)
    {
        var resumen = await _abonoRepository.GetResumenAbonosAsync(filtro);
        return Ok(resumen);
    }

    /// <summary>
    /// Obtiene abonos por cobrador
    /// </summary>
    [HttpGet("abonos/cobrador/{codigoCobrador}")]
    public async Task<IActionResult> GetAbonosByCobrador(string codigoCobrador, [FromQuery] DateTime? fechaDesde, [FromQuery] DateTime? fechaHasta)
    {
        var abonos = await _abonoRepository.GetAbonosByCobradorAsync(codigoCobrador, fechaDesde, fechaHasta);
        return Ok(abonos);
    }

    // ==========================================
    // RECAUDACIÓN CON AFECTACIÓN DE SALDOS (CobranzaService)
    // ==========================================

    /// <summary>
    /// Obtiene documentos pendientes de pago con filtros
    /// </summary>
    [HttpGet("recaudacion/pendientes")]
    public async Task<IActionResult> GetDocumentosPendientes(
        [FromQuery] long? rutCliente = null,
        [FromQuery] long? rutDeudor = null,
        [FromQuery] int? tipoDocumento = null,
        [FromQuery] DateTime? fechaVencimientoDesde = null,
        [FromQuery] DateTime? fechaVencimientoHasta = null,
        [FromQuery] bool soloMorosos = false)
    {
        var documentos = await _cobranzaService.GetDocumentosPendientesAsync(
            rutCliente, rutDeudor, tipoDocumento,
            fechaVencimientoDesde, fechaVencimientoHasta, soloMorosos);
        return Ok(documentos);
    }

    /// <summary>
    /// Registra un pago con afectación de saldos
    /// Soporta pagos totales y parciales, genera excedentes automáticamente
    /// </summary>
    [HttpPost("recaudacion/pagar")]
    public async Task<IActionResult> RegistrarPago([FromBody] RegistrarPagoRequest request)
    {
        var resultado = await _cobranzaService.RegistrarPagoAsync(request);

        if (!resultado.Exito)
            return BadRequest(new { resultado.Mensaje });

        return Ok(resultado);
    }

    /// <summary>
    /// Obtiene excedentes pendientes de un cliente
    /// </summary>
    [HttpGet("excedentes/{rutCliente}")]
    public async Task<IActionResult> GetExcedentesPendientes(long rutCliente)
    {
        var excedentes = await _cobranzaService.GetExcedentesPendientesAsync(rutCliente);
        return Ok(excedentes);
    }

    /// <summary>
    /// Aplica un excedente a una operación existente
    /// </summary>
    [HttpPost("excedentes/aplicar")]
    public async Task<IActionResult> AplicarExcedente(
        [FromQuery] int numeroExcedente,
        [FromQuery] int numeroOperacion,
        [FromQuery] string usuario)
    {
        var (exito, mensaje) = await _cobranzaService.AplicarExcedenteAsync(numeroExcedente, numeroOperacion, usuario);

        if (!exito)
            return BadRequest(new { Mensaje = mensaje });

        return Ok(new { Mensaje = mensaje });
    }

    /// <summary>
    /// Obtiene resumen de cartera de un cliente
    /// </summary>
    [HttpGet("cartera/{rutCliente}")]
    public async Task<IActionResult> GetResumenCartera(long rutCliente)
    {
        var resumen = await _cobranzaService.GetResumenCarteraClienteAsync(rutCliente);
        return Ok(resumen);
    }

    /// <summary>
    /// Actualiza el estado de documentos morosos (proceso batch)
    /// </summary>
    [HttpPost("procesos/actualizar-morosos")]
    public async Task<IActionResult> ActualizarMorosos()
    {
        var documentosActualizados = await _cobranzaService.ActualizarDocumentosMorososAsync();
        return Ok(new {
            Mensaje = $"Se actualizaron {documentosActualizados} documentos como morosos",
            DocumentosActualizados = documentosActualizados
        });
    }

    // ==========================================
    // VERIFICACIÓN DIARIA (F_CO2510.frm)
    // ==========================================

    /// <summary>
    /// Obtiene verificaciones diarias por fecha
    /// </summary>
    /// <param name="fecha">Fecha de verificación</param>
    /// <param name="tipoDocumento">Tipo de documento (opcional)</param>
    /// <returns>Lista de verificaciones del día</returns>
    [HttpGet("verificacion-diaria")]
    public async Task<IActionResult> GetVerificacionesDiarias([FromQuery] DateTime? fecha, [FromQuery] int? tipoDocumento = null)
    {
        fecha ??= DateTime.Today;
        var verificaciones = await _verificacionDiariaRepository.GetByFechaAsync(fecha, tipoDocumento);

        // Mapear a DTOs con RUT formateado
        var dtos = verificaciones.Select(v => new VerificacionDiariaDto
        {
            FechaVerificacion = v.FechaVerificacion,
            HoraVerificacion = v.HoraVerificacion,
            TipoDocumento = v.TipoDocumento,
            DescripcionTipoDocumento = v.DescripcionTipoDocumento,
            RutCliente = v.RutCliente,
            RutClienteFormateado = FormatRut(v.RutCliente),
            NombreCliente = v.NombreCliente,
            RutDeudor = v.RutDeudor,
            RutDeudorFormateado = FormatRut(v.RutDeudor),
            NombreDeudor = v.NombreDeudor,
            NumeroOperacion = v.NumeroOperacion,
            NumeroDocumento = v.NumeroDocumento,
            FechaVencimiento = v.FechaVencimiento,
            MontoDocumento = v.MontoDocumento,
            EstadoVerificacion = v.EstadoVerificacion,
            DescripcionEstadoVerificacion = v.Resultado,
            Resultado = v.Resultado,
            Observacion = v.Observacion,
            UsuarioVerificacion = v.UsuarioVerificacion,
            PersonaContacto = v.PersonaContacto,
            CargoPersona = v.CargoPersona,
            FacturaRecibida = v.FacturaRecibida,
            FacturaIngresada = v.FacturaIngresada,
            AceptaNotificacion = v.AceptaNotificacion,
            DocumentoCedido = v.DocumentoCedido,
            DepositoCliente = v.DepositoCliente,
            EntregaInformacion = v.EntregaInformacion,
            TimbreDeudor = v.TimbreDeudor
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene documentos pendientes de verificación para una fecha
    /// </summary>
    /// <param name="fecha">Fecha para verificar (por defecto hoy)</param>
    /// <returns>Lista de documentos sin verificar</returns>
    [HttpGet("verificacion-diaria/pendientes")]
    public async Task<IActionResult> GetVerificacionesPendientes([FromQuery] DateTime? fecha)
    {
        fecha ??= DateTime.Today;
        var pendientes = await _verificacionDiariaRepository.GetPendientesAsync(fecha);

        var dtos = pendientes.Select(v => new VerificacionDiariaDto
        {
            FechaVerificacion = v.FechaVerificacion,
            TipoDocumento = v.TipoDocumento,
            DescripcionTipoDocumento = v.DescripcionTipoDocumento,
            RutCliente = v.RutCliente,
            RutClienteFormateado = FormatRut(v.RutCliente),
            NombreCliente = v.NombreCliente,
            RutDeudor = v.RutDeudor,
            RutDeudorFormateado = FormatRut(v.RutDeudor),
            NombreDeudor = v.NombreDeudor,
            NumeroOperacion = v.NumeroOperacion,
            NumeroDocumento = v.NumeroDocumento,
            FechaVencimiento = v.FechaVencimiento,
            MontoDocumento = v.MontoDocumento,
            EstadoVerificacion = 0,
            DescripcionEstadoVerificacion = "Pendiente"
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Registra una nueva verificación de documento
    /// </summary>
    /// <param name="dto">Datos de la verificación</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("verificacion-diaria")]
    public async Task<IActionResult> RegistrarVerificacion([FromBody] RegistrarVerificacionDto dto)
    {
        var usuario = User.Identity?.Name ?? "SISTEMA";

        var verificacion = new Factoring.Core.Entities.VerificacionDiaria
        {
            RutCliente = dto.RutCliente,
            RutDeudor = dto.RutDeudor,
            NumeroOperacion = dto.NumeroOperacion,
            NumeroDocumento = dto.NumeroDocumento,
            TipoDocumento = dto.TipoDocumento,
            FechaVerificacion = DateTime.Today,
            EstadoVerificacion = dto.EstadoVerificacion,
            Observacion = dto.Observacion,
            UsuarioVerificacion = usuario,
            PersonaContacto = dto.PersonaContacto,
            CargoPersona = dto.CargoPersona,
            FacturaRecibida = dto.FacturaRecibida,
            FacturaIngresada = dto.FacturaIngresada,
            AceptaNotificacion = dto.AceptaNotificacion,
            DocumentoCedido = dto.DocumentoCedido,
            DepositoCliente = dto.DepositoCliente,
            EntregaInformacion = dto.EntregaInformacion,
            TimbreDeudor = dto.TimbreDeudor
        };

        var result = await _verificacionDiariaRepository.RegistrarVerificacionAsync(verificacion);

        if (!result)
            return BadRequest(new { Mensaje = "No se pudo registrar la verificación" });

        return Ok(new { Mensaje = "Verificación registrada correctamente" });
    }

    /// <summary>
    /// Actualiza el resultado de una verificación existente
    /// </summary>
    [HttpPut("verificacion-diaria/resultado")]
    public async Task<IActionResult> ActualizarResultadoVerificacion(
        [FromQuery] int numeroOperacion,
        [FromQuery] int tipoDocumento,
        [FromQuery] long numeroDocumento,
        [FromQuery] string resultado,
        [FromQuery] string? observacion)
    {
        var result = await _verificacionDiariaRepository.UpdateResultadoAsync(
            numeroOperacion, tipoDocumento, numeroDocumento, resultado, observacion);

        if (!result)
            return NotFound(new { Mensaje = "No se encontró la verificación a actualizar" });

        return Ok(new { Mensaje = "Resultado actualizado correctamente" });
    }

    /// <summary>
    /// Obtiene el resumen de verificaciones del día
    /// </summary>
    [HttpGet("verificacion-diaria/resumen")]
    public async Task<IActionResult> GetResumenVerificacionDiaria([FromQuery] DateTime? fecha)
    {
        fecha ??= DateTime.Today;

        var verificados = await _verificacionDiariaRepository.GetByFechaAsync(fecha, null);
        var pendientes = await _verificacionDiariaRepository.GetPendientesAsync(fecha);

        var verificadosList = verificados.ToList();
        var pendientesList = pendientes.ToList();

        var resumen = new ResumenVerificacionDiariaDto
        {
            Fecha = fecha,
            TotalDocumentos = verificadosList.Count + pendientesList.Count,
            DocumentosVerificados = verificadosList.Count,
            DocumentosPendientes = pendientesList.Count,
            MontoTotalVerificado = verificadosList.Sum(v => v.MontoDocumento),
            MontoTotalPendiente = pendientesList.Sum(v => v.MontoDocumento),
            PorEstado = verificadosList
                .GroupBy(v => v.EstadoVerificacion)
                .Select(g => new ResumenVerificacionPorEstadoDto
                {
                    EstadoVerificacion = g.Key,
                    DescripcionEstado = g.First().Resultado,
                    Cantidad = g.Count(),
                    Monto = g.Sum(v => v.MontoDocumento)
                })
                .ToList()
        };

        return Ok(resumen);
    }

    // ==========================================
    // DOCUMENTOS NO CEDIDOS (F_CO2600.frm)
    // ==========================================

    /// <summary>
    /// Obtiene todos los documentos no cedidos
    /// </summary>
    /// <returns>Lista de documentos no cedidos</returns>
    [HttpGet("documentos-no-cedidos")]
    public async Task<IActionResult> GetDocumentosNoCedidos()
    {
        var documentos = await _documentoNoCedidoRepository.GetAllAsync();

        var dtos = documentos.Select(d => new DocumentoNoCedidoDto
        {
            NumeroOperacion = d.NumeroOperacion,
            TipoDocumento = d.TipoDocumento,
            DescripcionTipoDocumento = d.DescripcionTipoDocumento,
            NumeroDocumento = d.NumeroDocumento,
            RutCliente = d.RutCliente,
            NombreCliente = d.NombreCliente,
            RutDeudor = d.RutDeudor,
            NombreDeudor = d.NombreDeudor,
            FechaVencimiento = d.FechaVencimiento,
            MontoDocumento = d.MontoDocumento,
            MotivoNoCesion = d.MotivoNoCesion,
            FechaRegistro = d.FechaRegistro
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene documentos no cedidos de un cliente
    /// </summary>
    /// <param name="rutCliente">RUT del cliente</param>
    /// <returns>Lista de documentos no cedidos del cliente</returns>
    [HttpGet("documentos-no-cedidos/cliente/{rutCliente}")]
    public async Task<IActionResult> GetDocumentosNoCedidosByCliente(long rutCliente)
    {
        var documentos = await _documentoNoCedidoRepository.GetByClienteAsync(rutCliente);

        var dtos = documentos.Select(d => new DocumentoNoCedidoDto
        {
            NumeroOperacion = d.NumeroOperacion,
            TipoDocumento = d.TipoDocumento,
            DescripcionTipoDocumento = d.DescripcionTipoDocumento,
            NumeroDocumento = d.NumeroDocumento,
            RutCliente = d.RutCliente,
            NombreCliente = d.NombreCliente,
            RutDeudor = d.RutDeudor,
            NombreDeudor = d.NombreDeudor,
            FechaVencimiento = d.FechaVencimiento,
            MontoDocumento = d.MontoDocumento,
            MotivoNoCesion = d.MotivoNoCesion,
            FechaRegistro = d.FechaRegistro
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene documentos no cedidos de un deudor
    /// </summary>
    /// <param name="rutDeudor">RUT del deudor</param>
    /// <returns>Lista de documentos no cedidos del deudor</returns>
    [HttpGet("documentos-no-cedidos/deudor/{rutDeudor}")]
    public async Task<IActionResult> GetDocumentosNoCedidosByDeudor(long rutDeudor)
    {
        var documentos = await _documentoNoCedidoRepository.GetByDeudorAsync(rutDeudor);

        var dtos = documentos.Select(d => new DocumentoNoCedidoDto
        {
            NumeroOperacion = d.NumeroOperacion,
            TipoDocumento = d.TipoDocumento,
            DescripcionTipoDocumento = d.DescripcionTipoDocumento,
            NumeroDocumento = d.NumeroDocumento,
            RutCliente = d.RutCliente,
            NombreCliente = d.NombreCliente,
            RutDeudor = d.RutDeudor,
            NombreDeudor = d.NombreDeudor,
            FechaVencimiento = d.FechaVencimiento,
            MontoDocumento = d.MontoDocumento,
            MotivoNoCesion = d.MotivoNoCesion,
            FechaRegistro = d.FechaRegistro
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene documentos no cedidos por rango de fechas
    /// </summary>
    /// <param name="fechaDesde">Fecha desde</param>
    /// <param name="fechaHasta">Fecha hasta</param>
    /// <returns>Lista de documentos no cedidos en el rango</returns>
    [HttpGet("documentos-no-cedidos/fecha")]
    public async Task<IActionResult> GetDocumentosNoCedidosByFecha(
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        fechaDesde ??= DateTime.Today.AddMonths(-1);
        fechaHasta ??= DateTime.Today;

        var documentos = await _documentoNoCedidoRepository.GetByFechaAsync(fechaDesde, fechaHasta);

        var dtos = documentos.Select(d => new DocumentoNoCedidoDto
        {
            NumeroOperacion = d.NumeroOperacion,
            TipoDocumento = d.TipoDocumento,
            DescripcionTipoDocumento = d.DescripcionTipoDocumento,
            NumeroDocumento = d.NumeroDocumento,
            RutCliente = d.RutCliente,
            NombreCliente = d.NombreCliente,
            RutDeudor = d.RutDeudor,
            NombreDeudor = d.NombreDeudor,
            FechaVencimiento = d.FechaVencimiento,
            MontoDocumento = d.MontoDocumento,
            MotivoNoCesion = d.MotivoNoCesion,
            FechaRegistro = d.FechaRegistro
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Registra un documento como no cedido
    /// </summary>
    /// <param name="dto">Datos del documento a marcar como no cedido</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("documentos-no-cedidos")]
    public async Task<IActionResult> RegistrarDocumentoNoCedido([FromBody] RegistrarDocumentoNoCedidoDto dto)
    {
        var result = await _documentoNoCedidoRepository.RegistrarNoCedidoAsync(
            dto.TipoDocumento, dto.NumeroDocumento, dto.NumeroOperacion, dto.Motivo);

        if (!result)
            return BadRequest(new { Mensaje = "No se pudo registrar el documento como no cedido" });

        return Ok(new { Mensaje = "Documento registrado como no cedido correctamente" });
    }

    /// <summary>
    /// Obtiene resumen de documentos no cedidos
    /// </summary>
    /// <returns>Resumen con totales</returns>
    [HttpGet("documentos-no-cedidos/resumen")]
    public async Task<IActionResult> GetResumenDocumentosNoCedidos()
    {
        var documentos = await _documentoNoCedidoRepository.GetAllAsync();
        var lista = documentos.ToList();

        var resumen = new
        {
            TotalDocumentos = lista.Count,
            MontoTotal = lista.Sum(d => d.MontoDocumento),
            PorTipoDocumento = lista
                .GroupBy(d => d.TipoDocumento)
                .Select(g => new
                {
                    TipoDocumento = g.Key,
                    DescripcionTipoDocumento = g.First().DescripcionTipoDocumento,
                    Cantidad = g.Count(),
                    Monto = g.Sum(d => d.MontoDocumento)
                })
                .ToList(),
            PorCliente = lista
                .GroupBy(d => new { d.RutCliente, d.NombreCliente })
                .Select(g => new
                {
                    g.Key.RutCliente,
                    g.Key.NombreCliente,
                    Cantidad = g.Count(),
                    Monto = g.Sum(d => d.MontoDocumento)
                })
                .OrderByDescending(x => x.Monto)
                .Take(10)
                .ToList()
        };

        return Ok(resumen);
    }

    // ==========================================
    // CONSULTA MASIVA DE DOCUMENTOS
    // ==========================================

    /// <summary>
    /// Consulta masiva de documentos con filtros avanzados
    /// </summary>
    /// <param name="filtro">Filtros de búsqueda</param>
    /// <returns>Lista de documentos que coinciden con los filtros</returns>
    [HttpPost("consulta-masiva")]
    public async Task<IActionResult> ConsultaMasiva([FromBody] FiltroConsultaMasivaDto filtro)
    {
        var documentos = await _consultaMasivaRepository.GetDocumentosAsync(filtro);

        var dtos = documentos.Select(d => new DocumentoConsultaMasivaDto
        {
            NumeroOperacion = d.NumeroOperacion,
            TipoDocumento = d.TipoDocumento,
            DescripcionTipoDocumento = d.DescripcionTipoDocumento,
            NumeroDocumento = d.NumeroDocumento,
            RutCliente = d.RutCliente,
            RutClienteFormateado = FormatRut(d.RutCliente),
            NombreCliente = d.NombreCliente,
            RutDeudor = d.RutDeudor,
            RutDeudorFormateado = FormatRut(d.RutDeudor),
            NombreDeudor = d.NombreDeudor,
            FechaEmision = d.FechaEmision,
            FechaVencimiento = d.FechaVencimiento,
            FechaVencimientoReal = d.FechaVencimientoReal,
            MontoDocumento = d.MontoDocumento,
            SaldoPendiente = d.SaldoPendiente,
            MontoAbonado = d.MontoAbonado,
            EstadoDocumento = d.EstadoDocumento,
            DescripcionEstado = d.DescripcionEstado ?? GetDescripcionEstadoDocumento(d.EstadoDocumento),
            EstadoCesion = d.EstadoCesion,
            DescripcionCesion = d.EstadoCesion == 1 ? "Cedido" : "No Cedido",
            DiasVencidos = d.DiasVencidos,
            Protestado = d.Protestado,
            EnCobranza = d.EnCobranza,
            Ejecutivo = d.Ejecutivo,
            CodigoEjecutivo = d.CodigoEjecutivo
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene resumen de consulta masiva
    /// </summary>
    /// <param name="filtro">Filtros de búsqueda</param>
    /// <returns>Resumen con totales y agrupaciones</returns>
    [HttpPost("consulta-masiva/resumen")]
    public async Task<IActionResult> GetResumenConsultaMasiva([FromBody] FiltroConsultaMasivaDto filtro)
    {
        var resumen = await _consultaMasivaRepository.GetResumenAsync(filtro);
        return Ok(resumen);
    }

    /// <summary>
    /// Obtiene documentos vencidos
    /// </summary>
    /// <param name="diasVencidos">Mínimo de días vencidos (opcional)</param>
    /// <returns>Lista de documentos vencidos</returns>
    [HttpGet("consulta-masiva/vencidos")]
    public async Task<IActionResult> GetDocumentosVencidos([FromQuery] int? diasVencidos = null)
    {
        var documentos = await _consultaMasivaRepository.GetDocumentosVencidosAsync(diasVencidos);

        var dtos = documentos.Select(d => new DocumentoConsultaMasivaDto
        {
            NumeroOperacion = d.NumeroOperacion,
            TipoDocumento = d.TipoDocumento,
            DescripcionTipoDocumento = d.DescripcionTipoDocumento,
            NumeroDocumento = d.NumeroDocumento,
            RutCliente = d.RutCliente,
            RutClienteFormateado = FormatRut(d.RutCliente),
            NombreCliente = d.NombreCliente,
            RutDeudor = d.RutDeudor,
            RutDeudorFormateado = FormatRut(d.RutDeudor),
            NombreDeudor = d.NombreDeudor,
            FechaVencimiento = d.FechaVencimiento,
            MontoDocumento = d.MontoDocumento,
            SaldoPendiente = d.SaldoPendiente,
            DiasVencidos = d.DiasVencidos,
            Protestado = d.Protestado,
            EnCobranza = d.EnCobranza
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene documentos por cliente
    /// </summary>
    /// <param name="rutCliente">RUT del cliente</param>
    /// <returns>Lista de documentos del cliente</returns>
    [HttpGet("consulta-masiva/cliente/{rutCliente}")]
    public async Task<IActionResult> GetDocumentosByCliente(long rutCliente)
    {
        var documentos = await _consultaMasivaRepository.GetDocumentosByClienteAsync(rutCliente);

        var dtos = documentos.Select(d => new DocumentoConsultaMasivaDto
        {
            NumeroOperacion = d.NumeroOperacion,
            TipoDocumento = d.TipoDocumento,
            DescripcionTipoDocumento = d.DescripcionTipoDocumento,
            NumeroDocumento = d.NumeroDocumento,
            RutCliente = d.RutCliente,
            RutClienteFormateado = FormatRut(d.RutCliente),
            NombreCliente = d.NombreCliente,
            RutDeudor = d.RutDeudor,
            RutDeudorFormateado = FormatRut(d.RutDeudor),
            NombreDeudor = d.NombreDeudor,
            FechaVencimiento = d.FechaVencimiento,
            MontoDocumento = d.MontoDocumento,
            SaldoPendiente = d.SaldoPendiente,
            DiasVencidos = d.DiasVencidos
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene documentos por deudor
    /// </summary>
    /// <param name="rutDeudor">RUT del deudor</param>
    /// <returns>Lista de documentos del deudor</returns>
    [HttpGet("consulta-masiva/deudor/{rutDeudor}")]
    public async Task<IActionResult> GetDocumentosByDeudor(long rutDeudor)
    {
        var documentos = await _consultaMasivaRepository.GetDocumentosByDeudorAsync(rutDeudor);

        var dtos = documentos.Select(d => new DocumentoConsultaMasivaDto
        {
            NumeroOperacion = d.NumeroOperacion,
            TipoDocumento = d.TipoDocumento,
            DescripcionTipoDocumento = d.DescripcionTipoDocumento,
            NumeroDocumento = d.NumeroDocumento,
            RutCliente = d.RutCliente,
            RutClienteFormateado = FormatRut(d.RutCliente),
            NombreCliente = d.NombreCliente,
            RutDeudor = d.RutDeudor,
            RutDeudorFormateado = FormatRut(d.RutDeudor),
            NombreDeudor = d.NombreDeudor,
            FechaVencimiento = d.FechaVencimiento,
            MontoDocumento = d.MontoDocumento,
            SaldoPendiente = d.SaldoPendiente,
            DiasVencidos = d.DiasVencidos
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene documentos por operación
    /// </summary>
    /// <param name="numeroOperacion">Número de operación</param>
    /// <returns>Lista de documentos de la operación</returns>
    [HttpGet("consulta-masiva/operacion/{numeroOperacion}")]
    public async Task<IActionResult> GetDocumentosByOperacion(int numeroOperacion)
    {
        var documentos = await _consultaMasivaRepository.GetDocumentosByOperacionAsync(numeroOperacion);

        var dtos = documentos.Select(d => new DocumentoConsultaMasivaDto
        {
            NumeroOperacion = d.NumeroOperacion,
            TipoDocumento = d.TipoDocumento,
            DescripcionTipoDocumento = d.DescripcionTipoDocumento,
            NumeroDocumento = d.NumeroDocumento,
            RutCliente = d.RutCliente,
            NombreCliente = d.NombreCliente,
            RutDeudor = d.RutDeudor,
            NombreDeudor = d.NombreDeudor,
            FechaVencimiento = d.FechaVencimiento,
            MontoDocumento = d.MontoDocumento,
            SaldoPendiente = d.SaldoPendiente
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene documentos protestados
    /// </summary>
    /// <returns>Lista de documentos protestados</returns>
    [HttpGet("consulta-masiva/protestados")]
    public async Task<IActionResult> GetDocumentosProtestados()
    {
        var documentos = await _consultaMasivaRepository.GetDocumentosProtestadosAsync();

        var dtos = documentos.Select(d => new DocumentoConsultaMasivaDto
        {
            NumeroOperacion = d.NumeroOperacion,
            TipoDocumento = d.TipoDocumento,
            DescripcionTipoDocumento = d.DescripcionTipoDocumento,
            NumeroDocumento = d.NumeroDocumento,
            RutCliente = d.RutCliente,
            RutClienteFormateado = FormatRut(d.RutCliente),
            NombreCliente = d.NombreCliente,
            RutDeudor = d.RutDeudor,
            RutDeudorFormateado = FormatRut(d.RutDeudor),
            NombreDeudor = d.NombreDeudor,
            FechaVencimiento = d.FechaVencimiento,
            MontoDocumento = d.MontoDocumento,
            SaldoPendiente = d.SaldoPendiente,
            DiasVencidos = d.DiasVencidos,
            Protestado = true
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene documentos en cobranza judicial
    /// </summary>
    /// <returns>Lista de documentos en cobranza judicial</returns>
    [HttpGet("consulta-masiva/en-cobranza")]
    public async Task<IActionResult> GetDocumentosEnCobranza()
    {
        var documentos = await _consultaMasivaRepository.GetDocumentosEnCobranzaAsync();

        var dtos = documentos.Select(d => new DocumentoConsultaMasivaDto
        {
            NumeroOperacion = d.NumeroOperacion,
            TipoDocumento = d.TipoDocumento,
            DescripcionTipoDocumento = d.DescripcionTipoDocumento,
            NumeroDocumento = d.NumeroDocumento,
            RutCliente = d.RutCliente,
            RutClienteFormateado = FormatRut(d.RutCliente),
            NombreCliente = d.NombreCliente,
            RutDeudor = d.RutDeudor,
            RutDeudorFormateado = FormatRut(d.RutDeudor),
            NombreDeudor = d.NombreDeudor,
            FechaVencimiento = d.FechaVencimiento,
            MontoDocumento = d.MontoDocumento,
            SaldoPendiente = d.SaldoPendiente,
            DiasVencidos = d.DiasVencidos,
            EnCobranza = true
        });

        return Ok(dtos);
    }

    // ==========================================
    // HELPERS
    // ==========================================

    /// <summary>
    /// Obtiene descripción de estado de documento
    /// </summary>
    private static string GetDescripcionEstadoDocumento(int estado)
    {
        return estado switch
        {
            1 => "Vigente",
            2 => "Vencido",
            3 => "En Cobranza",
            4 => "Protestado",
            5 => "Pagado",
            6 => "Anulado",
            _ => "Otro"
        };
    }

    /// <summary>
    /// Formatea un RUT chileno con dígito verificador
    /// </summary>
    private static string FormatRut(long rut)
    {
        if (rut <= 0) return string.Empty;

        var dv = CalcularDigitoVerificador(rut);
        return $"{rut:N0}-{dv}".Replace(",", ".");
    }

    /// <summary>
    /// Calcula el dígito verificador de un RUT chileno (módulo 11)
    /// </summary>
    private static string CalcularDigitoVerificador(long rut)
    {
        var suma = 0;
        var multiplicador = 2;
        var rutStr = rut.ToString();

        for (var i = rutStr.Length - 1; i >= 0; i--)
        {
            suma += int.Parse(rutStr[i].ToString()) * multiplicador;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
        }

        var resto = 11 - (suma % 11);
        return resto switch
        {
            11 => "0",
            10 => "K",
            _ => resto.ToString()
        };
    }
}
