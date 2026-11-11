using BCrypt.Net;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OtpNet;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Factoring.Infrastructure.Services;

/// <summary>
/// Servicio de autenticacion avanzada con soporte para:
/// - Refresh tokens persistidos en BD con rotacion
/// - Two-Factor Authentication (TOTP)
/// - Bloqueo temporal por intentos fallidos
/// - Politicas de password configurables
/// - Control de sesiones concurrentes
/// </summary>
public class AuthAdvancedService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IBitacoraRepository _bitacoraRepository;
    private readonly IConfiguration _configuration;

    private readonly int _horasExpiracionToken;
    private readonly int _diasExpiracionRefreshToken;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly byte[] _jwtKey;

    public AuthAdvancedService(
        IUsuarioRepository usuarioRepository,
        IAuthRepository authRepository,
        IBitacoraRepository bitacoraRepository,
        IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _authRepository = authRepository;
        _bitacoraRepository = bitacoraRepository;
        _configuration = configuration;

        _horasExpiracionToken = configuration.GetValue<int>("Auth:HorasExpiracionToken", 8);
        _diasExpiracionRefreshToken = configuration.GetValue<int>("Auth:DiasExpiracionRefreshToken", 30);
        _issuer = configuration["Jwt:Issuer"] ?? "FactoringSystem";
        _audience = configuration["Jwt:Audience"] ?? "FactoringClients";
        _jwtKey = Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "FactoringSystemSecretKey2024!@#$%");
    }

    // ==================== LOGIN ====================

    public async Task<LoginAdvancedResponse> LoginAsync(LoginRequest request, string ip, string dispositivo)
    {
        var response = new LoginAdvancedResponse();
        var politica = await _authRepository.GetPoliticaPasswordAsync();

        // Validaciones basicas
        if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Password))
        {
            response.Mensaje = "Debe ingresar usuario y contrasena";
            return response;
        }

        var usuarioLogin = request.Usuario.ToUpper();

        // Verificar bloqueo temporal
        var (bloqueadoTemp, finBloqueo) = await _authRepository.VerificarBloqueoTemporalAsync(usuarioLogin);
        if (bloqueadoTemp)
        {
            response.Mensaje = "Cuenta bloqueada temporalmente";
            response.BloqueadoTemporalmente = true;
            response.MinutosRestantesBloqueo = (int)Math.Ceiling((finBloqueo!.Value - DateTime.Now).TotalMinutes);
            await RegistrarIntentoAsync(usuarioLogin, ip, false, "Cuenta bloqueada temporalmente");
            return response;
        }

        // Buscar usuario
        var usuario = await _usuarioRepository.GetByLoginAsync(usuarioLogin);
        if (usuario == null)
        {
            await RegistrarIntentoAsync(usuarioLogin, ip, false, "Usuario no existe");
            response.Mensaje = "Acceso denegado";
            response.IntentosRestantes = await CalcularIntentosRestantes(usuarioLogin, politica);
            return response;
        }

        // Verificar bloqueo permanente (USR_EST_PAS = 3)
        if (usuario.UsrEstPas == 3)
        {
            response.Mensaje = "Cuenta bloqueada, contacte al administrador";
            return response;
        }

        // Verificar estado de password
        switch (usuario.UsrEstPas)
        {
            case 1 when !request.CambiarPassword:
                response.Mensaje = "El administrador solicita cambio de clave";
                response.RequiereCambioPassword = true;
                return response;

            case 2 when !request.CambiarPassword:
                response.Mensaje = "Primer acceso, debe cambiar la clave";
                response.RequiereCambioPassword = true;
                return response;
        }

        // Verificar expiracion de cuenta
        if (usuario.UsrFecExp.HasValue && usuario.UsrFecExp.Value < DateTime.Today)
        {
            response.Mensaje = "Permiso de acceso ha expirado";
            return response;
        }

        // Verificar vigencia de password
        if (usuario.UsrUltCambio.HasValue)
        {
            var diasDesdeUltimoCambio = (DateTime.Today - usuario.UsrUltCambio.Value).Days;
            if (diasDesdeUltimoCambio > politica.DiasVigencia && !request.CambiarPassword)
            {
                response.Mensaje = "Password expirada, debe cambiarla";
                response.RequiereCambioPassword = true;
                return response;
            }
        }

        // Validar password
        bool passwordValida = VerificarPassword(request.Password, usuario.UsrPasUsr);
        if (!passwordValida)
        {
            await RegistrarIntentoAsync(usuarioLogin, ip, false, "Password incorrecta");

            // Verificar si debe bloquearse temporalmente
            var intentosFallidos = await _authRepository.ContarIntentosFallidosAsync(usuarioLogin, politica.MinutosBloqueoTemporal);
            if (intentosFallidos >= politica.MaxIntentosAntesBloqueo)
            {
                await _authRepository.RegistrarBloqueoTemporalAsync(usuarioLogin, politica.MinutosBloqueoTemporal);
                response.Mensaje = $"Cuenta bloqueada por {politica.MinutosBloqueoTemporal} minutos";
                response.BloqueadoTemporalmente = true;
                response.MinutosRestantesBloqueo = politica.MinutosBloqueoTemporal;
            }
            else
            {
                response.Mensaje = "Acceso denegado";
                response.IntentosRestantes = politica.MaxIntentosAntesBloqueo - intentosFallidos;
            }
            return response;
        }

        // Password correcta - verificar si requiere cambio
        if (request.CambiarPassword && !string.IsNullOrEmpty(request.NuevoPassword))
        {
            var validacionPassword = await ValidarPasswordAsync(request.NuevoPassword, usuarioLogin);
            if (!validacionPassword.EsValida)
            {
                response.Mensaje = string.Join("; ", validacionPassword.Errores);
                return response;
            }

            var hashNuevo = BCrypt.Net.BCrypt.HashPassword(request.NuevoPassword);
            await _usuarioRepository.CambiarPasswordAsync(usuarioLogin, hashNuevo);
            await _authRepository.GuardarPasswordEnHistorialAsync(usuarioLogin, hashNuevo);
        }

        // Resetear intentos erroneos
        await _usuarioRepository.ResetearIntentosAsync(usuarioLogin);

        // Verificar 2FA
        var config2FA = await _authRepository.GetConfiguracion2FAAsync(usuarioLogin);
        if (config2FA?.Habilitado == true)
        {
            // Generar token temporal para 2FA
            var tokenTemporal = GenerarTokenTemporal(usuarioLogin);
            response.Requiere2FA = true;
            response.TokenTemporal2FA = tokenTemporal;
            response.Mensaje = "Se requiere verificacion 2FA";
            await RegistrarIntentoAsync(usuarioLogin, ip, true, "Esperando 2FA");
            return response;
        }

        // Login completo sin 2FA
        return await CompletarLoginAsync(usuario, ip, dispositivo, request.RecordarSesion);
    }

    public async Task<LoginAdvancedResponse> Verificar2FAAsync(Verificar2FARequest request, string ip, string dispositivo)
    {
        var response = new LoginAdvancedResponse();

        // Validar token temporal
        var usuarioLogin = ValidarTokenTemporal(request.TokenTemporal ?? "");
        if (string.IsNullOrEmpty(usuarioLogin))
        {
            response.Mensaje = "Token temporal invalido o expirado";
            return response;
        }

        // Obtener configuracion 2FA
        var config2FA = await _authRepository.GetConfiguracion2FAAsync(usuarioLogin);
        if (config2FA == null || !config2FA.Habilitado)
        {
            response.Mensaje = "2FA no esta configurado";
            return response;
        }

        // Verificar codigo TOTP o codigo de recuperacion
        bool codigoValido = false;

        if (request.Codigo.Length == 6 && request.Codigo.All(char.IsDigit))
        {
            // Es un codigo TOTP
            codigoValido = VerificarCodigoTOTP(config2FA.SecretoTotp!, request.Codigo);
        }
        else
        {
            // Es un codigo de recuperacion
            codigoValido = await VerificarCodigoRecuperacionAsync(usuarioLogin, request.Codigo, config2FA);
        }

        if (!codigoValido)
        {
            await RegistrarIntentoAsync(usuarioLogin, ip, false, "Codigo 2FA incorrecto");
            response.Mensaje = "Codigo incorrecto";
            return response;
        }

        // 2FA exitoso - completar login
        var usuario = await _usuarioRepository.GetByLoginAsync(usuarioLogin);
        if (usuario == null)
        {
            response.Mensaje = "Usuario no encontrado";
            return response;
        }

        return await CompletarLoginAsync(usuario, ip, dispositivo, true);
    }

    private async Task<LoginAdvancedResponse> CompletarLoginAsync(Usuario usuario, string ip, string dispositivo, bool recordarSesion)
    {
        var response = new LoginAdvancedResponse();
        var politica = await _authRepository.GetPoliticaPasswordAsync();

        // Verificar sesiones concurrentes
        var sesionesActivas = await _authRepository.ContarSesionesActivasAsync(usuario.UsrLogUsr);
        if (sesionesActivas >= politica.MaxSesionesConcurrentes)
        {
            // Cerrar sesion mas antigua
            var sesiones = await _authRepository.GetSesionesActivasAsync(usuario.UsrLogUsr);
            var sesionAntigua = sesiones.OrderBy(s => s.FechaInicio).FirstOrDefault();
            if (sesionAntigua != null)
            {
                await _authRepository.CerrarSesionAsync(sesionAntigua.Id);
            }
        }

        // Generar JWT con JTI unico
        var jti = Guid.NewGuid().ToString();
        var tokenExpiracion = DateTime.Now.AddHours(_horasExpiracionToken);
        var token = GenerarToken(usuario, tokenExpiracion, jti);

        // Registrar sesion
        var sesionId = await _authRepository.RegistrarSesionAsync(new SesionUsuario
        {
            UsuarioLogin = usuario.UsrLogUsr,
            TokenJti = jti,
            FechaInicio = DateTime.Now,
            DireccionIp = ip,
            Dispositivo = dispositivo,
            Activa = true
        });

        // Obtener menu
        var menuOpciones = new List<MenuOpcionDto>();
        if (usuario.UsrCodPer.HasValue)
        {
            var opciones = await _usuarioRepository.GetMenuAutorizadoAsync(usuario.UsrCodPer.Value);
            menuOpciones = opciones.Select(o => new MenuOpcionDto
            {
                Codigo = o.PueCodPue,
                Nombre = o.PueNombre,
                Descripcion = o.PueDescripcion,
                Ruta = o.PueRuta,
                Icono = o.PueIcono,
                Orden = o.PueOrden
            }).ToList();
        }

        // Registrar acceso exitoso
        await _bitacoraRepository.RegistrarAccesoAsync(new BitacoraAcceso
        {
            BitCodUsr = usuario.UsrLogUsr,
            BitNomMaq = dispositivo,
            BitEstAdo = 0,
            BitIp = ip,
            BitFecIns = DateTime.Now,
            BitHorIns = DateTime.Now.ToString("HH:mm:ss"),
            BitObservacion = "Login exitoso",
            BitResultado = "OK"
        });

        response.Exitoso = true;
        response.Token = token;
        response.TokenExpira = tokenExpiracion;
        response.SesionId = sesionId;
        response.Usuario = new UsuarioDto
        {
            Login = usuario.UsrLogUsr,
            Nombre = usuario.UsrNomUsr,
            CodigoPerfil = usuario.UsrCodPer,
            CodigoEjecutivo = usuario.UsrCodEje,
            CodigoSucursal = usuario.UsrCodSuc
        };
        response.Menu = menuOpciones;

        // Generar refresh token si se solicita
        if (recordarSesion)
        {
            var refreshToken = GenerarRefreshToken();
            var refreshExpiracion = DateTime.Now.AddDays(_diasExpiracionRefreshToken);

            await _authRepository.GuardarRefreshTokenAsync(new RefreshToken
            {
                Token = refreshToken,
                UsuarioLogin = usuario.UsrLogUsr,
                FechaCreacion = DateTime.Now,
                FechaExpiracion = refreshExpiracion,
                DireccionIp = ip,
                Dispositivo = dispositivo
            });

            response.RefreshToken = refreshToken;
            response.RefreshTokenExpira = refreshExpiracion;
        }

        await RegistrarIntentoAsync(usuario.UsrLogUsr, ip, true, "Login exitoso");
        return response;
    }

    // ==================== REFRESH TOKEN ====================

    public async Task<LoginAdvancedResponse> RefreshTokenAsync(string refreshToken, string ip, string dispositivo)
    {
        var response = new LoginAdvancedResponse();

        // Buscar refresh token en BD
        var tokenData = await _authRepository.GetRefreshTokenAsync(refreshToken);
        if (tokenData == null)
        {
            response.Mensaje = "Refresh token invalido";
            return response;
        }

        if (tokenData.Revocado)
        {
            // Token ya usado - posible ataque de replay
            await _authRepository.RevocarTodosRefreshTokensAsync(tokenData.UsuarioLogin);
            await _authRepository.CerrarTodasSesionesAsync(tokenData.UsuarioLogin);
            response.Mensaje = "Refresh token revocado. Por seguridad, todas las sesiones han sido cerradas.";
            return response;
        }

        if (tokenData.FechaExpiracion < DateTime.Now)
        {
            response.Mensaje = "Refresh token expirado";
            return response;
        }

        // Obtener usuario
        var usuario = await _usuarioRepository.GetByLoginAsync(tokenData.UsuarioLogin);
        if (usuario == null || usuario.UsrEstPas == 3)
        {
            response.Mensaje = "Usuario no disponible";
            return response;
        }

        // Rotar refresh token (revocar el actual y generar nuevo)
        var nuevoRefreshToken = GenerarRefreshToken();
        var refreshExpiracion = DateTime.Now.AddDays(_diasExpiracionRefreshToken);

        await _authRepository.RevocarRefreshTokenAsync(refreshToken, nuevoRefreshToken);
        await _authRepository.GuardarRefreshTokenAsync(new RefreshToken
        {
            Token = nuevoRefreshToken,
            UsuarioLogin = usuario.UsrLogUsr,
            FechaCreacion = DateTime.Now,
            FechaExpiracion = refreshExpiracion,
            DireccionIp = ip,
            Dispositivo = dispositivo
        });

        // Generar nuevo JWT
        var jti = Guid.NewGuid().ToString();
        var tokenExpiracion = DateTime.Now.AddHours(_horasExpiracionToken);
        var token = GenerarToken(usuario, tokenExpiracion, jti);

        // Actualizar sesion
        var sesionId = await _authRepository.RegistrarSesionAsync(new SesionUsuario
        {
            UsuarioLogin = usuario.UsrLogUsr,
            TokenJti = jti,
            FechaInicio = DateTime.Now,
            DireccionIp = ip,
            Dispositivo = dispositivo,
            Activa = true
        });

        // Obtener menu
        var menuOpciones = new List<MenuOpcionDto>();
        if (usuario.UsrCodPer.HasValue)
        {
            var opciones = await _usuarioRepository.GetMenuAutorizadoAsync(usuario.UsrCodPer.Value);
            menuOpciones = opciones.Select(o => new MenuOpcionDto
            {
                Codigo = o.PueCodPue,
                Nombre = o.PueNombre,
                Descripcion = o.PueDescripcion,
                Ruta = o.PueRuta,
                Icono = o.PueIcono,
                Orden = o.PueOrden
            }).ToList();
        }

        response.Exitoso = true;
        response.Token = token;
        response.TokenExpira = tokenExpiracion;
        response.RefreshToken = nuevoRefreshToken;
        response.RefreshTokenExpira = refreshExpiracion;
        response.SesionId = sesionId;
        response.Usuario = new UsuarioDto
        {
            Login = usuario.UsrLogUsr,
            Nombre = usuario.UsrNomUsr,
            CodigoPerfil = usuario.UsrCodPer,
            CodigoEjecutivo = usuario.UsrCodEje,
            CodigoSucursal = usuario.UsrCodSuc
        };
        response.Menu = menuOpciones;

        return response;
    }

    public async Task RevocarRefreshTokenAsync(string refreshToken)
    {
        await _authRepository.RevocarRefreshTokenAsync(refreshToken);
    }

    // ==================== 2FA ====================

    public async Task<Setup2FAResponse> GenerarConfiguracion2FAAsync(string usuarioLogin)
    {
        var response = new Setup2FAResponse();

        // Generar secreto TOTP
        var secreto = KeyGeneration.GenerateRandomKey(20);
        var secretoBase32 = Base32Encoding.ToString(secreto);

        // Generar codigos de recuperacion
        var codigosRecuperacion = Enumerable.Range(0, 10)
            .Select(_ => GenerarCodigoRecuperacion())
            .ToList();

        // Guardar configuracion (aun no habilitada)
        await _authRepository.GuardarConfiguracion2FAAsync(new ConfiguracionTwoFactor
        {
            UsuarioLogin = usuarioLogin,
            Habilitado = false,
            SecretoTotp = secretoBase32,
            CodigosRecuperacion = JsonSerializer.Serialize(codigosRecuperacion)
        });

        // Generar URL para QR
        var usuario = await _usuarioRepository.GetByLoginAsync(usuarioLogin);
        var nombreUsuario = usuario?.UsrNomUsr ?? usuarioLogin;
        var qrUrl = $"otpauth://totp/FactoringSystem:{Uri.EscapeDataString(nombreUsuario)}?secret={secretoBase32}&issuer=FactoringSystem&digits=6";

        response.Exitoso = true;
        response.Secreto = secretoBase32;
        response.QrCodeUrl = qrUrl;
        response.CodigosRecuperacion = codigosRecuperacion;

        return response;
    }

    public async Task<bool> Habilitar2FAAsync(string usuarioLogin, string codigoVerificacion)
    {
        var config = await _authRepository.GetConfiguracion2FAAsync(usuarioLogin);
        if (config == null || string.IsNullOrEmpty(config.SecretoTotp))
        {
            return false;
        }

        // Verificar codigo
        if (!VerificarCodigoTOTP(config.SecretoTotp, codigoVerificacion))
        {
            return false;
        }

        // Activar 2FA
        config.Habilitado = true;
        config.FechaActivacion = DateTime.Now;
        await _authRepository.GuardarConfiguracion2FAAsync(config);

        return true;
    }

    public async Task<bool> Deshabilitar2FAAsync(string usuarioLogin, string password, string? codigoRecuperacion)
    {
        var usuario = await _usuarioRepository.GetByLoginAsync(usuarioLogin);
        if (usuario == null)
        {
            return false;
        }

        // Verificar password
        if (!VerificarPassword(password, usuario.UsrPasUsr))
        {
            return false;
        }

        // Si se proporciono codigo de recuperacion, verificarlo
        if (!string.IsNullOrEmpty(codigoRecuperacion))
        {
            var config = await _authRepository.GetConfiguracion2FAAsync(usuarioLogin);
            if (config == null || !await VerificarCodigoRecuperacionAsync(usuarioLogin, codigoRecuperacion, config))
            {
                return false;
            }
        }

        await _authRepository.Desactivar2FAAsync(usuarioLogin);
        return true;
    }

    public async Task<Estado2FAResponse> GetEstado2FAAsync(string usuarioLogin)
    {
        var config = await _authRepository.GetConfiguracion2FAAsync(usuarioLogin);

        var codigosRestantes = 0;
        if (config?.CodigosRecuperacion != null)
        {
            try
            {
                var codigos = JsonSerializer.Deserialize<List<string>>(config.CodigosRecuperacion);
                codigosRestantes = codigos?.Count ?? 0;
            }
            catch { }
        }

        return new Estado2FAResponse
        {
            Habilitado = config?.Habilitado ?? false,
            FechaActivacion = config?.FechaActivacion,
            CodigosRecuperacionRestantes = codigosRestantes
        };
    }

    // ==================== SESIONES ====================

    public async Task<IEnumerable<SesionActivaDto>> GetSesionesActivasAsync(string usuarioLogin, string jtiActual)
    {
        var sesiones = await _authRepository.GetSesionesActivasAsync(usuarioLogin);

        return sesiones.Select(s => new SesionActivaDto
        {
            Id = s.Id,
            FechaInicio = s.FechaInicio,
            DireccionIp = s.DireccionIp,
            Dispositivo = s.Dispositivo,
            EsSesionActual = s.TokenJti == jtiActual,
            TiempoActiva = FormatearTiempoActivo(s.FechaInicio)
        });
    }

    public async Task CerrarSesionAsync(int sesionId)
    {
        await _authRepository.CerrarSesionAsync(sesionId);
    }

    public async Task CerrarOtrasSesionesAsync(string usuarioLogin, string jtiActual)
    {
        await _authRepository.CerrarOtrasSesionesAsync(usuarioLogin, jtiActual);
        await _authRepository.RevocarTodosRefreshTokensAsync(usuarioLogin);
    }

    public async Task LogoutAsync(string usuarioLogin, string dispositivo, string? refreshToken, string? jti)
    {
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authRepository.RevocarRefreshTokenAsync(refreshToken);
        }

        if (!string.IsNullOrEmpty(jti))
        {
            var sesiones = await _authRepository.GetSesionesActivasAsync(usuarioLogin);
            var sesion = sesiones.FirstOrDefault(s => s.TokenJti == jti);
            if (sesion != null)
            {
                await _authRepository.CerrarSesionAsync(sesion.Id);
            }
        }

        await _bitacoraRepository.RegistrarSalidaAsync(usuarioLogin, dispositivo);
    }

    // ==================== POLITICAS DE PASSWORD ====================

    public async Task<ValidacionPasswordResponse> ValidarPasswordAsync(string password, string? usuarioLogin = null)
    {
        var politica = await _authRepository.GetPoliticaPasswordAsync();
        var response = new ValidacionPasswordResponse { EsValida = true };
        var errores = new List<string>();

        // Longitud
        if (password.Length < politica.LongitudMinima)
        {
            errores.Add($"Debe tener al menos {politica.LongitudMinima} caracteres");
        }
        if (password.Length > politica.LongitudMaxima)
        {
            errores.Add($"No puede exceder {politica.LongitudMaxima} caracteres");
        }

        // Mayusculas
        if (politica.RequiereMayusculas && !Regex.IsMatch(password, @"[A-Z]"))
        {
            errores.Add("Debe contener al menos una mayuscula");
        }

        // Minusculas
        if (politica.RequiereMinusculas && !Regex.IsMatch(password, @"[a-z]"))
        {
            errores.Add("Debe contener al menos una minuscula");
        }

        // Numeros
        if (politica.RequiereNumeros && !Regex.IsMatch(password, @"\d"))
        {
            errores.Add("Debe contener al menos un numero");
        }

        // Caracteres especiales
        if (politica.RequiereCaracteresEspeciales && !Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]"))
        {
            errores.Add("Debe contener al menos un caracter especial");
        }

        // Verificar historial
        if (!string.IsNullOrEmpty(usuarioLogin))
        {
            var hashNuevo = BCrypt.Net.BCrypt.HashPassword(password);
            if (await _authRepository.PasswordEnHistorialAsync(usuarioLogin, hashNuevo, politica.HistorialNoRepetir))
            {
                errores.Add($"No puede reutilizar las ultimas {politica.HistorialNoRepetir} contrasenas");
            }
        }

        response.Errores = errores;
        response.EsValida = errores.Count == 0;
        response.Fortaleza = CalcularFortalezaPassword(password);
        response.NivelFortaleza = response.Fortaleza switch
        {
            >= 80 => "Muy Fuerte",
            >= 60 => "Fuerte",
            >= 40 => "Media",
            _ => "Debil"
        };

        return response;
    }

    public async Task<PoliticaPasswordDto> GetPoliticaPasswordAsync()
    {
        var politica = await _authRepository.GetPoliticaPasswordAsync();
        return new PoliticaPasswordDto
        {
            LongitudMinima = politica.LongitudMinima,
            LongitudMaxima = politica.LongitudMaxima,
            RequiereMayusculas = politica.RequiereMayusculas,
            RequiereMinusculas = politica.RequiereMinusculas,
            RequiereNumeros = politica.RequiereNumeros,
            RequiereCaracteresEspeciales = politica.RequiereCaracteresEspeciales,
            DiasVigencia = politica.DiasVigencia,
            MaxSesionesConcurrentes = politica.MaxSesionesConcurrentes
        };
    }

    public async Task ActualizarPoliticaPasswordAsync(PoliticaPassword politica)
    {
        await _authRepository.ActualizarPoliticaPasswordAsync(politica);
    }

    // ==================== BLOQUEO ====================

    public async Task<EstadoBloqueoDto> GetEstadoBloqueoAsync(string usuarioLogin)
    {
        var politica = await _authRepository.GetPoliticaPasswordAsync();
        var usuario = await _usuarioRepository.GetByLoginAsync(usuarioLogin);
        var (bloqueadoTemp, finBloqueo) = await _authRepository.VerificarBloqueoTemporalAsync(usuarioLogin);
        var intentosFallidos = await _authRepository.ContarIntentosFallidosAsync(usuarioLogin, politica.MinutosBloqueoTemporal);

        return new EstadoBloqueoDto
        {
            Bloqueado = (usuario?.UsrEstPas == 3) || bloqueadoTemp,
            TipoBloqueo = usuario?.UsrEstPas == 3 ? "Permanente" : (bloqueadoTemp ? "Temporal" : ""),
            FinBloqueo = finBloqueo,
            MinutosRestantes = finBloqueo.HasValue ? (int)Math.Ceiling((finBloqueo.Value - DateTime.Now).TotalMinutes) : null,
            IntentosRealizados = intentosFallidos,
            IntentosMaximos = politica.MaxIntentosAntesBloqueo
        };
    }

    public async Task DesbloquearUsuarioAsync(string usuarioLogin)
    {
        await _authRepository.LimpiarBloqueosExpiradosAsync();
        await _usuarioRepository.ResetearIntentosAsync(usuarioLogin);
    }

    // ==================== METODOS PRIVADOS ====================

    private bool VerificarPassword(string password, string hashAlmacenado)
    {
        try
        {
            if (hashAlmacenado.StartsWith("$2"))
            {
                return BCrypt.Net.BCrypt.Verify(password, hashAlmacenado);
            }
            return password == hashAlmacenado;
        }
        catch
        {
            return false;
        }
    }

    private bool VerificarCodigoTOTP(string secretoBase32, string codigo)
    {
        try
        {
            var secreto = Base32Encoding.ToBytes(secretoBase32);
            var totp = new Totp(secreto);
            return totp.VerifyTotp(codigo, out _, new VerificationWindow(previous: 1, future: 1));
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> VerificarCodigoRecuperacionAsync(string usuarioLogin, string codigo, ConfiguracionTwoFactor config)
    {
        if (string.IsNullOrEmpty(config.CodigosRecuperacion))
        {
            return false;
        }

        try
        {
            var codigos = JsonSerializer.Deserialize<List<string>>(config.CodigosRecuperacion);
            if (codigos == null || !codigos.Contains(codigo.ToUpper()))
            {
                return false;
            }

            // Remover codigo usado
            codigos.Remove(codigo.ToUpper());
            await _authRepository.ActualizarCodigosRecuperacionAsync(usuarioLogin, JsonSerializer.Serialize(codigos));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GenerarToken(Usuario usuario, DateTime expiracion, string jti)
    {
        var key = new SymmetricSecurityKey(_jwtKey);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.UsrLogUsr),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(ClaimTypes.Name, usuario.UsrLogUsr),
            new Claim(ClaimTypes.GivenName, usuario.UsrNomUsr),
            new Claim("perfil", usuario.UsrCodPer?.ToString() ?? "0"),
            new Claim("ejecutivo", usuario.UsrCodEje?.ToString() ?? "0"),
            new Claim("sucursal", usuario.UsrCodSuc?.ToString() ?? "0")
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiracion,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerarTokenTemporal(string usuarioLogin)
    {
        var payload = $"{usuarioLogin}|{DateTime.UtcNow.AddMinutes(5).Ticks}";
        var bytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(_jwtKey);
        var signature = hmac.ComputeHash(bytes);
        return $"{Convert.ToBase64String(bytes)}.{Convert.ToBase64String(signature)}";
    }

    private string? ValidarTokenTemporal(string token)
    {
        try
        {
            var partes = token.Split('.');
            if (partes.Length != 2) return null;

            var bytes = Convert.FromBase64String(partes[0]);
            using var hmac = new HMACSHA256(_jwtKey);
            var signature = hmac.ComputeHash(bytes);
            if (Convert.ToBase64String(signature) != partes[1]) return null;

            var payload = Encoding.UTF8.GetString(bytes);
            var partesPayload = payload.Split('|');
            if (partesPayload.Length != 2) return null;

            var expiracion = new DateTime(long.Parse(partesPayload[1]), DateTimeKind.Utc);
            if (expiracion < DateTime.UtcNow) return null;

            return partesPayload[0];
        }
        catch
        {
            return null;
        }
    }

    private static string GenerarRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static string GenerarCodigoRecuperacion()
    {
        var random = new byte[5];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(random);
        return Convert.ToHexString(random).ToUpper();
    }

    private static int CalcularFortalezaPassword(string password)
    {
        int fortaleza = 0;

        // Longitud
        fortaleza += Math.Min(password.Length * 4, 40);

        // Mayusculas
        if (Regex.IsMatch(password, @"[A-Z]")) fortaleza += 15;

        // Minusculas
        if (Regex.IsMatch(password, @"[a-z]")) fortaleza += 15;

        // Numeros
        if (Regex.IsMatch(password, @"\d")) fortaleza += 15;

        // Especiales
        if (Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]")) fortaleza += 15;

        return Math.Min(fortaleza, 100);
    }

    private static string FormatearTiempoActivo(DateTime inicio)
    {
        var duracion = DateTime.Now - inicio;
        if (duracion.TotalMinutes < 60)
            return $"{(int)duracion.TotalMinutes} minutos";
        if (duracion.TotalHours < 24)
            return $"{(int)duracion.TotalHours} horas";
        return $"{(int)duracion.TotalDays} dias";
    }

    private async Task RegistrarIntentoAsync(string usuarioLogin, string ip, bool exitoso, string motivo)
    {
        await _authRepository.RegistrarIntentoLoginAsync(new IntentoLogin
        {
            UsuarioLogin = usuarioLogin,
            FechaIntento = DateTime.Now,
            Exitoso = exitoso,
            DireccionIp = ip,
            MotivoFallo = exitoso ? null : motivo
        });
    }

    private async Task<int> CalcularIntentosRestantes(string usuarioLogin, PoliticaPassword politica)
    {
        var intentosFallidos = await _authRepository.ContarIntentosFallidosAsync(usuarioLogin, politica.MinutosBloqueoTemporal);
        return Math.Max(0, politica.MaxIntentosAntesBloqueo - intentosFallidos);
    }
}
