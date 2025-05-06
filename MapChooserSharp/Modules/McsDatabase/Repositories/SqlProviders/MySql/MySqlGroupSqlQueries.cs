using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.MySql;

internal sealed class MySqlGroupSqlQueries : IMcsGroupSqlQueries
{
    public string GetEnsureTableExistsSql() => throw new NotImplementedException("This functionality is not implemented.");
    
    public string GetDecrementCooldownsSql() => throw new NotImplementedException("This functionality is not implemented.");
    
    public string GetUpsertGroupCooldownSql() => throw new NotImplementedException("This functionality is not implemented.");
    
    public string GetAllGroupInfosSql() => throw new NotImplementedException("This functionality is not implemented.");
    
    public string GetInsertGroupInfoSql() => throw new NotImplementedException("This functionality is not implemented.");
}