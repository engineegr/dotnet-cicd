namespace LdapQueryAPI.Models;

public class User
{
    public string UserName { get; set; } = default!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? UserPrincipalName { get; set; }

    public string FullName => this.LastName + ", " + this.FirstName;

    public string? Email { get; set; }
}
