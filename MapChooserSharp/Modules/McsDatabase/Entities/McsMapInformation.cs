namespace MapChooserSharp.Modules.McsDatabase.Entities;

public class McsMapInformation
{
    public int Id { get; set; }

    public string MapName { get; set; } = string.Empty;
    
    public int CooldownRemains { get; set; }
}