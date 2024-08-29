using Log4JForwardExtension.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4JForwardExtension.Parsers;

internal interface IParser
{

    bool TryParse(string line, out LogMessage? result);
}
