using System.Data;

namespace PopcornBytes.Api.Persistence;

public interface IDbConnectionFactory
{
    IDbConnection CreateSqlConnection();
}
