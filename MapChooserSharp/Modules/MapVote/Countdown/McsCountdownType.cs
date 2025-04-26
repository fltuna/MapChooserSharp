namespace MapChooserSharp.Modules.MapVote.Countdown;

[Flags]
public enum McsCountdownType
{
    None = 0,
    CenterHud = 1 << 0,
    CenterAlert = 1 << 1,
    CenterHtml = 1 << 2,
    Chat = 1 << 3,
}