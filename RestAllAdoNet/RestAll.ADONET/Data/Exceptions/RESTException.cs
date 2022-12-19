using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
#nullable disable
namespace RESTAll.Data.Exceptions
{
    public class RESTException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Reason { set; get; }
        public RESTException(string message, HttpStatusCode code) : base(message)
        {
            StatusCode = code;
        }

        public RESTException(string message, string reason):base(message)
        {
            Reason = reason;
        }
    }
}
