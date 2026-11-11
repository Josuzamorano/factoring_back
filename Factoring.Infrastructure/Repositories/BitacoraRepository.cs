using Dapper;
using Factoring.Core.Entities;
using Factoring.Core.Interfaces;

namespace Factoring.Infrastructure.Repositories;

public class BitacoraRepository : IBitacoraRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BitacoraRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task RegistrarAccesoAsync(BitacoraAcceso bitacora)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Equivalente a VB6:
        // "INSERT INTO BIT_IOS VALUES('" & Login & "','" & Maquina & "',0,'" & IP & "'..."
        const string sql = @"
            INSERT INTO BIT_IOS (
                BIT_COD_USR, BIT_NOM_MAQ, BIT_EST_ADO, BIT_IP,
                BIT_FEC_INS, BIT_HOR_INS, BIT_FEC_OUS, BIT_HOR_OUS,
                BIT_OBSERVACION, BIT_RESULTADO
            ) VALUES (
                @BitCodUsr, @BitNomMaq, @BitEstAdo, @BitIp,
                @BitFecIns, @BitHorIns, @BitFecOus, @BitHorOus,
                @BitObservacion, @BitResultado
            )";
        
        await connection.ExecuteAsync(sql, bitacora);
    }

    public async Task RegistrarSalidaAsync(string usuario, string maquina)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Equivalente a VB6 en CB3_PRIN_Click:
        // "UPDATE BIT_IOS SET BIT_EST_ADO=1, BIT_FEC_OUS=..., BIT_HOR_OUS=... 
        //  WHERE BIT_COD_USR='...' AND BIT_NOM_MAQ='...' AND BIT_EST_ADO=0"
        const string sql = @"
            UPDATE BIT_IOS 
            SET BIT_EST_ADO = 1,
                BIT_FEC_OUS = @FechaSalida,
                BIT_HOR_OUS = @HoraSalida
            WHERE BIT_COD_USR = @Usuario 
                AND BIT_NOM_MAQ = @Maquina 
                AND BIT_EST_ADO = 0";
        
        await connection.ExecuteAsync(sql, new 
        { 
            Usuario = usuario,
            Maquina = maquina,
            FechaSalida = DateTime.Now.ToString("yyyyMMdd"),
            HoraSalida = DateTime.Now.ToString("HH:mm:ss")
        });
    }
}
