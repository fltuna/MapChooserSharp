using MapChooserSharp.Modules.McsDatabase.Entities;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.Interfaces;

internal interface IMcsGroupInformationRepository
{
    Task UpsertGroupCooldownAsync(string groupName, int cooldownValue);
    
    Task DecrementAllCooldownsAsync();

    Task EnsureAllGroupInfoExistsAsync();
    
    Task CollectAllGroupCooldownsAsync();
}