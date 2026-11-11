using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;

namespace Factoring.Infrastructure.Services;

/// <summary>
/// Servicio de autorizacion granular
/// - Gestion de roles y permisos
/// - Restricciones por monto
/// - Flujo de aprobaciones (4 ojos)
/// - Delegacion de permisos
/// </summary>
public class AutorizacionService
{
    private readonly IAutorizacionRepository _autorizacionRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public AutorizacionService(
        IAutorizacionRepository autorizacionRepository,
        IUsuarioRepository usuarioRepository)
    {
        _autorizacionRepository = autorizacionRepository;
        _usuarioRepository = usuarioRepository;
    }

    // ==================== MODULOS Y ACCIONES ====================

    public async Task<IEnumerable<ModuloDto>> GetModulosConAccionesAsync()
    {
        var modulos = await _autorizacionRepository.GetModulosAsync();
        var acciones = await _autorizacionRepository.GetAccionesAsync();

        return modulos.Select(m => new ModuloDto
        {
            Id = m.Id,
            Codigo = m.Codigo,
            Nombre = m.Nombre,
            Descripcion = m.Descripcion,
            Icono = m.Icono,
            Orden = m.Orden,
            Acciones = acciones
                .Where(a => a.ModuloId == m.Id)
                .Select(a => new AccionDto
                {
                    Id = a.Id,
                    Codigo = a.Codigo,
                    Nombre = a.Nombre,
                    Descripcion = a.Descripcion,
                    RequiereAprobacion = a.RequiereAprobacion,
                    MontoMaximo = a.MontoMaximo
                }).ToList()
        });
    }

    // ==================== ROLES ====================

    public async Task<IEnumerable<RolDto>> GetRolesAsync()
    {
        var roles = await _autorizacionRepository.GetRolesAsync();
        var result = new List<RolDto>();

        foreach (var rol in roles)
        {
            var cantidadUsuarios = await _autorizacionRepository.ContarUsuariosPorRolAsync(rol.Id);
            var rolPadre = rol.RolPadreId.HasValue
                ? await _autorizacionRepository.GetRolByIdAsync(rol.RolPadreId.Value)
                : null;

            result.Add(new RolDto
            {
                Id = rol.Id,
                Codigo = rol.Codigo,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion,
                Nivel = rol.Nivel,
                RolPadreId = rol.RolPadreId,
                RolPadreNombre = rolPadre?.Nombre,
                EsAdministrador = rol.EsAdministrador,
                CantidadUsuarios = cantidadUsuarios
            });
        }

        return result;
    }

    public async Task<RolDto?> GetRolConPermisosAsync(int rolId)
    {
        var rol = await _autorizacionRepository.GetRolByIdAsync(rolId);
        if (rol == null) return null;

        var permisos = await _autorizacionRepository.GetPermisosRolAsync(rolId);
        var acciones = await _autorizacionRepository.GetAccionesAsync();
        var modulos = await _autorizacionRepository.GetModulosAsync();
        var cantidadUsuarios = await _autorizacionRepository.ContarUsuariosPorRolAsync(rolId);

        return new RolDto
        {
            Id = rol.Id,
            Codigo = rol.Codigo,
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion,
            Nivel = rol.Nivel,
            RolPadreId = rol.RolPadreId,
            EsAdministrador = rol.EsAdministrador,
            CantidadUsuarios = cantidadUsuarios,
            Permisos = permisos.Select(p =>
            {
                var accion = acciones.FirstOrDefault(a => a.Id == p.AccionId);
                var modulo = accion != null ? modulos.FirstOrDefault(m => m.Id == accion.ModuloId) : null;
                return new PermisoRolDto
                {
                    AccionId = p.AccionId,
                    AccionCodigo = accion?.Codigo ?? "",
                    AccionNombre = accion?.Nombre ?? "",
                    ModuloCodigo = modulo?.Codigo ?? "",
                    ModuloNombre = modulo?.Nombre ?? "",
                    Permitido = p.Permitido,
                    MontoMaximo = p.MontoMaximo,
                    SucursalId = p.SucursalId
                };
            }).ToList()
        };
    }

    public async Task<int> CrearRolAsync(CrearRolRequest request, string usuarioCreador)
    {
        var rol = new Rol
        {
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Nivel = request.Nivel,
            RolPadreId = request.RolPadreId,
            EsAdministrador = request.EsAdministrador
        };

        var rolId = await _autorizacionRepository.CrearRolAsync(rol);

        // Asignar permisos
        foreach (var permiso in request.Permisos)
        {
            await _autorizacionRepository.AsignarPermisoRolAsync(new RolPermiso
            {
                RolId = rolId,
                AccionId = permiso.AccionId,
                Permitido = permiso.Permitido,
                MontoMaximo = permiso.MontoMaximo,
                SucursalId = permiso.SucursalId
            });
        }

        return rolId;
    }

    public async Task ActualizarRolAsync(int rolId, CrearRolRequest request)
    {
        var rol = await _autorizacionRepository.GetRolByIdAsync(rolId);
        if (rol == null) throw new InvalidOperationException("Rol no encontrado");

        rol.Codigo = request.Codigo;
        rol.Nombre = request.Nombre;
        rol.Descripcion = request.Descripcion;
        rol.Nivel = request.Nivel;
        rol.RolPadreId = request.RolPadreId;
        rol.EsAdministrador = request.EsAdministrador;

        await _autorizacionRepository.ActualizarRolAsync(rol);

        // Actualizar permisos
        var permisos = request.Permisos.Select(p => new RolPermiso
        {
            AccionId = p.AccionId,
            Permitido = p.Permitido,
            MontoMaximo = p.MontoMaximo,
            SucursalId = p.SucursalId
        });

        await _autorizacionRepository.ActualizarPermisosRolAsync(rolId, permisos);
    }

    public async Task EliminarRolAsync(int rolId)
    {
        var usuarios = await _autorizacionRepository.ContarUsuariosPorRolAsync(rolId);
        if (usuarios > 0)
            throw new InvalidOperationException($"No se puede eliminar el rol, tiene {usuarios} usuarios asignados");

        await _autorizacionRepository.EliminarRolAsync(rolId);
    }

    // ==================== USUARIOS Y PERMISOS ====================

    public async Task<UsuarioPermisosDto?> GetPermisosUsuarioAsync(string usuarioLogin)
    {
        var usuario = await _usuarioRepository.GetByLoginAsync(usuarioLogin);
        if (usuario == null) return null;

        var roles = await _autorizacionRepository.GetRolesUsuarioAsync(usuarioLogin);
        var permisosEspeciales = await _autorizacionRepository.GetPermisosUsuarioAsync(usuarioLogin);
        var permisosEfectivos = await _autorizacionRepository.GetPermisosEfectivosAsync(usuarioLogin);

        var rolesData = await _autorizacionRepository.GetRolesAsync();
        var acciones = await _autorizacionRepository.GetAccionesAsync();
        var modulos = await _autorizacionRepository.GetModulosAsync();

        return new UsuarioPermisosDto
        {
            UsuarioLogin = usuario.UsrLogUsr,
            UsuarioNombre = usuario.UsrNomUsr,
            Roles = roles.Select(r =>
            {
                var rolData = rolesData.FirstOrDefault(rd => rd.Id == r.RolId);
                return new RolAsignadoDto
                {
                    RolId = r.RolId,
                    RolCodigo = rolData?.Codigo ?? "",
                    RolNombre = rolData?.Nombre ?? "",
                    FechaAsignacion = r.FechaAsignacion,
                    FechaExpiracion = r.FechaExpiracion
                };
            }).ToList(),
            PermisosEspeciales = permisosEspeciales.Select(p =>
            {
                var accion = acciones.FirstOrDefault(a => a.Id == p.AccionId);
                return new PermisoEspecialDto
                {
                    Id = p.Id,
                    AccionCodigo = accion?.Codigo ?? "",
                    AccionNombre = accion?.Nombre ?? "",
                    Permitido = p.Permitido,
                    MontoMaximo = p.MontoMaximo,
                    Motivo = p.Motivo,
                    FechaAsignacion = p.FechaAsignacion,
                    AsignadoPor = p.AsignadoPor
                };
            }).ToList(),
            PermisosEfectivos = permisosEfectivos.Select(p =>
            {
                var modulo = modulos.FirstOrDefault(m => m.Codigo == p.ModuloCodigo);
                var accion = acciones.FirstOrDefault(a => a.Codigo == p.AccionCodigo);
                return new PermisoEfectivoDto
                {
                    ModuloCodigo = p.ModuloCodigo,
                    ModuloNombre = modulo?.Nombre ?? "",
                    AccionCodigo = p.AccionCodigo,
                    AccionNombre = accion?.Nombre ?? "",
                    Permitido = p.Permitido,
                    MontoMaximo = p.MontoMaximo,
                    RequiereAprobacion = p.RequiereAprobacion,
                    Origen = p.Origen
                };
            }).ToList()
        };
    }

    public async Task AsignarRolAsync(AsignarRolRequest request, string asignadoPor)
    {
        await _autorizacionRepository.AsignarRolUsuarioAsync(new UsuarioRol
        {
            UsuarioLogin = request.UsuarioLogin,
            RolId = request.RolId,
            FechaAsignacion = DateTime.Now,
            FechaExpiracion = request.FechaExpiracion,
            Activo = true
        });
    }

    public async Task RevocarRolAsync(string usuarioLogin, int rolId)
    {
        await _autorizacionRepository.RevocarRolUsuarioAsync(usuarioLogin, rolId);
    }

    public async Task AsignarPermisoUsuarioAsync(AsignarPermisoUsuarioRequest request, string asignadoPor)
    {
        await _autorizacionRepository.AsignarPermisoUsuarioAsync(new UsuarioPermiso
        {
            UsuarioLogin = request.UsuarioLogin,
            AccionId = request.AccionId,
            Permitido = request.Permitido,
            MontoMaximo = request.MontoMaximo,
            SucursalId = request.SucursalId,
            Motivo = request.Motivo,
            FechaAsignacion = DateTime.Now,
            AsignadoPor = asignadoPor
        });
    }

    public async Task RevocarPermisoUsuarioAsync(int permisoId)
    {
        await _autorizacionRepository.RevocarPermisoUsuarioAsync(permisoId);
    }

    // ==================== VERIFICACION DE PERMISOS ====================

    public async Task<VerificarPermisoResponse> VerificarPermisoAsync(string usuarioLogin, VerificarPermisoRequest request)
    {
        var permiso = await _autorizacionRepository.GetPermisoEfectivoAsync(usuarioLogin, request.AccionCodigo);

        if (permiso == null)
        {
            return new VerificarPermisoResponse
            {
                Permitido = false,
                MotivoRechazo = "No tiene permisos para esta accion"
            };
        }

        if (!permiso.Permitido)
        {
            return new VerificarPermisoResponse
            {
                Permitido = false,
                MotivoRechazo = "Accion denegada explicitamente"
            };
        }

        // Verificar monto
        if (request.Monto.HasValue && permiso.MontoMaximo.HasValue && request.Monto.Value > permiso.MontoMaximo.Value)
        {
            // Buscar si requiere aprobacion
            var roles = await _autorizacionRepository.GetRolesUsuarioAsync(usuarioLogin);
            foreach (var rol in roles)
            {
                var restriccion = await _autorizacionRepository.GetRestriccionMontoAsync(rol.RolId, "Operacion");
                if (restriccion != null && request.Monto.Value <= restriccion.MontoMaximo)
                {
                    if (restriccion.RequiereAprobacion)
                    {
                        var rolAprobador = restriccion.RolAprobadorId.HasValue
                            ? await _autorizacionRepository.GetRolByIdAsync(restriccion.RolAprobadorId.Value)
                            : null;

                        return new VerificarPermisoResponse
                        {
                            Permitido = true,
                            RequiereAprobacion = true,
                            MontoMaximoPermitido = permiso.MontoMaximo,
                            NivelAprobacionRequerido = rolAprobador?.Nivel ?? 1
                        };
                    }
                }
            }

            return new VerificarPermisoResponse
            {
                Permitido = false,
                MotivoRechazo = $"Monto excede el maximo permitido ({permiso.MontoMaximo:N0})",
                MontoMaximoPermitido = permiso.MontoMaximo
            };
        }

        // Verificar sucursal
        if (request.SucursalId.HasValue && permiso.SucursalId.HasValue && permiso.SucursalId.Value != request.SucursalId.Value)
        {
            return new VerificarPermisoResponse
            {
                Permitido = false,
                MotivoRechazo = "No tiene permisos para esta sucursal"
            };
        }

        return new VerificarPermisoResponse
        {
            Permitido = true,
            RequiereAprobacion = permiso.RequiereAprobacion,
            MontoMaximoPermitido = permiso.MontoMaximo,
            NivelAprobacionRequerido = permiso.RequiereAprobacion ? 1 : null
        };
    }

    public async Task<bool> TienePermisoAsync(string usuarioLogin, string accionCodigo, decimal? monto = null, int? sucursalId = null)
    {
        return await _autorizacionRepository.TienePermisoAsync(usuarioLogin, accionCodigo, monto, sucursalId);
    }

    // ==================== APROBACIONES (4 OJOS) ====================

    public async Task<IEnumerable<SolicitudAprobacionDto>> GetSolicitudesPendientesAsync(string usuarioLogin)
    {
        var solicitudes = await _autorizacionRepository.GetSolicitudesPendientesParaUsuarioAsync(usuarioLogin);
        var result = new List<SolicitudAprobacionDto>();

        foreach (var s in solicitudes)
        {
            var solicitante = await _usuarioRepository.GetByLoginAsync(s.SolicitadoPor);
            result.Add(new SolicitudAprobacionDto
            {
                Id = s.Id,
                Tipo = s.Tipo,
                EntidadId = s.EntidadId,
                SolicitadoPor = s.SolicitadoPor,
                SolicitadoPorNombre = solicitante?.UsrNomUsr ?? s.SolicitadoPor,
                FechaSolicitud = s.FechaSolicitud,
                Estado = s.Estado,
                Comentarios = s.Comentarios,
                Monto = s.Monto,
                NivelAprobacionRequerido = s.NivelAprobacionRequerido,
                PuedeAprobar = true
            });
        }

        return result;
    }

    public async Task<int> CrearSolicitudAprobacionAsync(CrearSolicitudAprobacionRequest request, string solicitadoPor, int nivelRequerido)
    {
        return await _autorizacionRepository.CrearSolicitudAprobacionAsync(new SolicitudAprobacion
        {
            Tipo = request.Tipo,
            EntidadId = request.EntidadId,
            SolicitadoPor = solicitadoPor,
            Comentarios = request.Comentarios,
            Monto = request.Monto,
            NivelAprobacionRequerido = nivelRequerido
        });
    }

    public async Task<bool> ProcesarAprobacionAsync(ProcesarAprobacionRequest request, string procesadoPor)
    {
        var solicitud = await _autorizacionRepository.GetSolicitudAprobacionAsync(request.SolicitudId);
        if (solicitud == null || solicitud.Estado != "Pendiente")
            return false;

        // Verificar que el usuario puede aprobar
        var roles = await _autorizacionRepository.GetRolesUsuarioAsync(procesadoPor);
        var rolesData = await _autorizacionRepository.GetRolesAsync();
        var puedeAprobar = roles.Any(r =>
        {
            var rol = rolesData.FirstOrDefault(rd => rd.Id == r.RolId);
            return rol != null && rol.Nivel <= solicitud.NivelAprobacionRequerido;
        });

        if (!puedeAprobar)
            return false;

        // No puede aprobar su propia solicitud
        if (solicitud.SolicitadoPor == procesadoPor)
            return false;

        solicitud.AprobadoPor = procesadoPor;
        solicitud.FechaAprobacion = DateTime.Now;
        solicitud.Estado = request.Aprobar ? "Aprobado" : "Rechazado";
        if (!string.IsNullOrEmpty(request.Comentarios))
            solicitud.Comentarios = request.Comentarios;

        await _autorizacionRepository.ActualizarSolicitudAprobacionAsync(solicitud);

        // Registrar historial
        await _autorizacionRepository.RegistrarHistorialAprobacionAsync(new HistorialAprobacion
        {
            SolicitudId = request.SolicitudId,
            Usuario = procesadoPor,
            Accion = request.Aprobar ? "Aprobar" : "Rechazar",
            Fecha = DateTime.Now,
            Comentarios = request.Comentarios
        });

        return true;
    }

    public async Task<IEnumerable<HistorialAprobacionDto>> GetHistorialAprobacionAsync(int solicitudId)
    {
        var historial = await _autorizacionRepository.GetHistorialAprobacionAsync(solicitudId);
        var result = new List<HistorialAprobacionDto>();

        foreach (var h in historial)
        {
            var usuario = await _usuarioRepository.GetByLoginAsync(h.Usuario);
            result.Add(new HistorialAprobacionDto
            {
                Usuario = h.Usuario,
                UsuarioNombre = usuario?.UsrNomUsr ?? h.Usuario,
                Accion = h.Accion,
                Fecha = h.Fecha,
                Comentarios = h.Comentarios
            });
        }

        return result;
    }

    // ==================== DELEGACIONES ====================

    public async Task<IEnumerable<DelegacionDto>> GetDelegacionesAsync(string usuarioLogin)
    {
        var recibidas = await _autorizacionRepository.GetDelegacionesActivasAsync(usuarioLogin);
        var otorgadas = await _autorizacionRepository.GetDelegacionesOtorgadasAsync(usuarioLogin);

        var todas = recibidas.Concat(otorgadas).DistinctBy(d => d.Id);
        var result = new List<DelegacionDto>();

        foreach (var d in todas)
        {
            var origen = await _usuarioRepository.GetByLoginAsync(d.UsuarioOrigen);
            var destino = await _usuarioRepository.GetByLoginAsync(d.UsuarioDestino);
            result.Add(new DelegacionDto
            {
                Id = d.Id,
                UsuarioOrigen = d.UsuarioOrigen,
                UsuarioOrigenNombre = origen?.UsrNomUsr ?? d.UsuarioOrigen,
                UsuarioDestino = d.UsuarioDestino,
                UsuarioDestinoNombre = destino?.UsrNomUsr ?? d.UsuarioDestino,
                FechaInicio = d.FechaInicio,
                FechaFin = d.FechaFin,
                Motivo = d.Motivo,
                Activa = d.Activa
            });
        }

        return result;
    }

    public async Task<int> CrearDelegacionAsync(CrearDelegacionRequest request, string usuarioOrigen)
    {
        if (await _autorizacionRepository.TieneDelegacionActivaAsync(usuarioOrigen, request.UsuarioDestino))
            throw new InvalidOperationException("Ya existe una delegacion activa para este usuario");

        return await _autorizacionRepository.CrearDelegacionAsync(new DelegacionPermiso
        {
            UsuarioOrigen = usuarioOrigen,
            UsuarioDestino = request.UsuarioDestino,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            Motivo = request.Motivo,
            Activa = true
        });
    }

    public async Task CancelarDelegacionAsync(int delegacionId, string usuarioSolicitante)
    {
        await _autorizacionRepository.CancelarDelegacionAsync(delegacionId);
    }

    // ==================== RESTRICCIONES POR MONTO ====================

    public async Task<IEnumerable<RestriccionMontoDto>> GetRestriccionesMontoAsync()
    {
        var restricciones = await _autorizacionRepository.GetRestriccionesMontoAsync();
        var roles = await _autorizacionRepository.GetRolesAsync();

        return restricciones.Select(r =>
        {
            var rol = roles.FirstOrDefault(ro => ro.Id == r.RolId);
            var rolAprobador = r.RolAprobadorId.HasValue
                ? roles.FirstOrDefault(ro => ro.Id == r.RolAprobadorId.Value)
                : null;

            return new RestriccionMontoDto
            {
                Id = r.Id,
                RolId = r.RolId,
                RolNombre = rol?.Nombre ?? "",
                TipoOperacion = r.TipoOperacion,
                MontoMinimo = r.MontoMinimo,
                MontoMaximo = r.MontoMaximo,
                RequiereAprobacion = r.RequiereAprobacion,
                RolAprobadorId = r.RolAprobadorId,
                RolAprobadorNombre = rolAprobador?.Nombre
            };
        });
    }

    public async Task<int> CrearRestriccionMontoAsync(CrearRestriccionMontoRequest request)
    {
        return await _autorizacionRepository.CrearRestriccionMontoAsync(new RestriccionMonto
        {
            RolId = request.RolId,
            TipoOperacion = request.TipoOperacion,
            MontoMinimo = request.MontoMinimo,
            MontoMaximo = request.MontoMaximo,
            RequiereAprobacion = request.RequiereAprobacion,
            RolAprobadorId = request.RolAprobadorId
        });
    }

    // ==================== RESUMEN ====================

    public async Task<ResumenSeguridadDto> GetResumenSeguridadAsync()
    {
        var roles = await _autorizacionRepository.GetRolesAsync();
        var modulos = await _autorizacionRepository.GetModulosAsync();
        var acciones = await _autorizacionRepository.GetAccionesAsync();
        var pendientes = await _autorizacionRepository.ContarSolicitudesPendientesAsync();
        var delegaciones = await _autorizacionRepository.ContarDelegacionesActivasAsync();

        var rolesMasUsados = new List<RolResumenDto>();
        foreach (var rol in roles.Take(5))
        {
            var cantidad = await _autorizacionRepository.ContarUsuariosPorRolAsync(rol.Id);
            rolesMasUsados.Add(new RolResumenDto
            {
                RolNombre = rol.Nombre,
                CantidadUsuarios = cantidad
            });
        }

        return new ResumenSeguridadDto
        {
            TotalRoles = roles.Count(),
            TotalModulos = modulos.Count(),
            TotalAcciones = acciones.Count(),
            SolicitudesPendientes = pendientes,
            DelegacionesActivas = delegaciones,
            RolesMasUsados = rolesMasUsados.OrderByDescending(r => r.CantidadUsuarios).ToList()
        };
    }
}
