using Oracle.ManagedDataAccess.Client;

namespace CampusSecondHand.Api.Database;

public interface IOracleConnectionFactory
{
    Task<OracleConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
