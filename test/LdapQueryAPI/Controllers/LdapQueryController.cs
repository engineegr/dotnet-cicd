using LdapQueryAPI.Fakes;
using LdapQueryAPI.Models.ActiveDirectory;
using LdapQueryAPI.Services;
using LdapQueryAPI.Services.Ldap;
using LdapQueryAPI.Services.Ldap.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LdapQueryAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LdapQueryController : ControllerBase
    {

        private readonly IActiveDirectoryService _ldapConnectService;

        private readonly ICurrentUserService _currentUserService;

        public LdapQueryController(IActiveDirectoryService ldapConnectService, ICurrentUserService currentUserService)
        {
            _ldapConnectService = ldapConnectService;
            _currentUserService = currentUserService;
        }

        [HttpGet("userinfo/{samaccountname}")]
        public ActionResult GetUserInfo(string samaccountname)
        {
            var ldapDefaultUsr = this._ldapConnectService.FindUser(samaccountname);

            if (ldapDefaultUsr != null)
            {
                return Ok(string.Format("User: {0}; Email: {1}, UserPrincipalName: {2}", samaccountname, ldapDefaultUsr.Email, ldapDefaultUsr.UserPrincipalName));
            }
            return NotFound(string.Format("Couldn't find user by SAMAccoutName {0}", samaccountname));
        }

        [HttpGet("currentuserinfo")]
        public ActionResult GetCurrentUserInfo()
        {
            var normalizedCurrentUsername = this._currentUserService.GetSAMAccountName();
            var ldapDefaultUsr = this._ldapConnectService.FindUser(normalizedCurrentUsername);

            if (ldapDefaultUsr != null)
            {
                return Ok(string.Format("User: {0}; Email: {1}, UserPrincipalName: {2}", ldapDefaultUsr, ldapDefaultUsr.Email, ldapDefaultUsr.UserPrincipalName));
            }
            return NotFound(string.Format("Couldn't find user by SAMAccoutName {0}", normalizedCurrentUsername));
        }

    }
}
