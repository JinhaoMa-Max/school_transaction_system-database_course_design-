using System.Data;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace CampusTrade.Backend.Infrastructure;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class OracleConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public OracleConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("OracleDb")
            ?? throw new InvalidOperationException("OracleDb connection string is missing.");
    }

    public IDbConnection CreateConnection()
    {
        return new OracleConnection(_connectionString);
    }
}