using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RESTAll.Data.Common;
#nullable disable
namespace RESTAll.Data.Models
{
    public class EntityResultSet
    {
        public EntityResultSet(DataTable data)
        {
            Data = data;
        }

        public string CurrentPage { set; get; }
        public string NextPage { set; get; }

        public DataTable Data { set; get; }

        public string NextPageUrl { set; get; }

        public int ResultCount()
        {
            return Data == null ? 0 : Data.Rows.Count;
        }

        public bool HasResults()
        {
            return Data != null && Data.Rows.Count > 0;
        }

        public object GetScalar()
        {
            if (Data != null)
            {
                return Data.Rows[0][0];
            }

            return null;
        }

        public virtual bool HasColumnMetadata()
        {
            return Data != null && Data.Columns.Count > 0;
        }

        public object GetValue(int columnOrdinal, int position)
        {
            return Data.Rows[columnOrdinal][position];
        }

        public virtual void LoadNextPage()
        {
            throw new NotSupportedException();
        }

        public virtual bool HasMoreRecords()
        {
            return false;
        }
    }
}
