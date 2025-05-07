using System.Security;
using MapChooserSharp.Modules.McsDatabase;

namespace MapChooserSharp.Modules.PluginConfig.Interfaces;

public interface IMcsSqlConfig
{
    internal McsSupportedSqlType DataBaseType { get; }
    
    internal string Address { get; }
    
    internal string User { get; }
    
    internal SecureString Password { get; }
    
    internal string GroupSettingsSqlTableName { get; }
    
    internal string MapSettingsSqlTableName { get; }
}