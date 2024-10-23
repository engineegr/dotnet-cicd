namespace LdapQueryAPI.Services.Ldap;

using LdapQueryAPI.Models.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Threading.Tasks;

using LdapQueryAPI.Models;
using System.Reflection.PortableExecutable;

public class LdapConnect
{
    private readonly ActiveDirectoryEntry setting;

    public LdapConnect(ActiveDirectoryEntry setting)
    {
        this.setting = setting;
    }

    public User? FindUser(string userName)
    {
        using var context = this.GetPrincipalContext();

        var userPrincipal = context.QueryFirst($"(&(objectclass=user)(samaccountname={userName}))");

        if (userPrincipal == null)
        {
            return null;
        }

        var user = new User();

        MapUserPrincipal(userPrincipal, user);

        return user;
    }

    public async Task<User?> FindUserAsync(string userName)
    {
        using var context = this.GetPrincipalContext();

        var userPrincipal = await context.QueryFirstAsync($"(&(objectclass=user)(samaccountname={userName}))");

        if (userPrincipal == null)
        {
            return null;
        }

        var user = new User();

        MapUserPrincipal(userPrincipal, user);

        return user;
    }

    private static void MapUserPrincipal(SearchResultEntry adresponse, User usr)
    {
        if (adresponse.Attributes.Contains("givenName"))
        {
            usr.FirstName = (string?)adresponse.Attributes["givenName"][0];
        }
        if (adresponse.Attributes.Contains("sn"))
        {
            usr.LastName = (string?)adresponse.Attributes["sn"][0];
        }
        if (adresponse.Attributes.Contains("mail"))
        {
            usr.Email = (string?)adresponse.Attributes["mail"][0];
        }

        if (adresponse.Attributes.Contains("userPrincipalName"))
        {
            usr.UserPrincipalName = (string?)adresponse.Attributes["userPrincipalName"][0];
        }
    }

    private LdapContext GetPrincipalContext() => new(this.setting);
}
