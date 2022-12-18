using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace RESTAll.Data.Exceptions
{
    public class RestAuthenticationException: AuthenticationException
    {
        public HttpStatusCode Status { set; get; }

        public RestAuthenticationException(HttpStatusCode code,string message):base(message)
        {
            Status = code;
        }

        public RestAuthenticationException(Exception ex):base(ex.Message)
        {
            
        }
    }
}
