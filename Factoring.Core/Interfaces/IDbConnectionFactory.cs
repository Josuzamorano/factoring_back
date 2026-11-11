using System.Data;

namespace Factoring.Core.Interfaces;

/// <summary>
/// Factory para crear conexiones a la base de datos
/// Similar a cómo BaseDato se usaba en VB6
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
