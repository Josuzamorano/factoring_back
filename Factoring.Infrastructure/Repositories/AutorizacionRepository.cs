using Dapper;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;

namespace Factoring.Infrastructure.Repositories;

public class AutorizacionRepository : IAutorizacionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AutorizacionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // ==================== MODULOS ====================

    public async Task<IEnumerable<Modulo>> GetModulosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Modulo>(@"
            SELECT Id, Codigo, Nombre, Descripcion, Icono, Orden, Activo
            FROM AUT_MODULO WHERE Activo = 1 ORDER BY Orden");
    }

    public async Task<Modulo?> GetModuloByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Modulo>(@"
            SELECT Id, Codigo, Nombre, Descripcion, Icono, Orden, Activo
            FROM AUT_MODULO WHERE Id = @Id", new { Id = id });
    }

    public async Task<Modulo?> GetModuloByCodigoAsync(string codigo)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Modulo>(@"
            SELECT Id, Codigo, Nombre, Descripcion, Icono, Orden, Activo
            FROM AUT_MODULO WHERE Codigo = @Codigo", new { Codigo = codigo });
    }

    // ==================== ACCIONES ====================

    public async Task<IEnumerable<Accion>> GetAccionesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Accion>(@"
            SELECT Id, ModuloId, Codigo, Nombre, Descripcion, RequiereAprobacion, MontoMaximo, Activo
            FROM AUT_ACCION WHERE Activo = 1 ORDER BY ModuloId, Codigo");
    }

    public async Task<IEnumerable<Accion>> GetAccionesByModuloAsync(int moduloId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Accion>(@"
            SELECT Id, ModuloId, Codigo, Nombre, Descripcion, RequiereAprobacion, MontoMaximo, Activo
            FROM AUT_ACCION WHERE ModuloId = @ModuloId AND Activo = 1 ORDER BY Codigo",
            new { ModuloId = moduloId });
    }

    public async Task<Accion?> GetAccionByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Accion>(@"
            SELECT Id, ModuloId, Codigo, Nombre, Descripcion, RequiereAprobacion, MontoMaximo, Activo
            FROM AUT_ACCION WHERE Id = @Id", new { Id = id });
    }

    public async Task<Accion?> GetAccionByCodigoAsync(string codigo)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Accion>(@"
            SELECT Id, ModuloId, Codigo, Nombre, Descripcion, RequiereAprobacion, MontoMaximo, Activo
            FROM AUT_ACCION WHERE Codigo = @Codigo", new { Codigo = codigo });
    }

    // ==================== ROLES ====================

    public async Task<IEnumerable<Rol>> GetRolesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Rol>(@"
            SELECT Id, Codigo, Nombre, Descripcion, Nivel, RolPadreId, EsAdministrador, Activo
            FROM AUT_ROL WHERE Activo = 1 ORDER BY Nivel, Nombre");
    }

    public async Task<Rol?> GetRolByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Rol>(@"
            SELECT Id, Codigo, Nombre, Descripcion, Nivel, RolPadreId, EsAdministrador, Activo
            FROM AUT_ROL WHERE Id = @Id", new { Id = id });
    }

    public async Task<Rol?> GetRolByCodigoAsync(string codigo)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Rol>(@"
            SELECT Id, Codigo, Nombre, Descripcion, Nivel, RolPadreId, EsAdministrador, Activo
            FROM AUT_ROL WHERE Codigo = @Codigo", new { Codigo = codigo });
    }

    public async Task<int> CrearRolAsync(Rol rol)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO AUT_ROL (Codigo, Nombre, Descripcion, Nivel, RolPadreId, EsAdministrador, Activo)
            VALUES (@Codigo, @Nombre, @Descripcion, @Nivel, @RolPadreId, @EsAdministrador, 1);
            SELECT CAST(SCOPE_IDENTITY() as int)", rol);
    }

    public async Task ActualizarRolAsync(Rol rol)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            UPDATE AUT_ROL SET
                Codigo = @Codigo, Nombre = @Nombre, Descripcion = @Descripcion,
                Nivel = @Nivel, RolPadreId = @RolPadreId, EsAdministrador = @EsAdministrador
            WHERE Id = @Id", rol);
    }

    public async Task EliminarRolAsync(int rolId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("UPDATE AUT_ROL SET Activo = 0 WHERE Id = @RolId", new { RolId = rolId });
    }

    public async Task<int> ContarUsuariosPorRolAsync(int rolId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM AUT_USUARIO_ROL
            WHERE RolId = @RolId AND Activo = 1", new { RolId = rolId });
    }

    // ==================== PERMISOS DE ROL ====================

    public async Task<IEnumerable<RolPermiso>> GetPermisosRolAsync(int rolId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<RolPermiso>(@"
            SELECT Id, RolId, AccionId, Permitido, MontoMaximo, SucursalId
            FROM AUT_ROL_PERMISO WHERE RolId = @RolId", new { RolId = rolId });
    }

    public async Task AsignarPermisoRolAsync(RolPermiso permiso)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            MERGE AUT_ROL_PERMISO AS target
            USING (SELECT @RolId AS RolId, @AccionId AS AccionId) AS source
            ON target.RolId = source.RolId AND target.AccionId = source.AccionId
            WHEN MATCHED THEN
                UPDATE SET Permitido = @Permitido, MontoMaximo = @MontoMaximo, SucursalId = @SucursalId
            WHEN NOT MATCHED THEN
                INSERT (RolId, AccionId, Permitido, MontoMaximo, SucursalId)
                VALUES (@RolId, @AccionId, @Permitido, @MontoMaximo, @SucursalId);", permiso);
    }

    public async Task RevocarPermisoRolAsync(int rolId, int accionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            DELETE FROM AUT_ROL_PERMISO WHERE RolId = @RolId AND AccionId = @AccionId",
            new { RolId = rolId, AccionId = accionId });
    }

    public async Task ActualizarPermisosRolAsync(int rolId, IEnumerable<RolPermiso> permisos)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Eliminar permisos actuales
        await connection.ExecuteAsync("DELETE FROM AUT_ROL_PERMISO WHERE RolId = @RolId", new { RolId = rolId });

        // Insertar nuevos
        foreach (var permiso in permisos)
        {
            permiso.RolId = rolId;
            await connection.ExecuteAsync(@"
                INSERT INTO AUT_ROL_PERMISO (RolId, AccionId, Permitido, MontoMaximo, SucursalId)
                VALUES (@RolId, @AccionId, @Permitido, @MontoMaximo, @SucursalId)", permiso);
        }
    }

    // ==================== USUARIOS Y ROLES ====================

    public async Task<IEnumerable<UsuarioRol>> GetRolesUsuarioAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<UsuarioRol>(@"
            SELECT Id, UsuarioLogin, RolId, FechaAsignacion, FechaExpiracion, Activo
            FROM AUT_USUARIO_ROL
            WHERE UsuarioLogin = @UsuarioLogin AND Activo = 1
              AND (FechaExpiracion IS NULL OR FechaExpiracion > GETDATE())",
            new { UsuarioLogin = usuarioLogin });
    }

    public async Task AsignarRolUsuarioAsync(UsuarioRol asignacion)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO AUT_USUARIO_ROL (UsuarioLogin, RolId, FechaAsignacion, FechaExpiracion, Activo)
            VALUES (@UsuarioLogin, @RolId, GETDATE(), @FechaExpiracion, 1)", asignacion);
    }

    public async Task RevocarRolUsuarioAsync(string usuarioLogin, int rolId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            UPDATE AUT_USUARIO_ROL SET Activo = 0
            WHERE UsuarioLogin = @UsuarioLogin AND RolId = @RolId",
            new { UsuarioLogin = usuarioLogin, RolId = rolId });
    }

    public async Task<bool> UsuarioTieneRolAsync(string usuarioLogin, int rolId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM AUT_USUARIO_ROL
            WHERE UsuarioLogin = @UsuarioLogin AND RolId = @RolId AND Activo = 1
              AND (FechaExpiracion IS NULL OR FechaExpiracion > GETDATE())",
            new { UsuarioLogin = usuarioLogin, RolId = rolId });
        return count > 0;
    }

    // ==================== PERMISOS ESPECIALES DE USUARIO ====================

    public async Task<IEnumerable<UsuarioPermiso>> GetPermisosUsuarioAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<UsuarioPermiso>(@"
            SELECT Id, UsuarioLogin, AccionId, Permitido, MontoMaximo, SucursalId, Motivo, FechaAsignacion, AsignadoPor
            FROM AUT_USUARIO_PERMISO WHERE UsuarioLogin = @UsuarioLogin",
            new { UsuarioLogin = usuarioLogin });
    }

    public async Task AsignarPermisoUsuarioAsync(UsuarioPermiso permiso)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            MERGE AUT_USUARIO_PERMISO AS target
            USING (SELECT @UsuarioLogin AS UsuarioLogin, @AccionId AS AccionId) AS source
            ON target.UsuarioLogin = source.UsuarioLogin AND target.AccionId = source.AccionId
            WHEN MATCHED THEN
                UPDATE SET Permitido = @Permitido, MontoMaximo = @MontoMaximo, SucursalId = @SucursalId,
                           Motivo = @Motivo, FechaAsignacion = GETDATE(), AsignadoPor = @AsignadoPor
            WHEN NOT MATCHED THEN
                INSERT (UsuarioLogin, AccionId, Permitido, MontoMaximo, SucursalId, Motivo, FechaAsignacion, AsignadoPor)
                VALUES (@UsuarioLogin, @AccionId, @Permitido, @MontoMaximo, @SucursalId, @Motivo, GETDATE(), @AsignadoPor);",
            permiso);
    }

    public async Task RevocarPermisoUsuarioAsync(int permisoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM AUT_USUARIO_PERMISO WHERE Id = @Id", new { Id = permisoId });
    }

    // ==================== PERMISOS EFECTIVOS ====================

    public async Task<IEnumerable<PermisoEfectivo>> GetPermisosEfectivosAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<PermisoEfectivo>(@"
            WITH PermisosRol AS (
                SELECT
                    @UsuarioLogin AS UsuarioLogin,
                    m.Codigo AS ModuloCodigo,
                    a.Codigo AS AccionCodigo,
                    rp.Permitido,
                    COALESCE(rp.MontoMaximo, a.MontoMaximo) AS MontoMaximo,
                    rp.SucursalId,
                    'Rol: ' + r.Nombre AS Origen,
                    a.RequiereAprobacion
                FROM AUT_USUARIO_ROL ur
                JOIN AUT_ROL r ON r.Id = ur.RolId AND r.Activo = 1
                JOIN AUT_ROL_PERMISO rp ON rp.RolId = r.Id
                JOIN AUT_ACCION a ON a.Id = rp.AccionId AND a.Activo = 1
                JOIN AUT_MODULO m ON m.Id = a.ModuloId AND m.Activo = 1
                WHERE ur.UsuarioLogin = @UsuarioLogin AND ur.Activo = 1
                  AND (ur.FechaExpiracion IS NULL OR ur.FechaExpiracion > GETDATE())
            ),
            PermisosUsuario AS (
                SELECT
                    up.UsuarioLogin,
                    m.Codigo AS ModuloCodigo,
                    a.Codigo AS AccionCodigo,
                    up.Permitido,
                    COALESCE(up.MontoMaximo, a.MontoMaximo) AS MontoMaximo,
                    up.SucursalId,
                    'Usuario' AS Origen,
                    a.RequiereAprobacion
                FROM AUT_USUARIO_PERMISO up
                JOIN AUT_ACCION a ON a.Id = up.AccionId AND a.Activo = 1
                JOIN AUT_MODULO m ON m.Id = a.ModuloId AND m.Activo = 1
                WHERE up.UsuarioLogin = @UsuarioLogin
            ),
            PermisosDelegacion AS (
                SELECT
                    d.UsuarioDestino AS UsuarioLogin,
                    m.Codigo AS ModuloCodigo,
                    a.Codigo AS AccionCodigo,
                    rp.Permitido,
                    COALESCE(rp.MontoMaximo, a.MontoMaximo) AS MontoMaximo,
                    rp.SucursalId,
                    'Delegacion de: ' + d.UsuarioOrigen AS Origen,
                    a.RequiereAprobacion
                FROM AUT_DELEGACION d
                JOIN AUT_USUARIO_ROL ur ON ur.UsuarioLogin = d.UsuarioOrigen AND ur.Activo = 1
                JOIN AUT_ROL_PERMISO rp ON rp.RolId = ur.RolId
                JOIN AUT_ACCION a ON a.Id = rp.AccionId AND a.Activo = 1
                JOIN AUT_MODULO m ON m.Id = a.ModuloId AND m.Activo = 1
                WHERE d.UsuarioDestino = @UsuarioLogin AND d.Activa = 1
                  AND d.FechaInicio <= GETDATE() AND d.FechaFin >= GETDATE()
            )
            SELECT * FROM PermisosUsuario
            UNION ALL
            SELECT * FROM PermisosRol pr
            WHERE NOT EXISTS (SELECT 1 FROM PermisosUsuario pu
                              WHERE pu.AccionCodigo = pr.AccionCodigo)
            UNION ALL
            SELECT * FROM PermisosDelegacion pd
            WHERE NOT EXISTS (SELECT 1 FROM PermisosUsuario pu
                              WHERE pu.AccionCodigo = pd.AccionCodigo)
              AND NOT EXISTS (SELECT 1 FROM PermisosRol pr
                              WHERE pr.AccionCodigo = pd.AccionCodigo)",
            new { UsuarioLogin = usuarioLogin });
    }

    public async Task<PermisoEfectivo?> GetPermisoEfectivoAsync(string usuarioLogin, string accionCodigo)
    {
        var permisos = await GetPermisosEfectivosAsync(usuarioLogin);
        return permisos.FirstOrDefault(p => p.AccionCodigo == accionCodigo);
    }

    public async Task<bool> TienePermisoAsync(string usuarioLogin, string accionCodigo, decimal? monto = null, int? sucursalId = null)
    {
        var permiso = await GetPermisoEfectivoAsync(usuarioLogin, accionCodigo);
        if (permiso == null || !permiso.Permitido)
            return false;

        // Verificar monto
        if (monto.HasValue && permiso.MontoMaximo.HasValue && monto.Value > permiso.MontoMaximo.Value)
            return false;

        // Verificar sucursal
        if (sucursalId.HasValue && permiso.SucursalId.HasValue && permiso.SucursalId.Value != sucursalId.Value)
            return false;

        return true;
    }

    // ==================== RESTRICCIONES POR MONTO ====================

    public async Task<IEnumerable<RestriccionMonto>> GetRestriccionesMontoAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<RestriccionMonto>(@"
            SELECT Id, RolId, TipoOperacion, MontoMinimo, MontoMaximo, RequiereAprobacion, RolAprobadorId
            FROM AUT_RESTRICCION_MONTO ORDER BY RolId, TipoOperacion");
    }

    public async Task<IEnumerable<RestriccionMonto>> GetRestriccionesMontoRolAsync(int rolId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<RestriccionMonto>(@"
            SELECT Id, RolId, TipoOperacion, MontoMinimo, MontoMaximo, RequiereAprobacion, RolAprobadorId
            FROM AUT_RESTRICCION_MONTO WHERE RolId = @RolId", new { RolId = rolId });
    }

    public async Task<RestriccionMonto?> GetRestriccionMontoAsync(int rolId, string tipoOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<RestriccionMonto>(@"
            SELECT Id, RolId, TipoOperacion, MontoMinimo, MontoMaximo, RequiereAprobacion, RolAprobadorId
            FROM AUT_RESTRICCION_MONTO WHERE RolId = @RolId AND TipoOperacion = @TipoOperacion",
            new { RolId = rolId, TipoOperacion = tipoOperacion });
    }

    public async Task<int> CrearRestriccionMontoAsync(RestriccionMonto restriccion)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO AUT_RESTRICCION_MONTO (RolId, TipoOperacion, MontoMinimo, MontoMaximo, RequiereAprobacion, RolAprobadorId)
            VALUES (@RolId, @TipoOperacion, @MontoMinimo, @MontoMaximo, @RequiereAprobacion, @RolAprobadorId);
            SELECT CAST(SCOPE_IDENTITY() as int)", restriccion);
    }

    public async Task ActualizarRestriccionMontoAsync(RestriccionMonto restriccion)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            UPDATE AUT_RESTRICCION_MONTO SET
                MontoMinimo = @MontoMinimo, MontoMaximo = @MontoMaximo,
                RequiereAprobacion = @RequiereAprobacion, RolAprobadorId = @RolAprobadorId
            WHERE Id = @Id", restriccion);
    }

    public async Task EliminarRestriccionMontoAsync(int restriccionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM AUT_RESTRICCION_MONTO WHERE Id = @Id", new { Id = restriccionId });
    }

    // ==================== APROBACIONES ====================

    public async Task<IEnumerable<SolicitudAprobacion>> GetSolicitudesPendientesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<SolicitudAprobacion>(@"
            SELECT Id, Tipo, EntidadId, SolicitadoPor, FechaSolicitud, AprobadoPor, FechaAprobacion,
                   Estado, Comentarios, Monto, NivelAprobacionRequerido
            FROM AUT_SOLICITUD_APROBACION WHERE Estado = 'Pendiente' ORDER BY FechaSolicitud");
    }

    public async Task<IEnumerable<SolicitudAprobacion>> GetSolicitudesPendientesParaUsuarioAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<SolicitudAprobacion>(@"
            SELECT s.Id, s.Tipo, s.EntidadId, s.SolicitadoPor, s.FechaSolicitud, s.AprobadoPor, s.FechaAprobacion,
                   s.Estado, s.Comentarios, s.Monto, s.NivelAprobacionRequerido
            FROM AUT_SOLICITUD_APROBACION s
            JOIN AUT_USUARIO_ROL ur ON ur.UsuarioLogin = @UsuarioLogin AND ur.Activo = 1
            JOIN AUT_ROL r ON r.Id = ur.RolId AND r.Activo = 1 AND r.Nivel <= s.NivelAprobacionRequerido
            WHERE s.Estado = 'Pendiente' AND s.SolicitadoPor != @UsuarioLogin
            ORDER BY s.FechaSolicitud",
            new { UsuarioLogin = usuarioLogin });
    }

    public async Task<IEnumerable<SolicitudAprobacion>> GetSolicitudesPorEntidadAsync(string tipo, int entidadId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<SolicitudAprobacion>(@"
            SELECT Id, Tipo, EntidadId, SolicitadoPor, FechaSolicitud, AprobadoPor, FechaAprobacion,
                   Estado, Comentarios, Monto, NivelAprobacionRequerido
            FROM AUT_SOLICITUD_APROBACION WHERE Tipo = @Tipo AND EntidadId = @EntidadId
            ORDER BY FechaSolicitud DESC",
            new { Tipo = tipo, EntidadId = entidadId });
    }

    public async Task<SolicitudAprobacion?> GetSolicitudAprobacionAsync(int solicitudId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<SolicitudAprobacion>(@"
            SELECT Id, Tipo, EntidadId, SolicitadoPor, FechaSolicitud, AprobadoPor, FechaAprobacion,
                   Estado, Comentarios, Monto, NivelAprobacionRequerido
            FROM AUT_SOLICITUD_APROBACION WHERE Id = @Id", new { Id = solicitudId });
    }

    public async Task<int> CrearSolicitudAprobacionAsync(SolicitudAprobacion solicitud)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO AUT_SOLICITUD_APROBACION
                (Tipo, EntidadId, SolicitadoPor, FechaSolicitud, Estado, Comentarios, Monto, NivelAprobacionRequerido)
            VALUES (@Tipo, @EntidadId, @SolicitadoPor, GETDATE(), 'Pendiente', @Comentarios, @Monto, @NivelAprobacionRequerido);
            SELECT CAST(SCOPE_IDENTITY() as int)", solicitud);
    }

    public async Task ActualizarSolicitudAprobacionAsync(SolicitudAprobacion solicitud)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            UPDATE AUT_SOLICITUD_APROBACION SET
                AprobadoPor = @AprobadoPor, FechaAprobacion = @FechaAprobacion,
                Estado = @Estado, Comentarios = @Comentarios
            WHERE Id = @Id", solicitud);
    }

    public async Task<IEnumerable<HistorialAprobacion>> GetHistorialAprobacionAsync(int solicitudId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<HistorialAprobacion>(@"
            SELECT Id, SolicitudId, Usuario, Accion, Fecha, Comentarios
            FROM AUT_HISTORIAL_APROBACION WHERE SolicitudId = @SolicitudId ORDER BY Fecha",
            new { SolicitudId = solicitudId });
    }

    public async Task RegistrarHistorialAprobacionAsync(HistorialAprobacion historial)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO AUT_HISTORIAL_APROBACION (SolicitudId, Usuario, Accion, Fecha, Comentarios)
            VALUES (@SolicitudId, @Usuario, @Accion, GETDATE(), @Comentarios)", historial);
    }

    // ==================== DELEGACIONES ====================

    public async Task<IEnumerable<DelegacionPermiso>> GetDelegacionesActivasAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<DelegacionPermiso>(@"
            SELECT Id, UsuarioOrigen, UsuarioDestino, FechaInicio, FechaFin, Motivo, Activa
            FROM AUT_DELEGACION
            WHERE UsuarioDestino = @UsuarioLogin AND Activa = 1
              AND FechaInicio <= GETDATE() AND FechaFin >= GETDATE()",
            new { UsuarioLogin = usuarioLogin });
    }

    public async Task<IEnumerable<DelegacionPermiso>> GetDelegacionesOtorgadasAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<DelegacionPermiso>(@"
            SELECT Id, UsuarioOrigen, UsuarioDestino, FechaInicio, FechaFin, Motivo, Activa
            FROM AUT_DELEGACION WHERE UsuarioOrigen = @UsuarioLogin ORDER BY FechaInicio DESC",
            new { UsuarioLogin = usuarioLogin });
    }

    public async Task<int> CrearDelegacionAsync(DelegacionPermiso delegacion)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO AUT_DELEGACION (UsuarioOrigen, UsuarioDestino, FechaInicio, FechaFin, Motivo, Activa)
            VALUES (@UsuarioOrigen, @UsuarioDestino, @FechaInicio, @FechaFin, @Motivo, 1);
            SELECT CAST(SCOPE_IDENTITY() as int)", delegacion);
    }

    public async Task CancelarDelegacionAsync(int delegacionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync("UPDATE AUT_DELEGACION SET Activa = 0 WHERE Id = @Id", new { Id = delegacionId });
    }

    public async Task<bool> TieneDelegacionActivaAsync(string usuarioOrigen, string usuarioDestino)
    {
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM AUT_DELEGACION
            WHERE UsuarioOrigen = @UsuarioOrigen AND UsuarioDestino = @UsuarioDestino AND Activa = 1
              AND FechaInicio <= GETDATE() AND FechaFin >= GETDATE()",
            new { UsuarioOrigen = usuarioOrigen, UsuarioDestino = usuarioDestino });
        return count > 0;
    }

    // ==================== ESTADISTICAS ====================

    public async Task<int> ContarSolicitudesPendientesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM AUT_SOLICITUD_APROBACION WHERE Estado = 'Pendiente'");
    }

    public async Task<int> ContarDelegacionesActivasAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM AUT_DELEGACION
            WHERE Activa = 1 AND FechaInicio <= GETDATE() AND FechaFin >= GETDATE()");
    }
}
