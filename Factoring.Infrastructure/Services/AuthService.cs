using BCrypt.Net;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Factoring.Infrastructure.Services;

public class AuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IBitacoraRepository _bitacoraRepository;
    private readonly IConfiguration _configuration;

    // Constantes migradas de VB6 (IntLoginErr, IntDiasCambio)
    private readonly int _maxIntentosLogin;
    private readonly int _diasCambioPassword;
    private readonly int _horasExpiracionToken;
    private readonly int _diasExpiracionRefreshToken;

    // Cache simple para refresh tokens (en produccion usar Redis o BD)
    private static readonly Dictionary<string, (string Usuario, DateTime Expiracion)> _refreshTokens = new();

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IBitacoraRepository bitacoraRepository,
        IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _bitacoraRepository = bitacoraRepository;
        _configuration = configuration;
        _maxIntentosLogin = configuration.GetValue<int>("Auth:MaxIntentosLogin", 3);
        _diasCambioPassword = configuration.GetValue<int>("Auth:DiasCambioPassword", 90);
        _horasExpiracionToken = configuration.GetValue<int>("Auth:HorasExpiracionToken", 8);
        _diasExpiracionRefreshToken = configuration.GetValue<int>("Auth:DiasExpiracionRefreshToken", 30);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string ip, string maquina)
    {
        var response = new LoginResponse();
        
        // Verificar que se ingresó usuario y password
        if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Password))
        {
            response.Mensaje = "Debe ingresar usuario y contraseña";
            return response;
        }

        // Buscar usuario
        var usuario = await _usuarioRepository.GetByLoginAsync(request.Usuario);
        if (usuario == null)
        {
            await RegistrarAccesoFallidoAsync(request.Usuario, maquina, ip, "Usuario no existe");
            response.Mensaje = "Acceso denegado";
            return response;
        }

        // Verificar estado de password (lógica migrada de VB6)
        // USR_EST_PAS: 0=Normal, 1=Admin solicita cambio, 2=Primer acceso, 3=Bloqueado
        switch (usuario.UsrEstPas)
        {
            case 1 when !request.CambiarPassword:
                response.Mensaje = "Administrador solicita cambio de clave";
                response.RequiereCambioPassword = true;
                return response;
            
            case 2 when !request.CambiarPassword:
                response.Mensaje = "Este es su primer acceso, debe cambiar la clave";
                response.RequiereCambioPassword = true;
                return response;
            
            case 3:
                response.Mensaje = "Clave bloqueada, contáctese con el Administrador";
                return response;
        }

        // Verificar fecha de expiración
        if (usuario.UsrFecExp.HasValue && usuario.UsrFecExp.Value < DateTime.Today)
        {
            response.Mensaje = "Permiso de acceso ha expirado";
            return response;
        }

        // Verificar días desde último cambio de password
        if (usuario.UsrUltCambio.HasValue)
        {
            var diasDesdeUltimoCambio = (DateTime.Today - usuario.UsrUltCambio.Value).Days;
            if (diasDesdeUltimoCambio > _diasCambioPassword && !request.CambiarPassword)
            {
                response.Mensaje = "Debe cambiar clave, periodo ha concluido";
                response.RequiereCambioPassword = true;
                return response;
            }
        }

        // Validar password con BCrypt
        bool passwordValida = false;
        try
        {
            // Si el hash empieza con $2a$ o $2b$, es BCrypt
            if (usuario.UsrPasUsr?.StartsWith("$2") == true)
            {
                passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.UsrPasUsr);
            }
            else
            {
                // Comparación legacy para passwords sin hash
                passwordValida = usuario.UsrPasUsr == request.Password;
            }
        }
        catch
        {
            passwordValida = false;
        }
        
        if (!passwordValida)
        {
            var intentos = await _usuarioRepository.IncrementarIntentosErroneos(request.Usuario);
            
            if (intentos >= _maxIntentosLogin)
            {
                await _usuarioRepository.BloquearUsuarioAsync(request.Usuario);
                await RegistrarAccesoFallidoAsync(request.Usuario, maquina, ip, "Usuario ha sido bloqueado");
                response.Mensaje = "Usuario ha sido bloqueado";
            }
            else
            {
                await RegistrarAccesoFallidoAsync(request.Usuario, maquina, ip, "Intento de ingresar erróneo");
                response.Mensaje = "Acceso denegado";
            }
            return response;
        }

        // Password correcta - resetear intentos
        await _usuarioRepository.ResetearIntentosAsync(request.Usuario);

        // Si pidió cambio de password, procesarlo
        if (request.CambiarPassword && !string.IsNullOrEmpty(request.NuevoPassword))
        {
            await _usuarioRepository.CambiarPasswordAsync(request.Usuario, request.NuevoPassword);
        }

        // Obtener menú autorizado
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
            BitCodUsr = request.Usuario,
            BitNomMaq = maquina,
            BitEstAdo = 0,
            BitIp = ip,
            BitFecIns = DateTime.Now,
            BitHorIns = DateTime.Now.ToString("HH:mm:ss"),
            BitObservacion = "Login exitoso",
            BitResultado = "OK"
        });

        // Generar token JWT
        var tokenExpiracion = DateTime.Now.AddHours(_horasExpiracionToken);
        var token = GenerarToken(usuario, tokenExpiracion);

        response.Exitoso = true;
        response.Token = token;
        response.TokenExpira = tokenExpiracion;
        response.Usuario = new UsuarioDto
        {
            Login = usuario.UsrLogUsr,
            Nombre = usuario.UsrNomUsr,
            CodigoPerfil = usuario.UsrCodPer,
            CodigoEjecutivo = usuario.UsrCodEje,
            CodigoSucursal = usuario.UsrCodSuc
        };
        response.Menu = menuOpciones;

        // Si RecordarSesion, generar refresh token
        if (request.RecordarSesion)
        {
            var refreshToken = GenerarRefreshToken();
            var refreshExpiracion = DateTime.Now.AddDays(_diasExpiracionRefreshToken);

            // Guardar refresh token
            lock (_refreshTokens)
            {
                // Limpiar tokens expirados del usuario
                var tokensExpirados = _refreshTokens
                    .Where(x => x.Value.Usuario == usuario.UsrLogUsr || x.Value.Expiracion < DateTime.Now)
                    .Select(x => x.Key)
                    .ToList();
                foreach (var key in tokensExpirados)
                {
                    _refreshTokens.Remove(key);
                }

                _refreshTokens[refreshToken] = (usuario.UsrLogUsr, refreshExpiracion);
            }

            response.RefreshToken = refreshToken;
            response.RefreshTokenExpira = refreshExpiracion;
        }

        return response;
    }

    /// <summary>
    /// Renueva el token JWT usando un refresh token valido
    /// </summary>
    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken, string ip, string maquina)
    {
        var response = new LoginResponse();

        // Validar refresh token
        (string Usuario, DateTime Expiracion) tokenData;
        lock (_refreshTokens)
        {
            if (!_refreshTokens.TryGetValue(refreshToken, out tokenData))
            {
                response.Mensaje = "Refresh token invalido";
                return response;
            }

            if (tokenData.Expiracion < DateTime.Now)
            {
                _refreshTokens.Remove(refreshToken);
                response.Mensaje = "Refresh token expirado";
                return response;
            }
        }

        // Obtener usuario
        var usuario = await _usuarioRepository.GetByLoginAsync(tokenData.Usuario);
        if (usuario == null)
        {
            response.Mensaje = "Usuario no encontrado";
            return response;
        }

        // Verificar que el usuario no este bloqueado
        if (usuario.UsrEstPas == 3)
        {
            response.Mensaje = "Usuario bloqueado";
            return response;
        }

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

        // Generar nuevo token JWT
        var tokenExpiracion = DateTime.Now.AddHours(_horasExpiracionToken);
        var token = GenerarToken(usuario, tokenExpiracion);

        // Rotar refresh token (seguridad adicional)
        var nuevoRefreshToken = GenerarRefreshToken();
        var refreshExpiracion = DateTime.Now.AddDays(_diasExpiracionRefreshToken);

        lock (_refreshTokens)
        {
            _refreshTokens.Remove(refreshToken);
            _refreshTokens[nuevoRefreshToken] = (usuario.UsrLogUsr, refreshExpiracion);
        }

        // Registrar en bitacora
        await _bitacoraRepository.RegistrarAccesoAsync(new BitacoraAcceso
        {
            BitCodUsr = usuario.UsrLogUsr,
            BitNomMaq = maquina,
            BitEstAdo = 0,
            BitIp = ip,
            BitFecIns = DateTime.Now,
            BitHorIns = DateTime.Now.ToString("HH:mm:ss"),
            BitObservacion = "Token renovado via refresh token",
            BitResultado = "OK"
        });

        response.Exitoso = true;
        response.Token = token;
        response.TokenExpira = tokenExpiracion;
        response.RefreshToken = nuevoRefreshToken;
        response.RefreshTokenExpira = refreshExpiracion;
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

    /// <summary>
    /// Invalida un refresh token (logout completo)
    /// </summary>
    public void RevocarRefreshToken(string refreshToken)
    {
        lock (_refreshTokens)
        {
            _refreshTokens.Remove(refreshToken);
        }
    }

    public async Task LogoutAsync(string usuario, string maquina)
    {
        await _bitacoraRepository.RegistrarSalidaAsync(usuario, maquina);
    }

    private async Task RegistrarAccesoFallidoAsync(string usuario, string maquina, string ip, string observacion)
    {
        await _bitacoraRepository.RegistrarAccesoAsync(new BitacoraAcceso
        {
            BitCodUsr = usuario.Length > 10 ? usuario.Substring(0, 10) : usuario,
            BitNomMaq = maquina,
            BitEstAdo = 0,
            BitIp = ip,
            BitFecIns = DateTime.Now,
            BitHorIns = DateTime.Now.ToString("HH:mm:ss"),
            BitObservacion = observacion,
            BitResultado = "Acceso denegado"
        });
    }

    private string GenerarToken(Usuario usuario, DateTime expiracion)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "FactoringSystemSecretKey2024!@#$%"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, usuario.UsrLogUsr),
            new Claim(ClaimTypes.GivenName, usuario.UsrNomUsr),
            new Claim("perfil", usuario.UsrCodPer?.ToString() ?? "0"),
            new Claim("ejecutivo", usuario.UsrCodEje?.ToString() ?? "0"),
            new Claim("sucursal", usuario.UsrCodSuc?.ToString() ?? "0")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "FactoringSystem",
            audience: _configuration["Jwt:Audience"] ?? "FactoringClients",
            claims: claims,
            expires: expiracion,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerarRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
