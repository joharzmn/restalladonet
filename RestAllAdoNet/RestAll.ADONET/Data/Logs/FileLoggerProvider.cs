﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTAll.Data.Logs
{
    public class IgnoreLogger
       : Microsoft.Extensions.Logging.ILogger
    {

        public class IgnoreScope
            : IDisposable
        {
            void IDisposable.Dispose()
            {
            }
        }

        IDisposable Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState state)
        {
            return new IgnoreScope();
        }

        bool Microsoft.Extensions.Logging.ILogger.IsEnabled(
            Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return false;
        }

        void Microsoft.Extensions.Logging.ILogger.Log<TState>(
              Microsoft.Extensions.Logging.LogLevel logLevel
            , Microsoft.Extensions.Logging.EventId eventId
            , TState state
            , Exception exception
            , Func<TState, Exception, string> formatter)
        { }

    }


    public class FileLoggerProvider
        : Microsoft.Extensions.Logging.ILoggerProvider
    {

        protected FileLoggerOptions m_options;
        protected IgnoreLogger m_nullLogger;
        protected FileLogger m_cachedLogger;


        public FileLoggerProvider(Microsoft.Extensions.Options.IOptions<FileLoggerOptions> fso)
        {
            this.m_options = fso.Value;
            this.m_nullLogger = new IgnoreLogger();
            this.m_cachedLogger = new FileLogger(this, this.m_options, "OneInstanceFitsAll");
        } // End Constructor 


        Microsoft.Extensions.Logging.ILogger Microsoft.Extensions.Logging.ILoggerProvider
            .CreateLogger(string categoryName)
        {
            // Microsoft.Extensions.Hosting.Internal.ApplicationLifetime
            // Microsoft.Extensions.Hosting.Internal.Host
            // Microsoft.Hosting.Lifetime
            if (categoryName.StartsWith("Microsoft", System.StringComparison.Ordinal))
                return this.m_nullLogger; // NULL is not a valid value... 

            return this.m_cachedLogger;
        } // End Function CreateLogger 



        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                disposedValue = true;
            }
        }


        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~FileLoggerProvider() {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }


        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        void System.IDisposable.Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            // GC.SuppressFinalize(this);
        }


    } // End Class FileLoggerProvider 

}
