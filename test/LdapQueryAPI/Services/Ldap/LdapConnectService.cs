namespace LdapQueryAPI.Services.Ldap;

using Microsoft.Extensions.Logging;
using LdapQueryAPI.Models.ActiveDirectory;
using System.Threading.Tasks;
using LdapQueryAPI.Models;
using LdapQueryAPI.Services.Ldap.Interfaces;

public class LdapConnectService : IActiveDirectoryService
{
    private readonly ActiveDirectoryEntry[] settings;

    private readonly int settingsCount;

    private readonly ILogger logger;

    public LdapConnectService(ILogger<LdapConnectService> logger, ActiveDirectoryConfig activeDirectoryConfig)
    {
        this.logger = logger;
        this.settings = activeDirectoryConfig.ActiveDirectoryEntries.ToArray();
        this.settingsCount = this.settings.Length;
    }

    public User? FindUser(string userName)
    {
        foreach (var s in this.settings)
        {
            var usr = this.TryFindUser(userName, s);

            if (usr != null)
            {
                return usr; // ok, return the first one
            }
        }

        return null;
    }

    public async Task<User?> FindUserAsync(string userName)
    {
        var tasks = new Task<User?>[this.settingsCount];

        for (var i = 0; i < this.settingsCount; i++)
        {
            tasks[i] = this.TryFindUserAsync(userName, this.settings[i]);
        }

        var users = await Task.WhenAll(tasks);

        foreach (var usr in users)
        {
            if (usr != null)
            {
                return usr; // ok, return the first one
            }
        }

        return null;
    }

    public User? TryFindUser(string userName, ActiveDirectoryEntry s)
    {
        try
        {
            return new LdapConnect(s).FindUser(userName);
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception.ToString());
            return null;
        }
    }

    public async Task<User?> TryFindUserAsync(string userName, ActiveDirectoryEntry s)
    {
        try
        {
            return await new LdapConnect(s).FindUserAsync(userName);
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception.ToString());
            return null;
        }
    }
}
