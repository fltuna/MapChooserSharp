using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Sqlite;

internal class SqliteGroupSqlQueries : IMcsGroupSqlQueries
{
    public string GetEnsureTableExistsSql() => @"
        CREATE TABLE IF NOT EXISTS McsGroupInformation (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            GroupName TEXT NOT NULL UNIQUE,
            CooldownRemains INTEGER NOT NULL
        )";
    
    public string GetDecrementCooldownsSql() => 
        "UPDATE McsGroupInformation SET CooldownRemains = CooldownRemains - 1 WHERE CooldownRemains > 0";
    
    public string GetUpsertGroupCooldownSql() => @"
        INSERT INTO McsGroupInformation (GroupName, CooldownRemains) 
        VALUES (@GroupName, @CooldownValue)
        ON CONFLICT(GroupName) DO UPDATE SET 
        CooldownRemains = @CooldownValue";
    
    public string GetAllGroupInfosSql() => 
        "SELECT * FROM McsGroupInformation";
    
    public string GetInsertGroupInfoSql() => 
        "INSERT INTO McsGroupInformation (GroupName, CooldownRemains) VALUES (@GroupName, @CooldownRemains)";
}