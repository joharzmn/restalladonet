using System.ComponentModel;

namespace RESTAll.Data.Models;

public enum RefreshTokenType
{
    [Description("client_credentials")]
    ClientCredentials,
    [Description("refresh_token")]
    RefreshToken,
}