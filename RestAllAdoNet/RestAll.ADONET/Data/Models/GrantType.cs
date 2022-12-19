using System.ComponentModel;

namespace RESTAll.Data.Models;

internal enum GrantType
{
    [Description("authorization_code")]
    Code,
    [Description("client_credentials")]
    ClientCredentials
}