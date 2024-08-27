using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4JForwardExtension;

internal class Log4JTransmitter
{
    public static Log4JTransmitter Instance { get; private set; }

    private Log4JTransmitter()
    {

    }



    public static void Initialize(AsyncPackage package)
    {
        Instance = new Log4JTransmitter();
    }


    public void HandleDebugLine(string line)
    {

    }



}
