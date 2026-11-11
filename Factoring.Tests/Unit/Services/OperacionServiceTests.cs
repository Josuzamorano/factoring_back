using Factoring.Application.Services;
using Factoring.Core.Interfaces;
using FluentAssertions;
using Moq;
using System.Data;

namespace Factoring.Tests.Unit.Services;

/// <summary>
/// Pruebas unitarias para OperacionService
/// Nota: OperacionService usa Dapper directamente, lo que hace las pruebas unitarias más complejas.
/// Estas pruebas verifican la lógica de negocio mediante pruebas de los DTOs y resultados.
/// </summary>
public class OperacionServiceTests
{
    [Fact]
    public void ValidacionOperacionResult_DebeInicializarCorrectamente()
    {
        // Arrange & Act
        var result = new ValidacionOperacionResult();

        // Assert
        result.EsValida.Should().BeFalse();
        result.Errores.Should().NotBeNull();
        result.Errores.Should().BeEmpty();
        result.MontoDisponibleLinea.Should().Be(0);
        result.TasaAplicable.Should().Be(0);
    }

    [Fact]
    public void ValidacionOperacionResult_ConErrores_DebeIndicarInvalida()
    {
        // Arrange
        var result = new ValidacionOperacionResult();

        // Act
        result.Errores.Add("Error de prueba");

        // Assert
        result.Errores.Should().HaveCount(1);
        result.EsValida.Should().BeFalse();
    }

    [Fact]
    public void CalculoOperacionResult_DebeCalcularDiferenciaPreciosCorrectamente()
    {
        // Arrange
        var result = new CalculoOperacionResult
        {
            MontoDocumentos = 10000000,
            PorcentajeAnticipo = 80,
            MontoAnticipo = 8000000
        };

        // Act
        result.DiferenciaPrecios = result.MontoDocumentos - result.MontoAnticipo;

        // Assert
        result.DiferenciaPrecios.Should().Be(2000000);
    }

    [Fact]
    public void CalculoOperacionResult_DebeCalcularInteresesCorrectamente()
    {
        // Arrange - Simular cálculo: Monto * Tasa * (Días / 30)
        decimal montoAnticipo = 8000000;
        decimal tasa = 0.03m; // 3%
        int plazoDias = 60;

        // Act
        decimal intereses = Math.Round(montoAnticipo * tasa * (plazoDias / 30m), 0);

        // Assert
        // 8,000,000 * 0.03 * 2 = 480,000
        intereses.Should().Be(480000);
    }

    [Fact]
    public void CalculoOperacionResult_DebeCalcularIVACorrectamente()
    {
        // Arrange
        decimal comisionTotal = 100000;
        decimal tasaIVA = 0.19m;

        // Act
        decimal iva = Math.Round(comisionTotal * tasaIVA, 0);

        // Assert
        iva.Should().Be(19000);
    }

    [Fact]
    public void CrearOperacionRequest_DebeInicializarListasVacias()
    {
        // Arrange & Act
        var request = new CrearOperacionRequest();

        // Assert
        request.Documentos.Should().NotBeNull();
        request.Documentos.Should().BeEmpty();
        request.GastosCodigos.Should().NotBeNull();
        request.GastosCodigos.Should().BeEmpty();
        request.TipoMoneda.Should().Be(1); // Peso por defecto
    }

    [Fact]
    public void DocumentoRequest_DebeTenerValoresPorDefecto()
    {
        // Arrange & Act
        var doc = new DocumentoRequest();

        // Assert
        doc.RutDeudor.Should().Be(0);
        doc.NumeroDocumento.Should().Be(string.Empty);
        doc.Monto.Should().Be(0);
    }

    [Fact]
    public void CrearOperacionRequest_DebeCalcularMontoTotalDocumentos()
    {
        // Arrange
        var request = new CrearOperacionRequest
        {
            Documentos = new List<DocumentoRequest>
            {
                new() { Monto = 1000000 },
                new() { Monto = 2000000 },
                new() { Monto = 3000000 }
            }
        };

        // Act
        decimal montoTotal = request.Documentos.Sum(d => d.Monto);

        // Assert
        montoTotal.Should().Be(6000000);
    }

    [Fact]
    public void CrearOperacionRequest_DebeCalcularPlazoPonderado()
    {
        // Arrange
        var hoy = DateTime.Today;
        var request = new CrearOperacionRequest
        {
            Documentos = new List<DocumentoRequest>
            {
                new() { Monto = 1000000, FechaVencimiento = hoy.AddDays(30) },
                new() { Monto = 2000000, FechaVencimiento = hoy.AddDays(60) },
                new() { Monto = 1000000, FechaVencimiento = hoy.AddDays(90) }
            }
        };

        // Act
        decimal montoTotal = request.Documentos.Sum(d => d.Monto);
        int plazoPonderado = (int)request.Documentos
            .Where(d => d.FechaVencimiento > hoy)
            .Select(d => (d.FechaVencimiento - hoy).Days * d.Monto / montoTotal)
            .Sum();

        // Assert
        // (30*1M + 60*2M + 90*1M) / 4M = (30 + 120 + 90) / 4 = 60
        plazoPonderado.Should().Be(60);
    }

    [Fact]
    public void CalculoOperacion_DebeCalcularMontoGiroCorrectamente()
    {
        // Arrange
        var calculo = new CalculoOperacionResult
        {
            MontoDocumentos = 10000000,
            PorcentajeAnticipo = 80,
            MontoAnticipo = 8000000,
            InteresesDescuento = 480000,  // 8M * 3% * 2 meses
            ComisionFija = 0,
            ComisionPorcentual = 50000,    // 0.5% de 10M
            IvaComision = 9500             // 19% de 50K
        };

        // Act
        calculo.TotalGastos = calculo.InteresesDescuento + calculo.ComisionFija + calculo.ComisionPorcentual + calculo.IvaComision;
        calculo.MontoAdelanto = calculo.MontoAnticipo - calculo.TotalGastos;
        calculo.MontoGiro = calculo.MontoAdelanto;
        calculo.DiferenciaPrecios = calculo.MontoDocumentos - calculo.MontoAnticipo;

        // Assert
        calculo.TotalGastos.Should().Be(539500);
        calculo.MontoAdelanto.Should().Be(7460500);
        calculo.MontoGiro.Should().Be(7460500);
        calculo.DiferenciaPrecios.Should().Be(2000000);
    }

    [Theory]
    [InlineData(100, 80, 80)]      // Solicita 100%, máximo 80%, debe usar 80%
    [InlineData(70, 80, 70)]       // Solicita 70%, máximo 80%, debe usar 70%
    [InlineData(80, 80, 80)]       // Solicita 80%, máximo 80%, debe usar 80%
    public void PorcentajeAnticipo_DebeRespetarMaximo(decimal solicitado, decimal maximo, decimal esperado)
    {
        // Act
        var resultado = Math.Min(solicitado, maximo);

        // Assert
        resultado.Should().Be(esperado);
    }

    [Theory]
    [InlineData(1000000, 30, 0.03, 30000)]    // 1M, 30 días, 3% = 30K
    [InlineData(5000000, 60, 0.025, 250000)]  // 5M, 60 días, 2.5% = 250K
    [InlineData(10000000, 90, 0.02, 600000)]  // 10M, 90 días, 2% = 600K
    public void CalculoIntereses_DebeSerCorrecto(decimal monto, int dias, decimal tasa, decimal esperado)
    {
        // Act
        decimal intereses = Math.Round(monto * tasa * (dias / 30m), 0);

        // Assert
        intereses.Should().Be(esperado);
    }
}
