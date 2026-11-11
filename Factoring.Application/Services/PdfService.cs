// =============================================
// Servicio de Generacion de PDFs
// Liquidaciones, Pagares, Estados de Cuenta
// =============================================

using Dapper;
using Factoring.Core.Interfaces;

namespace Factoring.Application.Services;

/// <summary>
/// Datos para liquidacion de operacion
/// </summary>
public class LiquidacionOperacionDto
{
    // Datos de la operacion
    public int NumeroOperacion { get; set; }
    public DateTime FechaOperacion { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public int PlazoDias { get; set; }
    public string TipoOperacion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;

    // Datos del cliente
    public long RutCliente { get; set; }
    public string DvCliente { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public string DireccionCliente { get; set; } = string.Empty;
    public string ComunaCliente { get; set; } = string.Empty;
    public string TelefonoCliente { get; set; } = string.Empty;

    // Datos de la linea
    public int NumeroLinea { get; set; }
    public decimal MontoLinea { get; set; }
    public decimal CupoDisponible { get; set; }

    // Documentos de la operacion
    public List<DocumentoLiquidacionDto> Documentos { get; set; } = new();

    // Totales
    public decimal TotalDocumentos { get; set; }
    public decimal PorcentajeAnticipo { get; set; }
    public decimal MontoAnticipo { get; set; }
    public decimal DiferenciaPrecio { get; set; }

    // Descuentos
    public decimal TasaInteres { get; set; }
    public decimal MontoIntereses { get; set; }
    public decimal TasaComision { get; set; }
    public decimal MontoComision { get; set; }
    public decimal MontoGastos { get; set; }
    public decimal MontoIVA { get; set; }
    public decimal TotalDescuentos { get; set; }

    // Giro
    public decimal MontoGiro { get; set; }
    public string FormaPago { get; set; } = string.Empty;
    public string BancoDestino { get; set; } = string.Empty;
    public string CuentaDestino { get; set; } = string.Empty;

    // Gastos detallados
    public List<GastoLiquidacionDto> Gastos { get; set; } = new();

    // Ejecutivo
    public string NombreEjecutivo { get; set; } = string.Empty;
}

/// <summary>
/// Documento en la liquidacion
/// </summary>
public class DocumentoLiquidacionDto
{
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string DvDeudor { get; set; } = string.Empty;
    public string NombreDeudor { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public int PlazoDias { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal MontoAnticipado { get; set; }
}

/// <summary>
/// Gasto en la liquidacion
/// </summary>
public class GastoLiquidacionDto
{
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public bool AfectoIVA { get; set; }
}

/// <summary>
/// Datos para pagare
/// </summary>
public class PagareDto
{
    public int NumeroOperacion { get; set; }
    public int NumeroPagare { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string LugarEmision { get; set; } = "Santiago";

    // Deudor principal
    public long RutDeudor { get; set; }
    public string DvDeudor { get; set; } = string.Empty;
    public string NombreDeudor { get; set; } = string.Empty;
    public string DireccionDeudor { get; set; } = string.Empty;
    public string ComunaDeudor { get; set; } = string.Empty;

    // Monto
    public decimal MontoPagare { get; set; }
    public string MontoEnLetras { get; set; } = string.Empty;
    public decimal TasaInteresMora { get; set; }

    // Beneficiario
    public string NombreBeneficiario { get; set; } = string.Empty;
    public string RutBeneficiario { get; set; } = string.Empty;
}

/// <summary>
/// Estado de cuenta del cliente
/// </summary>
public class EstadoCuentaDto
{
    public DateTime FechaEmision { get; set; }
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }

    // Cliente
    public long RutCliente { get; set; }
    public string DvCliente { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public string DireccionCliente { get; set; } = string.Empty;

    // Resumen
    public decimal SaldoAnterior { get; set; }
    public decimal TotalOperaciones { get; set; }
    public decimal TotalPagos { get; set; }
    public decimal SaldoActual { get; set; }
    public decimal ExcedentesDisponibles { get; set; }

    // Movimientos
    public List<MovimientoEstadoCuentaDto> Movimientos { get; set; } = new();

    // Documentos pendientes
    public List<DocumentoPendienteEstadoCuentaDto> DocumentosPendientes { get; set; } = new();
}

public class MovimientoEstadoCuentaDto
{
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cargo { get; set; }
    public decimal Abono { get; set; }
    public decimal Saldo { get; set; }
}

public class DocumentoPendienteEstadoCuentaDto
{
    public int NumeroOperacion { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string NombreDeudor { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
    public int DiasVencido { get; set; }
    public decimal SaldoPendiente { get; set; }
}

/// <summary>
/// Servicio de generacion de PDFs
/// </summary>
public class PdfService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PdfService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Obtiene datos para liquidacion de operacion
    /// </summary>
    public async Task<LiquidacionOperacionDto?> GetLiquidacionOperacionAsync(int numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Obtener datos de la operacion
        var operacion = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT
                O.OPE_NUM_OPE,
                O.OPE_FEC_OPE,
                O.OPE_FEC_VTO,
                DATEDIFF(DAY, O.OPE_FEC_OPE, O.OPE_FEC_VTO) as PlazoDias,
                CASE O.OPE_TIP_OPE WHEN 1 THEN 'Facturas' WHEN 2 THEN 'Cheques' WHEN 3 THEN 'Letras' ELSE 'Otro' END as TipoOperacion,
                CASE O.OPE_EST_OPE WHEN 1 THEN 'Ingresada' WHEN 2 THEN 'Aprobada' WHEN 3 THEN 'Otorgada' WHEN 4 THEN 'Cerrada' ELSE 'Otro' END as Estado,
                O.OPE_RUT_CLI,
                C.CLI_DIG_VER as DvCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                ISNULL(C.CLI_DIR_CLI, '') as DireccionCliente,
                ISNULL(C.CLI_COM_CLI, '') as ComunaCliente,
                ISNULL(C.CLI_TEL_CLI, '') as TelefonoCliente,
                O.OPE_NUM_LIN,
                ISNULL(L.LCR_MON_LIN, 0) as MontoLinea,
                ISNULL(L.LCR_CUP_DIS, 0) as CupoDisponible,
                ISNULL(O.OPE_TOT_DOC, 0) as TotalDocumentos,
                ISNULL(O.OPE_POR_ANT, 80) as PorcentajeAnticipo,
                ISNULL(O.OPE_TOT_ANT, 0) as MontoAnticipo,
                ISNULL(O.OPE_DIF_PRE, 0) as DiferenciaPrecio,
                ISNULL(O.OPE_TAS_INT, 0) as TasaInteres,
                ISNULL(O.OPE_TOT_INT, 0) as MontoIntereses,
                ISNULL(O.OPE_TAS_COM, 0) as TasaComision,
                ISNULL(O.OPE_TOT_COM, 0) as MontoComision,
                ISNULL(O.OPE_TOT_GAS, 0) as MontoGastos,
                ISNULL(O.OPE_TOT_IVA, 0) as MontoIVA,
                ISNULL(O.OPE_TOT_DES, 0) as TotalDescuentos,
                ISNULL(O.OPE_TOT_GIR, 0) as MontoGiro,
                CASE O.OPE_FOR_PAG WHEN 1 THEN 'Transferencia' WHEN 2 THEN 'Cheque' WHEN 3 THEN 'Vale Vista' ELSE 'Transferencia' END as FormaPago,
                ISNULL(B.BAN_NOM_BAN, '') as BancoDestino,
                ISNULL(CB.CLB_NUM_CTA, '') as CuentaDestino,
                ISNULL(U.USR_NOM_USR, 'Sistema') as NombreEjecutivo
            FROM OPE O
            INNER JOIN CLI C ON O.OPE_RUT_CLI = C.CLI_RUT_CLI
            LEFT JOIN LCR L ON O.OPE_NUM_LIN = L.LCR_NUM_LIN
            LEFT JOIN CLB CB ON O.OPE_RUT_CLI = CB.CLB_RUT_CLI AND CB.CLB_CTA_PRI = 1
            LEFT JOIN BAN B ON CB.CLB_COD_BAN = B.BAN_COD_BAN
            LEFT JOIN USR U ON O.OPE_USR_CRE = U.USR_COD_USR
            WHERE O.OPE_NUM_OPE = @NumeroOperacion",
            new { NumeroOperacion = numeroOperacion });

        if (operacion == null)
            return null;

        var liquidacion = new LiquidacionOperacionDto
        {
            NumeroOperacion = operacion.OPE_NUM_OPE,
            FechaOperacion = operacion.OPE_FEC_OPE,
            FechaVencimiento = operacion.OPE_FEC_VTO ?? DateTime.Today.AddDays(30),
            PlazoDias = operacion.PlazoDias ?? 30,
            TipoOperacion = operacion.TipoOperacion,
            Estado = operacion.Estado,
            RutCliente = operacion.OPE_RUT_CLI,
            DvCliente = operacion.DvCliente ?? "",
            NombreCliente = operacion.NombreCliente,
            DireccionCliente = operacion.DireccionCliente,
            ComunaCliente = operacion.ComunaCliente,
            TelefonoCliente = operacion.TelefonoCliente,
            NumeroLinea = operacion.OPE_NUM_LIN ?? 0,
            MontoLinea = operacion.MontoLinea,
            CupoDisponible = operacion.CupoDisponible,
            TotalDocumentos = operacion.TotalDocumentos,
            PorcentajeAnticipo = operacion.PorcentajeAnticipo,
            MontoAnticipo = operacion.MontoAnticipo,
            DiferenciaPrecio = operacion.DiferenciaPrecio,
            TasaInteres = operacion.TasaInteres,
            MontoIntereses = operacion.MontoIntereses,
            TasaComision = operacion.TasaComision,
            MontoComision = operacion.MontoComision,
            MontoGastos = operacion.MontoGastos,
            MontoIVA = operacion.MontoIVA,
            TotalDescuentos = operacion.TotalDescuentos,
            MontoGiro = operacion.MontoGiro,
            FormaPago = operacion.FormaPago,
            BancoDestino = operacion.BancoDestino,
            CuentaDestino = operacion.CuentaDestino,
            NombreEjecutivo = operacion.NombreEjecutivo
        };

        // Obtener documentos (facturas)
        var facturas = await connection.QueryAsync<DocumentoLiquidacionDto>(@"
            SELECT
                'Factura' as TipoDocumento,
                F.FAO_NUM_FAC as NumeroDocumento,
                F.FAO_RUT_DEU as RutDeudor,
                ISNULL(D.DEU_DIG_VER, '') as DvDeudor,
                ISNULL(D.DEU_RAZ_SOC, 'Sin Deudor') as NombreDeudor,
                F.FAO_FEC_EMI as FechaEmision,
                F.FAO_FEC_VTO as FechaVencimiento,
                DATEDIFF(DAY, F.FAO_FEC_EMI, F.FAO_FEC_VTO) as PlazoDias,
                F.FAO_MON_FAC as MontoDocumento,
                ISNULL(F.FAO_MON_ANT, 0) as MontoAnticipado
            FROM FAO F
            LEFT JOIN DEU D ON F.FAO_RUT_DEU = D.DEU_RUT_DEU AND F.FAO_RUT_CLI = D.DEU_RUT_CLI
            WHERE F.FAO_NUM_OPE = @NumeroOperacion
            ORDER BY F.FAO_FEC_VTO",
            new { NumeroOperacion = numeroOperacion });

        liquidacion.Documentos.AddRange(facturas);

        // Obtener cheques
        var cheques = await connection.QueryAsync<DocumentoLiquidacionDto>(@"
            SELECT
                'Cheque' as TipoDocumento,
                CH.CHO_NUM_FAC as NumeroDocumento,
                CH.CHO_RUT_DEU as RutDeudor,
                ISNULL(D.DEU_DIG_VER, '') as DvDeudor,
                ISNULL(D.DEU_RAZ_SOC, 'Sin Deudor') as NombreDeudor,
                CH.CHO_FEC_EMI as FechaEmision,
                CH.CHO_FEC_VTO as FechaVencimiento,
                DATEDIFF(DAY, CH.CHO_FEC_EMI, CH.CHO_FEC_VTO) as PlazoDias,
                CH.CHO_MON_FAC as MontoDocumento,
                ISNULL(CH.CHO_MON_ANT, 0) as MontoAnticipado
            FROM CHO CH
            LEFT JOIN DEU D ON CH.CHO_RUT_DEU = D.DEU_RUT_DEU AND CH.CHO_RUT_CLI = D.DEU_RUT_CLI
            WHERE CH.CHO_NUM_OPE = @NumeroOperacion
            ORDER BY CH.CHO_FEC_VTO",
            new { NumeroOperacion = numeroOperacion });

        liquidacion.Documentos.AddRange(cheques);

        // Obtener gastos
        var gastos = await connection.QueryAsync<GastoLiquidacionDto>(@"
            SELECT
                ISNULL(TG.TGO_DES_GAS, 'Gasto') as Descripcion,
                G.GOP_MON_GAS as Monto,
                ISNULL(TG.TGO_AFE_IVA, 0) as AfectoIVA
            FROM GOP G
            LEFT JOIN TGO TG ON G.GOP_COD_GAS = TG.TGO_COD_GAS
            WHERE G.GOP_NUM_OPE = @NumeroOperacion",
            new { NumeroOperacion = numeroOperacion });

        liquidacion.Gastos.AddRange(gastos);

        return liquidacion;
    }

    /// <summary>
    /// Obtiene datos para pagare
    /// </summary>
    public async Task<PagareDto?> GetPagareAsync(int numeroOperacion)
    {
        using var connection = _connectionFactory.CreateConnection();

        var datos = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT
                O.OPE_NUM_OPE,
                O.OPE_FEC_OPE,
                O.OPE_FEC_VTO,
                ISNULL(O.OPE_TOT_DOC, 0) as MontoPagare,
                -- Deudor principal (primer deudor de la operacion)
                (SELECT TOP 1 FAO_RUT_DEU FROM FAO WHERE FAO_NUM_OPE = O.OPE_NUM_OPE) as RutDeudor,
                (SELECT TOP 1 D.DEU_DIG_VER FROM FAO F
                 INNER JOIN DEU D ON F.FAO_RUT_DEU = D.DEU_RUT_DEU AND F.FAO_RUT_CLI = D.DEU_RUT_CLI
                 WHERE F.FAO_NUM_OPE = O.OPE_NUM_OPE) as DvDeudor,
                (SELECT TOP 1 D.DEU_RAZ_SOC FROM FAO F
                 INNER JOIN DEU D ON F.FAO_RUT_DEU = D.DEU_RUT_DEU AND F.FAO_RUT_CLI = D.DEU_RUT_CLI
                 WHERE F.FAO_NUM_OPE = O.OPE_NUM_OPE) as NombreDeudor,
                (SELECT TOP 1 D.DEU_DIR_DEU FROM FAO F
                 INNER JOIN DEU D ON F.FAO_RUT_DEU = D.DEU_RUT_DEU AND F.FAO_RUT_CLI = D.DEU_RUT_CLI
                 WHERE F.FAO_NUM_OPE = O.OPE_NUM_OPE) as DireccionDeudor,
                (SELECT TOP 1 D.DEU_COM_DEU FROM FAO F
                 INNER JOIN DEU D ON F.FAO_RUT_DEU = D.DEU_RUT_DEU AND F.FAO_RUT_CLI = D.DEU_RUT_CLI
                 WHERE F.FAO_NUM_OPE = O.OPE_NUM_OPE) as ComunaDeudor,
                -- Tasa de interes mora
                ISNULL(O.OPE_TAS_MOR, 1.5) as TasaInteresMora
            FROM OPE O
            WHERE O.OPE_NUM_OPE = @NumeroOperacion",
            new { NumeroOperacion = numeroOperacion });

        if (datos == null)
            return null;

        return new PagareDto
        {
            NumeroOperacion = datos.OPE_NUM_OPE,
            NumeroPagare = datos.OPE_NUM_OPE,
            FechaEmision = datos.OPE_FEC_OPE,
            FechaVencimiento = datos.OPE_FEC_VTO ?? DateTime.Today.AddDays(30),
            LugarEmision = "Santiago",
            RutDeudor = datos.RutDeudor ?? 0,
            DvDeudor = datos.DvDeudor ?? "",
            NombreDeudor = datos.NombreDeudor ?? "Sin Deudor",
            DireccionDeudor = datos.DireccionDeudor ?? "",
            ComunaDeudor = datos.ComunaDeudor ?? "",
            MontoPagare = datos.MontoPagare,
            MontoEnLetras = ConvertirNumeroALetras(datos.MontoPagare),
            TasaInteresMora = datos.TasaInteresMora,
            NombreBeneficiario = "FACTORING S.A.",
            RutBeneficiario = "76.000.000-0"
        };
    }

    /// <summary>
    /// Obtiene estado de cuenta del cliente
    /// </summary>
    public async Task<EstadoCuentaDto?> GetEstadoCuentaAsync(long rutCliente, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        fechaDesde ??= DateTime.Today.AddMonths(-1);
        fechaHasta ??= DateTime.Today;

        // Datos del cliente
        var cliente = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT
                CLI_RUT_CLI as RutCliente,
                CLI_DIG_VER as DvCliente,
                CLI_RAZ_SOC as NombreCliente,
                ISNULL(CLI_DIR_CLI, '') as DireccionCliente
            FROM CLI WHERE CLI_RUT_CLI = @RutCliente",
            new { RutCliente = rutCliente });

        if (cliente == null)
            return null;

        var estado = new EstadoCuentaDto
        {
            FechaEmision = DateTime.Today,
            FechaDesde = fechaDesde.Value,
            FechaHasta = fechaHasta.Value,
            RutCliente = cliente.RutCliente,
            DvCliente = cliente.DvCliente ?? "",
            NombreCliente = cliente.NombreCliente,
            DireccionCliente = cliente.DireccionCliente
        };

        // Movimientos del periodo
        var movimientos = new List<MovimientoEstadoCuentaDto>();

        // Operaciones del periodo
        var operaciones = await connection.QueryAsync<dynamic>(@"
            SELECT
                OPE_FEC_OPE as Fecha,
                'Operacion' as Tipo,
                'Op. ' + CAST(OPE_NUM_OPE as VARCHAR) + ' - ' +
                    CASE OPE_TIP_OPE WHEN 1 THEN 'Facturas' WHEN 2 THEN 'Cheques' ELSE 'Otros' END as Descripcion,
                ISNULL(OPE_TOT_DOC, 0) as Cargo,
                ISNULL(OPE_TOT_GIR, 0) as Abono
            FROM OPE
            WHERE OPE_RUT_CLI = @RutCliente
              AND OPE_FEC_OPE BETWEEN @FechaDesde AND @FechaHasta
              AND OPE_EST_OPE IN (2, 3, 4)
            ORDER BY OPE_FEC_OPE",
            new { RutCliente = rutCliente, FechaDesde = fechaDesde, FechaHasta = fechaHasta });

        foreach (var op in operaciones)
        {
            movimientos.Add(new MovimientoEstadoCuentaDto
            {
                Fecha = op.Fecha,
                Tipo = op.Tipo,
                Descripcion = op.Descripcion,
                Cargo = op.Cargo,
                Abono = op.Abono
            });
        }

        // Pagos del periodo
        var existePag = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PAG'");

        if (existePag > 0)
        {
            var pagos = await connection.QueryAsync<dynamic>(@"
                SELECT
                    PAG_FEC_PAG as Fecha,
                    'Pago' as Tipo,
                    'Pago Doc. ' + PAG_NUM_DOC as Descripcion,
                    0 as Cargo,
                    PAG_MTO_ABO as Abono
                FROM PAG
                WHERE PAG_RUT_CLI = @RutCliente
                  AND PAG_FEC_PAG BETWEEN @FechaDesde AND @FechaHasta
                  AND PAG_EST_PAG = 2
                ORDER BY PAG_FEC_PAG",
                new { RutCliente = rutCliente, FechaDesde = fechaDesde, FechaHasta = fechaHasta });

            foreach (var pago in pagos)
            {
                movimientos.Add(new MovimientoEstadoCuentaDto
                {
                    Fecha = pago.Fecha,
                    Tipo = pago.Tipo,
                    Descripcion = pago.Descripcion,
                    Cargo = 0,
                    Abono = pago.Abono
                });
            }
        }

        // Ordenar y calcular saldos
        movimientos = movimientos.OrderBy(m => m.Fecha).ToList();
        decimal saldo = 0;
        foreach (var mov in movimientos)
        {
            saldo = saldo + mov.Cargo - mov.Abono;
            mov.Saldo = saldo;
        }

        estado.Movimientos = movimientos;
        estado.TotalOperaciones = movimientos.Where(m => m.Tipo == "Operacion").Sum(m => m.Cargo);
        estado.TotalPagos = movimientos.Where(m => m.Tipo == "Pago").Sum(m => m.Abono);
        estado.SaldoActual = saldo;

        // Documentos pendientes
        var docsPendientes = await connection.QueryAsync<DocumentoPendienteEstadoCuentaDto>(@"
            SELECT
                FAO_NUM_OPE as NumeroOperacion,
                'Factura' as TipoDocumento,
                FAO_NUM_FAC as NumeroDocumento,
                ISNULL(D.DEU_RAZ_SOC, 'Sin Deudor') as NombreDeudor,
                FAO_FEC_VTO as FechaVencimiento,
                DATEDIFF(DAY, FAO_FEC_VTO, GETDATE()) as DiasVencido,
                ISNULL(FAO_SAL_PEN, FAO_MON_FAC) as SaldoPendiente
            FROM FAO F
            LEFT JOIN DEU D ON F.FAO_RUT_DEU = D.DEU_RUT_DEU AND F.FAO_RUT_CLI = D.DEU_RUT_CLI
            WHERE F.FAO_RUT_CLI = @RutCliente
              AND F.FAO_EST_FAC NOT IN (4, 7)
              AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0
            ORDER BY FAO_FEC_VTO",
            new { RutCliente = rutCliente });

        estado.DocumentosPendientes = docsPendientes.ToList();

        // Excedentes disponibles
        var existeExc = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EXC'");

        if (existeExc > 0)
        {
            estado.ExcedentesDisponibles = await connection.ExecuteScalarAsync<decimal>(@"
                SELECT ISNULL(SUM(EXC_MON_EXC), 0) FROM EXC
                WHERE EXC_RUT_CLI = @RutCliente AND EXC_EST_EXC = 1",
                new { RutCliente = rutCliente });
        }

        return estado;
    }

    /// <summary>
    /// Convierte un numero a letras (simplificado)
    /// </summary>
    private string ConvertirNumeroALetras(decimal numero)
    {
        var entero = (long)Math.Floor(numero);

        if (entero == 0) return "CERO PESOS";

        string[] unidades = { "", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE" };
        string[] especiales = { "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE" };
        string[] decenas = { "", "", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };
        string[] centenas = { "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS" };

        string resultado = "";

        // Millones
        if (entero >= 1000000)
        {
            long millones = entero / 1000000;
            if (millones == 1)
                resultado += "UN MILLON ";
            else
                resultado += ConvertirCentenas(millones, unidades, especiales, decenas, centenas) + " MILLONES ";
            entero %= 1000000;
        }

        // Miles
        if (entero >= 1000)
        {
            long miles = entero / 1000;
            if (miles == 1)
                resultado += "MIL ";
            else
                resultado += ConvertirCentenas(miles, unidades, especiales, decenas, centenas) + " MIL ";
            entero %= 1000;
        }

        // Centenas
        if (entero > 0)
        {
            if (entero == 100)
                resultado += "CIEN";
            else
                resultado += ConvertirCentenas(entero, unidades, especiales, decenas, centenas);
        }

        return resultado.Trim() + " PESOS";
    }

    private string ConvertirCentenas(long numero, string[] unidades, string[] especiales, string[] decenas, string[] centenas)
    {
        string resultado = "";

        if (numero >= 100)
        {
            resultado += centenas[numero / 100] + " ";
            numero %= 100;
        }

        if (numero >= 20)
        {
            resultado += decenas[numero / 10];
            if (numero % 10 > 0)
                resultado += " Y " + unidades[numero % 10];
        }
        else if (numero >= 10)
        {
            resultado += especiales[numero - 10];
        }
        else if (numero > 0)
        {
            resultado += unidades[numero];
        }

        return resultado.Trim();
    }
}
