// =============================================
// Generador de PDFs con QuestPDF
// =============================================

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Factoring.Application.Services;

namespace Factoring.API.Services;

/// <summary>
/// Servicio para generar documentos PDF
/// </summary>
public class PdfGeneratorService
{
    static PdfGeneratorService()
    {
        // Configurar licencia Community
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Genera PDF de liquidacion de operacion
    /// </summary>
    public byte[] GenerarLiquidacion(LiquidacionOperacionDto liquidacion)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                // Encabezado
                page.Header().Element(c => ComposeHeader(c, liquidacion));

                // Contenido
                page.Content().Element(c => ComposeLiquidacionContent(c, liquidacion));

                // Pie de pagina
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    /// <summary>
    /// Genera PDF de pagare
    /// </summary>
    public byte[] GenerarPagare(PagareDto pagare)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Element(c => ComposePagareContent(c, pagare));
            });
        });

        return document.GeneratePdf();
    }

    /// <summary>
    /// Genera PDF de estado de cuenta
    /// </summary>
    public byte[] GenerarEstadoCuenta(EstadoCuentaDto estado)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComposeEstadoCuentaHeader(c, estado));
                page.Content().Element(c => ComposeEstadoCuentaContent(c, estado));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    // ==========================================
    // COMPONENTES DE LIQUIDACION
    // ==========================================

    private void ComposeHeader(IContainer container, LiquidacionOperacionDto liquidacion)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("FACTORING S.A.").Bold().FontSize(16);
                col.Item().Text("RUT: 76.000.000-0").FontSize(8);
                col.Item().Text("Santiago, Chile").FontSize(8);
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text("LIQUIDACION DE OPERACION").Bold().FontSize(14);
                col.Item().Text($"N {liquidacion.NumeroOperacion}").Bold().FontSize(12);
                col.Item().Text($"Fecha: {liquidacion.FechaOperacion:dd/MM/yyyy}").FontSize(10);
            });
        });

        container.PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Medium);
    }

    private void ComposeLiquidacionContent(IContainer container, LiquidacionOperacionDto liquidacion)
    {
        container.PaddingVertical(10).Column(col =>
        {
            // Datos del cliente
            col.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(clienteCol =>
            {
                clienteCol.Item().Text("DATOS DEL CLIENTE").Bold();
                clienteCol.Item().Row(r =>
                {
                    r.RelativeItem().Text($"RUT: {liquidacion.RutCliente:N0}-{liquidacion.DvCliente}");
                    r.RelativeItem().Text($"Linea: {liquidacion.NumeroLinea}");
                });
                clienteCol.Item().Text($"Razon Social: {liquidacion.NombreCliente}");
                clienteCol.Item().Text($"Direccion: {liquidacion.DireccionCliente}, {liquidacion.ComunaCliente}");
            });

            col.Item().PaddingTop(10);

            // Datos de la operacion
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Tipo: {liquidacion.TipoOperacion}").Bold();
                    c.Item().Text($"Estado: {liquidacion.Estado}");
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Vencimiento: {liquidacion.FechaVencimiento:dd/MM/yyyy}");
                    c.Item().Text($"Plazo: {liquidacion.PlazoDias} dias");
                });
            });

            col.Item().PaddingTop(10);

            // Tabla de documentos
            col.Item().Text("DOCUMENTOS").Bold();
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);  // Tipo
                    columns.RelativeColumn();    // Numero
                    columns.RelativeColumn(2);   // Deudor
                    columns.ConstantColumn(70);  // Vencimiento
                    columns.ConstantColumn(80);  // Monto
                    columns.ConstantColumn(80);  // Anticipado
                });

                // Encabezado
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Tipo").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Numero").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Deudor").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Venc.").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).AlignRight().Text("Monto").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).AlignRight().Text("Anticipado").Bold();
                });

                // Filas
                foreach (var doc in liquidacion.Documentos)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(doc.TipoDocumento);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(doc.NumeroDocumento);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text($"{doc.RutDeudor:N0}-{doc.DvDeudor} {doc.NombreDeudor}").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(doc.FechaVencimiento.ToString("dd/MM/yy"));
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text($"${doc.MontoDocumento:N0}");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text($"${doc.MontoAnticipado:N0}");
                }

                // Total
                table.Cell().ColumnSpan(4).Padding(3).AlignRight().Text("TOTAL:").Bold();
                table.Cell().Padding(3).AlignRight().Text($"${liquidacion.TotalDocumentos:N0}").Bold();
                table.Cell().Padding(3).AlignRight().Text($"${liquidacion.MontoAnticipo:N0}").Bold();
            });

            col.Item().PaddingTop(15);

            // Resumen financiero
            col.Item().Row(row =>
            {
                // Columna izquierda - Anticipos
                row.RelativeItem().Border(1).Padding(10).Column(c =>
                {
                    c.Item().Text("ANTICIPO").Bold();
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Total Documentos:");
                        r.ConstantItem(100).AlignRight().Text($"${liquidacion.TotalDocumentos:N0}");
                    });
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"Anticipo ({liquidacion.PorcentajeAnticipo}%):");
                        r.ConstantItem(100).AlignRight().Text($"${liquidacion.MontoAnticipo:N0}");
                    });
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Diferencia Precio:");
                        r.ConstantItem(100).AlignRight().Text($"${liquidacion.DiferenciaPrecio:N0}");
                    });
                });

                row.ConstantItem(20);

                // Columna derecha - Descuentos
                row.RelativeItem().Border(1).Padding(10).Column(c =>
                {
                    c.Item().Text("DESCUENTOS").Bold();
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"Intereses ({liquidacion.TasaInteres:F2}%):");
                        r.ConstantItem(100).AlignRight().Text($"${liquidacion.MontoIntereses:N0}");
                    });
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"Comision ({liquidacion.TasaComision:F2}%):");
                        r.ConstantItem(100).AlignRight().Text($"${liquidacion.MontoComision:N0}");
                    });
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Gastos:");
                        r.ConstantItem(100).AlignRight().Text($"${liquidacion.MontoGastos:N0}");
                    });
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text("IVA:");
                        r.ConstantItem(100).AlignRight().Text($"${liquidacion.MontoIVA:N0}");
                    });
                    c.Item().PaddingTop(5).BorderTop(1).Row(r =>
                    {
                        r.RelativeItem().Text("Total Descuentos:").Bold();
                        r.ConstantItem(100).AlignRight().Text($"${liquidacion.TotalDescuentos:N0}").Bold();
                    });
                });
            });

            col.Item().PaddingTop(15);

            // Monto a girar
            col.Item().Background(Colors.Blue.Lighten4).Padding(10).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("MONTO A GIRAR").Bold().FontSize(12);
                    c.Item().Text($"Forma de Pago: {liquidacion.FormaPago}");
                    if (!string.IsNullOrEmpty(liquidacion.BancoDestino))
                        c.Item().Text($"Banco: {liquidacion.BancoDestino} - Cta: {liquidacion.CuentaDestino}");
                });
                row.ConstantItem(150).AlignRight().AlignMiddle().Text($"${liquidacion.MontoGiro:N0}").Bold().FontSize(16);
            });

            col.Item().PaddingTop(20);

            // Firma
            col.Item().Row(row =>
            {
                row.RelativeItem().AlignCenter().Column(c =>
                {
                    c.Item().PaddingTop(40).BorderTop(1).Width(200).AlignCenter().Text("Firma Cliente");
                    c.Item().Text(liquidacion.NombreCliente).FontSize(8);
                });
                row.RelativeItem().AlignCenter().Column(c =>
                {
                    c.Item().PaddingTop(40).BorderTop(1).Width(200).AlignCenter().Text("Ejecutivo");
                    c.Item().Text(liquidacion.NombreEjecutivo).FontSize(8);
                });
            });
        });
    }

    // ==========================================
    // COMPONENTES DE PAGARE
    // ==========================================

    private void ComposePagareContent(IContainer container, PagareDto pagare)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().Text("PAGARE").Bold().FontSize(20);
            col.Item().AlignCenter().Text($"N {pagare.NumeroPagare}").Bold().FontSize(14);

            col.Item().PaddingTop(20);

            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"{pagare.LugarEmision}, {pagare.FechaEmision:dd} de {pagare.FechaEmision:MMMM} de {pagare.FechaEmision:yyyy}");
                row.ConstantItem(150).AlignRight().Column(c =>
                {
                    c.Item().Text($"Vencimiento:");
                    c.Item().Text($"{pagare.FechaVencimiento:dd/MM/yyyy}").Bold();
                });
            });

            col.Item().PaddingTop(20);

            col.Item().Border(1).Padding(15).Column(c =>
            {
                c.Item().Text($"Por este PAGARE, yo/nosotros {pagare.NombreDeudor}, RUT {pagare.RutDeudor:N0}-{pagare.DvDeudor}, " +
                    $"domiciliado en {pagare.DireccionDeudor}, {pagare.ComunaDeudor}, " +
                    $"prometo(emos) pagar incondicionalmente a la orden de {pagare.NombreBeneficiario}, RUT {pagare.RutBeneficiario}, " +
                    $"o a quien sus derechos represente, la suma de:").LineHeight(1.5f);

                c.Item().PaddingTop(10).AlignCenter().Text($"${pagare.MontoPagare:N0}").Bold().FontSize(18);
                c.Item().AlignCenter().Text($"({pagare.MontoEnLetras})").FontSize(9);

                c.Item().PaddingTop(10).Text($"Pagaderos el dia {pagare.FechaVencimiento:dd} de {pagare.FechaVencimiento:MMMM} de {pagare.FechaVencimiento:yyyy}, " +
                    $"en las oficinas del beneficiario ubicadas en Santiago.").LineHeight(1.5f);

                c.Item().PaddingTop(10).Text($"En caso de mora, se devengara un interes del {pagare.TasaInteresMora:F2}% mensual sobre el saldo insoluto, " +
                    "sin perjuicio de las demas acciones legales que procedan.").LineHeight(1.5f);

                c.Item().PaddingTop(10).Text("Para todos los efectos derivados de este pagare, las partes fijan domicilio en la ciudad de Santiago y " +
                    "se someten a la jurisdiccion de sus Tribunales Ordinarios de Justicia.").LineHeight(1.5f);
            });

            col.Item().PaddingTop(40);

            col.Item().Row(row =>
            {
                row.RelativeItem().AlignCenter().Column(c =>
                {
                    c.Item().PaddingTop(50).BorderTop(1).Width(250).AlignCenter().Text("Firma Deudor");
                    c.Item().Text(pagare.NombreDeudor).FontSize(9);
                    c.Item().Text($"RUT: {pagare.RutDeudor:N0}-{pagare.DvDeudor}").FontSize(9);
                });
            });

            col.Item().PaddingTop(30);

            col.Item().Text("INSTRUCCIONES NOTARIALES:").Bold().FontSize(8);
            col.Item().Text("Autorizo al Notario ante quien se protocolice este documento para que proceda a su protesto " +
                "sin necesidad de requerimiento personal, pudiendo protestar administrativamente este instrumento.").FontSize(8).LineHeight(1.3f);
        });
    }

    // ==========================================
    // COMPONENTES DE ESTADO DE CUENTA
    // ==========================================

    private void ComposeEstadoCuentaHeader(IContainer container, EstadoCuentaDto estado)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("FACTORING S.A.").Bold().FontSize(14);
                col.Item().Text("RUT: 76.000.000-0").FontSize(8);
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text("ESTADO DE CUENTA").Bold().FontSize(14);
                col.Item().Text($"Emision: {estado.FechaEmision:dd/MM/yyyy}");
                col.Item().Text($"Periodo: {estado.FechaDesde:dd/MM/yyyy} al {estado.FechaHasta:dd/MM/yyyy}").FontSize(8);
            });
        });

        container.PaddingTop(5).BorderBottom(1).BorderColor(Colors.Grey.Medium);
    }

    private void ComposeEstadoCuentaContent(IContainer container, EstadoCuentaDto estado)
    {
        container.PaddingVertical(10).Column(col =>
        {
            // Datos del cliente
            col.Item().Background(Colors.Grey.Lighten3).Padding(8).Column(c =>
            {
                c.Item().Text($"Cliente: {estado.RutCliente:N0}-{estado.DvCliente} - {estado.NombreCliente}").Bold();
                c.Item().Text($"Direccion: {estado.DireccionCliente}");
            });

            col.Item().PaddingTop(10);

            // Resumen
            col.Item().Row(row =>
            {
                row.RelativeItem().Border(1).Padding(8).Column(c =>
                {
                    c.Item().Text("Total Operaciones:").FontSize(8);
                    c.Item().AlignRight().Text($"${estado.TotalOperaciones:N0}").Bold();
                });
                row.RelativeItem().Border(1).Padding(8).Column(c =>
                {
                    c.Item().Text("Total Pagos:").FontSize(8);
                    c.Item().AlignRight().Text($"${estado.TotalPagos:N0}").Bold();
                });
                row.RelativeItem().Border(1).Padding(8).Background(Colors.Blue.Lighten4).Column(c =>
                {
                    c.Item().Text("Saldo Actual:").FontSize(8);
                    c.Item().AlignRight().Text($"${estado.SaldoActual:N0}").Bold();
                });
                row.RelativeItem().Border(1).Padding(8).Column(c =>
                {
                    c.Item().Text("Excedentes:").FontSize(8);
                    c.Item().AlignRight().Text($"${estado.ExcedentesDisponibles:N0}").Bold();
                });
            });

            col.Item().PaddingTop(15);

            // Movimientos
            if (estado.Movimientos.Any())
            {
                col.Item().Text("MOVIMIENTOS DEL PERIODO").Bold();
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(70);   // Fecha
                        columns.ConstantColumn(60);   // Tipo
                        columns.RelativeColumn();     // Descripcion
                        columns.ConstantColumn(80);   // Cargo
                        columns.ConstantColumn(80);   // Abono
                        columns.ConstantColumn(80);   // Saldo
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Fecha").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Tipo").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Descripcion").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(3).AlignRight().Text("Cargo").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(3).AlignRight().Text("Abono").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(3).AlignRight().Text("Saldo").Bold();
                    });

                    foreach (var mov in estado.Movimientos)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(mov.Fecha.ToString("dd/MM/yy"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(mov.Tipo);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(mov.Descripcion).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).AlignRight().Text(mov.Cargo > 0 ? $"${mov.Cargo:N0}" : "");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).AlignRight().Text(mov.Abono > 0 ? $"${mov.Abono:N0}" : "");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).AlignRight().Text($"${mov.Saldo:N0}");
                    }
                });
            }

            col.Item().PaddingTop(15);

            // Documentos pendientes
            if (estado.DocumentosPendientes.Any())
            {
                col.Item().Text("DOCUMENTOS PENDIENTES").Bold();
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);   // Op
                        columns.ConstantColumn(60);   // Tipo
                        columns.ConstantColumn(80);   // Numero
                        columns.RelativeColumn();     // Deudor
                        columns.ConstantColumn(70);   // Venc
                        columns.ConstantColumn(50);   // Dias
                        columns.ConstantColumn(80);   // Saldo
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Op").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Tipo").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Numero").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Deudor").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Venc.").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(2).Text("Dias").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(2).AlignRight().Text("Saldo").Bold();
                    });

                    foreach (var doc in estado.DocumentosPendientes)
                    {
                        var bgColor = doc.DiasVencido > 0 ? Colors.Red.Lighten4 : Colors.White;
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(doc.NumeroOperacion.ToString());
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(doc.TipoDocumento);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(doc.NumeroDocumento);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(doc.NombreDeudor).FontSize(8);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(doc.FechaVencimiento.ToString("dd/MM/yy"));
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(doc.DiasVencido > 0 ? doc.DiasVencido.ToString() : "-");
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).AlignRight().Text($"${doc.SaldoPendiente:N0}");
                    }

                    // Total
                    table.Cell().ColumnSpan(6).Padding(2).AlignRight().Text("TOTAL PENDIENTE:").Bold();
                    table.Cell().Padding(2).AlignRight().Text($"${estado.DocumentosPendientes.Sum(d => d.SaldoPendiente):N0}").Bold();
                });
            }
        });
    }

    // ==========================================
    // PIE DE PAGINA
    // ==========================================

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(5).Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span("FACTORING S.A. - ").FontSize(8);
                text.Span("Documento generado automaticamente").FontSize(7).FontColor(Colors.Grey.Medium);
            });
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Pagina ").FontSize(8);
                text.CurrentPageNumber().FontSize(8);
                text.Span(" de ").FontSize(8);
                text.TotalPages().FontSize(8);
            });
        });
    }
}
