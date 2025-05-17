using MapChooserSharp.API;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapCycleController;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.API.Nomination;
using MapChooserSharp.API.RtvController;

namespace MapChooserSharp.Models;

public sealed class McsApi(
    IMcsEventSystem eventSystem,
    IMcsMapCycleControllerApi mcsMapCycleController,
    IMcsMapCycleExtendControllerApi mcsMapCycleExtendController,
    IMcsMapCycleExtendVoteControllerApi mcsMapCycleExtendVoteController,
    IMcsNominationApi mcsNominationApi,
    IMcsMapVoteControllerApi mcsMapVoteControllerApi,
    IMcsRtvControllerApi mcsRtvControllerApi,
    IMcsMapConfigProviderApi mcsMapConfigProviderApi): IMapChooserSharpApi
{
    public IMcsEventSystem EventSystem { get; } = eventSystem;
    public IMcsMapCycleControllerApi McsMapCycleController { get; } = mcsMapCycleController;
    public IMcsMapCycleExtendControllerApi McsMapCycleExtendController { get; } = mcsMapCycleExtendController;
    public IMcsMapCycleExtendVoteControllerApi McsMapCycleExtendVoteController { get; } = mcsMapCycleExtendVoteController;
    public IMcsNominationApi McsNominationApi { get; } = mcsNominationApi;
    public IMcsMapVoteControllerApi McsMapVoteControllerApi { get; } = mcsMapVoteControllerApi;
    public IMcsRtvControllerApi McsRtvControllerApi { get; } = mcsRtvControllerApi;
    public IMcsMapConfigProviderApi McsMapConfigProviderApi { get; } = mcsMapConfigProviderApi;
}