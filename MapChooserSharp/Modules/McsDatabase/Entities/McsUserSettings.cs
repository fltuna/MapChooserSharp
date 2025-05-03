using System.ComponentModel.DataAnnotations;
using MapChooserSharp.Modules.McsMenu;

namespace MapChooserSharp.Modules.McsDatabase.Entities;

public class McsUserSettings
{
    public int Id { get; set; }
    
    public long SteamId { get; set; }
    
    public McsSupportedMenuType VoteMenuType { get; set; }
}