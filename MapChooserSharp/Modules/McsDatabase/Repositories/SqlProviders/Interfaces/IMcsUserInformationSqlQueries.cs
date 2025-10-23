namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

internal interface IMcsUserInformationSqlQueries
{
    string TableName { get; }
    
    string GetEnsureTableExistsSql();
    
    string GetUpsertUserInfoSql();
    
    string GetIncrementSessionTimeSql();
    
    string GetResetSessionTimeSql();
    
    string GetResetSessionTimeWithStartTimeSql();
        
    string GetUserInfoBySteamIdSql();
}