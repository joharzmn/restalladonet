using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions;
using Argotic.Syndication.Specialized;

namespace RESTAll.Data.Syndication
{
    public class Rsd10SyndicationAdapter : SyndicationResourceAdapter
    {
        public Rsd10SyndicationAdapter(
          XPathNavigator navigator,
          SyndicationResourceLoadSettings settings)
          : base(navigator, settings)
        {
        }

        public void Fill(RsdEntityDocument resource)
        {
            Guard.ArgumentNotNull((object)resource, nameof(resource));
            XmlNamespaceManager namespaceManager = RsdUtility.CreateNamespaceManager(this.Navigator.NameTable);
            XPathNavigator source = RsdUtility.SelectSafeSingleNode(this.Navigator, "rsd:rsd/rsd:service", (IXmlNamespaceResolver)namespaceManager) ?? RsdUtility.SelectSafeSingleNode(this.Navigator, "rsd:rsd/service", (IXmlNamespaceResolver)namespaceManager);
            if (source != null)
            {
                XPathNavigator xpathNavigator1 = RsdUtility.SelectSafeSingleNode(source, "rsd:table", (IXmlNamespaceResolver)namespaceManager);
                XPathNavigator xpathNavigator2 = RsdUtility.SelectSafeSingleNode(source, "rsd:engineLink", (IXmlNamespaceResolver)namespaceManager);
                XPathNavigator xpathNavigator3 = RsdUtility.SelectSafeSingleNode(source, "rsd:homePageLink", (IXmlNamespaceResolver)namespaceManager);
                XPathNodeIterator xpathNodeIterator = RsdUtility.SelectSafe(source, "rsd:calls/rsd:api", (IXmlNamespaceResolver)namespaceManager);
                if (xpathNavigator1 != null && !string.IsNullOrEmpty(xpathNavigator1.Value))
                    resource.Table = xpathNavigator1.Value;
                Uri result1;
                //if (xpathNavigator2 != null && Uri.TryCreate(xpathNavigator2.Value, UriKind.RelativeOrAbsolute, out result1))
                //    resource.EngineLink = result1;
                //Uri result2;
                //if (xpathNavigator3 != null && Uri.TryCreate(xpathNavigator3.Value, UriKind.RelativeOrAbsolute, out result2))
                //    resource.Homepage = result2;
                if (xpathNodeIterator != null && xpathNodeIterator.Count > 0)
                {
                    int num = 0;
                    while (xpathNodeIterator.MoveNext())
                    {
                        RsdTableInterface applicationInterface = new RsdTableInterface();
                        ++num;
                        if (applicationInterface.Load(xpathNodeIterator.Current, this.Settings))
                        {
                            if (this.Settings.RetrievalLimit == 0 || num <= this.Settings.RetrievalLimit)
                                ((Collection<RsdTableInterface>)resource.Interfaces).Add(applicationInterface);
                            else
                                break;
                        }
                    }
                }
            }
            new SyndicationExtensionAdapter(RsdUtility.SelectSafeSingleNode(this.Navigator, "rsd:rsd", (IXmlNamespaceResolver)namespaceManager), this.Settings).Fill((IExtensibleSyndicationObject)resource, namespaceManager);
        }
    }
}
