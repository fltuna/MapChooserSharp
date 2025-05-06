using System.Data;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;
using Npgsql;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.PostgreSql;

internal sealed class PostgreSqlProvider : IMcsSqlQueryProvider
{
    private readonly IMcsMapInformationSqlQueries _mcsMapInformationSqlQueries = new PostgreSqlMapInformationSqlQueries();
    
    public IMcsMapInformationSqlQueries MapInfoSqlQueries() => _mcsMapInformationSqlQueries;


    private readonly IMcsGroupSqlQueries _mcsGroupSqlQueries = new PostgreSqlGroupSqlQueries();
    
    public IMcsGroupSqlQueries GroupSqlQueries() => _mcsGroupSqlQueries;
    
    public IDbConnection CreateConnection(string connectionString) => 
        new NpgsqlConnection(connectionString);
}