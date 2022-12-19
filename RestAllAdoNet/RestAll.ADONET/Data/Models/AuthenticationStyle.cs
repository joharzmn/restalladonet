using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTAll.Data.Models
{
    internal enum AuthenticationStyle
    {
        None,
        OAuth2,
        OAuth,
        Password,
        Basic
    }
}
