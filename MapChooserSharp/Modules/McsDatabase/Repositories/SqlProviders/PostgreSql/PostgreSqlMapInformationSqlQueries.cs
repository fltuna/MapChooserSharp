using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.PostgreSql;

public sealed class PostgreSqlMapInformationSqlQueries(string tableName) : IMcsMapInformationSqlQueries
{
    public string TableName { get; } = tableName;
    
    public string GetEnsureTableExistsSql() => throw new NotImplementedException("This functionality is not implemented.");
    
    public string GetDecrementCooldownsSql() => throw new NotImplementedException("This functionality is not implemented.");
    
    public string GetUpsertMapInfoSql() => throw new NotImplementedException("This functionality is not implemented.");
    
    public string GetAllMapInfosSql() => throw new NotImplementedException("This functionality is not implemented.");
    
    public string GetMapInfoByNameSql() => throw new NotImplementedException("This functionality is not implemented.");

    public string GetInsertMapInfoSql() => throw new NotImplementedException("This functionality is not implemented.");
}