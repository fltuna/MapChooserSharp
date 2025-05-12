using CounterStrikeSharp.API;
using MapChooserSharp.API.MapCycleController;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.MapCycle.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.MapCycle;

internal class McsMapCycleExtendController(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider), IMcsInternalMapCycleExtendControllerApi
{
    public override string PluginModuleName => "McsMapCycleExtendController";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private ITimeLeftUtil _timeLeftUtil = null!;

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMcsInternalMapCycleExtendControllerApi>(this);
    }

    protected override void OnAllPluginsLoaded()
    {
        _timeLeftUtil = ServiceProvider.GetRequiredService<ITimeLeftUtil>();
    }

    protected override void OnUnloadModule()
    {
    }



    public McsMapCycleExtendResult ExtendCurrentMap(int extendTime)
    {
        _timeLeftUtil.ReDetermineExtendType();
        switch (_timeLeftUtil.ExtendType)
        {
            case McsMapExtendType.TimeLimit:
                if (_timeLeftUtil.TimeLimit + extendTime < 1)
                    return McsMapCycleExtendResult.FailedTimeCannotBeZeroOrNegative;
                
                if (_timeLeftUtil.ExtendTimeLimit(extendTime))
                    return McsMapCycleExtendResult.Extended;
                
                break;
            
            case McsMapExtendType.Rounds:
                if (_timeLeftUtil.RoundsLeft + extendTime < 1)
                    return McsMapCycleExtendResult.FailedTimeCannotBeZeroOrNegative;

                if (_timeLeftUtil.ExtendRounds(extendTime))
                    return McsMapCycleExtendResult.Extended;
                
                break;
            
            case McsMapExtendType.RoundTime:
                if (_timeLeftUtil.RoundTimeLeft + extendTime < 1)
                    return McsMapCycleExtendResult.FailedTimeCannotBeZeroOrNegative;

                if (_timeLeftUtil.ExtendRoundTime(extendTime))
                    return McsMapCycleExtendResult.Extended;
                
                break;
        }

        return McsMapCycleExtendResult.FailedToExtend;
    }
}