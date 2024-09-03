using Log4JForwardExtension.Data;
using System;

namespace Log4JForwardExtension.Parsers;


/// <summary>
/// This parser handles MetroLog.Net6 and MetroLog.MAUI default format
/// </summary>
internal class MetroLogParser : IParser
{
    public bool TryParse(string line, out LogMessage? result)
    {
        DateTime timestamp;
        LogLevel logLevel;
        int thread;
        string loggerName;
        string message;

        //Default on fail
        result = null;


        //get parts and check length
        var args = line.Split(['|'], 6);
        if(args.Length < 5) 
        {
            //Logger can contain 5 or 6 parts (depending if loggername is included).
            return false;
        }


        if(!DateTime.TryParse(args[1], out timestamp)){
            return false;
        }

        if(!Enum.TryParse< LogLevel>(args[2], ignoreCase:true, out logLevel)){
            return false;
        }

        if (!int.TryParse(args[3], out thread))
        {
            return false;
        }

        if(args.Length == 6)
        {
            //loggername is included
            loggerName = args[4].Trim();
            message = args[5].Trim();
        }
        else
        {
            //loggername is not included
            loggerName = "ROOT";
            message = args[4].Trim();
        }

        if (string.IsNullOrEmpty(loggerName))
        {
            return false;
        }

        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        //Sucess decoded message
        result = new LogMessage()
        {
            Timestamp = timestamp,
            Level = logLevel,
            LoggerName = $"MetroLog.{loggerName}",
            Thread = thread,
            Message = message
        };
        return true;

    }
}
