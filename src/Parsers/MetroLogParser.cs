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
        if(args.Length < 6) 
        {
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

        //Get loggername part
        loggerName = args[4].Trim();
        if (string.IsNullOrEmpty(loggerName))
        {
            return false;
        }

        //Get message part and check if we got a valid message
        message = args[5].Trim();
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        //Sucess decoded message
        result = new LogMessage()
        {
            Timestamp = timestamp,
            Level = logLevel,
            LoggerName = loggerName,
            Thread = thread,
            Message = message
        };
        return true;

    }
}
