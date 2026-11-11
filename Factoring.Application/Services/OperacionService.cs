// =============================================
// Servicio de Operaciones - Lógica de Negocio
// Implementa validaciones, cálculos y flujos de negocio
// =============================================

using Dapper;
using Factoring.Core.Interfaces;
using Factoring.Domain.Entities;
using Factoring.Application.Interfaces;

namespace Factoring.Application.Services;

/// <summary>
/// Resultado de validación de operación
/// </summary>
public class ValidacionOperacionResult
{
    public bool EsValida { get; set; }
    public List<string> Errores { get; set; } = new();
    public decimal MontoDisponibleLinea { get; set; }
    public decimal MontoDisponibleProducto { get; set; }
    public decimal MontoDisponibleDeudor { get; set; }
    public decimal TasaAplicable { get; set; }
    public decimal PorcentajeAnticipoMaximo { get; set; }
}

/// <summary>
/// Resultado del cálculo de operación
/// </summary>
public class CalculoOperacionResult
{
    public decimal MontoDocumentos { get; set; }
    public decimal PorcentajeAnticipo { get; set; }
    public decimal MontoAnticipo { get; set; }
    public decimal TasaNegocio { get; set; }
    public int PlazoDias { get; set; }
    public decimal InteresesDescuento { get; set; }
    public decimal ComisionFija { get; set; }
    public decimal ComisionPorcentual { get; set; }
    public decimal IvaComision { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal MontoAdelanto { get; set; }
    public decimal MontoGiro { get; set; }
    public decimal DiferenciaPrecios { get; set; }
}

/// <summary>
/// DTO para crear una operación completa
/// </summary>
public class CrearOperacionRequest
{
    public long RutCliente { get; set; }
    public int NumeroLinea { get; set; }
    public int TipoDocumento { get; set; }
    public int TipoMoneda { get; set; } = 1;
    public decimal PorcentajeAnticipo { get; set; }
    public int? CodigoBanco { get; set; }
    public string? CuentaCorriente { get; set; }
    public string? Observaciones { get; set; }
    public string? Usuario { get; set; }
    public List<DocumentoRequest> Documentos { get; set; } = new();
    public List<int> GastosCodigos { get; set; } = new();
}

public class DocumentoRequest
{
    public long RutDeudor { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public decimal Monto { get; set; }
    public int? CodigoBanco { get; set; } // Solo para cheques
}

/// <summary>
/// Servicio principal de operaciones
/// </summary>
public class OperacionService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IOperacionRepository _operacionRepository;
    private readonly IFacturaOtorgadaRepository _facturaRepository;
    private readonly IChequeOtorgadoRepository _chequeRepository;
    private readonly IGastoOperacionRepository _gastoRepository;
    private readonly ITipoGastoRepository _tipoGastoRepository;

    private const decimal IVA = 0.19m; // 19% IVA Chile

    public OperacionService(
        IDbConnectionFactory connectionFactory,
        IOperacionRepository operacionRepository,
        IFacturaOtorgadaRepository facturaRepository,
        IChequeOtorgadoRepository chequeRepository,
        IGastoOperacionRepository gastoRepository,
        ITipoGastoRepository tipoGastoRepository)
    {
        _connectionFactory = connectionFactory;
        _operacionRepository = operacionRepository;
        _facturaRepository = facturaRepository;
        _chequeRepository = chequeRepository;
        _gastoRepository = gastoRepository;
        _tipoGastoRepository = tipoGastoRepository;
    }

    /// <summary>
    /// Valida si una operación puede ser cursada según línea de crédito
    /// </summary>
    public async Task<ValidacionOperacionResult> ValidarOperacionAsync(
        long rutCliente, int numeroLinea, int tipoDocumento, decimal montoOperacion, long? rutDeudor = null)
    {
        var result = new ValidacionOperacionResult();

        using var connection = _connectionFactory.CreateConnection();

        // 1. Verificar que la línea de crédito exista y esté vigente
        var lineaCredito = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT
                LCR_EST_LIN as EstadoLinea,
                LCR_EST_SOL as EstadoSolicitud,
                LCR_MON_APR as MontoAprobado,
                LCR_MON_DIS as MontoDisponible,
                LCR_FVI_DES as VigenciaDesde,
                LCR_FVI_HAS as VigenciaHasta,
                LCR_TAS_NEG as TasaNegocio,
                LCR_SPR_EAD as Spread
            FROM LCR
            WHERE LCR_RUT_CLI = @RutCliente AND LCR_NUM_LIN = @NumeroLinea",
            new { RutCliente = rutCliente, NumeroLinea = numeroLinea });

        if (lineaCredito == null)
        {
            result.Errores.Add("No existe línea de crédito para este cliente");
            return result;
        }

        // 2. Verificar estado de línea (2 = Vigente)
        if ((int)lineaCredito.EstadoLinea != 2)
        {
            result.Errores.Add($"La línea de crédito no está vigente (Estado: {lineaCredito.EstadoLinea})");
        }

        // 3. Verificar vigencia temporal
        var hoy = DateTime.Today;
        if (lineaCredito.VigenciaHasta != null && (DateTime)lineaCredito.VigenciaHasta < hoy)
        {
            result.Errores.Add($"La línea de crédito está vencida (Venció: {lineaCredito.VigenciaHasta:dd/MM/yyyy})");
        }

        // 4. Verificar monto disponible en línea global
        decimal montoDisponible = (decimal)(lineaCredito.MontoDisponible ?? 0);
        result.MontoDisponibleLinea = montoDisponible;

        if (montoOperacion > montoDisponible)
        {
            result.Errores.Add($"El monto excede el cupo disponible (Disponible: {montoDisponible:N0}, Solicitado: {montoOperacion:N0})");
        }

        // 5. Verificar sublínea por producto
        var sublineaProducto = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT
                ANT_MON_APR as MontoAprobado,
                ANT_MON_DIS as MontoDisponible,
                ANT_POR_ANT as PorcentajeAnticipo,
                ANT_PLA_VTO as PlazoVencimiento
            FROM ANT
            WHERE ANT_RUT_CLI = @RutCliente
              AND ANT_NUM_LIN = @NumeroLinea
              AND ANT_TIP_DOC = @TipoDocumento",
            new { RutCliente = rutCliente, NumeroLinea = numeroLinea, TipoDocumento = tipoDocumento });

        if (sublineaProducto != null)
        {
            decimal montoDisponibleProd = (decimal)(sublineaProducto.MontoDisponible ?? 0);
            result.MontoDisponibleProducto = montoDisponibleProd;
            result.PorcentajeAnticipoMaximo = (decimal)(sublineaProducto.PorcentajeAnticipo ?? 100);

            if (montoOperacion > montoDisponibleProd)
            {
                result.Errores.Add($"El monto excede el cupo del producto (Disponible: {montoDisponibleProd:N0})");
            }
        }

        // 6. Verificar sublínea por deudor (si aplica)
        if (rutDeudor.HasValue)
        {
            var sublineaDeudor = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT
                    SUB_MON_APR as MontoAprobado,
                    SUB_MON_DIS as MontoDisponible,
                    SUB_EST_LIN as EstadoLinea
                FROM SUB
                WHERE SUB_RUT_CLI = @RutCliente
                  AND SUB_NUM_LIN = @NumeroLinea
                  AND SUB_RUT_DEU = @RutDeudor
                  AND SUB_TIP_DOC = @TipoDocumento",
                new { RutCliente = rutCliente, NumeroLinea = numeroLinea, RutDeudor = rutDeudor, TipoDocumento = tipoDocumento });

            if (sublineaDeudor != null)
            {
                decimal montoDisponibleDeudor = (decimal)(sublineaDeudor.MontoDisponible ?? 0);
                result.MontoDisponibleDeudor = montoDisponibleDeudor;

                if (montoOperacion > montoDisponibleDeudor && montoDisponibleDeudor > 0)
                {
                    result.Errores.Add($"El monto excede el cupo del deudor (Disponible: {montoDisponibleDeudor:N0})");
                }
            }
        }

        // 7. Verificar morosidad del cliente
        var documentosMorosos = await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM FAO
            WHERE FAO_RUT_CLI = @RutCliente
              AND FAO_EST_FAC = 5 -- Estado Moroso
              AND FAO_SAL_PEN > 0",
            new { RutCliente = rutCliente });

        if (documentosMorosos > 0)
        {
            result.Errores.Add($"El cliente tiene {documentosMorosos} documento(s) en mora");
        }

        // 8. Calcular tasa aplicable
        decimal tasaBase = (decimal)(lineaCredito.TasaNegocio ?? 0);
        decimal spread = (decimal)(lineaCredito.Spread ?? 0);
        result.TasaAplicable = tasaBase + spread;

        result.EsValida = result.Errores.Count == 0;
        return result;
    }

    /// <summary>
    /// Calcula los valores de una operación
    /// </summary>
    public async Task<CalculoOperacionResult> CalcularOperacionAsync(
        long rutCliente, int numeroLinea, int tipoDocumento,
        decimal montoDocumentos, int plazoDias, decimal? porcentajeAnticipoSolicitado = null)
    {
        var result = new CalculoOperacionResult();

        using var connection = _connectionFactory.CreateConnection();

        // Obtener tasa según monto y plazo de la tabla TYP
        var tasa = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT TOP 1
                TYP_TAS_BAS as TasaBase,
                TYP_SPR_EAD as Spread,
                TYP_TAS_TOT as TasaTotal
            FROM TYP
            WHERE TYP_TIP_MON = 1
              AND TYP_MON_DES <= @Monto
              AND TYP_MON_HAS >= @Monto
              AND TYP_PLA_DES <= @Plazo
              AND TYP_PLA_HAS >= @Plazo
              AND TYP_EST_TYP = 1
            ORDER BY TYP_MON_DES, TYP_PLA_DES",
            new { Monto = montoDocumentos, Plazo = plazoDias });

        // Si no hay tasa específica, usar la de la línea de crédito
        if (tasa == null)
        {
            var lineaTasa = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT LCR_TAS_NEG as TasaNegocio, LCR_SPR_EAD as Spread
                FROM LCR
                WHERE LCR_RUT_CLI = @RutCliente AND LCR_NUM_LIN = @NumeroLinea",
                new { RutCliente = rutCliente, NumeroLinea = numeroLinea });

            if (lineaTasa != null)
            {
                result.TasaNegocio = (decimal)(lineaTasa.TasaNegocio ?? 0) + (decimal)(lineaTasa.Spread ?? 0);
            }
            else
            {
                result.TasaNegocio = 0.03m; // Tasa por defecto 3%
            }
        }
        else
        {
            result.TasaNegocio = (decimal)(tasa.TasaTotal ?? 0);
        }

        // Obtener porcentaje de anticipo máximo del producto
        var sublinea = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT ANT_POR_ANT as PorcentajeAnticipo, ANT_COM_POR as ComisionPorcentual, ANT_COM_MIN as ComisionMinima
            FROM ANT
            WHERE ANT_RUT_CLI = @RutCliente AND ANT_NUM_LIN = @NumeroLinea AND ANT_TIP_DOC = @TipoDocumento",
            new { RutCliente = rutCliente, NumeroLinea = numeroLinea, TipoDocumento = tipoDocumento });

        decimal porcentajeMaximo = 100m;
        decimal comisionPorcentual = 0.005m; // 0.5% por defecto
        decimal comisionMinima = 0m;

        if (sublinea != null)
        {
            porcentajeMaximo = (decimal)(sublinea.PorcentajeAnticipo ?? 100);
            comisionPorcentual = (decimal)(sublinea.ComisionPorcentual ?? 0) / 100;
            comisionMinima = (decimal)(sublinea.ComisionMinima ?? 0);
        }

        // Usar el porcentaje solicitado o el máximo
        result.PorcentajeAnticipo = porcentajeAnticipoSolicitado.HasValue
            ? Math.Min(porcentajeAnticipoSolicitado.Value, porcentajeMaximo)
            : porcentajeMaximo;

        result.MontoDocumentos = montoDocumentos;
        result.PlazoDias = plazoDias;

        // Calcular monto de anticipo
        result.MontoAnticipo = Math.Round(montoDocumentos * (result.PorcentajeAnticipo / 100), 0);

        // Calcular intereses (descuento anticipado)
        // Fórmula: Monto * Tasa * (Días / 30)
        result.InteresesDescuento = Math.Round(result.MontoAnticipo * result.TasaNegocio * (plazoDias / 30m), 0);

        // Calcular comisión
        result.ComisionPorcentual = Math.Round(montoDocumentos * comisionPorcentual, 0);
        result.ComisionFija = comisionMinima;
        decimal totalComision = Math.Max(result.ComisionPorcentual + result.ComisionFija, comisionMinima);

        // Calcular IVA de comisión
        result.IvaComision = Math.Round(totalComision * IVA, 0);

        // Total gastos
        result.TotalGastos = result.InteresesDescuento + totalComision + result.IvaComision;

        // Monto adelanto (neto a girar)
        result.MontoAdelanto = result.MontoAnticipo - result.TotalGastos;

        // Monto giro final
        result.MontoGiro = result.MontoAdelanto;

        // Diferencia de precios (lo que queda retenido)
        result.DiferenciaPrecios = montoDocumentos - result.MontoAnticipo;

        return result;
    }

    /// <summary>
    /// Crea una operación completa con documentos y gastos
    /// </summary>
    public async Task<(bool Exito, long NumeroOperacion, string Mensaje)> CrearOperacionCompletaAsync(CrearOperacionRequest request)
    {
        if (request.Documentos.Count == 0)
            return (false, 0, "Debe incluir al menos un documento");

        // Calcular monto total de documentos
        decimal montoDocumentos = request.Documentos.Sum(d => d.Monto);

        // Calcular plazo promedio ponderado
        var hoy = DateTime.Today;
        int plazoPonderado = (int)request.Documentos
            .Where(d => d.FechaVencimiento > hoy)
            .Select(d => (d.FechaVencimiento - hoy).Days * d.Monto / montoDocumentos)
            .Sum();

        if (plazoPonderado <= 0) plazoPonderado = 30;

        // Validar operación
        var validacion = await ValidarOperacionAsync(
            request.RutCliente, request.NumeroLinea, request.TipoDocumento,
            montoDocumentos * (request.PorcentajeAnticipo / 100));

        if (!validacion.EsValida)
        {
            return (false, 0, string.Join("; ", validacion.Errores));
        }

        // Calcular valores
        var calculo = await CalcularOperacionAsync(
            request.RutCliente, request.NumeroLinea, request.TipoDocumento,
            montoDocumentos, plazoPonderado, request.PorcentajeAnticipo);

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Crear la operación
            var sqlOperacion = @"
                INSERT INTO OPE (
                    OPE_RUT_CLI, OPE_NUM_LIN, OPE_TIP_DOC, OPE_FEC_OPE, OPE_EST_OPE, OPE_MON_OPE,
                    OPE_MON_DOC, OPE_POR_ANT, OPE_MON_ANT, OPE_MON_ADE, OPE_MON_GAS,
                    OPE_COM_TOT, OPE_IVA_COM, OPE_TAS_NEG, OPE_TOT_GIR, OPE_DIF_PRE,
                    OPE_PLA_PRO, OPE_COD_BAN, OPE_CTA_CTE, OPE_OBS_OPE,
                    OPE_USR_CRE, OPE_FEC_CRE
                ) VALUES (
                    @RutCliente, @NumeroLinea, @TipoDocumento, @FechaOperacion, 1, @TipoMoneda,
                    @MontoDocumentos, @PorcentajeAnticipo, @MontoAnticipo, @MontoAdelanto, @MontoGastos,
                    @ComisionTotal, @IvaComision, @TasaNegocio, @TotalGiro, @DiferenciaPrecios,
                    @PlazoDias, @CodigoBanco, @CuentaCorriente, @Observaciones,
                    @Usuario, GETDATE()
                );
                SELECT SCOPE_IDENTITY();";

            var numeroOperacion = await connection.ExecuteScalarAsync<long>(sqlOperacion, new
            {
                request.RutCliente,
                request.NumeroLinea,
                request.TipoDocumento,
                FechaOperacion = DateTime.Today,
                request.TipoMoneda,
                calculo.MontoDocumentos,
                calculo.PorcentajeAnticipo,
                calculo.MontoAnticipo,
                calculo.MontoAdelanto,
                MontoGastos = calculo.TotalGastos,
                ComisionTotal = calculo.ComisionFija + calculo.ComisionPorcentual,
                calculo.IvaComision,
                calculo.TasaNegocio,
                TotalGiro = calculo.MontoGiro,
                calculo.DiferenciaPrecios,
                PlazoDias = calculo.PlazoDias,
                request.CodigoBanco,
                request.CuentaCorriente,
                request.Observaciones,
                Usuario = request.Usuario ?? "SISTEMA"
            }, transaction);

            // Insertar documentos
            foreach (var doc in request.Documentos)
            {
                if (request.TipoDocumento == 1) // Factura
                {
                    await connection.ExecuteAsync(@"
                        INSERT INTO FAO (FAO_NUM_OPE, FAO_RUT_CLI, FAO_RUT_DEU, FAO_NUM_FAC, FAO_TIP_DOC,
                            FAO_FEC_EMI, FAO_FEC_VTO, FAO_MON_FAC, FAO_SAL_PEN, FAO_POR_ANT, FAO_MON_ANT,
                            FAO_EST_FAC, FAO_USR_CRE, FAO_FEC_CRE)
                        VALUES (@NumOpe, @RutCli, @RutDeu, @NumFac, @TipDoc,
                            @FecEmi, @FecVto, @MonFac, @MonFac, @PorAnt, @MonAnt,
                            1, @Usuario, GETDATE())",
                        new
                        {
                            NumOpe = numeroOperacion,
                            RutCli = request.RutCliente,
                            RutDeu = doc.RutDeudor,
                            NumFac = doc.NumeroDocumento,
                            TipDoc = 1,
                            FecEmi = doc.FechaEmision,
                            FecVto = doc.FechaVencimiento,
                            MonFac = doc.Monto,
                            PorAnt = calculo.PorcentajeAnticipo,
                            MonAnt = doc.Monto * (calculo.PorcentajeAnticipo / 100),
                            Usuario = request.Usuario ?? "SISTEMA"
                        }, transaction);
                }
                else if (request.TipoDocumento == 2) // Cheque
                {
                    await connection.ExecuteAsync(@"
                        INSERT INTO CHO (CHO_NUM_OPE, CHO_RUT_CLI, CHO_RUT_DEU, CHO_NUM_FAC, CHO_TIP_DOC,
                            CHO_COD_BAN, CHO_FEC_EMI, CHO_FEC_VTO, CHO_MON_FAC, CHO_SAL_PEN, CHO_POR_ANT, CHO_MON_ANT,
                            CHO_EST_FAC, CHO_USR_CRE, CHO_FEC_CRE)
                        VALUES (@NumOpe, @RutCli, @RutDeu, @NumFac, @TipDoc,
                            @CodBan, @FecEmi, @FecVto, @MonFac, @MonFac, @PorAnt, @MonAnt,
                            1, @Usuario, GETDATE())",
                        new
                        {
                            NumOpe = numeroOperacion,
                            RutCli = request.RutCliente,
                            RutDeu = doc.RutDeudor,
                            NumFac = doc.NumeroDocumento,
                            TipDoc = 2,
                            CodBan = doc.CodigoBanco,
                            FecEmi = doc.FechaEmision,
                            FecVto = doc.FechaVencimiento,
                            MonFac = doc.Monto,
                            PorAnt = calculo.PorcentajeAnticipo,
                            MonAnt = doc.Monto * (calculo.PorcentajeAnticipo / 100),
                            Usuario = request.Usuario ?? "SISTEMA"
                        }, transaction);
                }
            }

            // Actualizar disponible de línea de crédito
            await connection.ExecuteAsync(@"
                UPDATE LCR SET
                    LCR_MON_DIS = LCR_MON_DIS - @MontoOperacion,
                    LCR_FEC_MOD = GETDATE()
                WHERE LCR_RUT_CLI = @RutCliente AND LCR_NUM_LIN = @NumeroLinea",
                new
                {
                    MontoOperacion = calculo.MontoAnticipo,
                    request.RutCliente,
                    request.NumeroLinea
                }, transaction);

            // Actualizar disponible de sublínea producto (si existe)
            await connection.ExecuteAsync(@"
                UPDATE ANT SET
                    ANT_MON_DIS = ANT_MON_DIS - @MontoOperacion
                WHERE ANT_RUT_CLI = @RutCliente
                  AND ANT_NUM_LIN = @NumeroLinea
                  AND ANT_TIP_DOC = @TipoDocumento",
                new
                {
                    MontoOperacion = calculo.MontoAnticipo,
                    request.RutCliente,
                    request.NumeroLinea,
                    request.TipoDocumento
                }, transaction);

            transaction.Commit();

            return (true, numeroOperacion, $"Operación {numeroOperacion} creada exitosamente. Giro: ${calculo.MontoGiro:N0}");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return (false, 0, $"Error al crear operación: {ex.Message}");
        }
    }

    /// <summary>
    /// Otorga una operación y genera el giro
    /// </summary>
    public async Task<(bool Exito, string Mensaje)> OtorgarOperacionAsync(long numeroOperacion, string usuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Verificar estado actual
            var operacion = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT OPE_EST_OPE as Estado, OPE_RUT_CLI as RutCliente, OPE_TOT_GIR as MontoGiro,
                       OPE_COD_BAN as CodigoBanco, OPE_CTA_CTE as CuentaCorriente
                FROM OPE WHERE OPE_NUM_OPE = @NumeroOperacion",
                new { NumeroOperacion = numeroOperacion }, transaction);

            if (operacion == null)
                return (false, "Operación no encontrada");

            if ((int)operacion.Estado != 1)
                return (false, "La operación no está en estado Ingresada");

            // Actualizar estado a Otorgada
            await connection.ExecuteAsync(@"
                UPDATE OPE SET
                    OPE_EST_OPE = 2,
                    OPE_FEC_OTO = @FechaOtorgamiento,
                    OPE_FEC_GIR = @FechaGiro,
                    OPE_USR_OTO = @Usuario,
                    OPE_FEC_MOD = GETDATE()
                WHERE OPE_NUM_OPE = @NumeroOperacion",
                new
                {
                    NumeroOperacion = numeroOperacion,
                    FechaOtorgamiento = DateTime.Today,
                    FechaGiro = DateTime.Today,
                    Usuario = usuario
                }, transaction);

            // Generar registro de giro en tabla GIR
            var existeGir = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'GIR'",
                transaction: transaction);

            if (existeGir > 0)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO GIR (
                        GIR_NUM_OPE, GIR_RUT_CLI, GIR_FEC_GIR, GIR_MON_GIR,
                        GIR_COD_BAN, GIR_CTA_CTE, GIR_EST_GIR, GIR_FEC_CRE
                    ) VALUES (
                        @NumeroOperacion, @RutCliente, @FechaGiro, @MontoGiro,
                        @CodigoBanco, @CuentaCorriente, 1, GETDATE()
                    )",
                    new
                    {
                        NumeroOperacion = numeroOperacion,
                        RutCliente = (long)operacion.RutCliente,
                        FechaGiro = DateTime.Today,
                        MontoGiro = (decimal)operacion.MontoGiro,
                        CodigoBanco = (int?)operacion.CodigoBanco,
                        CuentaCorriente = (string?)operacion.CuentaCorriente
                    }, transaction);
            }

            // Generar movimiento de tesorería en MOV (si existe la tabla)
            var existeMov = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MOV'",
                transaction: transaction);

            if (existeMov > 0)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO MOV (
                        MOV_NUM_OPE, MOV_RUT_CLI, MOV_FEC_MOV, MOV_TIP_MOV,
                        MOV_MON_MOV, MOV_DES_MOV, MOV_EST_MOV, MOV_FEC_CRE
                    ) VALUES (
                        @NumeroOperacion, @RutCliente, @FechaMov, 'E',
                        @MontoMov, @Descripcion, 1, GETDATE()
                    )",
                    new
                    {
                        NumeroOperacion = numeroOperacion,
                        RutCliente = (long)operacion.RutCliente,
                        FechaMov = DateTime.Today,
                        MontoMov = (decimal)operacion.MontoGiro,
                        Descripcion = $"Giro Operación {numeroOperacion}"
                    }, transaction);
            }

            transaction.Commit();
            return (true, $"Operación {numeroOperacion} otorgada. Giro generado por ${(decimal)operacion.MontoGiro:N0}");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return (false, $"Error al otorgar operación: {ex.Message}");
        }
    }

    /// <summary>
    /// Anula una operación y revierte movimientos
    /// </summary>
    public async Task<(bool Exito, string Mensaje)> AnularOperacionAsync(long numeroOperacion, string motivo, string usuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Obtener datos de la operación
            var operacion = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT OPE_EST_OPE as Estado, OPE_RUT_CLI as RutCliente, OPE_NUM_LIN as NumeroLinea,
                       OPE_TIP_DOC as TipoDocumento, OPE_MON_ANT as MontoAnticipo
                FROM OPE WHERE OPE_NUM_OPE = @NumeroOperacion",
                new { NumeroOperacion = numeroOperacion }, transaction);

            if (operacion == null)
                return (false, "Operación no encontrada");

            if ((int)operacion.Estado >= 4)
                return (false, "La operación ya está anulada");

            // Cambiar estado a Anulada
            await connection.ExecuteAsync(@"
                UPDATE OPE SET
                    OPE_EST_OPE = 4,
                    OPE_OBS_REC = @Motivo,
                    OPE_USR_MOD = @Usuario,
                    OPE_FEC_MOD = GETDATE()
                WHERE OPE_NUM_OPE = @NumeroOperacion",
                new { NumeroOperacion = numeroOperacion, Motivo = motivo, Usuario = usuario }, transaction);

            // Revertir cupo de línea de crédito
            await connection.ExecuteAsync(@"
                UPDATE LCR SET
                    LCR_MON_DIS = LCR_MON_DIS + @MontoAnticipo,
                    LCR_FEC_MOD = GETDATE()
                WHERE LCR_RUT_CLI = @RutCliente AND LCR_NUM_LIN = @NumeroLinea",
                new
                {
                    MontoAnticipo = (decimal)operacion.MontoAnticipo,
                    RutCliente = (long)operacion.RutCliente,
                    NumeroLinea = (int)operacion.NumeroLinea
                }, transaction);

            // Revertir sublínea producto
            await connection.ExecuteAsync(@"
                UPDATE ANT SET
                    ANT_MON_DIS = ANT_MON_DIS + @MontoAnticipo
                WHERE ANT_RUT_CLI = @RutCliente
                  AND ANT_NUM_LIN = @NumeroLinea
                  AND ANT_TIP_DOC = @TipoDocumento",
                new
                {
                    MontoAnticipo = (decimal)operacion.MontoAnticipo,
                    RutCliente = (long)operacion.RutCliente,
                    NumeroLinea = (int)operacion.NumeroLinea,
                    TipoDocumento = (int)operacion.TipoDocumento
                }, transaction);

            // Anular documentos
            await connection.ExecuteAsync(@"
                UPDATE FAO SET FAO_EST_FAC = 7 WHERE FAO_NUM_OPE = @NumeroOperacion",
                new { NumeroOperacion = numeroOperacion }, transaction);

            await connection.ExecuteAsync(@"
                UPDATE CHO SET CHO_EST_FAC = 7 WHERE CHO_NUM_OPE = @NumeroOperacion",
                new { NumeroOperacion = numeroOperacion }, transaction);

            transaction.Commit();
            return (true, $"Operación {numeroOperacion} anulada correctamente");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return (false, $"Error al anular operación: {ex.Message}");
        }
    }
}
