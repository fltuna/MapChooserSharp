﻿using MapChooserSharp.Modules.McsDatabase.Entities;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.Interfaces;

public interface IMcsUserInformationRepository
{
    Task UpsertUserInformationAsync(long steamId, McsUserInformation  userInformation);
    
    Task IncrementUserSessionTimeAsync(long steamId);
    
    Task IncrementUserSessionTimeWithResetCheckAsync(long steamId, TimeSpan resetTimeOfDay);
    
    Task ResetUserSessionTimeAsync(long steamId);
    
    Task<McsUserInformation?> GetUserInformationAsync(long steamId);
}