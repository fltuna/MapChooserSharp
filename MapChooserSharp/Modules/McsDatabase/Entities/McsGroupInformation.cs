namespace MapChooserSharp.Modules.McsDatabase.Entities;

public class McsGroupInformation
{
    public int Id { get; set; }

    public string GroupName { get; set; } = string.Empty;
    
    public int CooldownRemains { get; set; }
}