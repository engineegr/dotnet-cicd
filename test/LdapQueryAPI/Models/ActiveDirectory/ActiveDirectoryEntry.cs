namespace LdapQueryAPI.Models.ActiveDirectory;

public class ActiveDirectoryEntry
{
    public string? DomainName { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? LdapSearch { get; set; }

    public string LdapServerUrl { get; set; }
}
