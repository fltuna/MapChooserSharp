using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Nomination;

/// <summary>
/// API for nomination module
/// </summary>
public interface IMcsNominationApi
{
    /// <summary>
    /// Map list of currently nominated
    /// </summary>
    /// <returns>Nominated map list</returns>
    public IReadOnlyDictionary<string, IMcsNominationData> GetNominatedMaps();
    
    /// <summary>
    /// Try to nominate map.
    /// This method is no grantee to nomiante map.
    /// If you want to force fully nominate map, then use AdminNominateMap()
    /// </summary>
    /// <param name="player">Player controller</param>
    /// <param name="mapConfig">Map config to nominate</param>
    public void NominateMap(CCSPlayerController player, IMapConfig mapConfig);

    /// <summary>
    /// Try to nominate map.
    /// This method will forcefully nominate a map. But exceptionally we can't nominate if target map's ProhibitAdminNomination is true.
    /// </summary>
    /// <param name="player">Player controller, You can pass null, then treated as Console</param>
    /// <param name="mapConfig">Map config to nominate</param>
    public void AdminNominateMap(CCSPlayerController? player, IMapConfig mapConfig);


    /// <summary>
    /// Show nomination menu to player, this method is show maps in given config list
    /// </summary>
    /// <param name="player">Player controller</param>
    /// <param name="configs">Map configs to show</param>
    /// <param name="isAdminNomination">This nomination menu is should be admin nomination menu?</param>
    public void ShowNominationMenu(CCSPlayerController player, List<IMapConfig> configs, bool isAdminNomination = false);

    /// <summary>
    /// Show nomination menu to player, this method is shows all maps in map config provider
    /// </summary>
    /// <param name="player">Player controller</param>
    /// <param name="isAdminNomination">This nomination menu is should be admin nomination menu?</param>
    public void ShowNominationMenu(CCSPlayerController player, bool isAdminNomination = false);

    /// <summary>
    /// Show remove nomination menu to player with given config list<br/>
    /// You can show this menu to any player, but non admin player is cannot be execute the command.
    /// </summary>
    /// <param name="player">Player controller</param>
    /// <param name="nominationData">Nomination Data to show</param>
    public void ShowRemoveNominationMenu(CCSPlayerController player, List<IMcsNominationData> nominationData);

    /// <summary>
    /// Show remove nomination menu to player with all nominated maps<br/>
    /// You can show this menu to any player, but non admin player is cannot be execute the command.
    /// </summary>
    /// <param name="player">Player controller</param>
    public void ShowRemoveNominationMenu(CCSPlayerController player);

    /// <summary>
    /// Remove nominated map with given map name.
    /// </summary>
    /// <param name="player">Player controller</param>
    /// <param name="mapConfig">Map config to remove</param>
    public void RemoveNomination(CCSPlayerController? player, IMapConfig mapConfig);
}