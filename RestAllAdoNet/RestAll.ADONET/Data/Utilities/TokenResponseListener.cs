using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RESTAll.Data.Utilities
{
    public class TokenResponseListener
    {


        public static Dictionary<string, string> ListenCode(string callUrl, string readParams)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = callUrl,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
            var seprated = readParams.ToLower().Split(",").ToList();
            if (!seprated.Contains("code"))
            {
                seprated.Add("code");
            }
            using var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5678/");

            listener.Start();

            Console.WriteLine("Listening on port 8001...");

            HttpListenerContext ctx = listener.GetContext();
            HttpListenerRequest req = ctx.Request;
            listener.Stop();
            var paramsDict = new Dictionary<string, string>();
            foreach (var s in seprated)
            {
                paramsDict.Add(s, req.QueryString.Get(s));
            }

            return paramsDict;
        }
    }
}
