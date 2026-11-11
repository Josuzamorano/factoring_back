using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Factoring.Infrastructure.Services;

/// <summary>
/// Servicio para encriptacion de datos sensibles
/// Usa AES-256-GCM para encriptacion simetrica
/// </summary>
public class EncryptionService
{
    private readonly byte[] _masterKey;
    private readonly ILogger<EncryptionService> _logger;

    // Constantes de configuracion
    private const int KeySize = 256; // bits
    private const int NonceSize = 12; // bytes (96 bits para GCM)
    private const int TagSize = 16; // bytes (128 bits para GCM)
    private const int SaltSize = 16; // bytes
    private const int Iterations = 100000; // PBKDF2 iterations

    public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
    {
        _logger = logger;

        // Obtener clave maestra de configuracion
        var masterKeyBase64 = configuration["Security:EncryptionKey"];

        if (string.IsNullOrEmpty(masterKeyBase64))
        {
            // Generar clave si no existe (solo para desarrollo)
            _masterKey = GenerateRandomKey();
            _logger.LogWarning(
                "No se encontro clave de encriptacion en configuracion. " +
                "Usando clave generada (solo para desarrollo). " +
                "Configure 'Security:EncryptionKey' en produccion.");
        }
        else
        {
            _masterKey = Convert.FromBase64String(masterKeyBase64);
        }

        if (_masterKey.Length != KeySize / 8)
        {
            throw new InvalidOperationException(
                $"La clave de encriptacion debe ser de {KeySize} bits ({KeySize / 8} bytes)");
        }
    }

    /// <summary>
    /// Encripta un texto plano
    /// </summary>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        try
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = EncryptBytes(plainBytes);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al encriptar datos");
            throw new CryptographicException("Error al encriptar datos", ex);
        }
    }

    /// <summary>
    /// Desencripta un texto encriptado
    /// </summary>
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            var cipherBytes = Convert.FromBase64String(cipherText);
            var plainBytes = DecryptBytes(cipherBytes);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desencriptar datos");
            throw new CryptographicException("Error al desencriptar datos", ex);
        }
    }

    /// <summary>
    /// Encripta bytes usando AES-GCM
    /// </summary>
    public byte[] EncryptBytes(byte[] plainBytes)
    {
        // Generar nonce aleatorio
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        // Preparar buffers
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        // Encriptar
        using var aes = new AesGcm(_masterKey, TagSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Combinar: nonce + tag + ciphertext
        var result = new byte[NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, result, NonceSize + TagSize, cipherBytes.Length);

        return result;
    }

    /// <summary>
    /// Desencripta bytes usando AES-GCM
    /// </summary>
    public byte[] DecryptBytes(byte[] encryptedBytes)
    {
        if (encryptedBytes.Length < NonceSize + TagSize)
            throw new CryptographicException("Datos encriptados invalidos");

        // Extraer componentes
        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var cipherBytes = new byte[encryptedBytes.Length - NonceSize - TagSize];

        Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(encryptedBytes, NonceSize, tag, 0, TagSize);
        Buffer.BlockCopy(encryptedBytes, NonceSize + TagSize, cipherBytes, 0, cipherBytes.Length);

        // Desencriptar
        var plainBytes = new byte[cipherBytes.Length];
        using var aes = new AesGcm(_masterKey, TagSize);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return plainBytes;
    }

    /// <summary>
    /// Encripta un campo especifico para almacenamiento en BD
    /// Incluye un prefijo para identificar datos encriptados
    /// </summary>
    public string EncryptField(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Prefijo para identificar datos encriptados
        return "ENC:" + Encrypt(value);
    }

    /// <summary>
    /// Desencripta un campo de BD
    /// </summary>
    public string DecryptField(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (!value.StartsWith("ENC:"))
            return value; // No esta encriptado

        return Decrypt(value.Substring(4));
    }

    /// <summary>
    /// Verifica si un valor esta encriptado
    /// </summary>
    public bool IsEncrypted(string value)
    {
        return !string.IsNullOrEmpty(value) && value.StartsWith("ENC:");
    }

    /// <summary>
    /// Genera hash de password usando PBKDF2
    /// </summary>
    public string HashPassword(string password)
    {
        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            32); // 256 bits

        // Combinar: salt + hash
        var result = new byte[SaltSize + 32];
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, result, SaltSize, 32);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Verifica un password contra su hash
    /// </summary>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);
            if (hashBytes.Length != SaltSize + 32)
                return false;

            var salt = new byte[SaltSize];
            var storedHash = new byte[32];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(hashBytes, SaltSize, storedHash, 0, 32);

            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                32);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Genera un token aleatorio seguro
    /// </summary>
    public string GenerateSecureToken(int length = 32)
    {
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Genera una clave AES-256 aleatoria
    /// </summary>
    public static byte[] GenerateRandomKey()
    {
        var key = new byte[KeySize / 8];
        RandomNumberGenerator.Fill(key);
        return key;
    }

    /// <summary>
    /// Genera una clave AES-256 y la retorna en Base64
    /// (util para generar clave inicial de configuracion)
    /// </summary>
    public static string GenerateRandomKeyBase64()
    {
        return Convert.ToBase64String(GenerateRandomKey());
    }

    /// <summary>
    /// Enmascara un valor sensible para logs/display
    /// </summary>
    public static string Mask(string value, int visibleChars = 4)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length <= visibleChars * 2)
            return new string('*', value.Length);

        return value.Substring(0, visibleChars) +
               new string('*', value.Length - visibleChars * 2) +
               value.Substring(value.Length - visibleChars);
    }

    /// <summary>
    /// Enmascara un RUT chileno (muestra solo ultimos 4 digitos)
    /// </summary>
    public static string MaskRut(string rut)
    {
        if (string.IsNullOrEmpty(rut) || rut.Length < 4)
            return rut;

        return new string('*', rut.Length - 4) + rut.Substring(rut.Length - 4);
    }

    /// <summary>
    /// Enmascara un email (muestra solo primeras 2 letras y dominio)
    /// </summary>
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return email;

        var parts = email.Split('@');
        if (parts.Length != 2)
            return Mask(email);

        var user = parts[0];
        var domain = parts[1];

        if (user.Length <= 2)
            return user + "@" + domain;

        return user.Substring(0, 2) + new string('*', user.Length - 2) + "@" + domain;
    }

    /// <summary>
    /// Enmascara un numero de cuenta bancaria
    /// </summary>
    public static string MaskBankAccount(string account)
    {
        if (string.IsNullOrEmpty(account) || account.Length < 4)
            return account;

        return new string('*', account.Length - 4) + account.Substring(account.Length - 4);
    }
}

/// <summary>
/// Extension para configurar el servicio de encriptacion
/// </summary>
public static class EncryptionServiceExtensions
{
    public static IServiceCollection AddEncryptionService(this IServiceCollection services)
    {
        services.AddSingleton<EncryptionService>();
        return services;
    }
}
