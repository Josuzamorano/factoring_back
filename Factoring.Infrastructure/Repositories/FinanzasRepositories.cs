using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Factoring.Core.Entities;
using Factoring.Core.DTOs;
using Factoring.Core.Interfaces;
using Factoring.Infrastructure.Data;

namespace Factoring.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de Tasas y Plazos
/// Migrado de F_FI1100.frm - Tabla TYP
/// </summary>
public class TasaPlazoRepository : ITasaPlazoRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    // Códigos de tablas de parámetros del sistema VB6
    private const int TAB_MONTORANGO = 20; // Rango de montos
    private const int TAB_MONEDA = 2;      // Tipo de moneda

    public TasaPlazoRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<TasaPlazo>> GetByFechaYMonedaAsync(DateTime? fecha, int tipoMoneda)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Query basada en SEL_TYP del VB6
        const string sql = @"
            SELECT 
                TYP.TYP_FEC_TAS AS FechaVigencia,
                TYP.TYP_TIP_TAS AS TipoTasa,
                PAR1.PAR_DES_PAR AS DescripcionTipoTasa,
                TYP.TYP_TIP_MON AS TipoMoneda,
                PAR2.PAR_DES_PAR AS DescripcionMoneda,
                TYP.TYP_DIA_TAS AS DiasTramo,
                TYP.TYP_VTA_NEG AS TasaBase,
                TYP.TYP_VAL_TMO AS TasaMora,
                TYP.TYP_MAX_CON AS TasaMaximaConvencional,
                TYP.TYP_TAS_AUX AS TasaAuxiliar
            FROM TYP
            LEFT JOIN PAR AS PAR1 ON PAR1.PAR_COD_TAB = @TabMontoRango 
                AND PAR1.PAR_COD_PAR = TYP.TYP_TIP_TAS
            LEFT JOIN PAR AS PAR2 ON PAR2.PAR_COD_TAB = @TabMoneda 
                AND PAR2.PAR_COD_PAR = TYP.TYP_TIP_MON
            WHERE TYP.TYP_FEC_TAS = @Fecha
              AND TYP.TYP_TIP_MON = @TipoMoneda
            ORDER BY TYP.TYP_TIP_TAS, TYP.TYP_DIA_TAS";

        return await connection.QueryAsync<TasaPlazo>(sql, new 
        { 
            Fecha = fecha,
            TipoMoneda = tipoMoneda,
            TabMontoRango = TAB_MONTORANGO,
            TabMoneda = TAB_MONEDA
        });
    }

    public async Task<TasaPlazo?> GetByClaveAsync(DateTime? fecha, int tipoTasa, int tipoMoneda, int dias)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                TYP.TYP_FEC_TAS AS FechaVigencia,
                TYP.TYP_TIP_TAS AS TipoTasa,
                PAR1.PAR_DES_PAR AS DescripcionTipoTasa,
                TYP.TYP_TIP_MON AS TipoMoneda,
                PAR2.PAR_DES_PAR AS DescripcionMoneda,
                TYP.TYP_DIA_TAS AS DiasTramo,
                TYP.TYP_VTA_NEG AS TasaBase,
                TYP.TYP_VAL_TMO AS TasaMora,
                TYP.TYP_MAX_CON AS TasaMaximaConvencional,
                TYP.TYP_TAS_AUX AS TasaAuxiliar
            FROM TYP
            LEFT JOIN PAR AS PAR1 ON PAR1.PAR_COD_TAB = @TabMontoRango 
                AND PAR1.PAR_COD_PAR = TYP.TYP_TIP_TAS
            LEFT JOIN PAR AS PAR2 ON PAR2.PAR_COD_TAB = @TabMoneda 
                AND PAR2.PAR_COD_PAR = TYP.TYP_TIP_MON
            WHERE TYP.TYP_FEC_TAS = @Fecha
              AND TYP.TYP_TIP_TAS = @TipoTasa
              AND TYP.TYP_TIP_MON = @TipoMoneda
              AND TYP.TYP_DIA_TAS = @Dias";

        return await connection.QueryFirstOrDefaultAsync<TasaPlazo>(sql, new 
        { 
            Fecha = fecha,
            TipoTasa = tipoTasa,
            TipoMoneda = tipoMoneda,
            Dias = dias,
            TabMontoRango = TAB_MONTORANGO,
            TabMoneda = TAB_MONEDA
        });
    }

    public async Task<IEnumerable<TasaPlazo>> GetByFiltroAsync(FiltroTasaPlazoDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                TYP.TYP_FEC_TAS AS FechaVigencia,
                TYP.TYP_TIP_TAS AS TipoTasa,
                PAR1.PAR_DES_PAR AS DescripcionTipoTasa,
                TYP.TYP_TIP_MON AS TipoMoneda,
                PAR2.PAR_DES_PAR AS DescripcionMoneda,
                TYP.TYP_DIA_TAS AS DiasTramo,
                TYP.TYP_VTA_NEG AS TasaBase,
                TYP.TYP_VAL_TMO AS TasaMora,
                TYP.TYP_MAX_CON AS TasaMaximaConvencional,
                TYP.TYP_TAS_AUX AS TasaAuxiliar
            FROM TYP
            LEFT JOIN PAR AS PAR1 ON PAR1.PAR_COD_TAB = @TabMontoRango 
                AND PAR1.PAR_COD_PAR = TYP.TYP_TIP_TAS
            LEFT JOIN PAR AS PAR2 ON PAR2.PAR_COD_TAB = @TabMoneda 
                AND PAR2.PAR_COD_PAR = TYP.TYP_TIP_MON
            WHERE 1=1";

        var parameters = new DynamicParameters();
        parameters.Add("TabMontoRango", TAB_MONTORANGO);
        parameters.Add("TabMoneda", TAB_MONEDA);

        if (filtro.FechaDesde.HasValue)
        {
            sql += " AND TYP.TYP_FEC_TAS >= @FechaDesde";
            parameters.Add("FechaDesde", filtro.FechaDesde.Value);
        }

        if (filtro.FechaHasta.HasValue)
        {
            sql += " AND TYP.TYP_FEC_TAS <= @FechaHasta";
            parameters.Add("FechaHasta", filtro.FechaHasta.Value);
        }

        if (filtro.TipoMoneda.HasValue)
        {
            sql += " AND TYP.TYP_TIP_MON = @TipoMoneda";
            parameters.Add("TipoMoneda", filtro.TipoMoneda.Value);
        }

        if (filtro.TipoTasa.HasValue)
        {
            sql += " AND TYP.TYP_TIP_TAS = @TipoTasa";
            parameters.Add("TipoTasa", filtro.TipoTasa.Value);
        }

        if (filtro.DiasDesde.HasValue)
        {
            sql += " AND TYP.TYP_DIA_TAS >= @DiasDesde";
            parameters.Add("DiasDesde", filtro.DiasDesde.Value);
        }

        if (filtro.DiasHasta.HasValue)
        {
            sql += " AND TYP.TYP_DIA_TAS <= @DiasHasta";
            parameters.Add("DiasHasta", filtro.DiasHasta.Value);
        }

        sql += " ORDER BY TYP.TYP_FEC_TAS DESC, TYP.TYP_TIP_MON, TYP.TYP_TIP_TAS, TYP.TYP_DIA_TAS";

        return await connection.QueryAsync<TasaPlazo>(sql, parameters);
    }

    public async Task<TasaPlazo?> GetTasaAplicableAsync(DateTime? fecha, int tipoMoneda, int dias, decimal monto)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Busca la tasa aplicable según el tramo de monto y días
        // Primero determina el tipo de tasa según el monto
        const string sql = @"
            SELECT TOP 1
                TYP.TYP_FEC_TAS AS FechaVigencia,
                TYP.TYP_TIP_TAS AS TipoTasa,
                PAR1.PAR_DES_PAR AS DescripcionTipoTasa,
                TYP.TYP_TIP_MON AS TipoMoneda,
                PAR2.PAR_DES_PAR AS DescripcionMoneda,
                TYP.TYP_DIA_TAS AS DiasTramo,
                TYP.TYP_VTA_NEG AS TasaBase,
                TYP.TYP_VAL_TMO AS TasaMora,
                TYP.TYP_MAX_CON AS TasaMaximaConvencional,
                TYP.TYP_TAS_AUX AS TasaAuxiliar
            FROM TYP
            LEFT JOIN PAR AS PAR1 ON PAR1.PAR_COD_TAB = @TabMontoRango 
                AND PAR1.PAR_COD_PAR = TYP.TYP_TIP_TAS
            LEFT JOIN PAR AS PAR2 ON PAR2.PAR_COD_TAB = @TabMoneda 
                AND PAR2.PAR_COD_PAR = TYP.TYP_TIP_MON
            WHERE TYP.TYP_FEC_TAS <= @Fecha
              AND TYP.TYP_TIP_MON = @TipoMoneda
              AND TYP.TYP_DIA_TAS >= @Dias
            ORDER BY TYP.TYP_FEC_TAS DESC, TYP.TYP_DIA_TAS ASC";

        return await connection.QueryFirstOrDefaultAsync<TasaPlazo>(sql, new 
        { 
            Fecha = fecha,
            TipoMoneda = tipoMoneda,
            Dias = dias,
            TabMontoRango = TAB_MONTORANGO,
            TabMoneda = TAB_MONEDA
        });
    }

    public async Task<bool> CreateAsync(TasaPlazo tasa)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Basado en INS_TYP del VB6
        const string sql = @"
            INSERT INTO TYP (
                TYP_FEC_TAS, TYP_TIP_TAS, TYP_TIP_MON, TYP_DIA_TAS,
                TYP_VTA_NEG, TYP_VAL_TMO, TYP_MAX_CON, TYP_TAS_AUX
            ) VALUES (
                @FechaVigencia, @TipoTasa, @TipoMoneda, @DiasTramo,
                @TasaBase, @TasaMora, @TasaMaximaConvencional, @TasaAuxiliar
            )";

        var result = await connection.ExecuteAsync(sql, tasa);
        return result > 0;
    }

    public async Task<bool> UpdateAsync(TasaPlazo tasa)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Basado en UPD_TYP del VB6
        const string sql = @"
            Update TYP SET
                TYP_VTA_NEG = @TasaBase,
                TYP_VAL_TMO = @TasaMora,
                TYP_MAX_CON = @TasaMaximaConvencional,
                TYP_TAS_AUX = @TasaAuxiliar
            WHERE TYP_FEC_TAS = @FechaVigencia
              AND TYP_TIP_TAS = @TipoTasa
              AND TYP_TIP_MON = @TipoMoneda
              AND TYP_DIA_TAS = @DiasTramo";

        var result = await connection.ExecuteAsync(sql, tasa);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(DateTime? fecha, int tipoTasa, int tipoMoneda, int dias)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            DELETE FROM TYP 
            WHERE TYP_FEC_TAS = @Fecha
              AND TYP_TIP_TAS = @TipoTasa
              AND TYP_TIP_MON = @TipoMoneda
              AND TYP_DIA_TAS = @Dias";

        var result = await connection.ExecuteAsync(sql, new 
        { 
            Fecha = fecha,
            TipoTasa = tipoTasa,
            TipoMoneda = tipoMoneda,
            Dias = dias
        });
        return result > 0;
    }

    public async Task<int> CopiarTasasAsync(DateTime? fechaOrigen, DateTime? fechaDestino, int tipoMoneda, decimal? ajuste = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var ajusteMultiplicador = 1 + (ajuste ?? 0) / 100;

        const string sql = @"
            INSERT INTO TYP (
                TYP_FEC_TAS, TYP_TIP_TAS, TYP_TIP_MON, TYP_DIA_TAS,
                TYP_VTA_NEG, TYP_VAL_TMO, TYP_MAX_CON, TYP_TAS_AUX
            )
            SELECT 
                @FechaDestino, TYP_TIP_TAS, TYP_TIP_MON, TYP_DIA_TAS,
                TYP_VTA_NEG * @Ajuste, TYP_VAL_TMO * @Ajuste, 
                TYP_MAX_CON * @Ajuste, TYP_TAS_AUX * @Ajuste
            FROM TYP
            WHERE TYP_FEC_TAS = @FechaOrigen
              AND TYP_TIP_MON = @TipoMoneda
              AND NOT EXISTS (
                  SELECT 1 FROM TYP T2 
                  WHERE T2.TYP_FEC_TAS = @FechaDestino 
                    AND T2.TYP_TIP_TAS = TYP.TYP_TIP_TAS
                    AND T2.TYP_TIP_MON = TYP.TYP_TIP_MON
                    AND T2.TYP_DIA_TAS = TYP.TYP_DIA_TAS
              )";

        return await connection.ExecuteAsync(sql, new 
        { 
            FechaOrigen = fechaOrigen,
            FechaDestino = fechaDestino,
            TipoMoneda = tipoMoneda,
            Ajuste = ajusteMultiplicador
        });
    }

    public async Task<ResumenTasas?> GetResumenAsync(DateTime? fecha, int tipoMoneda)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                TYP.TYP_FEC_TAS AS Fecha,
                TYP.TYP_TIP_MON AS TipoMoneda,
                PAR.PAR_DES_PAR AS DescripcionMoneda,
                COUNT(*) AS CantidadTramos,
                AVG(TYP_VTA_NEG) AS TasaBasePromedio,
                MIN(TYP_VTA_NEG) AS TasaBaseMinima,
                MAX(TYP_VTA_NEG) AS TasaBaseMaxima,
                AVG(TYP_VAL_TMO) AS TasaMoraPromedio
            FROM TYP
            LEFT JOIN PAR ON PAR.PAR_COD_TAB = @TabMoneda AND PAR.PAR_COD_PAR = TYP.TYP_TIP_MON
            WHERE TYP.TYP_FEC_TAS = @Fecha
              AND TYP.TYP_TIP_MON = @TipoMoneda
            GROUP BY TYP.TYP_FEC_TAS, TYP.TYP_TIP_MON, PAR.PAR_DES_PAR";

        return await connection.QueryFirstOrDefaultAsync<ResumenTasas>(sql, new 
        { 
            Fecha = fecha,
            TipoMoneda = tipoMoneda,
            TabMoneda = TAB_MONEDA
        });
    }

    public async Task<IEnumerable<HistoricoTasa>> GetHistoricoAsync(FiltroHistoricoTasaDto filtro)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                TYP.TYP_FEC_TAS AS Fecha,
                TYP.TYP_TIP_MON AS TipoMoneda,
                PAR.PAR_DES_PAR AS DescripcionMoneda,
                TYP.TYP_DIA_TAS AS DiasTramo,
                TYP.TYP_VTA_NEG AS TasaBase,
                TYP.TYP_VAL_TMO AS TasaMora,
                TYP.TYP_MAX_CON AS TasaMaximaConvencional
            FROM TYP
            LEFT JOIN PAR ON PAR.PAR_COD_TAB = @TabMoneda AND PAR.PAR_COD_PAR = TYP.TYP_TIP_MON
            WHERE TYP.TYP_FEC_TAS BETWEEN @FechaDesde AND @FechaHasta";

        var parameters = new DynamicParameters();
        parameters.Add("FechaDesde", filtro.FechaDesde);
        parameters.Add("FechaHasta", filtro.FechaHasta);
        parameters.Add("TabMoneda", TAB_MONEDA);

        if (filtro.TipoMoneda.HasValue)
        {
            sql += " AND TYP.TYP_TIP_MON = @TipoMoneda";
            parameters.Add("TipoMoneda", filtro.TipoMoneda.Value);
        }

        if (filtro.DiasTramo.HasValue)
        {
            sql += " AND TYP.TYP_DIA_TAS = @DiasTramo";
            parameters.Add("DiasTramo", filtro.DiasTramo.Value);
        }

        sql += " ORDER BY TYP.TYP_FEC_TAS, TYP.TYP_DIA_TAS";

        return await connection.QueryAsync<HistoricoTasa>(sql, parameters);
    }

    public async Task<IEnumerable<DateTime?>> GetFechasDisponiblesAsync(int tipoMoneda, int? ultimosDias = 30)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT DISTINCT TYP_FEC_TAS
            FROM TYP
            WHERE TYP_TIP_MON = @TipoMoneda
              AND TYP_FEC_TAS >= DATEADD(DAY, -@Dias, GETDATE())
            ORDER BY TYP_FEC_TAS DESC";

        return await connection.QueryAsync<DateTime?>(sql, new 
        { 
            TipoMoneda = tipoMoneda,
            Dias = ultimosDias ?? 30
        });
    }
}

/// <summary>
/// Implementación del repositorio de Indicadores Económicos
/// </summary>
public class IndicadorEconomicoRepository : IIndicadorEconomicoRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public IndicadorEconomicoRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IndicadorEconomico?> GetByCodigoYFechaAsync(string codigo, DateTime? fecha)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                IND_ID AS Id,
                IND_COD AS Codigo,
                IND_NOM AS Nombre,
                IND_FEC AS Fecha,
                IND_VAL AS Valor,
                IND_VAR_DIA AS VariacionDiaria,
                IND_VAR_MES AS VariacionMensual,
                IND_VAR_ANO AS VariacionAnual
            FROM IND
            WHERE IND_COD = @Codigo AND IND_FEC = @Fecha";

        return await connection.QueryFirstOrDefaultAsync<IndicadorEconomico>(sql, new { Codigo = codigo, Fecha = fecha });
    }

    public async Task<IndicadorEconomico?> GetUltimoAsync(string codigo)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT TOP 1
                IND_ID AS Id,
                IND_COD AS Codigo,
                IND_NOM AS Nombre,
                IND_FEC AS Fecha,
                IND_VAL AS Valor,
                IND_VAR_DIA AS VariacionDiaria,
                IND_VAR_MES AS VariacionMensual,
                IND_VAR_ANO AS VariacionAnual
            FROM IND
            WHERE IND_COD = @Codigo
            ORDER BY IND_FEC DESC";

        return await connection.QueryFirstOrDefaultAsync<IndicadorEconomico>(sql, new { Codigo = codigo });
    }

    public async Task<IEnumerable<IndicadorEconomico>> GetAllUltimosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                IND_ID AS Id,
                IND_COD AS Codigo,
                IND_NOM AS Nombre,
                IND_FEC AS Fecha,
                IND_VAL AS Valor,
                IND_VAR_DIA AS VariacionDiaria,
                IND_VAR_MES AS VariacionMensual,
                IND_VAR_ANO AS VariacionAnual
            FROM IND I1
            WHERE IND_FEC = (SELECT MAX(IND_FEC) FROM IND I2 WHERE I2.IND_COD = I1.IND_COD)
            ORDER BY IND_COD";

        return await connection.QueryAsync<IndicadorEconomico>(sql);
    }

    public async Task<IEnumerable<IndicadorEconomico>> GetHistoricoAsync(string codigo, DateTime? fechaDesde, DateTime? fechaHasta)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                IND_ID AS Id,
                IND_COD AS Codigo,
                IND_NOM AS Nombre,
                IND_FEC AS Fecha,
                IND_VAL AS Valor,
                IND_VAR_DIA AS VariacionDiaria,
                IND_VAR_MES AS VariacionMensual,
                IND_VAR_ANO AS VariacionAnual
            FROM IND
            WHERE IND_COD = @Codigo
              AND IND_FEC BETWEEN @FechaDesde AND @FechaHasta
            ORDER BY IND_FEC";

        return await connection.QueryAsync<IndicadorEconomico>(sql, new 
        { 
            Codigo = codigo,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta
        });
    }

    public async Task<bool> UpdateAsync(IndicadorEconomico indicador)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            MERGE INTO IND AS target
            USING (SELECT @Codigo AS Codigo, @Fecha AS Fecha) AS source
            ON target.IND_COD = source.Codigo AND target.IND_FEC = source.Fecha
            WHEN MATCHED THEN
                Update SET 
                    IND_VAL = @Valor,
                    IND_VAR_DIA = @VariacionDiaria,
                    IND_VAR_MES = @VariacionMensual,
                    IND_VAR_ANO = @VariacionAnual
            WHEN NOT MATCHED THEN
                INSERT (IND_COD, IND_NOM, IND_FEC, IND_VAL, IND_VAR_DIA, IND_VAR_MES, IND_VAR_ANO)
                VALUES (@Codigo, @Nombre, @Fecha, @Valor, @VariacionDiaria, @VariacionMensual, @VariacionAnual);";

        var result = await connection.ExecuteAsync(sql, indicador);
        return result > 0;
    }
}
