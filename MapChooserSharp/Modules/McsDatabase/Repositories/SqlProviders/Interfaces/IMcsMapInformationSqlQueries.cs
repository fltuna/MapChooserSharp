namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

internal interface IMcsMapInformationSqlQueries
{
    string TableName { get; }
    
    string GetEnsureTableExistsSql();
    
    string GetDecrementCooldownsSql();
    
    string GetUpsertMapInfoSql();
    
    string GetAllMapInfosSql();
    
    string GetMapInfoByNameSql();
    
    string GetInsertMapInfoSql();
}