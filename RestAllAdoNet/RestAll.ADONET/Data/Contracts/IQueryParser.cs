using RESTAll.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTAll.Data.Contracts
{
    public interface IQueryParser
    {
        List<QueryDescriptor> Parse(string query);
    }
}
