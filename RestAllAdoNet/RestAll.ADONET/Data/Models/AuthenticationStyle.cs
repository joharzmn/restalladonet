using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTAll.Data.Models
{
    public enum AuthenticationStyle
    {
        OAuth2,
        OAuth,
        Password,
        Basic
    }

    public enum GrantType
    {
        [Description("authorization_code")]
        Code,
        [Description("client_credentials")]
        ClientCredentials
    }

    public enum RefreshTokenType
    {
        [Description("client_credentials")]
        ClientCredentials,
        [Description("refresh_token")]
        RefreshToken,
    }
}
