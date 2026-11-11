using Dapper;
using Factoring.Core.DTOs;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Factoring.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para gestión de Pagarés
    /// Migrado de F_LE1100.frm y F_LE1400.frm
    /// </summary>
    public class PagareRepository : IPagareRepository
    {
        private readonly string _connectionString;

        public PagareRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<PagareDto>> GetAllAsync(FiltroPagaresDto filtro)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    PAG.PAG_ID AS Id,
                    PAG.PAG_RUT_CLI AS RutCliente,
                    PAG.PAG_DV_CLI AS DvCliente,
                    CONCAT(FORMAT(PAG.PAG_RUT_CLI, 'N0'), '-', PAG.PAG_DV_CLI) AS RutFormateado,
                    CLI.CLI_NOM_CLI AS NombreCliente,
                    PAG.PAG_NUM_PAG AS NumeroPagare,
                    PAG.PAG_FEC_EMI AS FechaEmision,
                    PAG.PAG_FEC_VEN AS FechaVencimiento,
                    PAG.PAG_MON_ORI AS MontoOriginal,
                    PAG.PAG_MON_ACT AS MontoActual,
                    PAG.PAG_COD_MON AS CodigoMoneda,
                    PAR_MON.PAR_DES_PAR AS DescripcionMoneda,
                    PAG.PAG_ESTADO AS Estado,
                    PAR_EST.PAR_DES_PAR AS DescripcionEstado,
                    PAG.PAG_TIP_EMI AS TipoEmision,
                    PAG.PAG_NUM_OPE AS NumeroOperacion,
                    PAG.PAG_NUM_LIN AS NumeroLinea,
                    PAG.PAG_NOM_DEU AS NombreDeudor,
                    PAG.PAG_DIR_DEU AS DireccionDeudor,
                    PAG.PAG_COD_NOT AS CodigoNotaria,
                    NOT.NOT_NOM_NOT AS NombreNotaria,
                    PAG.PAG_FEC_PRO AS FechaProtocolo,
                    PAG.PAG_OBS AS Observaciones,
                    PAG.PAG_IMPUESTO AS Impuesto
                FROM PAG
                INNER JOIN CLI ON CLI.CLI_RUT_CLI = PAG.PAG_RUT_CLI
                LEFT JOIN PAR PAR_MON ON PAR_MON.PAR_COD_TAB = 3 AND PAR_MON.PAR_COD_PAR = PAG.PAG_COD_MON
                LEFT JOIN PAR PAR_EST ON PAR_EST.PAR_COD_TAB = 60 AND PAR_EST.PAR_COD_PAR = PAG.PAG_ESTADO
                LEFT JOIN NOT ON NOT.NOT_COD_NOT = PAG.PAG_COD_NOT
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (filtro.RutCliente.HasValue)
            {
                sql += " AND PAG.PAG_RUT_CLI = @RutCliente";
                parameters.Add("RutCliente", filtro.RutCliente.Value);
            }

            if (filtro.NumeroOperacion.HasValue)
            {
                sql += " AND PAG.PAG_NUM_OPE = @NumeroOperacion";
                parameters.Add("NumeroOperacion", filtro.NumeroOperacion.Value);
            }

            if (filtro.NumeroLinea.HasValue)
            {
                sql += " AND PAG.PAG_NUM_LIN = @NumeroLinea";
                parameters.Add("NumeroLinea", filtro.NumeroLinea.Value);
            }

            if (filtro.Estado.HasValue)
            {
                sql += " AND PAG.PAG_ESTADO = @Estado";
                parameters.Add("Estado", filtro.Estado.Value);
            }

            if (filtro.FechaDesde.HasValue)
            {
                sql += " AND PAG.PAG_FEC_EMI >= @FechaDesde";
                parameters.Add("FechaDesde", filtro.FechaDesde.Value);
            }

            if (filtro.FechaHasta.HasValue)
            {
                sql += " AND PAG.PAG_FEC_EMI <= @FechaHasta";
                parameters.Add("FechaHasta", filtro.FechaHasta.Value.AddDays(1).AddSeconds(-1));
            }

            if (filtro.VencimientoDesde.HasValue)
            {
                sql += " AND PAG.PAG_FEC_VEN >= @VencimientoDesde";
                parameters.Add("VencimientoDesde", filtro.VencimientoDesde.Value);
            }

            if (filtro.VencimientoHasta.HasValue)
            {
                sql += " AND PAG.PAG_FEC_VEN <= @VencimientoHasta";
                parameters.Add("VencimientoHasta", filtro.VencimientoHasta.Value.AddDays(1).AddSeconds(-1));
            }

            if (filtro.SoloVigentes == true)
            {
                sql += " AND PAG.PAG_ESTADO = 1 AND (PAG.PAG_FEC_VEN IS NULL OR PAG.PAG_FEC_VEN >= GETDATE())";
            }

            if (filtro.SoloVencidos == true)
            {
                sql += " AND PAG.PAG_FEC_VEN < GETDATE() AND PAG.PAG_ESTADO = 1";
            }

            sql += " ORDER BY PAG.PAG_FEC_EMI DESC, PAG.PAG_NUM_PAG DESC";

            return await connection.QueryAsync<PagareDto>(sql, parameters);
        }

        public async Task<PagareDto?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    PAG.PAG_ID AS Id,
                    PAG.PAG_RUT_CLI AS RutCliente,
                    PAG.PAG_DV_CLI AS DvCliente,
                    CONCAT(FORMAT(PAG.PAG_RUT_CLI, 'N0'), '-', PAG.PAG_DV_CLI) AS RutFormateado,
                    CLI.CLI_NOM_CLI AS NombreCliente,
                    PAG.PAG_NUM_PAG AS NumeroPagare,
                    PAG.PAG_FEC_EMI AS FechaEmision,
                    PAG.PAG_FEC_VEN AS FechaVencimiento,
                    PAG.PAG_MON_ORI AS MontoOriginal,
                    PAG.PAG_MON_ACT AS MontoActual,
                    PAG.PAG_COD_MON AS CodigoMoneda,
                    PAR_MON.PAR_DES_PAR AS DescripcionMoneda,
                    PAG.PAG_ESTADO AS Estado,
                    PAR_EST.PAR_DES_PAR AS DescripcionEstado,
                    PAG.PAG_TIP_EMI AS TipoEmision,
                    PAG.PAG_NUM_OPE AS NumeroOperacion,
                    PAG.PAG_NUM_LIN AS NumeroLinea,
                    PAG.PAG_NOM_DEU AS NombreDeudor,
                    PAG.PAG_DIR_DEU AS DireccionDeudor,
                    PAG.PAG_COD_NOT AS CodigoNotaria,
                    NOT.NOT_NOM_NOT AS NombreNotaria,
                    PAG.PAG_FEC_PRO AS FechaProtocolo,
                    PAG.PAG_OBS AS Observaciones,
                    PAG.PAG_IMPUESTO AS Impuesto
                FROM PAG
                INNER JOIN CLI ON CLI.CLI_RUT_CLI = PAG.PAG_RUT_CLI
                LEFT JOIN PAR PAR_MON ON PAR_MON.PAR_COD_TAB = 3 AND PAR_MON.PAR_COD_PAR = PAG.PAG_COD_MON
                LEFT JOIN PAR PAR_EST ON PAR_EST.PAR_COD_TAB = 60 AND PAR_EST.PAR_COD_PAR = PAG.PAG_ESTADO
                LEFT JOIN NOT ON NOT.NOT_COD_NOT = PAG.PAG_COD_NOT
                WHERE PAG.PAG_ID = @Id";

            return await connection.QueryFirstOrDefaultAsync<PagareDto>(sql, new { Id = id });
        }

        public async Task<PagareDto?> GetByNumeroAsync(int numeroPagare)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    PAG.PAG_ID AS Id,
                    PAG.PAG_RUT_CLI AS RutCliente,
                    PAG.PAG_DV_CLI AS DvCliente,
                    CLI.CLI_NOM_CLI AS NombreCliente,
                    PAG.PAG_NUM_PAG AS NumeroPagare,
                    PAG.PAG_FEC_EMI AS FechaEmision,
                    PAG.PAG_FEC_VEN AS FechaVencimiento,
                    PAG.PAG_MON_ORI AS MontoOriginal,
                    PAG.PAG_MON_ACT AS MontoActual,
                    PAG.PAG_ESTADO AS Estado,
                    PAR_EST.PAR_DES_PAR AS DescripcionEstado
                FROM PAG
                INNER JOIN CLI ON CLI.CLI_RUT_CLI = PAG.PAG_RUT_CLI
                LEFT JOIN PAR PAR_EST ON PAR_EST.PAR_COD_TAB = 60 AND PAR_EST.PAR_COD_PAR = PAG.PAG_ESTADO
                WHERE PAG.PAG_NUM_PAG = @NumeroPagare";

            return await connection.QueryFirstOrDefaultAsync<PagareDto>(sql, new { NumeroPagare = numeroPagare });
        }

        public async Task<IEnumerable<PagareDto>> GetByClienteAsync(int rutCliente)
        {
            return await GetAllAsync(new FiltroPagaresDto { RutCliente = rutCliente });
        }

        public async Task<IEnumerable<PagareDto>> GetByOperacionAsync(int numeroOperacion)
        {
            return await GetAllAsync(new FiltroPagaresDto { NumeroOperacion = numeroOperacion });
        }

        public async Task<IEnumerable<PagareDto>> GetByLineaAsync(int numeroLinea)
        {
            return await GetAllAsync(new FiltroPagaresDto { NumeroLinea = numeroLinea });
        }

        public async Task<IEnumerable<PagareDto>> GetVigentesAsync()
        {
            return await GetAllAsync(new FiltroPagaresDto { SoloVigentes = true });
        }

        public async Task<IEnumerable<PagareDto>> GetVencidosAsync()
        {
            return await GetAllAsync(new FiltroPagaresDto { SoloVencidos = true });
        }

        public async Task<IEnumerable<PagareDto>> GetPorVencerAsync(int diasAnticipacion = 30)
        {
            return await GetAllAsync(new FiltroPagaresDto
            {
                VencimientoDesde = DateTime.Today,
                VencimientoHasta = DateTime.Today.AddDays(diasAnticipacion),
                Estado = 1 // Vigente
            });
        }

        public async Task<int> CreateAsync(CreatePagareDto dto, int usuarioId)
        {
            using var connection = CreateConnection();

            var numeroPagare = await GetSiguienteNumeroAsync();

            var sql = @"
                INSERT INTO PAG (
                    PAG_RUT_CLI, PAG_DV_CLI, PAG_NUM_PAG, PAG_FEC_EMI, PAG_FEC_VEN,
                    PAG_MON_ORI, PAG_MON_ACT, PAG_COD_MON, PAG_ESTADO, PAG_TIP_EMI,
                    PAG_NUM_OPE, PAG_NUM_LIN, PAG_NUM_SUB, PAG_NOM_DEU, PAG_DIR_DEU,
                    PAG_COM_DEU, PAG_CIU_DEU, PAG_COD_NOT, PAG_FEC_PRO, PAG_NUM_REP,
                    PAG_IMPUESTO, PAG_OBS, PAG_CTA_CTE, PAG_FEC_CRE, PAG_USR_CRE
                )
                OUTPUT INSERTED.PAG_ID
                VALUES (
                    @RutCliente, @DvCliente, @NumeroPagare, @FechaEmision, @FechaVencimiento,
                    @MontoOriginal, @MontoOriginal, @CodigoMoneda, 1, @TipoEmision,
                    @NumeroOperacion, @NumeroLinea, @NumeroSublinea, @NombreDeudor, @DireccionDeudor,
                    @ComunaDeudor, @CiudadDeudor, @CodigoNotaria, @FechaProtocolo, @NumeroRepertorio,
                    @Impuesto, @Observaciones, @CuentaCorriente, GETDATE(), @UsuarioId
                )";

            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                dto.RutCliente,
                dto.DvCliente,
                NumeroPagare = numeroPagare,
                dto.FechaEmision,
                dto.FechaVencimiento,
                dto.MontoOriginal,
                dto.CodigoMoneda,
                dto.TipoEmision,
                dto.NumeroOperacion,
                dto.NumeroLinea,
                dto.NumeroSublinea,
                dto.NombreDeudor,
                dto.DireccionDeudor,
                dto.ComunaDeudor,
                dto.CiudadDeudor,
                dto.CodigoNotaria,
                dto.FechaProtocolo,
                dto.NumeroRepertorio,
                dto.Impuesto,
                dto.Observaciones,
                dto.CuentaCorriente,
                UsuarioId = usuarioId
            });
        }

        public async Task<bool> UpdateAsync(int id, UpdatePagareDto dto, int usuarioId)
        {
            using var connection = CreateConnection();

            var sql = @"
                Update PAG SET
                    PAG_FEC_VEN = COALESCE(@FechaVencimiento, PAG_FEC_VEN),
                    PAG_MON_ACT = COALESCE(@MontoActual, PAG_MON_ACT),
                    PAG_ESTADO = COALESCE(@Estado, PAG_ESTADO),
                    PAG_NOM_DEU = CASE WHEN @NombreDeudor <> '' THEN @NombreDeudor ELSE PAG_NOM_DEU END,
                    PAG_DIR_DEU = CASE WHEN @DireccionDeudor <> '' THEN @DireccionDeudor ELSE PAG_DIR_DEU END,
                    PAG_COD_NOT = COALESCE(@CodigoNotaria, PAG_COD_NOT),
                    PAG_FEC_PRO = COALESCE(@FechaProtocolo, PAG_FEC_PRO),
                    PAG_NUM_REP = CASE WHEN @NumeroRepertorio <> '' THEN @NumeroRepertorio ELSE PAG_NUM_REP END,
                    PAG_OBS = CASE WHEN @Observaciones <> '' THEN @Observaciones ELSE PAG_OBS END,
                    PAG_FEC_MOD = GETDATE(),
                    PAG_USR_MOD = @UsuarioId
                WHERE PAG_ID = @Id";

            var rows = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                dto.FechaVencimiento,
                dto.MontoActual,
                dto.Estado,
                dto.NombreDeudor,
                dto.DireccionDeudor,
                dto.CodigoNotaria,
                dto.FechaProtocolo,
                dto.NumeroRepertorio,
                dto.Observaciones,
                UsuarioId = usuarioId
            });

            return rows > 0;
        }

        public async Task<bool> CambiarEstadoAsync(int id, int nuevoEstado, string motivo, int usuarioId)
        {
            using var connection = CreateConnection();

            var sql = @"
                Update PAG SET
                    PAG_ESTADO = @NuevoEstado,
                    PAG_OBS = CASE WHEN @Motivo <> '' THEN CONCAT(PAG_OBS, ' | ', @Motivo) ELSE PAG_OBS END,
                    PAG_FEC_MOD = GETDATE(),
                    PAG_USR_MOD = @UsuarioId
                WHERE PAG_ID = @Id";

            var rows = await connection.ExecuteAsync(sql, new { Id = id, NuevoEstado = nuevoEstado, Motivo = motivo, UsuarioId = usuarioId });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            using var connection = CreateConnection();

            // Soft delete o marcar como anulado
            var sql = @"
                Update PAG SET
                    PAG_ESTADO = 9, -- Anulado
                    PAG_FEC_MOD = GETDATE(),
                    PAG_USR_MOD = @UsuarioId
                WHERE PAG_ID = @Id";

            var rows = await connection.ExecuteAsync(sql, new { Id = id, UsuarioId = usuarioId });
            return rows > 0;
        }

        public async Task<int> GetSiguienteNumeroAsync()
        {
            using var connection = CreateConnection();
            var sql = "SELECT ISNULL(MAX(PAG_NUM_PAG), 0) + 1 FROM PAG";
            return await connection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<decimal> GetMontoTotalVigenteByClienteAsync(int rutCliente)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT ISNULL(SUM(PAG_MON_ACT), 0)
                FROM PAG
                WHERE PAG_RUT_CLI = @RutCliente
                  AND PAG_ESTADO = 1
                  AND (PAG_FEC_VEN IS NULL OR PAG_FEC_VEN >= GETDATE())";

            return await connection.ExecuteScalarAsync<decimal>(sql, new { RutCliente = rutCliente });
        }
    }

    /// <summary>
    /// Repositorio para gestión de Avales
    /// Migrado de F_LE1200.frm y F_LE1500.frm
    /// </summary>
    public class AvalRepository : IAvalRepository
    {
        private readonly string _connectionString;

        public AvalRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<AvalDto>> GetAllAsync(FiltroAvalesDto filtro)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    AVA.AVA_ID AS Id,
                    AVA.AVA_RUT_CLI AS RutCliente,
                    AVA.AVA_DV_CLI AS DvCliente,
                    CONCAT(FORMAT(AVA.AVA_RUT_CLI, 'N0'), '-', AVA.AVA_DV_CLI) AS RutClienteFormateado,
                    CLI.CLI_NOM_CLI AS NombreCliente,
                    AVA.AVA_RUT_AVA AS RutAval,
                    AVA.AVA_DV_AVA AS DvAval,
                    CONCAT(FORMAT(AVA.AVA_RUT_AVA, 'N0'), '-', AVA.AVA_DV_AVA) AS RutAvalFormateado,
                    AVA.AVA_NOM_AVA AS NombreAval,
                    AVA.AVA_TIP_AVA AS TipoAval,
                    PAR_TIP.PAR_DES_PAR AS DescripcionTipoAval,
                    AVA.AVA_EST_AVA AS EstadoAval,
                    PAR_EST.PAR_DES_PAR AS DescripcionEstado,
                    AVA.AVA_REG_MAT AS RegimenMatrimonial,
                    PAR_REG.PAR_DES_PAR AS DescripcionRegimenMatrimonial,
                    AVA.AVA_DIR_AVA AS Direccion,
                    AVA.AVA_COM_AVA AS CodigoComuna,
                    PAR_COM.PAR_DES_PAR AS DescripcionComuna,
                    AVA.AVA_TLF_AVA AS Telefono,
                    AVA.AVA_EMA_AVA AS Email,
                    AVA.AVA_PAT_DEC AS PatrimonioDeclarado,
                    AVA.AVA_FEC_ING AS FechaIngreso,
                    AVA.AVA_FEC_VIG AS FechaVigencia,
                    AVA.AVA_SOCIO AS EsSocio,
                    AVA.AVA_OBS AS Observaciones
                FROM AVA
                INNER JOIN CLI ON CLI.CLI_RUT_CLI = AVA.AVA_RUT_CLI
                LEFT JOIN PAR PAR_TIP ON PAR_TIP.PAR_COD_TAB = 61 AND PAR_TIP.PAR_COD_PAR = AVA.AVA_TIP_AVA
                LEFT JOIN PAR PAR_EST ON PAR_EST.PAR_COD_TAB = 62 AND PAR_EST.PAR_COD_PAR = AVA.AVA_EST_AVA
                LEFT JOIN PAR PAR_REG ON PAR_REG.PAR_COD_TAB = 63 AND PAR_REG.PAR_COD_PAR = AVA.AVA_REG_MAT
                LEFT JOIN PAR PAR_COM ON PAR_COM.PAR_COD_TAB = 5 AND PAR_COM.PAR_COD_PAR = AVA.AVA_COM_AVA
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (!filtro.ConsultarTodos && filtro.RutCliente.HasValue)
            {
                sql += " AND AVA.AVA_RUT_CLI = @RutCliente";
                parameters.Add("RutCliente", filtro.RutCliente.Value);
            }

            if (filtro.RutAval.HasValue)
            {
                sql += " AND AVA.AVA_RUT_AVA = @RutAval";
                parameters.Add("RutAval", filtro.RutAval.Value);
            }

            if (filtro.TipoAval.HasValue)
            {
                sql += " AND AVA.AVA_TIP_AVA = @TipoAval";
                parameters.Add("TipoAval", filtro.TipoAval.Value);
            }

            if (filtro.Estado.HasValue)
            {
                sql += " AND AVA.AVA_EST_AVA = @Estado";
                parameters.Add("Estado", filtro.Estado.Value);
            }

            if (filtro.SoloActivos == true)
            {
                sql += " AND AVA.AVA_EST_AVA = 1";
            }

            if (filtro.SoloSocios == true)
            {
                sql += " AND AVA.AVA_SOCIO = 1";
            }

            sql += " ORDER BY AVA.AVA_RUT_AVA";

            return await connection.QueryAsync<AvalDto>(sql, parameters);
        }

        public async Task<AvalDto?> GetByIdAsync(int id)
        {
            var filtro = new FiltroAvalesDto { ConsultarTodos = true };
            var avales = await GetAllAsync(filtro);
            return avales.FirstOrDefault(a => a.Id == id);
        }

        public async Task<IEnumerable<AvalDto>> GetByClienteAsync(int rutCliente)
        {
            return await GetAllAsync(new FiltroAvalesDto { RutCliente = rutCliente });
        }

        public async Task<IEnumerable<AvalDto>> GetByRutAvalAsync(int rutAval)
        {
            return await GetAllAsync(new FiltroAvalesDto { RutAval = rutAval, ConsultarTodos = true });
        }

        public async Task<IEnumerable<AvalDto>> GetActivosAsync()
        {
            return await GetAllAsync(new FiltroAvalesDto { SoloActivos = true, ConsultarTodos = true });
        }

        public async Task<IEnumerable<AvalDto>> GetPorVencerAsync(int diasAnticipacion = 30)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    AVA.AVA_ID AS Id,
                    AVA.AVA_RUT_CLI AS RutCliente,
                    AVA.AVA_RUT_AVA AS RutAval,
                    AVA.AVA_NOM_AVA AS NombreAval,
                    AVA.AVA_FEC_VIG AS FechaVigencia,
                    AVA.AVA_EST_AVA AS EstadoAval,
                    CLI.CLI_NOM_CLI AS NombreCliente
                FROM AVA
                INNER JOIN CLI ON CLI.CLI_RUT_CLI = AVA.AVA_RUT_CLI
                WHERE AVA.AVA_EST_AVA = 1
                  AND AVA.AVA_FEC_VIG BETWEEN GETDATE() AND DATEADD(DAY, @Dias, GETDATE())
                ORDER BY AVA.AVA_FEC_VIG";

            return await connection.QueryAsync<AvalDto>(sql, new { Dias = diasAnticipacion });
        }

        public async Task<int> CreateAsync(CreateAvalDto dto, int usuarioId)
        {
            using var connection = CreateConnection();

            var sql = @"
                INSERT INTO AVA (
                    AVA_RUT_CLI, AVA_DV_CLI, AVA_RUT_AVA, AVA_DV_AVA, AVA_NOM_AVA,
                    AVA_TIP_AVA, AVA_EST_AVA, AVA_REG_MAT, AVA_DIR_AVA, AVA_COM_AVA,
                    AVA_TLF_AVA, AVA_EMA_AVA, AVA_PAT_DEC, AVA_FEC_ING, AVA_FEC_VIG,
                    AVA_SOCIO, AVA_OBS, AVA_FEC_CRE, AVA_USR_CRE
                )
                OUTPUT INSERTED.AVA_ID
                VALUES (
                    @RutCliente, @DvCliente, @RutAval, @DvAval, @NombreAval,
                    @TipoAval, 1, @RegimenMatrimonial, @Direccion, @CodigoComuna,
                    @Telefono, @Email, @PatrimonioDeclarado, @FechaIngreso, @FechaVigencia,
                    @EsSocio, @Observaciones, GETDATE(), @UsuarioId
                )";

            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                dto.RutCliente,
                dto.DvCliente,
                dto.RutAval,
                dto.DvAval,
                dto.NombreAval,
                dto.TipoAval,
                dto.RegimenMatrimonial,
                dto.Direccion,
                dto.CodigoComuna,
                dto.Telefono,
                dto.Email,
                dto.PatrimonioDeclarado,
                FechaIngreso = dto.FechaIngreso ?? DateTime.Today,
                dto.FechaVigencia,
                dto.EsSocio,
                dto.Observaciones,
                UsuarioId = usuarioId
            });
        }

        public async Task<bool> UpdateAsync(int id, UpdateAvalDto dto, int usuarioId)
        {
            using var connection = CreateConnection();

            var sql = @"
                Update AVA SET
                    AVA_NOM_AVA = CASE WHEN @NombreAval <> '' THEN @NombreAval ELSE AVA_NOM_AVA END,
                    AVA_TIP_AVA = COALESCE(@TipoAval, AVA_TIP_AVA),
                    AVA_EST_AVA = COALESCE(@EstadoAval, AVA_EST_AVA),
                    AVA_REG_MAT = COALESCE(@RegimenMatrimonial, AVA_REG_MAT),
                    AVA_DIR_AVA = CASE WHEN @Direccion <> '' THEN @Direccion ELSE AVA_DIR_AVA END,
                    AVA_COM_AVA = COALESCE(@CodigoComuna, AVA_COM_AVA),
                    AVA_TLF_AVA = CASE WHEN @Telefono <> '' THEN @Telefono ELSE AVA_TLF_AVA END,
                    AVA_EMA_AVA = CASE WHEN @Email <> '' THEN @Email ELSE AVA_EMA_AVA END,
                    AVA_PAT_DEC = COALESCE(@PatrimonioDeclarado, AVA_PAT_DEC),
                    AVA_FEC_VIG = COALESCE(@FechaVigencia, AVA_FEC_VIG),
                    AVA_SOCIO = COALESCE(@EsSocio, AVA_SOCIO),
                    AVA_OBS = CASE WHEN @Observaciones <> '' THEN @Observaciones ELSE AVA_OBS END,
                    AVA_FEC_MOD = GETDATE(),
                    AVA_USR_MOD = @UsuarioId
                WHERE AVA_ID = @Id";

            var rows = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                dto.NombreAval,
                dto.TipoAval,
                dto.EstadoAval,
                dto.RegimenMatrimonial,
                dto.Direccion,
                dto.CodigoComuna,
                dto.Telefono,
                dto.Email,
                dto.PatrimonioDeclarado,
                dto.FechaVigencia,
                dto.EsSocio,
                dto.Observaciones,
                UsuarioId = usuarioId
            });

            return rows > 0;
        }

        public async Task<bool> CambiarEstadoAsync(int id, int nuevoEstado, int usuarioId)
        {
            using var connection = CreateConnection();

            var sql = @"
                Update AVA SET
                    AVA_EST_AVA = @NuevoEstado,
                    AVA_FEC_MOD = GETDATE(),
                    AVA_USR_MOD = @UsuarioId
                WHERE AVA_ID = @Id";

            var rows = await connection.ExecuteAsync(sql, new { Id = id, NuevoEstado = nuevoEstado, UsuarioId = usuarioId });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            // Soft delete
            return await CambiarEstadoAsync(id, 4, usuarioId); // 4 = Liberado/Eliminado
        }

        public async Task<bool> ExisteAvalParaClienteAsync(int rutCliente, int rutAval)
        {
            using var connection = CreateConnection();
            var sql = "SELECT COUNT(1) FROM AVA WHERE AVA_RUT_CLI = @RutCliente AND AVA_RUT_AVA = @RutAval AND AVA_EST_AVA = 1";
            return await connection.ExecuteScalarAsync<int>(sql, new { RutCliente = rutCliente, RutAval = rutAval }) > 0;
        }

        public async Task<int> GetCountActivosByClienteAsync(int rutCliente)
        {
            using var connection = CreateConnection();
            var sql = "SELECT COUNT(1) FROM AVA WHERE AVA_RUT_CLI = @RutCliente AND AVA_EST_AVA = 1";
            return await connection.ExecuteScalarAsync<int>(sql, new { RutCliente = rutCliente });
        }
    }

    /// <summary>
    /// Repositorio para gestión de Firmas
    /// Migrado de F_LE1300.frm y F_LE1600.frm
    /// </summary>
    public class FirmaRepository : IFirmaRepository
    {
        private readonly string _connectionString;

        public FirmaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<FirmaDto>> GetAllAsync(FiltroFirmasDto filtro)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    FIR.FIR_ID AS Id,
                    FIR.FIR_RUT_CLI AS RutCliente,
                    FIR.FIR_DV_CLI AS DvCliente,
                    CONCAT(FORMAT(FIR.FIR_RUT_CLI, 'N0'), '-', FIR.FIR_DV_CLI) AS RutClienteFormateado,
                    CLI.CLI_NOM_CLI AS NombreCliente,
                    FIR.FIR_RUT_FIR AS RutFirmante,
                    FIR.FIR_DV_FIR AS DvFirmante,
                    CONCAT(FORMAT(FIR.FIR_RUT_FIR, 'N0'), '-', FIR.FIR_DV_FIR) AS RutFirmanteFormateado,
                    FIR.FIR_NOM_FIR AS NombreFirmante,
                    FIR.FIR_CAR_FIR AS Cargo,
                    FIR.FIR_FEC_DES AS FechaVigenciaDesde,
                    FIR.FIR_FEC_HAS AS FechaVigenciaHasta,
                    FIR.FIR_ACTIVA AS Activa,
                    FIR.FIR_TIP_DOC AS TipoDocumentoFirma,
                    FIR.FIR_OBS AS Observaciones
                FROM FIR
                INNER JOIN CLI ON CLI.CLI_RUT_CLI = FIR.FIR_RUT_CLI
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (!filtro.ConsultarTodos && filtro.RutCliente.HasValue)
            {
                sql += " AND FIR.FIR_RUT_CLI = @RutCliente";
                parameters.Add("RutCliente", filtro.RutCliente.Value);
            }

            if (filtro.RutFirmante.HasValue)
            {
                sql += " AND FIR.FIR_RUT_FIR = @RutFirmante";
                parameters.Add("RutFirmante", filtro.RutFirmante.Value);
            }

            if (filtro.FechaVigenciaDesde.HasValue)
            {
                sql += " AND FIR.FIR_FEC_DES >= @FechaDesde";
                parameters.Add("FechaDesde", filtro.FechaVigenciaDesde.Value);
            }

            if (filtro.FechaVigenciaHasta.HasValue)
            {
                sql += " AND FIR.FIR_FEC_HAS <= @FechaHasta";
                parameters.Add("FechaHasta", filtro.FechaVigenciaHasta.Value);
            }

            if (filtro.SoloActivas == true)
            {
                sql += " AND FIR.FIR_ACTIVA = 1 AND (FIR.FIR_FEC_HAS IS NULL OR FIR.FIR_FEC_HAS >= GETDATE())";
            }

            sql += " ORDER BY FIR.FIR_NOM_FIR";

            return await connection.QueryAsync<FirmaDto>(sql, parameters);
        }

        public async Task<FirmaDto?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT 
                    FIR.FIR_ID AS Id,
                    FIR.FIR_RUT_CLI AS RutCliente,
                    FIR.FIR_RUT_FIR AS RutFirmante,
                    FIR.FIR_NOM_FIR AS NombreFirmante,
                    FIR.FIR_CAR_FIR AS Cargo,
                    FIR.FIR_FEC_DES AS FechaVigenciaDesde,
                    FIR.FIR_FEC_HAS AS FechaVigenciaHasta,
                    FIR.FIR_ACTIVA AS Activa,
                    FIR.FIR_OBS AS Observaciones,
                    CLI.CLI_NOM_CLI AS NombreCliente
                FROM FIR
                INNER JOIN CLI ON CLI.CLI_RUT_CLI = FIR.FIR_RUT_CLI
                WHERE FIR.FIR_ID = @Id";
            return await connection.QueryFirstOrDefaultAsync<FirmaDto>(sql, new { Id = id });
        }

        public async Task<IEnumerable<FirmaDto>> GetByClienteAsync(int rutCliente)
        {
            return await GetAllAsync(new FiltroFirmasDto { RutCliente = rutCliente });
        }

        public async Task<IEnumerable<FirmaDto>> GetByRutFirmanteAsync(int rutFirmante)
        {
            return await GetAllAsync(new FiltroFirmasDto { RutFirmante = rutFirmante, ConsultarTodos = true });
        }

        public async Task<IEnumerable<FirmaDto>> GetVigentesAsync()
        {
            return await GetAllAsync(new FiltroFirmasDto { SoloActivas = true, ConsultarTodos = true });
        }

        public async Task<IEnumerable<FirmaDto>> GetPorVencerAsync(int diasAnticipacion = 30)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT 
                    FIR.FIR_ID AS Id,
                    FIR.FIR_RUT_CLI AS RutCliente,
                    FIR.FIR_RUT_FIR AS RutFirmante,
                    FIR.FIR_NOM_FIR AS NombreFirmante,
                    FIR.FIR_FEC_HAS AS FechaVigenciaHasta,
                    CLI.CLI_NOM_CLI AS NombreCliente
                FROM FIR
                INNER JOIN CLI ON CLI.CLI_RUT_CLI = FIR.FIR_RUT_CLI
                WHERE FIR.FIR_ACTIVA = 1
                  AND FIR.FIR_FEC_HAS BETWEEN GETDATE() AND DATEADD(DAY, @Dias, GETDATE())
                ORDER BY FIR.FIR_FEC_HAS";
            return await connection.QueryAsync<FirmaDto>(sql, new { Dias = diasAnticipacion });
        }

        public async Task<int> CreateAsync(CreateFirmaDto dto, int usuarioId)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO FIR (
                    FIR_RUT_CLI, FIR_DV_CLI, FIR_RUT_FIR, FIR_DV_FIR, FIR_NOM_FIR,
                    FIR_CAR_FIR, FIR_FEC_DES, FIR_FEC_HAS, FIR_ACTIVA, FIR_TIP_DOC,
                    FIR_OBS, FIR_FEC_CRE, FIR_USR_CRE
                )
                OUTPUT INSERTED.FIR_ID
                VALUES (
                    @RutCliente, @DvCliente, @RutFirmante, @DvFirmante, @NombreFirmante,
                    @Cargo, @FechaVigenciaDesde, @FechaVigenciaHasta, 1, @TipoDocumentoFirma,
                    @Observaciones, GETDATE(), @UsuarioId
                )";

            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                dto.RutCliente,
                dto.DvCliente,
                dto.RutFirmante,
                dto.DvFirmante,
                dto.NombreFirmante,
                dto.Cargo,
                FechaVigenciaDesde = dto.FechaVigenciaDesde ?? DateTime.Today,
                dto.FechaVigenciaHasta,
                dto.TipoDocumentoFirma,
                dto.Observaciones,
                UsuarioId = usuarioId
            });
        }

        public async Task<bool> UpdateAsync(int id, UpdateFirmaDto dto, int usuarioId)
        {
            using var connection = CreateConnection();
            var sql = @"
                Update FIR SET
                    FIR_NOM_FIR = CASE WHEN @NombreFirmante <> '' THEN @NombreFirmante ELSE FIR_NOM_FIR END,
                    FIR_CAR_FIR = CASE WHEN @Cargo <> '' THEN @Cargo ELSE FIR_CAR_FIR END,
                    FIR_FEC_DES = COALESCE(@FechaVigenciaDesde, FIR_FEC_DES),
                    FIR_FEC_HAS = COALESCE(@FechaVigenciaHasta, FIR_FEC_HAS),
                    FIR_ACTIVA = COALESCE(@Activa, FIR_ACTIVA),
                    FIR_TIP_DOC = CASE WHEN @TipoDocumentoFirma <> '' THEN @TipoDocumentoFirma ELSE FIR_TIP_DOC END,
                    FIR_OBS = CASE WHEN @Observaciones <> '' THEN @Observaciones ELSE FIR_OBS END,
                    FIR_FEC_MOD = GETDATE(),
                    FIR_USR_MOD = @UsuarioId
                WHERE FIR_ID = @Id";

            var rows = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                dto.NombreFirmante,
                dto.Cargo,
                dto.FechaVigenciaDesde,
                dto.FechaVigenciaHasta,
                dto.Activa,
                dto.TipoDocumentoFirma,
                dto.Observaciones,
                UsuarioId = usuarioId
            });
            return rows > 0;
        }

        public async Task<bool> ActivarDesactivarAsync(int id, bool activa, int usuarioId)
        {
            using var connection = CreateConnection();
            var sql = "Update FIR SET FIR_ACTIVA = @Activa, FIR_FEC_MOD = GETDATE(), FIR_USR_MOD = @UsuarioId WHERE FIR_ID = @Id";
            var rows = await connection.ExecuteAsync(sql, new { Id = id, Activa = activa, UsuarioId = usuarioId });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            return await ActivarDesactivarAsync(id, false, usuarioId);
        }

        public async Task<bool> ExisteFirmaParaClienteAsync(int rutCliente, int rutFirmante)
        {
            using var connection = CreateConnection();
            var sql = "SELECT COUNT(1) FROM FIR WHERE FIR_RUT_CLI = @RutCliente AND FIR_RUT_FIR = @RutFirmante AND FIR_ACTIVA = 1";
            return await connection.ExecuteScalarAsync<int>(sql, new { RutCliente = rutCliente, RutFirmante = rutFirmante }) > 0;
        }

        public async Task<int> GetCountVigentesByClienteAsync(int rutCliente)
        {
            using var connection = CreateConnection();
            var sql = "SELECT COUNT(1) FROM FIR WHERE FIR_RUT_CLI = @RutCliente AND FIR_ACTIVA = 1";
            return await connection.ExecuteScalarAsync<int>(sql, new { RutCliente = rutCliente });
        }
    }

    /// <summary>
    /// Repositorio para resúmenes del módulo Legal
    /// </summary>
    public class LegalRepository : ILegalRepository
    {
        private readonly string _connectionString;

        public LegalRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<ResumenLegalDto> GetResumenGeneralAsync()
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT
                    (SELECT COUNT(*) FROM PAG) AS TotalPagares,
                    (SELECT COUNT(*) FROM PAG WHERE PAG_ESTADO = 1 AND (PAG_FEC_VEN IS NULL OR PAG_FEC_VEN >= GETDATE())) AS PagaresVigentes,
                    (SELECT COUNT(*) FROM PAG WHERE PAG_FEC_VEN < GETDATE() AND PAG_ESTADO = 1) AS PagaresVencidos,
                    (SELECT COUNT(*) FROM PAG WHERE CAST(PAG_FEC_EMI AS Dayjs) = CAST(GETDATE() AS Dayjs)) AS PagaresEmitidosHoy,
                    (SELECT ISNULL(SUM(PAG_MON_ACT), 0) FROM PAG WHERE PAG_ESTADO = 1) AS MontoTotalPagares,
                    (SELECT COUNT(*) FROM AVA) AS TotalAvales,
                    (SELECT COUNT(*) FROM AVA WHERE AVA_EST_AVA = 1) AS AvalesActivos,
                    (SELECT COUNT(*) FROM AVA WHERE AVA_EST_AVA = 1 AND AVA_FEC_VIG BETWEEN GETDATE() AND DATEADD(DAY, 30, GETDATE())) AS AvalesPorVencer,
                    (SELECT COUNT(*) FROM FIR) AS TotalFirmas,
                    (SELECT COUNT(*) FROM FIR WHERE FIR_ACTIVA = 1) AS FirmasVigentes,
                    (SELECT COUNT(*) FROM FIR WHERE FIR_ACTIVA = 1 AND FIR_FEC_HAS BETWEEN GETDATE() AND DATEADD(DAY, 30, GETDATE())) AS FirmasPorVencer,
                    0 AS TotalContratosMarco,
                    0 AS MontoTotalLineas,
                    0 AS MontoDisponibleLineas";

            return await connection.QueryFirstAsync<ResumenLegalDto>(sql);
        }

        public async Task<ResumenGarantiasClienteDto> GetResumenByClienteAsync(int rutCliente)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT
                    @RutCliente AS RutCliente,
                    CLI.CLI_DIG_VER AS DvCliente,
                    CONCAT(FORMAT(@RutCliente, 'N0'), '-', CLI.CLI_DIG_VER) AS RutFormateado,
                    CLI.CLI_NOM_CLI AS NombreCliente,
                    (SELECT COUNT(*) FROM PAG WHERE PAG_RUT_CLI = @RutCliente) AS TotalPagares,
                    (SELECT COUNT(*) FROM PAG WHERE PAG_RUT_CLI = @RutCliente AND PAG_ESTADO = 1) AS PagaresVigentes,
                    (SELECT ISNULL(SUM(PAG_MON_ACT), 0) FROM PAG WHERE PAG_RUT_CLI = @RutCliente AND PAG_ESTADO = 1) AS MontoPagaresVigentes,
                    (SELECT COUNT(*) FROM AVA WHERE AVA_RUT_CLI = @RutCliente) AS TotalAvales,
                    (SELECT COUNT(*) FROM AVA WHERE AVA_RUT_CLI = @RutCliente AND AVA_EST_AVA = 1) AS AvalesActivos,
                    (SELECT COUNT(*) FROM FIR WHERE FIR_RUT_CLI = @RutCliente) AS TotalFirmas,
                    (SELECT COUNT(*) FROM FIR WHERE FIR_RUT_CLI = @RutCliente AND FIR_ACTIVA = 1) AS FirmasVigentes,
                    0 AS TotalContratosMarco,
                    0 AS MontoLineasAprobadas,
                    0 AS MontoLineasDisponibles
                FROM CLI
                WHERE CLI.CLI_RUT_CLI = @RutCliente";

            return await connection.QueryFirstOrDefaultAsync<ResumenGarantiasClienteDto>(sql, new { RutCliente = rutCliente })
                ?? new ResumenGarantiasClienteDto { RutCliente = rutCliente };
        }

        public async Task<IEnumerable<ResumenGarantiasClienteDto>> GetResumenTodosClientesAsync()
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT
                    CLI.CLI_RUT_CLI AS RutCliente,
                    CLI.CLI_DIG_VER AS DvCliente,
                    CONCAT(FORMAT(CLI.CLI_RUT_CLI, 'N0'), '-', CLI.CLI_DIG_VER) AS RutFormateado,
                    CLI.CLI_NOM_CLI AS NombreCliente,
                    ISNULL(PAG_CNT.Total, 0) AS TotalPagares,
                    ISNULL(PAG_CNT.Vigentes, 0) AS PagaresVigentes,
                    ISNULL(PAG_CNT.MontoVigente, 0) AS MontoPagaresVigentes,
                    ISNULL(AVA_CNT.Total, 0) AS TotalAvales,
                    ISNULL(AVA_CNT.Activos, 0) AS AvalesActivos,
                    ISNULL(FIR_CNT.Total, 0) AS TotalFirmas,
                    ISNULL(FIR_CNT.Vigentes, 0) AS FirmasVigentes
                FROM CLI
                LEFT JOIN (
                    SELECT PAG_RUT_CLI, 
                           COUNT(*) AS Total,
                           SUM(CASE WHEN PAG_ESTADO = 1 THEN 1 ELSE 0 END) AS Vigentes,
                           SUM(CASE WHEN PAG_ESTADO = 1 THEN PAG_MON_ACT ELSE 0 END) AS MontoVigente
                    FROM PAG GROUP BY PAG_RUT_CLI
                ) PAG_CNT ON PAG_CNT.PAG_RUT_CLI = CLI.CLI_RUT_CLI
                LEFT JOIN (
                    SELECT AVA_RUT_CLI,
                           COUNT(*) AS Total,
                           SUM(CASE WHEN AVA_EST_AVA = 1 THEN 1 ELSE 0 END) AS Activos
                    FROM AVA GROUP BY AVA_RUT_CLI
                ) AVA_CNT ON AVA_CNT.AVA_RUT_CLI = CLI.CLI_RUT_CLI
                LEFT JOIN (
                    SELECT FIR_RUT_CLI,
                           COUNT(*) AS Total,
                           SUM(CASE WHEN FIR_ACTIVA = 1 THEN 1 ELSE 0 END) AS Vigentes
                    FROM FIR GROUP BY FIR_RUT_CLI
                ) FIR_CNT ON FIR_CNT.FIR_RUT_CLI = CLI.CLI_RUT_CLI
                WHERE CLI.CLI_EST_CLI = 'A'
                ORDER BY CLI.CLI_NOM_CLI";

            return await connection.QueryAsync<ResumenGarantiasClienteDto>(sql);
        }

        public async Task<bool> RegistrarHistorialAsync(string tipoEntidad, int idEntidad, string accion, string descripcion, int usuarioId)
        {
            // Por ahora retornamos true - Se puede implementar tabla HIS_LEG
            return true;
        }

        public async Task<IEnumerable<HistorialLegal>> GetHistorialByEntidadAsync(string tipoEntidad, int idEntidad)
        {
            return new List<HistorialLegal>();
        }
    }

    /// <summary>
    /// Repositorio para gestión de Contratos Marco
    /// Migrado de F_LE1700.frm - Contrato Global en Pesos
    /// </summary>
    public class ContratoMarcoRepository : IContratoMarcoRepository
    {
        private readonly string _connectionString;

        public ContratoMarcoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<ContratoMarcoDto>> GetAllAsync(FiltroContratosMarcoDto filtro)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    CM.CM_ID AS Id,
                    CM.CM_NUM_CON AS NumeroContrato,
                    CM.CM_RUT_CLI AS RutCliente,
                    CM.CM_DV_CLI AS DvCliente,
                    CONCAT(FORMAT(CM.CM_RUT_CLI, 'N0'), '-', CM.CM_DV_CLI) AS RutFormateado,
                    CLI.CLI_NOM_CLI AS NombreCliente,
                    CM.CM_FEC_SOL AS FechaSolicitud,
                    CM.CM_FEC_APR AS FechaAprobacion,
                    CM.CM_FEC_VEN AS FechaVencimiento,
                    CM.CM_MON_SOL AS MontoSolicitado,
                    CM.CM_MON_APR AS MontoAprobado,
                    CM.CM_MON_DIS AS MontoDisponible,
                    CM.CM_ESTADO AS Estado,
                    PAR.PAR_DES_PAR AS DescripcionEstado,
                    CM.CM_TAS_NEG AS TasaNegocio,
                    CM.CM_SPREAD AS Spread,
                    CM.CM_OBS AS Observaciones,
                    CM.CM_USR_CRE AS UsuarioCreacion,
                    CM.CM_FEC_CRE AS FechaCreacion,
                    CM.CM_USR_MOD AS UsuarioModificacion,
                    CM.CM_FEC_MOD AS FechaModificacion
                FROM CON_MARCO CM
                INNER JOIN CLI ON CLI.CLI_RUT_CLI = CM.CM_RUT_CLI
                LEFT JOIN PAR ON PAR.PAR_COD_TAB = 65 AND PAR.PAR_COD_PAR = CM.CM_ESTADO
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (filtro.RutCliente.HasValue)
            {
                sql += " AND CM.CM_RUT_CLI = @RutCliente";
                parameters.Add("RutCliente", filtro.RutCliente.Value);
            }

            if (filtro.NumeroContrato.HasValue)
            {
                sql += " AND CM.CM_NUM_CON = @NumeroContrato";
                parameters.Add("NumeroContrato", filtro.NumeroContrato.Value);
            }

            if (!string.IsNullOrEmpty(filtro.Estado))
            {
                sql += " AND CM.CM_ESTADO = @Estado";
                parameters.Add("Estado", filtro.Estado);
            }

            if (filtro.FechaSolicitudDesde.HasValue)
            {
                sql += " AND CM.CM_FEC_SOL >= @FechaSolicitudDesde";
                parameters.Add("FechaSolicitudDesde", filtro.FechaSolicitudDesde.Value);
            }

            if (filtro.FechaSolicitudHasta.HasValue)
            {
                sql += " AND CM.CM_FEC_SOL <= @FechaSolicitudHasta";
                parameters.Add("FechaSolicitudHasta", filtro.FechaSolicitudHasta.Value);
            }

            if (filtro.SoloVigentes == true)
            {
                sql += " AND CM.CM_ESTADO = 2 AND CM.CM_FEC_VEN >= GETDATE()";
            }

            sql += " ORDER BY CM.CM_FEC_SOL DESC";

            return await connection.QueryAsync<ContratoMarcoDto>(sql, parameters);
        }

        public async Task<ContratoMarcoDto?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    CM.CM_ID AS Id,
                    CM.CM_NUM_CON AS NumeroContrato,
                    CM.CM_RUT_CLI AS RutCliente,
                    CM.CM_DV_CLI AS DvCliente,
                    CONCAT(FORMAT(CM.CM_RUT_CLI, 'N0'), '-', CM.CM_DV_CLI) AS RutFormateado,
                    CLI.CLI_NOM_CLI AS NombreCliente,
                    CM.CM_FEC_SOL AS FechaSolicitud,
                    CM.CM_FEC_APR AS FechaAprobacion,
                    CM.CM_FEC_VEN AS FechaVencimiento,
                    CM.CM_MON_SOL AS MontoSolicitado,
                    CM.CM_MON_APR AS MontoAprobado,
                    CM.CM_MON_DIS AS MontoDisponible,
                    CM.CM_ESTADO AS Estado,
                    PAR.PAR_DES_PAR AS DescripcionEstado,
                    CM.CM_TAS_NEG AS TasaNegocio,
                    CM.CM_SPREAD AS Spread,
                    CM.CM_OBS AS Observaciones
                FROM CON_MARCO CM
                INNER JOIN CLI ON CLI.CLI_RUT_CLI = CM.CM_RUT_CLI
                LEFT JOIN PAR ON PAR.PAR_COD_TAB = 65 AND PAR.PAR_COD_PAR = CM.CM_ESTADO
                WHERE CM.CM_ID = @Id";

            return await connection.QueryFirstOrDefaultAsync<ContratoMarcoDto>(sql, new { Id = id });
        }

        public async Task<ContratoMarcoDto?> GetByNumeroAsync(int numeroContrato)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    CM.CM_ID AS Id,
                    CM.CM_NUM_CON AS NumeroContrato,
                    CM.CM_RUT_CLI AS RutCliente,
                    CM.CM_DV_CLI AS DvCliente,
                    CONCAT(FORMAT(CM.CM_RUT_CLI, 'N0'), '-', CM.CM_DV_CLI) AS RutFormateado,
                    CLI.CLI_NOM_CLI AS NombreCliente,
                    CM.CM_FEC_SOL AS FechaSolicitud,
                    CM.CM_FEC_APR AS FechaAprobacion,
                    CM.CM_FEC_VEN AS FechaVencimiento,
                    CM.CM_MON_SOL AS MontoSolicitado,
                    CM.CM_MON_APR AS MontoAprobado,
                    CM.CM_MON_DIS AS MontoDisponible,
                    CM.CM_ESTADO AS Estado,
                    PAR.PAR_DES_PAR AS DescripcionEstado,
                    CM.CM_TAS_NEG AS TasaNegocio,
                    CM.CM_SPREAD AS Spread,
                    CM.CM_OBS AS Observaciones
                FROM CON_MARCO CM
                INNER JOIN CLI ON CLI.CLI_RUT_CLI = CM.CM_RUT_CLI
                LEFT JOIN PAR ON PAR.PAR_COD_TAB = 65 AND PAR.PAR_COD_PAR = CM.CM_ESTADO
                WHERE CM.CM_NUM_CON = @NumeroContrato";

            return await connection.QueryFirstOrDefaultAsync<ContratoMarcoDto>(sql, new { NumeroContrato = numeroContrato });
        }

        public async Task<IEnumerable<ContratoMarcoDto>> GetByClienteAsync(int rutCliente)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    CM.CM_ID AS Id,
                    CM.CM_NUM_CON AS NumeroContrato,
                    CM.CM_RUT_CLI AS RutCliente,
                    CM.CM_DV_CLI AS DvCliente,
                    CONCAT(FORMAT(CM.CM_RUT_CLI, 'N0'), '-', CM.CM_DV_CLI) AS RutFormateado,
                    CLI.CLI_NOM_CLI AS NombreCliente,
                    CM.CM_FEC_SOL AS FechaSolicitud,
                    CM.CM_FEC_APR AS FechaAprobacion,
                    CM.CM_FEC_VEN AS FechaVencimiento,
                    CM.CM_MON_SOL AS MontoSolicitado,
                    CM.CM_MON_APR AS MontoAprobado,
                    CM.CM_MON_DIS AS MontoDisponible,
                    CM.CM_ESTADO AS Estado,
                    PAR.PAR_DES_PAR AS DescripcionEstado,
                    CM.CM_TAS_NEG AS TasaNegocio,
                    CM.CM_SPREAD AS Spread,
                    CM.CM_OBS AS Observaciones
                FROM CON_MARCO CM
                INNER JOIN CLI ON CLI.CLI_RUT_CLI = CM.CM_RUT_CLI
                LEFT JOIN PAR ON PAR.PAR_COD_TAB = 65 AND PAR.PAR_COD_PAR = CM.CM_ESTADO
                WHERE CM.CM_RUT_CLI = @RutCliente
                ORDER BY CM.CM_FEC_SOL DESC";

            return await connection.QueryAsync<ContratoMarcoDto>(sql, new { RutCliente = rutCliente });
        }

        public async Task<int> CreateAsync(CreateContratoMarcoDto dto, int usuarioId)
        {
            using var connection = CreateConnection();

            var siguienteNumero = await GetSiguienteNumeroAsync();

            var sql = @"
                INSERT INTO CON_MARCO (
                    CM_NUM_CON, CM_RUT_CLI, CM_DV_CLI, CM_FEC_SOL, CM_MON_SOL,
                    CM_TAS_NEG, CM_SPREAD, CM_ESTADO, CM_OBS, CM_USR_CRE, CM_FEC_CRE
                ) 
                OUTPUT INSERTED.CM_ID
                VALUES (
                    @NumeroContrato, @RutCliente, @DvCliente, @FechaSolicitud, @MontoSolicitado,
                    @TasaNegocio, @Spread, 1, @Observaciones, @UsuarioId, GETDATE()
                )";

            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                NumeroContrato = siguienteNumero,
                dto.RutCliente,
                DvCliente = "0", // Se podría calcular
                FechaSolicitud = DateTime.Now,
                dto.MontoSolicitado,
                dto.TasaNegocio,
                dto.Spread,
                dto.Observaciones,
                UsuarioId = usuarioId
            });
        }

        public async Task<bool> UpdateAsync(int id, UpdateContratoMarcoDto dto, int usuarioId)
        {
            using var connection = CreateConnection();

            var sql = @"
                Update CON_MARCO SET
                    CM_MON_SOL = @MontoSolicitado,
                    CM_TAS_NEG = @TasaNegocio,
                    CM_SPREAD = @Spread,
                    CM_FEC_VEN = @FechaVencimiento,
                    CM_OBS = @Observaciones,
                    CM_USR_MOD = @UsuarioId,
                    CM_FEC_MOD = GETDATE()
                WHERE CM_ID = @Id";

            var rows = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                dto.MontoSolicitado,
                dto.TasaNegocio,
                dto.Spread,
                dto.FechaVencimiento,
                dto.Observaciones,
                UsuarioId = usuarioId
            });

            return rows > 0;
        }

        public async Task<bool> AprobarAsync(int id, decimal montoAprobado, int usuarioId)
        {
            using var connection = CreateConnection();

            var sql = @"
                Update CON_MARCO SET
                    CM_ESTADO = 2,
                    CM_MON_APR = @MontoAprobado,
                    CM_MON_DIS = @MontoAprobado,
                    CM_FEC_APR = GETDATE(),
                    CM_USR_MOD = @UsuarioId,
                    CM_FEC_MOD = GETDATE()
                WHERE CM_ID = @Id AND CM_ESTADO = 1";

            var rows = await connection.ExecuteAsync(sql, new { Id = id, MontoAprobado = montoAprobado, UsuarioId = usuarioId });
            return rows > 0;
        }

        public async Task<bool> RechazarAsync(int id, string motivo, int usuarioId)
        {
            using var connection = CreateConnection();

            var sql = @"
                Update CON_MARCO SET
                    CM_ESTADO = 3,
                    CM_OBS = CONCAT(CM_OBS, ' | Rechazado: ', @Motivo),
                    CM_USR_MOD = @UsuarioId,
                    CM_FEC_MOD = GETDATE()
                WHERE CM_ID = @Id AND CM_ESTADO = 1";

            var rows = await connection.ExecuteAsync(sql, new { Id = id, Motivo = motivo, UsuarioId = usuarioId });
            return rows > 0;
        }

        public async Task<bool> ActualizarDisponibleAsync(int id, decimal nuevoDisponible)
        {
            using var connection = CreateConnection();

            var sql = @"Update CON_MARCO SET CM_MON_DIS = @NuevoDisponible WHERE CM_ID = @Id";

            var rows = await connection.ExecuteAsync(sql, new { Id = id, NuevoDisponible = nuevoDisponible });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            using var connection = CreateConnection();

            // Soft delete - cambiar estado a anulado
            var sql = @"
                Update CON_MARCO SET
                    CM_ESTADO = 9,
                    CM_USR_MOD = @UsuarioId,
                    CM_FEC_MOD = GETDATE()
                WHERE CM_ID = @Id";

            var rows = await connection.ExecuteAsync(sql, new { Id = id, UsuarioId = usuarioId });
            return rows > 0;
        }

        public async Task<int> GetSiguienteNumeroAsync()
        {
            using var connection = CreateConnection();
            var sql = "SELECT ISNULL(MAX(CM_NUM_CON), 0) + 1 FROM CON_MARCO";
            return await connection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<decimal> GetMontoDisponibleByClienteAsync(int rutCliente)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT ISNULL(SUM(CM_MON_DIS), 0) 
                FROM CON_MARCO 
                WHERE CM_RUT_CLI = @RutCliente AND CM_ESTADO = 2 AND CM_FEC_VEN >= GETDATE()";
            return await connection.ExecuteScalarAsync<decimal>(sql, new { RutCliente = rutCliente });
        }
    }
}
