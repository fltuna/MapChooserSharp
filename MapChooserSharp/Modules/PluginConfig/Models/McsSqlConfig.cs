using System.Security;
using MapChooserSharp.Modules.McsDatabase;
using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

public sealed class McsSqlConfig: IMcsSqlConfig
{
    public McsSqlConfig(string host, string port, string databaseName, string user, ref string password, string groupSettingsSqlTableName, string mapSettingsSqlTableName, McsSupportedSqlType dataBaseType)
    {
        Host = host;
        UserName = user;
        GroupSettingsSqlTableName = groupSettingsSqlTableName;
        MapSettingsSqlTableName = mapSettingsSqlTableName;
        DataBaseType = dataBaseType;
        Port = port;
        DatabaseName = databaseName;


        Password = ConvertToSecureString(password);

        // Ensure password is removed from memory
        ClearString(ref password);
    }

    public McsSupportedSqlType DataBaseType { get; }
    public string Host { get; }
    public string Port { get; }
    public string DatabaseName { get; }
    public string UserName { get; }
    public SecureString Password { get; }
    public string GroupSettingsSqlTableName { get; }
    public string MapSettingsSqlTableName { get; }
    
    
    private SecureString ConvertToSecureString(string password)
    {
        var securePassword = new SecureString();

        if (string.IsNullOrEmpty(password))
        {
            securePassword.MakeReadOnly();
            return securePassword;
        }
        
        
        foreach (char c in password)
        {
            securePassword.AppendChar(c);
        }
        
        securePassword.MakeReadOnly();
        return securePassword;
    }

    
    private void ClearString(ref string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        int length = text.Length;
        
        char[] charArray = text.ToCharArray();
        
        text = null!;
        
        unsafe
        {
            fixed (char* ptr = charArray)
            {
                for (int i = 0; i < length; i++)
                {
                    ptr[i] = '\0';
                }
                // Prevent aggressive compiler optimization from removing the memory clearing operations.
                Thread.MemoryBarrier();
            }
        }
    }
}