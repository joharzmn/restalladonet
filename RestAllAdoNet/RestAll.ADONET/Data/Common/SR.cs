using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RESTAll.Data.Common
{
    internal static class SR
    {
        private static ResourceManager s_resourceManager;
        private const string s_resourcesName = "Resources.SR";

        private static ResourceManager ResourceManager
        {
            get
            {
                if (s_resourceManager == null)
                    s_resourceManager = new ResourceManager(ResourceType);
                return s_resourceManager;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool UsingResourceKeys() => false;

        internal static string GetResourceString(string resourceKey, string defaultString)
        {
            string str = null;
            try
            {
                str = ResourceManager.GetString(resourceKey);
            }
            catch (MissingManifestResourceException ex)
            {
            }
            return defaultString != null && resourceKey.Equals(str, StringComparison.Ordinal) ? defaultString : str;
        }

        internal static string Format(string resourceFormat, params object[] args)
        {
            if (args == null)
                return resourceFormat;
            return UsingResourceKeys() ? resourceFormat + string.Join(", ", args) : string.Format(resourceFormat, args);
        }

        internal static string Format(string resourceFormat, object p1) => UsingResourceKeys() ? string.Join(", ", resourceFormat, p1) : string.Format(resourceFormat, new object[1]
        {
      p1
        });

        internal static string Format(string resourceFormat, object p1, object p2) => UsingResourceKeys() ? string.Join(", ", resourceFormat, p1, p2) : string.Format(resourceFormat, new object[2]
        {
      p1,
      p2
        });

        internal static string Format(string resourceFormat, object p1, object p2, object p3) => UsingResourceKeys() ? string.Join(", ", resourceFormat, p1, p2, p3) : string.Format(resourceFormat, new object[3]
        {
      p1,
      p2,
      p3
        });

        internal static string ADP_ConnectionStringSyntax => GetResourceString(nameof(ADP_ConnectionStringSyntax), null);

        internal static string ADP_EmptyString => GetResourceString(nameof(ADP_EmptyString), null);

        internal static string ADP_InternalProviderError => GetResourceString(nameof(ADP_InternalProviderError), null);

        internal static string ADP_InvalidKey => GetResourceString(nameof(ADP_InvalidKey), null);

        internal static string ADP_InvalidValue => GetResourceString(nameof(ADP_InvalidValue), null);

        internal static string ADP_KeywordNotSupported => GetResourceString(nameof(ADP_KeywordNotSupported), null);

        internal static string SqlConvert_ConvertFailed => GetResourceString(nameof(SqlConvert_ConvertFailed), null);

        internal static string ADP_InvalidSourceBufferIndex => GetResourceString(nameof(ADP_InvalidSourceBufferIndex), null);

        internal static string SQL_InvalidDataLength => GetResourceString(nameof(SQL_InvalidDataLength), null);

        internal static string SQL_InvalidBufferSizeOrIndex => GetResourceString(nameof(SQL_InvalidBufferSizeOrIndex), null);

        internal static string ADP_InvalidDestinationBufferIndex => GetResourceString(nameof(ADP_InvalidDestinationBufferIndex), null);

        internal static Type ResourceType => typeof(Resources.SR);
    }
}
