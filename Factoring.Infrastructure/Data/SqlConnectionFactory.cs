using System.Data;
using Factoring.Core.Interfaces;
using Microsoft.Data.SqlClient;

namespace Factoring.Infrastructure.Data;

/// <summary>
/// Factory para crear conexiones SQL Server
/// Equivalente a cómo se usaba BaseDato en VB6
/// </summary>
public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
