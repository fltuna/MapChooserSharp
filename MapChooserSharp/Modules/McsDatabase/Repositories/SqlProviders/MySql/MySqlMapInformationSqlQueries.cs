using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.MySql;

internal sealed class MySqlMapInformationSqlQueries(string tableName) : IMcsMapInformationSqlQueries
{
    public string TableName { get; } = tableName;

    public string GetEnsureTableExistsSql() => @$"
        CREATE TABLE IF NOT EXISTS {TableName} (
            Id INT AUTO_INCREMENT PRIMARY KEY,
            MapName VARCHAR(255) NOT NULL UNIQUE,
            CooldownRemains INT NOT NULL
        )";
    
    public string GetDecrementCooldownsSql() => 
        $"UPDATE {TableName} SET CooldownRemains = CooldownRemains - 1 WHERE CooldownRemains > 0";
    
    public string GetUpsertMapInfoSql() => @$"
        INSERT INTO {TableName} (MapName, CooldownRemains) 
        VALUES (@MapName, @CooldownRemains)
        ON DUPLICATE KEY UPDATE 
        CooldownRemains = @CooldownRemains";
    
    public string GetAllMapInfosSql() => 
        $"SELECT * FROM {TableName}";
    
    public string GetMapInfoByNameSql() => 
        $"SELECT * FROM {TableName} WHERE MapName = @MapName";

    public string GetInsertMapInfoSql() =>
        $"INSERT INTO {TableName} (MapName, CooldownRemains) VALUES (@MapName, @CooldownRemains)";
}