using Dapper;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;

namespace Factoring.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AuthRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // ==================== REFRESH TOKENS ====================

    public async Task<int> GuardarRefreshTokenAsync(RefreshToken token)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO AUTH_REFRESH_TOKEN
                (Token, UsuarioLogin, FechaCreacion, FechaExpiracion, Revocado, DireccionIp, Dispositivo)
            VALUES
                (@Token, @UsuarioLogin, @FechaCreacion, @FechaExpiracion, @Revocado, @DireccionIp, @Dispositivo);
            SELECT CAST(SCOPE_IDENTITY() as int)";

        return await connection.ExecuteScalarAsync<int>(sql, token);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT Id, Token, UsuarioLogin, FechaCreacion, FechaExpiracion,
                   Revocado, FechaRevocacion, TokenReemplazo, DireccionIp, Dispositivo
            FROM AUTH_REFRESH_TOKEN
            WHERE Token = @Token";

        return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
    }

    public async Task RevocarRefreshTokenAsync(string token, string? tokenReemplazo = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE AUTH_REFRESH_TOKEN
            SET Revocado = 1,
                FechaRevocacion = GETDATE(),
                TokenReemplazo = @TokenReemplazo
            WHERE Token = @Token";

        await connection.ExecuteAsync(sql, new { Token = token, TokenReemplazo = tokenReemplazo });
    }

    public async Task RevocarTodosRefreshTokensAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE AUTH_REFRESH_TOKEN
            SET Revocado = 1, FechaRevocacion = GETDATE()
            WHERE UsuarioLogin = @UsuarioLogin AND Revocado = 0";

        await connection.ExecuteAsync(sql, new { UsuarioLogin = usuarioLogin });
    }

    public async Task LimpiarRefreshTokensExpiradosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            DELETE FROM AUTH_REFRESH_TOKEN
            WHERE FechaExpiracion < DATEADD(DAY, -7, GETDATE())";

        await connection.ExecuteAsync(sql);
    }

    // ==================== SESIONES ====================

    public async Task<int> RegistrarSesionAsync(SesionUsuario sesion)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO AUTH_SESION
                (UsuarioLogin, TokenJti, FechaInicio, DireccionIp, Dispositivo, Activa)
            VALUES
                (@UsuarioLogin, @TokenJti, @FechaInicio, @DireccionIp, @Dispositivo, 1);
            SELECT CAST(SCOPE_IDENTITY() as int)";

        return await connection.ExecuteScalarAsync<int>(sql, sesion);
    }

    public async Task<IEnumerable<SesionUsuario>> GetSesionesActivasAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT Id, UsuarioLogin, TokenJti, FechaInicio, FechaFin, DireccionIp, Dispositivo, Activa
            FROM AUTH_SESION
            WHERE UsuarioLogin = @UsuarioLogin AND Activa = 1
            ORDER BY FechaInicio DESC";

        return await connection.QueryAsync<SesionUsuario>(sql, new { UsuarioLogin = usuarioLogin });
    }

    public async Task CerrarSesionAsync(int sesionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE AUTH_SESION
            SET Activa = 0, FechaFin = GETDATE()
            WHERE Id = @SesionId";

        await connection.ExecuteAsync(sql, new { SesionId = sesionId });
    }

    public async Task CerrarOtrasSesionesAsync(string usuarioLogin, string tokenJtiActual)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE AUTH_SESION
            SET Activa = 0, FechaFin = GETDATE()
            WHERE UsuarioLogin = @UsuarioLogin
              AND TokenJti != @TokenJtiActual
              AND Activa = 1";

        await connection.ExecuteAsync(sql, new { UsuarioLogin = usuarioLogin, TokenJtiActual = tokenJtiActual });
    }

    public async Task CerrarTodasSesionesAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE AUTH_SESION
            SET Activa = 0, FechaFin = GETDATE()
            WHERE UsuarioLogin = @UsuarioLogin AND Activa = 1";

        await connection.ExecuteAsync(sql, new { UsuarioLogin = usuarioLogin });
    }

    public async Task<int> ContarSesionesActivasAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(*) FROM AUTH_SESION
            WHERE UsuarioLogin = @UsuarioLogin AND Activa = 1";

        return await connection.ExecuteScalarAsync<int>(sql, new { UsuarioLogin = usuarioLogin });
    }

    // ==================== 2FA ====================

    public async Task<ConfiguracionTwoFactor?> GetConfiguracion2FAAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT UsuarioLogin, Habilitado, SecretoTotp, FechaActivacion, CodigosRecuperacion
            FROM AUTH_2FA
            WHERE UsuarioLogin = @UsuarioLogin";

        return await connection.QueryFirstOrDefaultAsync<ConfiguracionTwoFactor>(sql, new { UsuarioLogin = usuarioLogin });
    }

    public async Task GuardarConfiguracion2FAAsync(ConfiguracionTwoFactor config)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            MERGE AUTH_2FA AS target
            USING (SELECT @UsuarioLogin AS UsuarioLogin) AS source
            ON target.UsuarioLogin = source.UsuarioLogin
            WHEN MATCHED THEN
                UPDATE SET
                    Habilitado = @Habilitado,
                    SecretoTotp = @SecretoTotp,
                    FechaActivacion = @FechaActivacion,
                    CodigosRecuperacion = @CodigosRecuperacion
            WHEN NOT MATCHED THEN
                INSERT (UsuarioLogin, Habilitado, SecretoTotp, FechaActivacion, CodigosRecuperacion)
                VALUES (@UsuarioLogin, @Habilitado, @SecretoTotp, @FechaActivacion, @CodigosRecuperacion);";

        await connection.ExecuteAsync(sql, config);
    }

    public async Task Desactivar2FAAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE AUTH_2FA
            SET Habilitado = 0, SecretoTotp = NULL, FechaActivacion = NULL, CodigosRecuperacion = NULL
            WHERE UsuarioLogin = @UsuarioLogin";

        await connection.ExecuteAsync(sql, new { UsuarioLogin = usuarioLogin });
    }

    public async Task ActualizarCodigosRecuperacionAsync(string usuarioLogin, string codigosJson)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE AUTH_2FA
            SET CodigosRecuperacion = @CodigosJson
            WHERE UsuarioLogin = @UsuarioLogin";

        await connection.ExecuteAsync(sql, new { UsuarioLogin = usuarioLogin, CodigosJson = codigosJson });
    }

    // ==================== INTENTOS DE LOGIN ====================

    public async Task RegistrarIntentoLoginAsync(IntentoLogin intento)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO AUTH_INTENTO_LOGIN
                (UsuarioLogin, FechaIntento, Exitoso, DireccionIp, MotivoFallo)
            VALUES
                (@UsuarioLogin, @FechaIntento, @Exitoso, @DireccionIp, @MotivoFallo)";

        await connection.ExecuteAsync(sql, intento);
    }

    public async Task<int> ContarIntentosFallidosAsync(string usuarioLogin, int minutos)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(*)
            FROM AUTH_INTENTO_LOGIN
            WHERE UsuarioLogin = @UsuarioLogin
              AND Exitoso = 0
              AND FechaIntento > DATEADD(MINUTE, -@Minutos, GETDATE())";

        return await connection.ExecuteScalarAsync<int>(sql, new { UsuarioLogin = usuarioLogin, Minutos = minutos });
    }

    public async Task<DateTime?> GetUltimoIntentoFallidoAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT TOP 1 FechaIntento
            FROM AUTH_INTENTO_LOGIN
            WHERE UsuarioLogin = @UsuarioLogin AND Exitoso = 0
            ORDER BY FechaIntento DESC";

        return await connection.QueryFirstOrDefaultAsync<DateTime?>(sql, new { UsuarioLogin = usuarioLogin });
    }

    public async Task LimpiarIntentosAntiguosAsync(int diasAntiguedad)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            DELETE FROM AUTH_INTENTO_LOGIN
            WHERE FechaIntento < DATEADD(DAY, -@Dias, GETDATE())";

        await connection.ExecuteAsync(sql, new { Dias = diasAntiguedad });
    }

    // ==================== POLITICAS DE PASSWORD ====================

    public async Task<PoliticaPassword> GetPoliticaPasswordAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT TOP 1 Id, LongitudMinima, LongitudMaxima, RequiereMayusculas, RequiereMinusculas,
                   RequiereNumeros, RequiereCaracteresEspeciales, DiasVigencia, HistorialNoRepetir,
                   MaxIntentosAntesBloqueo, MinutosBloqueoTemporal, MaxSesionesConcurrentes, Activa
            FROM AUTH_POLITICA_PASSWORD
            WHERE Activa = 1";

        var politica = await connection.QueryFirstOrDefaultAsync<PoliticaPassword>(sql);

        // Retornar politica por defecto si no existe
        return politica ?? new PoliticaPassword();
    }

    public async Task ActualizarPoliticaPasswordAsync(PoliticaPassword politica)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE AUTH_POLITICA_PASSWORD SET Activa = 0 WHERE Activa = 1;

            INSERT INTO AUTH_POLITICA_PASSWORD
                (LongitudMinima, LongitudMaxima, RequiereMayusculas, RequiereMinusculas,
                 RequiereNumeros, RequiereCaracteresEspeciales, DiasVigencia, HistorialNoRepetir,
                 MaxIntentosAntesBloqueo, MinutosBloqueoTemporal, MaxSesionesConcurrentes, Activa)
            VALUES
                (@LongitudMinima, @LongitudMaxima, @RequiereMayusculas, @RequiereMinusculas,
                 @RequiereNumeros, @RequiereCaracteresEspeciales, @DiasVigencia, @HistorialNoRepetir,
                 @MaxIntentosAntesBloqueo, @MinutosBloqueoTemporal, @MaxSesionesConcurrentes, 1)";

        await connection.ExecuteAsync(sql, politica);
    }

    // ==================== HISTORIAL DE PASSWORD ====================

    public async Task GuardarPasswordEnHistorialAsync(string usuarioLogin, string passwordHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO AUTH_HISTORIAL_PASSWORD
                (UsuarioLogin, PasswordHash, FechaCambio)
            VALUES
                (@UsuarioLogin, @PasswordHash, GETDATE())";

        await connection.ExecuteAsync(sql, new { UsuarioLogin = usuarioLogin, PasswordHash = passwordHash });
    }

    public async Task<bool> PasswordEnHistorialAsync(string usuarioLogin, string passwordHash, int ultimasN)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(*)
            FROM (
                SELECT TOP (@UltimasN) PasswordHash
                FROM AUTH_HISTORIAL_PASSWORD
                WHERE UsuarioLogin = @UsuarioLogin
                ORDER BY FechaCambio DESC
            ) AS Ultimas
            WHERE PasswordHash = @PasswordHash";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { UsuarioLogin = usuarioLogin, PasswordHash = passwordHash, UltimasN = ultimasN });
        return count > 0;
    }

    public async Task LimpiarHistorialAntiguoAsync(string usuarioLogin, int mantenerUltimasN)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            WITH UltimasPasswords AS (
                SELECT TOP (@MantenerN) Id
                FROM AUTH_HISTORIAL_PASSWORD
                WHERE UsuarioLogin = @UsuarioLogin
                ORDER BY FechaCambio DESC
            )
            DELETE FROM AUTH_HISTORIAL_PASSWORD
            WHERE UsuarioLogin = @UsuarioLogin
              AND Id NOT IN (SELECT Id FROM UltimasPasswords)";

        await connection.ExecuteAsync(sql, new { UsuarioLogin = usuarioLogin, MantenerN = mantenerUltimasN });
    }

    // ==================== BLOQUEO TEMPORAL ====================

    public async Task RegistrarBloqueoTemporalAsync(string usuarioLogin, int minutosBloqueo)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            MERGE AUTH_BLOQUEO_TEMPORAL AS target
            USING (SELECT @UsuarioLogin AS UsuarioLogin) AS source
            ON target.UsuarioLogin = source.UsuarioLogin
            WHEN MATCHED THEN
                UPDATE SET FinBloqueo = DATEADD(MINUTE, @MinutosBloqueo, GETDATE())
            WHEN NOT MATCHED THEN
                INSERT (UsuarioLogin, FinBloqueo)
                VALUES (@UsuarioLogin, DATEADD(MINUTE, @MinutosBloqueo, GETDATE()));";

        await connection.ExecuteAsync(sql, new { UsuarioLogin = usuarioLogin, MinutosBloqueo = minutosBloqueo });
    }

    public async Task<(bool Bloqueado, DateTime? FinBloqueo)> VerificarBloqueoTemporalAsync(string usuarioLogin)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT FinBloqueo
            FROM AUTH_BLOQUEO_TEMPORAL
            WHERE UsuarioLogin = @UsuarioLogin AND FinBloqueo > GETDATE()";

        var finBloqueo = await connection.QueryFirstOrDefaultAsync<DateTime?>(sql, new { UsuarioLogin = usuarioLogin });
        return (finBloqueo.HasValue, finBloqueo);
    }

    public async Task LimpiarBloqueosExpiradosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            DELETE FROM AUTH_BLOQUEO_TEMPORAL
            WHERE FinBloqueo < GETDATE()";

        await connection.ExecuteAsync(sql);
    }
}
