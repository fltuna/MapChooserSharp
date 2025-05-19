using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.PostgreSql;

internal sealed class PostgreSqlMapInformationSqlQueries(string tableName) : IMcsMapInformationSqlQueries
{
    public string TableName { get; } = tableName;

    public string GetEnsureTableExistsSql() => @$"
        CREATE TABLE IF NOT EXISTS {TableName} (
            Id SERIAL PRIMARY KEY,
            MapName VARCHAR(255) NOT NULL UNIQUE,
            CooldownRemains INT NOT NULL
        )";
    
    public string GetDecrementCooldownsSql() => 
        $"UPDATE {TableName} SET CooldownRemains = CooldownRemains - 1 WHERE CooldownRemains > 0";
    
    public string GetUpsertMapInfoSql() => @$"
        INSERT INTO {TableName} (MapName, CooldownRemains) 
        VALUES (@MapName, @CooldownRemains)
        ON CONFLICT (MapName) DO UPDATE SET 
        CooldownRemains = @CooldownRemains";
    
    public string GetAllMapInfosSql() => 
        $"SELECT * FROM {TableName}";
    
    public string GetMapInfoByNameSql() => 
        $"SELECT * FROM {TableName} WHERE MapName = @MapName";

    public string GetInsertMapInfoSql() =>
        $"INSERT INTO {TableName} (MapName, CooldownRemains) VALUES (@MapName, @CooldownRemains)";
}