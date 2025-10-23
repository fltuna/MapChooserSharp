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
    
    /// <summary>
    /// Access User SQL queries
    /// </summary>
    IMcsUserInformationSqlQueries UserInfoSqlQueries();
    
    IDbConnection CreateConnection(string connectionString);
}