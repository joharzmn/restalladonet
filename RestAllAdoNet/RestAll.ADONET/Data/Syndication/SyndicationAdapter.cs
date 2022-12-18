// Decompiled with JetBrains decompiler
// Type: Argotic.Data.Adapters.SyndicationResourceAdapter
// Assembly: Argotic.Core, Version=3000.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: E0CBC2E5-2EE5-4386-AE5E-5F6574021465
// Assembly location: C:\Users\shahi\.nuget\packages\argotic.core\3000.0.3\lib\netstandard2.1\Argotic.Core.dll

using Argotic.Common;
using Argotic.Publishing;
using Argotic.Syndication;
using Argotic.Syndication.Specialized;
using System;
using System.Xml.XPath;
using RESTAll.Data.Syndication;

namespace Argotic.Data.Adapters
{
    public class SyndicationAdapter
    {
        private XPathNavigator adapterNavigator;
        private SyndicationResourceLoadSettings adapterSettings = new SyndicationResourceLoadSettings();

        public SyndicationAdapter(
          XPathNavigator navigator,
          SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull((object)navigator, nameof(navigator));
            Guard.ArgumentNotNull((object)settings, nameof(settings));
            this.adapterNavigator = navigator;
            this.adapterSettings = settings;
        }

        public XPathNavigator Navigator => this.adapterNavigator;

        public SyndicationResourceLoadSettings Settings => this.adapterSettings;

        public void Fill(ISyndicationResource resource, SyndicationContentFormat format)
        {
            Guard.ArgumentNotNull((object)resource, nameof(resource));
            if (format == SyndicationContentFormat.None)
                throw new ArgumentException(string.Format((IFormatProvider)null, "The specified syndication content format of {0} is invalid.", (object)format), nameof(format));
            SyndicationResourceMetadata resourceMetadata = new SyndicationResourceMetadata(this.Navigator);
            if (format != resourceMetadata.Format)
                throw new FormatException(string.Format((IFormatProvider)null, "The supplied syndication resource has a content format of {0}, which does not match the expected content format of {1}.", (object)resourceMetadata.Format, (object)format));
            switch (format)
            {
                case SyndicationContentFormat.Apml:
                    this.FillApmlResource(resource, resourceMetadata);
                    break;
                case SyndicationContentFormat.Atom:
                    this.FillAtomResource(resource, resourceMetadata);
                    break;
                case SyndicationContentFormat.BlogML:
                    this.FillBlogMLResource(resource, resourceMetadata);
                    break;
                case SyndicationContentFormat.Opml:
                    this.FillOpmlResource(resource, resourceMetadata);
                    break;
                case SyndicationContentFormat.Rsd:
                    this.FillRsdResource(resource, resourceMetadata);
                    break;
                case SyndicationContentFormat.Rss:
                    this.FillRssResource(resource, resourceMetadata);
                    break;
                case SyndicationContentFormat.AtomCategoryDocument:
                    this.FillAtomPublishingResource(resource, resourceMetadata);
                    break;
                case SyndicationContentFormat.AtomServiceDocument:
                    this.FillAtomPublishingResource(resource, resourceMetadata);
                    break;
            }
        }

        private void FillApmlResource(
          ISyndicationResource resource,
          SyndicationResourceMetadata resourceMetadata)
        {
            Guard.ArgumentNotNull((object)resource, nameof(resource));
            Guard.ArgumentNotNull((object)resourceMetadata, nameof(resourceMetadata));
            ApmlDocument resource1 = resource as ApmlDocument;
            if (!(resourceMetadata.Version == new Version("0.6")))
                return;
            new Apml06SyndicationResourceAdapter(this.Navigator, this.Settings).Fill(resource1);
        }

        private void FillAtomResource(
          ISyndicationResource resource,
          SyndicationResourceMetadata resourceMetadata)
        {
            Guard.ArgumentNotNull((object)resource, nameof(resource));
            Guard.ArgumentNotNull((object)resourceMetadata, nameof(resourceMetadata));
            AtomFeed resource1 = resource as AtomFeed;
            AtomEntry resource2 = resource as AtomEntry;
            if (resourceMetadata.Version == new Version("1.0"))
            {
                Atom10SyndicationResourceAdapter syndicationResourceAdapter = new Atom10SyndicationResourceAdapter(this.Navigator, this.Settings);
                if (resource1 != null)
                    syndicationResourceAdapter.Fill(resource1);
                else if (resource2 != null)
                    syndicationResourceAdapter.Fill(resource2);
            }
            if (!(resourceMetadata.Version == new Version("0.3")))
                return;
            Atom03SyndicationResourceAdapter syndicationResourceAdapter1 = new Atom03SyndicationResourceAdapter(this.Navigator, this.Settings);
            if (resource1 != null)
            {
                syndicationResourceAdapter1.Fill(resource1);
            }
            else
            {
                if (resource2 == null)
                    return;
                syndicationResourceAdapter1.Fill(resource2);
            }
        }

        private void FillAtomPublishingResource(
          ISyndicationResource resource,
          SyndicationResourceMetadata resourceMetadata)
        {
            Guard.ArgumentNotNull((object)resource, nameof(resource));
            Guard.ArgumentNotNull((object)resourceMetadata, nameof(resourceMetadata));
            AtomCategoryDocument resource1 = resource as AtomCategoryDocument;
            AtomServiceDocument resource2 = resource as AtomServiceDocument;
            if (!(resourceMetadata.Version == new Version("1.0")))
                return;
            AtomPublishing10SyndicationResourceAdapter syndicationResourceAdapter = new AtomPublishing10SyndicationResourceAdapter(this.Navigator, this.Settings);
            if (resource1 != (AtomCategoryDocument)null)
            {
                syndicationResourceAdapter.Fill(resource1);
            }
            else
            {
                if (resource2 == null)
                    return;
                syndicationResourceAdapter.Fill(resource2);
            }
        }

        private void FillBlogMLResource(
          ISyndicationResource resource,
          SyndicationResourceMetadata resourceMetadata)
        {
            Guard.ArgumentNotNull((object)resource, nameof(resource));
            Guard.ArgumentNotNull((object)resourceMetadata, nameof(resourceMetadata));
            BlogMLDocument resource1 = resource as BlogMLDocument;
            BlogML20SyndicationResourceAdapter syndicationResourceAdapter = new BlogML20SyndicationResourceAdapter(this.Navigator, this.Settings);
            if (!(resourceMetadata.Version == new Version("2.0")))
                return;
            syndicationResourceAdapter.Fill(resource1);
        }

        private void FillOpmlResource(
          ISyndicationResource resource,
          SyndicationResourceMetadata resourceMetadata)
        {
            Guard.ArgumentNotNull((object)resource, nameof(resource));
            Guard.ArgumentNotNull((object)resourceMetadata, nameof(resourceMetadata));
            OpmlDocument resource1 = resource as OpmlDocument;
            Opml20SyndicationResourceAdapter syndicationResourceAdapter = new Opml20SyndicationResourceAdapter(this.Navigator, this.Settings);
            if (resourceMetadata.Version == new Version("2.0"))
                syndicationResourceAdapter.Fill(resource1);
            if (resourceMetadata.Version == new Version("1.1"))
                syndicationResourceAdapter.Fill(resource1);
            if (!(resourceMetadata.Version == new Version("1.0")))
                return;
            syndicationResourceAdapter.Fill(resource1);
        }

        private void FillRsdResource(
          ISyndicationResource resource,
          SyndicationResourceMetadata resourceMetadata)
        {
            Guard.ArgumentNotNull((object)resource, nameof(resource));
            Guard.ArgumentNotNull((object)resourceMetadata, nameof(resourceMetadata));
            RsdEntityDocument resource1 = resource as RsdEntityDocument;
            if (resourceMetadata.Version == new Version("1.0"))
                new RESTAll.Data.Syndication.Rsd10SyndicationAdapter(this.Navigator, this.Settings).Fill(resource1);
            if (!(resourceMetadata.Version == new Version("0.6")))
                return;
            new Rsd10SyndicationAdapter(this.Navigator, this.Settings).Fill(resource1);
        }

        private void FillRssResource(
          ISyndicationResource resource,
          SyndicationResourceMetadata resourceMetadata)
        {
            Guard.ArgumentNotNull((object)resource, nameof(resource));
            Guard.ArgumentNotNull((object)resourceMetadata, nameof(resourceMetadata));
            RssFeed resource1 = resource as RssFeed;
            if (resourceMetadata.Version == new Version("2.0"))
                new Rss20SyndicationResourceAdapter(this.Navigator, this.Settings).Fill(resource1);
            if (resourceMetadata.Version == new Version("1.0"))
                new Rss10SyndicationResourceAdapter(this.Navigator, this.Settings).Fill(resource1);
            if (resourceMetadata.Version == new Version("0.92"))
                new Rss092SyndicationResourceAdapter(this.Navigator, this.Settings).Fill(resource1);
            if (resourceMetadata.Version == new Version("0.91"))
                new Rss091SyndicationResourceAdapter(this.Navigator, this.Settings).Fill(resource1);
            if (!(resourceMetadata.Version == new Version("0.9")))
                return;
            new Rss090SyndicationResourceAdapter(this.Navigator, this.Settings).Fill(resource1);
        }
    }
}
