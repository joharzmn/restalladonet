using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
#nullable disable
namespace RESTAll.Data.Common
{
    public class RestAllConnectionStringBuilder : DbConnectionStringBuilder
    {
        public RestAllConnectionStringBuilder() : base()
        {
            Debug.Write("constructed.");

        }

        public RestAllConnectionStringBuilder(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public RestAllConnectionStringBuilder(bool useOdbcRules)
            : base(useOdbcRules)
        {
            Debug.Write("constructed.");
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("The user name used to authenticate.")]
        [DisplayName("User Name")]
        [RefreshProperties(RefreshProperties.All)]
        public string Username
        {
            get => this.GetPropertyValue<string>("Username");
            set => this.SetProperty("Username", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("Location to Save OAuth Tokens")]
        [DisplayName("OAuthSettingsLocation")]
        [RefreshProperties(RefreshProperties.All)]
        public string OAuthSettingsLocation
        {
            get => this.GetPropertyValue<string>("OAuthSettingsLocation");
            set => this.SetProperty("OAuthSettingsLocation", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("SecurityToken for Authentication like Salesforce")]
        [DisplayName("SecurityToken")]
        [RefreshProperties(RefreshProperties.All)]
        public string SecurityToken
        {
            get => this.GetPropertyValue<string>("SecurityToken");
            set => this.SetProperty("SecurityToken", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("RefreshTokenMethod Client Credentials")]
        [DisplayName("RefreshTokenMethod")]
        [RefreshProperties(RefreshProperties.All)]
        public string RefreshTokenMethod
        {
            get => this.GetPropertyValue<string>("RefreshTokenMethod");
            set => this.SetProperty("RefreshTokenMethod", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("OAuthVerifier")]
        [DisplayName("OAuthVerifier")]
        [RefreshProperties(RefreshProperties.All)]
        public string OAuthVerifier
        {
            get => this.GetPropertyValue<string>("OAuthVerifier");
            set => this.SetProperty("OAuthVerifier", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("RequireCSRF")]
        [DisplayName("RequireCSRF")]
        [RefreshProperties(RefreshProperties.All)]
        public bool RequireCSRF
        {
            get => this.GetPropertyValue<bool>("RequireCSRF");
            set => this.SetProperty("RequireCSRF", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("OAuthAuthorizationUrl")]
        [DisplayName("OAuthAuthorizationUrl")]
        [RefreshProperties(RefreshProperties.All)]
        public string OAuthAuthorizationUrl
        {
            get => this.GetPropertyValue<string>("OAuthAuthorizationUrl");
            set => this.SetProperty("OAuthAuthorizationUrl", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("Scope")]
        [DisplayName("Scope")]
        [RefreshProperties(RefreshProperties.All)]
        public string Scope
        {
            get => this.GetPropertyValue<string>("Scope");
            set => this.SetProperty("Scope", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("GrantType")]
        [DisplayName("GrantType")]
        [RefreshProperties(RefreshProperties.All)]
        public string GrantType
        {
            get => this.GetPropertyValue<string>("GrantType");
            set => this.SetProperty("GrantType", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("ReadUrlParameters")]
        [DisplayName("ReadUrlParameters")]
        [RefreshProperties(RefreshProperties.All)]
        public string ReadUrlParameters
        {
            get => this.GetPropertyValue<string>("ReadUrlParameters");
            set => this.SetProperty("ReadUrlParameters", value);
        }


        [Category("Authentication")]
        [DefaultValue("")]
        [Description("The password used to authenticate.")]
        [DisplayName("Password")]
        [RefreshProperties(RefreshProperties.All)]
        public string Password
        {
            get => this.GetPropertyValue<string>("Password");
            set => this.SetProperty("Password", value);
        }

        [Category("Configuration")]
        [DefaultValue("")]
        [Description("PageSize")]
        [DisplayName("PageSize")]
        [RefreshProperties(RefreshProperties.All)]
        public int PageSize
        {
            get => this.GetPropertyValue<int>("PageSize");
            set => this.SetProperty("PageSize", value);
        }

        [Category("Cache")]
        [DefaultValue("")]
        [Description("CacheLocation")]
        [DisplayName("CacheLocation")]
        [RefreshProperties(RefreshProperties.All)]
        public string CacheLocation
        {
            get => this.GetPropertyValue<string>("CacheLocation");
            set => this.SetProperty("CacheLocation", value);
        }

        [Category("Configuration")]
        [DefaultValue("")]
        [Description("Profile")]
        [DisplayName("Profile")]
        [RefreshProperties(RefreshProperties.All)]
        public string Profile
        {
            get => this.GetPropertyValue<string>("Profile");
            set => this.SetProperty("Profile", value);
        }

        [Category("Configuration")]
        [DefaultValue("Main")]
        [Description("Schema")]
        [DisplayName("Schema")]
        [RefreshProperties(RefreshProperties.All)]
        public string Schema
        {
            get
            {
                var value = this.GetPropertyValue<string>("Schema");
                if (string.IsNullOrEmpty(value))
                {
                    return "Main";
                }

                return value;
            }
            set
            {
                if (value.ToLower() == "sys" || value.ToLower() == "response")
                {
                    throw new ArgumentException("Cannot set schema for Reserved Schema it cannot be `sys,response`");
                }

                this.SetProperty("Schema", value);
            }
        }

        [Category("Configuration")]
        [DefaultValue("")]
        [Description("LogLevel")]
        [DisplayName("LogLevel")]
        [RefreshProperties(RefreshProperties.All)]
        public string LogLevel
        {
            get
            {
                var val = this.GetPropertyValue<string>("LogLevel");
                if (string.IsNullOrEmpty(val))
                {
                    return "None";
                }

                return val;
            }
            set => this.SetProperty("LogLevel", value);
        }

        [Category("Configuration")]
        [DefaultValue("")]
        [Description("LogFilePath")]
        [DisplayName("LogFilePath")]
        [RefreshProperties(RefreshProperties.All)]
        public string LogFilePath
        {
            get => this.GetPropertyValue<string>("LogFilePath");
            set => this.SetProperty("LogFilePath", value);
        }

        [Category("Configuration")]
        [DefaultValue("")]
        [Description("BatchSize")]
        [DisplayName("BatchSize")]
        [RefreshProperties(RefreshProperties.All)]
        public int BatchSize
        {
            get => this.GetPropertyValue<int>("BatchSize");
            set => this.SetProperty("BatchSize", value);
        }

        [Category("Configuration")]
        [DefaultValue("REST")]
        [Description("Provider for Adapter it can be a File or REST")]
        [DisplayName("Provider")]
        [RefreshProperties(RefreshProperties.All)]
        public string Provider
        {
            get
            {
                var value = this.GetPropertyValue<string>("Provider");
                if (string.IsNullOrEmpty(value))
                {
                    return "REST";
                }

                return value;

            }
            set => this.SetProperty("Provider", value);
        }


        [Category("Authentication")]
        [DefaultValue("")]
        [Description("The url of the CRM organisation to connect to.")]
        [DisplayName("InstanceUrl")]
        [RefreshProperties(RefreshProperties.All)]
        public string Url
        {
            get => this.GetPropertyValue<string>("URL");
            set => this.SetProperty("URL", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("The client to access salesforce connection")]
        [DisplayName("OAuthClientId")]
        [RefreshProperties(RefreshProperties.None)]
        public string OAuthClientId
        {
            get => this.GetPropertyValue<string>("OAuthClientId");
            set => this.SetProperty("OAuthClientId", value);
        }
        [Category("Authentication")]
        [DefaultValue("")]
        [Description("The client to access salesforce connection")]
        [DisplayName("OAuthClientSecret")]
        [RefreshProperties(RefreshProperties.None)]
        public string OAuthClientSecret
        {
            get => this.GetPropertyValue<string>("OAuthClientSecret");
            set => this.SetProperty("OAuthClientSecret", value);
        }


        [Category("Other Options")]
        [DefaultValue("00:02:00")]
        [Description("The timeout for the Crm connection.")]
        [DisplayName("ConnectionTimeout")]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(TimeSpanConverter))]
        public TimeSpan ConnectionTimeout
        {
            get => this.GetPropertyValue<TimeSpan>("ConnectionTimeout");
            set => this.SetProperty("ConnectionTimeout", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("Refresh Token of Salesforce")]
        [DisplayName("Refresh Token")]
        [RefreshProperties(RefreshProperties.All)]
        public string RefreshToken
        {
            get => this.GetPropertyValue<string>("RefreshToken");
            set => this.SetProperty("RefreshToken", value);
        }


        [Category("Authentication")]
        [DefaultValue("")]
        [Description("Access Token of Salesforce")]
        [DisplayName("Access Token")]
        [RefreshProperties(RefreshProperties.All)]
        public string AccessToken
        {
            get => this.GetPropertyValue<string>("AccessToken");
            set => this.SetProperty("AccessToken", value);
        }


        [Category("Authentication")]
        [DefaultValue("None")]
        [Description("AuthType of App")]
        [DisplayName("AuthType")]
        [RefreshProperties(RefreshProperties.All)]
        public string AuthType
        {
            get
            {
                var value = this.GetPropertyValue<string>("AuthType");
                if (string.IsNullOrEmpty(value))
                {
                    return "None";
                }

                return value;
            }
            set => this.SetProperty("AuthType", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("APIKey")]
        [DisplayName("APIKey")]
        [RefreshProperties(RefreshProperties.All)]
        public string ApiKey
        {
            get => this.GetPropertyValue<string>("APIKey");
            set => this.SetProperty("APIKey", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("OAuthRefreshTokenUrl")]
        [DisplayName("OAuthRefreshTokenUrl")]
        [RefreshProperties(RefreshProperties.All)]
        public string OAuthRefreshTokenUrl
        {
            get => this.GetPropertyValue<string>("OAuthRefreshTokenUrl");
            set => this.SetProperty("OAuthRefreshTokenUrl", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("OAuthAccessTokenUrl")]
        [DisplayName("OAuthAccessTokenUrl")]
        [RefreshProperties(RefreshProperties.All)]
        public string OAuthAccessTokenUrl
        {
            get => this.GetPropertyValue<string>("OAuthAccessTokenUrl");
            set => this.SetProperty("OAuthAccessTokenUrl", value);
        }

        [Category("Authentication")]
        [DefaultValue("")]
        [Description("CallBackUrl")]
        [DisplayName("CallBackUrl")]
        [RefreshProperties(RefreshProperties.All)]
        public string CallBackUrl
        {
            get => this.GetPropertyValue<string>("CallBackUrl");
            set => this.SetProperty("CallBackUrl", value);
        }


        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        private T GetPropertyValue<T>(string key)
        {
            object val;
            var type = typeof(T);
            bool loaded = TryGetValue(key, out val);

            if (loaded)
            {
                if (type.IsEnum)
                {
                    if (val is string)
                    {

                        return (T)Enum.Parse(type, val as string);
                    }
                }
                if (type == typeof(TimeSpan))
                {
                    if (val is string)
                    {
                        return (T)System.Convert.ChangeType(TimeSpan.Parse(val as string), type);
                    }
                }
                return (T)System.Convert.ChangeType(val, type);
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Sets the property value as a string.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void SetProperty(string key, object value)
        {
            base[key] = Convert.ToString(value);
        }


    }


}

