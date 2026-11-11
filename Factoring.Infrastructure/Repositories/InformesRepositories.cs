using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;
using Factoring.Infrastructure.Data;

namespace Factoring.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de informes
    /// Basado en consultas de F_INF*.frm
    /// </summary>
    public class InformesRepository : IInformesRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public InformesRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        #region Definiciones de Informes

        public async Task<IEnumerable<DefinicionInforme>> GetInformesDisponiblesAsync()
        {
            // Retorna lista estática de informes disponibles basada en F_INF1000.frm
            var informes = new List<DefinicionInforme>
            {
                new() { Id = 1, Codigo = "INF1100", Nombre = "Anticipos Cursados", Descripcion = "Informe de anticipos cursados por período", Tipo = TipoInformeEnum.AnticiposCursados, Categoria = "Operaciones", RequiereFechas = true, RequiereCliente = false, PermiteExcel = true, PermitePDF = true, Activo = true, Orden = 1 },
                new() { Id = 2, Codigo = "INF1200", Nombre = "Protestos Diarios", Descripcion = "Informe diario de protestos por ejecutivo", Tipo = TipoInformeEnum.ProtestosDiarios, Categoria = "Cobranza", RequiereFechas = true, RequiereEjecutivo = true, PermiteExcel = true, PermitePDF = true, Activo = true, Orden = 2 },
                new() { Id = 3, Codigo = "INF1300", Nombre = "Documentos por Deudor", Descripcion = "Informe de documentos comprados por deudor", Tipo = TipoInformeEnum.DocumentosPorDeudor, Categoria = "Cartera", RequiereDeudor = true, RequiereTipoDocumento = true, PermiteExcel = true, PermitePDF = true, Activo = true, Orden = 3 },
                new() { Id = 4, Codigo = "INF1400", Nombre = "Cartera Cliente", Descripcion = "Informe de cartera por cliente", Tipo = TipoInformeEnum.CarteraCliente, Categoria = "Cartera", RequiereCliente = true, RequiereTipoDocumento = true, PermiteExcel = true, PermitePDF = true, Activo = true, Orden = 4 },
                new() { Id = 5, Codigo = "INF1500", Nombre = "Detalle Devengamiento", Descripcion = "Informe de detalle de devengamiento de intereses", Tipo = TipoInformeEnum.DetalleDevengamiento, Categoria = "Finanzas", RequiereFechas = true, RequiereCliente = false, PermiteExcel = true, PermitePDF = true, Activo = true, Orden = 5 },
                new() { Id = 6, Codigo = "INF1600", Nombre = "Adelantos", Descripcion = "Informe de adelantos otorgados", Tipo = TipoInformeEnum.Adelantos, Categoria = "Operaciones", RequiereFechas = true, RequiereCliente = false, PermiteExcel = true, PermitePDF = true, Activo = true, Orden = 6 },
                new() { Id = 7, Codigo = "INF1700", Nombre = "Cupo Cliente por Ejecutivo", Descripcion = "Informe de cupos de clientes por ejecutivo", Tipo = TipoInformeEnum.CupoCliente, Categoria = "Comercial", RequiereEjecutivo = true, PermiteExcel = true, PermitePDF = true, Activo = true, Orden = 7 },
                new() { Id = 8, Codigo = "INF1800", Nombre = "Volumen de Operaciones", Descripcion = "Informe de volumen de operaciones por período", Tipo = TipoInformeEnum.VolumenOperaciones, Categoria = "Estadísticas", RequiereFechas = true, PermiteExcel = true, PermitePDF = true, Activo = true, Orden = 8 },
                new() { Id = 9, Codigo = "INF1900", Nombre = "Detalle Excedentes", Descripcion = "Informe de detalle de excedentes", Tipo = TipoInformeEnum.Excedentes, Categoria = "Comercial", RequiereFechas = true, RequiereCliente = false, PermiteExcel = true, PermitePDF = true, Activo = true, Orden = 9 },
                new() { Id = 10, Codigo = "INF2000", Nombre = "Concentración Terceros", Descripcion = "Informe de concentración por terceros/deudores", Tipo = TipoInformeEnum.ConcentracionTerceros, Categoria = "Riesgo", RequiereFechas = true, PermiteExcel = true, PermitePDF = true, Activo = true, Orden = 10 },
                new() { Id = 11, Codigo = "INF_GEST", Nombre = "Informe de Gestión", Descripcion = "Informe de gestión comercial", Tipo = TipoInformeEnum.InformeGestion, Categoria = "Gestión", RequiereFechas = true, PermiteExcel = true, PermitePDF = true, Activo = true, Orden = 11 },
                new() { Id = 12, Codigo = "INF_PROT", Nombre = "Protestos por Cliente", Descripcion = "Informe de protestos agrupados por cliente", Tipo = TipoInformeEnum.ProtestosCliente, Categoria = "Cobranza", RequiereFechas = true, RequiereCliente = true, PermiteExcel = true, PermitePDF = true, Activo = true, Orden = 12 }
            };
            
            return await Task.FromResult(informes);
        }

        public async Task<DefinicionInforme?> GetInformePorTipoAsync(TipoInformeEnum tipo)
        {
            var informes = await GetInformesDisponiblesAsync();
            return informes.FirstOrDefault(i => i.Tipo == tipo);
        }

        #endregion

        #region Anticipos Cursados (F_INF1100)

        public async Task<IEnumerable<InformeAnticipoCursado>> GetAnticiposCursadosAsync(
            DateTime? fechaDesde, DateTime? fechaHasta, int? tipoDocumento = null, long? rutCliente = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT 
                    o.OPE_RUT_CLI AS RutCliente,
                    c.CLI_DIG_VER AS DvCliente,
                    c.CLI_RAZ_SOC AS RazonSocialCliente,
                    o.OPE_NUM_OPE AS NumeroOperacion,
                    o.OPE_FEC_OPE AS FechaOperacion,
                    o.OPE_FEC_CUR AS FechaCurse,
                    ISNULL(p.PAR_DES_PAR, 'N/A') AS TipoDocumento,
                    o.OPE_CAN_DOC AS CantidadDocumentos,
                    o.OPE_MON_TOT AS MontoDocumentos,
                    o.OPE_MON_ANT AS MontoAnticipo,
                    o.OPE_TAS_OPE AS TasaOperacion,
                    o.OPE_MON_DIF AS Diferencia,
                    o.OPE_COM_OPE AS Comision,
                    o.OPE_GAS_OPE AS Gastos,
                    ISNULL(e.EJE_NOM_EJE, 'N/A') AS NombreEjecutivo,
                    ISNULL(o.OPE_COD_BCO, 0) AS CodigoBanco,
                    ISNULL(b.BCO_DES_BCO, 'N/A') AS NombreBanco
                FROM OPE o
                INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
                LEFT JOIN PAR p ON o.OPE_TIP_DOC = p.PAR_COD_PAR AND p.PAR_COD_TAB = 1
                LEFT JOIN EJE e ON c.CLI_COD_EJE = e.EJE_COD_EJE
                LEFT JOIN BCO b ON o.OPE_COD_BCO = b.BCO_COD_BCO
                WHERE o.OPE_FEC_CUR BETWEEN @FechaDesde AND @FechaHasta
                  AND o.OPE_EST_OPE IN (3, 4) -- Cursadas/Cerradas
                  AND (@TipoDocumento IS NULL OR o.OPE_TIP_DOC = @TipoDocumento)
                  AND (@RutCliente IS NULL OR o.OPE_RUT_CLI = @RutCliente)
                ORDER BY o.OPE_FEC_CUR DESC, o.OPE_NUM_OPE";

            return await connection.QueryAsync<InformeAnticipoCursado>(sql, new 
            { 
                FechaDesde = fechaDesde, 
                FechaHasta = fechaHasta,
                TipoDocumento = tipoDocumento,
                RutCliente = rutCliente
            });
        }

        #endregion

        #region Protestos Diarios (F_INF1200)

        public async Task<IEnumerable<InformeProtestoDiario>> GetProtestosDiariosAsync(
            DateTime? fechaDesde, DateTime? fechaHasta, int? codigoEjecutivo = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT 
                    d.DOC_FEC_PRO AS FechaProtesto,
                    ISNULL(e.EJE_COD_EJE, 0) AS CodigoEjecutivo,
                    ISNULL(e.EJE_NOM_EJE, 'N/A') AS NombreEjecutivo,
                    o.OPE_RUT_CLI AS RutCliente,
                    c.CLI_DIG_VER AS DvCliente,
                    c.CLI_RAZ_SOC AS RazonSocialCliente,
                    d.DOC_RUT_DEU AS RutDeudor,
                    ISNULL(de.DEU_DIG_VER, '0') AS DvDeudor,
                    ISNULL(de.DEU_RAZ_SOC, 'N/A') AS NombreDeudor,
                    o.OPE_NUM_OPE AS NumeroOperacion,
                    d.DOC_NUM_DOC AS NumeroDocumento,
                    ISNULL(p.PAR_DES_PAR, 'N/A') AS TipoDocumento,
                    d.DOC_MON_DOC AS MontoDocumento,
                    d.DOC_FEC_VEN AS FechaVencimiento,
                    DATEDIFF(DAY, d.DOC_FEC_VEN, GETDATE()) AS DiasVencido,
                    ISNULL(d.DOC_CAU_PRO, 'Sin Información') AS CausalProtesto
                FROM DOC d
                INNER JOIN OPE o ON d.DOC_NUM_OPE = o.OPE_NUM_OPE
                INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
                LEFT JOIN DEU de ON d.DOC_RUT_DEU = de.DEU_RUT_DEU AND o.OPE_RUT_CLI = de.DEU_RUT_CLI
                LEFT JOIN EJE e ON c.CLI_COD_EJE = e.EJE_COD_EJE
                LEFT JOIN PAR p ON o.OPE_TIP_DOC = p.PAR_COD_PAR AND p.PAR_COD_TAB = 1
                WHERE d.DOC_FEC_PRO BETWEEN @FechaDesde AND @FechaHasta
                  AND d.DOC_EST_PRO = 1 -- Protestado
                  AND (@CodigoEjecutivo IS NULL OR e.EJE_COD_EJE = @CodigoEjecutivo)
                ORDER BY d.DOC_FEC_PRO DESC, e.EJE_NOM_EJE, o.OPE_RUT_CLI";

            return await connection.QueryAsync<InformeProtestoDiario>(sql, new 
            { 
                FechaDesde = fechaDesde, 
                FechaHasta = fechaHasta,
                CodigoEjecutivo = codigoEjecutivo
            });
        }

        #endregion

        #region Documentos por Deudor (F_INF1300)

        public async Task<IEnumerable<InformeDocumentoDeudor>> GetDocumentosPorDeudorAsync(
            long rutDeudor, int? tipoDocumento = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT 
                    d.DOC_RUT_DEU AS RutDeudor,
                    ISNULL(de.DEU_DIG_VER, '0') AS DvDeudor,
                    ISNULL(de.DEU_RAZ_SOC, 'N/A') AS NombreDeudor,
                    o.OPE_RUT_CLI AS RutCliente,
                    c.CLI_DIG_VER AS DvCliente,
                    c.CLI_RAZ_SOC AS RazonSocialCliente,
                    o.OPE_NUM_OPE AS NumeroOperacion,
                    d.DOC_NUM_DOC AS NumeroDocumento,
                    ISNULL(p.PAR_DES_PAR, 'N/A') AS TipoDocumento,
                    o.OPE_FEC_OPE AS FechaOperacion,
                    d.DOC_FEC_VEN AS FechaVencimiento,
                    d.DOC_MON_DOC AS MontoDocumento,
                    d.DOC_SAL_DOC AS SaldoDocumento,
                    CASE d.DOC_EST_DOC 
                        WHEN 0 THEN 'Vigente'
                        WHEN 1 THEN 'Vencido'
                        WHEN 2 THEN 'Pagado'
                        WHEN 3 THEN 'Protestado'
                        ELSE 'Otro'
                    END AS EstadoDocumento,
                    CASE 
                        WHEN d.DOC_FEC_VEN < GETDATE() THEN DATEDIFF(DAY, d.DOC_FEC_VEN, GETDATE())
                        ELSE 0
                    END AS DiasVencido
                FROM DOC d
                INNER JOIN OPE o ON d.DOC_NUM_OPE = o.OPE_NUM_OPE
                INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
                LEFT JOIN DEU de ON d.DOC_RUT_DEU = de.DEU_RUT_DEU AND o.OPE_RUT_CLI = de.DEU_RUT_CLI
                LEFT JOIN PAR p ON o.OPE_TIP_DOC = p.PAR_COD_PAR AND p.PAR_COD_TAB = 1
                WHERE d.DOC_RUT_DEU = @RutDeudor
                  AND (@TipoDocumento IS NULL OR o.OPE_TIP_DOC = @TipoDocumento)
                ORDER BY o.OPE_FEC_OPE DESC, d.DOC_NUM_DOC";

            return await connection.QueryAsync<InformeDocumentoDeudor>(sql, new 
            { 
                RutDeudor = rutDeudor,
                TipoDocumento = tipoDocumento
            });
        }

        #endregion

        #region Cartera Cliente (F_INF1400)

        public async Task<InformeCarteraCliente?> GetCarteraClienteAsync(
            long rutCliente, int? tipoDocumento = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sqlResumen = @"
                SELECT 
                    c.CLI_RUT_CLI AS RutCliente,
                    c.CLI_DIG_VER AS DvCliente,
                    c.CLI_RAZ_SOC AS RazonSocialCliente,
                    ISNULL(e.EJE_NOM_EJE, 'N/A') AS NombreEjecutivo,
                    ISNULL(p.PAR_DES_PAR, 'Todos') AS TipoDocumento,
                    COUNT(d.DOC_NUM_DOC) AS TotalDocumentos,
                    SUM(CASE WHEN d.DOC_FEC_VEN >= GETDATE() THEN d.DOC_SAL_DOC ELSE 0 END) AS MontoVigente,
                    SUM(CASE WHEN d.DOC_FEC_VEN < GETDATE() THEN d.DOC_SAL_DOC ELSE 0 END) AS MontoMora,
                    SUM(d.DOC_SAL_DOC) AS MontoTotal,
                    0 AS PorcentajeConcentracion
                FROM CLI c
                LEFT JOIN OPE o ON c.CLI_RUT_CLI = o.OPE_RUT_CLI
                LEFT JOIN DOC d ON o.OPE_NUM_OPE = d.DOC_NUM_OPE AND d.DOC_SAL_DOC > 0
                LEFT JOIN EJE e ON c.CLI_COD_EJE = e.EJE_COD_EJE
                LEFT JOIN PAR p ON o.OPE_TIP_DOC = p.PAR_COD_PAR AND p.PAR_COD_TAB = 1
                WHERE c.CLI_RUT_CLI = @RutCliente
                  AND (@TipoDocumento IS NULL OR o.OPE_TIP_DOC = @TipoDocumento)
                GROUP BY c.CLI_RUT_CLI, c.CLI_DIG_VER, c.CLI_RAZ_SOC, e.EJE_NOM_EJE, p.PAR_DES_PAR";

            var cartera = await connection.QueryFirstOrDefaultAsync<InformeCarteraCliente>(sqlResumen, new 
            { 
                RutCliente = rutCliente,
                TipoDocumento = tipoDocumento
            });

            if (cartera != null)
            {
                cartera.Detalle = (await GetDetalleCarteraAsync(rutCliente, tipoDocumento)).ToList();
            }

            return cartera;
        }

        public async Task<IEnumerable<DetalleCarteraCliente>> GetDetalleCarteraAsync(
            long rutCliente, int? tipoDocumento = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT 
                    o.OPE_NUM_OPE AS NumeroOperacion,
                    d.DOC_NUM_DOC AS NumeroDocumento,
                    ISNULL(p.PAR_DES_PAR, 'N/A') AS TipoDocumento,
                    d.DOC_RUT_DEU AS RutDeudor,
                    ISNULL(de.DEU_RAZ_SOC, 'N/A') AS NombreDeudor,
                    o.OPE_FEC_OPE AS FechaOperacion,
                    d.DOC_FEC_VEN AS FechaVencimiento,
                    d.DOC_MON_DOC AS MontoOriginal,
                    d.DOC_SAL_DOC AS Saldo,
                    CASE 
                        WHEN d.DOC_FEC_VEN < GETDATE() THEN DATEDIFF(DAY, d.DOC_FEC_VEN, GETDATE())
                        ELSE 0
                    END AS DiasVencido,
                    CASE d.DOC_EST_DOC 
                        WHEN 0 THEN 'Vigente'
                        WHEN 1 THEN 'Vencido'
                        WHEN 2 THEN 'Pagado'
                        WHEN 3 THEN 'Protestado'
                        ELSE 'Otro'
                    END AS Estado
                FROM DOC d
                INNER JOIN OPE o ON d.DOC_NUM_OPE = o.OPE_NUM_OPE
                LEFT JOIN DEU de ON d.DOC_RUT_DEU = de.DEU_RUT_DEU AND o.OPE_RUT_CLI = de.DEU_RUT_CLI
                LEFT JOIN PAR p ON o.OPE_TIP_DOC = p.PAR_COD_PAR AND p.PAR_COD_TAB = 1
                WHERE o.OPE_RUT_CLI = @RutCliente
                  AND d.DOC_SAL_DOC > 0
                  AND (@TipoDocumento IS NULL OR o.OPE_TIP_DOC = @TipoDocumento)
                ORDER BY d.DOC_FEC_VEN";

            return await connection.QueryAsync<DetalleCarteraCliente>(sql, new 
            { 
                RutCliente = rutCliente,
                TipoDocumento = tipoDocumento
            });
        }

        #endregion

        #region Devengamiento (F_INF1500)

        public async Task<IEnumerable<InformeDevengamiento>> GetDevengamientoAsync(
            DateTime? fechaCorte, long? rutCliente = null, int? centroCosto = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT 
                    @FechaCorte AS FechaCorte,
                    o.OPE_RUT_CLI AS RutCliente,
                    c.CLI_DIG_VER AS DvCliente,
                    c.CLI_RAZ_SOC AS RazonSocialCliente,
                    o.OPE_NUM_OPE AS NumeroOperacion,
                    d.DOC_NUM_DOC AS NumeroDocumento,
                    o.OPE_FEC_OPE AS FechaOperacion,
                    d.DOC_FEC_VEN AS FechaVencimiento,
                    DATEDIFF(DAY, o.OPE_FEC_OPE, d.DOC_FEC_VEN) AS PlazoTotal,
                    CASE 
                        WHEN @FechaCorte >= d.DOC_FEC_VEN THEN DATEDIFF(DAY, o.OPE_FEC_OPE, d.DOC_FEC_VEN)
                        WHEN @FechaCorte <= o.OPE_FEC_OPE THEN 0
                        ELSE DATEDIFF(DAY, o.OPE_FEC_OPE, @FechaCorte)
                    END AS DiasDevengados,
                    CASE 
                        WHEN @FechaCorte >= d.DOC_FEC_VEN THEN 0
                        WHEN @FechaCorte <= o.OPE_FEC_OPE THEN DATEDIFF(DAY, o.OPE_FEC_OPE, d.DOC_FEC_VEN)
                        ELSE DATEDIFF(DAY, @FechaCorte, d.DOC_FEC_VEN)
                    END AS DiasPorDevengar,
                    d.DOC_MON_DOC AS MontoDocumento,
                    o.OPE_TAS_OPE AS TasaOperacion,
                    (d.DOC_MON_DOC * o.OPE_TAS_OPE * DATEDIFF(DAY, o.OPE_FEC_OPE, d.DOC_FEC_VEN) / 36000.0) AS InteresTotal,
                    (d.DOC_MON_DOC * o.OPE_TAS_OPE * 
                        CASE 
                            WHEN @FechaCorte >= d.DOC_FEC_VEN THEN DATEDIFF(DAY, o.OPE_FEC_OPE, d.DOC_FEC_VEN)
                            WHEN @FechaCorte <= o.OPE_FEC_OPE THEN 0
                            ELSE DATEDIFF(DAY, o.OPE_FEC_OPE, @FechaCorte)
                        END / 36000.0) AS InteresDevengado,
                    (d.DOC_MON_DOC * o.OPE_TAS_OPE * 
                        CASE 
                            WHEN @FechaCorte >= d.DOC_FEC_VEN THEN 0
                            WHEN @FechaCorte <= o.OPE_FEC_OPE THEN DATEDIFF(DAY, o.OPE_FEC_OPE, d.DOC_FEC_VEN)
                            ELSE DATEDIFF(DAY, @FechaCorte, d.DOC_FEC_VEN)
                        END / 36000.0) AS InteresPorDevengar,
                    ISNULL(o.OPE_CEN_COS, 0) AS CentroCosto,
                    ISNULL(cc.PAR_DES_PAR, 'N/A') AS DescripcionCentroCosto
                FROM DOC d
                INNER JOIN OPE o ON d.DOC_NUM_OPE = o.OPE_NUM_OPE
                INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
                LEFT JOIN PAR cc ON o.OPE_CEN_COS = cc.PAR_COD_PAR AND cc.PAR_COD_TAB = 50
                WHERE d.DOC_SAL_DOC > 0
                  AND o.OPE_FEC_OPE <= @FechaCorte
                  AND d.DOC_FEC_VEN >= o.OPE_FEC_OPE
                  AND (@RutCliente IS NULL OR o.OPE_RUT_CLI = @RutCliente)
                  AND (@CentroCosto IS NULL OR o.OPE_CEN_COS = @CentroCosto)
                ORDER BY o.OPE_RUT_CLI, o.OPE_NUM_OPE, d.DOC_FEC_VEN";

            return await connection.QueryAsync<InformeDevengamiento>(sql, new 
            { 
                FechaCorte = fechaCorte,
                RutCliente = rutCliente,
                CentroCosto = centroCosto
            });
        }

        #endregion

        #region Adelantos (F_INF1600)

        public async Task<IEnumerable<InformeAdelanto>> GetAdelantosAsync(
            DateTime? fechaDesde = null, DateTime? fechaHasta = null, long? rutCliente = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT 
                    o.OPE_RUT_CLI AS RutCliente,
                    c.CLI_DIG_VER AS DvCliente,
                    c.CLI_RAZ_SOC AS RazonSocialCliente,
                    o.OPE_NUM_OPE AS NumeroOperacion,
                    o.OPE_FEC_OPE AS FechaOperacion,
                    o.OPE_FEC_CUR AS FechaAdelanto,
                    o.OPE_MON_ANT AS MontoAdelanto,
                    o.OPE_MON_TOT AS MontoDocumentos,
                    CASE WHEN o.OPE_MON_TOT > 0 
                        THEN (o.OPE_MON_ANT / o.OPE_MON_TOT * 100) 
                        ELSE 0 
                    END AS PorcentajeAdelanto,
                    CASE o.OPE_EST_OPE
                        WHEN 1 THEN 'Ingresada'
                        WHEN 2 THEN 'Aprobada'
                        WHEN 3 THEN 'Cursada'
                        WHEN 4 THEN 'Cerrada'
                        ELSE 'Otro'
                    END AS Estado,
                    NULL AS FechaRecuperacion,
                    NULL AS MontoRecuperado
                FROM OPE o
                INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
                WHERE o.OPE_MON_ANT > 0
                  AND (@FechaDesde IS NULL OR o.OPE_FEC_OPE >= @FechaDesde)
                  AND (@FechaHasta IS NULL OR o.OPE_FEC_OPE <= @FechaHasta)
                  AND (@RutCliente IS NULL OR o.OPE_RUT_CLI = @RutCliente)
                ORDER BY o.OPE_FEC_OPE DESC";

            return await connection.QueryAsync<InformeAdelanto>(sql, new 
            { 
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                RutCliente = rutCliente
            });
        }

        #endregion

        #region Cupo Cliente (F_INF1700)

        public async Task<IEnumerable<InformeCupoCliente>> GetCupoClientesAsync(
            int? codigoEjecutivo = null, long? rutCliente = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT 
                    ISNULL(e.EJE_COD_EJE, 0) AS CodigoEjecutivo,
                    ISNULL(e.EJE_NOM_EJE, 'N/A') AS NombreEjecutivo,
                    c.CLI_RUT_CLI AS RutCliente,
                    c.CLI_DIG_VER AS DvCliente,
                    c.CLI_RAZ_SOC AS RazonSocialCliente,
                    ISNULL(l.LIC_MON_LIN, 0) AS LineaAprobada,
                    ISNULL(
                        (SELECT SUM(d.DOC_SAL_DOC) 
                         FROM OPE o2 
                         INNER JOIN DOC d ON o2.OPE_NUM_OPE = d.DOC_NUM_OPE 
                         WHERE o2.OPE_RUT_CLI = c.CLI_RUT_CLI AND d.DOC_SAL_DOC > 0), 
                        0) AS MontoUtilizado,
                    ISNULL(l.LIC_MON_LIN, 0) - 
                        ISNULL((SELECT SUM(d.DOC_SAL_DOC) 
                                FROM OPE o2 
                                INNER JOIN DOC d ON o2.OPE_NUM_OPE = d.DOC_NUM_OPE 
                                WHERE o2.OPE_RUT_CLI = c.CLI_RUT_CLI AND d.DOC_SAL_DOC > 0), 0) AS MontoDisponible,
                    CASE WHEN ISNULL(l.LIC_MON_LIN, 0) > 0 
                        THEN (ISNULL((SELECT SUM(d.DOC_SAL_DOC) 
                                      FROM OPE o2 
                                      INNER JOIN DOC d ON o2.OPE_NUM_OPE = d.DOC_NUM_OPE 
                                      WHERE o2.OPE_RUT_CLI = c.CLI_RUT_CLI AND d.DOC_SAL_DOC > 0), 0) 
                              / l.LIC_MON_LIN * 100)
                        ELSE 0 
                    END AS PorcentajeUtilizacion,
                    l.LIC_FEC_VEN AS FechaVencimientoLinea,
                    CASE l.LIC_EST_LIN
                        WHEN 0 THEN 'Activa'
                        WHEN 1 THEN 'Suspendida'
                        WHEN 2 THEN 'Vencida'
                        ELSE 'N/A'
                    END AS EstadoLinea
                FROM CLI c
                LEFT JOIN EJE e ON c.CLI_COD_EJE = e.EJE_COD_EJE
                LEFT JOIN LIC l ON c.CLI_RUT_CLI = l.LIC_RUT_CLI AND l.LIC_EST_LIN = 0
                WHERE c.CLI_EST_CLI = 0
                  AND (@CodigoEjecutivo IS NULL OR e.EJE_COD_EJE = @CodigoEjecutivo)
                  AND (@RutCliente IS NULL OR c.CLI_RUT_CLI = @RutCliente)
                ORDER BY e.EJE_NOM_EJE, c.CLI_RAZ_SOC";

            return await connection.QueryAsync<InformeCupoCliente>(sql, new 
            { 
                CodigoEjecutivo = codigoEjecutivo,
                RutCliente = rutCliente
            });
        }

        #endregion

        #region Volumen Operaciones (F_INF1800)

        public async Task<IEnumerable<InformeVolumenOperaciones>> GetVolumenOperacionesAsync(
            int anio, int? mes = null, int? codigoEjecutivo = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT 
                    YEAR(o.OPE_FEC_OPE) AS Anio,
                    MONTH(o.OPE_FEC_OPE) AS Mes,
                    CASE MONTH(o.OPE_FEC_OPE)
                        WHEN 1 THEN 'Enero' WHEN 2 THEN 'Febrero' WHEN 3 THEN 'Marzo'
                        WHEN 4 THEN 'Abril' WHEN 5 THEN 'Mayo' WHEN 6 THEN 'Junio'
                        WHEN 7 THEN 'Julio' WHEN 8 THEN 'Agosto' WHEN 9 THEN 'Septiembre'
                        WHEN 10 THEN 'Octubre' WHEN 11 THEN 'Noviembre' WHEN 12 THEN 'Diciembre'
                    END AS NombreMes,
                    ISNULL(e.EJE_COD_EJE, 0) AS CodigoEjecutivo,
                    ISNULL(e.EJE_NOM_EJE, 'Sin Asignar') AS NombreEjecutivo,
                    COUNT(DISTINCT o.OPE_NUM_OPE) AS CantidadOperaciones,
                    SUM(o.OPE_CAN_DOC) AS CantidadDocumentos,
                    SUM(o.OPE_MON_TOT) AS MontoOperado,
                    SUM(o.OPE_MON_ANT) AS MontoAnticipado,
                    SUM(o.OPE_COM_OPE) AS ComisionesGeneradas,
                    SUM(o.OPE_INT_OPE) AS InteresesGenerados,
                    0 AS ClientesNuevos,
                    COUNT(DISTINCT o.OPE_RUT_CLI) AS ClientesActivos
                FROM OPE o
                INNER JOIN CLI c ON o.OPE_RUT_CLI = c.CLI_RUT_CLI
                LEFT JOIN EJE e ON c.CLI_COD_EJE = e.EJE_COD_EJE
                WHERE YEAR(o.OPE_FEC_OPE) = @Anio
                  AND o.OPE_EST_OPE IN (3, 4) -- Cursadas/Cerradas
                  AND (@Mes IS NULL OR MONTH(o.OPE_FEC_OPE) = @Mes)
                  AND (@CodigoEjecutivo IS NULL OR e.EJE_COD_EJE = @CodigoEjecutivo)
                GROUP BY YEAR(o.OPE_FEC_OPE), MONTH(o.OPE_FEC_OPE), e.EJE_COD_EJE, e.EJE_NOM_EJE
                ORDER BY Anio, Mes, NombreEjecutivo";

            return await connection.QueryAsync<InformeVolumenOperaciones>(sql, new 
            { 
                Anio = anio,
                Mes = mes,
                CodigoEjecutivo = codigoEjecutivo
            });
        }

        #endregion

        #region Excedentes (F_INF1900)

        public async Task<IEnumerable<InformeExcedente>> GetExcedentesAsync(
            DateTime? fechaDesde = null, DateTime? fechaHasta = null, long? rutCliente = null, string? estado = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT 
                    ex.EXC_RUT_CLI AS RutCliente,
                    c.CLI_DIG_VER AS DvCliente,
                    c.CLI_RAZ_SOC AS RazonSocialCliente,
                    ex.EXC_NUM_OPE AS NumeroOperacion,
                    o.OPE_FEC_OPE AS FechaOperacion,
                    ex.EXC_MON_EXC AS MontoExcedente,
                    ex.EXC_FEC_GEN AS FechaGeneracion,
                    ex.EXC_FEC_DEV AS FechaDevolucion,
                    ex.EXC_MON_DEV AS MontoDevuelto,
                    CASE ex.EXC_EST_EXC
                        WHEN 0 THEN 'Pendiente'
                        WHEN 1 THEN 'Devuelto'
                        WHEN 2 THEN 'Aplicado'
                        ELSE 'Otro'
                    END AS Estado,
                    CASE ex.EXC_FOR_PAG
                        WHEN 1 THEN 'Cheque'
                        WHEN 2 THEN 'Transferencia'
                        WHEN 3 THEN 'Aplicación'
                        ELSE 'N/A'
                    END AS FormaPago,
                    ISNULL(e.EJE_NOM_EJE, 'N/A') AS NombreEjecutivo
                FROM EXC ex
                INNER JOIN CLI c ON ex.EXC_RUT_CLI = c.CLI_RUT_CLI
                LEFT JOIN OPE o ON ex.EXC_NUM_OPE = o.OPE_NUM_OPE
                LEFT JOIN EJE e ON c.CLI_COD_EJE = e.EJE_COD_EJE
                WHERE (@FechaDesde IS NULL OR ex.EXC_FEC_GEN >= @FechaDesde)
                  AND (@FechaHasta IS NULL OR ex.EXC_FEC_GEN <= @FechaHasta)
                  AND (@RutCliente IS NULL OR ex.EXC_RUT_CLI = @RutCliente)
                  AND (@Estado IS NULL OR 
                       (@Estado = 'Pendiente' AND ex.EXC_EST_EXC = 0) OR
                       (@Estado = 'Devuelto' AND ex.EXC_EST_EXC = 1) OR
                       (@Estado = 'Aplicado' AND ex.EXC_EST_EXC = 2))
                ORDER BY ex.EXC_FEC_GEN DESC";

            return await connection.QueryAsync<InformeExcedente>(sql, new 
            { 
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                RutCliente = rutCliente,
                Estado = estado
            });
        }

        #endregion

        #region Concentración Terceros (F_INF2000)

        public async Task<IEnumerable<InformeConcentracionTerceros>> GetConcentracionTercerosAsync(
            DateTime? fechaCorte = null, long? rutDeudor = null, decimal? porcentajeMinimo = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                WITH CarteraTotal AS (
                    SELECT SUM(d.DOC_SAL_DOC) AS TotalCartera
                    FROM DOC d
                    WHERE d.DOC_SAL_DOC > 0
                ),
                ConcentracionDeudor AS (
                    SELECT 
                        d.DOC_RUT_DEU AS RutDeudor,
                        COUNT(DISTINCT o.OPE_RUT_CLI) AS CantidadClientes,
                        COUNT(d.DOC_NUM_DOC) AS CantidadDocumentos,
                        SUM(d.DOC_SAL_DOC) AS MontoTotal,
                        SUM(CASE WHEN d.DOC_FEC_VEN >= GETDATE() THEN d.DOC_SAL_DOC ELSE 0 END) AS MontoVigente,
                        SUM(CASE WHEN d.DOC_FEC_VEN < GETDATE() THEN d.DOC_SAL_DOC ELSE 0 END) AS MontoVencido
                    FROM DOC d
                    INNER JOIN OPE o ON d.DOC_NUM_OPE = o.OPE_NUM_OPE
                    WHERE d.DOC_SAL_DOC > 0
                      AND (@RutDeudor IS NULL OR d.DOC_RUT_DEU = @RutDeudor)
                    GROUP BY d.DOC_RUT_DEU
                )
                SELECT 
                    cd.RutDeudor,
                    ISNULL(de.DEU_DIG_VER, '0') AS DvDeudor,
                    ISNULL(de.DEU_RAZ_SOC, 'N/A') AS NombreDeudor,
                    cd.CantidadClientes,
                    cd.CantidadDocumentos,
                    cd.MontoTotal,
                    cd.MontoVigente,
                    cd.MontoVencido,
                    CASE WHEN ct.TotalCartera > 0 
                        THEN (cd.MontoTotal / ct.TotalCartera * 100) 
                        ELSE 0 
                    END AS PorcentajeConcentracion,
                    NULL AS CustodiaAsignada
                FROM ConcentracionDeudor cd
                CROSS JOIN CarteraTotal ct
                LEFT JOIN DEU de ON cd.RutDeudor = de.DEU_RUT_DEU
                WHERE (@PorcentajeMinimo IS NULL OR 
                       (ct.TotalCartera > 0 AND (cd.MontoTotal / ct.TotalCartera * 100) >= @PorcentajeMinimo))
                ORDER BY cd.MontoTotal DESC";

            return await connection.QueryAsync<InformeConcentracionTerceros>(sql, new 
            { 
                RutDeudor = rutDeudor,
                PorcentajeMinimo = porcentajeMinimo
            });
        }

        #endregion

        #region Resumen y Estadísticas

        public async Task<ResumenInformes> GetResumenGeneralAsync(DateTime? fechaCorte = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var fecha = fechaCorte ?? DateTime.Today;
            var primerDiaMes = new DateTime(fecha.Year, fecha.Month, 1);
            
            var sql = @"
                SELECT 
                    @FechaCorte AS FechaCorte,
                    (SELECT COUNT(*) FROM OPE WHERE OPE_FEC_OPE >= @PrimerDiaMes AND OPE_FEC_OPE <= @FechaCorte AND OPE_EST_OPE IN (3,4)) AS TotalOperacionesMes,
                    (SELECT ISNULL(SUM(OPE_MON_TOT), 0) FROM OPE WHERE OPE_FEC_OPE >= @PrimerDiaMes AND OPE_FEC_OPE <= @FechaCorte AND OPE_EST_OPE IN (3,4)) AS MontoOperadoMes,
                    (SELECT COUNT(DISTINCT OPE_RUT_CLI) FROM OPE WHERE OPE_FEC_OPE >= DATEADD(MONTH, -3, @FechaCorte)) AS TotalClientesActivos,
                    (SELECT ISNULL(SUM(DOC_SAL_DOC), 0) FROM DOC WHERE DOC_SAL_DOC > 0 AND DOC_FEC_VEN >= @FechaCorte) AS CarteraVigente,
                    (SELECT ISNULL(SUM(DOC_SAL_DOC), 0) FROM DOC WHERE DOC_SAL_DOC > 0 AND DOC_FEC_VEN < @FechaCorte) AS CarteraVencida,
                    CASE 
                        WHEN (SELECT SUM(DOC_SAL_DOC) FROM DOC WHERE DOC_SAL_DOC > 0) > 0 
                        THEN ((SELECT SUM(DOC_SAL_DOC) FROM DOC WHERE DOC_SAL_DOC > 0 AND DOC_FEC_VEN < @FechaCorte) / 
                              (SELECT SUM(DOC_SAL_DOC) FROM DOC WHERE DOC_SAL_DOC > 0) * 100)
                        ELSE 0 
                    END AS PorcentajeMorosidad,
                    (SELECT COUNT(*) FROM DOC WHERE DOC_EST_PRO = 1 AND DOC_FEC_PRO >= @PrimerDiaMes) AS DocumentosProtestados,
                    (SELECT ISNULL(SUM(DOC_MON_DOC), 0) FROM DOC WHERE DOC_EST_PRO = 1 AND DOC_FEC_PRO >= @PrimerDiaMes) AS MontoProtestado,
                    (SELECT ISNULL(SUM(EXC_MON_EXC), 0) FROM EXC WHERE EXC_EST_EXC = 0) AS ExcedentesPendientes";

            return await connection.QueryFirstAsync<ResumenInformes>(sql, new 
            { 
                FechaCorte = fecha,
                PrimerDiaMes = primerDiaMes
            });
        }

        #endregion

        #region Historial

        public async Task<int> RegistrarHistorialAsync(HistorialInforme historial)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                INSERT INTO HIS_INF (
                    HIS_TIP_INF, HIS_NOM_INF, HIS_FEC_GEN, HIS_USR_GEN,
                    HIS_PAR_JSON, HIS_FOR_INF, HIS_TOT_REG, HIS_TIE_EJE,
                    HIS_EXI_INF, HIS_MSG_ERR
                ) VALUES (
                    @TipoInforme, @NombreInforme, @FechaGeneracion, @UsuarioGeneracion,
                    @ParametrosJson, @Formato, @TotalRegistros, @TiempoEjecucionMs,
                    @Exitoso, @MensajeError
                );
                SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<int>(sql, historial);
        }

        public async Task<IEnumerable<HistorialInforme>> GetHistorialAsync(
            DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? usuario = null, int? tipoInforme = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = @"
                SELECT 
                    HIS_ID AS Id,
                    HIS_TIP_INF AS TipoInforme,
                    HIS_NOM_INF AS NombreInforme,
                    HIS_FEC_GEN AS FechaGeneracion,
                    HIS_USR_GEN AS UsuarioGeneracion,
                    HIS_PAR_JSON AS ParametrosJson,
                    HIS_FOR_INF AS Formato,
                    HIS_TOT_REG AS TotalRegistros,
                    HIS_TIE_EJE AS TiempoEjecucionMs,
                    HIS_EXI_INF AS Exitoso,
                    HIS_MSG_ERR AS MensajeError
                FROM HIS_INF
                WHERE (@FechaDesde IS NULL OR HIS_FEC_GEN >= @FechaDesde)
                  AND (@FechaHasta IS NULL OR HIS_FEC_GEN <= @FechaHasta)
                  AND (@Usuario IS NULL OR HIS_USR_GEN = @Usuario)
                  AND (@TipoInforme IS NULL OR HIS_TIP_INF = @TipoInforme)
                ORDER BY HIS_FEC_GEN DESC";

            return await connection.QueryAsync<HistorialInforme>(sql, new 
            { 
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                Usuario = usuario,
                TipoInforme = tipoInforme
            });
        }

        #endregion
    }
}
