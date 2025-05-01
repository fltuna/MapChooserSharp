using System.Data;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

internal interface IMcsSqlQueryProvider
{
    /// <summary>
    /// Acess Map SQL queries
    /// </summary>
    /// <returns></returns>
    IMcsMapInformationSqlQueries MapInfoSqlQueries();
    
    /// <summary>
    /// Access Group SQL queries
    /// </summary>
    IMcsGroupSqlQueries GroupSqlQueries();
    
    IDbConnection CreateConnection(string connectionString);
}