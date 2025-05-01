using System.Data;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders;

internal class UnsupportedSqlProvider: IMcsSqlQueryProvider
{
    public IMcsMapInformationSqlQueries MapInfoSqlQueries() => throw new NotSupportedException("Database provider not supported");
    
    public IMcsGroupSqlQueries GroupSqlQueries() => throw new NotSupportedException("Database provider not supported");

    public IDbConnection CreateConnection(string connectionString) => throw new NotSupportedException("Database provider not supported");
}