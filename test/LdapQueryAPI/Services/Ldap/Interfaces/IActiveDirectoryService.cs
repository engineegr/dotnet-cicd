namespace LdapQueryAPI.Services.Ldap.Interfaces;

using LdapQueryAPI.Models;

public interface IActiveDirectoryService
{
    User? FindUser(string userName);

    Task<User?> FindUserAsync(string userName);
}
