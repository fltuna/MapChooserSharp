using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Sqlite;

public sealed class SqliteMapInformationSqlQueries: IMcsMapInformationSqlQueries
{
    public string GetEnsureTableExistsSql() => @"
        CREATE TABLE IF NOT EXISTS McsMapInformation (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            MapName TEXT NOT NULL UNIQUE,
            CooldownRemains INTEGER NOT NULL
        )";
    
    public string GetDecrementCooldownsSql() => 
        "UPDATE McsMapInformation SET CooldownRemains = CooldownRemains - 1 WHERE CooldownRemains > 0";
    
    public string GetUpsertMapInfoSql() => @"
        INSERT INTO McsMapInformation (MapName, CooldownRemains) 
        VALUES (@MapName, @CooldownRemains)
        ON CONFLICT(MapName) DO UPDATE SET 
        CooldownRemains = @CooldownRemains";
    
    public string GetAllMapInfosSql() => 
        "SELECT * FROM McsMapInformation";
    
    public string GetMapInfoByNameSql() => 
        "SELECT * FROM McsMapInformation WHERE MapName = @MapName";

    public string GetInsertMapInfoSql() =>
        "INSERT INTO McsMapInformation (MapName, CooldownRemains) VALUES (@MapName, @CooldownRemains)";
}