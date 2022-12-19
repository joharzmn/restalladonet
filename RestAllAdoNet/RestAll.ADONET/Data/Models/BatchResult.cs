using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable
namespace RESTAll.Data.Models
{
    internal class BatchResult
    {
        public DataRow DataRow { set; get; }
        public BatchState BatchState { set; get; }
        public Exception Exception { set; get; }
        public int RowIndex { set; get; }
        public object RawResult { set; get; }
    }

    internal enum BatchState
    {
        Error,
        Success,
    }
}
