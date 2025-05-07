using System.Data;
using System.Data.SQLite;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Sqlite;

internal sealed class SqliteSqlProvider(string tableName) : IMcsSqlQueryProvider
{
    private readonly IMcsMapInformationSqlQueries _mcsMapInformationSqlQueries = new SqliteMapInformationSqlQueries(tableName);
    
    public IMcsMapInformationSqlQueries MapInfoSqlQueries() => _mcsMapInformationSqlQueries;

    
    private readonly IMcsGroupSqlQueries _mcsGroupSqlQueries = new SqliteGroupSqlQueries(tableName);
    
    public IMcsGroupSqlQueries GroupSqlQueries() => _mcsGroupSqlQueries;
    
    public IDbConnection CreateConnection(string connectionString) => 
        new SQLiteConnection(connectionString);
}