using System.Data;
using System.Data.SQLite;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Sqlite;

internal class SqliteSqlProvider : IMcsSqlQueryProvider
{
    private readonly IMcsMapInformationSqlQueries _mcsMapInformationSqlQueries = new SqliteMapInformationSqlQueries();
    
    public IMcsMapInformationSqlQueries MapInfoSqlQueries() => _mcsMapInformationSqlQueries;

    
    private readonly IMcsGroupSqlQueries _mcsGroupSqlQueries = new SqliteGroupSqlQueries();
    
    public IMcsGroupSqlQueries GroupSqlQueries() => _mcsGroupSqlQueries;
    
    public IDbConnection CreateConnection(string connectionString) => 
        new SQLiteConnection(connectionString);
}