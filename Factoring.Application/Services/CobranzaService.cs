// =============================================
// Servicio de Cobranza - Lógica de Negocio
// Implementa recaudación, pagos parciales y excedentes
// =============================================

using Dapper;
using Factoring.Core.Interfaces;

namespace Factoring.Application.Services;

/// <summary>
/// Resultado de registro de pago
/// </summary>
public class RegistroPagoResult
{
    public bool Exito { get; set; }
    public int NumeroPago { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public decimal SaldoAnterior { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoNuevo { get; set; }
    public decimal? MontoExcedente { get; set; }
    public bool DocumentoCancelado { get; set; }
}

/// <summary>
/// DTO para registrar un pago
/// </summary>
public class RegistrarPagoRequest
{
    public int TipoDocumento { get; set; } // 1=Factura, 2=Cheque, 3=Letra
    public string NumeroDocumento { get; set; } = string.Empty;
    public int NumeroOperacion { get; set; }
    public long RutCliente { get; set; }
    public long RutDeudor { get; set; }
    public decimal MontoPago { get; set; }
    public int TipoPago { get; set; } // 1=Pago Deudor, 2=Pago Cliente, 3=Depósito
    public int? CodigoBanco { get; set; }
    public string? NumeroComprobante { get; set; }
    public DateTime FechaPago { get; set; }
    public string? Observaciones { get; set; }
    public string Usuario { get; set; } = "SISTEMA";
}

/// <summary>
/// Documento con saldo pendiente
/// </summary>
public class DocumentoPendiente
{
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public string TipoDocumentoNombre { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
    public int DiasVencido { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal MontoAnticipado { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public int EstadoDocumento { get; set; }
    public string EstadoNombre { get; set; } = string.Empty;
}

/// <summary>
/// Excedente generado
/// </summary>
public class ExcedenteCliente
{
    public int NumeroExcedente { get; set; }
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public int NumeroOperacion { get; set; }
    public int NumeroPago { get; set; }
    public decimal MontoExcedente { get; set; }
    public int EstadoExcedente { get; set; } // 1=Pendiente, 2=Aplicado, 3=Girado
    public DateTime FechaGeneracion { get; set; }
    public DateTime? FechaAplicacion { get; set; }
}

/// <summary>
/// Resumen de cartera por cliente
/// </summary>
public class ResumenCarteraCliente
{
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public int CantidadDocumentos { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal SaldoTotal { get; set; }
    public int DocumentosVigentes { get; set; }
    public int DocumentosMorosos { get; set; }
    public decimal MontoMoroso { get; set; }
    public decimal ExcedentesDisponibles { get; set; }
}

/// <summary>
/// Servicio principal de cobranza
/// </summary>
public class CobranzaService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CobranzaService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Obtiene documentos pendientes de pago
    /// </summary>
    public async Task<IEnumerable<DocumentoPendiente>> GetDocumentosPendientesAsync(
        long? rutCliente = null, long? rutDeudor = null, int? tipoDocumento = null,
        DateTime? fechaVencimientoDesde = null, DateTime? fechaVencimientoHasta = null,
        bool soloMorosos = false)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT
                FAO_NUM_OPE as NumeroOperacion,
                1 as TipoDocumento,
                'Factura' as TipoDocumentoNombre,
                FAO_NUM_FAC as NumeroDocumento,
                F.FAO_RUT_CLI as RutCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                F.FAO_RUT_DEU as RutDeudor,
                D.DEU_RAZ_SOC as NombreDeudor,
                F.FAO_FEC_VTO as FechaVencimiento,
                DATEDIFF(DAY, F.FAO_FEC_VTO, GETDATE()) as DiasVencido,
                F.FAO_MON_FAC as MontoDocumento,
                ISNULL(F.FAO_MON_ANT, 0) as MontoAnticipado,
                ISNULL(F.FAO_PAG_PEN, 0) as MontoPagado,
                ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) as SaldoPendiente,
                F.FAO_EST_FAC as EstadoDocumento,
                CASE F.FAO_EST_FAC
                    WHEN 1 THEN 'Vigente'
                    WHEN 2 THEN 'Prorrogado'
                    WHEN 3 THEN 'Renovado'
                    WHEN 4 THEN 'Cancelado'
                    WHEN 5 THEN 'Moroso'
                    WHEN 6 THEN 'Protestado'
                    WHEN 7 THEN 'Anulado'
                    ELSE 'Desconocido'
                END as EstadoNombre
            FROM FAO F
            INNER JOIN CLI C ON F.FAO_RUT_CLI = C.CLI_RUT_CLI
            LEFT JOIN DEU D ON F.FAO_RUT_DEU = D.DEU_RUT_DEU AND F.FAO_RUT_CLI = D.DEU_RUT_CLI
            WHERE F.FAO_EST_FAC NOT IN (4, 7) -- Excluir cancelados y anulados
              AND ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) > 0";

        var parameters = new DynamicParameters();

        if (rutCliente.HasValue)
        {
            sql += " AND F.FAO_RUT_CLI = @RutCliente";
            parameters.Add("RutCliente", rutCliente.Value);
        }

        if (rutDeudor.HasValue)
        {
            sql += " AND F.FAO_RUT_DEU = @RutDeudor";
            parameters.Add("RutDeudor", rutDeudor.Value);
        }

        if (fechaVencimientoDesde.HasValue)
        {
            sql += " AND F.FAO_FEC_VTO >= @FechaDesde";
            parameters.Add("FechaDesde", fechaVencimientoDesde.Value);
        }

        if (fechaVencimientoHasta.HasValue)
        {
            sql += " AND F.FAO_FEC_VTO <= @FechaHasta";
            parameters.Add("FechaHasta", fechaVencimientoHasta.Value);
        }

        if (soloMorosos)
        {
            sql += " AND F.FAO_FEC_VTO < GETDATE()";
        }

        // Agregar cheques
        sql += @"
            UNION ALL
            SELECT
                CHO_NUM_OPE,
                2,
                'Cheque',
                CHO_NUM_FAC,
                CH.CHO_RUT_CLI,
                C.CLI_RAZ_SOC,
                CH.CHO_RUT_DEU,
                D.DEU_RAZ_SOC,
                CH.CHO_FEC_VTO,
                DATEDIFF(DAY, CH.CHO_FEC_VTO, GETDATE()),
                CH.CHO_MON_FAC,
                ISNULL(CH.CHO_MON_ANT, 0),
                ISNULL(CH.CHO_PAG_PEN, 0),
                ISNULL(CH.CHO_SAL_PEN, CH.CHO_MON_FAC),
                CH.CHO_EST_FAC,
                CASE CH.CHO_EST_FAC
                    WHEN 1 THEN 'Vigente'
                    WHEN 4 THEN 'Cancelado'
                    WHEN 5 THEN 'Moroso'
                    ELSE 'Otro'
                END
            FROM CHO CH
            INNER JOIN CLI C ON CH.CHO_RUT_CLI = C.CLI_RUT_CLI
            LEFT JOIN DEU D ON CH.CHO_RUT_DEU = D.DEU_RUT_DEU AND CH.CHO_RUT_CLI = D.DEU_RUT_CLI
            WHERE CH.CHO_EST_FAC NOT IN (4, 7)
              AND ISNULL(CH.CHO_SAL_PEN, CH.CHO_MON_FAC) > 0";

        if (rutCliente.HasValue)
            sql += " AND CH.CHO_RUT_CLI = @RutCliente";
        if (rutDeudor.HasValue)
            sql += " AND CH.CHO_RUT_DEU = @RutDeudor";
        if (soloMorosos)
            sql += " AND CH.CHO_FEC_VTO < GETDATE()";

        sql += " ORDER BY FechaVencimiento ASC";

        return await connection.QueryAsync<DocumentoPendiente>(sql, parameters);
    }

    /// <summary>
    /// Registra un pago y actualiza saldos
    /// </summary>
    public async Task<RegistroPagoResult> RegistrarPagoAsync(RegistrarPagoRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Obtener saldo actual del documento
            decimal saldoActual = 0;
            string tablaDoc = "";

            if (request.TipoDocumento == 1) // Factura
            {
                tablaDoc = "FAO";
                saldoActual = await connection.ExecuteScalarAsync<decimal>(@"
                    SELECT ISNULL(FAO_SAL_PEN, FAO_MON_FAC)
                    FROM FAO
                    WHERE FAO_NUM_OPE = @NumOpe AND FAO_NUM_FAC = @NumDoc",
                    new { NumOpe = request.NumeroOperacion, NumDoc = request.NumeroDocumento },
                    transaction);
            }
            else if (request.TipoDocumento == 2) // Cheque
            {
                tablaDoc = "CHO";
                saldoActual = await connection.ExecuteScalarAsync<decimal>(@"
                    SELECT ISNULL(CHO_SAL_PEN, CHO_MON_FAC)
                    FROM CHO
                    WHERE CHO_NUM_OPE = @NumOpe AND CHO_NUM_FAC = @NumDoc",
                    new { NumOpe = request.NumeroOperacion, NumDoc = request.NumeroDocumento },
                    transaction);
            }

            if (saldoActual <= 0)
            {
                return new RegistroPagoResult
                {
                    Exito = false,
                    Mensaje = "El documento no tiene saldo pendiente"
                };
            }

            // 2. Calcular montos
            decimal montoAplicar = Math.Min(request.MontoPago, saldoActual);
            decimal saldoNuevo = saldoActual - montoAplicar;
            decimal? excedente = request.MontoPago > saldoActual ? request.MontoPago - saldoActual : null;
            bool documentoCancelado = saldoNuevo <= 0;

            // 3. Verificar si existe tabla PAG
            var existePag = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PAG'",
                transaction: transaction);

            int numeroPago = 0;

            if (existePag > 0)
            {
                // 4. Crear registro de pago en PAG
                numeroPago = await connection.ExecuteScalarAsync<int>(@"
                    INSERT INTO PAG (
                        PAG_FEC_PAG, PAG_TIP_DOC, PAG_NUM_DOC, PAG_NUM_OPE,
                        PAG_RUT_CLI, PAG_RUT_DEU, PAG_MTO_DOC, PAG_SAL_DOC,
                        PAG_MTO_PAG, PAG_MTO_ABO, PAG_TIP_PAG, PAG_EST_PAG,
                        PAG_OBS_PAG, PAG_USR_CRE, PAG_FEC_CRE
                    ) VALUES (
                        @FechaPago, @TipoDocumento, @NumeroDocumento, @NumeroOperacion,
                        @RutCliente, @RutDeudor, @MontoDocumento, @SaldoAnterior,
                        @MontoPago, @MontoAplicar, @TipoPago, 2,
                        @Observaciones, @Usuario, GETDATE()
                    );
                    SELECT SCOPE_IDENTITY();",
                    new
                    {
                        request.FechaPago,
                        request.TipoDocumento,
                        request.NumeroDocumento,
                        request.NumeroOperacion,
                        request.RutCliente,
                        request.RutDeudor,
                        MontoDocumento = saldoActual,
                        SaldoAnterior = saldoActual,
                        request.MontoPago,
                        MontoAplicar = montoAplicar,
                        request.TipoPago,
                        request.Observaciones,
                        request.Usuario
                    }, transaction);
            }

            // 5. Actualizar saldo del documento
            if (request.TipoDocumento == 1)
            {
                await connection.ExecuteAsync(@"
                    UPDATE FAO SET
                        FAO_PAG_PEN = ISNULL(FAO_PAG_PEN, 0) + @MontoAplicar,
                        FAO_SAL_PEN = @SaldoNuevo,
                        FAO_EST_FAC = CASE WHEN @SaldoNuevo <= 0 THEN 4 ELSE FAO_EST_FAC END,
                        FAO_FEC_PAG = @FechaPago,
                        FAO_FEC_MOD = GETDATE()
                    WHERE FAO_NUM_OPE = @NumOpe AND FAO_NUM_FAC = @NumDoc",
                    new
                    {
                        MontoAplicar = montoAplicar,
                        SaldoNuevo = saldoNuevo,
                        request.FechaPago,
                        NumOpe = request.NumeroOperacion,
                        NumDoc = request.NumeroDocumento
                    }, transaction);
            }
            else if (request.TipoDocumento == 2)
            {
                await connection.ExecuteAsync(@"
                    UPDATE CHO SET
                        CHO_PAG_PEN = ISNULL(CHO_PAG_PEN, 0) + @MontoAplicar,
                        CHO_SAL_PEN = @SaldoNuevo,
                        CHO_EST_FAC = CASE WHEN @SaldoNuevo <= 0 THEN 4 ELSE CHO_EST_FAC END,
                        CHO_FEC_PAG = @FechaPago,
                        CHO_FEC_MOD = GETDATE()
                    WHERE CHO_NUM_OPE = @NumOpe AND CHO_NUM_FAC = @NumDoc",
                    new
                    {
                        MontoAplicar = montoAplicar,
                        SaldoNuevo = saldoNuevo,
                        request.FechaPago,
                        NumOpe = request.NumeroOperacion,
                        NumDoc = request.NumeroDocumento
                    }, transaction);
            }

            // 6. Generar excedente si corresponde
            if (excedente.HasValue && excedente.Value > 0)
            {
                // Verificar si existe tabla EXC
                var existeExc = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EXC'",
                    transaction: transaction);

                if (existeExc > 0)
                {
                    await connection.ExecuteAsync(@"
                        INSERT INTO EXC (
                            EXC_RUT_CLI, EXC_NUM_OPE, EXC_NUM_PAG, EXC_MON_EXC,
                            EXC_EST_EXC, EXC_FEC_GEN, EXC_USR_CRE, EXC_FEC_CRE
                        ) VALUES (
                            @RutCliente, @NumeroOperacion, @NumeroPago, @MontoExcedente,
                            1, @FechaGeneracion, @Usuario, GETDATE()
                        )",
                        new
                        {
                            request.RutCliente,
                            request.NumeroOperacion,
                            NumeroPago = numeroPago,
                            MontoExcedente = excedente.Value,
                            FechaGeneracion = DateTime.Today,
                            request.Usuario
                        }, transaction);
                }
            }

            // 7. Actualizar cupo disponible de línea si documento cancelado
            if (documentoCancelado)
            {
                // Liberar la diferencia de precios que estaba retenida
                var operacion = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT OPE_RUT_CLI, OPE_NUM_LIN, OPE_TIP_DOC FROM OPE WHERE OPE_NUM_OPE = @NumOpe",
                    new { NumOpe = request.NumeroOperacion }, transaction);

                if (operacion != null)
                {
                    // Calcular diferencia de precios del documento
                    decimal montoDoc = await connection.ExecuteScalarAsync<decimal>($@"
                        SELECT {(request.TipoDocumento == 1 ? "FAO_MON_FAC" : "CHO_MON_FAC")}
                        FROM {tablaDoc}
                        WHERE {(request.TipoDocumento == 1 ? "FAO" : "CHO")}_NUM_OPE = @NumOpe
                          AND {(request.TipoDocumento == 1 ? "FAO" : "CHO")}_NUM_FAC = @NumDoc",
                        new { NumOpe = request.NumeroOperacion, NumDoc = request.NumeroDocumento },
                        transaction);

                    decimal montoAnt = await connection.ExecuteScalarAsync<decimal>($@"
                        SELECT ISNULL({(request.TipoDocumento == 1 ? "FAO_MON_ANT" : "CHO_MON_ANT")}, 0)
                        FROM {tablaDoc}
                        WHERE {(request.TipoDocumento == 1 ? "FAO" : "CHO")}_NUM_OPE = @NumOpe
                          AND {(request.TipoDocumento == 1 ? "FAO" : "CHO")}_NUM_FAC = @NumDoc",
                        new { NumOpe = request.NumeroOperacion, NumDoc = request.NumeroDocumento },
                        transaction);

                    decimal diferenciaPrecio = montoDoc - montoAnt;

                    if (diferenciaPrecio > 0)
                    {
                        // Liberar diferencia de precio como excedente
                        var existeExc = await connection.ExecuteScalarAsync<int>(
                            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EXC'",
                            transaction: transaction);

                        if (existeExc > 0)
                        {
                            await connection.ExecuteAsync(@"
                                INSERT INTO EXC (
                                    EXC_RUT_CLI, EXC_NUM_OPE, EXC_NUM_PAG, EXC_MON_EXC,
                                    EXC_TIP_EXC, EXC_EST_EXC, EXC_FEC_GEN, EXC_USR_CRE, EXC_FEC_CRE
                                ) VALUES (
                                    @RutCliente, @NumeroOperacion, @NumeroPago, @MontoExcedente,
                                    2, 1, @FechaGeneracion, @Usuario, GETDATE()
                                )",
                                new
                                {
                                    request.RutCliente,
                                    request.NumeroOperacion,
                                    NumeroPago = numeroPago,
                                    MontoExcedente = diferenciaPrecio,
                                    FechaGeneracion = DateTime.Today,
                                    request.Usuario
                                }, transaction);
                        }
                    }
                }
            }

            // 8. Registrar movimiento de tesorería
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
                        @NumeroOperacion, @RutCliente, @FechaMov, 'I',
                        @MontoMov, @Descripcion, 1, GETDATE()
                    )",
                    new
                    {
                        request.NumeroOperacion,
                        request.RutCliente,
                        FechaMov = request.FechaPago,
                        MontoMov = montoAplicar,
                        Descripcion = $"Pago documento {request.NumeroDocumento}"
                    }, transaction);
            }

            transaction.Commit();

            return new RegistroPagoResult
            {
                Exito = true,
                NumeroPago = numeroPago,
                Mensaje = documentoCancelado
                    ? $"Documento cancelado completamente. Pago registrado: ${montoAplicar:N0}"
                    : $"Pago parcial registrado: ${montoAplicar:N0}. Saldo pendiente: ${saldoNuevo:N0}",
                SaldoAnterior = saldoActual,
                MontoPagado = montoAplicar,
                SaldoNuevo = saldoNuevo,
                MontoExcedente = excedente,
                DocumentoCancelado = documentoCancelado
            };
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return new RegistroPagoResult
            {
                Exito = false,
                Mensaje = $"Error al registrar pago: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene excedentes pendientes de un cliente
    /// </summary>
    public async Task<IEnumerable<ExcedenteCliente>> GetExcedentesPendientesAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Verificar si existe tabla EXC
        var existeExc = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EXC'");

        if (existeExc == 0)
            return Enumerable.Empty<ExcedenteCliente>();

        const string sql = @"
            SELECT
                E.EXC_NUM_EXC as NumeroExcedente,
                E.EXC_RUT_CLI as RutCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                E.EXC_NUM_OPE as NumeroOperacion,
                E.EXC_NUM_PAG as NumeroPago,
                E.EXC_MON_EXC as MontoExcedente,
                E.EXC_EST_EXC as EstadoExcedente,
                E.EXC_FEC_GEN as FechaGeneracion,
                E.EXC_FEC_APL as FechaAplicacion
            FROM EXC E
            INNER JOIN CLI C ON E.EXC_RUT_CLI = C.CLI_RUT_CLI
            WHERE E.EXC_RUT_CLI = @RutCliente AND E.EXC_EST_EXC = 1
            ORDER BY E.EXC_FEC_GEN DESC";

        return await connection.QueryAsync<ExcedenteCliente>(sql, new { RutCliente = rutCliente });
    }

    /// <summary>
    /// Aplica excedente a nueva operación
    /// </summary>
    public async Task<(bool Exito, string Mensaje)> AplicarExcedenteAsync(
        int numeroExcedente, int numeroOperacion, string usuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Obtener excedente
            var excedente = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT EXC_MON_EXC as Monto, EXC_RUT_CLI as RutCliente, EXC_EST_EXC as Estado
                FROM EXC WHERE EXC_NUM_EXC = @NumeroExcedente",
                new { NumeroExcedente = numeroExcedente }, transaction);

            if (excedente == null)
                return (false, "Excedente no encontrado");

            if ((int)excedente.Estado != 1)
                return (false, "El excedente ya fue utilizado");

            // Aplicar a operación (reducir monto a girar)
            await connection.ExecuteAsync(@"
                UPDATE OPE SET
                    OPE_MON_EXC = ISNULL(OPE_MON_EXC, 0) + @MontoExcedente,
                    OPE_TOT_GIR = OPE_TOT_GIR - @MontoExcedente,
                    OPE_FEC_MOD = GETDATE()
                WHERE OPE_NUM_OPE = @NumeroOperacion",
                new { MontoExcedente = (decimal)excedente.Monto, NumeroOperacion = numeroOperacion },
                transaction);

            // Marcar excedente como aplicado
            await connection.ExecuteAsync(@"
                UPDATE EXC SET
                    EXC_EST_EXC = 2,
                    EXC_OPE_APL = @NumeroOperacion,
                    EXC_FEC_APL = GETDATE(),
                    EXC_USR_MOD = @Usuario
                WHERE EXC_NUM_EXC = @NumeroExcedente",
                new { NumeroOperacion = numeroOperacion, Usuario = usuario, NumeroExcedente = numeroExcedente },
                transaction);

            transaction.Commit();
            return (true, $"Excedente de ${(decimal)excedente.Monto:N0} aplicado a operación {numeroOperacion}");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return (false, $"Error al aplicar excedente: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene resumen de cartera por cliente
    /// </summary>
    public async Task<ResumenCarteraCliente> GetResumenCarteraClienteAsync(long rutCliente)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                @RutCliente as RutCliente,
                C.CLI_RAZ_SOC as NombreCliente,
                COUNT(*) as CantidadDocumentos,
                SUM(F.FAO_MON_FAC) as MontoTotal,
                SUM(ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC)) as SaldoTotal,
                SUM(CASE WHEN F.FAO_FEC_VTO >= GETDATE() THEN 1 ELSE 0 END) as DocumentosVigentes,
                SUM(CASE WHEN F.FAO_FEC_VTO < GETDATE() THEN 1 ELSE 0 END) as DocumentosMorosos,
                SUM(CASE WHEN F.FAO_FEC_VTO < GETDATE() THEN ISNULL(F.FAO_SAL_PEN, F.FAO_MON_FAC) ELSE 0 END) as MontoMoroso
            FROM FAO F
            INNER JOIN CLI C ON F.FAO_RUT_CLI = C.CLI_RUT_CLI
            WHERE F.FAO_RUT_CLI = @RutCliente AND F.FAO_EST_FAC NOT IN (4, 7)
            GROUP BY C.CLI_RAZ_SOC";

        var resumen = await connection.QueryFirstOrDefaultAsync<ResumenCarteraCliente>(sql, new { RutCliente = rutCliente });

        if (resumen == null)
        {
            return new ResumenCarteraCliente
            {
                RutCliente = rutCliente,
                NombreCliente = "Sin documentos"
            };
        }

        // Obtener excedentes disponibles
        var existeExc = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EXC'");

        if (existeExc > 0)
        {
            resumen.ExcedentesDisponibles = await connection.ExecuteScalarAsync<decimal>(@"
                SELECT ISNULL(SUM(EXC_MON_EXC), 0) FROM EXC
                WHERE EXC_RUT_CLI = @RutCliente AND EXC_EST_EXC = 1",
                new { RutCliente = rutCliente });
        }

        return resumen;
    }

    /// <summary>
    /// Actualiza estado de documentos morosos
    /// </summary>
    public async Task<int> ActualizarDocumentosMorososAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // Marcar como morosos documentos vencidos hace más de X días
        var rowsFacturas = await connection.ExecuteAsync(@"
            UPDATE FAO SET FAO_EST_FAC = 5
            WHERE FAO_FEC_VTO < DATEADD(DAY, -30, GETDATE())
              AND FAO_EST_FAC = 1
              AND ISNULL(FAO_SAL_PEN, FAO_MON_FAC) > 0");

        var rowsCheques = await connection.ExecuteAsync(@"
            UPDATE CHO SET CHO_EST_FAC = 5
            WHERE CHO_FEC_VTO < DATEADD(DAY, -30, GETDATE())
              AND CHO_EST_FAC = 1
              AND ISNULL(CHO_SAL_PEN, CHO_MON_FAC) > 0");

        return rowsFacturas + rowsCheques;
    }
}
