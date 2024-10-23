using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace LdapQueryAPI.Services
{
    public class CurrentUserService: ICurrentUserService
    {
        private static readonly Regex USER_NAME_DOMAIN_REGEX = new Regex(@"(\S+)\\(\S+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex USER_NAME_DOMAIN_KERBEROS_REGEX = new Regex(@"(\S+)@([\w-]+)(\.\w+)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly String UNKNOWN_CURRENT_USERNAME = "UNKNOWN_CURRENT_USERNAME";

        private readonly IHttpContextAccessor httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public static (string domain, string loginName) ParseUsername(string username)
        {
            if (username == null)
            {
                return (null, null);
            }

            const int domainPosition = 1;
            const int loginNamePosition = 2;

            var match = USER_NAME_DOMAIN_REGEX.Match(username);

            return (match.Groups[domainPosition].Value, match.Groups[loginNamePosition].Value);
        }

        public static string NormilizeLdapName(string identity)
        {
            if (identity == null)
            {
                return identity;
            }

            if (USER_NAME_DOMAIN_REGEX.IsMatch(identity))
            {
                //ok, NTLM -> russia\username

                return identity;
            }

            var match = USER_NAME_DOMAIN_KERBEROS_REGEX.Match(identity);

            if (match.Success)
            {
                //ok, KERBEROS... Swap login and domain: username@russia -> russia\username

                identity = string.Concat(match.Groups[2].Value, @"\", match.Groups[1].Value);
            }

            return identity;
        }

        public string GetCurrentUsername()
        {
            var identityName = this.httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (identityName != null)
            {
                return NormilizeLdapName(identityName);
            }
            return UNKNOWN_CURRENT_USERNAME;
        }

        public string GetSAMAccountName()
        {
            var currentUsername = this.GetCurrentUsername();
            if (currentUsername != null)
            {
                return ParseUsername(currentUsername).loginName;
            }
            return UNKNOWN_CURRENT_USERNAME;
        }

    }
}
