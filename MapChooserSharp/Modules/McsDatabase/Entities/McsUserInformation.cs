namespace MapChooserSharp.Modules.McsDatabase.Entities;

public class McsUserInformation
{
    public int Id { get; set; }

    public long SteamId { get; set; }
    
    public uint SessionTime { get; set; }
    
    public DateTime LastLoggedInAt { get; set; }
    
    public DateTime UserSessionStartedAt { get; set; }
}

