using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace Factoring.Infrastructure.Services;

/// <summary>
/// Servicio para registrar eventos de auditoria de cambios criticos
/// </summary>
public class AuditService
{
    private readonly IAuditRepository _auditRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditRepository auditRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _auditRepository = auditRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Registra un evento de auditoria
    /// </summary>
    public async Task RegistrarAsync(
        string entidad,
        string entidadId,
        string accion,
        string modulo,
        string? descripcion = null,
        object? valoresAnteriores = null,
        object? valoresNuevos = null,
        string nivelCriticidad = AuditNivel.Medium)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            var usuario = context?.User?.Identity?.Name ?? "sistema";
            var ip = context?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var maquina = context?.Request.Headers["X-Machine-Name"].FirstOrDefault() ?? ip;
            var correlationId = context?.TraceIdentifier;

            var audit = new AuditLog
            {
                AudEntidad = entidad,
                AudEntidadId = entidadId,
                AudAccion = accion,
                AudUsuario = usuario,
                AudFechaHora = DateTime.Now,
                AudIp = ip,
                AudMaquina = maquina,
                AudDescripcion = descripcion,
                AudCorrelationId = correlationId,
                AudModulo = modulo,
                AudNivelCriticidad = nivelCriticidad,
                AudValoresAnteriores = valoresAnteriores != null
                    ? JsonSerializer.Serialize(valoresAnteriores, new JsonSerializerOptions { WriteIndented = false })
                    : null,
                AudValoresNuevos = valoresNuevos != null
                    ? JsonSerializer.Serialize(valoresNuevos, new JsonSerializerOptions { WriteIndented = false })
                    : null
            };

            await _auditRepository.RegistrarAsync(audit);

            // Log adicional para eventos criticos
            if (nivelCriticidad == AuditNivel.High || nivelCriticidad == AuditNivel.Critical)
            {
                _logger.LogWarning(
                    "Evento de auditoria critico: {Accion} en {Entidad} [{EntidadId}] por {Usuario}. {Descripcion}",
                    accion, entidad, entidadId, usuario, descripcion);
            }
        }
        catch (Exception ex)
        {
            // No fallar la operacion principal si falla la auditoria
            _logger.LogError(ex, "Error al registrar auditoria para {Entidad} {EntidadId}", entidad, entidadId);
        }
    }

    /// <summary>
    /// Registra creacion de una entidad
    /// </summary>
    public Task RegistrarCreacionAsync<T>(
        string entidad,
        string entidadId,
        string modulo,
        T valoresNuevos,
        string? descripcion = null,
        string nivelCriticidad = AuditNivel.Medium)
    {
        return RegistrarAsync(
            entidad,
            entidadId,
            AuditAccion.Insert,
            modulo,
            descripcion ?? $"Creacion de {entidad}",
            null,
            valoresNuevos,
            nivelCriticidad);
    }

    /// <summary>
    /// Registra actualizacion de una entidad
    /// </summary>
    public Task RegistrarActualizacionAsync<T>(
        string entidad,
        string entidadId,
        string modulo,
        T valoresAnteriores,
        T valoresNuevos,
        string? descripcion = null,
        string nivelCriticidad = AuditNivel.Medium)
    {
        return RegistrarAsync(
            entidad,
            entidadId,
            AuditAccion.Update,
            modulo,
            descripcion ?? $"Actualizacion de {entidad}",
            valoresAnteriores,
            valoresNuevos,
            nivelCriticidad);
    }

    /// <summary>
    /// Registra eliminacion de una entidad
    /// </summary>
    public Task RegistrarEliminacionAsync<T>(
        string entidad,
        string entidadId,
        string modulo,
        T valoresAnteriores,
        string? descripcion = null,
        string nivelCriticidad = AuditNivel.High)
    {
        return RegistrarAsync(
            entidad,
            entidadId,
            AuditAccion.Delete,
            modulo,
            descripcion ?? $"Eliminacion de {entidad}",
            valoresAnteriores,
            null,
            nivelCriticidad);
    }

    /// <summary>
    /// Registra cambio de estado de una entidad
    /// </summary>
    public Task RegistrarCambioEstadoAsync(
        string entidad,
        string entidadId,
        string modulo,
        string estadoAnterior,
        string estadoNuevo,
        string? descripcion = null,
        string nivelCriticidad = AuditNivel.Medium)
    {
        return RegistrarAsync(
            entidad,
            entidadId,
            AuditAccion.StatusChange,
            modulo,
            descripcion ?? $"Cambio de estado de {entidad}: {estadoAnterior} -> {estadoNuevo}",
            new { Estado = estadoAnterior },
            new { Estado = estadoNuevo },
            nivelCriticidad);
    }

    /// <summary>
    /// Registra aprobacion de una operacion
    /// </summary>
    public Task RegistrarAprobacionAsync(
        string entidad,
        string entidadId,
        string modulo,
        string? descripcion = null)
    {
        return RegistrarAsync(
            entidad,
            entidadId,
            AuditAccion.Approve,
            modulo,
            descripcion ?? $"Aprobacion de {entidad}",
            null,
            null,
            AuditNivel.High);
    }

    /// <summary>
    /// Registra rechazo de una operacion
    /// </summary>
    public Task RegistrarRechazoAsync(
        string entidad,
        string entidadId,
        string modulo,
        string motivo)
    {
        return RegistrarAsync(
            entidad,
            entidadId,
            AuditAccion.Reject,
            modulo,
            $"Rechazo de {entidad}: {motivo}",
            null,
            new { Motivo = motivo },
            AuditNivel.High);
    }

    /// <summary>
    /// Registra exportacion de datos
    /// </summary>
    public Task RegistrarExportacionAsync(
        string entidad,
        string modulo,
        string formato,
        int cantidadRegistros)
    {
        return RegistrarAsync(
            entidad,
            "EXPORT",
            AuditAccion.Export,
            modulo,
            $"Exportacion de {cantidadRegistros} registros de {entidad} en formato {formato}",
            null,
            new { Formato = formato, CantidadRegistros = cantidadRegistros },
            AuditNivel.Low);
    }

    /// <summary>
    /// Obtiene registros de auditoria por entidad
    /// </summary>
    public Task<IEnumerable<AuditLog>> GetByEntidadAsync(string entidad, string entidadId)
    {
        return _auditRepository.GetByEntidadAsync(entidad, entidadId);
    }

    /// <summary>
    /// Obtiene registros de auditoria por usuario
    /// </summary>
    public Task<IEnumerable<AuditLog>> GetByUsuarioAsync(string usuario, DateTime? desde = null, DateTime? hasta = null)
    {
        return _auditRepository.GetByUsuarioAsync(usuario, desde, hasta);
    }

    /// <summary>
    /// Obtiene registros de auditoria por modulo
    /// </summary>
    public Task<IEnumerable<AuditLog>> GetByModuloAsync(string modulo, DateTime? desde = null, DateTime? hasta = null)
    {
        return _auditRepository.GetByModuloAsync(modulo, desde, hasta);
    }

    /// <summary>
    /// Obtiene registros de auditoria criticos
    /// </summary>
    public Task<IEnumerable<AuditLog>> GetCriticosAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        return _auditRepository.GetCriticosAsync(desde, hasta);
    }

    /// <summary>
    /// Busqueda general de registros de auditoria
    /// </summary>
    public Task<IEnumerable<AuditLog>> BuscarAsync(
        string? entidad = null,
        string? usuario = null,
        string? modulo = null,
        string? accion = null,
        string? nivelCriticidad = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        int limite = 100)
    {
        return _auditRepository.BuscarAsync(
            entidad, usuario, modulo, accion, nivelCriticidad, desde, hasta, limite);
    }

    // ==================== CONSULTAS PAGINADAS ====================

    /// <summary>
    /// Busqueda paginada de registros de auditoria
    /// </summary>
    public async Task<AuditoriaPaginadaDto> BuscarPaginadoAsync(AuditoriaFiltrosDto filtros)
    {
        var (registros, total) = await _auditRepository.BuscarPaginadoAsync(filtros);

        var registrosDto = registros.Select(MapToRegistroDto).ToList();

        return new AuditoriaPaginadaDto
        {
            Registros = registrosDto,
            TotalRegistros = total,
            Pagina = filtros.Pagina,
            RegistrosPorPagina = filtros.RegistrosPorPagina,
            TotalPaginas = (int)Math.Ceiling(total / (double)filtros.RegistrosPorPagina)
        };
    }

    /// <summary>
    /// Obtiene un registro por ID
    /// </summary>
    public async Task<AuditoriaRegistroDto?> GetByIdAsync(long id)
    {
        var audit = await _auditRepository.GetByIdAsync(id);
        return audit != null ? MapToRegistroDto(audit) : null;
    }

    // ==================== REPORTES Y ESTADISTICAS ====================

    /// <summary>
    /// Obtiene resumen de auditoria
    /// </summary>
    public async Task<AuditoriaResumenDto> GetResumenAsync()
    {
        var hoy = DateTime.Today;
        var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        // Ejecutar consultas en paralelo
        var totalTask = _auditRepository.ContarRegistrosAsync();
        var hoyTask = _auditRepository.ContarRegistrosAsync(hoy, hoy.AddDays(1));
        var semanaTask = _auditRepository.ContarRegistrosAsync(inicioSemana, hoy.AddDays(1));
        var mesTask = _auditRepository.ContarRegistrosAsync(inicioMes, hoy.AddDays(1));
        var nivelesTask = _auditRepository.ContarPorNivelCriticidadAsync(inicioMes, hoy.AddDays(1));
        var modulosTask = _auditRepository.ContarPorModuloAsync(inicioMes, hoy.AddDays(1));
        var usuariosTask = _auditRepository.GetUsuariosMasActivosAsync(5, inicioMes, hoy.AddDays(1));
        var accionesTask = _auditRepository.ContarPorAccionAsync(inicioMes, hoy.AddDays(1));
        var tendenciaTask = _auditRepository.GetActividadPorDiaAsync(inicioSemana, hoy);

        await Task.WhenAll(totalTask, hoyTask, semanaTask, mesTask, nivelesTask,
            modulosTask, usuariosTask, accionesTask, tendenciaTask);

        var niveles = await nivelesTask;

        return new AuditoriaResumenDto
        {
            TotalRegistros = await totalTask,
            RegistrosHoy = await hoyTask,
            RegistrosSemana = await semanaTask,
            RegistrosMes = await mesTask,
            EventosCriticos = niveles.GetValueOrDefault("CRITICAL", 0),
            EventosAltos = niveles.GetValueOrDefault("HIGH", 0),
            ActividadPorModulo = (await modulosTask)
                .Select(x => new ActividadPorModuloDto
                {
                    Modulo = x.Key,
                    Cantidad = x.Value
                }).ToList(),
            UsuariosMasActivos = (await usuariosTask)
                .Select(x => new ActividadPorUsuarioDto
                {
                    Usuario = x.Usuario,
                    Cantidad = x.Cantidad,
                    UltimaActividad = x.UltimaActividad
                }).ToList(),
            AccionesMasFrecuentes = (await accionesTask)
                .Take(10)
                .Select(x => new ActividadPorAccionDto
                {
                    Accion = x.Key,
                    Cantidad = x.Value
                }).ToList(),
            TendenciaSemanal = (await tendenciaTask)
                .Select(x => new ActividadPorDiaDto
                {
                    Fecha = x.Fecha,
                    Cantidad = x.Cantidad,
                    Criticos = x.Criticos
                }).ToList()
        };
    }

    /// <summary>
    /// Obtiene timeline de actividad de una entidad
    /// </summary>
    public async Task<TimelineEntidadDto> GetTimelineEntidadAsync(string entidad, string entidadId)
    {
        var eventos = await _auditRepository.GetByEntidadAsync(entidad, entidadId);

        return new TimelineEntidadDto
        {
            Entidad = entidad,
            EntidadId = entidadId,
            Eventos = eventos.Select(e => new TimelineEventoDto
            {
                AuditId = e.AudId,
                FechaHora = e.AudFechaHora,
                Accion = e.AudAccion,
                Usuario = e.AudUsuario,
                Descripcion = e.AudDescripcion,
                NivelCriticidad = e.AudNivelCriticidad,
                Icono = GetIconoAccion(e.AudAccion),
                Color = GetColorNivel(e.AudNivelCriticidad)
            }).ToList()
        };
    }

    /// <summary>
    /// Obtiene estadisticas detalladas por periodo
    /// </summary>
    public async Task<EstadisticasAuditoriaDto> GetEstadisticasAsync(DateTime desde, DateTime hasta)
    {
        var totalTask = _auditRepository.ContarRegistrosAsync(desde, hasta);
        var nivelesTask = _auditRepository.ContarPorNivelCriticidadAsync(desde, hasta);
        var modulosTask = _auditRepository.ContarPorModuloAsync(desde, hasta);
        var accionesTask = _auditRepository.ContarPorAccionAsync(desde, hasta);
        var usuariosTask = _auditRepository.ContarUsuariosUnicosAsync(desde, hasta);
        var ipsTask = _auditRepository.ContarIpsUnicasAsync(desde, hasta);
        var horasTask = _auditRepository.ContarPorHoraAsync(desde, hasta);

        await Task.WhenAll(totalTask, nivelesTask, modulosTask, accionesTask,
            usuariosTask, ipsTask, horasTask);

        var dias = (hasta - desde).Days + 1;
        var total = await totalTask;
        var horasPico = (await horasTask)
            .OrderByDescending(x => x.Value)
            .Take(3)
            .Select(x => new HoraPicoDto { Hora = x.Key, Cantidad = x.Value })
            .ToList();

        return new EstadisticasAuditoriaDto
        {
            PeriodoDesde = desde,
            PeriodoHasta = hasta,
            TotalEventos = total,
            EventosPorDiaPromedio = dias > 0 ? total / dias : 0,
            UsuariosUnicos = await usuariosTask,
            IpsUnicas = await ipsTask,
            EventosPorAccion = await accionesTask,
            EventosPorModulo = await modulosTask,
            EventosPorNivel = await nivelesTask,
            HorasPico = horasPico
        };
    }

    /// <summary>
    /// Obtiene reporte de actividad de usuario
    /// </summary>
    public async Task<ReporteActividadUsuarioDto> GetReporteUsuarioAsync(
        string usuario, DateTime desde, DateTime hasta)
    {
        var eventos = await _auditRepository.GetByUsuarioAsync(usuario, desde, hasta);
        var eventosList = eventos.ToList();

        // Agrupar por modulo
        var porModulo = eventosList
            .GroupBy(e => e.AudModulo ?? "SIN_MODULO")
            .Select(g => new AccionPorModuloDto
            {
                Modulo = g.Key,
                Creaciones = g.Count(e => e.AudAccion == AuditAccion.Insert),
                Actualizaciones = g.Count(e => e.AudAccion == AuditAccion.Update),
                Eliminaciones = g.Count(e => e.AudAccion == AuditAccion.Delete),
                Otros = g.Count(e => !new[] { AuditAccion.Insert, AuditAccion.Update, AuditAccion.Delete }
                    .Contains(e.AudAccion))
            }).ToList();

        return new ReporteActividadUsuarioDto
        {
            Usuario = usuario,
            FechaDesde = desde,
            FechaHasta = hasta,
            TotalAcciones = eventosList.Count,
            AccionesPorModulo = porModulo,
            UltimasAcciones = eventosList.Take(20).Select(MapToRegistroDto).ToList()
        };
    }

    // ==================== RETENCION ====================

    /// <summary>
    /// Obtiene configuracion de retencion
    /// </summary>
    public Task<ConfiguracionRetencionDto?> GetConfiguracionRetencionAsync()
    {
        return _auditRepository.GetConfiguracionRetencionAsync();
    }

    /// <summary>
    /// Actualiza configuracion de retencion
    /// </summary>
    public async Task ActualizarConfiguracionRetencionAsync(ActualizarRetencionRequest request)
    {
        var config = await _auditRepository.GetConfiguracionRetencionAsync();
        if (config == null)
        {
            throw new InvalidOperationException("No se encontro configuracion de retencion");
        }

        config.DiasRetencionGeneral = request.DiasRetencionGeneral;
        config.DiasRetencionCriticos = request.DiasRetencionCriticos;
        config.DiasRetencionLogin = request.DiasRetencionLogin;
        config.ArchivarAntesDeEliminar = request.ArchivarAntesDeEliminar;

        await _auditRepository.ActualizarConfiguracionRetencionAsync(config);

        await RegistrarAsync(
            "CONFIGURACION_RETENCION",
            config.Id.ToString(),
            AuditAccion.Update,
            "AUDITORIA",
            "Actualizacion de politica de retencion",
            null,
            request,
            AuditNivel.High);
    }

    /// <summary>
    /// Ejecuta limpieza de registros antiguos
    /// </summary>
    public async Task<ResultadoLimpiezaDto> EjecutarLimpiezaAsync()
    {
        var resultado = new ResultadoLimpiezaDto
        {
            FechaEjecucion = DateTime.Now
        };

        try
        {
            var config = await _auditRepository.GetConfiguracionRetencionAsync();
            if (config == null || !config.Activo)
            {
                resultado.Exitoso = false;
                resultado.Error = "Configuracion de retencion no activa";
                return resultado;
            }

            // Archivar si esta configurado
            if (config.ArchivarAntesDeEliminar)
            {
                var fechaArchivo = DateTime.Now.AddDays(-config.DiasRetencionGeneral);
                resultado.RegistrosArchivados = await _auditRepository.ArchivarRegistrosAsync(
                    fechaArchivo,
                    config.RutaArchivo ?? "");
            }

            // Limpiar registros generales (excluyendo criticos)
            resultado.RegistrosEliminados = await _auditRepository.LimpiarRegistrosAntiguosAsync(
                config.DiasRetencionGeneral,
                "CRITICAL");

            // Limpiar criticos con su propia politica
            resultado.RegistrosEliminados += await _auditRepository.LimpiarRegistrosAntiguosAsync(
                config.DiasRetencionCriticos,
                null);

            resultado.Exitoso = true;

            _logger.LogInformation(
                "Limpieza de auditoria completada. Eliminados: {Eliminados}, Archivados: {Archivados}",
                resultado.RegistrosEliminados,
                resultado.RegistrosArchivados);
        }
        catch (Exception ex)
        {
            resultado.Exitoso = false;
            resultado.Error = ex.Message;
            _logger.LogError(ex, "Error en limpieza de auditoria");
        }

        return resultado;
    }

    // ==================== ALERTAS ====================

    /// <summary>
    /// Detecta multiples fallos de login
    /// </summary>
    public async Task<bool> DetectarFallosLoginAsync(string? usuario, string? ip, int maxIntentos = 5, int minutos = 15)
    {
        var fallos = await _auditRepository.ContarFallosLoginAsync(usuario, ip, minutos);
        return fallos >= maxIntentos;
    }

    /// <summary>
    /// Obtiene actividad inusual de un usuario
    /// </summary>
    public async Task<IEnumerable<AuditoriaRegistroDto>> GetActividadInusualAsync(string usuario, int dias = 7)
    {
        var desde = DateTime.Now.AddDays(-dias);
        var eventos = await _auditRepository.GetActividadInusualAsync(usuario, desde);
        return eventos.Select(MapToRegistroDto);
    }

    // ==================== HELPERS ====================

    private static AuditoriaRegistroDto MapToRegistroDto(AuditLog audit)
    {
        var dto = new AuditoriaRegistroDto
        {
            Id = audit.AudId,
            Entidad = audit.AudEntidad,
            EntidadId = audit.AudEntidadId,
            Accion = audit.AudAccion,
            AccionDescripcion = GetDescripcionAccion(audit.AudAccion),
            Usuario = audit.AudUsuario,
            FechaHora = audit.AudFechaHora,
            Ip = audit.AudIp,
            Maquina = audit.AudMaquina,
            Descripcion = audit.AudDescripcion,
            Modulo = audit.AudModulo,
            NivelCriticidad = audit.AudNivelCriticidad,
            CorrelationId = audit.AudCorrelationId
        };

        // Parsear valores JSON
        if (!string.IsNullOrEmpty(audit.AudValoresAnteriores))
        {
            try
            {
                dto.ValoresAnteriores = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    audit.AudValoresAnteriores);
            }
            catch { /* Ignorar errores de parsing */ }
        }

        if (!string.IsNullOrEmpty(audit.AudValoresNuevos))
        {
            try
            {
                dto.ValoresNuevos = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    audit.AudValoresNuevos);
            }
            catch { /* Ignorar errores de parsing */ }
        }

        // Calcular cambios detallados
        if (dto.ValoresAnteriores != null && dto.ValoresNuevos != null)
        {
            dto.CambiosDetallados = new List<CambioCampoDto>();
            foreach (var key in dto.ValoresNuevos.Keys)
            {
                var valorNuevo = dto.ValoresNuevos[key]?.ToString();
                var valorAnterior = dto.ValoresAnteriores.GetValueOrDefault(key)?.ToString();

                if (valorNuevo != valorAnterior)
                {
                    dto.CambiosDetallados.Add(new CambioCampoDto
                    {
                        Campo = key,
                        ValorAnterior = valorAnterior,
                        ValorNuevo = valorNuevo
                    });
                }
            }
        }

        return dto;
    }

    private static string GetDescripcionAccion(string accion)
    {
        return accion switch
        {
            AuditAccion.Insert => "Creacion",
            AuditAccion.Update => "Actualizacion",
            AuditAccion.Delete => "Eliminacion",
            AuditAccion.View => "Consulta",
            AuditAccion.Export => "Exportacion",
            AuditAccion.Approve => "Aprobacion",
            AuditAccion.Reject => "Rechazo",
            AuditAccion.StatusChange => "Cambio de estado",
            AuditAccion.Login => "Inicio de sesion",
            AuditAccion.Logout => "Cierre de sesion",
            _ => accion
        };
    }

    private static string GetIconoAccion(string accion)
    {
        return accion switch
        {
            AuditAccion.Insert => "add_circle",
            AuditAccion.Update => "edit",
            AuditAccion.Delete => "delete",
            AuditAccion.View => "visibility",
            AuditAccion.Export => "download",
            AuditAccion.Approve => "check_circle",
            AuditAccion.Reject => "cancel",
            AuditAccion.StatusChange => "swap_horiz",
            AuditAccion.Login => "login",
            AuditAccion.Logout => "logout",
            _ => "info"
        };
    }

    private static string GetColorNivel(string nivel)
    {
        return nivel switch
        {
            AuditNivel.Critical => "error",
            AuditNivel.High => "warning",
            AuditNivel.Medium => "info",
            AuditNivel.Low => "success",
            _ => "default"
        };
    }
}
