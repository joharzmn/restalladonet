using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using RESTAll.Data.Common;
using RESTAll.Data.Contracts;
using AntiCSRF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RESTAll.Data.Exceptions;
using RESTAll.Data.Extensions;
using RESTAll.Data.Models;
using RESTAll.Data.Providers;

namespace RESTAll.Data.Utilities
{
    public class AuthenticationClient : IAuthenticationClient
    {
        private const string UserAgent = "RESTALL-DataClient";
        private string TokenRequestEndpointUrl = "";
        private HttpClient _httpClient;
        private readonly bool _disposeHttpClient;
        private RestAllConnectionStringBuilder _Builder;
        public Dictionary<string, object> Token { set; get; }
        public bool IsAuthenticated { set; get; }
        private ILogger<AuthenticationClient> _logger;
        public AuthenticationClient(RestAllConnectionStringBuilder biBuilder, ILogger<AuthenticationClient> logger, HttpClient httpClient)
        {
            TokenRequestEndpointUrl = biBuilder.OAuthAccessTokenUrl;
            _Builder = biBuilder;
            _logger = logger;
            _httpClient = httpClient;

        }

        private void ReadToken()
        {
            _logger.LogDebug("Reading Token");
            if (!string.IsNullOrEmpty(_Builder.OAuthSettingsLocation) && File.Exists(_Builder.OAuthSettingsLocation))
            {
                var fileText = File.ReadAllText(_Builder.OAuthSettingsLocation);
                var lastModified = File.GetLastWriteTime(_Builder.OAuthSettingsLocation);
                _logger.LogInformation($"OAuthSettingsLocation:{_Builder.OAuthSettingsLocation}");
                if (!string.IsNullOrEmpty(fileText))
                {
                    var authToken = JsonConvert.DeserializeObject<Dictionary<string, Object>>(fileText);
                    if (authToken != null)
                    {
                        Token = authToken;
                        _Builder.AccessToken = authToken["access_token"].ToString();
                        if (authToken.TryGetValue("refresh_token", out object val))
                        {
                            _Builder.RefreshToken = val.ToString();
                        }
                        if (authToken.TryGetValue("id_token", out object refreshToken))
                        {
                            _Builder.RefreshToken = refreshToken.ToString();
                        }

                        if (authToken.TryGetValue("expires_in", out object expiresin))
                        {
                            if (lastModified.AddSeconds(Convert.ToInt32(expiresin)) < DateTime.Now)
                            {
                                TokenRefreshAsync().Wait();
                            }
                        }
                        IsAuthenticated = true;
                    }
                }
                else
                {
                    _logger.LogDebug("Token data is empty");
                }
            }

            if (string.IsNullOrEmpty(_Builder.OAuthSettingsLocation))
            {
                throw new RESTException("OAuthSettingLocation is null", "ConnectionString");
            }
        }

        public async Task GetAuthenticatedAsync()
        {
            ReadToken();
            _logger.LogDebug($"Authenticated:{IsAuthenticated}");
            if (IsAuthenticated) return;
            _logger.LogInformation($"AuthType:{_Builder.AuthType}");
            if (_Builder.AuthType.ToAuthenticationType() == AuthenticationStyle.OAuth2 && _Builder.GrantType.ToGrantType() == GrantType.Code)
            {
                _logger.LogInformation($"AuthType:OAuth");
                var authorizationUrl = "";
                if (_Builder.RequireCSRF)
                {
                    var token = AntiCSRFToken.GenerateToken(_Builder.OAuthClientId, _Builder.OAuthClientSecret);
                    _logger.LogDebug($"CSRF Token: {token}");
                    authorizationUrl =
                        $"{_Builder.OAuthAuthorizationUrl}?client_id={_Builder.OAuthClientId}&response_type={_Builder.GrantType}&scope={_Builder.Scope}&redirect_uri={_Builder.CallBackUrl}&state={token}";
                    _logger.LogDebug($"AuthorizationUrl: {authorizationUrl}");
                }
                else
                {
                    authorizationUrl =
                        $"{_Builder.OAuthAuthorizationUrl}?client_id={_Builder.OAuthClientId}&response_type={_Builder.GrantType}&scope={_Builder.Scope}&redirect_uri={_Builder.CallBackUrl}";
                    _logger.LogDebug($"AuthorizationUrl: {authorizationUrl}");
                }
                _logger.LogInformation("Requesting Authorization Code");
                var parmsDictionary = TokenResponseListener.ListenCode(authorizationUrl, _Builder.ReadUrlParameters);
                foreach (var item in parmsDictionary)
                {
                    _logger.LogDebug($"Authorization Code Response: {item.Key} : {item.Value}");
                }
                await WebServerAsync(_Builder.OAuthClientId, _Builder.OAuthClientSecret, _Builder.CallBackUrl, parmsDictionary);

            }

            if (_Builder.GrantType.ToLower() == "password")
            {
                await UsernamePasswordAsync(_Builder.OAuthClientId, _Builder.OAuthClientSecret, _Builder.Username,
                    _Builder.Password);
            }


        }

        public Task UsernamePasswordAsync(
          string clientId,
          string clientSecret,
          string username,
          string password)
        {
            return UsernamePasswordAsync(clientId, clientSecret, username, password, TokenRequestEndpointUrl);
        }

        public async Task UsernamePasswordAsync(
          string clientId,
          string clientSecret,
          string username,
          string password,
          string tokenRequestEndpointUrl)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId));
            if (string.IsNullOrEmpty(clientSecret))
                throw new ArgumentNullException(nameof(clientSecret));
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(tokenRequestEndpointUrl))
                throw new ArgumentNullException(nameof(tokenRequestEndpointUrl));
            if (!Uri.IsWellFormedUriString(tokenRequestEndpointUrl, UriKind.Absolute))
                throw new FormatException(nameof(tokenRequestEndpointUrl));
            FormUrlEncodedContent content = new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)new KeyValuePair<string, string>[5]
            {
        new KeyValuePair<string, string>("grant_type", nameof (password)),
        new KeyValuePair<string, string>("client_id", clientId),
        new KeyValuePair<string, string>("client_secret", clientSecret),
        new KeyValuePair<string, string>(nameof (username), username),
        new KeyValuePair<string, string>(nameof (password), password)
            });
            _logger.LogInformation("Getting Token with Password Authentication");
            _logger.LogDebug($"Password Authentication Request: grant_type=password,client_id={clientId}");

            HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(tokenRequestEndpointUrl),
                Content = content
            };
            request.Headers.UserAgent.ParseAdd("RESTAll-DataClient");
            HttpResponseMessage responseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false);
            string response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode)
            {
                _logger.LogInformation("Token Success with GrantType Password");
                File.WriteAllText(_Builder.OAuthSettingsLocation, response);
                Token = response.ToDictionary();
                IsAuthenticated = true;
            }
            else
            {
                throw new RestAuthenticationException(responseMessage.StatusCode, response);
            }
        }

        public Task WebServerAsync(
          string clientId,
          string clientSecret,
          string redirectUri,
          Dictionary<string, string> paramsDict)
        {
            return WebServerAsync(clientId, clientSecret, redirectUri, paramsDict, TokenRequestEndpointUrl);
        }

        public async Task WebServerAsync(
          string clientId,
          string clientSecret,
          string redirectUri,
          Dictionary<string, string> paramsDict,
          string tokenRequestEndpointUrl)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId));
            if (string.IsNullOrEmpty(clientSecret))
                throw new ArgumentNullException(nameof(clientSecret));
            if (string.IsNullOrEmpty(redirectUri))
                throw new ArgumentNullException(nameof(redirectUri));
            if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
                throw new FormatException(nameof(redirectUri));
            if (string.IsNullOrEmpty(paramsDict["code"]))
                throw new ArgumentNullException("code");
            if (string.IsNullOrEmpty(tokenRequestEndpointUrl))
                throw new ArgumentNullException(nameof(tokenRequestEndpointUrl));
            if (!Uri.IsWellFormedUriString(tokenRequestEndpointUrl, UriKind.Absolute))
                throw new FormatException(nameof(tokenRequestEndpointUrl));
            FormUrlEncodedContent content = new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)new KeyValuePair<string, string>[5]
            {
        new ("grant_type", _Builder.GrantType.ToGrantType().Description()),
        new ("client_id", clientId),
        new ("client_secret", clientSecret),
        new ("redirect_uri", redirectUri),
        new ("code", paramsDict["code"])
            });
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();
            }
            _logger.LogInformation("Requesting Token");
            _logger.LogDebug($"Token Request: {await content.ReadAsStringAsync()}");
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(tokenRequestEndpointUrl),
                Content = content
            };
            request.Headers.UserAgent.ParseAdd("RESTAll-DataClient");
            HttpResponseMessage responseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false);
            string response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Access Token Success");

                Token = response.ToDictionary();
                foreach (var item in paramsDict)
                {
                    Token[item.Key] = item.Value;
                }
                IsAuthenticated = true;
                var tokenString = JsonConvert.SerializeObject(Token);
                _logger.LogDebug($"Token Response: {tokenString}");
                File.WriteAllText(_Builder.OAuthSettingsLocation, tokenString);
            }
            else
            {
                throw new RestAuthenticationException(responseMessage.StatusCode, response);
            }
        }


        public async Task TokenRefreshAsync()
        {
            _logger.LogInformation("Refreshing Access Token");
            string url = FormatRefreshTokenUrl(_Builder.OAuthRefreshTokenUrl, _Builder.OAuthClientId, this.Token["refresh_token"].ToString(), _Builder.OAuthClientSecret);
            _logger.LogDebug($"Refresh Token Url: {url}");
            var response = "";


            if (_Builder.RefreshTokenMethod.ToRefreshTokenType() == RefreshTokenType.ClientCredentials)
            {

                url = $"{_Builder.OAuthRefreshTokenUrl}?client_id={_Builder.OAuthClientId}";
                HttpRequestMessage request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url)
                };
                _logger.LogInformation("Refresh Token Method is Client Credentials");
                var formData = new Dictionary<string, string>
                {
                    {"grant_type", "refresh_token"},
                    {"refresh_token", this.Token["refresh_token"].ToString()}
                };
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                request.Content = new FormUrlEncodedContent(formData);
                request.Headers.Authorization = new BasicAuthenticationHeaderValue(_Builder.OAuthClientId, _Builder.OAuthClientSecret);
                request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = await _httpClient.SendAsync(request);
                response = await result.Content.ReadAsStringAsync();
                _logger.LogDebug($"Refresh Token Response: {response}");
                var responseDict = response.ToDictionary();
                foreach (var item in responseDict)
                {
                    Token[item.Key] = item.Value;
                }

                File.WriteAllText(_Builder.OAuthSettingsLocation, JsonConvert.SerializeObject(Token));
                IsAuthenticated = true;
            }
            else
            {
                HttpRequestMessage request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url)
                };
                request.Headers.UserAgent.ParseAdd("RESTAll-DataClient");
                HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);
                response = await responseMessage.Content.ReadAsStringAsync();
                if (!responseMessage.IsSuccessStatusCode)
                {
                    _logger.LogError($"Refresh Token Error: {response} with StatusCode={responseMessage.StatusCode}");
                    throw new RestAuthenticationException(responseMessage.StatusCode, response);
                }
                var responseDict = response.ToDictionary();
                foreach (var item in responseDict)
                {
                    Token[item.Key] = item.Value;
                }

                File.WriteAllText(_Builder.OAuthSettingsLocation, JsonConvert.SerializeObject(Token));
                IsAuthenticated = true;
            }
        }


        public string FormatRefreshTokenUrl(
            string tokenRefreshUrl,
            string clientId,
            string refreshToken,
            string clientSecret = "")
        {
            if (tokenRefreshUrl == null)
                throw new ArgumentNullException(nameof(tokenRefreshUrl));
            if (clientId == null)
                throw new ArgumentNullException(nameof(clientId));
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));
            string str = "";
            if (!string.IsNullOrEmpty(clientSecret))
                str = $"&client_secret={clientSecret}";
            if (_Builder.RefreshTokenMethod.ToRefreshTokenType() == RefreshTokenType.ClientCredentials)
            {
                return $"{tokenRefreshUrl}?grant_type=refresh_token&refresh_token={refreshToken}";
            }
            return
                $"{tokenRefreshUrl}?grant_type=refresh_token&client_id={clientId}{str}&refresh_token={refreshToken}";
        }

        public AuthenticationHeaderValue GetAuthenticationHeader()
        {
            if (IsAuthenticated && (_Builder.AuthType.ToAuthenticationType() == AuthenticationStyle.OAuth2 || _Builder.AuthType.ToAuthenticationType() == AuthenticationStyle.OAuth || _Builder.AuthType.ToAuthenticationType() == AuthenticationStyle.Password))
            {
                return new AuthenticationHeaderValue("Bearer",
                    Token["access_token"].ToString());
            }
            
            if (_Builder.AuthType.ToAuthenticationType() == AuthenticationStyle.Basic)
            {
                return new BasicAuthenticationHeaderValue(_Builder.Username,
                    _Builder.Password);
            }

            return null;
        }

        public void Dispose()
        {
            if (!_disposeHttpClient)
                return;
            _httpClient.Dispose();
        }
    }
}
