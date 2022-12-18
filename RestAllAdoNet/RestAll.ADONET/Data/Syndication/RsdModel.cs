using Argotic.Syndication.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTAll.Data.Syndication
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Script")]
    public static class RsdModel
    {
        public static void CreateExample()
        {
            RsdDocument document = new RsdDocument();

            document.EngineName = "Blog Munging CMS";
            document.EngineLink = new Uri("http://www.blogmunging.com/");
            document.Homepage = new Uri("http://www.userdomain.com/");

            document.AddInterface(new RsdApplicationInterface("MetaWeblog", new Uri("http://example.com/xml/rpc/url"), true, "123abc"));
            document.AddInterface(new RsdApplicationInterface("Blogger", new Uri("http://example.com/xml/rpc/url"), false, "123abc"));
            document.AddInterface(new RsdApplicationInterface("MetaWiki", new Uri("http://example.com/some/other/url"), false, "123abc"));
            document.AddInterface(new RsdApplicationInterface("Antville", new Uri("http://example.com/yet/another/url"), false, "123abc"));

            RsdApplicationInterface conversantApi = new RsdApplicationInterface("Conversant", new Uri("http://example.com/xml/rpc/url"), false, String.Empty);
            conversantApi.Documentation = new Uri("http://www.conversant.com/docs/api/");
            conversantApi.Notes = "Additional explanation here.";
            conversantApi.Settings.Add("service-specific-setting", "a value");
            conversantApi.Settings.Add("another-setting", "another value");
            document.AddInterface(conversantApi);
        }
    }
}
