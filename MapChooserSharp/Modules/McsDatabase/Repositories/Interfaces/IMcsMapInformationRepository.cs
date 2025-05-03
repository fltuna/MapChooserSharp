using MapChooserSharp.Modules.McsDatabase.Entities;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.Interfaces;

internal interface IMcsMapInformationRepository
{
    Task UpsertMapCooldownAsync(string mapName, int cooldownValue);
    
    Task DecrementAllCooldownsAsync();

    Task EnsureAllMapInfoExistsAsync();
    
    Task CollectAllCooldownsAsync();
}