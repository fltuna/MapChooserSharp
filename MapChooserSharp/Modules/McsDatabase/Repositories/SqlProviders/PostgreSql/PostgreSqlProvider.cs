using System.Data;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;
using Npgsql;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.PostgreSql;

internal sealed class PostgreSqlProvider(string tableName) : IMcsSqlQueryProvider
{
    private readonly IMcsMapInformationSqlQueries _mcsMapInformationSqlQueries = new PostgreSqlMapInformationSqlQueries(tableName);
    
    public IMcsMapInformationSqlQueries MapInfoSqlQueries() => _mcsMapInformationSqlQueries;


    private readonly IMcsGroupSqlQueries _mcsGroupSqlQueries = new PostgreSqlGroupSqlQueries(tableName);
    
    public IMcsGroupSqlQueries GroupSqlQueries() => _mcsGroupSqlQueries;
    
    public IDbConnection CreateConnection(string connectionString) => 
        new NpgsqlConnection(connectionString);
}