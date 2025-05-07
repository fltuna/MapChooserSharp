using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.MySql;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.PostgreSql;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Sqlite;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.McsDatabase.Repositories;

public abstract class McsDatabaseRepositoryBase(IServiceProvider provider, string tableName): PluginBasicFeatureBase(provider)
{
    internal IMcsSqlQueryProvider CreateSqlProvider(McsSupportedSqlType type)
    {
        return type switch
        {
            McsSupportedSqlType.Sqlite => new SqliteSqlProvider(tableName),
            McsSupportedSqlType.MySql => new MySqlSqlProvider(tableName),
            McsSupportedSqlType.PostgreSql => new PostgreSqlProvider(tableName),
            _ => new UnsupportedSqlProvider()
        };
    }
}