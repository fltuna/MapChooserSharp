using System.Data;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;
using MySqlConnector;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.MySql;

internal sealed class MySqlSqlProvider(string tableName) : IMcsSqlQueryProvider
{
    private readonly IMcsMapInformationSqlQueries _mcsMapInformationSqlQueries = new MySqlMapInformationSqlQueries(tableName);
    
    public IMcsMapInformationSqlQueries MapInfoSqlQueries() => _mcsMapInformationSqlQueries;

    
    private readonly IMcsGroupSqlQueries _mcsGroupSqlQueries = new MySqlGroupSqlQueries(tableName);
    
    public IMcsGroupSqlQueries GroupSqlQueries() => _mcsGroupSqlQueries;

    public IDbConnection CreateConnection(string connectionString) => 
        new MySqlConnection(connectionString);
}