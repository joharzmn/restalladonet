using Argotic.Common;
using Argotic.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace RESTAll.Data.Syndication
{
    [Serializable]
    public class RsdTableInterface : IComparable, IExtensibleSyndicationObject
    {
        public string ActionType { set; get; }
        public string Method { set; get; }
        public string Url { set; get; }
        private const string RsdNamespace = "http://archipelago.phrasewise.com/rsd";
        public Uri Documentation { set; get; }
        public Dictionary<string, object> Settings { set; get; }
        /// <summary>
        /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
        /// </summary>
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;

        public RsdTableInterface()
        {
            
        }
        public RsdTableInterface(string actionType, string method, string uri)
        {
            Method = method;
            ActionType = actionType;
            Settings = new Dictionary<string, object>();
            Url = uri;
        }

        public int CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }

        public bool AddExtension(ISyndicationExtension extension)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Loads this <see cref="RsdApplicationInterface"/> using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <returns><b>true</b> if the <see cref="RsdApplicationInterface"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RsdApplicationInterface"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            XmlNamespaceManager manager = RsdUtility.CreateNamespaceManager(source.NameTable);
            if (source.HasAttributes)
            {
                string actionAttribute = source.GetAttribute("ActionType", String.Empty);
                string methodAttribute = source.GetAttribute("Method", String.Empty);
                string urlAttribute = source.GetAttribute("Url", String.Empty);
               

                if (!String.IsNullOrEmpty(actionAttribute))
                {
                    this.ActionType = actionAttribute;
                    wasLoaded = true;
                }

                if (!String.IsNullOrEmpty(methodAttribute))
                {

                    this.Method = methodAttribute;
                    wasLoaded = true;

                }

                if (!String.IsNullOrEmpty(urlAttribute))
                {

                    this.Url = urlAttribute;
                    wasLoaded = true;

                }

                //if (!String.IsNullOrEmpty(blogIdAttribute))
                //{
                //    this.WeblogId = blogIdAttribute;
                //    wasLoaded = true;
                //}
            }

            if (source.HasChildren)
            {
                XPathNavigator settingsNavigator = RsdUtility.SelectSafeSingleNode(source, "rsd:api/rsd:settings", manager);

                if (settingsNavigator != null)
                {
                    XPathNavigator docsNavigator = RsdUtility.SelectSafeSingleNode(settingsNavigator, "rsd:docs", manager);
                    XPathNavigator notesNavigator = RsdUtility.SelectSafeSingleNode(settingsNavigator, "rsd:notes", manager);
                    XPathNodeIterator settingIterator = RsdUtility.SelectSafe(settingsNavigator, "rsd:setting", manager);

                    if (docsNavigator != null)
                    {
                        Uri documentation;
                        if (Uri.TryCreate(docsNavigator.Value, UriKind.RelativeOrAbsolute, out documentation))
                        {
                            this.Documentation = documentation;
                            wasLoaded = true;
                        }
                    }

                    //if (notesNavigator != null)
                    //{
                    //    this.Notes = notesNavigator.Value;
                    //    wasLoaded = true;
                    //}

                    if (settingIterator != null && settingIterator.Count > 0)
                    {
                        while (settingIterator.MoveNext())
                        {
                            string settingName = settingIterator.Current.GetAttribute("name", String.Empty);
                            string settingValue = settingIterator.Current.Value;

                            if (!this.Settings.ContainsKey(settingName))
                            {
                                this.Settings.Add(settingName, settingValue);
                                wasLoaded = true;
                            }
                        }
                    }
                }
            }

            return wasLoaded;
        }

        /// <summary>
        /// Loads this <see cref="RsdApplicationInterface"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
        /// <returns><b>true</b> if the <see cref="RsdApplicationInterface"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <remarks>
        ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RsdApplicationInterface"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(settings, "settings");
            wasLoaded = this.Load(source);
            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(source, settings);
            adapter.Fill(this);

            return wasLoaded;
        }

        public ISyndicationExtension FindExtension(Predicate<ISyndicationExtension> match)
        {
            throw new NotImplementedException();
        }

        public bool RemoveExtension(ISyndicationExtension extension)
        {
            throw new NotImplementedException();
        }

        public void WriteTo(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            writer.WriteStartElement("api", RsdNamespace);

            writer.WriteAttributeString("Method", this.Method);
            writer.WriteAttributeString("ActionType", this.ActionType != null ? this.ActionType.ToString() : String.Empty);
            writer.WriteAttributeString("Url", this.Url.ToString());

            if (this.Documentation != null || this.Settings.Count > 0)
            {
                writer.WriteStartElement("settings", RsdNamespace);

                //if (this.Documentation != null)
                //{
                //    writer.WriteElementString("docs", RsdUtility.RsdNamespace, this.Documentation.ToString());
                //}

                //if (!String.IsNullOrEmpty(this.Notes))
                //{
                //    writer.WriteElementString("notes", RsdUtility.RsdNamespace, this.Notes);
                //}

                foreach (string settingName in this.Settings.Keys)
                {
                    writer.WriteStartElement("setting", RsdNamespace);
                    writer.WriteAttributeString("name", settingName);
                    writer.WriteString(this.Settings[settingName].ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);


            writer.WriteEndElement();
        }

        public IEnumerable<ISyndicationExtension> Extensions
        {
            get
            {
                if (objectSyndicationExtensions == null)
                {
                    objectSyndicationExtensions = new Collection<ISyndicationExtension>();
                }
                return objectSyndicationExtensions;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                objectSyndicationExtensions = value;
            }
        }
        public bool HasExtensions
        {
            get
            {
                return ((Collection<ISyndicationExtension>)this.Extensions).Count > 0;
            }
        }
    }
}
