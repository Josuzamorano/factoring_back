using Microsoft.AspNetCore.Mvc;
using Factoring.Application.Services;
using ClosedXML.Excel;
using Swashbuckle.AspNetCore.Annotations;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using System.Globalization;

namespace Factoring.API.Controllers;

/// <summary>
/// Controlador para reportes, dashboard y exportaciones Excel
/// Incluye metricas del sistema, cartera, operaciones, lineas de credito y pagos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Reportes, dashboard ejecutivo y exportacion a Excel")]
public class ReportesController : ControllerBase
{
    private readonly ExportacionService _exportacionService;

    public ReportesController(ExportacionService exportacionService)
    {
        _exportacionService = exportacionService;
    }

    // ==========================================
    // DASHBOARD
    // ==========================================

    /// <summary>
    /// Obtiene metricas completas del dashboard ejecutivo
    /// </summary>
    /// <remarks>
    /// Retorna KPIs principales del sistema:
    ///
    /// **Cartera:**
    /// - Total cartera, cartera vigente, cartera morosa
    /// - Porcentaje de morosidad
    ///
    /// **Operaciones del mes:**
    /// - Cantidad de operaciones
    /// - Monto total operado
    /// - Intereses y comisiones generadas
    ///
    /// **Lineas de credito:**
    /// - Lineas activas
    /// - Cupo total, utilizado y disponible
    ///
    /// **Recaudacion:**
    /// - Recaudacion del mes
    /// - Cantidad de pagos
    /// - Excedentes pendientes
    ///
    /// **Alertas:**
    /// - Documentos proximos a vencer (7 dias)
    /// - Lineas proximas a vencer (30 dias)
    /// </remarks>
    /// <returns>Metricas del dashboard</returns>
    [HttpGet("dashboard")]
    [SwaggerOperation(Summary = "Dashboard ejecutivo", Description = "Retorna KPIs y metricas principales del sistema")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        var metricas = await _exportacionService.GetDashboardMetricasAsync();
        return Ok(metricas);
    }

    // ==========================================
    // DATOS PARA GRILLAS
    // ==========================================

    /// <summary>
    /// Obtiene datos de cartera con filtros
    /// </summary>
    /// <remarks>
    /// Retorna listado de documentos en cartera.
    /// Filtros disponibles: cliente, fechas, morosos, vigentes.
    /// Usar endpoint /cartera/excel para exportar a Excel.
    /// </remarks>
    /// <param name="rutCliente">Filtrar por RUT de cliente</param>
    /// <param name="fechaDesde">Fecha inicial</param>
    /// <param name="fechaHasta">Fecha final</param>
    /// <param name="soloMorosos">Solo documentos morosos</param>
    /// <param name="soloVigentes">Solo documentos vigentes</param>
    /// <returns>Lista de documentos en cartera</returns>
    [HttpGet("cartera")]
    [SwaggerOperation(Summary = "Consultar cartera", Description = "Lista documentos en cartera con filtros")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCartera(
        [FromQuery] long? rutCliente = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] bool soloMorosos = false,
        [FromQuery] bool soloVigentes = false)
    {
        var datos = await _exportacionService.GetCarteraParaExportarAsync(
            rutCliente, fechaDesde, fechaHasta, soloMorosos, soloVigentes);
        return Ok(datos);
    }

    /// <summary>
    /// Obtiene datos de operaciones
    /// </summary>
    [HttpGet("operaciones")]
    public async Task<IActionResult> GetOperaciones(
        [FromQuery] long? rutCliente = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] int? estado = null)
    {
        var datos = await _exportacionService.GetOperacionesParaExportarAsync(
            rutCliente, fechaDesde, fechaHasta, estado);
        return Ok(datos);
    }

    /// <summary>
    /// Obtiene datos de lineas de credito
    /// </summary>
    [HttpGet("lineas-credito")]
    public async Task<IActionResult> GetLineasCredito(
        [FromQuery] long? rutCliente = null,
        [FromQuery] bool soloActivas = false)
    {
        var datos = await _exportacionService.GetLineasCreditoParaExportarAsync(rutCliente, soloActivas);
        return Ok(datos);
    }

    /// <summary>
    /// Obtiene datos de pagos
    /// </summary>
    [HttpGet("pagos")]
    public async Task<IActionResult> GetPagos(
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] long? rutCliente = null)
    {
        var datos = await _exportacionService.GetPagosParaExportarAsync(fechaDesde, fechaHasta, rutCliente);
        return Ok(datos);
    }

    // ==========================================
    // EXPORTACIONES A EXCEL
    // ==========================================

    /// <summary>
    /// Exporta cartera a archivo Excel
    /// </summary>
    /// <remarks>
    /// Genera archivo Excel (.xlsx) con datos de cartera.
    /// Incluye: Nro Operacion, Tipo/Nro Documento, Cliente, Deudor,
    /// Fechas, Montos, Estado, Ejecutivo.
    /// El archivo se descarga automaticamente.
    /// </remarks>
    /// <param name="rutCliente">Filtrar por RUT de cliente</param>
    /// <param name="fechaDesde">Fecha inicial</param>
    /// <param name="fechaHasta">Fecha final</param>
    /// <param name="soloMorosos">Solo documentos morosos</param>
    /// <param name="soloVigentes">Solo documentos vigentes</param>
    /// <returns>Archivo Excel</returns>
    [HttpGet("cartera/excel")]
    [SwaggerOperation(Summary = "Exportar cartera a Excel", Description = "Descarga archivo Excel con datos de cartera")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportCarteraExcel(
        [FromQuery] long? rutCliente = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] bool soloMorosos = false,
        [FromQuery] bool soloVigentes = false)
    {
        var datos = await _exportacionService.GetCarteraParaExportarAsync(
            rutCliente, fechaDesde, fechaHasta, soloMorosos, soloVigentes);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Cartera");

        // Encabezados
        var headers = new[] { "Nro Op", "Tipo Doc", "Nro Doc", "RUT Cliente", "Cliente",
            "RUT Deudor", "Deudor", "Fecha Op", "Vencimiento", "Dias Vencido",
            "Monto Doc", "Anticipado", "Saldo", "Estado", "Ejecutivo" };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Datos
        int row = 2;
        foreach (var item in datos)
        {
            worksheet.Cell(row, 1).Value = item.NumeroOperacion;
            worksheet.Cell(row, 2).Value = item.TipoDocumento;
            worksheet.Cell(row, 3).Value = item.NumeroDocumento;
            worksheet.Cell(row, 4).Value = item.RutCliente;
            worksheet.Cell(row, 5).Value = item.NombreCliente;
            worksheet.Cell(row, 6).Value = item.RutDeudor;
            worksheet.Cell(row, 7).Value = item.NombreDeudor;
            worksheet.Cell(row, 8).Value = item.FechaOperacion;
            worksheet.Cell(row, 9).Value = item.FechaVencimiento;
            worksheet.Cell(row, 10).Value = item.DiasVencido;
            worksheet.Cell(row, 11).Value = item.MontoDocumento;
            worksheet.Cell(row, 12).Value = item.MontoAnticipado;
            worksheet.Cell(row, 13).Value = item.SaldoPendiente;
            worksheet.Cell(row, 14).Value = item.Estado;
            worksheet.Cell(row, 15).Value = item.Ejecutivo;
            row++;
        }

        // Formato de columnas numericas
        worksheet.Column(11).Style.NumberFormat.Format = "#,##0";
        worksheet.Column(12).Style.NumberFormat.Format = "#,##0";
        worksheet.Column(13).Style.NumberFormat.Format = "#,##0";
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Cartera_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    /// <summary>
    /// Exporta operaciones a Excel
    /// </summary>
    [HttpGet("operaciones/excel")]
    public async Task<IActionResult> ExportOperacionesExcel(
        [FromQuery] long? rutCliente = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] int? estado = null)
    {
        var datos = await _exportacionService.GetOperacionesParaExportarAsync(
            rutCliente, fechaDesde, fechaHasta, estado);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Operaciones");

        // Encabezados
        var headers = new[] { "Nro Op", "Fecha", "RUT Cliente", "Cliente", "Nro Linea",
            "Tipo Op", "Monto Docs", "Anticipo", "Intereses", "Comision",
            "Gastos", "IVA", "Giro", "Estado", "Ejecutivo" };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Datos
        int row = 2;
        foreach (var item in datos)
        {
            worksheet.Cell(row, 1).Value = item.NumeroOperacion;
            worksheet.Cell(row, 2).Value = item.FechaOperacion;
            worksheet.Cell(row, 3).Value = item.RutCliente;
            worksheet.Cell(row, 4).Value = item.NombreCliente;
            worksheet.Cell(row, 5).Value = item.NumeroLinea;
            worksheet.Cell(row, 6).Value = item.TipoOperacionNombre;
            worksheet.Cell(row, 7).Value = item.MontoDocumentos;
            worksheet.Cell(row, 8).Value = item.MontoAnticipo;
            worksheet.Cell(row, 9).Value = item.MontoIntereses;
            worksheet.Cell(row, 10).Value = item.MontoComision;
            worksheet.Cell(row, 11).Value = item.MontoGastos;
            worksheet.Cell(row, 12).Value = item.MontoIVA;
            worksheet.Cell(row, 13).Value = item.MontoGiro;
            worksheet.Cell(row, 14).Value = item.Estado;
            worksheet.Cell(row, 15).Value = item.Ejecutivo;
            row++;
        }

        // Formato de columnas numericas
        for (int col = 7; col <= 13; col++)
        {
            worksheet.Column(col).Style.NumberFormat.Format = "#,##0";
        }
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Operaciones_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    /// <summary>
    /// Exporta lineas de credito a Excel
    /// </summary>
    [HttpGet("lineas-credito/excel")]
    public async Task<IActionResult> ExportLineasCreditoExcel(
        [FromQuery] long? rutCliente = null,
        [FromQuery] bool soloActivas = false)
    {
        var datos = await _exportacionService.GetLineasCreditoParaExportarAsync(rutCliente, soloActivas);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Lineas de Credito");

        // Encabezados
        var headers = new[] { "Nro Linea", "RUT Cliente", "Cliente", "Fecha Aprobacion",
            "Vencimiento", "Dias p/Vencer", "Monto Linea", "Cupo Utilizado",
            "Cupo Disponible", "% Utilizacion", "Estado", "Ejecutivo" };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Datos
        int row = 2;
        foreach (var item in datos)
        {
            worksheet.Cell(row, 1).Value = item.NumeroLinea;
            worksheet.Cell(row, 2).Value = item.RutCliente;
            worksheet.Cell(row, 3).Value = item.NombreCliente;
            worksheet.Cell(row, 4).Value = item.FechaAprobacion;
            worksheet.Cell(row, 5).Value = item.FechaVencimiento;
            worksheet.Cell(row, 6).Value = item.DiasParaVencer;
            worksheet.Cell(row, 7).Value = item.MontoLinea;
            worksheet.Cell(row, 8).Value = item.CupoUtilizado;
            worksheet.Cell(row, 9).Value = item.CupoDisponible;
            worksheet.Cell(row, 10).Value = item.PorcentajeUtilizacion;
            worksheet.Cell(row, 11).Value = item.Estado;
            worksheet.Cell(row, 12).Value = item.Ejecutivo;
            row++;
        }

        // Formato
        worksheet.Column(7).Style.NumberFormat.Format = "#,##0";
        worksheet.Column(8).Style.NumberFormat.Format = "#,##0";
        worksheet.Column(9).Style.NumberFormat.Format = "#,##0";
        worksheet.Column(10).Style.NumberFormat.Format = "0.00%";
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"LineasCredito_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    /// <summary>
    /// Exporta pagos a Excel
    /// </summary>
    [HttpGet("pagos/excel")]
    public async Task<IActionResult> ExportPagosExcel(
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] long? rutCliente = null)
    {
        var datos = await _exportacionService.GetPagosParaExportarAsync(fechaDesde, fechaHasta, rutCliente);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Pagos");

        // Encabezados
        var headers = new[] { "Nro Pago", "Fecha", "Tipo Doc", "Nro Doc", "Nro Op",
            "RUT Cliente", "Cliente", "RUT Deudor", "Deudor", "Monto Pago",
            "Saldo Anterior", "Saldo Nuevo", "Tipo Pago", "Estado" };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Datos
        int row = 2;
        foreach (var item in datos)
        {
            worksheet.Cell(row, 1).Value = item.NumeroPago;
            worksheet.Cell(row, 2).Value = item.FechaPago;
            worksheet.Cell(row, 3).Value = item.TipoDocumento;
            worksheet.Cell(row, 4).Value = item.NumeroDocumento;
            worksheet.Cell(row, 5).Value = item.NumeroOperacion;
            worksheet.Cell(row, 6).Value = item.RutCliente;
            worksheet.Cell(row, 7).Value = item.NombreCliente;
            worksheet.Cell(row, 8).Value = item.RutDeudor;
            worksheet.Cell(row, 9).Value = item.NombreDeudor;
            worksheet.Cell(row, 10).Value = item.MontoPago;
            worksheet.Cell(row, 11).Value = item.SaldoAnterior;
            worksheet.Cell(row, 12).Value = item.SaldoNuevo;
            worksheet.Cell(row, 13).Value = item.TipoPago;
            worksheet.Cell(row, 14).Value = item.Estado;
            row++;
        }

        // Formato
        worksheet.Column(10).Style.NumberFormat.Format = "#,##0";
        worksheet.Column(11).Style.NumberFormat.Format = "#,##0";
        worksheet.Column(12).Style.NumberFormat.Format = "#,##0";
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Pagos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    // ==========================================
    // CONCENTRACION DE CARTERA
    // ==========================================

    /// <summary>
    /// Obtiene analisis de concentracion por deudor
    /// </summary>
    /// <remarks>
    /// Retorna analisis de concentracion de cartera por deudor:
    /// - Clasificacion ABC (A=80%, B=15%, C=5%)
    /// - Indice Herfindahl-Hirschman (HHI)
    /// - Top 5, 10, 20 concentracion
    /// - Morosidad por deudor
    /// </remarks>
    [HttpGet("concentracion/deudor")]
    [SwaggerOperation(Summary = "Concentracion por deudor", Description = "Analisis de concentracion de cartera por deudor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConcentracionPorDeudor([FromQuery] long? rutCliente = null)
    {
        var (resumen, detalle) = await _exportacionService.GetConcentracionPorDeudorAsync(rutCliente);
        return Ok(new { resumen, detalle });
    }

    /// <summary>
    /// Obtiene analisis de concentracion por cliente
    /// </summary>
    /// <remarks>
    /// Retorna analisis de concentracion de cartera por cliente:
    /// - Clasificacion ABC
    /// - Indice HHI de concentracion
    /// - Utilizacion de linea de credito
    /// - Morosidad por cliente
    /// </remarks>
    [HttpGet("concentracion/cliente")]
    [SwaggerOperation(Summary = "Concentracion por cliente", Description = "Analisis de concentracion de cartera por cliente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConcentracionPorCliente()
    {
        var (resumen, detalle) = await _exportacionService.GetConcentracionPorClienteAsync();
        return Ok(new { resumen, detalle });
    }

    /// <summary>
    /// Exporta concentracion por deudor a Excel
    /// </summary>
    [HttpGet("concentracion/deudor/excel")]
    [SwaggerOperation(Summary = "Exportar concentracion deudor a Excel")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> ExportConcentracionDeudorExcel([FromQuery] long? rutCliente = null)
    {
        var (resumen, detalle) = await _exportacionService.GetConcentracionPorDeudorAsync(rutCliente);

        using var workbook = new XLWorkbook();

        // Hoja Resumen
        var wsResumen = workbook.Worksheets.Add("Resumen");
        wsResumen.Cell(1, 1).Value = "ANALISIS DE CONCENTRACION POR DEUDOR";
        wsResumen.Cell(1, 1).Style.Font.Bold = true;
        wsResumen.Cell(1, 1).Style.Font.FontSize = 14;

        wsResumen.Cell(3, 1).Value = "Cartera Total:";
        wsResumen.Cell(3, 2).Value = resumen.CarteraTotal;
        wsResumen.Cell(3, 2).Style.NumberFormat.Format = "$#,##0";

        wsResumen.Cell(4, 1).Value = "Total Deudores:";
        wsResumen.Cell(4, 2).Value = resumen.TotalDeudores;

        wsResumen.Cell(6, 1).Value = "CONCENTRACION TOP";
        wsResumen.Cell(6, 1).Style.Font.Bold = true;
        wsResumen.Cell(7, 1).Value = "Top 5:";
        wsResumen.Cell(7, 2).Value = resumen.Top5Porcentaje / 100;
        wsResumen.Cell(7, 2).Style.NumberFormat.Format = "0.00%";
        wsResumen.Cell(8, 1).Value = "Top 10:";
        wsResumen.Cell(8, 2).Value = resumen.Top10Porcentaje / 100;
        wsResumen.Cell(8, 2).Style.NumberFormat.Format = "0.00%";
        wsResumen.Cell(9, 1).Value = "Top 20:";
        wsResumen.Cell(9, 2).Value = resumen.Top20Porcentaje / 100;
        wsResumen.Cell(9, 2).Style.NumberFormat.Format = "0.00%";

        wsResumen.Cell(11, 1).Value = "INDICE HHI:";
        wsResumen.Cell(11, 2).Value = resumen.IndiceHHI;
        wsResumen.Cell(12, 1).Value = "Nivel Concentracion:";
        wsResumen.Cell(12, 2).Value = resumen.NivelConcentracion;

        wsResumen.Cell(14, 1).Value = "CLASIFICACION ABC";
        wsResumen.Cell(14, 1).Style.Font.Bold = true;
        wsResumen.Cell(15, 1).Value = "Clase A (80%):";
        wsResumen.Cell(15, 2).Value = $"{resumen.DeudoresClaseA} deudores";
        wsResumen.Cell(15, 3).Value = resumen.MontoClaseA;
        wsResumen.Cell(15, 3).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(16, 1).Value = "Clase B (15%):";
        wsResumen.Cell(16, 2).Value = $"{resumen.DeudoresClaseB} deudores";
        wsResumen.Cell(16, 3).Value = resumen.MontoClaseB;
        wsResumen.Cell(16, 3).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(17, 1).Value = "Clase C (5%):";
        wsResumen.Cell(17, 2).Value = $"{resumen.DeudoresClaseC} deudores";
        wsResumen.Cell(17, 3).Value = resumen.MontoClaseC;
        wsResumen.Cell(17, 3).Style.NumberFormat.Format = "$#,##0";

        wsResumen.Columns().AdjustToContents();

        // Hoja Detalle
        var wsDetalle = workbook.Worksheets.Add("Detalle Deudores");
        var headers = new[] { "RUT Deudor", "Nombre Deudor", "Cant. Docs", "Monto Total",
            "Saldo Pendiente", "% Cartera", "% Acumulado", "Clase", "Monto Moroso", "% Morosidad" };

        for (int i = 0; i < headers.Length; i++)
        {
            wsDetalle.Cell(1, i + 1).Value = headers[i];
            wsDetalle.Cell(1, i + 1).Style.Font.Bold = true;
            wsDetalle.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var item in detalle)
        {
            wsDetalle.Cell(row, 1).Value = item.RutDeudor;
            wsDetalle.Cell(row, 2).Value = item.NombreDeudor;
            wsDetalle.Cell(row, 3).Value = item.CantidadDocumentos;
            wsDetalle.Cell(row, 4).Value = item.MontoTotal;
            wsDetalle.Cell(row, 5).Value = item.SaldoPendiente;
            wsDetalle.Cell(row, 6).Value = item.PorcentajeCartera / 100;
            wsDetalle.Cell(row, 7).Value = item.PorcentajeAcumulado / 100;
            wsDetalle.Cell(row, 8).Value = item.Clasificacion;
            wsDetalle.Cell(row, 9).Value = item.MontoMoroso;
            wsDetalle.Cell(row, 10).Value = item.PorcentajeMorosidad / 100;

            // Color por clasificacion
            var color = item.Clasificacion == "A" ? XLColor.LightGreen :
                       item.Clasificacion == "B" ? XLColor.LightYellow : XLColor.LightCoral;
            wsDetalle.Cell(row, 8).Style.Fill.BackgroundColor = color;

            row++;
        }

        wsDetalle.Column(4).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(5).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(6).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Column(7).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Column(9).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(10).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Concentracion_Deudor_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    /// <summary>
    /// Exporta concentracion por cliente a Excel
    /// </summary>
    [HttpGet("concentracion/cliente/excel")]
    [SwaggerOperation(Summary = "Exportar concentracion cliente a Excel")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> ExportConcentracionClienteExcel()
    {
        var (resumen, detalle) = await _exportacionService.GetConcentracionPorClienteAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Concentracion Clientes");

        var headers = new[] { "RUT Cliente", "Nombre Cliente", "Cant. Docs", "Cant. Deudores",
            "Monto Cartera", "Saldo Pendiente", "% Cartera", "% Acumulado", "Clase",
            "Linea Credito", "Cupo Utilizado", "% Utilizacion", "Monto Moroso", "% Morosidad" };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var item in detalle)
        {
            worksheet.Cell(row, 1).Value = item.RutCliente;
            worksheet.Cell(row, 2).Value = item.NombreCliente;
            worksheet.Cell(row, 3).Value = item.CantidadDocumentos;
            worksheet.Cell(row, 4).Value = item.CantidadDeudores;
            worksheet.Cell(row, 5).Value = item.MontoCartera;
            worksheet.Cell(row, 6).Value = item.SaldoPendiente;
            worksheet.Cell(row, 7).Value = item.PorcentajeCartera / 100;
            worksheet.Cell(row, 8).Value = item.PorcentajeAcumulado / 100;
            worksheet.Cell(row, 9).Value = item.Clasificacion;
            worksheet.Cell(row, 10).Value = item.MontoLineaCredito;
            worksheet.Cell(row, 11).Value = item.CupoUtilizado;
            worksheet.Cell(row, 12).Value = item.PorcentajeUtilizacion / 100;
            worksheet.Cell(row, 13).Value = item.MontoMoroso;
            worksheet.Cell(row, 14).Value = item.PorcentajeMorosidad / 100;
            row++;
        }

        worksheet.Column(5).Style.NumberFormat.Format = "#,##0";
        worksheet.Column(6).Style.NumberFormat.Format = "#,##0";
        worksheet.Column(7).Style.NumberFormat.Format = "0.00%";
        worksheet.Column(8).Style.NumberFormat.Format = "0.00%";
        worksheet.Column(10).Style.NumberFormat.Format = "#,##0";
        worksheet.Column(11).Style.NumberFormat.Format = "#,##0";
        worksheet.Column(12).Style.NumberFormat.Format = "0.00%";
        worksheet.Column(13).Style.NumberFormat.Format = "#,##0";
        worksheet.Column(14).Style.NumberFormat.Format = "0.00%";
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Concentracion_Cliente_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    // ==========================================
    // RENTABILIDAD POR CLIENTE
    // ==========================================

    /// <summary>
    /// Obtiene analisis de rentabilidad por cliente
    /// </summary>
    /// <remarks>
    /// Retorna analisis de rentabilidad por cliente:
    /// - Ingresos: intereses, comisiones, gastos
    /// - Tasa promedio, plazo ponderado, ticket promedio
    /// - Morosidad y provision estimada
    /// - Rentabilidad sobre cartera (anualizada)
    /// - Clasificacion: Premium, Estandar, Bajo
    /// </remarks>
    [HttpGet("rentabilidad/cliente")]
    [SwaggerOperation(Summary = "Rentabilidad por cliente", Description = "Analisis de rentabilidad por cliente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRentabilidadPorCliente(
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        var (resumen, detalle) = await _exportacionService.GetRentabilidadPorClienteAsync(fechaDesde, fechaHasta);
        return Ok(new { resumen, detalle });
    }

    /// <summary>
    /// Exporta rentabilidad por cliente a Excel
    /// </summary>
    [HttpGet("rentabilidad/cliente/excel")]
    [SwaggerOperation(Summary = "Exportar rentabilidad a Excel")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> ExportRentabilidadClienteExcel(
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        var (resumen, detalle) = await _exportacionService.GetRentabilidadPorClienteAsync(fechaDesde, fechaHasta);

        using var workbook = new XLWorkbook();

        // Hoja Resumen
        var wsResumen = workbook.Worksheets.Add("Resumen");
        wsResumen.Cell(1, 1).Value = "ANALISIS DE RENTABILIDAD POR CLIENTE";
        wsResumen.Cell(1, 1).Style.Font.Bold = true;
        wsResumen.Cell(1, 1).Style.Font.FontSize = 14;

        wsResumen.Cell(3, 1).Value = $"Periodo: {resumen.FechaDesde:dd/MM/yyyy} - {resumen.FechaHasta:dd/MM/yyyy}";

        wsResumen.Cell(5, 1).Value = "INGRESOS TOTALES";
        wsResumen.Cell(5, 1).Style.Font.Bold = true;
        wsResumen.Cell(6, 1).Value = "Intereses:";
        wsResumen.Cell(6, 2).Value = resumen.TotalIntereses;
        wsResumen.Cell(6, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(7, 1).Value = "Comisiones:";
        wsResumen.Cell(7, 2).Value = resumen.TotalComisiones;
        wsResumen.Cell(7, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(8, 1).Value = "Gastos:";
        wsResumen.Cell(8, 2).Value = resumen.TotalGastos;
        wsResumen.Cell(8, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(9, 1).Value = "TOTAL INGRESOS:";
        wsResumen.Cell(9, 1).Style.Font.Bold = true;
        wsResumen.Cell(9, 2).Value = resumen.TotalIngresos;
        wsResumen.Cell(9, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(9, 2).Style.Font.Bold = true;

        wsResumen.Cell(11, 1).Value = "PROMEDIOS";
        wsResumen.Cell(11, 1).Style.Font.Bold = true;
        wsResumen.Cell(12, 1).Value = "Tasa Promedio:";
        wsResumen.Cell(12, 2).Value = resumen.TasaPromedioGeneral / 100;
        wsResumen.Cell(12, 2).Style.NumberFormat.Format = "0.00%";
        wsResumen.Cell(13, 1).Value = "Plazo Promedio:";
        wsResumen.Cell(13, 2).Value = $"{resumen.PlazoPromedioGeneral} dias";
        wsResumen.Cell(14, 1).Value = "Ticket Promedio:";
        wsResumen.Cell(14, 2).Value = resumen.TicketPromedioGeneral;
        wsResumen.Cell(14, 2).Style.NumberFormat.Format = "$#,##0";

        wsResumen.Cell(16, 1).Value = "CLASIFICACION CLIENTES";
        wsResumen.Cell(16, 1).Style.Font.Bold = true;
        wsResumen.Cell(17, 1).Value = "Premium (Top 20%):";
        wsResumen.Cell(17, 2).Value = $"{resumen.ClientesPremium} clientes";
        wsResumen.Cell(17, 3).Value = resumen.IngresosPremium;
        wsResumen.Cell(17, 3).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(18, 1).Value = "Estandar (60%):";
        wsResumen.Cell(18, 2).Value = $"{resumen.ClientesEstandar} clientes";
        wsResumen.Cell(18, 3).Value = resumen.IngresosEstandar;
        wsResumen.Cell(18, 3).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(19, 1).Value = "Bajo (Bottom 20%):";
        wsResumen.Cell(19, 2).Value = $"{resumen.ClientesBajo} clientes";
        wsResumen.Cell(19, 3).Value = resumen.IngresosBajo;
        wsResumen.Cell(19, 3).Style.NumberFormat.Format = "$#,##0";

        wsResumen.Columns().AdjustToContents();

        // Hoja Detalle
        var wsDetalle = workbook.Worksheets.Add("Detalle Clientes");
        var headers = new[] { "Ranking", "RUT", "Cliente", "Cant. Ops", "Monto Operado",
            "Intereses", "Comisiones", "Total Ingresos", "Tasa Prom.", "Plazo Prom.",
            "Monto Moroso", "% Morosidad", "Rentab. Anual.", "Clase" };

        for (int i = 0; i < headers.Length; i++)
        {
            wsDetalle.Cell(1, i + 1).Value = headers[i];
            wsDetalle.Cell(1, i + 1).Style.Font.Bold = true;
            wsDetalle.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var item in detalle)
        {
            wsDetalle.Cell(row, 1).Value = item.Ranking;
            wsDetalle.Cell(row, 2).Value = item.RutCliente;
            wsDetalle.Cell(row, 3).Value = item.NombreCliente;
            wsDetalle.Cell(row, 4).Value = item.CantidadOperaciones;
            wsDetalle.Cell(row, 5).Value = item.MontoOperado;
            wsDetalle.Cell(row, 6).Value = item.InteresesGanados;
            wsDetalle.Cell(row, 7).Value = item.ComisionesGanadas;
            wsDetalle.Cell(row, 8).Value = item.TotalIngresos;
            wsDetalle.Cell(row, 9).Value = item.TasaPromedioMensual / 100;
            wsDetalle.Cell(row, 10).Value = item.PlazoPonderadoPromedio;
            wsDetalle.Cell(row, 11).Value = item.MontoMoroso;
            wsDetalle.Cell(row, 12).Value = item.PorcentajeMorosidad / 100;
            wsDetalle.Cell(row, 13).Value = item.RentabilidadAnualizada / 100;
            wsDetalle.Cell(row, 14).Value = item.Clasificacion;

            // Color por clasificacion
            var color = item.Clasificacion == "Premium" ? XLColor.LightGreen :
                       item.Clasificacion == "Estandar" ? XLColor.LightYellow : XLColor.LightCoral;
            wsDetalle.Cell(row, 14).Style.Fill.BackgroundColor = color;

            row++;
        }

        wsDetalle.Column(5).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(6).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(7).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(8).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(9).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Column(11).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(12).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Column(13).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Rentabilidad_Cliente_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    // ==========================================
    // EVOLUCION DE CARTERA MENSUAL
    // ==========================================

    /// <summary>
    /// Obtiene evolucion de cartera mensual
    /// </summary>
    /// <remarks>
    /// Retorna analisis de evolucion mensual de la cartera:
    /// - Cartera total, vigente y morosa por mes
    /// - Operaciones nuevas y montos operados
    /// - Ingresos por intereses y comisiones
    /// - Recaudacion mensual
    /// - Variaciones mes a mes
    /// - Resumen con promedios y maximos/minimos
    ///
    /// Parametros:
    /// - fechaDesde/fechaHasta: Rango de fechas a analizar
    /// - meses: Alternativa, cantidad de meses hacia atras (default 12)
    /// </remarks>
    [HttpGet("evolucion-cartera")]
    [SwaggerOperation(Summary = "Evolucion de cartera mensual", Description = "Analisis de evolucion mensual de la cartera")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvolucionCarteraMensual(
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] int? meses = null)
    {
        var (resumen, detalle) = await _exportacionService.GetEvolucionCarteraMensualAsync(fechaDesde, fechaHasta, meses);
        return Ok(new { resumen, detalle });
    }

    /// <summary>
    /// Exporta evolucion de cartera mensual a Excel
    /// </summary>
    [HttpGet("evolucion-cartera/excel")]
    [SwaggerOperation(Summary = "Exportar evolucion cartera a Excel")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> ExportEvolucionCarteraExcel(
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] int? meses = null)
    {
        var (resumen, detalle) = await _exportacionService.GetEvolucionCarteraMensualAsync(fechaDesde, fechaHasta, meses);

        using var workbook = new XLWorkbook();

        // Hoja Resumen
        var wsResumen = workbook.Worksheets.Add("Resumen");
        wsResumen.Cell(1, 1).Value = "EVOLUCION DE CARTERA MENSUAL";
        wsResumen.Cell(1, 1).Style.Font.Bold = true;
        wsResumen.Cell(1, 1).Style.Font.FontSize = 14;

        wsResumen.Cell(3, 1).Value = $"Periodo: {resumen.FechaDesde:dd/MM/yyyy} - {resumen.FechaHasta:dd/MM/yyyy}";
        wsResumen.Cell(4, 1).Value = $"Total meses analizados: {resumen.TotalMeses}";

        wsResumen.Cell(6, 1).Value = "EVOLUCION DE CARTERA";
        wsResumen.Cell(6, 1).Style.Font.Bold = true;
        wsResumen.Cell(7, 1).Value = "Cartera Inicial:";
        wsResumen.Cell(7, 2).Value = resumen.CarteraInicial;
        wsResumen.Cell(7, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(8, 1).Value = "Cartera Final:";
        wsResumen.Cell(8, 2).Value = resumen.CarteraFinal;
        wsResumen.Cell(8, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(9, 1).Value = "Crecimiento:";
        wsResumen.Cell(9, 2).Value = resumen.CrecimientoCartera;
        wsResumen.Cell(9, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(9, 3).Value = resumen.CrecimientoPorcentaje / 100;
        wsResumen.Cell(9, 3).Style.NumberFormat.Format = "0.00%";
        if (resumen.CrecimientoCartera >= 0)
            wsResumen.Cell(9, 2).Style.Font.FontColor = XLColor.DarkGreen;
        else
            wsResumen.Cell(9, 2).Style.Font.FontColor = XLColor.Red;

        wsResumen.Cell(11, 1).Value = "PROMEDIOS MENSUALES";
        wsResumen.Cell(11, 1).Style.Font.Bold = true;
        wsResumen.Cell(12, 1).Value = "Cartera Promedio:";
        wsResumen.Cell(12, 2).Value = resumen.CarteraPromedio;
        wsResumen.Cell(12, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(13, 1).Value = "Morosidad Promedio:";
        wsResumen.Cell(13, 2).Value = resumen.MorosidadPromedio / 100;
        wsResumen.Cell(13, 2).Style.NumberFormat.Format = "0.00%";
        wsResumen.Cell(14, 1).Value = "Operaciones/Mes:";
        wsResumen.Cell(14, 2).Value = resumen.OperacionesMesPromedio;
        wsResumen.Cell(15, 1).Value = "Ingresos Mensuales:";
        wsResumen.Cell(15, 2).Value = resumen.IngresoMensualPromedio;
        wsResumen.Cell(15, 2).Style.NumberFormat.Format = "$#,##0";

        wsResumen.Cell(17, 1).Value = "DESTACADOS";
        wsResumen.Cell(17, 1).Style.Font.Bold = true;
        wsResumen.Cell(18, 1).Value = "Mes Mayor Cartera:";
        wsResumen.Cell(18, 2).Value = resumen.MesMayorCartera;
        wsResumen.Cell(18, 3).Value = resumen.MayorCartera;
        wsResumen.Cell(18, 3).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(19, 1).Value = "Mes Menor Morosidad:";
        wsResumen.Cell(19, 2).Value = resumen.MesMenorMorosidad;
        wsResumen.Cell(19, 3).Value = resumen.MenorMorosidad / 100;
        wsResumen.Cell(19, 3).Style.NumberFormat.Format = "0.00%";
        wsResumen.Cell(20, 1).Value = "Mes Mayor Ingresos:";
        wsResumen.Cell(20, 2).Value = resumen.MesMayorIngreso;
        wsResumen.Cell(20, 3).Value = resumen.MayorIngreso;
        wsResumen.Cell(20, 3).Style.NumberFormat.Format = "$#,##0";

        wsResumen.Cell(22, 1).Value = "TOTALES DEL PERIODO";
        wsResumen.Cell(22, 1).Style.Font.Bold = true;
        wsResumen.Cell(23, 1).Value = "Total Operado:";
        wsResumen.Cell(23, 2).Value = resumen.TotalOperado;
        wsResumen.Cell(23, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(24, 1).Value = "Total Intereses:";
        wsResumen.Cell(24, 2).Value = resumen.TotalIntereses;
        wsResumen.Cell(24, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(25, 1).Value = "Total Comisiones:";
        wsResumen.Cell(25, 2).Value = resumen.TotalComisiones;
        wsResumen.Cell(25, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(26, 1).Value = "Total Recaudado:";
        wsResumen.Cell(26, 2).Value = resumen.TotalRecaudado;
        wsResumen.Cell(26, 2).Style.NumberFormat.Format = "$#,##0";

        wsResumen.Columns().AdjustToContents();

        // Hoja Detalle Mensual
        var wsDetalle = workbook.Worksheets.Add("Detalle Mensual");
        var headers = new[] { "Periodo", "Mes", "Cartera Total", "Cartera Vigente", "Cartera Morosa",
            "% Morosidad", "Total Docs", "Docs Vigentes", "Docs Morosos", "Ops. Nuevas",
            "Monto Operado", "Intereses", "Comisiones", "Total Ingresos", "Recaudacion",
            "Var. Cartera", "Var. %", "Var. Morosidad" };

        for (int i = 0; i < headers.Length; i++)
        {
            wsDetalle.Cell(1, i + 1).Value = headers[i];
            wsDetalle.Cell(1, i + 1).Style.Font.Bold = true;
            wsDetalle.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var item in detalle)
        {
            wsDetalle.Cell(row, 1).Value = item.Periodo;
            wsDetalle.Cell(row, 2).Value = item.NombreMes;
            wsDetalle.Cell(row, 3).Value = item.CarteraTotal;
            wsDetalle.Cell(row, 4).Value = item.CarteraVigente;
            wsDetalle.Cell(row, 5).Value = item.CarteraMorosa;
            wsDetalle.Cell(row, 6).Value = item.PorcentajeMorosidad / 100;
            wsDetalle.Cell(row, 7).Value = item.TotalDocumentos;
            wsDetalle.Cell(row, 8).Value = item.DocumentosVigentes;
            wsDetalle.Cell(row, 9).Value = item.DocumentosMorosos;
            wsDetalle.Cell(row, 10).Value = item.OperacionesNuevas;
            wsDetalle.Cell(row, 11).Value = item.MontoOperado;
            wsDetalle.Cell(row, 12).Value = item.InteresesGanados;
            wsDetalle.Cell(row, 13).Value = item.ComisionesGanadas;
            wsDetalle.Cell(row, 14).Value = item.TotalIngresos;
            wsDetalle.Cell(row, 15).Value = item.RecaudacionMes;
            wsDetalle.Cell(row, 16).Value = item.VariacionCartera;
            wsDetalle.Cell(row, 17).Value = item.VariacionCarteraPorcentaje / 100;
            wsDetalle.Cell(row, 18).Value = item.VariacionMorosidad / 100;

            // Color para variaciones
            if (item.VariacionCartera > 0)
                wsDetalle.Cell(row, 16).Style.Font.FontColor = XLColor.DarkGreen;
            else if (item.VariacionCartera < 0)
                wsDetalle.Cell(row, 16).Style.Font.FontColor = XLColor.Red;

            if (item.VariacionMorosidad < 0)
                wsDetalle.Cell(row, 18).Style.Font.FontColor = XLColor.DarkGreen;
            else if (item.VariacionMorosidad > 0)
                wsDetalle.Cell(row, 18).Style.Font.FontColor = XLColor.Red;

            row++;
        }

        // Formato de columnas
        wsDetalle.Column(3).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(4).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(5).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(6).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Column(11).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(12).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(13).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(14).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(15).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(16).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(17).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Column(18).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Columns().AdjustToContents();

        // Agregar grafico simple (tabla de tendencia)
        var wsTendencia = workbook.Worksheets.Add("Tendencia");
        wsTendencia.Cell(1, 1).Value = "TENDENCIA DE CARTERA";
        wsTendencia.Cell(1, 1).Style.Font.Bold = true;
        wsTendencia.Cell(1, 1).Style.Font.FontSize = 14;

        wsTendencia.Cell(3, 1).Value = "Periodo";
        wsTendencia.Cell(3, 2).Value = "Cartera";
        wsTendencia.Cell(3, 3).Value = "Tendencia Visual";
        wsTendencia.Row(3).Style.Font.Bold = true;

        row = 4;
        var maxCartera = detalle.Any() ? detalle.Max(d => d.CarteraTotal) : 1;
        foreach (var item in detalle)
        {
            wsTendencia.Cell(row, 1).Value = item.Periodo;
            wsTendencia.Cell(row, 2).Value = item.CarteraTotal;
            wsTendencia.Cell(row, 2).Style.NumberFormat.Format = "$#,##0";

            // Barra visual simple
            var porcentaje = maxCartera > 0 ? (int)((item.CarteraTotal / maxCartera) * 30) : 0;
            wsTendencia.Cell(row, 3).Value = new string('█', porcentaje);
            wsTendencia.Cell(row, 3).Style.Font.FontColor = XLColor.Blue;
            row++;
        }

        wsTendencia.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Evolucion_Cartera_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    // ==========================================
    // COMPARATIVO AÑO ANTERIOR
    // ==========================================

    /// <summary>
    /// Obtiene comparativo con año anterior
    /// </summary>
    /// <remarks>
    /// Compara métricas del año seleccionado con el año anterior:
    /// - Operaciones: cantidad y monto operado
    /// - Ingresos: intereses, comisiones, total
    /// - Cartera: promedio mensual y morosidad
    /// - Detalle mes a mes con variaciones
    /// - Identificación del mejor mes de cada año
    /// </remarks>
    [HttpGet("comparativo-anual")]
    [SwaggerOperation(Summary = "Comparativo año anterior", Description = "Compara métricas con el año anterior")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComparativoAnual([FromQuery] int? anio = null)
    {
        var (resumen, detalleMensual, metricas) = await _exportacionService.GetComparativoAnioAnteriorAsync(anio);
        return Ok(new { resumen, detalleMensual, metricas });
    }

    /// <summary>
    /// Exporta comparativo anual a Excel
    /// </summary>
    [HttpGet("comparativo-anual/excel")]
    [SwaggerOperation(Summary = "Exportar comparativo anual a Excel")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> ExportComparativoAnualExcel([FromQuery] int? anio = null)
    {
        var (resumen, detalleMensual, metricas) = await _exportacionService.GetComparativoAnioAnteriorAsync(anio);

        using var workbook = new XLWorkbook();

        // Hoja Resumen
        var wsResumen = workbook.Worksheets.Add("Resumen");
        wsResumen.Cell(1, 1).Value = $"COMPARATIVO {resumen.AnioActual} vs {resumen.AnioAnterior}";
        wsResumen.Cell(1, 1).Style.Font.Bold = true;
        wsResumen.Cell(1, 1).Style.Font.FontSize = 14;

        // Encabezados
        wsResumen.Cell(3, 1).Value = "Métrica";
        wsResumen.Cell(3, 2).Value = resumen.AnioActual.ToString();
        wsResumen.Cell(3, 3).Value = resumen.AnioAnterior.ToString();
        wsResumen.Cell(3, 4).Value = "Variación";
        wsResumen.Cell(3, 5).Value = "Var. %";
        wsResumen.Range(3, 1, 3, 5).Style.Font.Bold = true;
        wsResumen.Range(3, 1, 3, 5).Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 4;
        void AddResumenRow(string metrica, decimal actual, decimal anterior, string formato = "#,##0")
        {
            wsResumen.Cell(row, 1).Value = metrica;
            wsResumen.Cell(row, 2).Value = actual;
            wsResumen.Cell(row, 2).Style.NumberFormat.Format = formato;
            wsResumen.Cell(row, 3).Value = anterior;
            wsResumen.Cell(row, 3).Style.NumberFormat.Format = formato;
            wsResumen.Cell(row, 4).Value = actual - anterior;
            wsResumen.Cell(row, 4).Style.NumberFormat.Format = formato;
            var variacion = anterior != 0 ? ((actual - anterior) / Math.Abs(anterior)) : 0;
            wsResumen.Cell(row, 5).Value = variacion;
            wsResumen.Cell(row, 5).Style.NumberFormat.Format = "0.00%";
            if (variacion > 0)
                wsResumen.Cell(row, 5).Style.Font.FontColor = XLColor.DarkGreen;
            else if (variacion < 0)
                wsResumen.Cell(row, 5).Style.Font.FontColor = XLColor.Red;
            row++;
        }

        wsResumen.Cell(row, 1).Value = "OPERACIONES";
        wsResumen.Cell(row, 1).Style.Font.Bold = true;
        row++;
        AddResumenRow("Cantidad Operaciones", resumen.OperacionesActual, resumen.OperacionesAnterior, "#,##0");
        AddResumenRow("Monto Operado", resumen.MontoOperadoActual, resumen.MontoOperadoAnterior, "$#,##0");

        row++;
        wsResumen.Cell(row, 1).Value = "INGRESOS";
        wsResumen.Cell(row, 1).Style.Font.Bold = true;
        row++;
        AddResumenRow("Total Ingresos", resumen.IngresosActual, resumen.IngresosAnterior, "$#,##0");

        row++;
        wsResumen.Cell(row, 1).Value = "CARTERA";
        wsResumen.Cell(row, 1).Style.Font.Bold = true;
        row++;
        AddResumenRow("Cartera Promedio", resumen.CarteraActual, resumen.CarteraAnterior, "$#,##0");
        AddResumenRow("Morosidad Promedio (%)", resumen.MorosidadPromedioActual, resumen.MorosidadPromedioAnterior, "0.00");

        row += 2;
        wsResumen.Cell(row, 1).Value = "MEJOR MES";
        wsResumen.Cell(row, 1).Style.Font.Bold = true;
        row++;
        wsResumen.Cell(row, 1).Value = $"Mejor mes {resumen.AnioActual}:";
        wsResumen.Cell(row, 2).Value = resumen.MejorMesActual;
        wsResumen.Cell(row, 3).Value = resumen.MejorIngresoActual;
        wsResumen.Cell(row, 3).Style.NumberFormat.Format = "$#,##0";
        row++;
        wsResumen.Cell(row, 1).Value = $"Mejor mes {resumen.AnioAnterior}:";
        wsResumen.Cell(row, 2).Value = resumen.MejorMesAnterior;
        wsResumen.Cell(row, 3).Value = resumen.MejorIngresoAnterior;
        wsResumen.Cell(row, 3).Style.NumberFormat.Format = "$#,##0";

        wsResumen.Columns().AdjustToContents();

        // Hoja Detalle Mensual
        var wsDetalle = workbook.Worksheets.Add("Detalle Mensual");
        var headers = new[] { "Mes",
            $"Cartera {resumen.AnioActual}", $"Cartera {resumen.AnioAnterior}", "Var. %",
            $"Ops {resumen.AnioActual}", $"Ops {resumen.AnioAnterior}", "Var. %",
            $"Ingresos {resumen.AnioActual}", $"Ingresos {resumen.AnioAnterior}", "Var. %",
            $"Mora % {resumen.AnioActual}", $"Mora % {resumen.AnioAnterior}", "Var." };

        for (int i = 0; i < headers.Length; i++)
        {
            wsDetalle.Cell(1, i + 1).Value = headers[i];
            wsDetalle.Cell(1, i + 1).Style.Font.Bold = true;
            wsDetalle.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        row = 2;
        foreach (var item in detalleMensual)
        {
            wsDetalle.Cell(row, 1).Value = item.NombreMes;
            wsDetalle.Cell(row, 2).Value = item.CarteraActual;
            wsDetalle.Cell(row, 3).Value = item.CarteraAnterior;
            wsDetalle.Cell(row, 4).Value = item.VariacionCartera / 100;
            wsDetalle.Cell(row, 5).Value = item.OperacionesActual;
            wsDetalle.Cell(row, 6).Value = item.OperacionesAnterior;
            wsDetalle.Cell(row, 7).Value = item.VariacionOperaciones / 100;
            wsDetalle.Cell(row, 8).Value = item.IngresosActual;
            wsDetalle.Cell(row, 9).Value = item.IngresosAnterior;
            wsDetalle.Cell(row, 10).Value = item.VariacionIngresos / 100;
            wsDetalle.Cell(row, 11).Value = item.MorosidadActual / 100;
            wsDetalle.Cell(row, 12).Value = item.MorosidadAnterior / 100;
            wsDetalle.Cell(row, 13).Value = item.VariacionMorosidad / 100;

            // Colores para variaciones
            if (item.VariacionCartera > 0)
                wsDetalle.Cell(row, 4).Style.Font.FontColor = XLColor.DarkGreen;
            else if (item.VariacionCartera < 0)
                wsDetalle.Cell(row, 4).Style.Font.FontColor = XLColor.Red;

            if (item.VariacionOperaciones > 0)
                wsDetalle.Cell(row, 7).Style.Font.FontColor = XLColor.DarkGreen;
            else if (item.VariacionOperaciones < 0)
                wsDetalle.Cell(row, 7).Style.Font.FontColor = XLColor.Red;

            if (item.VariacionIngresos > 0)
                wsDetalle.Cell(row, 10).Style.Font.FontColor = XLColor.DarkGreen;
            else if (item.VariacionIngresos < 0)
                wsDetalle.Cell(row, 10).Style.Font.FontColor = XLColor.Red;

            // Morosidad: menos es mejor
            if (item.VariacionMorosidad < 0)
                wsDetalle.Cell(row, 13).Style.Font.FontColor = XLColor.DarkGreen;
            else if (item.VariacionMorosidad > 0)
                wsDetalle.Cell(row, 13).Style.Font.FontColor = XLColor.Red;

            row++;
        }

        // Formato
        wsDetalle.Column(2).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(3).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(4).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Column(7).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Column(8).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(9).Style.NumberFormat.Format = "#,##0";
        wsDetalle.Column(10).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Column(11).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Column(12).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Column(13).Style.NumberFormat.Format = "0.00%";
        wsDetalle.Columns().AdjustToContents();

        // Hoja Métricas
        var wsMetricas = workbook.Worksheets.Add("Metricas");
        wsMetricas.Cell(1, 1).Value = "Categoría";
        wsMetricas.Cell(1, 2).Value = "Métrica";
        wsMetricas.Cell(1, 3).Value = resumen.AnioActual.ToString();
        wsMetricas.Cell(1, 4).Value = resumen.AnioAnterior.ToString();
        wsMetricas.Cell(1, 5).Value = "Variación";
        wsMetricas.Cell(1, 6).Value = "Var. %";
        wsMetricas.Cell(1, 7).Value = "Tendencia";
        wsMetricas.Range(1, 1, 1, 7).Style.Font.Bold = true;
        wsMetricas.Range(1, 1, 1, 7).Style.Fill.BackgroundColor = XLColor.LightGray;

        row = 2;
        foreach (var m in metricas)
        {
            wsMetricas.Cell(row, 1).Value = m.Categoria;
            wsMetricas.Cell(row, 2).Value = m.Metrica;
            wsMetricas.Cell(row, 3).Value = m.ValorActual;
            wsMetricas.Cell(row, 4).Value = m.ValorAnterior;
            wsMetricas.Cell(row, 5).Value = m.VariacionAbsoluta;
            wsMetricas.Cell(row, 6).Value = m.VariacionPorcentaje / 100;
            wsMetricas.Cell(row, 7).Value = m.Tendencia;

            // Color tendencia
            var color = m.Tendencia == "Subió" ? XLColor.LightGreen :
                       m.Tendencia == "Bajó" ? XLColor.LightCoral : XLColor.LightYellow;
            wsMetricas.Cell(row, 7).Style.Fill.BackgroundColor = color;

            row++;
        }

        wsMetricas.Column(3).Style.NumberFormat.Format = "#,##0";
        wsMetricas.Column(4).Style.NumberFormat.Format = "#,##0";
        wsMetricas.Column(5).Style.NumberFormat.Format = "#,##0";
        wsMetricas.Column(6).Style.NumberFormat.Format = "0.00%";
        wsMetricas.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Comparativo_{resumen.AnioActual}_vs_{resumen.AnioAnterior}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    // ==========================================
    // PROYECCIÓN DE FLUJO DE CAJA
    // ==========================================

    /// <summary>
    /// Obtiene proyección de flujo de caja
    /// </summary>
    /// <remarks>
    /// Proyecta flujo de caja basado en:
    /// - Vencimientos de documentos de cartera
    /// - Tasa de recuperación histórica
    /// - Giros programados por operaciones
    ///
    /// Incluye:
    /// - Detalle diario de cobranzas proyectadas
    /// - Agregación semanal
    /// - Alertas de días con flujo negativo
    /// - Saldo mínimo proyectado
    /// </remarks>
    [HttpGet("proyeccion-flujo")]
    [SwaggerOperation(Summary = "Proyección de flujo de caja", Description = "Proyecta flujo de caja para los próximos días")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProyeccionFlujoCaja(
        [FromQuery] int dias = 30,
        [FromQuery] decimal? saldoInicial = null)
    {
        var (resumen, detalleDiario, detalleSemanal) = await _exportacionService.GetProyeccionFlujoCajaAsync(dias, saldoInicial);
        return Ok(new { resumen, detalleDiario, detalleSemanal });
    }

    /// <summary>
    /// Exporta proyección de flujo de caja a Excel
    /// </summary>
    [HttpGet("proyeccion-flujo/excel")]
    [SwaggerOperation(Summary = "Exportar proyección flujo a Excel")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> ExportProyeccionFlujoExcel(
        [FromQuery] int dias = 30,
        [FromQuery] decimal? saldoInicial = null)
    {
        var (resumen, detalleDiario, detalleSemanal) = await _exportacionService.GetProyeccionFlujoCajaAsync(dias, saldoInicial);

        using var workbook = new XLWorkbook();

        // Hoja Resumen
        var wsResumen = workbook.Worksheets.Add("Resumen");
        wsResumen.Cell(1, 1).Value = "PROYECCIÓN DE FLUJO DE CAJA";
        wsResumen.Cell(1, 1).Style.Font.Bold = true;
        wsResumen.Cell(1, 1).Style.Font.FontSize = 14;

        wsResumen.Cell(3, 1).Value = $"Periodo: {resumen.FechaInicio:dd/MM/yyyy} - {resumen.FechaFin:dd/MM/yyyy}";
        wsResumen.Cell(4, 1).Value = $"Días proyectados: {resumen.DiasProyectados}";
        wsResumen.Cell(5, 1).Value = $"Tasa de recuperación histórica: {resumen.TasaRecuperacionHistorica:P2}";

        wsResumen.Cell(7, 1).Value = "SALDOS";
        wsResumen.Cell(7, 1).Style.Font.Bold = true;
        wsResumen.Cell(8, 1).Value = "Saldo Inicial:";
        wsResumen.Cell(8, 2).Value = resumen.SaldoInicial;
        wsResumen.Cell(8, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(9, 1).Value = "Saldo Final Proyectado:";
        wsResumen.Cell(9, 2).Value = resumen.SaldoFinalProyectado;
        wsResumen.Cell(9, 2).Style.NumberFormat.Format = "$#,##0";
        if (resumen.SaldoFinalProyectado >= resumen.SaldoInicial)
            wsResumen.Cell(9, 2).Style.Font.FontColor = XLColor.DarkGreen;
        else
            wsResumen.Cell(9, 2).Style.Font.FontColor = XLColor.Red;

        wsResumen.Cell(10, 1).Value = "Saldo Mínimo:";
        wsResumen.Cell(10, 2).Value = resumen.MinimoSaldoProyectado;
        wsResumen.Cell(10, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(10, 3).Value = resumen.FechaMinimoSaldo.HasValue ? $"({resumen.FechaMinimoSaldo:dd/MM/yyyy})" : "";
        if (resumen.MinimoSaldoProyectado < 0)
            wsResumen.Cell(10, 2).Style.Font.FontColor = XLColor.Red;

        wsResumen.Cell(12, 1).Value = "FLUJOS PROYECTADOS";
        wsResumen.Cell(12, 1).Style.Font.Bold = true;
        wsResumen.Cell(13, 1).Value = "Total Cobranza Esperada:";
        wsResumen.Cell(13, 2).Value = resumen.TotalCobranzaProyectada;
        wsResumen.Cell(13, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(14, 1).Value = "Total Cobranza Ajustada:";
        wsResumen.Cell(14, 2).Value = resumen.TotalCobranzaAjustada;
        wsResumen.Cell(14, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(15, 1).Value = "Total Giros Proyectados:";
        wsResumen.Cell(15, 2).Value = resumen.TotalGirosProyectados;
        wsResumen.Cell(15, 2).Style.NumberFormat.Format = "$#,##0";
        wsResumen.Cell(16, 1).Value = "Flujo Neto:";
        wsResumen.Cell(16, 2).Value = resumen.FlujoNetoTotal;
        wsResumen.Cell(16, 2).Style.NumberFormat.Format = "$#,##0";
        if (resumen.FlujoNetoTotal >= 0)
            wsResumen.Cell(16, 2).Style.Font.FontColor = XLColor.DarkGreen;
        else
            wsResumen.Cell(16, 2).Style.Font.FontColor = XLColor.Red;

        wsResumen.Cell(18, 1).Value = "ALERTAS";
        wsResumen.Cell(18, 1).Style.Font.Bold = true;
        wsResumen.Cell(19, 1).Value = "Días con flujo negativo:";
        wsResumen.Cell(19, 2).Value = resumen.DiasConFlujoNegativo;
        if (resumen.DiasConFlujoNegativo > 0)
            wsResumen.Cell(19, 2).Style.Font.FontColor = XLColor.Red;
        if (resumen.MinimoSaldoProyectado < 0)
        {
            wsResumen.Cell(20, 1).Value = "⚠️ ATENCIÓN: Saldo mínimo proyectado es negativo";
            wsResumen.Cell(20, 1).Style.Font.FontColor = XLColor.Red;
            wsResumen.Cell(20, 1).Style.Font.Bold = true;
        }

        wsResumen.Columns().AdjustToContents();

        // Hoja Detalle Diario
        var wsDiario = workbook.Worksheets.Add("Detalle Diario");
        var headersDiario = new[] { "Fecha", "Día", "Hábil", "Docs Vencen", "Cobranza Esperada",
            "Cobranza Ajustada", "Giros", "Flujo Neto", "Saldo Acumulado" };

        for (int i = 0; i < headersDiario.Length; i++)
        {
            wsDiario.Cell(1, i + 1).Value = headersDiario[i];
            wsDiario.Cell(1, i + 1).Style.Font.Bold = true;
            wsDiario.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var item in detalleDiario)
        {
            wsDiario.Cell(row, 1).Value = item.Fecha;
            wsDiario.Cell(row, 2).Value = item.DiaSemana;
            wsDiario.Cell(row, 3).Value = item.EsHabil ? "Sí" : "No";
            wsDiario.Cell(row, 4).Value = item.DocumentosVencen;
            wsDiario.Cell(row, 5).Value = item.CobranzaProyectada;
            wsDiario.Cell(row, 6).Value = item.CobranzaAjustada;
            wsDiario.Cell(row, 7).Value = item.GirosProyectados;
            wsDiario.Cell(row, 8).Value = item.FlujoNeto;
            wsDiario.Cell(row, 9).Value = item.SaldoAcumulado;

            // Colores
            if (!item.EsHabil)
                wsDiario.Row(row).Style.Fill.BackgroundColor = XLColor.LightYellow;

            if (item.FlujoNeto < 0)
                wsDiario.Cell(row, 8).Style.Font.FontColor = XLColor.Red;
            else if (item.FlujoNeto > 0)
                wsDiario.Cell(row, 8).Style.Font.FontColor = XLColor.DarkGreen;

            if (item.SaldoAcumulado < 0)
            {
                wsDiario.Cell(row, 9).Style.Font.FontColor = XLColor.Red;
                wsDiario.Cell(row, 9).Style.Font.Bold = true;
            }

            row++;
        }

        wsDiario.Column(1).Style.NumberFormat.Format = "dd/MM/yyyy";
        wsDiario.Column(5).Style.NumberFormat.Format = "#,##0";
        wsDiario.Column(6).Style.NumberFormat.Format = "#,##0";
        wsDiario.Column(7).Style.NumberFormat.Format = "#,##0";
        wsDiario.Column(8).Style.NumberFormat.Format = "#,##0";
        wsDiario.Column(9).Style.NumberFormat.Format = "#,##0";
        wsDiario.Columns().AdjustToContents();

        // Hoja Detalle Semanal
        var wsSemanal = workbook.Worksheets.Add("Resumen Semanal");
        var headersSemanal = new[] { "Semana", "Fecha Inicio", "Fecha Fin", "Docs Vencen",
            "Cobranza Proyectada", "Operaciones", "Giros", "Flujo Neto", "Saldo Acumulado" };

        for (int i = 0; i < headersSemanal.Length; i++)
        {
            wsSemanal.Cell(1, i + 1).Value = headersSemanal[i];
            wsSemanal.Cell(1, i + 1).Style.Font.Bold = true;
            wsSemanal.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        row = 2;
        foreach (var item in detalleSemanal)
        {
            wsSemanal.Cell(row, 1).Value = item.NumeroSemana;
            wsSemanal.Cell(row, 2).Value = item.FechaInicio;
            wsSemanal.Cell(row, 3).Value = item.FechaFin;
            wsSemanal.Cell(row, 4).Value = item.DocumentosVencen;
            wsSemanal.Cell(row, 5).Value = item.CobranzaProyectada;
            wsSemanal.Cell(row, 6).Value = item.OperacionesProyectadas;
            wsSemanal.Cell(row, 7).Value = item.GirosProyectados;
            wsSemanal.Cell(row, 8).Value = item.FlujoNeto;
            wsSemanal.Cell(row, 9).Value = item.SaldoAcumulado;

            // Colores
            if (item.FlujoNeto < 0)
                wsSemanal.Cell(row, 8).Style.Font.FontColor = XLColor.Red;
            else if (item.FlujoNeto > 0)
                wsSemanal.Cell(row, 8).Style.Font.FontColor = XLColor.DarkGreen;

            if (item.SaldoAcumulado < 0)
            {
                wsSemanal.Cell(row, 9).Style.Font.FontColor = XLColor.Red;
                wsSemanal.Cell(row, 9).Style.Font.Bold = true;
            }

            row++;
        }

        wsSemanal.Column(2).Style.NumberFormat.Format = "dd/MM/yyyy";
        wsSemanal.Column(3).Style.NumberFormat.Format = "dd/MM/yyyy";
        wsSemanal.Column(5).Style.NumberFormat.Format = "#,##0";
        wsSemanal.Column(6).Style.NumberFormat.Format = "#,##0";
        wsSemanal.Column(7).Style.NumberFormat.Format = "#,##0";
        wsSemanal.Column(8).Style.NumberFormat.Format = "#,##0";
        wsSemanal.Column(9).Style.NumberFormat.Format = "#,##0";
        wsSemanal.Columns().AdjustToContents();

        // Hoja de Gráfico de Tendencia
        var wsTendencia = workbook.Worksheets.Add("Tendencia Flujo");
        wsTendencia.Cell(1, 1).Value = "TENDENCIA DE SALDO ACUMULADO";
        wsTendencia.Cell(1, 1).Style.Font.Bold = true;
        wsTendencia.Cell(1, 1).Style.Font.FontSize = 14;

        wsTendencia.Cell(3, 1).Value = "Semana";
        wsTendencia.Cell(3, 2).Value = "Saldo";
        wsTendencia.Cell(3, 3).Value = "Tendencia Visual";
        wsTendencia.Row(3).Style.Font.Bold = true;

        row = 4;
        var maxSaldo = detalleSemanal.Any() ? detalleSemanal.Max(d => Math.Abs(d.SaldoAcumulado)) : 1;
        foreach (var item in detalleSemanal)
        {
            wsTendencia.Cell(row, 1).Value = $"Semana {item.NumeroSemana}";
            wsTendencia.Cell(row, 2).Value = item.SaldoAcumulado;
            wsTendencia.Cell(row, 2).Style.NumberFormat.Format = "$#,##0";

            var porcentaje = maxSaldo > 0 ? (int)(Math.Abs(item.SaldoAcumulado) / maxSaldo * 25) : 0;
            wsTendencia.Cell(row, 3).Value = new string('█', Math.Max(1, porcentaje));
            wsTendencia.Cell(row, 3).Style.Font.FontColor = item.SaldoAcumulado >= 0 ? XLColor.Blue : XLColor.Red;
            row++;
        }

        wsTendencia.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Proyeccion_Flujo_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    // ==========================================
    // EXPORTACIONES A CSV
    // ==========================================

    /// <summary>
    /// Exporta cartera a archivo CSV
    /// </summary>
    /// <remarks>
    /// Genera archivo CSV con datos de cartera para integración con otros sistemas.
    /// Formato: separador punto y coma (;), encoding UTF-8 con BOM.
    /// </remarks>
    [HttpGet("cartera/csv")]
    [SwaggerOperation(Summary = "Exportar cartera a CSV", Description = "Descarga archivo CSV con datos de cartera")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportCarteraCsv(
        [FromQuery] long? rutCliente = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] bool soloMorosos = false,
        [FromQuery] bool soloVigentes = false)
    {
        var datos = await _exportacionService.GetCarteraParaExportarAsync(
            rutCliente, fechaDesde, fechaHasta, soloMorosos, soloVigentes);

        var sb = new StringBuilder();
        // BOM para UTF-8
        sb.AppendLine("NroOp;TipoDoc;NroDoc;RutCliente;Cliente;RutDeudor;Deudor;FechaOp;Vencimiento;DiasVencido;MontoDoc;Anticipado;Saldo;Estado;Ejecutivo");

        foreach (var item in datos)
        {
            sb.AppendLine($"{item.NumeroOperacion};{item.TipoDocumento};{item.NumeroDocumento};{item.RutCliente};{EscapeCsv(item.NombreCliente)};{item.RutDeudor};{EscapeCsv(item.NombreDeudor)};{item.FechaOperacion:yyyy-MM-dd};{item.FechaVencimiento:yyyy-MM-dd};{item.DiasVencido};{item.MontoDocumento:F0};{item.MontoAnticipado:F0};{item.SaldoPendiente:F0};{item.Estado};{EscapeCsv(item.Ejecutivo)}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"Cartera_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Exporta operaciones a archivo CSV
    /// </summary>
    [HttpGet("operaciones/csv")]
    [SwaggerOperation(Summary = "Exportar operaciones a CSV")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportOperacionesCsv(
        [FromQuery] long? rutCliente = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] int? estado = null)
    {
        var datos = await _exportacionService.GetOperacionesParaExportarAsync(
            rutCliente, fechaDesde, fechaHasta, estado);

        var sb = new StringBuilder();
        sb.AppendLine("NroOp;Fecha;RutCliente;Cliente;NroLinea;TipoOp;MontoDocs;Anticipo;Intereses;Comision;Gastos;IVA;Giro;Estado;Ejecutivo");

        foreach (var item in datos)
        {
            sb.AppendLine($"{item.NumeroOperacion};{item.FechaOperacion:yyyy-MM-dd};{item.RutCliente};{EscapeCsv(item.NombreCliente)};{item.NumeroLinea};{EscapeCsv(item.TipoOperacionNombre)};{item.MontoDocumentos:F0};{item.MontoAnticipo:F0};{item.MontoIntereses:F0};{item.MontoComision:F0};{item.MontoGastos:F0};{item.MontoIVA:F0};{item.MontoGiro:F0};{item.Estado};{EscapeCsv(item.Ejecutivo)}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"Operaciones_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Exporta lineas de credito a CSV
    /// </summary>
    [HttpGet("lineas-credito/csv")]
    [SwaggerOperation(Summary = "Exportar lineas de credito a CSV")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportLineasCreditoCsv(
        [FromQuery] long? rutCliente = null,
        [FromQuery] bool soloActivas = false)
    {
        var datos = await _exportacionService.GetLineasCreditoParaExportarAsync(rutCliente, soloActivas);

        var sb = new StringBuilder();
        sb.AppendLine("NroLinea;RutCliente;Cliente;FechaAprobacion;Vencimiento;DiasParaVencer;MontoLinea;CupoUtilizado;CupoDisponible;PctUtilizacion;Estado;Ejecutivo");

        foreach (var item in datos)
        {
            sb.AppendLine($"{item.NumeroLinea};{item.RutCliente};{EscapeCsv(item.NombreCliente)};{item.FechaAprobacion:yyyy-MM-dd};{item.FechaVencimiento:yyyy-MM-dd};{item.DiasParaVencer};{item.MontoLinea:F0};{item.CupoUtilizado:F0};{item.CupoDisponible:F0};{item.PorcentajeUtilizacion:F2};{item.Estado};{EscapeCsv(item.Ejecutivo)}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"LineasCredito_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Exporta pagos a CSV
    /// </summary>
    [HttpGet("pagos/csv")]
    [SwaggerOperation(Summary = "Exportar pagos a CSV")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportPagosCsv(
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] long? rutCliente = null)
    {
        var datos = await _exportacionService.GetPagosParaExportarAsync(fechaDesde, fechaHasta, rutCliente);

        var sb = new StringBuilder();
        sb.AppendLine("NroPago;FechaPago;TipoDoc;NroDoc;NroOp;RutCliente;Cliente;RutDeudor;Deudor;MontoPago;SaldoAnterior;SaldoNuevo;TipoPago;Estado");

        foreach (var item in datos)
        {
            sb.AppendLine($"{item.NumeroPago};{item.FechaPago:yyyy-MM-dd};{item.TipoDocumento};{item.NumeroDocumento};{item.NumeroOperacion};{item.RutCliente};{EscapeCsv(item.NombreCliente)};{item.RutDeudor};{EscapeCsv(item.NombreDeudor)};{item.MontoPago:F0};{item.SaldoAnterior:F0};{item.SaldoNuevo:F0};{item.TipoPago};{item.Estado}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"Pagos_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Exporta dashboard a CSV
    /// </summary>
    [HttpGet("dashboard/csv")]
    [SwaggerOperation(Summary = "Exportar metricas dashboard a CSV")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportDashboardCsv()
    {
        var metricas = await _exportacionService.GetDashboardMetricasAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Categoria;Metrica;Valor");

        sb.AppendLine($"Cartera;Total;{metricas.CarteraTotal:F0}");
        sb.AppendLine($"Cartera;Vigente;{metricas.CarteraVigente:F0}");
        sb.AppendLine($"Cartera;Morosa;{metricas.CarteraMorosa:F0}");
        sb.AppendLine($"Cartera;PorcentajeMorosidad;{metricas.PorcentajeMorosidad:F2}");
        sb.AppendLine($"Operaciones;CantidadMes;{metricas.OperacionesMes}");
        sb.AppendLine($"Operaciones;MontoOperadoMes;{metricas.MontoOperadoMes:F0}");
        sb.AppendLine($"Operaciones;InteresesMes;{metricas.InteresesMes:F0}");
        sb.AppendLine($"Operaciones;ComisionesMes;{metricas.ComisionesMes:F0}");
        sb.AppendLine($"LineasCredito;Activas;{metricas.LineasActivas}");
        sb.AppendLine($"LineasCredito;CupoTotal;{metricas.CupoTotalAprobado:F0}");
        sb.AppendLine($"LineasCredito;CupoUtilizado;{metricas.CupoTotalUtilizado:F0}");
        sb.AppendLine($"LineasCredito;CupoDisponible;{metricas.CupoTotalDisponible:F0}");
        sb.AppendLine($"Recaudacion;MontoMes;{metricas.RecaudacionMes:F0}");
        sb.AppendLine($"Recaudacion;CantidadPagosMes;{metricas.PagosMes}");

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"Dashboard_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    // Helper para escapar valores CSV
    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    // ==========================================
    // EXPORTACIONES A PDF
    // ==========================================

    /// <summary>
    /// Exporta cartera a PDF
    /// </summary>
    /// <remarks>
    /// Genera reporte formal en PDF con datos de cartera.
    /// Incluye encabezado corporativo, resumen y detalle.
    /// </remarks>
    [HttpGet("cartera/pdf")]
    [SwaggerOperation(Summary = "Exportar cartera a PDF", Description = "Descarga reporte PDF con datos de cartera")]
    [Produces("application/pdf")]
    public async Task<IActionResult> ExportCarteraPdf(
        [FromQuery] long? rutCliente = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] bool soloMorosos = false,
        [FromQuery] bool soloVigentes = false)
    {
        var datos = await _exportacionService.GetCarteraParaExportarAsync(
            rutCliente, fechaDesde, fechaHasta, soloMorosos, soloVigentes);

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(header => ComposeHeader(header, "REPORTE DE CARTERA"));

                page.Content().Element(content =>
                {
                    content.PaddingVertical(10).Column(col =>
                    {
                        // Resumen
                        col.Item().Element(e => ComposeCarteraResumen(e, datos));

                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Tabla de datos
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);   // Nro Op
                                columns.RelativeColumn(1);   // Tipo Doc
                                columns.RelativeColumn(1.5f); // Nro Doc
                                columns.RelativeColumn(2);   // Cliente
                                columns.RelativeColumn(2);   // Deudor
                                columns.RelativeColumn(1);   // Vencimiento
                                columns.RelativeColumn(1);   // Dias
                                columns.RelativeColumn(1.2f); // Monto
                                columns.RelativeColumn(1.2f); // Saldo
                                columns.RelativeColumn(1);   // Estado
                            });

                            // Encabezados
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Op").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Tipo").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Documento").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Cliente").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Deudor").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Venc.").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Dias").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Monto").Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Saldo").Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Estado").Bold();
                            });

                            // Datos
                            foreach (var item in datos.Take(100)) // Limitar a 100 para PDF
                            {
                                var bgColor = item.DiasVencido > 0 ? Colors.Red.Lighten5 : Colors.White;

                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(item.NumeroOperacion.ToString());
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(item.TipoDocumento);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(item.NumeroDocumento);
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(Truncate(item.NombreCliente, 20));
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(Truncate(item.NombreDeudor, 20));
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(item.FechaVencimiento.ToString("dd/MM/yy"));
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(item.DiasVencido.ToString());
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(item.MontoDocumento.ToString("N0", CultureInfo.GetCultureInfo("es-CL")));
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(item.SaldoPendiente.ToString("N0", CultureInfo.GetCultureInfo("es-CL")));
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(item.Estado);
                            }
                        });

                        if (datos.Count() > 100)
                        {
                            col.Item().PaddingTop(10).Text($"Nota: Se muestran los primeros 100 registros de {datos.Count()} total").FontSize(8).Italic();
                        }
                    });
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"Cartera_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
    }

    /// <summary>
    /// Exporta dashboard ejecutivo a PDF
    /// </summary>
    [HttpGet("dashboard/pdf")]
    [SwaggerOperation(Summary = "Exportar dashboard a PDF", Description = "Reporte ejecutivo con KPIs principales")]
    [Produces("application/pdf")]
    public async Task<IActionResult> ExportDashboardPdf()
    {
        var metricas = await _exportacionService.GetDashboardMetricasAsync();

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(header => ComposeHeader(header, "DASHBOARD EJECUTIVO"));

                page.Content().Element(content =>
                {
                    content.PaddingVertical(15).Column(col =>
                    {
                        // KPIs principales en grid
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Element(e => ComposeKpiBox(e, "CARTERA TOTAL", metricas.CarteraTotal.ToString("C0", CultureInfo.GetCultureInfo("es-CL")), Colors.Blue.Darken2));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(e => ComposeKpiBox(e, "CARTERA VIGENTE", metricas.CarteraVigente.ToString("C0", CultureInfo.GetCultureInfo("es-CL")), Colors.Green.Darken2));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(e => ComposeKpiBox(e, "CARTERA MOROSA", metricas.CarteraMorosa.ToString("C0", CultureInfo.GetCultureInfo("es-CL")), Colors.Red.Darken2));
                        });

                        col.Item().PaddingTop(15).Row(row =>
                        {
                            row.RelativeItem().Element(e => ComposeKpiBox(e, "% MOROSIDAD", $"{metricas.PorcentajeMorosidad:F1}%", metricas.PorcentajeMorosidad > 10 ? Colors.Red.Darken2 : Colors.Green.Darken2));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(e => ComposeKpiBox(e, "OPERACIONES MES", metricas.OperacionesMes.ToString(), Colors.Blue.Darken2));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(e => ComposeKpiBox(e, "MONTO OPERADO", metricas.MontoOperadoMes.ToString("C0", CultureInfo.GetCultureInfo("es-CL")), Colors.Blue.Darken2));
                        });

                        col.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Sección Líneas de Crédito
                        col.Item().PaddingTop(15).Text("LÍNEAS DE CRÉDITO").Bold().FontSize(12);
                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Element(e => ComposeKpiBox(e, "LÍNEAS ACTIVAS", metricas.LineasActivas.ToString(), Colors.Teal.Darken2));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(e => ComposeKpiBox(e, "CUPO TOTAL", metricas.CupoTotalAprobado.ToString("C0", CultureInfo.GetCultureInfo("es-CL")), Colors.Teal.Darken2));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(e => ComposeKpiBox(e, "CUPO DISPONIBLE", metricas.CupoTotalDisponible.ToString("C0", CultureInfo.GetCultureInfo("es-CL")), Colors.Teal.Darken2));
                        });

                        col.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Sección Ingresos
                        col.Item().PaddingTop(15).Text("INGRESOS DEL MES").Bold().FontSize(12);
                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Element(e => ComposeKpiBox(e, "INTERESES", metricas.InteresesMes.ToString("C0", CultureInfo.GetCultureInfo("es-CL")), Colors.Orange.Darken2));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(e => ComposeKpiBox(e, "COMISIONES", metricas.ComisionesMes.ToString("C0", CultureInfo.GetCultureInfo("es-CL")), Colors.Orange.Darken2));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(e => ComposeKpiBox(e, "RECAUDACIÓN", metricas.RecaudacionMes.ToString("C0", CultureInfo.GetCultureInfo("es-CL")), Colors.Green.Darken2));
                        });

                        col.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Tabla resumen
                        col.Item().PaddingTop(15).Text("RESUMEN DE MÉTRICAS").Bold().FontSize(12);
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                            });

                            void AddMetricRow(string label, string value)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(label);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text(value).Bold();
                            }

                            AddMetricRow("Total Cartera", metricas.CarteraTotal.ToString("C0", CultureInfo.GetCultureInfo("es-CL")));
                            AddMetricRow("Cartera Vigente", metricas.CarteraVigente.ToString("C0", CultureInfo.GetCultureInfo("es-CL")));
                            AddMetricRow("Cartera Morosa", metricas.CarteraMorosa.ToString("C0", CultureInfo.GetCultureInfo("es-CL")));
                            AddMetricRow("Porcentaje Morosidad", $"{metricas.PorcentajeMorosidad:F2}%");
                            AddMetricRow("Líneas Activas", metricas.LineasActivas.ToString());
                            AddMetricRow("Cupo Total Líneas", metricas.CupoTotalAprobado.ToString("C0", CultureInfo.GetCultureInfo("es-CL")));
                            AddMetricRow("Cupo Utilizado", metricas.CupoTotalUtilizado.ToString("C0", CultureInfo.GetCultureInfo("es-CL")));
                            AddMetricRow("Cupo Disponible", metricas.CupoTotalDisponible.ToString("C0", CultureInfo.GetCultureInfo("es-CL")));
                            AddMetricRow("Operaciones del Mes", metricas.OperacionesMes.ToString());
                            AddMetricRow("Monto Operado Mes", metricas.MontoOperadoMes.ToString("C0", CultureInfo.GetCultureInfo("es-CL")));
                            AddMetricRow("Intereses del Mes", metricas.InteresesMes.ToString("C0", CultureInfo.GetCultureInfo("es-CL")));
                            AddMetricRow("Comisiones del Mes", metricas.ComisionesMes.ToString("C0", CultureInfo.GetCultureInfo("es-CL")));
                            AddMetricRow("Recaudación del Mes", metricas.RecaudacionMes.ToString("C0", CultureInfo.GetCultureInfo("es-CL")));
                            AddMetricRow("Pagos del Mes", metricas.PagosMes.ToString());
                        });
                    });
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"Dashboard_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
    }

    /// <summary>
    /// Exporta operaciones a PDF
    /// </summary>
    [HttpGet("operaciones/pdf")]
    [SwaggerOperation(Summary = "Exportar operaciones a PDF")]
    [Produces("application/pdf")]
    public async Task<IActionResult> ExportOperacionesPdf(
        [FromQuery] long? rutCliente = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] int? estado = null)
    {
        var datos = await _exportacionService.GetOperacionesParaExportarAsync(
            rutCliente, fechaDesde, fechaHasta, estado);

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Element(header => ComposeHeader(header, "REPORTE DE OPERACIONES"));

                page.Content().Element(content =>
                {
                    content.PaddingVertical(10).Column(col =>
                    {
                        // Resumen
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Element(e => ComposeKpiBoxSmall(e, "Total Operaciones", datos.Count().ToString()));
                            row.ConstantItem(5);
                            row.RelativeItem().Element(e => ComposeKpiBoxSmall(e, "Monto Total", datos.Sum(d => d.MontoDocumentos).ToString("C0", CultureInfo.GetCultureInfo("es-CL"))));
                            row.ConstantItem(5);
                            row.RelativeItem().Element(e => ComposeKpiBoxSmall(e, "Total Giros", datos.Sum(d => d.MontoGiro).ToString("C0", CultureInfo.GetCultureInfo("es-CL"))));
                            row.ConstantItem(5);
                            row.RelativeItem().Element(e => ComposeKpiBoxSmall(e, "Total Intereses", datos.Sum(d => d.MontoIntereses).ToString("C0", CultureInfo.GetCultureInfo("es-CL"))));
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Tabla
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(0.8f); // Nro Op
                                columns.RelativeColumn(1);    // Fecha
                                columns.RelativeColumn(2);    // Cliente
                                columns.RelativeColumn(1.2f); // Tipo
                                columns.RelativeColumn(1.2f); // Monto Docs
                                columns.RelativeColumn(1.2f); // Anticipo
                                columns.RelativeColumn(1);    // Intereses
                                columns.RelativeColumn(1);    // Comision
                                columns.RelativeColumn(1.2f); // Giro
                                columns.RelativeColumn(0.8f); // Estado
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Op").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Fecha").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Cliente").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Tipo").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Monto").Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Anticipo").Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Intereses").Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Comisión").Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Giro").Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Estado").Bold();
                            });

                            foreach (var item in datos.Take(100))
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(item.NumeroOperacion.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(item.FechaOperacion.ToString("dd/MM/yy"));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(Truncate(item.NombreCliente, 25));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(item.TipoOperacionNombre);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(item.MontoDocumentos.ToString("N0", CultureInfo.GetCultureInfo("es-CL")));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(item.MontoAnticipo.ToString("N0", CultureInfo.GetCultureInfo("es-CL")));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(item.MontoIntereses.ToString("N0", CultureInfo.GetCultureInfo("es-CL")));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(item.MontoComision.ToString("N0", CultureInfo.GetCultureInfo("es-CL")));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(item.MontoGiro.ToString("N0", CultureInfo.GetCultureInfo("es-CL")));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(item.Estado);
                            }
                        });

                        if (datos.Count() > 100)
                        {
                            col.Item().PaddingTop(10).Text($"Nota: Se muestran los primeros 100 de {datos.Count()} registros").FontSize(8).Italic();
                        }
                    });
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"Operaciones_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
    }

    /// <summary>
    /// Exporta concentracion de cartera a PDF
    /// </summary>
    [HttpGet("concentracion/pdf")]
    [SwaggerOperation(Summary = "Exportar concentracion a PDF", Description = "Reporte de concentracion por deudor con clasificacion ABC")]
    [Produces("application/pdf")]
    public async Task<IActionResult> ExportConcentracionPdf()
    {
        var (_, datos) = await _exportacionService.GetConcentracionPorDeudorAsync();

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(header => ComposeHeader(header, "ANÁLISIS DE CONCENTRACIÓN"));

                page.Content().Element(content =>
                {
                    content.PaddingVertical(10).Column(col =>
                    {
                        var totalCartera = datos.Sum(d => d.SaldoPendiente);
                        var grupoA = datos.Where(d => d.Clasificacion == "A").ToList();
                        var grupoB = datos.Where(d => d.Clasificacion == "B").ToList();
                        var grupoC = datos.Where(d => d.Clasificacion == "C").ToList();

                        // Resumen ABC
                        col.Item().Text("CLASIFICACIÓN ABC").Bold().FontSize(12);
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Grupo").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Deudores").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Monto").Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("% Cartera").Bold().AlignRight();
                            });

                            void AddAbcRow(string grupo, int count, decimal monto, string color)
                            {
                                var pct = totalCartera > 0 ? (monto / totalCartera * 100) : 0;
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(grupo).Bold();
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(count.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text(monto.ToString("C0", CultureInfo.GetCultureInfo("es-CL")));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"{pct:F1}%");
                            }

                            AddAbcRow("A (Alto valor)", grupoA.Count, grupoA.Sum(d => d.SaldoPendiente), "Green");
                            AddAbcRow("B (Medio valor)", grupoB.Count, grupoB.Sum(d => d.SaldoPendiente), "Yellow");
                            AddAbcRow("C (Bajo valor)", grupoC.Count, grupoC.Sum(d => d.SaldoPendiente), "Red");
                        });

                        col.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Top 20 deudores
                        col.Item().PaddingTop(15).Text("TOP 20 DEUDORES POR EXPOSICIÓN").Bold().FontSize(12);
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(0.5f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(0.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("#").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("RUT").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Deudor").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Saldo").Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("% Cart.").Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("ABC").Bold();
                            });

                            int rank = 1;
                            foreach (var item in datos.Take(20))
                            {
                                var bgColor = item.Clasificacion == "A" ? Colors.Green.Lighten5 :
                                             item.Clasificacion == "B" ? Colors.Yellow.Lighten5 : Colors.White;

                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(rank.ToString());
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(item.RutDeudor.ToString());
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(Truncate(item.NombreDeudor, 30));
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(item.SaldoPendiente.ToString("N0", CultureInfo.GetCultureInfo("es-CL")));
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text($"{item.PorcentajeCartera:F1}%");
                                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(item.Clasificacion).Bold();
                                rank++;
                            }
                        });
                    });
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"Concentracion_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
    }

    // Helpers para PDF
    private void ComposeHeader(IContainer container, string titulo)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("SISTEMA DE FACTORING").Bold().FontSize(14);
                col.Item().Text(titulo).FontSize(12).SemiBold();
            });
            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}").FontSize(9);
                col.Item().Text($"Hora: {DateTime.Now:HH:mm}").FontSize(9);
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Sistema de Factoring - ").FontSize(8);
            text.CurrentPageNumber().FontSize(8);
            text.Span(" / ").FontSize(8);
            text.TotalPages().FontSize(8);
        });
    }

    private void ComposeCarteraResumen(IContainer container, IEnumerable<CarteraExportDto> datos)
    {
        var totalDocs = datos.Count();
        var totalMonto = datos.Sum(d => d.MontoDocumento);
        var totalSaldo = datos.Sum(d => d.SaldoPendiente);
        var docsMorosos = datos.Count(d => d.DiasVencido > 0);
        var montoMoroso = datos.Where(d => d.DiasVencido > 0).Sum(d => d.SaldoPendiente);

        container.Row(row =>
        {
            row.RelativeItem().Element(e => ComposeKpiBoxSmall(e, "Total Documentos", totalDocs.ToString()));
            row.ConstantItem(10);
            row.RelativeItem().Element(e => ComposeKpiBoxSmall(e, "Monto Total", totalMonto.ToString("C0", CultureInfo.GetCultureInfo("es-CL"))));
            row.ConstantItem(10);
            row.RelativeItem().Element(e => ComposeKpiBoxSmall(e, "Saldo Pendiente", totalSaldo.ToString("C0", CultureInfo.GetCultureInfo("es-CL"))));
            row.ConstantItem(10);
            row.RelativeItem().Element(e => ComposeKpiBoxSmall(e, "Docs Morosos", $"{docsMorosos} ({(totalDocs > 0 ? docsMorosos * 100.0 / totalDocs : 0):F1}%)"));
        });
    }

    private void ComposeKpiBox(IContainer container, string label, string value, string color)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
        {
            col.Item().Text(label).FontSize(9).FontColor(Colors.Grey.Darken1);
            col.Item().PaddingTop(5).Text(value).Bold().FontSize(14).FontColor(color);
        });
    }

    private void ComposeKpiBoxSmall(IContainer container, string label, string value)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(col =>
        {
            col.Item().Text(label).FontSize(7).FontColor(Colors.Grey.Darken1);
            col.Item().Text(value).Bold().FontSize(9);
        });
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
    }
}
