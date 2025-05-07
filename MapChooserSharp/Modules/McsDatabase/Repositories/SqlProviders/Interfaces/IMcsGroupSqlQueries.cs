namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

internal interface IMcsGroupSqlQueries
{
    string TableName { get; }
    
    /// <summary>
    /// Get SQL for ensuring the McsGroupInformation table exists
    /// </summary>
    string GetEnsureTableExistsSql();
    
    /// <summary>
    /// Get SQL for decrementing all group cooldowns
    /// </summary>
    string GetDecrementCooldownsSql();
    
    /// <summary>
    /// Get SQL for upserting a group cooldown
    /// </summary>
    string GetUpsertGroupCooldownSql();
    
    /// <summary>
    /// Get SQL for retrieving all group information
    /// </summary>
    string GetAllGroupInfosSql();
    
    /// <summary>
    /// Get SQL for inserting a new group information
    /// </summary>
    string GetInsertGroupInfoSql();
}