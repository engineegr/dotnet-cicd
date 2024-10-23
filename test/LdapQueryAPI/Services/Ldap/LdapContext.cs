namespace LdapQueryAPI.Services.Ldap;

using LdapQueryAPI.Models.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Threading.Tasks;

public class LdapContext : IDisposable
{
    private readonly ActiveDirectoryEntry setting;

    private LdapConnection? connection;

    public LdapContext(ActiveDirectoryEntry setting)
    {
        this.setting = setting;

        var username = setting.UserName;
        var authType = AuthType.Basic;
        var ldapSrv = setting.DomainName;
        NetworkCredential? netCredential = null;

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(setting.Password))
        {
            netCredential = new NetworkCredential(username, setting.Password);
        }
        else if (OperatingSystem.IsWindows())
        {
            authType = AuthType.Negotiate;
        }
        else if (OperatingSystem.IsLinux())
        {
            authType = AuthType.Kerberos;
            ldapSrv = setting.LdapServerUrl;
        }

        this.connection = new LdapConnection(new LdapDirectoryIdentifier(ldapSrv), netCredential, authType);

        this.connection.SessionOptions.ProtocolVersion = 3;
        this.connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
        this.connection.Bind();
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public SearchRequest NewRequest(string query) => new(this.setting.LdapSearch, query, SearchScope.Subtree);

    public SearchResultEntry? QueryFirst(string query)
    {
        var request = this.NewRequest(query);
        return this.QueryFirst(request);
    }

    public SearchResultEntry? QueryFirst(SearchRequest request)
    {
        var response = (SearchResponse)this.connection!.SendRequest(request);
        var entries = response.Entries;

        return entries == null || entries.Count == 0 ? null : entries[0];
    }

    public Task<SearchResultEntry?> QueryFirstAsync(string query)
    {
        var request = this.NewRequest(query);
        return this.QueryFirstAsync(request);
    }

    public async Task<SearchResultEntry?> QueryFirstAsync(SearchRequest request)
    {
        var response = await this.SendRequestAsync(request);
        var entries = response.Entries;

        return entries == null || entries.Count == 0 ? null : entries[0];
    }

    public SearchResponse Query(string query) => this.SendRequest(query);

    public SearchResponse Query(SearchRequest request) => this.SendRequest(request);

    public Task<SearchResponse> QueryAsync(string query)
    {
        var request = this.NewRequest(query);
        return this.SendRequestAsync(request);
    }

    public Task<SearchResponse> QueryAsync(SearchRequest request) => this.SendRequestAsync(request);

    public SearchResponse SendRequest(string query)
    {
        var request = this.NewRequest(query);
        return this.SendRequest(request);
    }

    public SearchResponse SendRequest(SearchRequest request) => (SearchResponse)this.connection!.SendRequest(request);

    public async Task<SearchResponse> SendRequestAsync(SearchRequest request)
    {
        var conn = this.connection!;
        var t = Task.Factory.FromAsync<DirectoryRequest, PartialResultProcessing, DirectoryResponse>(
                        conn.BeginSendRequest!,
                        conn.EndSendRequest,
                        request,
                        PartialResultProcessing.NoPartialResultSupport,
                        null);
        return (SearchResponse)await t;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (this.connection != null)
            {
                this.connection.Dispose();
                this.connection = null;
            }
        }
    }
}
