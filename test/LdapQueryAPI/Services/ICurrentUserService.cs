namespace LdapQueryAPI.Services
{
    public interface ICurrentUserService
    {
        string? GetCurrentUsername();

        string? GetSAMAccountName();
    }
}
