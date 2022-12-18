using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSQL.Tokens;
#nullable disable
namespace RESTAll.Data.Models
{
    public class ParameterModel
    {
        public string Identifier { set; get; }
        public TSQLTokenType Type { set; get; }
        public string DestinationColumn { set; get; }
    }
}
