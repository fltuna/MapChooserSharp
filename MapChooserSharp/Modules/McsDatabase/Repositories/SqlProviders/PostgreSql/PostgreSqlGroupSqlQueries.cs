using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.PostgreSql;

internal sealed class PostgreSqlGroupSqlQueries(string tableName) : IMcsGroupSqlQueries
{
    public string TableName { get; } = tableName;
    
    public string GetEnsureTableExistsSql() => throw new NotImplementedException("This functionality is not implemented.");
    
    public string GetDecrementCooldownsSql() => throw new NotImplementedException("This functionality is not implemented.");
    
    public string GetUpsertGroupCooldownSql() => throw new NotImplementedException("This functionality is not implemented.");
    
    public string GetAllGroupInfosSql() => throw new NotImplementedException("This functionality is not implemented.");

    public string GetInsertGroupInfoSql() => throw new NotImplementedException("This functionality is not implemented.");
}