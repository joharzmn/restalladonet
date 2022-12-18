using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTAll.Data.Logs
{
    public class FileLoggerScope<TState>
        : System.IDisposable
    {
        protected FileLogger m_logger;
        protected TState m_scopeName;


        public TState ScopeName
        {
            get
            {
                return this.m_scopeName;
            }
        } // End Property ScopeName


        public FileLoggerScope(FileLogger logger, TState scopeName)
        {
            this.m_logger = logger;
            this.m_scopeName = scopeName;
        } // End Constructor  


        void System.IDisposable.Dispose()
        {
            this.m_logger.EndScope(this.m_scopeName);
        } // End Sub Dispose 


    } // End Class FileLoggerScope 
}
