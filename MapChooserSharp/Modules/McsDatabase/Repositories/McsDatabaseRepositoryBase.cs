using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.MySql;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.PostgreSql;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Sqlite;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.McsDatabase.Repositories;

public abstract class McsDatabaseRepositoryBase(IServiceProvider provider): PluginBasicFeatureBase(provider)
{
    internal static IMcsSqlQueryProvider CreateSqlProvider(string databaseType)
    {
        return databaseType switch
        {
            "sqlite" => new SqliteSqlProvider(),
            "mysql" => new MySqlSqlProvider(),
            "postgresql" => new PostgreSqlProvider(),
            _ => new UnsupportedSqlProvider()
        };
    }
}