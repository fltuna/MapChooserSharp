using MapChooserSharp.API;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.API.Nomination;
using MapChooserSharp.API.RtvController;

namespace MapChooserSharp.Models;

public sealed class McsApi(
    IMcsEventSystem eventSystem,
    INominationApi nominationApi,
    IMapVoteControllerApi mapVoteControllerApi,
    IRtvControllerApi rtvControllerApi
    ): IMapChooserSharpApi
{
    public IMcsEventSystem EventSystem { get; } = eventSystem;
    public INominationApi NominationApi { get; } = nominationApi;
    public IMapVoteControllerApi MapVoteControllerApi { get; } = mapVoteControllerApi;
    public IRtvControllerApi RtvControllerApi { get; } = rtvControllerApi;
}