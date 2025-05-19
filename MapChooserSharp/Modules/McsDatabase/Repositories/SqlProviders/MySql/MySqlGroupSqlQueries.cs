using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.MySql;

internal sealed class MySqlGroupSqlQueries(string tableName) : IMcsGroupSqlQueries
{
    public string TableName { get; } = tableName;

    public string GetEnsureTableExistsSql() => @$"
        CREATE TABLE IF NOT EXISTS {TableName} (
            Id INT AUTO_INCREMENT PRIMARY KEY,
            GroupName VARCHAR(255) NOT NULL UNIQUE,
            CooldownRemains INT NOT NULL
        )";
    
    public string GetDecrementCooldownsSql() => 
        $"UPDATE {TableName} SET CooldownRemains = CooldownRemains - 1 WHERE CooldownRemains > 0";
    
    public string GetUpsertGroupCooldownSql() => @$"
        INSERT INTO {TableName} (GroupName, CooldownRemains) 
        VALUES (@GroupName, @CooldownValue)
        ON DUPLICATE KEY UPDATE 
        CooldownRemains = @CooldownValue";
    
    public string GetAllGroupInfosSql() => 
        $"SELECT * FROM {TableName}";
    
    public string GetInsertGroupInfoSql() => 
        $"INSERT INTO {TableName} (GroupName, CooldownRemains) VALUES (@GroupName, @CooldownRemains)";
}