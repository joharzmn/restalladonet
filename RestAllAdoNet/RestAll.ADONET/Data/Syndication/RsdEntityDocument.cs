using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using RESTAll.Data.Models;
using System.Collections.ObjectModel;
using Argotic.Data.Adapters;
using RESTAll.Data.Common;

namespace RESTAll.Data.Syndication
{
    [Serializable]
    public class RsdEntityDocument : ISyndicationResource, IExtensibleSyndicationObject
    {
        private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;
        /// <summary>
        /// Private member to hold the syndication format for this syndication resource.
        /// </summary>
        private static SyndicationContentFormat documentFormat = SyndicationContentFormat.Opml;
        public string Table { set; get; }
        private const string RsdNamespace = "http://archipelago.phrasewise.com/rsd";
        /// <summary>
        /// Private member to hold HTTP web request used by asynchronous load operations.
        /// </summary>
        private static WebRequest asyncHttpWebRequest;
        public List<DataField> Fields { set; get; }
        /// <summary>
        /// Private member to hold a value indicating if the syndication resource asynchronous load operation was cancelled.
        /// </summary>
        private bool resourceAsyncLoadCancelled;
        /// <summary>
        /// Private member to hold a value indicating if the syndication resource is in the process of loading.
        /// </summary>
        private bool resourceIsLoading;
        public RsdEntityDocument()
        {
            Fields = new List<DataField>();

        }
        private IEnumerable<RsdTableInterface> tableInterfaces;

        public bool AddInterface(RsdTableInterface api)
        {
            bool wasAdded = false;
            Guard.ArgumentNotNull(api, "api");

            ((Collection<RsdTableInterface>)this.Interfaces).Add(api);
            wasAdded = true;

            return wasAdded;
        }

        public RsdTableInterface this[int index]
        {
            get
            {
                return ((Collection<RsdTableInterface>)this.Interfaces)[index];
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                ((Collection<RsdTableInterface>)this.Interfaces)[index] = value;
            }
        }

        /// <summary>
        /// Gets or sets the application interfaces that comprise the discoverable services for this document.
        /// </summary>
        /// <value>A <see cref="IEnumerable{T}"/> collection of <see cref="RsdApplicationInterface"/> objects that represents the application interfaces that comprise the discoverable services for this document.</value>
        /// <remarks>
        ///     This <see cref="IEnumerable{T}"/> collection of <see cref="RsdApplicationInterface"/> objects is internally represented as a <see cref="Collection{T}"/> collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public IEnumerable<RsdTableInterface> Interfaces
        {
            get
            {
                if (tableInterfaces == null)
                {
                    tableInterfaces = new Collection<RsdTableInterface>();
                }
                return tableInterfaces;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                tableInterfaces = value;
            }

        }

        /// <summary>
        /// Creates a new <see cref="RsdDocument"/> instance using the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <returns>An <see cref="RsdDocument"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <remarks>
        ///     The <see cref="RsdDocument"/> is created using the default <see cref="SyndicationResourceLoadSettings"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the Create method.">
        ///         <code 
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Rsd\RsdDocumentExample.cs" 
        ///             region="Create(Uri source)" 
        ///         />
        ///     </code>
        /// </example>
        public static RsdEntityDocument Create(Uri source)
        {
            return RsdEntityDocument.Create(source, new WebRequestOptions());
        }

        /// <summary>
        /// Creates a new <see cref="RsdDocument"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <returns>An <see cref="RsdDocument"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <remarks>
        ///     The <see cref="RsdDocument"/> is created using the default <see cref="SyndicationResourceLoadSettings"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
        public static RsdEntityDocument Create(Uri source, ICredentials credentials, IWebProxy proxy)
        {
            return RsdEntityDocument.Create(source, new WebRequestOptions(credentials, proxy));
        }

        /// <summary>
        /// Creates a new <see cref="RsdDocument"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <returns>An <see cref="RsdDocument"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <remarks>
        ///     The <see cref="RsdDocument"/> is created using the default <see cref="SyndicationResourceLoadSettings"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
        public static RsdEntityDocument Create(Uri source, WebRequestOptions options)
        {
            return RsdEntityDocument.Create(source, options, null);
        }

        /// <summary>
        /// Creates a new <see cref="RsdDocument"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, <see cref="IWebProxy"/>, and <see cref="SyndicationResourceLoadSettings"/> object.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RsdDocument"/> instance. This value can be <b>null</b>.</param>
        /// <returns>An <see cref="RsdDocument"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
        public static RsdEntityDocument Create(Uri source, ICredentials credentials, IWebProxy proxy, SyndicationResourceLoadSettings settings)
        {
            return RsdEntityDocument.Create(source, new WebRequestOptions(credentials, proxy), settings);
        }

        /// <summary>
        /// Creates a new <see cref="RsdDocument"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, <see cref="IWebProxy"/>, and <see cref="SyndicationResourceLoadSettings"/> object.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RsdDocument"/> instance. This value can be <b>null</b>.</param>
        /// <returns>An <see cref="RsdDocument"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
        public static RsdEntityDocument Create(Uri source, WebRequestOptions options, SyndicationResourceLoadSettings settings)
        {
            RsdEntityDocument syndicationResource = new RsdEntityDocument();
            Guard.ArgumentNotNull(source, "source");
            syndicationResource.Load(source, options, settings);

            return syndicationResource;
        }

        public XPathNavigator CreateNavigator()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    ConformanceLevel = ConformanceLevel.Document,
                    Indent = true,
                    OmitXmlDeclaration = false
                };

                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    this.Save(writer);
                    writer.Flush();
                }

                stream.Seek(0, SeekOrigin.Begin);

                XPathDocument document = new XPathDocument(stream);
                return document.CreateNavigator();
            }
        }

        public void Load(IXPathNavigable source)
        {
            this.Load(source, null);
        }

        public void Load(IXPathNavigable source, SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(source, "source");
            if (settings == null)
            {
                settings = new SyndicationResourceLoadSettings();
            }
            XPathNavigator navigator = source.CreateNavigator();
             
            this.Load(navigator, settings, new SyndicationResourceLoadedEventArgs(navigator));
        }

        public void Load(Stream stream)
        {
            this.Load(stream, null);
        }

        public void Load(Stream stream, SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(stream, "stream");
            if (settings != null)
            {
                this.Load(SyndicationEncodingUtility.CreateSafeNavigator(stream, settings.CharacterEncoding), settings);
            }
            else
            {
                this.Load(SyndicationEncodingUtility.CreateSafeNavigator(stream), settings);
            }
        }

        public void Load(XmlReader reader)
        {
            this.Load(reader, null);
        }

        public void Load(XmlReader reader, SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(reader, "reader");
            this.Load(new XPathDocument(reader), settings);
        }

        public void Load(Uri source, ICredentials credentials, IWebProxy proxy)
        {
            this.Load(source, new WebRequestOptions(credentials, proxy));
        }

        public void Load(Uri source, WebRequestOptions options)
        {
            this.Load(source, options, null);
        }

        public void Load(Uri source, ICredentials credentials, IWebProxy proxy, SyndicationResourceLoadSettings settings)
        {
            this.Load(source, new WebRequestOptions(credentials, proxy), settings);
        }

        public void Load(Uri source, WebRequestOptions options, SyndicationResourceLoadSettings settings)
        {
            XPathNavigator navigator = null;
            Guard.ArgumentNotNull(source, "source");
            if (settings == null)
            {
                settings = new SyndicationResourceLoadSettings();
            }
            if (settings.CharacterEncoding == System.Text.Encoding.UTF8)
            {
                navigator = SyndicationEncodingUtility.CreateSafeNavigator(source, options, null);
            }
            else
            {
                
                navigator = SyndicationEncodingUtility.CreateSafeNavigator(source, options, settings.CharacterEncoding);
            }
            this.Load(navigator, settings, new SyndicationResourceLoadedEventArgs(navigator, source, options));
        }

        public void LoadAsync(Uri source, object userToken)
        {
            this.LoadAsync(source, null, userToken);
        }

        public void LoadAsync(Uri source, SyndicationResourceLoadSettings settings, object userToken)
        {
            this.LoadAsync(source, settings, new WebRequestOptions(), userToken);
        }

        public void LoadAsync(Uri source, SyndicationResourceLoadSettings settings, ICredentials credentials, IWebProxy proxy,
            object userToken)
        {
            this.LoadAsync(source, settings, new WebRequestOptions(credentials, proxy), userToken);
        }

        /// <summary>
        /// Cancels an asynchronous operation to load this syndication resource.
        /// </summary>
        /// <remarks>
        ///     Use the LoadAsyncCancel method to cancel a pending <see cref="LoadAsync(Uri, Object)"/> operation. 
        ///     If there is a load operation in progress, this method releases resources used to execute the load operation. 
        ///     If there is no load operation pending, this method does nothing.
        /// </remarks>
        public void LoadAsyncCancel()
        {
            if (this.LoadOperationInProgress && !this.AsyncLoadHasBeenCancelled)
            {
                this.AsyncLoadHasBeenCancelled = true;
                asyncHttpWebRequest.Abort();
            }
        }

        public void LoadAsync(Uri source, SyndicationResourceLoadSettings settings, WebRequestOptions options, object userToken)
        {
            Guard.ArgumentNotNull(source, "source");
            if (settings == null)
            {
                settings = new SyndicationResourceLoadSettings();
            }
            if (this.LoadOperationInProgress)
            {
                throw new InvalidOperationException();
            }
            this.LoadOperationInProgress = true;
            this.AsyncLoadHasBeenCancelled = false;


            asyncHttpWebRequest = SyndicationEncodingUtility.CreateWebRequest(source, options);
            asyncHttpWebRequest.Timeout = Convert.ToInt32(settings.Timeout.TotalMilliseconds, System.Globalization.NumberFormatInfo.InvariantInfo);


            object[] state = new object[6] { asyncHttpWebRequest, this, source, settings, options, userToken };
            IAsyncResult result = asyncHttpWebRequest.BeginGetResponse(new AsyncCallback(AsyncLoadCallback), state);
            ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(AsyncTimeoutCallback), state, settings.Timeout, true);
        }

        /// <summary>
        /// Represents a method to be called when a <see cref="WaitHandle"/> is signaled or times out.
        /// </summary>
        /// <param name="state">An object containing information to be used by the callback method each time it executes.</param>
        /// <param name="timedOut"><b>true</b> if the <see cref="WaitHandle"/> timed out; <b>false</b> if it was signaled.</param>
        private void AsyncTimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                if (asyncHttpWebRequest != null)
                {
                    asyncHttpWebRequest.Abort();
                }
            }
            this.LoadOperationInProgress = false;
        }

        /// <summary>
        /// Gets or sets a value indicating if the syndication resource is in the process of loading.
        /// </summary>
        /// <value><b>true</b> if syndication resource is in the process of loading, otherwise <b>false</b>.</value>
        internal bool LoadOperationInProgress
        {
            get
            {
                return resourceIsLoading;
            }

            set
            {
                resourceIsLoading = value;
            }
        }
        public void Save(Stream stream)
        {
            this.Save(stream, null);
        }

        public void Save(Stream stream, SyndicationResourceSaveSettings settings)
        {
            Guard.ArgumentNotNull(stream, "stream");

            if (settings == null)
            {
                settings = new SyndicationResourceSaveSettings();
            }
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = false;
            writerSettings.Indent = !settings.MinimizeOutputSize;
            writerSettings.Encoding = settings.CharacterEncoding;

            using (XmlWriter writer = XmlWriter.Create(stream, writerSettings))
            {
                this.Save(writer, settings);
            }
        }

        /// <summary>
        /// Called when a corresponding asynchronous load operation completes.
        /// </summary>
        /// <param name="result">The result of the asynchronous operation.</param>
        private static void AsyncLoadCallback(IAsyncResult result)
        {
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            XPathNavigator navigator = null;
            WebRequest httpWebRequest = null;
            RsdEntityDocument document = null;
            Uri source = null;
            WebRequestOptions options = null;
            SyndicationResourceLoadSettings settings = null;
            if (result.IsCompleted)
            {
                object[] parameters = (object[])result.AsyncState;
                httpWebRequest = parameters[0] as WebRequest;
                document = parameters[1] as RsdEntityDocument;
                source = parameters[2] as Uri;
                settings = parameters[3] as SyndicationResourceLoadSettings;
                options = parameters[4] as WebRequestOptions;
                object userToken = parameters[5];
                if (document != null)
                {
                    WebResponse httpWebResponse = (WebResponse)httpWebRequest.EndGetResponse(result);
                    using (Stream stream = httpWebResponse.GetResponseStream())
                    {
                        if (settings != null)
                        {
                            encoding = settings.CharacterEncoding;
                        }

                        using (StreamReader streamReader = new StreamReader(stream, encoding))
                        {
                            XmlReaderSettings readerSettings = new XmlReaderSettings();
                            readerSettings.IgnoreComments = true;
                            readerSettings.IgnoreWhitespace = true;
                            readerSettings.DtdProcessing = DtdProcessing.Ignore;

                            using (XmlReader reader = XmlReader.Create(streamReader, readerSettings))
                            {
                                navigator = SyndicationEncodingUtility.CreateSafeNavigator(source, options, encoding == System.Text.Encoding.UTF8 ? null : settings.CharacterEncoding);
                                SyndicationResourceAdapter adapter = new SyndicationResourceAdapter(navigator, settings);
                                adapter.Fill(document, SyndicationContentFormat.Rsd);
                                document.OnDocumentLoaded(new SyndicationResourceLoadedEventArgs(navigator, source, options, userToken));
                            }
                        }
                    }
                    document.LoadOperationInProgress = false;
                }
            }
        }


        public void Save(XmlWriter writer)
        {
            Guard.ArgumentNotNull(writer, "writer");
            this.Save(writer, new SyndicationResourceSaveSettings());
        }

        public void Save(XmlWriter writer, SyndicationResourceSaveSettings settings)
        {
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNull(settings, "settings");
            writer.WriteStartElement("rsd", RsdNamespace);
            writer.WriteAttributeString("version", this.Version.ToString());

            if (settings.AutoDetectExtensions)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this, settings.SupportedExtensions);

                foreach (RsdTableInterface api in this.Interfaces)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(api, settings.SupportedExtensions);
                }
            }
            SyndicationExtensionAdapter.WriteXmlNamespaceDeclarations(settings.SupportedExtensions, writer);

            writer.WriteStartElement("service", RsdNamespace);
            if (!string.IsNullOrEmpty(this.Table))
            {
                writer.WriteElementString("table", RsdNamespace, this.Table);
            }
            //if (!String.IsNullOrEmpty(this.EngineName))
            //{
            //    writer.WriteElementString("engineName", RsdUtility.RsdNamespace, this.EngineName);
            //}

            //if (this.EngineLink != null)
            //{
            //    writer.WriteElementString("engineLink", RsdUtility.RsdNamespace, this.EngineLink.ToString());
            //}

            //if (this.Homepage != null)
            //{
            //    writer.WriteElementString("homePageLink", RsdUtility.RsdNamespace, this.Homepage.ToString());
            //}

            writer.WriteStartElement("calls", RsdNamespace);
            foreach (RsdTableInterface api in this.Interfaces)
            {
                api.WriteTo(writer);
            }
            writer.WriteEndElement();

            writer.WriteEndElement();
            SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Loads the syndication resource using the specified <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RsdDocument"/>.</param>
        /// <param name="eventData">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data used when raising the <see cref="RsdDocument.Loaded"/> event.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="RsdDocument.Loaded"/> event is raised using the specified <paramref name="eventData"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="eventData"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="navigator"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
        private void Load(XPathNavigator navigator, SyndicationResourceLoadSettings settings, SyndicationResourceLoadedEventArgs eventData)
        {
            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNull(settings, "settings");
            Guard.ArgumentNotNull(eventData, "eventData");

            SyndicationAdapter adapter = new SyndicationAdapter(navigator, settings);
            var meta=new SyndicationResourceMetadata(navigator);
            
            adapter.Fill(this,SyndicationContentFormat.Rsd);
            this.OnDocumentLoaded(eventData);
        }

        public SyndicationContentFormat Format
        {
            get { return documentFormat; }
        }
        public Version Version => new Version(1, 0);
        public event EventHandler<SyndicationResourceLoadedEventArgs>? Loaded;
        public bool AddExtension(ISyndicationExtension extension)
        {
            bool wasAdded = false;
            Guard.ArgumentNotNull(extension, "extension");
            ((Collection<ISyndicationExtension>)this.Extensions).Add(extension);
            wasAdded = true;

            return wasAdded;
        }

        /// <summary>
        /// Raises the <see cref="RsdDocument.Loaded"/> event.
        /// </summary>
        /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
        protected virtual void OnDocumentLoaded(SyndicationResourceLoadedEventArgs e)
        {
            EventHandler<SyndicationResourceLoadedEventArgs> handler = null;
            handler = this.Loaded;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public ISyndicationExtension FindExtension(Predicate<ISyndicationExtension> match)
        {
            Guard.ArgumentNotNull(match, "match");
            List<ISyndicationExtension> list = new List<ISyndicationExtension>(this.Extensions);
            return list.Find(match);
        }

        public bool RemoveExtension(ISyndicationExtension extension)
        {
            bool wasRemoved = false;
            Guard.ArgumentNotNull(extension, "extension");
            if (((Collection<ISyndicationExtension>)this.Extensions).Contains(extension))
            {
                ((Collection<ISyndicationExtension>)this.Extensions).Remove(extension);
                wasRemoved = true;
            }

            return wasRemoved;
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

        /// <summary>
        /// Gets or sets a value indicating if the syndication resource asynchronous load operation was cancelled.
        /// </summary>
        /// <value><b>true</b> if syndication resource asynchronous load operation has been cancelled, otherwise <b>false</b>.</value>
        internal bool AsyncLoadHasBeenCancelled
        {
            get
            {
                return resourceAsyncLoadCancelled;
            }

            set
            {
                resourceAsyncLoadCancelled = value;
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
