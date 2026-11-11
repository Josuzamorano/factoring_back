using Dapper;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;

namespace Factoring.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UsuarioRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Usuario?> GetByLoginAsync(string login)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Query equivalente a VB6: "SELECT * FROM USR WHERE USR_LOG_USR = '" & TextUsuario.Text & "'"
        const string sql = @"
            SELECT 
                USR_LOG_USR as UsrLogUsr,
                USR_PAS_USR as UsrPasUsr,
                USR_NOM_USR as UsrNomUsr,
                USR_EST_PAS as UsrEstPas,
                USR_INT_ERR as UsrIntErr,
                USR_FEC_EXP as UsrFecExp,
                USR_ULT_CAMBIO as UsrUltCambio,
                USR_COD_PER as UsrCodPer,
                USR_COD_EJE as UsrCodEje,
                USR_COD_SUC as UsrCodSuc
            FROM USR 
            WHERE USR_LOG_USR = @Login";
        
        return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Login = login.ToUpper() });
    }

    public async Task<bool> ValidarCredencialesAsync(string login, string password)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Query equivalente a VB6: "SELECT * FROM USR WHERE USR_LOG_USR = '...' AND USR_PAS_USR = '...'"
        const string sql = @"
            SELECT COUNT(1) 
            FROM USR 
            WHERE USR_LOG_USR = @Login AND USR_PAS_USR = @Password";
        
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Login = login.ToUpper(), Password = password });
        return count > 0;
    }

    public async Task<int> IncrementarIntentosErroneos(string login)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Equivalente a VB6: "UPDATE usr SET USR_INT_ERR = USR_INT_ERR + 1 WHERE USR_LOG_USR = '...'"
        const string sql = @"
            UPDATE USR 
            SET USR_INT_ERR = USR_INT_ERR + 1 
            OUTPUT INSERTED.USR_INT_ERR
            WHERE USR_LOG_USR = @Login";
        
        return await connection.ExecuteScalarAsync<int>(sql, new { Login = login.ToUpper() });
    }

    public async Task BloquearUsuarioAsync(string login)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Equivalente a VB6: "UPDATE usr SET USR_EST_PAS = 3 WHERE USR_LOG_USR = '...'"
        const string sql = @"
            UPDATE USR 
            SET USR_EST_PAS = 3 
            WHERE USR_LOG_USR = @Login";
        
        await connection.ExecuteAsync(sql, new { Login = login.ToUpper() });
    }

    public async Task ResetearIntentosAsync(string login)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Equivalente a VB6: "UPDATE usr SET USR_INT_ERR = 0 WHERE USR_LOG_USR = '...'"
        const string sql = @"
            UPDATE USR 
            SET USR_INT_ERR = 0 
            WHERE USR_LOG_USR = @Login";
        
        await connection.ExecuteAsync(sql, new { Login = login.ToUpper() });
    }

    public async Task CambiarPasswordAsync(string login, string nuevoPassword)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE USR 
            SET USR_PAS_USR = @NuevoPassword,
                USR_EST_PAS = 0,
                USR_ULT_CAMBIO = GETDATE()
            WHERE USR_LOG_USR = @Login";
        
        await connection.ExecuteAsync(sql, new { Login = login.ToUpper(), NuevoPassword = nuevoPassword });
    }

    public async Task<IEnumerable<MenuOpcion>> GetMenuAutorizadoAsync(int codigoPerfil)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Equivalente a la lógica de CargaMenuAutorizado en VB6
        // Busca en PUE las opciones tipo 'I' (Icono) que el perfil tiene permiso en NPP
        const string sql = @"
            SELECT 
                p.PUE_COD_PUE as PueCodPue,
                p.PUE_TIP_OBJ as PueTipObj,
                p.PUE_NUM_PUE as PueNumPue,
                p.PUE_NOM_PUE as PueNombre,
                p.PUE_DES_PUE as PueDescripcion,
                p.PUE_COD_PADRE as PuePadre,
                p.PUE_RUTA as PueRuta,
                p.PUE_ICONO as PueIcono,
                p.PUE_ORDEN as PueOrden,
                p.PUE_ACTIVO as PueActivo
            FROM PUE p
            INNER JOIN NPP n ON n.NPP_COD_PUE = p.PUE_COD_PUE
            WHERE n.NPP_COD_PER = @CodigoPerfil
                AND p.PUE_TIP_OBJ = 'I'
                AND p.PUE_NUM_PUE > 0
                AND ISNULL(p.PUE_ACTIVO, 1) = 1
            ORDER BY p.PUE_ORDEN, p.PUE_COD_PUE DESC";
        
        return await connection.QueryAsync<MenuOpcion>(sql, new { CodigoPerfil = codigoPerfil });
    }
}
