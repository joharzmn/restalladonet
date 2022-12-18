using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RESTAll.Data.Contracts
{
    public interface IAuthenticationClient : IDisposable
    {

        bool IsAuthenticated { set; get; }
        Dictionary<string, object> Token { set; get; }

        Task GetAuthenticatedAsync();
        AuthenticationHeaderValue GetAuthenticationHeader();
        Task UsernamePasswordAsync(
            string clientId,
            string clientSecret,
            string username,
            string password);

        Task UsernamePasswordAsync(
            string clientId,
            string clientSecret,
            string username,
            string password,
            string tokenRequestEndpointUrl);

        Task WebServerAsync(string clientId, string clientSecret, string redirectUri, Dictionary<string,string> paramsDict);

        Task WebServerAsync(
            string clientId,
            string clientSecret,
            string redirectUri,
            Dictionary<string,string> paramsDict,
            string tokenRequestEndpointUrl);

        Task TokenRefreshAsync();

    }
}
