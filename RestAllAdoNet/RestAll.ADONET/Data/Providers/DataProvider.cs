using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RESTAll.Data.Common;
using RESTAll.Data.Contracts;
using RESTAll.Data.Exceptions;
using RESTAll.Data.Extensions;
using RESTAll.Data.Models;
using RESTAll.Data.Utilities;
#nullable disable
namespace RESTAll.Data.Providers
{
    public class DataProvider
    {
        private const string UserAgent = "RESTALL-DataClient";
        private RestAllConnectionStringBuilder _Builder;
        private EntityDescriptor entity;
        private MetaDataProvider _metaDataProvider;
        private Dictionary<string, object> FilterValues = new Dictionary<string, object>();
        private IAuthenticationClient _authenticationClient;
        private ITemplateEngine _templateEngine;
        private ILogger<DataProvider> _logger;
        private HttpClient _httpClient;
        public DataProvider(RestAllConnectionStringBuilder cb, MetaDataProvider metaDataProvider, ITemplateEngine templateEngine, IAuthenticationClient authClient, ILogger<DataProvider> logger, HttpClient httpClient)
        {
            _Builder = cb;
            _logger = logger;
            _metaDataProvider = metaDataProvider;
            _templateEngine = templateEngine;
            _authenticationClient = authClient;
            _httpClient = httpClient;
        }

        public void SetFilterValue(string filter, object value)
        {
            FilterValues[filter] = value;
        }


        public async Task<EntityResultSet> GetAsync(string url, string entityName, string schema = "", int resultCount = 100)
        {
            LoadEntityDescriptor(entityName, new { }, _authenticationClient.Token, schema);
            if (_Builder.Provider.ToProviderType() == ProviderType.File)
            {
                var data = await ReadFile(url);
                var dt = ParseJson(entity, data.ToString());
                return new EntityResultSet(dt);
            }
            if (!string.IsNullOrEmpty(url))
            {
                var data = await GetDataAsync(url);
                var dt = ParseJson(entity, data);
                return new EntityResultSet(dt);
            }
            return new EntityResultSet(new DataTable());
        }

        public async Task<JObject> ReadFile(string fileName)
        {
            var data = await File.ReadAllTextAsync(fileName);
            return JObject.Parse(data);
        }

        private DataTable ParseJson(EntityDescriptor entity, string stringData)
        {
            var data = JObject.Parse(stringData);
            var dt = new DataTable();
            var dataArray = data.SelectToken(entity.RepeatElement);
            if (entity.AutoBuild)
            {
                dt = BuildEntity(dataArray, this.entity.RepeatElement, entity.Table.TableName);
            }
            else
            {
                dt = entity.GetBaseDataTable();
                if (dataArray == null)
                {
                    return dt;
                }
                foreach (var token in dataArray)
                {
                    var dr = dt.NewRow();
                    foreach (var entityField in entity.Table.Fields)
                    {
                        if (FilterValues.ContainsKey(entityField.Field))
                        {
                            dr[entityField.Field] = FilterValues[entityField.Field];
                        }
                        else
                        {
                            var dataToken = token.SelectToken(entityField.Path);
                            if (dataToken == null)
                            {
                                dr[entityField.Field] = DBNull.Value;
                            }
                            else
                            {
                                var tokenValue = dataToken.Value<object>();
                                if (tokenValue.ToString() == "")
                                {
                                    dr[entityField.Field] = DBNull.Value;
                                }
                                else
                                {
                                    dr[entityField.Field] = tokenValue;
                                }
                            }


                        }
                    }

                    dt.Rows.Add(dr);
                }
            }


            return dt;
        }

        private DataTable BuildEntity(JToken data, string root, string entityName)
        {
            var rootNew = root.Replace("$.", "");
            var dt = new DataTable();
            dt.TableName = entityName;
            var firstToken = data.FirstOrDefault();
            if (firstToken != null)
            {
                foreach (var item in firstToken)
                {
                    var path = item.Path.Replace($"{rootNew}[0].", "").Replace($"{root.Replace("$.", "")}", "");
                    dt.Columns.Add(path);
                }

                foreach (var token in data)
                {
                    var dr = dt.NewRow();
                    foreach (DataColumn dc in dt.Columns)
                    {
                        var dataToken = token.SelectToken(dc.ColumnName);
                        dr[dc] = dataToken.Value<object>();
                    }

                    dt.Rows.Add(dr);
                }
            }

            dt.AcceptChanges();
            return dt;
        }

        public void LoadEntityDescriptor(string entityName, object input, object token, string schema)
        {
            entity = _metaDataProvider.GetEntityDescriptor(entityName, new { }, token, schema);
        }

        private async Task<string> GetDataAsync(string url, bool repeat = false)
        {
            var action = entity.Actions.FirstOrDefault(x => x.Url == url);
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = new HttpMethod(string.IsNullOrEmpty(action.Method) ? "GET" : action.Method),
                Content = string.IsNullOrEmpty(action.Body) ? null : new StringContent(action.Body, Encoding.UTF8, string.IsNullOrEmpty(action.ContentType) ? "application/json" : action.ContentType),
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("ContentType", "application/json");
            request.Headers.Authorization = _authenticationClient.GetAuthenticationHeader();
            var result = await _httpClient.SendAsync(request);
            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadAsStringAsync();
            }
            else
            {
                if (result.StatusCode == HttpStatusCode.Unauthorized && !_authenticationClient.IsAuthenticated)
                {
                    if (GetAuthenticated())
                    {
                        return await GetDataAsync(url);
                    }
                    else
                    {
                        throw new AuthenticationException(await result.Content.ReadAsStringAsync());
                    }
                }
                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (repeat)
                    {
                        throw new UnauthorizedAccessException(await result.Content.ReadAsStringAsync());
                    }

                    await _authenticationClient.TokenRefreshAsync();

                    return await GetDataAsync(url, true);
                }

                throw new RESTException(await result.Content.ReadAsStringAsync(), result.StatusCode);

            }
        }

        public async Task<string> ExecuteBatchAsync(string data)
        {

            var batch = _metaDataProvider.GetBatch("", "", "", _authenticationClient.Token);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(batch.Endpoint),
                Method = new HttpMethod(string.IsNullOrEmpty(batch.Method) ? "GET" : batch.Method),
                Content = string.IsNullOrEmpty(data) ? null : new StringContent(data, Encoding.UTF8, string.IsNullOrEmpty(batch.ContentType) ? "application/json" : batch.ContentType)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = _authenticationClient.GetAuthenticationHeader();
            var result = await _httpClient.SendAsync(request);
            var responseData = await result.Content.ReadAsStringAsync();
            if (result.IsSuccessStatusCode)
            {
                _logger.LogTrace("Response:");
                _logger.LogTrace(responseData);
                return responseData;
            }

            throw new RESTException(responseData, result.StatusCode);

        }

        public async Task<string> PostDataAsync(string url, Dictionary<string, object> data, EntityDescriptor entityDescriptor)
        {
            entity = entityDescriptor;
            var unflattened = data.Unflatten();
            var action = entity.Actions.FirstOrDefault(x => x.Url == url);
            action.Body = unflattened.ToString();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = new HttpMethod(string.IsNullOrEmpty(action.Method) ? "GET" : action.Method),
                Content = string.IsNullOrEmpty(action.Body) ? null : new StringContent(action.Body, Encoding.UTF8, string.IsNullOrEmpty(action.ContentType) ? "application/json" : action.ContentType)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = _authenticationClient.GetAuthenticationHeader();
            var result = await _httpClient.SendAsync(request);
            var responseData = await result.Content.ReadAsStringAsync();
            if (result.IsSuccessStatusCode)
            {
                _logger.LogTrace("Response");
                _logger.LogTrace(responseData);
                return responseData;
            }

            throw new RESTException(responseData, result.StatusCode);

        }

        public bool GetAuthenticated()
        {
            if (_Builder.AuthType.ToAuthenticationType() == AuthenticationStyle.Password)
            {
                _authenticationClient.UsernamePasswordAsync(_Builder.OAuthClientId, _Builder.OAuthClientSecret, _Builder.Username,
                    _Builder.Password).Wait();
            }

            if (_Builder.AuthType.ToAuthenticationType() == AuthenticationStyle.OAuth2)
            {
                var authorizationUrl =
                    $"{_Builder.OAuthAuthorizationUrl}?client_id={_Builder.OAuthClientId}&response_type={_Builder.GrantType}&scope={_Builder.Scope}&redirect_uri={_Builder.CallBackUrl}";
                var code = TokenResponseListener.ListenCode(authorizationUrl, _Builder.ReadUrlParameters);
                _authenticationClient.WebServerAsync(_Builder.OAuthClientId, _Builder.OAuthClientSecret,
                   _Builder.CallBackUrl, code, _Builder.OAuthAccessTokenUrl).Wait();
                return true;
            }

            return false;
        }

    }
}
