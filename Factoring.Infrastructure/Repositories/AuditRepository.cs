using Dapper;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;
using System.Text;

namespace Factoring.Infrastructure.Repositories;

/// <summary>
/// Repositorio para operaciones de auditoria
/// </summary>
public class AuditRepository : IAuditRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AuditRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // Columnas base para select
    private const string SelectColumns = @"
        AUD_ID as AudId,
        AUD_ENTIDAD as AudEntidad,
        AUD_ENTIDAD_ID as AudEntidadId,
        AUD_ACCION as AudAccion,
        AUD_USUARIO as AudUsuario,
        AUD_FECHA_HORA as AudFechaHora,
        AUD_IP as AudIp,
        AUD_MAQUINA as AudMaquina,
        AUD_VALORES_ANTERIORES as AudValoresAnteriores,
        AUD_VALORES_NUEVOS as AudValoresNuevos,
        AUD_DESCRIPCION as AudDescripcion,
        AUD_CORRELATION_ID as AudCorrelationId,
        AUD_MODULO as AudModulo,
        AUD_NIVEL_CRITICIDAD as AudNivelCriticidad";

    public async Task RegistrarAsync(AuditLog audit)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO AUD_LOG (
                AUD_ENTIDAD, AUD_ENTIDAD_ID, AUD_ACCION, AUD_USUARIO,
                AUD_FECHA_HORA, AUD_IP, AUD_MAQUINA, AUD_VALORES_ANTERIORES,
                AUD_VALORES_NUEVOS, AUD_DESCRIPCION, AUD_CORRELATION_ID,
                AUD_MODULO, AUD_NIVEL_CRITICIDAD
            ) VALUES (
                @AudEntidad, @AudEntidadId, @AudAccion, @AudUsuario,
                @AudFechaHora, @AudIp, @AudMaquina, @AudValoresAnteriores,
                @AudValoresNuevos, @AudDescripcion, @AudCorrelationId,
                @AudModulo, @AudNivelCriticidad
            )";

        await connection.ExecuteAsync(sql, audit);
    }

    public async Task<IEnumerable<AuditLog>> GetByEntidadAsync(string entidad, string entidadId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                AUD_ID as AudId,
                AUD_ENTIDAD as AudEntidad,
                AUD_ENTIDAD_ID as AudEntidadId,
                AUD_ACCION as AudAccion,
                AUD_USUARIO as AudUsuario,
                AUD_FECHA_HORA as AudFechaHora,
                AUD_IP as AudIp,
                AUD_MAQUINA as AudMaquina,
                AUD_VALORES_ANTERIORES as AudValoresAnteriores,
                AUD_VALORES_NUEVOS as AudValoresNuevos,
                AUD_DESCRIPCION as AudDescripcion,
                AUD_CORRELATION_ID as AudCorrelationId,
                AUD_MODULO as AudModulo,
                AUD_NIVEL_CRITICIDAD as AudNivelCriticidad
            FROM AUD_LOG
            WHERE AUD_ENTIDAD = @Entidad AND AUD_ENTIDAD_ID = @EntidadId
            ORDER BY AUD_FECHA_HORA DESC";

        return await connection.QueryAsync<AuditLog>(sql, new { Entidad = entidad, EntidadId = entidadId });
    }

    public async Task<IEnumerable<AuditLog>> GetByUsuarioAsync(string usuario, DateTime? desde = null, DateTime? hasta = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT
                AUD_ID as AudId,
                AUD_ENTIDAD as AudEntidad,
                AUD_ENTIDAD_ID as AudEntidadId,
                AUD_ACCION as AudAccion,
                AUD_USUARIO as AudUsuario,
                AUD_FECHA_HORA as AudFechaHora,
                AUD_IP as AudIp,
                AUD_MAQUINA as AudMaquina,
                AUD_VALORES_ANTERIORES as AudValoresAnteriores,
                AUD_VALORES_NUEVOS as AudValoresNuevos,
                AUD_DESCRIPCION as AudDescripcion,
                AUD_CORRELATION_ID as AudCorrelationId,
                AUD_MODULO as AudModulo,
                AUD_NIVEL_CRITICIDAD as AudNivelCriticidad
            FROM AUD_LOG
            WHERE AUD_USUARIO = @Usuario";

        if (desde.HasValue)
            sql += " AND AUD_FECHA_HORA >= @Desde";
        if (hasta.HasValue)
            sql += " AND AUD_FECHA_HORA <= @Hasta";

        sql += " ORDER BY AUD_FECHA_HORA DESC";

        return await connection.QueryAsync<AuditLog>(sql, new { Usuario = usuario, Desde = desde, Hasta = hasta });
    }

    public async Task<IEnumerable<AuditLog>> GetByModuloAsync(string modulo, DateTime? desde = null, DateTime? hasta = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT
                AUD_ID as AudId,
                AUD_ENTIDAD as AudEntidad,
                AUD_ENTIDAD_ID as AudEntidadId,
                AUD_ACCION as AudAccion,
                AUD_USUARIO as AudUsuario,
                AUD_FECHA_HORA as AudFechaHora,
                AUD_IP as AudIp,
                AUD_MAQUINA as AudMaquina,
                AUD_VALORES_ANTERIORES as AudValoresAnteriores,
                AUD_VALORES_NUEVOS as AudValoresNuevos,
                AUD_DESCRIPCION as AudDescripcion,
                AUD_CORRELATION_ID as AudCorrelationId,
                AUD_MODULO as AudModulo,
                AUD_NIVEL_CRITICIDAD as AudNivelCriticidad
            FROM AUD_LOG
            WHERE AUD_MODULO = @Modulo";

        if (desde.HasValue)
            sql += " AND AUD_FECHA_HORA >= @Desde";
        if (hasta.HasValue)
            sql += " AND AUD_FECHA_HORA <= @Hasta";

        sql += " ORDER BY AUD_FECHA_HORA DESC";

        return await connection.QueryAsync<AuditLog>(sql, new { Modulo = modulo, Desde = desde, Hasta = hasta });
    }

    public async Task<IEnumerable<AuditLog>> GetCriticosAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT
                AUD_ID as AudId,
                AUD_ENTIDAD as AudEntidad,
                AUD_ENTIDAD_ID as AudEntidadId,
                AUD_ACCION as AudAccion,
                AUD_USUARIO as AudUsuario,
                AUD_FECHA_HORA as AudFechaHora,
                AUD_IP as AudIp,
                AUD_MAQUINA as AudMaquina,
                AUD_VALORES_ANTERIORES as AudValoresAnteriores,
                AUD_VALORES_NUEVOS as AudValoresNuevos,
                AUD_DESCRIPCION as AudDescripcion,
                AUD_CORRELATION_ID as AudCorrelationId,
                AUD_MODULO as AudModulo,
                AUD_NIVEL_CRITICIDAD as AudNivelCriticidad
            FROM AUD_LOG
            WHERE AUD_NIVEL_CRITICIDAD IN ('HIGH', 'CRITICAL')";

        if (desde.HasValue)
            sql += " AND AUD_FECHA_HORA >= @Desde";
        if (hasta.HasValue)
            sql += " AND AUD_FECHA_HORA <= @Hasta";

        sql += " ORDER BY AUD_FECHA_HORA DESC";

        return await connection.QueryAsync<AuditLog>(sql, new { Desde = desde, Hasta = hasta });
    }

    public async Task<IEnumerable<AuditLog>> BuscarAsync(
        string? entidad = null,
        string? usuario = null,
        string? modulo = null,
        string? accion = null,
        string? nivelCriticidad = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        int limite = 100)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT TOP (@Limite)
                AUD_ID as AudId,
                AUD_ENTIDAD as AudEntidad,
                AUD_ENTIDAD_ID as AudEntidadId,
                AUD_ACCION as AudAccion,
                AUD_USUARIO as AudUsuario,
                AUD_FECHA_HORA as AudFechaHora,
                AUD_IP as AudIp,
                AUD_MAQUINA as AudMaquina,
                AUD_VALORES_ANTERIORES as AudValoresAnteriores,
                AUD_VALORES_NUEVOS as AudValoresNuevos,
                AUD_DESCRIPCION as AudDescripcion,
                AUD_CORRELATION_ID as AudCorrelationId,
                AUD_MODULO as AudModulo,
                AUD_NIVEL_CRITICIDAD as AudNivelCriticidad
            FROM AUD_LOG
            WHERE 1=1";

        if (!string.IsNullOrEmpty(entidad))
            sql += " AND AUD_ENTIDAD = @Entidad";
        if (!string.IsNullOrEmpty(usuario))
            sql += " AND AUD_USUARIO = @Usuario";
        if (!string.IsNullOrEmpty(modulo))
            sql += " AND AUD_MODULO = @Modulo";
        if (!string.IsNullOrEmpty(accion))
            sql += " AND AUD_ACCION = @Accion";
        if (!string.IsNullOrEmpty(nivelCriticidad))
            sql += " AND AUD_NIVEL_CRITICIDAD = @NivelCriticidad";
        if (desde.HasValue)
            sql += " AND AUD_FECHA_HORA >= @Desde";
        if (hasta.HasValue)
            sql += " AND AUD_FECHA_HORA <= @Hasta";

        sql += " ORDER BY AUD_FECHA_HORA DESC";

        return await connection.QueryAsync<AuditLog>(sql, new
        {
            Limite = limite,
            Entidad = entidad,
            Usuario = usuario,
            Modulo = modulo,
            Accion = accion,
            NivelCriticidad = nivelCriticidad,
            Desde = desde,
            Hasta = hasta
        });
    }

    // ==================== CONSULTAS PAGINADAS ====================

    public async Task<(IEnumerable<AuditLog> Registros, int Total)> BuscarPaginadoAsync(AuditoriaFiltrosDto filtros)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClause = new StringBuilder("WHERE 1=1");
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(filtros.Entidad))
        {
            whereClause.Append(" AND AUD_ENTIDAD = @Entidad");
            parameters.Add("Entidad", filtros.Entidad);
        }

        if (!string.IsNullOrEmpty(filtros.EntidadId))
        {
            whereClause.Append(" AND AUD_ENTIDAD_ID = @EntidadId");
            parameters.Add("EntidadId", filtros.EntidadId);
        }

        if (!string.IsNullOrEmpty(filtros.Usuario))
        {
            whereClause.Append(" AND AUD_USUARIO LIKE @Usuario");
            parameters.Add("Usuario", $"%{filtros.Usuario}%");
        }

        if (!string.IsNullOrEmpty(filtros.Modulo))
        {
            whereClause.Append(" AND AUD_MODULO = @Modulo");
            parameters.Add("Modulo", filtros.Modulo);
        }

        if (!string.IsNullOrEmpty(filtros.Accion))
        {
            whereClause.Append(" AND AUD_ACCION = @Accion");
            parameters.Add("Accion", filtros.Accion);
        }

        if (!string.IsNullOrEmpty(filtros.NivelCriticidad))
        {
            whereClause.Append(" AND AUD_NIVEL_CRITICIDAD = @NivelCriticidad");
            parameters.Add("NivelCriticidad", filtros.NivelCriticidad);
        }

        if (!string.IsNullOrEmpty(filtros.Ip))
        {
            whereClause.Append(" AND AUD_IP = @Ip");
            parameters.Add("Ip", filtros.Ip);
        }

        if (filtros.FechaDesde.HasValue)
        {
            whereClause.Append(" AND AUD_FECHA_HORA >= @FechaDesde");
            parameters.Add("FechaDesde", filtros.FechaDesde.Value);
        }

        if (filtros.FechaHasta.HasValue)
        {
            whereClause.Append(" AND AUD_FECHA_HORA <= @FechaHasta");
            parameters.Add("FechaHasta", filtros.FechaHasta.Value);
        }

        if (!string.IsNullOrEmpty(filtros.TextoBusqueda))
        {
            whereClause.Append(" AND (AUD_DESCRIPCION LIKE @Texto OR AUD_VALORES_ANTERIORES LIKE @Texto OR AUD_VALORES_NUEVOS LIKE @Texto)");
            parameters.Add("Texto", $"%{filtros.TextoBusqueda}%");
        }

        // Contar total
        var countSql = $"SELECT COUNT(*) FROM AUD_LOG {whereClause}";
        var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // Ordenacion
        var orderColumn = filtros.OrdenarPor switch
        {
            "Usuario" => "AUD_USUARIO",
            "Modulo" => "AUD_MODULO",
            "Accion" => "AUD_ACCION",
            "NivelCriticidad" => "AUD_NIVEL_CRITICIDAD",
            _ => "AUD_FECHA_HORA"
        };
        var orderDir = filtros.Descendente ? "DESC" : "ASC";

        // Paginacion
        var offset = (filtros.Pagina - 1) * filtros.RegistrosPorPagina;
        parameters.Add("Offset", offset);
        parameters.Add("Limit", filtros.RegistrosPorPagina);

        var sql = $@"
            SELECT {SelectColumns}
            FROM AUD_LOG
            {whereClause}
            ORDER BY {orderColumn} {orderDir}
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        var registros = await connection.QueryAsync<AuditLog>(sql, parameters);

        return (registros, total);
    }

    public async Task<AuditLog?> GetByIdAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = $@"
            SELECT {SelectColumns}
            FROM AUD_LOG
            WHERE AUD_ID = @Id";

        return await connection.QueryFirstOrDefaultAsync<AuditLog>(sql, new { Id = id });
    }

    // ==================== ESTADISTICAS ====================

    public async Task<int> ContarRegistrosAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = "SELECT COUNT(*) FROM AUD_LOG WHERE 1=1";
        if (desde.HasValue)
            sql += " AND AUD_FECHA_HORA >= @Desde";
        if (hasta.HasValue)
            sql += " AND AUD_FECHA_HORA <= @Hasta";

        return await connection.ExecuteScalarAsync<int>(sql, new { Desde = desde, Hasta = hasta });
    }

    public async Task<Dictionary<string, int>> ContarPorNivelCriticidadAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT AUD_NIVEL_CRITICIDAD as Nivel, COUNT(*) as Cantidad
            FROM AUD_LOG
            WHERE 1=1";
        if (desde.HasValue)
            sql += " AND AUD_FECHA_HORA >= @Desde";
        if (hasta.HasValue)
            sql += " AND AUD_FECHA_HORA <= @Hasta";
        sql += " GROUP BY AUD_NIVEL_CRITICIDAD";

        var result = await connection.QueryAsync<(string Nivel, int Cantidad)>(sql, new { Desde = desde, Hasta = hasta });
        return result.ToDictionary(x => x.Nivel ?? "SIN_NIVEL", x => x.Cantidad);
    }

    public async Task<Dictionary<string, int>> ContarPorModuloAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT ISNULL(AUD_MODULO, 'SIN_MODULO') as Modulo, COUNT(*) as Cantidad
            FROM AUD_LOG
            WHERE 1=1";
        if (desde.HasValue)
            sql += " AND AUD_FECHA_HORA >= @Desde";
        if (hasta.HasValue)
            sql += " AND AUD_FECHA_HORA <= @Hasta";
        sql += " GROUP BY AUD_MODULO ORDER BY Cantidad DESC";

        var result = await connection.QueryAsync<(string Modulo, int Cantidad)>(sql, new { Desde = desde, Hasta = hasta });
        return result.ToDictionary(x => x.Modulo, x => x.Cantidad);
    }

    public async Task<Dictionary<string, int>> ContarPorAccionAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT AUD_ACCION as Accion, COUNT(*) as Cantidad
            FROM AUD_LOG
            WHERE 1=1";
        if (desde.HasValue)
            sql += " AND AUD_FECHA_HORA >= @Desde";
        if (hasta.HasValue)
            sql += " AND AUD_FECHA_HORA <= @Hasta";
        sql += " GROUP BY AUD_ACCION ORDER BY Cantidad DESC";

        var result = await connection.QueryAsync<(string Accion, int Cantidad)>(sql, new { Desde = desde, Hasta = hasta });
        return result.ToDictionary(x => x.Accion, x => x.Cantidad);
    }

    public async Task<IEnumerable<(string Usuario, int Cantidad, DateTime? UltimaActividad)>> GetUsuariosMasActivosAsync(
        int top = 10, DateTime? desde = null, DateTime? hasta = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = $@"
            SELECT TOP (@Top)
                AUD_USUARIO as Usuario,
                COUNT(*) as Cantidad,
                MAX(AUD_FECHA_HORA) as UltimaActividad
            FROM AUD_LOG
            WHERE 1=1";
        if (desde.HasValue)
            sql += " AND AUD_FECHA_HORA >= @Desde";
        if (hasta.HasValue)
            sql += " AND AUD_FECHA_HORA <= @Hasta";
        sql += " GROUP BY AUD_USUARIO ORDER BY Cantidad DESC";

        return await connection.QueryAsync<(string, int, DateTime?)>(sql, new { Top = top, Desde = desde, Hasta = hasta });
    }

    public async Task<IEnumerable<(DateTime Fecha, int Cantidad, int Criticos)>> GetActividadPorDiaAsync(
        DateTime desde, DateTime hasta)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                CAST(AUD_FECHA_HORA AS DATE) as Fecha,
                COUNT(*) as Cantidad,
                SUM(CASE WHEN AUD_NIVEL_CRITICIDAD IN ('HIGH', 'CRITICAL') THEN 1 ELSE 0 END) as Criticos
            FROM AUD_LOG
            WHERE AUD_FECHA_HORA >= @Desde AND AUD_FECHA_HORA <= @Hasta
            GROUP BY CAST(AUD_FECHA_HORA AS DATE)
            ORDER BY Fecha";

        return await connection.QueryAsync<(DateTime, int, int)>(sql, new { Desde = desde, Hasta = hasta });
    }

    public async Task<Dictionary<int, int>> ContarPorHoraAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT DATEPART(HOUR, AUD_FECHA_HORA) as Hora, COUNT(*) as Cantidad
            FROM AUD_LOG
            WHERE 1=1";
        if (desde.HasValue)
            sql += " AND AUD_FECHA_HORA >= @Desde";
        if (hasta.HasValue)
            sql += " AND AUD_FECHA_HORA <= @Hasta";
        sql += " GROUP BY DATEPART(HOUR, AUD_FECHA_HORA) ORDER BY Hora";

        var result = await connection.QueryAsync<(int Hora, int Cantidad)>(sql, new { Desde = desde, Hasta = hasta });
        return result.ToDictionary(x => x.Hora, x => x.Cantidad);
    }

    public async Task<int> ContarIpsUnicasAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = "SELECT COUNT(DISTINCT AUD_IP) FROM AUD_LOG WHERE AUD_IP IS NOT NULL";
        if (desde.HasValue)
            sql += " AND AUD_FECHA_HORA >= @Desde";
        if (hasta.HasValue)
            sql += " AND AUD_FECHA_HORA <= @Hasta";

        return await connection.ExecuteScalarAsync<int>(sql, new { Desde = desde, Hasta = hasta });
    }

    public async Task<int> ContarUsuariosUnicosAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = "SELECT COUNT(DISTINCT AUD_USUARIO) FROM AUD_LOG WHERE 1=1";
        if (desde.HasValue)
            sql += " AND AUD_FECHA_HORA >= @Desde";
        if (hasta.HasValue)
            sql += " AND AUD_FECHA_HORA <= @Hasta";

        return await connection.ExecuteScalarAsync<int>(sql, new { Desde = desde, Hasta = hasta });
    }

    // ==================== RETENCION ====================

    public async Task<ConfiguracionRetencionDto?> GetConfiguracionRetencionAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                CFG_ID as Id,
                CFG_DIAS_RETENCION_GENERAL as DiasRetencionGeneral,
                CFG_DIAS_RETENCION_CRITICOS as DiasRetencionCriticos,
                CFG_DIAS_RETENCION_LOGIN as DiasRetencionLogin,
                CFG_ARCHIVAR_ANTES_ELIMINAR as ArchivarAntesDeEliminar,
                CFG_RUTA_ARCHIVO as RutaArchivo,
                CFG_ACTIVO as Activo,
                CFG_ULTIMA_LIMPIEZA as UltimaLimpieza,
                CFG_REGISTROS_ELIMINADOS as RegistrosEliminados
            FROM AUD_CONFIGURACION_RETENCION
            WHERE CFG_ACTIVO = 1";

        return await connection.QueryFirstOrDefaultAsync<ConfiguracionRetencionDto>(sql);
    }

    public async Task ActualizarConfiguracionRetencionAsync(ConfiguracionRetencionDto config)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE AUD_CONFIGURACION_RETENCION SET
                CFG_DIAS_RETENCION_GENERAL = @DiasRetencionGeneral,
                CFG_DIAS_RETENCION_CRITICOS = @DiasRetencionCriticos,
                CFG_DIAS_RETENCION_LOGIN = @DiasRetencionLogin,
                CFG_ARCHIVAR_ANTES_ELIMINAR = @ArchivarAntesDeEliminar,
                CFG_RUTA_ARCHIVO = @RutaArchivo
            WHERE CFG_ID = @Id";

        await connection.ExecuteAsync(sql, config);
    }

    public async Task<int> LimpiarRegistrosAntiguosAsync(int diasRetencion, string? nivelExcluir = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var fechaLimite = DateTime.Now.AddDays(-diasRetencion);
        var sql = "DELETE FROM AUD_LOG WHERE AUD_FECHA_HORA < @FechaLimite";

        if (!string.IsNullOrEmpty(nivelExcluir))
            sql += " AND AUD_NIVEL_CRITICIDAD <> @NivelExcluir";

        var eliminados = await connection.ExecuteAsync(sql, new { FechaLimite = fechaLimite, NivelExcluir = nivelExcluir });

        // Actualizar configuracion
        await connection.ExecuteAsync(@"
            UPDATE AUD_CONFIGURACION_RETENCION SET
                CFG_ULTIMA_LIMPIEZA = GETDATE(),
                CFG_REGISTROS_ELIMINADOS = ISNULL(CFG_REGISTROS_ELIMINADOS, 0) + @Eliminados
            WHERE CFG_ACTIVO = 1", new { Eliminados = eliminados });

        return eliminados;
    }

    public async Task<int> ArchivarRegistrosAsync(DateTime hasta, string rutaArchivo)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Primero contamos los registros a archivar
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM AUD_LOG WHERE AUD_FECHA_HORA <= @Hasta",
            new { Hasta = hasta });

        if (count > 0)
        {
            // Insertamos en tabla de archivo
            const string sqlArchivo = @"
                INSERT INTO AUD_LOG_ARCHIVO (
                    AUD_ID_ORIGINAL, AUD_ENTIDAD, AUD_ENTIDAD_ID, AUD_ACCION, AUD_USUARIO,
                    AUD_FECHA_HORA, AUD_IP, AUD_MAQUINA, AUD_VALORES_ANTERIORES,
                    AUD_VALORES_NUEVOS, AUD_DESCRIPCION, AUD_CORRELATION_ID,
                    AUD_MODULO, AUD_NIVEL_CRITICIDAD, AUD_FECHA_ARCHIVO
                )
                SELECT
                    AUD_ID, AUD_ENTIDAD, AUD_ENTIDAD_ID, AUD_ACCION, AUD_USUARIO,
                    AUD_FECHA_HORA, AUD_IP, AUD_MAQUINA, AUD_VALORES_ANTERIORES,
                    AUD_VALORES_NUEVOS, AUD_DESCRIPCION, AUD_CORRELATION_ID,
                    AUD_MODULO, AUD_NIVEL_CRITICIDAD, GETDATE()
                FROM AUD_LOG
                WHERE AUD_FECHA_HORA <= @Hasta";

            await connection.ExecuteAsync(sqlArchivo, new { Hasta = hasta });
        }

        return count;
    }

    // ==================== ALERTAS ====================

    public async Task<int> ContarFallosLoginAsync(string? usuario, string? ip, int minutos)
    {
        using var connection = _connectionFactory.CreateConnection();

        var fechaLimite = DateTime.Now.AddMinutes(-minutos);
        var sql = @"
            SELECT COUNT(*) FROM AUD_LOG
            WHERE AUD_ACCION = 'LOGIN_FAILED'
            AND AUD_FECHA_HORA >= @FechaLimite";

        if (!string.IsNullOrEmpty(usuario))
            sql += " AND AUD_USUARIO = @Usuario";
        if (!string.IsNullOrEmpty(ip))
            sql += " AND AUD_IP = @Ip";

        return await connection.ExecuteScalarAsync<int>(sql, new { FechaLimite = fechaLimite, Usuario = usuario, Ip = ip });
    }

    public async Task<IEnumerable<AuditLog>> GetActividadInusualAsync(string usuario, DateTime desde)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Buscar actividades fuera de horario normal (antes 7am o despues 9pm)
        // O desde IPs diferentes a las habituales
        var sql = $@"
            SELECT {SelectColumns}
            FROM AUD_LOG
            WHERE AUD_USUARIO = @Usuario
            AND AUD_FECHA_HORA >= @Desde
            AND (
                DATEPART(HOUR, AUD_FECHA_HORA) < 7
                OR DATEPART(HOUR, AUD_FECHA_HORA) > 21
                OR AUD_IP NOT IN (
                    SELECT TOP 3 AUD_IP FROM AUD_LOG
                    WHERE AUD_USUARIO = @Usuario AND AUD_IP IS NOT NULL
                    GROUP BY AUD_IP ORDER BY COUNT(*) DESC
                )
            )
            ORDER BY AUD_FECHA_HORA DESC";

        return await connection.QueryAsync<AuditLog>(sql, new { Usuario = usuario, Desde = desde });
    }
}
