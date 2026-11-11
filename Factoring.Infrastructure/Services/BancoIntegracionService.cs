using Factoring.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace Factoring.Infrastructure.Services;

/// <summary>
/// Servicio de integración bancaria para generación de archivos planos
/// Soporta formatos de los principales bancos chilenos:
/// - Banco Estado
/// - Banco de Chile
/// - Banco Santander
/// - BCI
/// </summary>
public class BancoIntegracionService
{
    private readonly ILogger<BancoIntegracionService> _logger;
    private readonly CultureInfo _cultureCl;

    public BancoIntegracionService(ILogger<BancoIntegracionService> logger)
    {
        _logger = logger;
        _cultureCl = new CultureInfo("es-CL");
    }

    #region Tipos de Datos

    /// <summary>
    /// Datos para transferencia bancaria
    /// </summary>
    public class TransferenciaBancaria
    {
        public int Secuencia { get; set; }
        public long RutBeneficiario { get; set; }
        public string DvBeneficiario { get; set; } = string.Empty;
        public string NombreBeneficiario { get; set; } = string.Empty;
        public string CodigoBanco { get; set; } = string.Empty;
        public string TipoCuenta { get; set; } = string.Empty; // CC=Corriente, AH=Ahorro, VIS=Vista
        public string NumeroCuenta { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Glosa { get; set; } = string.Empty;
        public int? NumeroOperacion { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Today;
    }

    /// <summary>
    /// Resultado de generación de archivo
    /// </summary>
    public class ArchivoPlanoResult
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
        public string? NombreArchivo { get; set; }
        public string? Contenido { get; set; }
        public byte[]? ContenidoBytes { get; set; }
        public int TotalRegistros { get; set; }
        public decimal MontoTotal { get; set; }
    }

    /// <summary>
    /// Códigos de bancos chilenos
    /// </summary>
    public static class CodigosBancos
    {
        public const string BANCO_ESTADO = "012";
        public const string BANCO_CHILE = "001";
        public const string BANCO_SANTANDER = "037";
        public const string BCI = "016";
        public const string SCOTIABANK = "014";
        public const string ITAU = "039";
        public const string SECURITY = "049";
        public const string BICE = "028";
        public const string FALABELLA = "051";
        public const string RIPLEY = "053";
        public const string CONSORCIO = "055";
    }

    #endregion

    #region Banco Estado

    /// <summary>
    /// Genera archivo plano para Banco Estado (TEF)
    /// Formato: Posiciones fijas, largo 160 caracteres por línea
    /// </summary>
    public ArchivoPlanoResult GenerarArchivoBancoEstado(
        List<TransferenciaBancaria> transferencias,
        string rutEmpresa,
        string cuentaOrigen)
    {
        var result = new ArchivoPlanoResult();

        try
        {
            _logger.LogInformation(
                "Generando archivo Banco Estado: {Count} transferencias",
                transferencias.Count);

            var sb = new StringBuilder();
            int secuencia = 1;
            decimal montoTotal = 0;

            // Línea de encabezado
            sb.AppendLine(GenerarHeaderBancoEstado(rutEmpresa, cuentaOrigen, transferencias.Count));

            // Líneas de detalle
            foreach (var t in transferencias)
            {
                var linea = GenerarDetalleBancoEstado(t, secuencia++);
                sb.AppendLine(linea);
                montoTotal += t.Monto;
            }

            // Línea de control
            sb.AppendLine(GenerarControlBancoEstado(transferencias.Count, montoTotal));

            result.Exito = true;
            result.Contenido = sb.ToString();
            result.ContenidoBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(sb.ToString());
            result.NombreArchivo = $"TEF_BCOESTADO_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            result.TotalRegistros = transferencias.Count;
            result.MontoTotal = montoTotal;

            _logger.LogInformation(
                "Archivo Banco Estado generado: {Archivo}, {Registros} registros, ${Monto:N0}",
                result.NombreArchivo, result.TotalRegistros, result.MontoTotal);
        }
        catch (Exception ex)
        {
            result.Mensaje = $"Error generando archivo: {ex.Message}";
            _logger.LogError(ex, "Error generando archivo Banco Estado");
        }

        return result;
    }

    private string GenerarHeaderBancoEstado(string rutEmpresa, string cuenta, int totalRegistros)
    {
        // Formato encabezado Banco Estado
        // Pos 1-2: Tipo registro "01"
        // Pos 3-12: RUT empresa (10 dígitos con DV)
        // Pos 13-22: Número cuenta origen
        // Pos 23-30: Fecha proceso YYYYMMDD
        // Pos 31-36: Hora proceso HHMMSS
        // Pos 37-42: Total registros (6 dígitos)
        // Pos 43-160: Espacios

        return string.Format(
            "01{0}{1}{2}{3}{4}",
            rutEmpresa.PadLeft(10, '0'),
            cuenta.PadLeft(10, '0'),
            DateTime.Now.ToString("yyyyMMdd"),
            DateTime.Now.ToString("HHmmss"),
            totalRegistros.ToString().PadLeft(6, '0')
        ).PadRight(160);
    }

    private string GenerarDetalleBancoEstado(TransferenciaBancaria t, int secuencia)
    {
        // Formato detalle Banco Estado
        // Pos 1-2: Tipo registro "02"
        // Pos 3-8: Secuencia
        // Pos 9-18: RUT beneficiario
        // Pos 19-58: Nombre beneficiario (40 chars)
        // Pos 59-61: Código banco
        // Pos 62-63: Tipo cuenta (01=Cte, 02=Vista, 03=Ahorro)
        // Pos 64-83: Número cuenta
        // Pos 84-98: Monto (15 dígitos, sin decimales)
        // Pos 99-148: Glosa (50 chars)
        // Pos 149-160: Espacios

        string tipoCuenta = t.TipoCuenta switch
        {
            "CC" => "01",
            "VIS" => "02",
            "AH" => "03",
            _ => "01"
        };

        var monto = ((long)t.Monto).ToString().PadLeft(15, '0');

        return string.Format(
            "02{0}{1}{2}{3}{4}{5}{6}{7}",
            secuencia.ToString().PadLeft(6, '0'),
            $"{t.RutBeneficiario}-{t.DvBeneficiario}".PadLeft(10, '0'),
            LimpiarTexto(t.NombreBeneficiario, 40).PadRight(40),
            t.CodigoBanco.PadLeft(3, '0'),
            tipoCuenta,
            t.NumeroCuenta.PadLeft(20, '0'),
            monto,
            LimpiarTexto(t.Glosa, 50).PadRight(50)
        ).PadRight(160);
    }

    private string GenerarControlBancoEstado(int totalRegistros, decimal montoTotal)
    {
        // Línea de control
        // Pos 1-2: Tipo registro "99"
        // Pos 3-8: Total registros
        // Pos 9-23: Monto total
        // Pos 24-160: Espacios

        return string.Format(
            "99{0}{1}",
            totalRegistros.ToString().PadLeft(6, '0'),
            ((long)montoTotal).ToString().PadLeft(15, '0')
        ).PadRight(160);
    }

    #endregion

    #region Banco de Chile

    /// <summary>
    /// Genera archivo plano para Banco de Chile (formato CSV)
    /// </summary>
    public ArchivoPlanoResult GenerarArchivoBancoChile(
        List<TransferenciaBancaria> transferencias,
        string rutEmpresa)
    {
        var result = new ArchivoPlanoResult();

        try
        {
            _logger.LogInformation(
                "Generando archivo Banco de Chile: {Count} transferencias",
                transferencias.Count);

            var sb = new StringBuilder();
            decimal montoTotal = 0;

            // Header CSV
            sb.AppendLine("RUT_BENEFICIARIO;NOMBRE;BANCO;TIPO_CUENTA;NUMERO_CUENTA;MONTO;EMAIL;GLOSA");

            foreach (var t in transferencias)
            {
                sb.AppendLine(string.Format(
                    "{0}-{1};{2};{3};{4};{5};{6};{7};{8}",
                    t.RutBeneficiario,
                    t.DvBeneficiario,
                    LimpiarTexto(t.NombreBeneficiario, 40),
                    t.CodigoBanco,
                    t.TipoCuenta,
                    t.NumeroCuenta,
                    (long)t.Monto,
                    t.Email,
                    LimpiarTexto(t.Glosa, 50)
                ));
                montoTotal += t.Monto;
            }

            result.Exito = true;
            result.Contenido = sb.ToString();
            result.ContenidoBytes = Encoding.UTF8.GetBytes(sb.ToString());
            result.NombreArchivo = $"TEF_BCHILE_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            result.TotalRegistros = transferencias.Count;
            result.MontoTotal = montoTotal;

            _logger.LogInformation(
                "Archivo Banco de Chile generado: {Archivo}",
                result.NombreArchivo);
        }
        catch (Exception ex)
        {
            result.Mensaje = $"Error generando archivo: {ex.Message}";
            _logger.LogError(ex, "Error generando archivo Banco de Chile");
        }

        return result;
    }

    #endregion

    #region Banco Santander

    /// <summary>
    /// Genera archivo plano para Banco Santander
    /// Formato: Posiciones fijas, 200 caracteres por línea
    /// </summary>
    public ArchivoPlanoResult GenerarArchivoSantander(
        List<TransferenciaBancaria> transferencias,
        string rutEmpresa,
        string cuentaOrigen)
    {
        var result = new ArchivoPlanoResult();

        try
        {
            _logger.LogInformation(
                "Generando archivo Santander: {Count} transferencias",
                transferencias.Count);

            var sb = new StringBuilder();
            int secuencia = 1;
            decimal montoTotal = 0;

            // Header
            sb.AppendLine(string.Format(
                "H{0}{1}{2}{3}",
                rutEmpresa.Replace("-", "").PadLeft(10, '0'),
                cuentaOrigen.PadLeft(12, '0'),
                DateTime.Now.ToString("yyyyMMdd"),
                transferencias.Count.ToString().PadLeft(6, '0')
            ).PadRight(200));

            // Detalles
            foreach (var t in transferencias)
            {
                string tipoCuenta = t.TipoCuenta switch
                {
                    "CC" => "1",
                    "AH" => "2",
                    "VIS" => "3",
                    _ => "1"
                };

                sb.AppendLine(string.Format(
                    "D{0}{1}{2}{3}{4}{5}{6}{7}",
                    secuencia++.ToString().PadLeft(6, '0'),
                    $"{t.RutBeneficiario}{t.DvBeneficiario}".PadLeft(10, '0'),
                    LimpiarTexto(t.NombreBeneficiario, 50).PadRight(50),
                    t.CodigoBanco.PadLeft(3, '0'),
                    tipoCuenta,
                    t.NumeroCuenta.PadLeft(20, '0'),
                    ((long)t.Monto).ToString().PadLeft(15, '0'),
                    LimpiarTexto(t.Glosa, 80).PadRight(80)
                ).PadRight(200));

                montoTotal += t.Monto;
            }

            // Control
            sb.AppendLine(string.Format(
                "T{0}{1}",
                transferencias.Count.ToString().PadLeft(6, '0'),
                ((long)montoTotal).ToString().PadLeft(18, '0')
            ).PadRight(200));

            result.Exito = true;
            result.Contenido = sb.ToString();
            result.ContenidoBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(sb.ToString());
            result.NombreArchivo = $"TEF_SANTANDER_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            result.TotalRegistros = transferencias.Count;
            result.MontoTotal = montoTotal;

            _logger.LogInformation(
                "Archivo Santander generado: {Archivo}",
                result.NombreArchivo);
        }
        catch (Exception ex)
        {
            result.Mensaje = $"Error generando archivo: {ex.Message}";
            _logger.LogError(ex, "Error generando archivo Santander");
        }

        return result;
    }

    #endregion

    #region BCI

    /// <summary>
    /// Genera archivo plano para BCI
    /// Formato: CSV con campos específicos
    /// </summary>
    public ArchivoPlanoResult GenerarArchivoBCI(
        List<TransferenciaBancaria> transferencias,
        string rutEmpresa)
    {
        var result = new ArchivoPlanoResult();

        try
        {
            _logger.LogInformation(
                "Generando archivo BCI: {Count} transferencias",
                transferencias.Count);

            var sb = new StringBuilder();
            decimal montoTotal = 0;

            // BCI usa formato CSV con punto y coma
            sb.AppendLine("TIPO_TRANSACCION;RUT_BENEFICIARIO;DV;NOMBRE;COD_BANCO;TIPO_CUENTA;NUM_CUENTA;MONTO;EMAIL;REFERENCIA");

            foreach (var t in transferencias)
            {
                sb.AppendLine(string.Format(
                    "TEF;{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                    t.RutBeneficiario,
                    t.DvBeneficiario,
                    LimpiarTexto(t.NombreBeneficiario, 40),
                    t.CodigoBanco,
                    t.TipoCuenta,
                    t.NumeroCuenta,
                    (long)t.Monto,
                    t.Email,
                    t.NumeroOperacion?.ToString() ?? ""
                ));
                montoTotal += t.Monto;
            }

            result.Exito = true;
            result.Contenido = sb.ToString();
            result.ContenidoBytes = Encoding.UTF8.GetBytes(sb.ToString());
            result.NombreArchivo = $"TEF_BCI_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            result.TotalRegistros = transferencias.Count;
            result.MontoTotal = montoTotal;

            _logger.LogInformation(
                "Archivo BCI generado: {Archivo}",
                result.NombreArchivo);
        }
        catch (Exception ex)
        {
            result.Mensaje = $"Error generando archivo: {ex.Message}";
            _logger.LogError(ex, "Error generando archivo BCI");
        }

        return result;
    }

    #endregion

    #region Genérico/Multi-Banco

    /// <summary>
    /// Genera archivo según el banco especificado
    /// </summary>
    public ArchivoPlanoResult GenerarArchivo(
        string codigoBanco,
        List<TransferenciaBancaria> transferencias,
        string rutEmpresa,
        string cuentaOrigen)
    {
        return codigoBanco switch
        {
            CodigosBancos.BANCO_ESTADO => GenerarArchivoBancoEstado(transferencias, rutEmpresa, cuentaOrigen),
            CodigosBancos.BANCO_CHILE => GenerarArchivoBancoChile(transferencias, rutEmpresa),
            CodigosBancos.BANCO_SANTANDER => GenerarArchivoSantander(transferencias, rutEmpresa, cuentaOrigen),
            CodigosBancos.BCI => GenerarArchivoBCI(transferencias, rutEmpresa),
            _ => new ArchivoPlanoResult
            {
                Exito = false,
                Mensaje = $"Formato de banco {codigoBanco} no soportado"
            }
        };
    }

    /// <summary>
    /// Obtiene nombre del banco por código
    /// </summary>
    public string ObtenerNombreBanco(string codigo)
    {
        return codigo switch
        {
            CodigosBancos.BANCO_ESTADO => "Banco Estado",
            CodigosBancos.BANCO_CHILE => "Banco de Chile",
            CodigosBancos.BANCO_SANTANDER => "Banco Santander",
            CodigosBancos.BCI => "BCI",
            CodigosBancos.SCOTIABANK => "Scotiabank",
            CodigosBancos.ITAU => "Banco Itaú",
            CodigosBancos.SECURITY => "Banco Security",
            CodigosBancos.BICE => "Banco BICE",
            CodigosBancos.FALABELLA => "Banco Falabella",
            CodigosBancos.RIPLEY => "Banco Ripley",
            CodigosBancos.CONSORCIO => "Banco Consorcio",
            _ => $"Banco {codigo}"
        };
    }

    /// <summary>
    /// Lista de bancos soportados
    /// </summary>
    public List<(string Codigo, string Nombre, bool Soportado)> ObtenerBancosSoportados()
    {
        return new List<(string, string, bool)>
        {
            (CodigosBancos.BANCO_ESTADO, "Banco Estado", true),
            (CodigosBancos.BANCO_CHILE, "Banco de Chile", true),
            (CodigosBancos.BANCO_SANTANDER, "Banco Santander", true),
            (CodigosBancos.BCI, "BCI", true),
            (CodigosBancos.SCOTIABANK, "Scotiabank", false),
            (CodigosBancos.ITAU, "Banco Itaú", false),
            (CodigosBancos.SECURITY, "Banco Security", false),
            (CodigosBancos.BICE, "Banco BICE", false)
        };
    }

    #endregion

    #region Lectura de Cartolas

    /// <summary>
    /// Resultado de lectura de cartola bancaria
    /// </summary>
    public class MovimientoCartola
    {
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public decimal Cargo { get; set; }
        public decimal Abono { get; set; }
        public decimal Saldo { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty;
    }

    /// <summary>
    /// Parsea una cartola bancaria en formato estándar
    /// </summary>
    public List<MovimientoCartola> LeerCartolaEstandar(string contenido, string separador = ";")
    {
        var movimientos = new List<MovimientoCartola>();

        try
        {
            var lineas = contenido.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            bool headerPasado = false;

            foreach (var linea in lineas)
            {
                if (!headerPasado)
                {
                    headerPasado = true;
                    continue;
                }

                var campos = linea.Split(separador);
                if (campos.Length < 5) continue;

                var mov = new MovimientoCartola
                {
                    Fecha = ParsearFecha(campos[0].Trim()),
                    Descripcion = campos[1].Trim(),
                    NumeroDocumento = campos.Length > 2 ? campos[2].Trim() : "",
                    Cargo = ParsearMonto(campos.Length > 3 ? campos[3] : "0"),
                    Abono = ParsearMonto(campos.Length > 4 ? campos[4] : "0"),
                    Saldo = ParsearMonto(campos.Length > 5 ? campos[5] : "0")
                };

                mov.TipoMovimiento = mov.Cargo > 0 ? "CARGO" : "ABONO";
                movimientos.Add(mov);
            }

            _logger.LogInformation("Cartola leída: {Count} movimientos", movimientos.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leyendo cartola");
        }

        return movimientos;
    }

    private DateTime ParsearFecha(string fecha)
    {
        string[] formatos = { "dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "yyyyMMdd" };
        if (DateTime.TryParseExact(fecha, formatos, _cultureCl, DateTimeStyles.None, out var result))
        {
            return result;
        }
        return DateTime.Today;
    }

    private decimal ParsearMonto(string monto)
    {
        if (string.IsNullOrWhiteSpace(monto)) return 0;

        // Remover caracteres no numéricos excepto punto/coma decimal
        var limpio = monto.Replace("$", "").Replace(" ", "").Trim();

        // Formato chileno: puntos como separador de miles, coma como decimal
        limpio = limpio.Replace(".", "").Replace(",", ".");

        if (decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        return 0;
    }

    #endregion

    #region Utilidades

    private string LimpiarTexto(string texto, int maxLength)
    {
        if (string.IsNullOrEmpty(texto)) return "";

        // Remover caracteres especiales
        var limpio = texto
            .Replace("Ñ", "N")
            .Replace("ñ", "n")
            .Replace("á", "a").Replace("Á", "A")
            .Replace("é", "e").Replace("É", "E")
            .Replace("í", "i").Replace("Í", "I")
            .Replace("ó", "o").Replace("Ó", "O")
            .Replace("ú", "u").Replace("Ú", "U")
            .Replace(";", " ")
            .Replace(",", " ")
            .Replace("\"", " ");

        if (limpio.Length > maxLength)
        {
            limpio = limpio.Substring(0, maxLength);
        }

        return limpio;
    }

    #endregion
}
