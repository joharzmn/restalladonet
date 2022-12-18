using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSQL.Tokens;

namespace RESTAll.Parser
{
    internal interface ITSQLTokenizer :
        IEnumerator,
        IEnumerable,
        IEnumerator<TSQLToken>,
        IDisposable,
        IEnumerable<TSQLToken>
    {
    }
}
