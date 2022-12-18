using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RESTAll.Data.Models;

namespace RESTAll.Data.Contracts
{
    public interface IConfigProvider
    {
         bool IsConfigured { set; get; }
         ConfigMaps Config { set; get; }
    }
}
