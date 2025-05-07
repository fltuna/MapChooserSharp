using System.Runtime.InteropServices;
using System.Security;
using MapChooserSharp.Modules.McsDatabase;
using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

public sealed class McsSqlConfig: IMcsSqlConfig
{
    public McsSqlConfig(string address, string user, ref string password, string groupSettingsSqlTableName, string mapSettingsSqlTableName, McsSupportedSqlType dataBaseType)
    {
        Address = address;
        User = user;
        GroupSettingsSqlTableName = groupSettingsSqlTableName;
        MapSettingsSqlTableName = mapSettingsSqlTableName;
        DataBaseType = dataBaseType;


        Password = ConvertToSecureString(password);

        // Ensure password is removed from memory
        ClearString(ref password);
    }

    public McsSupportedSqlType DataBaseType { get; }
    public string Address { get; }
    public string User { get; }
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